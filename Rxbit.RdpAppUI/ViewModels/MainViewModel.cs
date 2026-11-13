using Rxbit.RdpAppUI.Models;
using Rxbit.RdpAppUI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Wpf.CustomControl.Mvvm;

namespace Rxbit.RdpAppUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RemoteDesktopService _remoteDesktopService;
        private readonly FileTransferService _fileTransferService;
        private NetworkReconnectionService _reconnectionService;
        private BitmapImage _remoteScreen;
        private bool _isConnected;
        private string _host;
        private int _port = 3389;
        private string _username;
        private string _password;
        private string _statusMessage;
        private bool _isReconnecting;
        private int _reconnectAttempts;

        public MainViewModel()
        {
            _remoteDesktopService = new RemoteDesktopService();
            _fileTransferService = new FileTransferService();

            ConnectCommand = new AsyncCommand(ConnectAsync);
            DisconnectCommand = new RelayCommand(Disconnect);
            UploadFileCommand = new AsyncCommand( UploadFileAsync);
            DownloadFileCommand = new AsyncCommand(DownloadFileAsync);
            SendMouseCommand = new AsyncCommand<object>(SendMouseAsync);
            SendKeyboardCommand = new AsyncCommand<object>(SendKeyboardAsync);

            _remoteDesktopService.ScreenUpdated += OnScreenUpdated;
            _remoteDesktopService.ErrorOccurred += OnError;
            _remoteDesktopService.ConnectionLost += OnConnectionLost;

            // 添加文件传输事件处理
            _fileTransferService.TransferProgressChanged += OnTransferProgressChanged;
            _fileTransferService.TransferCompleted += OnTransferCompleted;
            _fileTransferService.TransferFailed += OnTransferFailed;
        }

        public BitmapImage RemoteScreen
        {
            get => _remoteScreen;
            set { _remoteScreen = value; OnPropertyChanged(); }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        public string Host
        {
            get => _host;
            set { _host = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsReconnecting
        {
            get => _isReconnecting;
            set { _isReconnecting = value; OnPropertyChanged(); }
        }

        public int ReconnectAttempts
        {
            get => _reconnectAttempts;
            set { _reconnectAttempts = value; OnPropertyChanged(); }
        }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand DownloadFileCommand { get; }
        public AsyncCommand<object> SendMouseCommand { get; }
        public AsyncCommand<object> SendKeyboardCommand { get; }

        private async Task ConnectAsync()
        {
            try
            {
                StatusMessage = "正在连接...";
                bool connected = await _remoteDesktopService.ConnectAsync(Host, Port, Username, Password);

                if (connected)
                {
                    IsConnected = true;
                    StatusMessage = "连接成功";

                    // 初始化重连服务
                    _reconnectionService = new NetworkReconnectionService( _remoteDesktopService, Host, Port, Username, Password);

                    _reconnectionService.Reconnecting += OnReconnecting;
                    _reconnectionService.Reconnected += OnReconnected;
                    _reconnectionService.ReconnectFailed += OnReconnectFailed;
                    _reconnectionService.MaxReconnectAttemptsReached += OnMaxReconnectAttemptsReached;
                }
                else
                {
                    StatusMessage = "连接失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"连接错误: {ex.Message}";
            }
        }

        private void Disconnect()
        {
            _remoteDesktopService.Disconnect();
            _reconnectionService?.Dispose();
            IsConnected = false;
            RemoteScreen = null;
            StatusMessage = "已断开连接";
        }

        private async Task UploadFileAsync()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                StatusMessage = $"正在上传文件: {dialog.FileName}";
                await _fileTransferService.UploadFileAsync(dialog.FileName, $"/remote/{System.IO.Path.GetFileName(dialog.FileName)}");
            }
        }

        private async Task DownloadFileAsync()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                StatusMessage = $"正在下载文件到: {dialog.FileName}";
                await _fileTransferService.DownloadFileAsync("/remote/file.txt", dialog.FileName);
            }
        }

        private async Task SendMouseAsync(object parameter)
        {
            if (parameter is MouseEventArgs args)
            {
                var point = args.GetPosition(null);
                await _remoteDesktopService.SendMouseEventAsync(
                    (int)point.X, (int)point.Y, MouseEventType.Move);
            }
        }

        private async Task SendKeyboardAsync(object parameter)
        {
            if (parameter is int keyCode)
            {
                await _remoteDesktopService.SendKeyboardEventAsync(keyCode, true);
            }
        }

        private void OnScreenUpdated(BitmapImage screen)
        {
            RemoteScreen = screen;
        }

        private void OnError(string error)
        {
            StatusMessage = $"错误: {error}";
        }

        private void OnConnectionLost()
        {
            IsConnected = false;
            StatusMessage = "连接已断开，正在尝试重连...";
        }

        private void OnReconnecting()
        {
            IsReconnecting = true;
            StatusMessage = "正在重连...";
        }

        private void OnReconnected()
        {
            IsReconnecting = false;
            IsConnected = true;
            StatusMessage = "重连成功";
        }

        private void OnReconnectFailed(string message)
        {
            ReconnectAttempts = _reconnectionService.CurrentAttempt;
            StatusMessage = message;
        }

        private void OnMaxReconnectAttemptsReached()
        {
            IsReconnecting = false;
            StatusMessage = "重连失败，已达到最大重连次数";
        }

        private void OnTransferProgressChanged(FileTransferInfo info)
        {
            StatusMessage = $"传输中: {info.FileName} - {info.Progress:F1}%";
        }

        private void OnTransferCompleted(string message)
        {
            StatusMessage = message;
        }

        private void OnTransferFailed(string fileName, string error)
        {
            StatusMessage = $"传输失败: {fileName} - {error}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
