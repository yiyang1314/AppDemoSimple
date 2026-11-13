using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SD = System.Drawing;
using SDI = System.Drawing.Imaging;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// 图像处理工具类。
    /// 提供 buffer 转 ImageSource、Bitmap 转 ImageSource、
    /// BMP/PNG 文件保存、PNG 转 ICO 等常用图像处理功能。
    /// </summary>
    public static class ImageHelper
    {
        #region Buffer (byte[]) 转 ImageSource

        /// <summary>
        /// 将字节数组转换为 ImageSource（BitmapImage）。
        /// 支持常见格式：PNG、JPEG、BMP、GIF、TIFF 等。
        /// </summary>
        /// <param name="buffer">图像字节数据</param>
        /// <param name="freeze">是否冻结（冻结后可跨线程访问，无法修改）</param>
        /// <returns>BitmapImage 实例；buffer 为空返回 null</returns>
        public static BitmapImage BufferToImageSource(byte[] buffer, bool freeze = true)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }

            BitmapImage image = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
            }

            if (freeze && image.CanFreeze)
            {
                image.Freeze();
            }
            return image;
        }

        /// <summary>
        /// 将流转换为 ImageSource（BitmapImage）
        /// </summary>
        /// <param name="stream">图像流</param>
        /// <param name="freeze">是否冻结</param>
        /// <returns>BitmapImage 实例</returns>
        public static BitmapImage StreamToImageSource(Stream stream, bool freeze = true)
        {
            if (stream == null)
            {
                return null;
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();

            if (freeze && image.CanFreeze)
            {
                image.Freeze();
            }
            return image;
        }

        /// <summary>
        /// 从文件加载图像为 ImageSource
        /// </summary>
        /// <param name="filePath">图像文件路径</param>
        /// <param name="freeze">是否冻结</param>
        /// <returns>BitmapImage 实例</returns>
        public static BitmapImage LoadFromFile(string filePath, bool freeze = true)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(filePath, UriKind.Absolute);
            image.EndInit();

            if (freeze && image.CanFreeze)
            {
                image.Freeze();
            }
            return image;
        }

        #endregion

        #region System.Drawing.Bitmap 转 ImageSource

        /// <summary>
        /// 将 System.Drawing.Bitmap 转换为 ImageSource（BitmapSource）。
        /// 使用 BitmapSource.Create 保证像素格式正确。
        /// </summary>
        /// <param name="bitmap">System.Drawing.Bitmap 实例</param>
        /// <param name="freeze">是否冻结</param>
        /// <returns>BitmapSource 实例</returns>
        public static BitmapSource BitmapToImageSource(SD.Bitmap bitmap, bool freeze = true)
        {
            if (bitmap == null)
            {
                return null;
            }

            SDI.BitmapData bmpData = bitmap.LockBits(
                new SD.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                SDI.ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            BitmapSource bitmapSource = null;
            try
            {
                System.Windows.Media.PixelFormat pixelFormat = ConvertPixelFormat(bitmap.PixelFormat);
                int stride = bmpData.Stride;
                bitmapSource = BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    pixelFormat,
                    null,
                    bmpData.Scan0,
                    stride * bitmap.Height,
                    stride);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }

            if (freeze && bitmapSource.CanFreeze)
            {
                bitmapSource.Freeze();
            }
            return bitmapSource;
        }

        /// <summary>
        /// 将 System.Drawing.Bitmap 转换为 ImageSource（使用 HBitmap 方式，兼容性更好）
        /// </summary>
        /// <param name="bitmap">System.Drawing.Bitmap 实例</param>
        /// <param name="freeze">是否冻结</param>
        /// <returns>BitmapSource 实例</returns>
        public static BitmapSource BitmapToImageSourceByHBitmap(SD.Bitmap bitmap, bool freeze = true)
        {
            if (bitmap == null)
            {
                return null;
            }

            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSource bitmapSource = null;
            try
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            if (freeze && bitmapSource.CanFreeze)
            {
                bitmapSource.Freeze();
            }
            return bitmapSource;
        }

        /// <summary>
        /// 将 ImageSource 转换为 System.Drawing.Bitmap
        /// </summary>
        public static SD.Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource bitmapSource = imageSource as BitmapSource;
            if (bitmapSource == null)
            {
                return null;
            }

            int width = bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int stride = width * ((bitmapSource.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[height * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);

            SD.Bitmap bitmap = new SD.Bitmap(width, height, SDI.PixelFormat.Format32bppPArgb);
            SDI.BitmapData bmpData = bitmap.LockBits(
                new SD.Rectangle(0, 0, width, height),
                SDI.ImageLockMode.WriteOnly,
                bitmap.PixelFormat);
            try
            {
                Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }
            return bitmap;
        }

        #endregion

        #region 文件保存（BMP / PNG）

        /// <summary>
        /// 将 BitmapSource 保存为 PNG 文件
        /// </summary>
        /// <param name="source">图像源</param>
        /// <param name="filePath">目标文件路径</param>
        public static void SaveAsPng(BitmapSource source, string filePath)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            SaveWithEncoder(source, filePath, new PngBitmapEncoder());
        }

        /// <summary>
        /// 将 BitmapSource 保存为 BMP 文件
        /// </summary>
        /// <param name="source">图像源</param>
        /// <param name="filePath">目标文件路径</param>
        public static void SaveAsBmp(BitmapSource source, string filePath)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            SaveWithEncoder(source, filePath, new BmpBitmapEncoder());
        }

        /// <summary>
        /// 将 BitmapSource 保存为 JPEG 文件
        /// </summary>
        /// <param name="source">图像源</param>
        /// <param name="filePath">目标文件路径</param>
        /// <param name="qualityLevel">JPEG 质量（1-100）</param>
        public static void SaveAsJpeg(BitmapSource source, string filePath, int qualityLevel = 90)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = qualityLevel;
            SaveWithEncoder(source, filePath, encoder);
        }

        /// <summary>
        /// 将 System.Drawing.Bitmap 保存为 PNG 文件
        /// </summary>
        public static void SaveBitmapAsPng(SD.Bitmap bitmap, string filePath)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }
            FileHelper.EnsureDirectory(filePath);
            bitmap.Save(filePath, SDI.ImageFormat.Png);
        }

        /// <summary>
        /// 将 System.Drawing.Bitmap 保存为 BMP 文件
        /// </summary>
        public static void SaveBitmapAsBmp(SD.Bitmap bitmap, string filePath)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }
            FileHelper.EnsureDirectory(filePath);
            bitmap.Save(filePath, SDI.ImageFormat.Bmp);
        }

        /// <summary>
        /// 根据文件扩展名自动选择编码器保存
        /// </summary>
        /// <param name="source">图像源</param>
        /// <param name="filePath">目标文件路径（支持 .png/.bmp/.jpg/.jpeg/.tif/.tiff/.gif）</param>
        public static void SaveAuto(BitmapSource source, string filePath)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            BitmapEncoder encoder;
            switch (ext)
            {
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                case ".jpg":
                case ".jpeg":
                    encoder = new JpegBitmapEncoder();
                    ((JpegBitmapEncoder)encoder).QualityLevel = 90;
                    break;
                case ".tif":
                case ".tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                case ".gif":
                    encoder = new GifBitmapEncoder();
                    break;
                default:
                    throw new NotSupportedException("不支持的图像格式: " + ext);
            }

            SaveWithEncoder(source, filePath, encoder);
        }

        private static void SaveWithEncoder(BitmapSource source, string filePath, BitmapEncoder encoder)
        {
            FileHelper.EnsureDirectory(filePath);
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                encoder.Save(fs);
            }
        }

        #endregion

        #region PNG 转 ICO 文件

        /// <summary>
        /// 将 PNG 文件转换为 ICO 文件（嵌入 PNG 数据）。
        /// 适用于现代应用程序图标（支持透明度、32 位色深）。
        /// </summary>
        /// <param name="pngFilePath">源 PNG 文件路径</param>
        /// <param name="icoFilePath">目标 ICO 文件路径</param>
        /// <param name="size">图标尺寸（默认 32x32）</param>
        public static void PngToIco(string pngFilePath, string icoFilePath, int size = 32)
        {
            if (!File.Exists(pngFilePath))
            {
                throw new FileNotFoundException("PNG 文件不存在: " + pngFilePath);
            }

            byte[] pngBytes = File.ReadAllBytes(pngFilePath);
            PngBytesToIco(pngBytes, icoFilePath, size);
        }

        /// <summary>
        /// 将 BitmapSource 转换为 ICO 文件（先转为 PNG 嵌入）
        /// </summary>
        /// <param name="source">图像源</param>
        /// <param name="icoFilePath">目标 ICO 文件路径</param>
        /// <param name="size">图标尺寸（默认 32x32）</param>
        public static void ImageSourceToIco(BitmapSource source, string icoFilePath, int size = 32)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            // 转换为 PNG 字节
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(source));
            byte[] pngBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                pngEncoder.Save(ms);
                pngBytes = ms.ToArray();
            }

            PngBytesToIco(pngBytes, icoFilePath, size);
        }

        /// <summary>
        /// 将 System.Drawing.Bitmap 转换为 ICO 文件
        /// </summary>
        /// <param name="bitmap">源 Bitmap</param>
        /// <param name="icoFilePath">目标 ICO 文件路径</param>
        public static void BitmapToIco(SD.Bitmap bitmap, string icoFilePath)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            FileHelper.EnsureDirectory(icoFilePath);

            using (FileStream fs = new FileStream(icoFilePath, FileMode.Create, FileAccess.Write))
            {
                // 使用 Icon 类保存（保留原始位图大小）
                IntPtr hIcon = bitmap.GetHicon();
                try
                {
                    SD.Icon icon = SD.Icon.FromHandle(hIcon);
                    icon.Save(fs);
                }
                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }

        /// <summary>
        /// 将多个尺寸的 PNG 合并为多尺寸 ICO 文件
        /// </summary>
        /// <param name="pngFilePaths">PNG 文件路径列表（不同尺寸）</param>
        /// <param name="icoFilePath">目标 ICO 文件路径</param>
        public static void PngsToMultiSizeIco(string[] pngFilePaths, string icoFilePath)
        {
            if (pngFilePaths == null || pngFilePaths.Length == 0)
            {
                throw new ArgumentException("PNG 文件列表不能为空");
            }

            FileHelper.EnsureDirectory(icoFilePath);

            using (FileStream fs = new FileStream(icoFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // ICONDIR 头部
                writer.Write((ushort)0);        // 保留字段，必须为 0
                writer.Write((ushort)1);        // 类型，1 表示图标
                writer.Write((ushort)pngFilePaths.Length);  // 图像数量

                // 计算各图像数据偏移量
                int offset = 6 + pngFilePaths.Length * 16;
                var imageDataList = new System.Collections.Generic.List<byte[]>();

                foreach (string pngPath in pngFilePaths)
                {
                    if (!File.Exists(pngPath))
                    {
                        continue;
                    }

                    byte[] pngBytes = File.ReadAllBytes(pngPath);
                    imageDataList.Add(pngBytes);

                    // 获取尺寸
                    int width, height;
                    GetPngDimensions(pngBytes, out width, out height);

                    // ICONDIRENTRY（每个 16 字节）
                    writer.Write((byte)(width >= 256 ? 0 : width));    // 宽度（256 用 0 表示）
                    writer.Write((byte)(height >= 256 ? 0 : height));  // 高度（256 用 0 表示）
                    writer.Write((byte)0);                              // 调色板颜色数（0 表示无调色板）
                    writer.Write((byte)0);                              // 保留字段
                    writer.Write((ushort)1);                            // 色彩平面数
                    writer.Write((ushort)32);                           // 每像素位数
                    writer.Write((uint)pngBytes.Length);                // 图像数据大小
                    writer.Write((uint)offset);                         // 图像数据偏移

                    offset += pngBytes.Length;
                }

                // 写入所有图像数据
                foreach (byte[] data in imageDataList)
                {
                    writer.Write(data);
                }
            }
        }

        /// <summary>
        /// 将 PNG 字节数据嵌入 ICO 文件
        /// </summary>
        private static void PngBytesToIco(byte[] pngBytes, string icoFilePath, int size)
        {
            FileHelper.EnsureDirectory(icoFilePath);

            // 获取 PNG 实际尺寸
            int width, height;
            GetPngDimensions(pngBytes, out width, out height);

            using (FileStream fs = new FileStream(icoFilePath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // ICONDIR 头部（6 字节）
                writer.Write((ushort)0);        // 保留
                writer.Write((ushort)1);        // 类型：1 = ICO
                writer.Write((ushort)1);        // 图像数量：1

                // ICONDIRENTRY（16 字节）
                writer.Write((byte)(width >= 256 ? 0 : width));        // 宽度
                writer.Write((byte)(height >= 256 ? 0 : height));      // 高度
                writer.Write((byte)0);                                  // 调色板颜色数
                writer.Write((byte)0);                                  // 保留
                writer.Write((ushort)1);                                // 色彩平面数
                writer.Write((ushort)32);                               // 每像素位数
                writer.Write((uint)pngBytes.Length);                    // 图像数据大小
                writer.Write((uint)22);                                 // 图像数据偏移（6 + 16 = 22）

                // 图像数据
                writer.Write(pngBytes);
            }
        }

        /// <summary>
        /// 从 PNG 字节数据中读取尺寸
        /// </summary>
        private static void GetPngDimensions(byte[] pngBytes, out int width, out int height)
        {
            width = 0;
            height = 0;

            // PNG 文件头：8 字节签名 + IHDR 数据块
            // IHDR 前 4 字节为宽度，后 4 字节为高度（大端序）
            if (pngBytes.Length >= 24 &&
                pngBytes[0] == 0x89 && pngBytes[1] == 0x50 && pngBytes[2] == 0x4E && pngBytes[3] == 0x47)
            {
                width = (pngBytes[16] << 24) | (pngBytes[17] << 16) | (pngBytes[18] << 8) | pngBytes[19];
                height = (pngBytes[20] << 24) | (pngBytes[21] << 16) | (pngBytes[22] << 8) | pngBytes[23];
            }

            if (width <= 0) width = 32;
            if (height <= 0) height = 32;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// System.Drawing.Imaging.PixelFormat 转 System.Windows.Media.PixelFormat
        /// </summary>
        private static System.Windows.Media.PixelFormat ConvertPixelFormat(SDI.PixelFormat format)
        {
            if (format == SDI.PixelFormat.Format32bppArgb)
            {
                return PixelFormats.Bgra32;
            }
            if (format == SDI.PixelFormat.Format32bppRgb)
            {
                return PixelFormats.Bgr32;
            }
            if (format == SDI.PixelFormat.Format24bppRgb)
            {
                return PixelFormats.Bgr24;
            }
            if (format == SDI.PixelFormat.Format8bppIndexed)
            {
                return PixelFormats.Indexed8;
            }
            if (format == SDI.PixelFormat.Format16bppRgb555)
            {
                return PixelFormats.Bgr555;
            }
            if (format == SDI.PixelFormat.Format16bppRgb565)
            {
                return PixelFormats.Bgr565;
            }
            // 默认使用 Bgra32（保证 Alpha 通道）
            return PixelFormats.Bgra32;
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr handle);

        #endregion
    }
}
