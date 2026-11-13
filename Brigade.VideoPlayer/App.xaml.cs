using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Brigade.VideoPlayer
{
    public partial class App : System.Windows.Application
    {
        private string _logFile;

        protected override void OnStartup(StartupEventArgs e)
        {
            _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Log("App.OnStartup started");

            try
            {
                base.OnStartup(e);
                Log("App.OnStartup completed");
            }
            catch (Exception ex)
            {
                Log($"FATAL in OnStartup: {ex}");
                System.Windows.MessageBox.Show($"程序启动失败：\n\n{ex.Message}\n\n详细信息请查看：{_logFile}", "启动错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try { Log($"App.OnExit (exit code: {e.ApplicationExitCode})"); } catch { }
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log($"[DispatcherUnhandledException] {e.Exception}");
            if (!e.Handled)
            {
                System.Windows.MessageBox.Show($"程序发生未处理异常：\n\n{e.Exception.Message}\n\n{e.Exception.InnerException?.Message}\n\n详细信息请查看：{_logFile}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log($"[AppDomain.UnhandledException] {e.ExceptionObject}");
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log($"[TaskScheduler.UnobservedTaskException] {e.Exception}");
            e.SetObserved();
        }

        private void Log(string message)
        {
            try
            {
                File.AppendAllText(_logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
            }
            catch { }
        }
    }
}
