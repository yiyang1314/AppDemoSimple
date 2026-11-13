using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Rxbit.RdpAppUI.Services
{
    public class NetworkReconnectionService : IDisposable
    {
        private readonly RemoteDesktopService _remoteDesktopService;
        private readonly string _host;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;
        private readonly int _maxReconnectAttempts = 5;
        private readonly int _reconnectInterval = 5000; // 5秒
        private int _currentAttempt;
        private System.Timers.Timer _reconnectTimer;
        private bool _isReconnecting;
        private CancellationTokenSource _cancellationTokenSource;

        public event Action Reconnecting;
        public event Action Reconnected;
        public event Action<string> ReconnectFailed;
        public event Action MaxReconnectAttemptsReached;

        public bool IsReconnecting => _isReconnecting;
        public int CurrentAttempt => _currentAttempt;
        public int MaxAttempts => _maxReconnectAttempts;

        public NetworkReconnectionService(
            RemoteDesktopService remoteDesktopService,
            string host,
            int port,
            string username,
            string password)
        {
            _remoteDesktopService = remoteDesktopService;
            _host = host;
            _port = port;
            _username = username;
            _password = password;
            _cancellationTokenSource = new CancellationTokenSource();

            // 订阅连接断开事件
            _remoteDesktopService.ConnectionLost += OnConnectionLost;
        }

        private void OnConnectionLost()
        {
            if (!_isReconnecting)
            {
                StartReconnection();
            }
        }

        public void StartReconnection()
        {
            if (_isReconnecting) return;

            _isReconnecting = true;
            _currentAttempt = 0;
            _cancellationTokenSource = new CancellationTokenSource();

            Reconnecting?.Invoke();

            _reconnectTimer = new System.Timers.Timer(_reconnectInterval);
            _reconnectTimer.Elapsed += OnReconnectTimerElapsed;
            _reconnectTimer.Start();

            // 立即尝试一次重连
            _ = TryReconnectAsync();
        }

        private async void OnReconnectTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await TryReconnectAsync();
        }

        private async Task TryReconnectAsync()
        {
            if (_currentAttempt >= _maxReconnectAttempts)
            {
                StopReconnection();
                MaxReconnectAttemptsReached?.Invoke();
                return;
            }

            _currentAttempt++;

            try
            {
                bool connected = await _remoteDesktopService.ConnectAsync(
                    _host, _port, _username, _password);

                if (connected)
                {
                    StopReconnection();
                    Reconnected?.Invoke();
                }
                else
                {
                    ReconnectFailed?.Invoke($"重连尝试 {_currentAttempt}/{_maxReconnectAttempts} 失败");
                }
            }
            catch (Exception ex)
            {
                ReconnectFailed?.Invoke($"重连失败: {ex.Message}");
            }
        }

        public void StopReconnection()
        {
            _isReconnecting = false;
            _reconnectTimer?.Stop();
            _reconnectTimer?.Dispose();
            _reconnectTimer = null;
            _cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            StopReconnection();
            _cancellationTokenSource?.Dispose();
            _remoteDesktopService.ConnectionLost -= OnConnectionLost;
        }
    }
}
