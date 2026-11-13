using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using Rxbit.BrigadeApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class DetectionSensorInfoViewModel : ViewModelBase
    {
        public string SensorId
        {
            get => _sensorId;
            set
            {
                _sensorId = value;
                OnPropertyChanged(nameof(SensorId));
            }
        }
        private string _sensorId;

        public Gen1RadarDto Gen1Radar
        {
            get { return _Gen1Radar; }
            set { SetProperty(ref _Gen1Radar, value); }
        }
        private Gen1RadarDto _Gen1Radar;

        public Gen2RadarDto Gen2Radar
        {
            get { return _Gen2Radar; }
            set { SetProperty(ref _Gen2Radar, value); }
        }
        private Gen2RadarDto _Gen2Radar;

        private int _selectedColumnIndex;
        private UInt32 _selectedFrameId;
        private readonly DataMediatorService<RadarTargetDto> service;

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }
        private bool _isPaused;

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

        public int SelectedColumnIndex
        {
            get => _selectedColumnIndex;
            set
            {
                _selectedColumnIndex = value;
                OnPropertyChanged();
                UpdateSelection();
            }
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
        public AsyncRelayCommand LoadedCommand { get; set; }
        public RelayCommand ClosedCommand { get; set; }
        private readonly SensorConfigurationViewModel SensorVM;
        private readonly BrigadeConfigBus configBus;
        public DetectionSensorInfoViewModel()
        {
            configBus = ViewModelLocator.GetService<BrigadeConfigBus>();
            SensorId = $"0x{configBus.Sensor.CanID:x}";
            SensorVM = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            IsGen1Type = SensorVM.CurrSensor?.Type == GenType.Gen1Type;
            LoadedCommand = new AsyncRelayCommand(LoadData);
            ClosedCommand = new RelayCommand(() => { this.IsPaused = true; }, () => this.IsPaused);
            InitGenDataLists();
            SensorVM.SensorChangeEvevt -= SensorVM_PropertyChanged;
            SensorVM.SensorChangeEvevt += SensorVM_PropertyChanged;
            var rvm = ViewModelLocator.GetService<RadarDisplayViewModel>();
            rvm.NotifyChangeId -= NotifyChangeId_DataComed;
            rvm.NotifyChangeId += NotifyChangeId_DataComed;
            service = ViewModelLocator.GetService<DataMediatorService<RadarTargetDto>>();
            service.DataComed += RadarObj_DataComed;
        }

        private async Task LoadData()
        { 
            this.IsGen1Type = SensorVM.CurrSensor?.Type == GenType.Gen1Type;
            this.IsPaused = false;
            await Task.Delay(1000);
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

        private void SensorVM_PropertyChanged(SensorInfo info)
        {
            if (info == null) return;
            IsGen1Type = info.Type == Brigade.Models.EnumEntitys.GenType.Gen1Type;
            SensorId = $"0x{info.CanID:x}";
        }

        private void InitGenDataLists()
        {
            Gen2Radar = new Gen2RadarDto();
            Gen1Radar = new Gen1RadarDto();
            Init2xx();
            Init3xx();
        }

        private void RadarObj_DataComed(RadarTargetDto obj)
        {
            if (this._isPaused) return;
            var localObj = obj;
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (localObj.GenType == Brigade.Models.EnumEntitys.GenType.Gen1Type)
                {
                    foreach (var item in Gen1Radar.Gen1Items)
                    {
                        item.Items.Clear();
                    }
                    foreach (var item in localObj.RadarTargets)
                    {
                        var hobj = item.Value as RadarGen1Obj;
                        if (hobj == null) return;
                        Update3xx(hobj);
                    }
                    if (Gen1Radar?.Gen1Items != null && Gen1Radar.Gen1Items.Count > 0)
                    {
                        var hobj = localObj.RadarTargets.Values.FirstOrDefault() as RadarGen1Obj;
                        if (hobj != null)
                        {
                            Gen1Radar.MMIC = hobj.MMICError2;
                            Gen1Radar.CanStatus = hobj.CanError4;
                            Gen1Radar.Voltage = hobj.VoltageError1;
                            Gen1Radar.Temp = hobj.TemperatureError3;
                            Gen1Radar.FrameId = 0;
                            Gen1Radar.DetectedObjects = (byte)localObj.RadarTargets.Count;
                        }
                    }
                    if (Gen1Radar != null)
                    {
                        Gen1Radar.DetectedObjects = (byte)Gen1Radar.Gen1Items.Count;
                    }
                }
                else
                {
                    Resolve2XX(localObj);
                }
            }));
        }

        private void Resolve2XX(RadarTargetDto obj)
        {
            foreach (var item in Gen2Radar.Gen2Items)
            {
                item.Items.Clear();
            }
            RadarGen2Header header = obj.Gen2Header as RadarGen2Header;
            if (header != null)
            {
                Gen2Radar.IdentificationFlag = header.IdentificationFlag;
                Gen2Radar.FrameId = obj.FrameID;
                Gen2Radar.DetectedObjects = obj.NumObj;
                Gen2Radar.MMIC = header.MMICError2;
                Gen2Radar.Temp = header.TemperatureError3;
                Gen2Radar.TBD5 = header.TBDError5;
                Gen2Radar.TBD6 = header.TBDError6;
                Gen2Radar.GroundSpeed = header.GroundSpeed;
                Gen2Radar.CanStatus = header.CanError4;
            }
            foreach (var item in obj.RadarTargets)
            {
                var hobj = item.Value as RadarGen2Obj;
                if (hobj == null) return;
                Update2xx(hobj);
            }
        }
        private void Init2xx()
        {
            int pos = 0;
            Gen2Radar.IdentificationFlag = 0;
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "X Coordinate", Unit = "m" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Y Coordinate", Unit = "m" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Relative Speed Y", Unit = "KPH" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Rative Speed X", Unit = "KPH" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Existence Probability", Unit = "%" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Alive", Unit = "frame" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "RCS", Unit = "m²" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Magnitude", Unit = "n/a" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "SNR", Unit = "dB" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Acceleration X", Unit = "m/s²" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Acceleration Y", Unit = "m/s²" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Sport Attribute", Unit = "n/a" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Classification", Unit = "n/a" });
            Gen2Radar.Gen2Items.Add(new RadarParater() { Index = pos++, ColulmsName = "High Reflection Flag", Unit = "-" });
            //foreach (var item in Gen2Radar.Gen2Items)
            //{
            //    for (uint i = 0; i < 8; i++)
            //    {
            //        item.Items.Add(new DisplayObj() { Id = i + 1, Value = "--", IsSelected = i == 4 });
            //    }
            //}
        }
        private void Update2xx(RadarGen2Obj source, int index = 0)
        {
            if (source != null)
            {
                int pos = 0;
                var mIsSelected = source.Id == Gen2Id;
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.CoordinatesX.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.CoordinatesY.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.Vx.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.Vy.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.ProExist.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.Alive + "" });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.RCS.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.MAG + "" });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.SNR.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.AccX.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.AccY.ToString("F2") });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.MoveString[source.SportAttribut] });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.ClsassCate[source.Classification] });
                Gen2Radar.Gen2Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.YesArrs[source.HighReflectionFlag] });
            }
        }
        private void Init3xx()
        {
            int pos = 0;
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Distance", Unit = "m" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Angle", Unit = "Deg" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "X Coordinate", Unit = "m" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Y Coordinate", Unit = "m" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Relative speed", Unit = "KPH" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "signal Level", Unit = "dB" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Appearance Status", Unit = "-" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Trigger Event", Unit = "-" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Detection Flag", Unit = "-" });
            Gen1Radar.Gen1Items.Add(new RadarParater() { Index = pos++, ColulmsName = "Identification Flag", Unit = "-" });

            //foreach (var item in Gen1Radar.Gen1Items)
            //{
            //    for (uint i = 0; i < 8; i++)
            //    {
            //        item.Items.Add(new DisplayObj() { Id = i + 1, Value = "--", IsSelected = i == 5 });
            //    }
            //}
        }
        private void Update3xx(RadarGen1Obj source, int Id = 0)
        {
            var selectId = this.Gen1Id;
            if (source != null)
            {
                var mIsSelected = source.Id == selectId;
                int pos = 0;
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.PolarRadius.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.PolarAngle.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.CoordinatesX.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.CoordinatesY.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.Vr.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = source.SignalPower.ToString("F2") });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.RepeatArrs[source.Status] });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.Alivedecs[source.TriggerEvent] });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.VaildArrs[source.DetectionFlag] });
                this.Gen1Radar.Gen1Items[pos++].Items.Add(new DisplayObj() { IsSelected = mIsSelected, Id = source.Id, Value = RadarConstaint.IdentArrs[source.IdentificationFlag] });
            }
        }

        private void UpdateSelection()
        {
            var items = IsGen1Type ? Gen1Radar?.Gen1Items : Gen2Radar?.Gen2Items;
            if (items == null) return;

            foreach (var item in items)
            {
                if (item?.Items == null) continue;
                for (int i = 0; i < item.Items.Count; i++)
                {
                    if (item.Items[i] != null)
                    {
                        item.Items[i].IsSelected = (i + 1) == SelectedColumnIndex;
                    }
                }
            }
        }
    }

    public class Gen1RadarDto : ViewModelBase
    {
        #region 帧头
        private uint _frameId;
        public uint FrameId
        {
            get => _frameId;
            set => Set(ref _frameId, value);
        }

        private byte _detectedObjects;
        public byte DetectedObjects
        {
            get => _detectedObjects;
            set => Set(ref _detectedObjects, value);
        }

        public Byte Voltage
        {
            get => _Voltage;
            set => Set(ref _Voltage, value);
        }
        private Byte _Voltage;

        public Byte MMIC
        {
            get => _MMIC;
            set => Set(ref _MMIC, value);
        }
        private Byte _MMIC;
        public Byte Temp
        {
            get => _Temp;
            set => Set(ref _Temp, value);
        }
        private Byte _Temp;
        public Byte CanStatus
        {
            get => _CanStatus;
            set => Set(ref _CanStatus, value);
        }
        private Byte _CanStatus;
        #endregion

        #region 集合
        public ObservableCollection<RadarParater> Gen1Items { get; set; } = new ObservableCollection<RadarParater>();
        public ObservableCollection<DisplayObj> FirstRowItems
        {
            get
            { // 双层判空：外层行 + 内层单元格集合
                if (Gen1Items == null || Gen1Items.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                var firstRow = Gen1Items[0];
                if (firstRow.Items == null || firstRow.Items.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                return firstRow.Items;
            }
        }
        #endregion
    }
    /// <summary>
    /// 2XX 数据model
    /// </summary>
    public class Gen2RadarDto : ViewModelBase
    {
        #region 帧头
        private uint _frameId;
        public uint FrameId
        {
            get => _frameId;
            set => Set(ref _frameId, value);
        }

        private byte _detectedObjects;
        public byte DetectedObjects
        {
            get => _detectedObjects;
            set => Set(ref _detectedObjects, value);
        }

        private float _groundSpeed;
        public float GroundSpeed
        {
            get => _groundSpeed;
            set => Set(ref _groundSpeed, value);
        }

        public string IdentiType
        {
            get => _IdentifType;
            set => Set(ref _IdentifType, value);
        }
        private string _IdentifType = "-";

        public Byte IdentificationFlag
        {
            get => _IdentificationFlag;
            set  
            { 
                Set(ref _IdentificationFlag, value);
                IdentiType = RadarConstaint.IdentArrs[_IdentificationFlag];
                OnPropertyChanged(nameof(IdentiType));
            }
        }
        private Byte _IdentificationFlag;

        public Byte TBD5
        {
            get => _TBD5;
            set => Set(ref _TBD5, value);
        }
        private Byte _TBD5;

        public Byte TBD6
        {
            get => _TBD6;
            set => Set(ref _TBD6, value);
        }
        private Byte _TBD6;

        public Byte Voltage
        {
            get => _Voltage;
            set => Set(ref _Voltage, value);
        }
        private Byte _Voltage;

        public Byte MMIC
        {
            get => _MMIC;
            set => Set(ref _MMIC, value);
        }
        private Byte _MMIC;
        public Byte Temp
        {
            get => _Temp;
            set => Set(ref _Temp, value);
        }
        private Byte _Temp;
        public Byte CanStatus
        {
            get => _CanStatus;
            set => Set(ref _CanStatus, value);
        }
        private Byte _CanStatus;
        #endregion

        #region 集合
        public ObservableCollection<RadarParater> Gen2Items { get; set; } = new ObservableCollection<RadarParater>();
        public ObservableCollection<DisplayObj> FirstRowItems
        {
            get
            { // 双层判空：外层行 + 内层单元格集合
                if (Gen2Items == null || Gen2Items.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                var firstRow = Gen2Items[0];
                if (firstRow.Items == null || firstRow.Items.Count == 0)
                    return new ObservableCollection<DisplayObj>();
                return firstRow.Items;
            }
        }
        #endregion
    }
    public class RadarParater : ViewModelBase
    {
        private int _Index;
        public int Index
        {
            get => _Index;
            set => Set(ref _Index, value);
        }

        private string _Unit;
        public string Unit
        {
            get => _Unit;
            set => Set(ref _Unit, value);
        }

        private string _colsName;
        public string ColulmsName
        {
            get => _colsName;
            set => Set(ref _colsName, value);
        }
        public ObservableCollection<DisplayObj> Items { get; set; } = new ObservableCollection<DisplayObj>();
    }
}
