using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// 文件操作工具类。
    /// 提供文件读写、复制、移动、删除、目录操作等常用功能。
    /// </summary>
    public static class FileHelper
    {
        #region 文件读写

        /// <summary>
        /// 读取文件全部文本
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码，默认 UTF-8</param>
        /// <returns>文件文本内容</returns>
        public static string ReadAllText(string filePath, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            return File.ReadAllText(filePath, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 异步读取文件全部文本
        /// </summary>
        public static string ReadAllTextAsync(string filePath, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            // .NET Framework 4.8 没有原生的 ReadAllTextAsync，使用线程池模拟
            return System.Threading.Tasks.Task.Factory.StartNew(
                () => File.ReadAllText(filePath, encoding ?? Encoding.UTF8)).Result;
        }

        /// <summary>
        /// 写入文本到文件（覆盖）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">文本内容</param>
        /// <param name="encoding">编码，默认 UTF-8</param>
        public static void WriteAllText(string filePath, string content, Encoding encoding = null)
        {
            EnsureDirectory(filePath);
            File.WriteAllText(filePath, content, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 追加文本到文件
        /// </summary>
        public static void AppendAllText(string filePath, string content, Encoding encoding = null)
        {
            EnsureDirectory(filePath);
            File.AppendAllText(filePath, content, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件所有行
        /// </summary>
        public static string[] ReadAllLines(string filePath, Encoding encoding = null)
        {
            if (!File.Exists(filePath))
            {
                return new string[0];
            }
            return File.ReadAllLines(filePath, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 写入所有行到文件（覆盖）
        /// </summary>
        public static void WriteAllLines(string filePath, IEnumerable<string> lines, Encoding encoding = null)
        {
            EnsureDirectory(filePath);
            File.WriteAllLines(filePath, lines, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// 读取文件所有字节
        /// </summary>
        public static byte[] ReadAllBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new byte[0];
            }
            return File.ReadAllBytes(filePath);
        }

        /// <summary>
        /// 写入字节到文件（覆盖）
        /// </summary>
        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            EnsureDirectory(filePath);
            File.WriteAllBytes(filePath, bytes);
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 复制文件（可覆盖）
        /// </summary>
        public static void Copy(string sourceFile, string destFile, bool overwrite = true)
        {
            EnsureDirectory(destFile);
            File.Copy(sourceFile, destFile, overwrite);
        }

        /// <summary>
        /// 移动文件（可重命名）
        /// </summary>
        public static void Move(string sourceFile, string destFile)
        {
            EnsureDirectory(destFile);
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
            File.Move(sourceFile, destFile);
        }

        /// <summary>
        /// 删除文件（如果存在）
        /// </summary>
        public static void Delete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        /// <summary>
        /// 文件是否存在
        /// </summary>
        public static bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        #endregion

        #region 目录操作

        /// <summary>
        /// 确保文件所在目录存在
        /// </summary>
        public static void EnsureDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// 复制目录（递归）
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destDir">目标目录</param>
        /// <param name="overwrite">是否覆盖</param>
        public static void CopyDirectory(string sourceDir, string destDir, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
            {
                throw new DirectoryNotFoundException("源目录不存在: " + sourceDir);
            }

            EnsureDirectoryExists(destDir);

            // 复制文件
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, overwrite);
            }

            // 递归复制子目录
            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (string subDir in subDirs)
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectory(subDir, destSubDir, overwrite);
            }
        }

        /// <summary>
        /// 删除目录（递归）
        /// </summary>
        public static void DeleteDirectory(string directoryPath, bool recursive = true)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive);
            }
        }

        /// <summary>
        /// 获取目录下所有文件（递归可选）
        /// </summary>
        public static string[] GetFiles(string directoryPath, string searchPattern = "*", bool recursive = false)
        {
            if (!Directory.Exists(directoryPath))
            {
                return new string[0];
            }
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directoryPath, searchPattern, option);
        }

        /// <summary>
        /// 清空目录（保留目录，删除所有内容）
        /// </summary>
        public static void ClearDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            string[] files = Directory.GetFiles(directoryPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            string[] subDirs = Directory.GetDirectories(directoryPath);
            foreach (string subDir in subDirs)
            {
                Directory.Delete(subDir, true);
            }
        }

        #endregion

        #region 文件信息

        /// <summary>
        /// 获取文件大小（字节）
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return 0;
            }
            FileInfo info = new FileInfo(filePath);
            return info.Length;
        }

        /// <summary>
        /// 获取目录总大小（字节，递归）
        /// </summary>
        public static long GetDirectorySize(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return 0;
            }

            long size = 0;
            string[] files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                FileInfo info = new FileInfo(file);
                size += info.Length;
            }
            return size;
        }

        /// <summary>
        /// 格式化文件大小（自动转换为 KB/MB/GB）
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
            {
                return bytes + " B";
            }
            if (bytes < 1024 * 1024)
            {
                return (bytes / 1024.0).ToString("F2") + " KB";
            }
            if (bytes < 1024 * 1024 * 1024)
            {
                return (bytes / (1024.0 * 1024)).ToString("F2") + " MB";
            }
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }

        #endregion
    }
}
