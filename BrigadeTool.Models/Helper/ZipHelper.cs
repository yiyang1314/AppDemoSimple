using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Rxbit.Brigade.Models.Helper
{
    /// <summary>
    /// 原生Zip压缩解压工具（兼容.NET Framework 4.8
    /// </summary>
    public static class ZipHelper
    {
        /// <summary>压缩整个文件夹</summary>
        public static void CompressDirectory(string sourceDir, string zipFilePath, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException("待压缩目录不存在：" + sourceDir);

            if (File.Exists(zipFilePath))
            {
                if (overwrite)
                    File.Delete(zipFilePath);
                else
                    throw new IOException("压缩文件已存在，禁止覆盖");
            }
            ZipFile.CreateFromDirectory(sourceDir, zipFilePath, CompressionLevel.Optimal, true);
        }

        /// <summary>解压Zip，兼容.NET Framework，手动处理文件覆盖</summary>
        public static void ExtractZip(string zipFilePath, string targetDir, bool overwriteFile = true)
        {
            if (!File.Exists(zipFilePath))
                throw new FileNotFoundException("压缩包不存在", zipFilePath);
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            // 兼容.NET Framework：不使用三参重载，手动遍历条目控制覆盖
            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string fullPath = Path.Combine(targetDir, entry.FullName);
                    string entryDir = Path.GetDirectoryName(fullPath);

                    // 创建子文件夹
                    if (!Directory.Exists(entryDir))
                        Directory.CreateDirectory(entryDir);

                    // 跳过目录条目（只处理文件）
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    // 文件存在且不允许覆盖则跳过
                    if (File.Exists(fullPath) && !overwriteFile)
                        continue;

                    // 覆盖/写入文件
                    entry.ExtractToFile(fullPath, overwriteFile);
                }
            }
        }

        /// <summary>批量打包指定文件列表</summary>
        public static void CompressFiles(IEnumerable<string> filePaths, string zipOutputPath)
        {
            using FileStream fs = new FileStream(zipOutputPath, FileMode.Create);
            using ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create);

            foreach (string file in filePaths)
            {
                if (!File.Exists(file)) continue;
                string fileName = Path.GetFileName(file);
                ZipArchiveEntry entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);

                using Stream entryStream = entry.Open();
                using FileStream fileStream = File.OpenRead(file);
                fileStream.CopyTo(entryStream);
            }
        }

        /// <summary>
        /// demo 测试
        /// </summary>
        public static void Test() 
        {
            // Zip 解压覆盖
            ZipHelper.ExtractZip(@"D:\test.zip", @"D:\unzip", overwriteFile: true);
            // Zip 解压不覆盖已有文件
            ZipHelper.ExtractZip(@"D:\test.zip", @"D:\unzip", overwriteFile: false);

            // 压缩文件夹
            ZipHelper.CompressDirectory(@"D:\FlashFiles", @"D:\backup.zip");

            // 解压
            ZipHelper.ExtractZip(@"D:\backup.zip", @"D:\UnzipDir");

            // 打包多个单独文件
            List<string> files = new List<string> { @"D:\a.bin", @"D:\log.txt" };
            ZipHelper.CompressFiles(files, @"D:\filePack.zip");
        }
    }
}
