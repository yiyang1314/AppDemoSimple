using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Rxbit.RdpAppUI.Services
{
    public class RemoteDesktopService : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isConnected;

        public event Action<BitmapImage> ScreenUpdated;
        public event Action<string> ErrorOccurred;
        public event Action ConnectionLost;
        public event Action ConnectionRestored;

        public async Task<bool> ConnectAsync(string host, int port, string username, string password)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _networkStream = _tcpClient.GetStream();
                _isConnected = true;
                _cancellationTokenSource = new CancellationTokenSource();

                // 启动屏幕捕获任务
                _ = Task.Run(() => ReceiveScreenAsync(_cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"连接失败: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveScreenAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _isConnected)
                {
                    // 接收屏幕数据
                    byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                    int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead > 0)
                    {
                        using (var ms = new MemoryStream(buffer, 0, bytesRead))
                        {
                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = ms;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();

                            ScreenUpdated?.Invoke(bitmapImage);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"接收屏幕数据失败: {ex.Message}");
                ConnectionLost?.Invoke();
            }
        }

        public async Task SendMouseEventAsync(int x, int y, MouseEventType eventType)
        {
            if (!_isConnected) return;

            try
            {
                byte[] data = new byte[9];
                data[0] = (byte)CommandType.MouseEvent;
                data[1] = (byte)eventType;
                BitConverter.GetBytes(x).CopyTo(data, 2);
                BitConverter.GetBytes(y).CopyTo(data, 6);

                await _networkStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"发送鼠标事件失败: {ex.Message}");
            }
        }

        public async Task SendKeyboardEventAsync(int keyCode, bool isKeyDown)
        {
            if (!_isConnected) return;

            try
            {
                byte[] data = new byte[6];
                data[0] = (byte)CommandType.KeyboardEvent;
                data[1] = (byte)(isKeyDown ? 1 : 0);
                BitConverter.GetBytes(keyCode).CopyTo(data, 2);

                await _networkStream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"发送键盘事件失败: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            _cancellationTokenSource?.Cancel();
            _isConnected = false;
            _networkStream?.Close();
            _tcpClient?.Close();
        }

        public void Dispose()
        {
            Disconnect();
            _cancellationTokenSource?.Dispose();
        }
    }

    public enum MouseEventType
    {
        Move,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp,
        Scroll
    }

    public enum CommandType
    {
        ScreenData,
        MouseEvent,
        KeyboardEvent,
        FileTransfer
    }
}
