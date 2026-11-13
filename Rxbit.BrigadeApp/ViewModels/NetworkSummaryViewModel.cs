using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class NetworkSummaryViewModel : INotifyPropertyChanged
    {

        private bool _isGen1Type;
        public bool IsGen1Type
        {
            get => _isGen1Type;
            set
            {
                _isGen1Type = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<NetworkSummaryRow> _Gen1Items;
        private ObservableCollection<NetworkSummaryRow> _Gen2Items;
        private string _selectedCanId;

        // 图3: Network Summary 数据 - CAN ID X,Y 格式
        public ObservableCollection<NetworkSummaryRow> Gen1Items
        {
            get => _Gen1Items;
            set
            {
                _Gen1Items = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<NetworkSummaryRow> Gen2Items
        {
            get => _Gen2Items;
            set
            {
                _Gen2Items = value;
                OnPropertyChanged();
            }
        }
        public int SelectedObjectIndex
        {
            get => _selectedObjectIndex;
            set
            {
                _selectedObjectIndex = value;
                OnPropertyChanged();
                UpdateRowSelections();
            }
        }
        private int _selectedObjectIndex = 0;

        public string SelectedCanId
        {
            get => _selectedCanId;
            set
            {
                _selectedCanId = value;
                OnPropertyChanged();
            }
        }
        public AsyncRelayCommand LoadedCommand { get; set; }
        public RelayCommand ClosedCommand { get; set; }
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

        private GenType _GenType = GenType.GenNoInit;

        public GenType Type
        {
            get { return _GenType; }
            set
            {
                _GenType = value;
                if (_GenType == GenType.Gen1Type)
                {
                    GenTitle = "3XX";
                }
                else
                {
                    GenTitle = "2XX";
                }
                OnPropertyChanged();
            }
        }

        private string _GenTitle;

        public string GenTitle
        {
            get { return _GenTitle; }
            set { _GenTitle = value; OnPropertyChanged(nameof(GenTitle)); }
        }

        public int Gen1Id
        {
            get { return _gen1Id; }
            set { _gen1Id = value; OnPropertyChanged(nameof(Gen1Id)); }
        }
        private int _gen1Id = 0;
        public int Gen2Id
        {
            get { return _gen2Id; }
            set { _gen2Id = value; OnPropertyChanged(nameof(Gen2Id)); }
        }
        private int _gen2Id = 0;

        public ObservableCollection<DisplayObj> FirstRowItems
        {
            get
            {
                var list = IsGen1Type ? Gen1Items : Gen2Items;
                if (list == null || list.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                var firstRow = list[0];
                if (firstRow.Items == null || firstRow.Items.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                return firstRow.Items;
            }
        }

        private readonly DataMediatorService<RadarTargetDto> service;
        private readonly SensorConfigurationViewModel SensorVM;
        public NetworkSummaryViewModel()
        {
            service = ViewModelLocator.GetService<DataMediatorService<RadarTargetDto>>();
            SensorVM = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            SensorVM.SensorChangeEvevt -= SensorVM_PropertyChanged;
            SensorVM.SensorChangeEvevt += SensorVM_PropertyChanged;
            var rvm = ViewModelLocator.GetService<RadarDisplayViewModel>();
            rvm.NotifyChangeId -= NotifyChangeId_DataComed;
            rvm.NotifyChangeId += NotifyChangeId_DataComed;
            service.DataComed += RadarObj_DataComed;
            LoadedCommand = new AsyncRelayCommand(LoadData);
            ClosedCommand = new RelayCommand(() => { this.IsPaused = true; }, () => this.IsPaused);
            Gen1Items = new ObservableCollection<NetworkSummaryRow>(InitializeMockData(0x310));
            Gen2Items = new ObservableCollection<NetworkSummaryRow>(InitializeMockData(0x210));
            _flushTimer = new Timer(50);
            _flushTimer.Elapsed += FlushBufferToUi;
            _flushTimer?.Start();
        }

        private void NotifyChangeId_DataComed(SensorInfo info, byte Id)
        {
            if (info.Type == GenType.Gen1Type)
            {
                this.Gen1Id = Id;
            }
            else if (info.Type == GenType.Gen2Type)
            {
                this.Gen2Id = Id;
            }
            else
            {
                this.Gen2Id = 0;
            }
        }

        private async Task LoadData()
        {
            this.IsPaused = false;
            this.IsGen1Type = SensorVM.CurrSensor.Type == GenType.Gen1Type;
            UpdateDefaultSelection(SensorVM.CurrSensor.CanID);
            await Task.Delay(1000);
        }

        private void SensorVM_PropertyChanged(SensorInfo info)
        {
            if (info == null) return;
            this.IsGen1Type = info.Type == GenType.Gen1Type;
            this.Type = info.Type;
            UpdateDefaultSelection(info.CanID);
        }

        private void UpdateDefaultSelection(uint canId)
        {
            var list = IsGen1Type ? Gen1Items : Gen2Items;
            foreach (var row in list)
            {
                row.IsSelected = row.SensorId == canId;
            }
        }

        ~NetworkSummaryViewModel()
        {
            _flushTimer?.Stop();
        }

        private void FlushBufferToUi(object sender, ElapsedEventArgs e)
        {
            if (_msgBuffer.IsEmpty) return;
            Application.Current.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                int processedCount = 0;
                while (_msgBuffer.TryDequeue(out RadarTargetDto radar))
                {
                    if (this.IsPaused) continue;
                    int index = (int)((radar.CanID >> 4) & 0x0f);
                    var list = Gen2Items;
                    var rId = this.Gen2Id;
                    if (radar.GenType == GenType.Gen1Type)
                    {
                        list = Gen1Items;
                        rId = this.Gen1Id;
                    }
                    if (list == null || list.Count < 2 * index)
                    {
                        processedCount++;
                        continue;
                    }
                    var others = list.Where(u => (u.Index != (2 * index - 2)) && u.Index != (2 * index - 1));
                    foreach (var item in others)
                    {
                        var uuu = item.Items.Where(u => u.Id == rId).FirstOrDefault();
                        uuu.IsSelected = true;
                    }
                    var xlist = list.ElementAt(2 * index - 2).Items;
                    var ylist = list.ElementAt(2 * index - 1).Items;
                    for (int i = 1; i <= 16; i++)
                    {
                        var xobj = xlist.ElementAt(i - 1);
                        var yobj = ylist.ElementAt(i - 1);
                        if (radar.RadarTargets.ContainsKey(xobj.Id))
                        {
                            xobj.Value = radar.RadarTargets[xobj.Id].X.ToString("F2");
                            yobj.Value = radar.RadarTargets[xobj.Id].Y.ToString("F2");
                        }
                        else
                        {
                            xobj.Value = "—";
                            yobj.Value = "—";
                        }
                        xobj.IsSelected = rId == i;
                        yobj.IsSelected = rId == i;
                    }
                    processedCount++;
                }
            }));
        }

        private readonly ConcurrentQueue<RadarTargetDto> _msgBuffer = new ConcurrentQueue<RadarTargetDto>();
        private const int MaxProcessCount = 50;

        // 50ms批量刷新定时器
        private readonly Timer _flushTimer;
        private const int MaxStoreCount = 800;
        private void RadarObj_DataComed(RadarTargetDto dto)
        {
            this.Type = dto.GenType;
            if (!this._isPaused)
            {
                _msgBuffer.Enqueue(dto);
            }
        }


        private List<NetworkSummaryRow> InitializeMockData(uint canID)
        {
            var list = new List<NetworkSummaryRow>();
            uint cId = canID;
            int pos = 0;
            for (uint i = 0; i < 8; i++)
            {
                var Id = cId + i * 16;
                var x = new NetworkSummaryRow {Index = pos++, SensorId = Id, CanId = $"0x{Id:x} X", Unit = "m", Items = new ObservableCollection<DisplayObj>(), IsConnected = true };
                var y = new NetworkSummaryRow {Index = pos++, SensorId = Id, CanId = $"0x{Id:x} Y", Unit = "m", Items = new ObservableCollection<DisplayObj>(), IsConnected = true };
                for (uint j = 1; j <= 16; j++)
                {
                    x.Items.Add(new DisplayObj { Id = j, Value = "—" });
                    y.Items.Add(new DisplayObj { Id = j, Value = "—"});
                }
                list.Add(x);
                list.Add(y);
            }
            return list;
        }

        private void UpdateRowSelections()
        {
            var list = IsGen1Type ? Gen1Items : Gen2Items;
            if (list == null) return;

            foreach (var row in list)
            {
                if (row?.Items == null) continue;
                for (int i = 0; i < row.Items.Count; i++)
                {
                    if (row.Items[i] != null)
                    {
                        row.Items[i].IsSelected = (i + 1) == SelectedObjectIndex;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // 图3数据模型 - Network Summary 行
    public class NetworkSummaryRow : ViewModelBase
    {
        private bool _isConnected;
        private bool _isSelected;
        public int Index { get; set; }
        public uint SensorId { get; set; }
        public string CanId { get; set; }
        public string Unit { get; set; }
        public ObservableCollection<DisplayObj> Items { get; set; } = new ObservableCollection<DisplayObj>();

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public class DisplayObj : ViewModelBase
    {
        public string Value
        {
            get { return _Value; }
            set { SetProperty(ref _Value, value); }
        }
        private string _Value = "-";

        public bool IsSelected
        {
            get { return _Selected; }
            set { SetProperty(ref _Selected, value); }
        }
        private bool _Selected = false;

        public uint Id { get; set; }
    }
}