using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.ViewModels;
using Rxbit.BrigadeApp.Views;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Rxbit.BrigadeApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            base.OnStartup(e);
            // 先创建并显示主窗口，避免主窗口被启动时的模态设备窗口阻塞
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            // 启动时弹出 CAN 设备配置窗口（模态，关闭该窗口不会退出程序）
            CanDeviceView deviceView = new CanDeviceView
            {
                Owner = mainWindow
            };
            deviceView.ShowDialog();
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            string errorMsg = $"Task 异常: {e.Exception.Message}\n\n堆栈跟踪:\n{e.Exception.StackTrace}";
            LogHelper.Error(errorMsg,e.Exception);
            e.SetObserved();
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMsg = $"Dispatcher异常: {e.Exception.Message}\n\n堆栈跟踪:\n{e.Exception.StackTrace}";
            LogHelper.Error(errorMsg,e.Exception);
            //MessageBox.Show(errorMsg, "程序异常", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            string errorMsg = $"未处理异常: {ex?.Message ?? "未知错误"}\n\n堆栈跟踪:\n{ex?.StackTrace}";
            LogHelper.Error(errorMsg,ex);
            //MessageBox.Show(errorMsg, "程序异常.3后自动退出", MessageBoxButton.OK, MessageBoxImage.Error);
            Task.Delay(3000).Wait();
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            // 系统即将关机，保存当前任务配置
            SaveCurrentConfig();
            base.OnSessionEnding(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SaveCurrentConfig();
            try
            {
                ViewModelLocator.GetService<CanDeviceViewModel>().Dispose();
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
            ViewModelLocator.Cleanup();
            base.OnExit(e);
        }

        private static void SaveCurrentConfig()
        {
            try
            {
                var bus = ViewModelLocator.GetService<BrigadeConfigBus>();
                // 保存全局配置
                ConfigHelper.SaveConfig(bus, RadarConstaint.SettingFile);
            }
            catch (Exception ex)
            {
                // 配置文件目录不可写（权限/路径异常）时不应影响程序退出
                LogHelper.Error($"保存配置失败: {ex.Message}", ex);
            }
        }
    }
}
