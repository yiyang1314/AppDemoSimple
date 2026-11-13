using System;
using System.Windows;

namespace Rxbit.Brigade.Controll
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show($"未处理的异常: {args.Exception.Message}\n\n堆栈: {args.Exception.StackTrace}",
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                MessageBox.Show($"致命错误: {ex?.Message}\n\n堆栈: {ex?.StackTrace}",
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            };
        }
    }
}