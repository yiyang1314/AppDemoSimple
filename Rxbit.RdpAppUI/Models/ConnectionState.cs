using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.RdpAppUI.Models
{
    public class ConnectionState : INotifyPropertyChanged
    {
        private bool _isConnected;
        private bool _isReconnecting;
        private int _reconnectAttempts;
        private DateTime _lastConnected;
        private string _statusMessage;

        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(nameof(IsConnected)); }
        }

        public bool IsReconnecting
        {
            get => _isReconnecting;
            set { _isReconnecting = value; OnPropertyChanged(nameof(IsReconnecting)); }
        }

        public int ReconnectAttempts
        {
            get => _reconnectAttempts;
            set { _reconnectAttempts = value; OnPropertyChanged(nameof(ReconnectAttempts)); }
        }

        public DateTime LastConnected
        {
            get => _lastConnected;
            set { _lastConnected = value; OnPropertyChanged(nameof(LastConnected)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
