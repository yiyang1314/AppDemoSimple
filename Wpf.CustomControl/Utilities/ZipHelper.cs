using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// ZIP 压缩/解压工具类。
    /// 基于 System.IO.Compression.ZipArchive，支持单文件、目录、覆盖等操作。
    /// </summary>
    public static class ZipHelper
    {
        #region 压缩

        /// <summary>
        /// 压缩单个文件到 ZIP 文件
        /// </summary>
        /// <param name="sourceFile">源文件路径</param>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="entryName">ZIP 内条目名（默认为源文件名）</param>
        /// <param name="overwrite">是否覆盖已存在的 ZIP 文件</param>
        public static void CompressFile(string sourceFile, string zipPath,
            string entryName = null, bool overwrite = true)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException("源文件不存在: " + sourceFile);
            }
            if (string.IsNullOrEmpty(zipPath))
            {
                throw new ArgumentNullException("zipPath");
            }

            FileHelper.EnsureDirectory(zipPath);
            if (overwrite && File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            if (string.IsNullOrEmpty(entryName))
            {
                entryName = Path.GetFileName(sourceFile);
            }

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                using (Stream entryStream = entry.Open())
                using (FileStream fileStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        /// <summary>
        /// 压缩整个目录到 ZIP 文件
        /// </summary>
        /// <param name="sourceDir">源目录路径</param>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="includeBaseDir">是否在 ZIP 中包含源目录名作为根目录</param>
        /// <param name="overwrite">是否覆盖已存在的 ZIP 文件</param>
        public static void CompressDirectory(string sourceDir, string zipPath,
            bool includeBaseDir = false, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException("源目录不存在: " + sourceDir);
            }
            if (string.IsNullOrEmpty(zipPath))
            {
                throw new ArgumentNullException("zipPath");
            }

            FileHelper.EnsureDirectory(zipPath);
            if (overwrite && File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            string basePath = includeBaseDir ? Directory.GetParent(sourceDir).FullName : sourceDir;

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                string[] files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string entryName = GetRelativePath(basePath, file);
                    // 统一路径分隔符为 ZIP 标准的 '/'
                    entryName = entryName.Replace('\\', '/');

                    ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (Stream entryStream = entry.Open())
                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }

        /// <summary>
        /// 压缩多个文件到 ZIP 文件
        /// </summary>
        /// <param name="sourceFiles">源文件列表</param>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的 ZIP 文件</param>
        public static void CompressFiles(IEnumerable<string> sourceFiles, string zipPath, bool overwrite = true)
        {
            if (string.IsNullOrEmpty(zipPath))
            {
                throw new ArgumentNullException("zipPath");
            }

            FileHelper.EnsureDirectory(zipPath);
            if (overwrite && File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (string file in sourceFiles)
                {
                    if (!File.Exists(file))
                    {
                        continue;
                    }
                    string entryName = Path.GetFileName(file);
                    ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using (Stream entryStream = entry.Open())
                    using (FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }

        #endregion

        #region 解压

        /// <summary>
        /// 解压 ZIP 文件到指定目录
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="destDir">目标目录</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void ExtractToDirectory(string zipPath, string destDir, bool overwrite = true)
        {
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException("ZIP 文件不存在: " + zipPath);
            }
            if (string.IsNullOrEmpty(destDir))
            {
                throw new ArgumentNullException("destDir");
            }

            FileHelper.EnsureDirectoryExists(destDir);

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destPath = Path.Combine(destDir, entry.FullName);

                    // 防止路径穿越攻击
                    string fullDestDir = Path.GetFullPath(destDir);
                    string fullDestPath = Path.GetFullPath(destPath);
                    if (!fullDestPath.StartsWith(fullDestDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // 目录条目
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(destPath);
                        continue;
                    }

                    // 文件条目
                    FileHelper.EnsureDirectory(destPath);
                    if (!overwrite && File.Exists(destPath))
                    {
                        continue;
                    }

                    entry.ExtractToFile(destPath, overwrite);
                }
            }
        }

        /// <summary>
        /// 解压 ZIP 中的指定条目到文件
        /// </summary>
        /// <param name="zipPath">ZIP 文件路径</param>
        /// <param name="entryName">条目名</param>
        /// <param name="destFile">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在的文件</param>
        public static void ExtractFile(string zipPath, string entryName, string destFile, bool overwrite = true)
        {
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException("ZIP 文件不存在: " + zipPath);
            }

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = archive.GetEntry(entryName);
                if (entry == null)
                {
                    throw new KeyNotFoundException("ZIP 中未找到条目: " + entryName);
                }

                FileHelper.EnsureDirectory(destFile);
                entry.ExtractToFile(destFile, overwrite);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取 ZIP 文件中所有条目名
        /// </summary>
        public static List<string> GetEntries(string zipPath)
        {
            List<string> result = new List<string>();
            if (!File.Exists(zipPath))
            {
                return result;
            }

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    result.Add(entry.FullName);
                }
            }
            return result;
        }

        /// <summary>
        /// 检查 ZIP 文件中是否包含指定条目
        /// </summary>
        public static bool ContainsEntry(string zipPath, string entryName)
        {
            if (!File.Exists(zipPath))
            {
                return false;
            }

            using (FileStream zipStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                return archive.GetEntry(entryName) != null;
            }
        }

        #endregion

        #region 辅助方法

        private static string GetRelativePath(string basePath, string fullPath)
        {
            string baseFullPath = Path.GetFullPath(basePath);
            string fullFullPath = Path.GetFullPath(fullPath);

            if (!fullFullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(fullPath);
            }

            // 去掉基础路径（包括末尾分隔符）
            int baseLength = baseFullPath.Length;
            if (!baseFullPath.EndsWith(Path.DirectorySeparatorChar.ToString()) &&
                !baseFullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
            {
                baseLength++;
            }

            return fullFullPath.Substring(baseLength);
        }

        #endregion
    }
}
