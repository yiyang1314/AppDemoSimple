using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf.CustomControl.Utilities
{
    public static class LogHelper
    {
        #region 配置参数
        // 日志根目录
        private static readonly string _logRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        // 日志文件前缀
        private const string _logFilePrefix = "Error";
        // 文件后缀
        private const string _logFileSuffix = ".log";
        // 多线程写入锁
        private static readonly object _writeLocker = new object();
        // 当前可用日志文件路径（自动处理占用副本）
        private static string _currentLogPath;
        #endregion

        #region 静态初始化（程序启动自动获取可用日志文件）
        static LogHelper()
        {
            // 创建日志根目录
            if (!Directory.Exists(_logRootPath))
            {
                Directory.CreateDirectory(_logRootPath);
            }
            // 初始化可用日志文件
            _currentLogPath = GetValidLogFilePath();
            // 写入程序启动标记
            Info("============ 程序启动 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ============");
        }
        #endregion

        #region 核心：获取无占用的合法日志文件路径（冲突自动生成副本）
        /// <summary>
        /// 检测文件占用，被占用则生成 run_1.log / run_2.log 副本
        /// </summary>
        private static string GetValidLogFilePath()
        {
            // 按日期创建子文件夹 Logs/2026-06-26
            string dateDir = Path.Combine(_logRootPath, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(dateDir))
                Directory.CreateDirectory(dateDir);

            int fileSerial = 0;
            while (true)
            {
                string fileName = fileSerial == 0
                    ? $"{_logFilePrefix}{_logFileSuffix}"
                    : $"{_logFilePrefix}_{fileSerial}{_logFileSuffix}";

                string fullPath = Path.Combine(dateDir, fileName);
                // 文件不存在 → 直接使用
                if (!File.Exists(fullPath))
                    return fullPath;

                // 文件存在，检测是否被占用
                if (!IsFileLocked(fullPath))
                    return fullPath;

                // 文件被占用，序号+1重试
                fileSerial++;
            }
        }

        /// <summary>
        /// 检测文件是否被其他进程占用锁定
        /// </summary>
        private static bool IsFileLocked(string filePath)
        {
            FileStream stream = null;
            try
            {
                // OpenOrCreate + ReadWrite 独占模式测试占用
                stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 4096);
                return false;
            }
            catch (IOException)
            {
                // 抛出IO异常 = 文件被占用
                return true;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        }
        #endregion

        #region 底层统一写入方法（线程安全、异常捕获）
        /// <summary>
        /// 底层写入，全局锁+try-catch兜底，绝不闪退
        /// </summary>
        private static void WriteLog(string level, string content, Exception ex = null)
        {
            lock (_writeLocker) // 多线程排队写入，日志不穿插错乱
            {
                try
                {
                    // 拼接日志内容
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] [{level}] ");
                    sb.AppendLine(content);

                    // 追加异常堆栈（报错日志）
                    if (ex != null)
                    {
                        sb.AppendLine($"【异常堆栈】{ex.Message}");
                        sb.AppendLine(ex.StackTrace);
                        sb.AppendLine("----------------------------------------");
                    }

                    // 共享读取模式打开文件，允许其他软件只读打开，仅我方写入
                    using (FileStream fs = new FileStream(_currentLogPath, FileMode.Append, FileAccess.Write, FileShare.Read, 4096))
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.Write(sb.ToString());
                    }
                }
                catch (IOException ioEx)
                {
                    // 当前文件彻底无法写入（持续占用/权限不足），重新获取新副本文件再重试一次
                    try
                    {
                        _currentLogPath = GetValidLogFilePath();
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"[{DateTime.Now:HH:mm:ss.fff}] [Warn] 原日志文件占用，切换新文件：{_currentLogPath}");
                        sb.AppendLine(content);
                        if (ex != null) sb.AppendLine(ex.ToString());

                        using (FileStream fs = new FileStream(_currentLogPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                        using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            sw.Write(sb.ToString());
                        }
                    }
                    catch (Exception innerEx)
                    {
                        // 双重失败兜底：输出到控制台，保证程序不崩溃
                        Console.WriteLine($"日志写入双重失败：{ioEx.Message} | {innerEx.Message}");
                    }
                }
                catch (Exception globalEx)
                {
                    // 所有未知异常兜底捕获，防止WPF程序直接闪退
                    Console.WriteLine($"日志写入未知异常：{globalEx.Message}");
                }
            }
        }
        #endregion

        #region 对外分级日志API
        /// <summary>调试信息</summary>
        public static void Debug(string msg) => WriteLog("DEBUG", msg);

        /// <summary>普通信息</summary>
        public static void Info(string msg) => WriteLog("INFO", msg);

        /// <summary>警告</summary>
        public static void Warn(string msg) => WriteLog("WARN", msg);

        /// <summary>错误（附带异常）</summary>
        public static void Error(string msg, Exception ex = null) => WriteLog("ERROR", msg, ex);

        /// <summary>致命崩溃</summary>
        public static void Fatal(string msg, Exception ex = null) => WriteLog("FATAL", msg, ex);
        #endregion
    }
}
