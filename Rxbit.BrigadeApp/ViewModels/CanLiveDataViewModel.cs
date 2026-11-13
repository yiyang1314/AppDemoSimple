using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.IServices;
using Rxbit.Brigade.CanDevices.Structs;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.BrigadeApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class CanLiveDataViewModel : INotifyPropertyChanged
    {
        // ����������̨CAN�߳�ֻ��ӣ�����UI����
        private readonly ConcurrentQueue<RXCanMessage> _msgBuffer = new ConcurrentQueue<RXCanMessage>();
        private readonly object _canMsgLock = new object();
        // 50ms����ˢ�¶�ʱ��
        private readonly Timer _flushTimer;
        // ��������̶�300
        private const int MaxStoreCount = 800;
        // ÿ�δ�����消息数量限制
        private const int MaxProcessCount = 50;

        public ObservableCollection<CanMessage> CanMessages { get; set; }
        private readonly BrigadeConfigBus Bus;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }
        private bool _isPaused = true;
        private int _messageCount;
        public int MessageCount
        {
            get => _messageCount;
            set
            {
                _messageCount = value;
                OnPropertyChanged();
            }
        }
        private string _Tipmsg = "CAN Live Data - Real-time Log";
        public string Tipmsg
        {
            get => _Tipmsg;
            set
            {
                _Tipmsg = value;
                OnPropertyChanged();
            }
        }

        private DateTime StartTime;
        private UInt64 BeginTime;
        public ICommand ClearCommand { get; }
        public ICommand ExportCommand { get; }

        public RelayCommand LoadedCommand { get; set; }
        public RelayCommand ClosedCommand { get; set; }
        public CanLiveDataViewModel()
        {
            CanMessages = new ObservableCollection<CanMessage>();
            IsPaused = false;
            MessageCount = 0;
            LoadedCommand = new RelayCommand(() => { _flushTimer.Start(); this.IsPaused = false; });
            ClosedCommand = new RelayCommand(() => { _flushTimer?.Stop(); this.IsPaused = true; });
            ClearCommand = new RelayCommand(Clear);
            ExportCommand = new AsyncRelayCommand(ExportCsv);
            var canLiveService = ViewModelLocator.GetService<CanLiveService>();
            canLiveService.CanDataComed += CanLiveService_CanDataComed;
            Bus = ViewModelLocator.GetService<BrigadeConfigBus>();
            _flushTimer = new Timer(20);
            _flushTimer.Elapsed += FlushBufferToUi;
        }


        /// ��ʱ����ˢ��UI���ϣ�50msһ�Σ��������UIˢ�´���
        private void FlushBufferToUi(object sender, ElapsedEventArgs e)
        {
            if (_msgBuffer.IsEmpty || StartTime == null) return;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                lock (_canMsgLock)
                {
                    int processedCount = 0;
                    while (_msgBuffer.TryDequeue(out RXCanMessage rawItem) && processedCount < MaxProcessCount)
                    {
                        if (CheckMsgType(rawItem, out string msg))
                        {
                            string hexData = BytesToHex(rawItem.DATA);
                            CanMessages.Add(new CanMessage()
                            {
                                CanId = rawItem.ID,
                                Message = msg,
                                Dlc = rawItem.DLC,
                                Data = hexData,
                                Time = StartTime.AddMilliseconds((rawItem.TIMESTAMP - BeginTime) / 1000)
                            });
                            if (CanMessages.Count > MaxStoreCount)
                                CanMessages.RemoveAt(0);
                            processedCount++;
                        }
                    }
                    MessageCount = CanMessages.Count;
                }
            }));
        }

        /// ������ByteתHex����LINQ����Ƶ���ַ�����ֵGC
        private static string BytesToHex(byte[] data)
        {
            if (data == null || data.Length == 0)
                return "";
            return BitConverter.ToString(data).Replace("-", " 0x").Insert(0, "0x");
        }
        private void Clear()
        {
            lock (_canMsgLock)
            {
                CanMessages.Clear();
                // ��ջ�������������
                while (_msgBuffer.TryDequeue(out _)) { }
                MessageCount = 0;
            }
        }

        private async Task ExportCsv()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "CSV|*.csv" };
            dialog.InitialDirectory = RadarConstaint.RecondingPath;
            if (dialog.ShowDialog() != true) return;
            var list = CanMessages.ToList();
            string path = dialog.FileName;
            await Task.Run(() =>
            {
                using var sw = new StreamWriter(path, false, Encoding.UTF8);
                sw.WriteLine("Time.CANID,DLC,Message,Data");
                foreach (var item in CanMessages)
                {
                    sw.WriteLine($"{item.Time},{item.CanId:X},{item.Dlc},{item.Message},{item.Data}");
                }
                sw.Close();
            });
            Application.Current.Dispatcher.Invoke(() => MessageBox.Show("�������"));
        }
        private void CanLiveService_CanDataComed(CanMessageWrapper obj)
        {
            if (BeginTime == 0 && obj.Messages.Count > 0)
            {
                StartTime = DateTime.Now;
                BeginTime = obj.Messages.FirstOrDefault().TIMESTAMP;
            }
            if (this._isPaused) return;
            foreach (var item in obj.Messages)
            {
                _msgBuffer.Enqueue(item);
            }
        }

        private bool CheckMsgType(RXCanMessage item, out string msg)
        {
            msg = string.Empty;
            if (item.ID == 0x390)
            {
                msg = "Base Configuration";
                return true;
            }
            else if ((item.ID >= 0x150 && item.ID <= 0x158) || (item.ID >= 0x200 && item.ID <= 0x207))
            {
                msg = BrigadeHeler.GetBaseId(item.DATA[0]);
                return true;
            }
            else if (item.ID >= 0x210 && item.ID <= 0x282)
            {
                var CT = (item.ID & 0x2F3) & 0x003;
                if (CT == 0)
                {
                    msg = BrigadeHeler.GetGen2N0(item.ID, (ushort)((item.DATA[1] << 8) + item.DATA[0]));
                }
                else if (CT == 1)
                {
                    msg = BrigadeHeler.GetGen2N1((byte)(item.DATA[0] & 0x1f));
                }
                else if (CT == 2)
                {
                    msg = BrigadeHeler.GetGen2N2(item.ID, item.DATA[7]);
                }
                return true;
            }
            else if ((item.ID >= 0x310 && item.ID <= 0x38F))
            {
                msg = BrigadeHeler.GetGen1(item.DATA[6]);
                return true;
            }
            else
            {

            }
            return false;
        }

        public ChannelConfig Config
        {
            get => _config;
            set
            {
                _config = value;
                OnPropertyChanged();
            }
        }
        private ChannelConfig _config;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
