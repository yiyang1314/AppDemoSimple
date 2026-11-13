using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Rxbit.Brigade.HalconVision
{
    public partial class App : Application
    {
        private static readonly string LogPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += App_DomainUnhandledException;
            TaskScheduler.UnobservedTaskException += App_UnobservedTaskException;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                WriteLog("=== Application_Startup begin ===");
                var window = new MainWindow();
                WriteLog("MainWindow constructed, showing...");
                window.Show();
                WriteLog("MainWindow shown OK");
            }
            catch (Exception ex)
            {
                WriteLog("FATAL in Application_Startup: " + ex);
                MessageBox.Show("启动失败：" + ex.Message + "\n\n详情见 crash.log", "HalconVision",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteLog("DispatcherUnhandledException: " + e.Exception);
            e.Handled = true;
        }

        private void App_DomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteLog("AppDomain.UnhandledException: " + (e.ExceptionObject as Exception));
        }

        private void App_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            WriteLog("UnobservedTaskException: " + e.Exception);
            e.SetObserved();
        }

        private static void WriteLog(string message)
        {
            try
            {
                File.AppendAllText(LogPath,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
            catch
            {
                // ignore
            }
        }
    }
}
