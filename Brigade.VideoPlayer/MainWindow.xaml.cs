using System;
using System.IO;
using System.Windows;

namespace Brigade.VideoPlayer
{
    public partial class MainWindow : Window
    {
        private string _logFile;

        public MainWindow()
        {
            _logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
            Log("MainWindow.ctor start");

            try
            {
                InitializeComponent();
                Log("MainWindow.InitializeComponent completed");

                Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                Log($"MainWindow.ctor FAILED: {ex}");
                System.Windows.MessageBox.Show($"主窗口初始化失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Log($"MainWindow.Loaded - WindowState={WindowState}, Visibility={Visibility}, Left={Left}, Top={Top}, Width={Width}, Height={Height}");
            Log($"MainWindow is now visible: {IsVisible}");

            if (RbPlayer.IsChecked == true)
            {
                PlayerView.Visibility = Visibility.Visible;
                MakerView.Visibility = Visibility.Collapsed;
            }
            else
            {
                PlayerView.Visibility = Visibility.Collapsed;
                MakerView.Visibility = Visibility.Visible;
            }
        }

        private void RbPlayer_Checked(object sender, RoutedEventArgs e)
        {
            if (PlayerView != null && MakerView != null)
            {
                PlayerView.Visibility = Visibility.Visible;
                MakerView.Visibility = Visibility.Collapsed;
            }
        }

        private void RbMaker_Checked(object sender, RoutedEventArgs e)
        {
            if (PlayerView != null && MakerView != null)
            {
                PlayerView.Visibility = Visibility.Collapsed;
                MakerView.Visibility = Visibility.Visible;
            }
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
