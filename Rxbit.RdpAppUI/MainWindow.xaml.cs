using Rxbit.RdpAppUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rxbit.RdpAppUI
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewModel;
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Password = pwdPassword.Password;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.ConnectCommand.Execute(null);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.DisconnectCommand.Execute(null);
        }

        private void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.UploadFileCommand.Execute(null);
        }

        private void DownloadFile_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.DownloadFileCommand.Execute(null);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel.IsConnected)
            {
                _viewModel.SendMouseCommand.Execute(e);
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsConnected)
            {
                // 发送鼠标按下事件
                var point = e.GetPosition(imgRemoteScreen);
                _viewModel.SendMouseCommand.Execute(new { X = point.X, Y = point.Y, Action = "Down" });
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsConnected)
            {
                var point = e.GetPosition(imgRemoteScreen);
                _viewModel.SendMouseCommand.Execute(new { X = point.X, Y = point.Y, Action = "Up" });
            }
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewModel.IsConnected)
            {
                _viewModel.SendMouseCommand.Execute(new { Delta = e.Delta });
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Connect_Click(sender: this, e: new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                Disconnect_Click(sender: this, e: new RoutedEventArgs());
                e.Handled = true;
                return;
            }

            if (_viewModel != null && _viewModel.IsConnected)
            {
                _viewModel.SendKeyboardCommand.Execute((int)e.Key);
            }
            base.OnKeyDown(e);
        }
    }
}
