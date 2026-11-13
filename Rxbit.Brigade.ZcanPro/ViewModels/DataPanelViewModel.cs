using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rxbit.Brigade.ZcanPro.Mvvm;

namespace Rxbit.Brigade.ZcanPro.ViewModels
{
    public class DataPanelViewModel : ObservableObject
    {
        private int _viewId;
        private string _title;
        private ObservableCollection<ChannelDataItem> _channels;
        private ObservableCollection<MessageItem> _messages;
        private string _statusText = "就绪";
        private bool _isConnected;
        private int _totalMessages;
        private string _filterText;

        public int ViewId { get => _viewId; set => SetProperty(ref _viewId, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ObservableCollection<ChannelDataItem> Channels { get => _channels; set => SetProperty(ref _channels, value); }
        public ObservableCollection<MessageItem> Messages { get => _messages; set => SetProperty(ref _messages, value); }
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }
        public int TotalMessages { get => _totalMessages; set => SetProperty(ref _totalMessages, value); }
        public string FilterText { get => _filterText; set => SetProperty(ref _filterText, value); }

        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand ExportCommand { get; }

        public DataPanelViewModel(int id)
        {
            _viewId = id;
            _title = $"数据监控面板 {id}";
            _channels = new ObservableCollection<ChannelDataItem>();
            _messages = new ObservableCollection<MessageItem>();

            // 初始化通道
            _channels.Add(new ChannelDataItem { Name = "CAN 1", BaudRate = "500 Kbps", Status = "正常", Messages = 15234, Errors = 0, Load = "45%" });
            _channels.Add(new ChannelDataItem { Name = "CAN 2", BaudRate = "250 Kbps", Status = "正常", Messages = 8921, Errors = 2, Load = "28%" });
            _channels.Add(new ChannelDataItem { Name = "LIN 1", BaudRate = "19.2 Kbps", Status = "正常", Messages = 3456, Errors = 0, Load = "12%" });
            _channels.Add(new ChannelDataItem { Name = "FlexRay", BaudRate = "10 Mbps", Status = "断开", Messages = 0, Errors = 0, Load = "0%" });

            // 初始化消息
            for (int i = 0; i < 20; i++)
            {
                _messages.Add(new MessageItem
                {
                    Timestamp = DateTime.Now.AddSeconds(-i * 0.1).ToString("HH:mm:ss.fff"),
                    Channel = i % 2 == 0 ? "CAN 1" : "CAN 2",
                    Id = $"0x{(0x100 + i).ToString("X3")}",
                    DLC = (i % 8 + 1).ToString(),
                    Data = GenerateRandomData(i),
                    Type = i % 5 == 0 ? "远程" : "数据"
                });
            }

            TotalMessages = 27611;

            ConnectCommand = new RelayCommand(() => { IsConnected = true; StatusText = "已连接"; });
            DisconnectCommand = new RelayCommand(() => { IsConnected = false; StatusText = "已断开"; });
            ClearCommand = new RelayCommand(() => { Messages.Clear(); StatusText = "已清空"; TotalMessages = 0; });
            ExportCommand = new RelayCommand(() => { StatusText = "数据已导出"; });
        }

        private string GenerateRandomData(int seed)
        {
            var rng = new Random(seed);
            var bytes = new byte[8];
            rng.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", " ");
        }
    }

    public class ChannelDataItem : ObservableObject
    {
        private string _name;
        private string _baudRate;
        private string _status;
        private int _messages;
        private int _errors;
        private string _load;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public int Messages { get => _messages; set => SetProperty(ref _messages, value); }
        public int Errors { get => _errors; set => SetProperty(ref _errors, value); }
        public string Load { get => _load; set => SetProperty(ref _load, value); }
    }

    public class MessageItem : ObservableObject
    {
        private string _timestamp;
        private string _channel;
        private string _id;
        private string _dlc;
        private string _data;
        private string _type;

        public string Timestamp { get => _timestamp; set => SetProperty(ref _timestamp, value); }
        public string Channel { get => _channel; set => SetProperty(ref _channel, value); }
        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string DLC { get => _dlc; set => SetProperty(ref _dlc, value); }
        public string Data { get => _data; set => SetProperty(ref _data, value); }
        public string Type { get => _type; set => SetProperty(ref _type, value); }
    }
}