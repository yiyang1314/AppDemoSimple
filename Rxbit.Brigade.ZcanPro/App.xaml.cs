using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Rxbit.Brigade.ZcanPro
{
    public partial class App : Application
    {
        private static readonly string CrashLogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

        private void App_Startup(object sender, StartupEventArgs e)
        {
            LogCrash("=== Application_Startup begin ===");

            // UI 线程未处理异常
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            // 非托管/后台线程未捕获异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task 中未观察的异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            try
            {
                var window = new MainWindow();
                LogCrash("MainWindow constructed, showing...");
                window.Show();
                LogCrash("MainWindow shown OK");
            }
            catch (Exception ex)
            {
                LogCrash("FATAL during startup: " + ex);
                MessageBox.Show("启动失败：" + ex.Message + "\r\n\r\n详情见 crash.log",
                    "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogCrash("DispatcherUnhandledException: " + e.Exception);
            MessageBox.Show("发生未处理异常：" + e.Exception.Message + "\r\n\r\n详情见 crash.log",
                "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogCrash("AppDomain.UnhandledException: " + (e.ExceptionObject ?? "(null)"));
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogCrash("TaskScheduler.UnobservedTaskException: " + e.Exception);
            e.SetObserved();
        }

        public static void LogCrash(string message)
        {
            try
            {
                File.AppendAllText(CrashLogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch
            {
                // 日志失败不应再抛出
            }
        }
    }
}
