using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Rxbit.Brigade.Models.Helper
{
    public static class PngToIcoSameSize
    {
        /// <summary>
        /// 原图尺寸不变，直接导出ICO（宽高与输入PNG完全一致）
        /// </summary>
        /// <param name="pngFilePath">PNG文件路径</param>
        /// <param name="icoSavePath">输出ICO路径</param>
        public static void ConvertNoResize(string pngFilePath, string icoSavePath)
        {
            if (!File.Exists(pngFilePath))
                throw new FileNotFoundException("文件不存在", pngFilePath);

            // 加载原图，不缩放
            BitmapImage sourceBmp = new BitmapImage(new Uri(pngFilePath));
            int originalW = (int)sourceBmp.PixelWidth;
            int originalH = (int)sourceBmp.PixelHeight;

            // 把原图编码为PNG二进制
            MemoryStream pngBuffer = new MemoryStream();
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(sourceBmp));
            pngEncoder.Save(pngBuffer);
            byte[] rawPngData = pngBuffer.ToArray();

            // 构造标准ICO文件
            using FileStream fs = new FileStream(icoSavePath, FileMode.Create, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fs);

            // 1. ICO 文件头 6字节
            bw.Write((short)0);       // 保留位
            bw.Write((short)1);       // ICO标识
            bw.Write((short)1);       // 图标数量=1

            // 2. 目录项 16字节
            // ICO规范：256尺寸用0填充，其他写实际宽高
            bw.Write((byte)(originalW == 256 ? 0 : originalW));
            bw.Write((byte)(originalH == 256 ? 0 : originalH));
            bw.Write((byte)0);        // 调色板颜色数
            bw.Write((byte)0);        // 保留
            bw.Write((short)0);       // 色彩平面
            bw.Write((short)32);      // 32位ARGB透明
            bw.Write(rawPngData.Length); // 图像字节长度
            bw.Write(6 + 16);         // 图像数据起始偏移（头6 + 目录16）

            // 3. 写入原图PNG数据
            bw.Write(rawPngData);
            bw.Flush();
            bw.Close();
        }
    }

    public static class PngToIcoConverter
    {
        #region 1. 简易：单张PNG转单尺寸ICO
        /// <summary>
        /// 单个PNG图片输出单尺寸ICO
        /// </summary>
        /// <param name="pngFilePath">PNG文件路径</param>
        /// <param name="icoSavePath">输出ico路径</param>
        public static void SinglePngToIco(string pngFilePath, string icoSavePath)
        {
            if (!File.Exists(pngFilePath))
                throw new FileNotFoundException("PNG文件不存在", pngFilePath);

            // WPF读取PNG透明位图
            var wpfBitmap = new BitmapImage(new Uri(pngFilePath));
            int width = (int)wpfBitmap.PixelWidth;
            int height = (int)wpfBitmap.PixelHeight;

            // 转换为GDI+ Bitmap
            using var gdiBmp = WpfBitmapToGdiBitmap(wpfBitmap);
            using var ms = new MemoryStream();
            // GDI直接保存为ICO流
            gdiBmp.Save(ms, ImageFormat.Icon);
            ms.Position = 0;

            // 写入目标ico文件
            File.WriteAllBytes(icoSavePath, ms.ToArray());
        }
        #endregion

        #region 2. 标准多尺寸ICO（Windows系统标准，推荐）
        /// <summary>
        /// 生成包含多尺寸的标准ICO文件（16,32,48,64,256）
        /// </summary>
        /// <param name="sourcePngPath">原图PNG（建议原图≥256px）</param>
        /// <param name="outputIcoPath">输出ico</param>
        public static void CreateMultiSizeIco(string sourcePngPath, string outputIcoPath)
        {
            int[] iconSizes = { 16, 32, 48, 64, 256 };
            var wpfSource = new BitmapImage(new Uri(sourcePngPath));
            var bitmaps = new Bitmap[iconSizes.Length];

            // 缩放生成各尺寸位图
            for (int i = 0; i < iconSizes.Length; i++)
            {
                int size = iconSizes[i];
                var resizedWpf = ResizeWpfBitmap(wpfSource, size, size);
                bitmaps[i] = WpfBitmapToGdiBitmap(resizedWpf);
            }

            // 组合多尺寸ICO
            SaveMultiSizeIco(bitmaps, outputIcoPath);

            // 释放GDI资源
            foreach (var bmp in bitmaps)
                bmp.Dispose();
        }
        #endregion

        #region 内部工具方法
        /// <summary>
        /// WPF BitmapImage 缩放至指定宽高
        /// </summary>
        /// <summary>
        /// WPF BitmapImage 缩放至指定宽高
        /// </summary>
        private static BitmapSource ResizeWpfBitmap(BitmapImage source, int targetW, int targetH)
        {
            var drawingVisual = new System.Windows.Media.DrawingVisual();
            using var dc = drawingVisual.RenderOpen();
            dc.DrawImage(source, new System.Windows.Rect(0, 0, targetW, targetH));
            dc.Close();

            var renderBmp = new RenderTargetBitmap(targetW, targetH, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            renderBmp.Render(drawingVisual);
            return renderBmp;
        }

        /// <summary>
        /// WPF BitmapImage → GDI+ System.Drawing.Bitmap（保留透明通道）
        /// </summary>
        private static Bitmap WpfBitmapToGdiBitmap(BitmapSource wpfBmp)
        {
            int w = (int)wpfBmp.PixelWidth;
            int h = (int)wpfBmp.PixelHeight;
            var stride = w * 4;
            byte[] pixelData = new byte[h * stride];

            wpfBmp.CopyPixels(pixelData, stride, 0);

            var gdiBmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // 修正拼写 Rectangle
            var bmpData = gdiBmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Runtime.InteropServices.Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            gdiBmp.UnlockBits(bmpData);
            return gdiBmp;
        }

        /// <summary>
        /// 多Bitmap合并写入标准ICO文件
        /// </summary>
        private static void SaveMultiSizeIco(Bitmap[] images, string savePath)
        {
            using var stream = new FileStream(savePath, FileMode.Create);
            // ICO 文件头结构
            byte[] icoHeader = { 0, 0, 1, 0, (byte)images.Length, 0 };
            stream.Write(icoHeader, 0, icoHeader.Length);

            long imageDataOffset = 6 + images.Length * 16;
            foreach (var bmp in images)
            {
                int width = bmp.Width == 256 ? 0 : bmp.Width;
                int height = bmp.Height == 256 ? 0 : bmp.Height;

                using var imgStream = new MemoryStream();
                bmp.Save(imgStream, ImageFormat.Png);
                byte[] imgBytes = imgStream.ToArray();
                int imgSize = imgBytes.Length;

                // ICO 目录项 16字节
                byte[] dirEntry = new byte[16];
                dirEntry[0] = (byte)width;
                dirEntry[1] = (byte)height;
                dirEntry[2] = 0; dirEntry[3] = 0;
                BitConverter.GetBytes((short)0).CopyTo(dirEntry, 4);
                BitConverter.GetBytes((short)32).CopyTo(dirEntry, 6);
                BitConverter.GetBytes(imgSize).CopyTo(dirEntry, 8);
                BitConverter.GetBytes((int)imageDataOffset).CopyTo(dirEntry, 12);
                stream.Write(dirEntry, 0, 16);

                imageDataOffset += imgSize;
            }

            // 写入所有图片二进制数据
            foreach (var bmp in images)
            {
                using var imgStream = new MemoryStream();
                bmp.Save(imgStream, ImageFormat.Png);
                byte[] imgBytes = imgStream.ToArray();
                stream.Write(imgBytes, 0, imgBytes.Length);
            }
        }
        #endregion


        public static async Task<bool> ConvertIco()
        {
            OpenFileDialog openDlg = new OpenFileDialog
            {
                Filter = "PNG图片 (*.png)|*.png",
                Title = "选择PNG图标"
            };
            if (openDlg.ShowDialog() != true) return false;

            SaveFileDialog saveDlg = new SaveFileDialog
            {
                Filter = "ICO图标 (*.ico)|*.ico",
                FileName = "icon.ico"
            };
            if (saveDlg.ShowDialog() != true) return false;

            try
            {
                PngToIcoSameSize.ConvertNoResize(openDlg.FileName, saveDlg.FileName);
                MessageBox.Show($"转换完成，输出尺寸与原图完全一致！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转换失败：{ex.Message}");
            }
            return true;
        }

        public static async Task<bool> ConvertIco(string srcImage, string icoPath)
        {
            try
            {
                PngToIcoSameSize.ConvertNoResize(srcImage, icoPath);
               
                MessageBox.Show($"转换完成，输出尺寸与原图完全一致！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"转换失败：{ex.Message}");
            }
            return true;
        }
    }
}
