using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Rxbit.BrigadeApp.Services.Media
{
    /// <summary>
    /// JPEG 帧编码辅助：屏幕采集用 GDI+，摄像头帧用 WPF 编码器。
    /// </summary>
    public static class MjpegFrameEncoder
    {
        private static readonly ImageCodecInfo JpegCodec = FindJpegCodec();

        private static ImageCodecInfo FindJpegCodec()
        {
            foreach (var codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static byte[] Encode(Bitmap bitmap, long quality)
        {
            if (bitmap == null || JpegCodec == null)
            {
                return null;
            }

            using (var encoderParams = new EncoderParameters(1))
            using (var qualityParam = new EncoderParameter(Encoder.Quality, quality))
            {
                encoderParams.Param[0] = qualityParam;
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, JpegCodec, encoderParams);
                    return ms.ToArray();
                }
            }
        }

        public static byte[] Encode(BitmapSource source, int quality)
        {
            if (source == null)
            {
                return null;
            }

            var encoder = new JpegBitmapEncoder { QualityLevel = quality };
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }
    }
}
