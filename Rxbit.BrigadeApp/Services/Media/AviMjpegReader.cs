using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace Rxbit.BrigadeApp.Services.Media
{
    /// <summary>
    /// 读取 AVI(MJPEG) 文件：解析 hdrl/avih/strh 与 idx1，按需解码单帧 JPEG，离线可用。
    /// </summary>
    public sealed class AviMjpegReader : IDisposable
    {
        private struct FrameEntry
        {
            public long Offset;
            public int Size;
        }

        private readonly FileStream _stream;
        private readonly List<FrameEntry> _frames = new List<FrameEntry>();
        private long _moviFourCcPos = -1;
        private bool _disposed;

        public string FilePath { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public double FrameRate { get; private set; }
        public int FrameCount => _frames.Count;

        public AviMjpegReader(string filePath)
        {
            FilePath = filePath;
            _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Parse();
        }

        private void Parse()
        {
            // RIFF + size + 'AVI '
            ReadFourCc();
            _stream.Position += 4;
            if (ReadFourCc() != "AVI ")
            {
                throw new InvalidDataException("Not a valid AVI file.");
            }

            long length = _stream.Length;
            while (_stream.Position + 8 <= length)
            {
                string fourCc = ReadFourCc();
                int size = ReadInt32();
                long dataStart = _stream.Position;

                if (fourCc == "LIST")
                {
                    string listType = ReadFourCc();
                    if (listType == "hdrl")
                    {
                        ParseHdrl(dataStart + size);
                    }
                    else if (listType == "movi")
                    {
                        _moviFourCcPos = dataStart;
                    }
                    _stream.Position = dataStart + size + (size & 1);
                }
                else if (fourCc == "idx1")
                {
                    ParseIndex(dataStart, size);
                    _stream.Position = dataStart + size + (size & 1);
                }
                else
                {
                    _stream.Position = dataStart + size + (size & 1);
                }
            }

            if (_frames.Count == 0 && _moviFourCcPos >= 0)
            {
                BuildIndexByScanning();
            }

            if (Width <= 0 || Height <= 0)
            {
                throw new InvalidDataException("Cannot read video frame size.");
            }
            if (FrameRate <= 0)
            {
                FrameRate = 10;
            }
        }

        private void ParseHdrl(long end)
        {
            while (_stream.Position + 8 <= end)
            {
                string fourCc = ReadFourCc();
                int size = ReadInt32();
                long dataStart = _stream.Position;

                switch (fourCc)
                {
                    case "avih":
                        _stream.Position = dataStart;
                        _stream.Position += 4;
                        _stream.Position += 4;
                        _stream.Position += 4;
                        _stream.Position += 4;
                        _stream.Position += 4; // totalFrames
                        _stream.Position += 4; // initialFrames
                        _stream.Position += 4; // streams
                        _stream.Position += 4; // suggested buffer
                        Width = ReadInt32();
                        Height = ReadInt32();
                        break;
                    case "LIST":
                        string listType = ReadFourCc();
                        if (listType == "strl")
                        {
                            ParseStrl(dataStart + size);
                        }
                        break;
                }

                _stream.Position = dataStart + size + (size & 1);
            }
        }

        private void ParseStrl(long end)
        {
            while (_stream.Position + 8 <= end)
            {
                string fourCc = ReadFourCc();
                int size = ReadInt32();
                long dataStart = _stream.Position;

                if (fourCc == "strh")
                {
                    _stream.Position = dataStart;
                    string type = ReadFourCc();
                    if (type == "vids")
                    {
                        _stream.Position = dataStart + 20; // dwScale 位置
                        int scale = ReadInt32();
                        int rate = ReadInt32();
                        if (scale > 0 && rate > 0)
                        {
                            FrameRate = (double)rate / scale;
                        }
                    }
                }

                _stream.Position = dataStart + size + (size & 1);
            }
        }

        private void ParseIndex(long dataStart, int size)
        {
            int count = size / 16;
            _stream.Position = dataStart;
            var entries = new List<FrameEntry>();
            for (int i = 0; i < count; i++)
            {
                string id = ReadFourCc();
                ReadInt32(); // flags
                int offset = ReadInt32();
                int frameSize = ReadInt32();
                if (id.Length > 0 && id[0] >= '0' && id[0] <= '9' && id.EndsWith("dc", StringComparison.Ordinal))
                {
                    entries.Add(new FrameEntry { Offset = offset, Size = frameSize });
                }
            }

            _frames.Clear();
            _frames.AddRange(entries);
        }

        private void BuildIndexByScanning()
        {
            long length = _stream.Length;
            _stream.Position = _moviFourCcPos + 4;
            while (_stream.Position + 8 <= length)
            {
                long chunkStart = _stream.Position;
                string id = ReadFourCc();
                if (id == "idx1" || id == "LIST")
                {
                    break;
                }
                int size = ReadInt32();
                if (id.EndsWith("dc", StringComparison.Ordinal))
                {
                    _frames.Add(new FrameEntry { Offset = chunkStart - _moviFourCcPos, Size = size });
                }
                _stream.Position = chunkStart + 8 + size + (size & 1);
            }
        }

        private string ReadFourCc()
        {
            byte[] buffer = new byte[4];
            if (_stream.Read(buffer, 0, 4) < 4)
            {
                return string.Empty;
            }
            return Encoding.ASCII.GetString(buffer);
        }

        private int ReadInt32()
        {
            byte[] buffer = new byte[4];
            if (_stream.Read(buffer, 0, 4) < 4)
            {
                return 0;
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        /// 解码指定帧为可冻结的 BitmapSource。
        /// </summary>
        public BitmapSource GetFrame(int index)
        {
            if (_disposed || index < 0 || index >= _frames.Count)
            {
                return null;
            }

            var entry = _frames[index];
            _stream.Position = _moviFourCcPos + entry.Offset;
            if (ReadFourCc().Length == 0)
            {
                return null;
            }
            int size = ReadInt32();
            if (size <= 0 || size > 64 * 1024 * 1024)
            {
                return null;
            }

            byte[] data = new byte[size];
            int read = 0;
            while (read < size)
            {
                int n = _stream.Read(data, read, size - read);
                if (n <= 0) break;
                read += n;
            }
            if (read < size)
            {
                return null;
            }

            using (var ms = new MemoryStream(data))
            {
                var decoder = new JpegBitmapDecoder(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                if (decoder.Frames.Count == 0)
                {
                    return null;
                }
                var frame = decoder.Frames[0];
                frame.Freeze();
                return frame;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stream.Dispose();
        }
    }
}
