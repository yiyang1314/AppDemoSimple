using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rxbit.BrigadeApp.Services.Media
{
    /// <summary>
    /// 轻量 AVI(MJPEG) 写入器：逐帧写入 JPEG 数据，无需任何第三方编码器或驱动。
    /// 生成的文件可被 Windows 自带播放器与本程序的回放窗口打开。
    /// </summary>
    public sealed class AviMjpegWriter : IDisposable
    {
        private readonly FileStream _stream;
        private readonly BinaryWriter _writer;
        private readonly int _width;
        private readonly int _height;
        private readonly int _frameRate;
        private readonly int _frameBytes;

        private readonly List<long> _frameOffsets = new List<long>();
        private readonly List<int> _frameSizes = new List<int>();

        private long _riffSizePos;
        private long _hdrlSizePos;
        private long _strlSizePos;
        private long _avihTotalFramesPos;
        private long _strhLengthPos;
        private long _moviListSizePos;
        private long _moviFourCcPos;
        private bool _closed;

        public string FilePath { get; }
        public int FrameCount => _frameOffsets.Count;
        public int Width => _width;
        public int Height => _height;
        public int FrameRate => _frameRate;

        public AviMjpegWriter(string filePath, int width, int height, int frameRate)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Invalid frame size.");
            }
            if (frameRate <= 0)
            {
                throw new ArgumentException("Invalid frame rate.");
            }

            var folder = Path.GetDirectoryName(Path.GetFullPath(filePath));
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _width = width;
            _height = height;
            _frameRate = frameRate;
            _frameBytes = width * height * 3;
            FilePath = filePath;

            _stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new BinaryWriter(_stream, Encoding.ASCII);
            WriteHeader();
        }

        private static byte[] FourCc(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private void WriteFourCc(string value)
        {
            _writer.Write(FourCc(value));
        }

        private void WriteHeader()
        {
            // RIFF header
            WriteFourCc("RIFF");
            _riffSizePos = _stream.Position;
            _writer.Write(0); // 占位，关闭时回填
            WriteFourCc("AVI ");

            // LIST hdrl
            WriteFourCc("LIST");
            _hdrlSizePos = _stream.Position;
            _writer.Write(0);
            WriteFourCc("hdrl");

            // avih
            WriteFourCc("avih");
            _writer.Write(56);
            _writer.Write(1000000 / _frameRate); // dwMicroSecPerFrame
            _writer.Write(0);                    // dwMaxBytesPerSec
            _writer.Write(0);                    // dwPaddingGranularity
            _writer.Write(0x10);                 // dwFlags: AVIF_HASINDEX
            _avihTotalFramesPos = _stream.Position;
            _writer.Write(0);                    // dwTotalFrames
            _writer.Write(0);                    // dwInitialFrames
            _writer.Write(1);                    // dwStreams
            _writer.Write(_frameBytes);          // dwSuggestedBufferSize
            _writer.Write(_width);
            _writer.Write(_height);
            _writer.Write(0);
            _writer.Write(0);
            _writer.Write(0);
            _writer.Write(0);

            // LIST strl
            WriteFourCc("LIST");
            _strlSizePos = _stream.Position;
            _writer.Write(0);
            WriteFourCc("strl");

            // strh
            WriteFourCc("strh");
            _writer.Write(56);
            WriteFourCc("vids");
            WriteFourCc("MJPG");
            _writer.Write(0);          // dwFlags
            _writer.Write((short)0);   // wPriority
            _writer.Write((short)0);   // wLanguage
            _writer.Write(0);          // dwInitialFrames
            _writer.Write(1000);       // dwScale
            _writer.Write(_frameRate * 1000); // dwRate
            _writer.Write(0);          // dwStart
            _strhLengthPos = _stream.Position;
            _writer.Write(0);          // dwLength
            _writer.Write(_frameBytes); // dwSuggestedBufferSize
            _writer.Write(-1);         // dwQuality
            _writer.Write(0);          // dwSampleSize
            _writer.Write((short)0);   // rcFrame.left
            _writer.Write((short)0);   // rcFrame.top
            _writer.Write((short)_width);
            _writer.Write((short)Math.Min(_height, short.MaxValue));

            // strf
            WriteFourCc("strf");
            _writer.Write(40);
            _writer.Write(40);         // biSize
            _writer.Write(_width);     // biWidth
            _writer.Write(_height);    // biHeight
            _writer.Write((short)1);   // biPlanes
            _writer.Write((short)24);  // biBitCount
            WriteFourCc("MJPG");       // biCompression
            _writer.Write(_frameBytes);// biSizeImage
            _writer.Write(0);          // biXPelsPerMeter
            _writer.Write(0);          // biYPelsPerMeter
            _writer.Write(0);          // biClrUsed
            _writer.Write(0);          // biClrImportant

            PatchSize(_strlSizePos);
            PatchSize(_hdrlSizePos);

            // LIST movi
            WriteFourCc("LIST");
            _moviListSizePos = _stream.Position;
            _writer.Write(0);
            WriteFourCc("movi");
            _moviFourCcPos = _stream.Position - 4;
        }

        private void PatchSize(long sizePosition)
        {
            var current = _stream.Position;
            _stream.Position = sizePosition;
            _writer.Write((int)(current - (sizePosition + 4)));
            _stream.Position = current;
        }

        /// <summary>
        /// 追加一帧 JPEG 图片数据。
        /// </summary>
        public void AddFrame(byte[] jpegBytes)
        {
            if (_closed)
            {
                throw new ObjectDisposedException(nameof(AviMjpegWriter));
            }
            if (jpegBytes == null || jpegBytes.Length == 0)
            {
                return;
            }

            long chunkStart = _stream.Position;
            WriteFourCc("00dc");
            _writer.Write(jpegBytes.Length);
            _writer.Write(jpegBytes);
            if ((jpegBytes.Length & 1) == 1)
            {
                _writer.Write((byte)0);
            }

            _frameOffsets.Add(chunkStart - _moviFourCcPos);
            _frameSizes.Add(jpegBytes.Length);
        }

        private void Finish()
        {
            long moviEnd = _stream.Position;

            // idx1
            WriteFourCc("idx1");
            int indexSize = _frameOffsets.Count * 16;
            _writer.Write(indexSize);
            for (int i = 0; i < _frameOffsets.Count; i++)
            {
                WriteFourCc("00dc");
                _writer.Write(0x10); // AVIIF_KEYFRAME
                _writer.Write((int)_frameOffsets[i]);
                _writer.Write(_frameSizes[i]);
            }

            long fileEnd = _stream.Position;

            // 回填长度
            _stream.Position = _moviListSizePos;
            _writer.Write((int)(moviEnd - (_moviListSizePos + 4)));

            _stream.Position = _avihTotalFramesPos;
            _writer.Write(_frameOffsets.Count);

            _stream.Position = _strhLengthPos;
            _writer.Write(_frameOffsets.Count);

            _stream.Position = _riffSizePos;
            _writer.Write((int)(fileEnd - 8));

            _stream.Position = fileEnd;
        }

        public void Dispose()
        {
            if (_closed)
            {
                return;
            }

            _closed = true;
            try
            {
                Finish();
            }
            finally
            {
                _writer.Flush();
                _writer.Dispose();
                _stream.Dispose();
            }
        }
    }
}
