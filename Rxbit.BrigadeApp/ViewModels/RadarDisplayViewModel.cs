using Rxbit.Brigade.CanDevices.IServices;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class RadarDisplayViewModel : ViewModelBase
    {
        #region 字段
        private readonly List<ILiveService> liveServices = new List<ILiveService>();
        private readonly List<IRecordingService> recordingServices = new List<IRecordingService>();
        private double _scanAngle = -90;
        public event Action<SensorInfo, byte> NotifyChangeId;
        #endregion

        #region 属性

        private RadarCoordinateSystem _coordinateSystem;
        public RadarCoordinateSystem CoordinateSystem
        {
            get => _coordinateSystem;
            set => SetProperty(ref _coordinateSystem, value);
        }

        private ObservableCollection<RadarTarget> _targets = new ObservableCollection<RadarTarget>();
        public ObservableCollection<RadarTarget> Targets
        {
            get => _targets;
            set => SetProperty(ref _targets, value);
        }
        private RadarTarget _selectedTarget;
        public RadarTarget SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                // 取消旧目标的选中状态
                if (_selectedTarget != null)
                    _selectedTarget.IsSelected = false;

                if (SetProperty(ref _selectedTarget, value))
                {
                    // 设置新目标的选中状态
                    if (_selectedTarget != null)
                        _selectedTarget.IsSelected = true;
                    UpdateDetailInfo();
                    OnPropertyChanged(nameof(SelectedTargetInfo));
                    if (value != null)
                    {
                        Parallel.ForEach(NotifyChangeId.GetInvocationList(), u => {
                            u.DynamicInvoke(ViewModelLocator.GetService<SensorConfigurationViewModel>().CurrSensor, value.Id);
                        });
                    }
                }
            }
        }

        public ObservableCollection<RadarTarget> RadarTargets
        {
            get => _datatargets;
            set => SetProperty(ref _datatargets, value);
        }
        private ObservableCollection<RadarTarget> _datatargets = new ObservableCollection<RadarTarget>();

        private Byte _ObjId = 0;
        public Byte ObjId
        {
            get => _ObjId;
            set { SetProperty(ref _ObjId, value); }
        }

        private UInt32 frameId;
        public UInt32 FrameId
        {
            get { return frameId; }
            set { SetProperty(ref frameId, value); }
        }

        private UInt32 _sensorId;
        public UInt32 SensorId
        {
            get { return _sensorId; }
            set { SetProperty(ref _sensorId, value); }
        }

        private int _targetCount;
        public int TargetCount
        {
            get => _targetCount;
            set => SetProperty(ref _targetCount, value);
        }

        private double _zoomLevel = 1.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (SetProperty(ref _zoomLevel, value) && CoordinateSystem != null)
                {
                    CoordinateSystem.ZoomLevel = value;
                    OnPropertyChanged(nameof(ZoomInfoText));
                }
            }
        }

        private Point _zoomCenter = new Point(0, 0);
        public Point ZoomCenter
        {
            get => _zoomCenter;
            set
            {
                if (SetProperty(ref _zoomCenter, value) && CoordinateSystem != null)
                {
                    CoordinateSystem.ZoomCenter = value;
                }
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private string _coordinateText = "(0.0m, 0.0m)";
        public string CoordinateText
        {
            get => _coordinateText;
            set => SetProperty(ref _coordinateText, value);
        }

        private string _zoomInfoText = "";
        public string ZoomInfoText
        {
            get => _zoomInfoText;
            set => SetProperty(ref _zoomInfoText, value);
        }

        private string _fpsText = "";
        public string FpsText
        {
            get => _fpsText;
            set => SetProperty(ref _fpsText, value);
        }

        private string _selectedTargetInfo = "-";
        public string SelectedTargetInfo
        {
            get => _selectedTargetInfo;
            set => SetProperty(ref _selectedTargetInfo, value);
        }

        private bool _IsRecvi;
        public bool IsRecvi
        {
            get => _IsRecvi;
            set
            {
                if (SetProperty(ref _IsRecvi, value))
                {
                    OnPropertyChanged(nameof(CanStartRecv));
                    OnPropertyChanged(nameof(CanStartRecording));
                }
            }
        }

        private bool _isRecoding;
        public bool IsRecoding
        {
            get => _isRecoding;
            set
            {
                if (SetProperty(ref _isRecoding, value))
                {
                    OnPropertyChanged(nameof(CanStartRecording));
                }
            }
        }

        public bool CanStartRecording
        {
            get { return _IsRecvi & !_isRecoding; }
        }

        public bool CanStartRecv
        {
            get { return !_IsRecvi; }
        }
        private double _canvasWidth;
        public double CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                if (SetProperty(ref _canvasWidth, value) && CoordinateSystem != null)
                {
                    CoordinateSystem.ActualWidth = value;
                }
            }
        }

        private double _canvasHeight;
        public double CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                if (SetProperty(ref _canvasHeight, value) && CoordinateSystem != null)
                {
                    CoordinateSystem.ActualHeight = value;
                }
            }
        }

        public double ScanAngle
        {
            get => _scanAngle;
            private set => SetProperty(ref _scanAngle, value);
        }

        public bool? IsOnLine
        {
            get { return _IsOnLine; }
            set 
            {
                if (SetProperty(ref _IsOnLine, value)) 
                {
                    var vm = ViewModelLocator.GetService<SensorConfigurationViewModel>();
                    if (_IsOnLine == true && ReplayVM.SavePreSensor != null)
                    {
                        vm.NotifySensorChange(ReplayVM.SavePreSensor.CanID);
                        ReplayVM.SavePreSensor = null;
                    }
                    else
                    {
                    
                    }
                }

            }
        }
        private bool? _IsOnLine = true;


        public string RecodingTimeStr
        {
            get { return _RecodingTimeStr; }
            set { SetProperty(ref _RecodingTimeStr, value); }
        }
        private string _RecodingTimeStr = string.Empty;

        public string RecodingFoder
        {
            get { return _RecodingFoder; }
            set { SetProperty(ref _RecodingFoder, value); }
        }
        private string _RecodingFoder = string.Empty;
        #endregion

        #region 命令
        public ReplayEyeViewModel ReplayVM 
        { 
            get{ return replaymodel; }
            set { SetProperty(ref replaymodel,value); }
        }
        private ReplayEyeViewModel replaymodel;
        public AsyncRelayCommand StartCommand { get; }
        public AsyncRelayCommand StopCommand { get; }
        public AsyncRelayCommand StartRecodingCommand { get; }
        public AsyncRelayCommand StopRecodingCommand { get; }
        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand<object> CanvasSizeChangedCommand { get; }
        public RelayCommand<object> TargetSelectedCommand { get; }
        public RelayCommand<object> CanvasMouseWheelCommand { get; }
        public RelayCommand<object> CanvasMouseMoveCommand { get; }
        public RelayCommand<object> CanvasMouseDragCommand { get; }
        private readonly DataMediatorService<RadarTargetDto> dataMediator;
        #endregion

        #region 构造函数

        public RadarDisplayViewModel()
        {
            // 初始化坐标系统
            CoordinateSystem = new RadarCoordinateSystem
            {
                MinX = -9,
                MaxX = 9,
                MinY = 0,
                MaxY = 65,
                TickIntervalX = 1,
                TickIntervalY = 5,
                ScaleX = 1.0,
                ScaleY = 1.0,
                ZoomLevel = 1.0,
                ZoomCenter = new Point(0, 0),
                MarginLeft = 50,
                MarginRight = 50,
                MarginTop = 70,
                MarginBottom = 50
            };
            // 初始化命令
            this.StartCommand = new AsyncRelayCommand(StartRecvMsg);
            this.StopCommand = new AsyncRelayCommand(StopRecvMsg);
            this.StartRecodingCommand = new AsyncRelayCommand(StartRecoding);
            this.StopRecodingCommand = new AsyncRelayCommand(StopRecoding);
            this.CloseCommand = new RelayCommand<Window>((win) => win?.Close());
            this.CanvasSizeChangedCommand = new RelayCommand<object>(OnCanvasSizeChanged);
            this.TargetSelectedCommand = new RelayCommand<object>(OnTargetSelected);
            this.CanvasMouseWheelCommand = new RelayCommand<object>(OnCanvasMouseWheel);
            this.CanvasMouseDragCommand = new RelayCommand<object>(OnCanvasDrag);
            this.dataMediator = ViewModelLocator.GetService<DataMediatorService<RadarTargetDto>>();
            this.dataMediator.DataComed += DataMediator_DataComed;
            this.dataMediator.DataComed += DataBirdsView_DataComed;
            this.NotifyChangeId -= NotifyChangeId_DataComed;
            this.NotifyChangeId += NotifyChangeId_DataComed;
            var canLiveService = ViewModelLocator.GetService<CanLiveService>();
            var brigadeparse = ViewModelLocator.GetService<BrigadeCanMessageParser>();
            canLiveService.AddParseService(brigadeparse);
            liveServices.Add(canLiveService);
            recordingServices.Add(canLiveService);
            InitRadarTarget();
            ReplayVM = new ReplayEyeViewModel();
        }

        private void NotifyChangeId_DataComed(SensorInfo info, byte Id)
        {
            if (info.Type == GenType.Gen1Type || info.Type == GenType.Gen2Type)
            {
                this.ObjId = Id;
            }
            else
            {
                this.ObjId = 0;
            }
        }

        private void InitRadarTarget(int sid = -1)
        {
            for (byte i = 0; i <= 16; i++)
            {
                this.Targets.Add(new RadarTarget() { Id = (byte)(i), IsSelected = i == sid });
            }
        }

        private async Task StopRecoding()
        {
            if (!this.IsRecvi || !this.IsRecoding) return;
            this.IsRecoding = false;
            await Task.Run(() =>
            {
                Parallel.ForEach(recordingServices, services =>
                {
                    services.StopRecording();
                });
                _flushTimer?.Stop();
                RecodingTimeStr = string.Empty;
                Timestap = 0;
            });
        }
        private DispatcherTimer _flushTimer;
        private async Task StartRecoding()
        {
            if (!this.IsRecvi || this.IsRecoding) return;

            var bus = ViewModelLocator.GetService<SensorConfigurationViewModel>().BusConfig;
            string recodngPath = Path.Combine(RadarConstaint.RecondingPath, RecodingFoder);
            if (string.IsNullOrEmpty(RecodingFoder)) 
            {
                recodngPath = Path.Combine(RadarConstaint.RecondingPath, $"Brigade_{bus.Sensor.Type}_{bus.Sensor.CanID:x}");
            }
            if (!Directory.Exists(recodngPath))
            {
                Directory.CreateDirectory(recodngPath);
            }
            string file = Path.Combine(recodngPath, $"Brigade_{bus.Sensor.CanID:x}_{DateTime.Now:HH_mm_ss}");
            this.IsRecoding = true;
            await Task.Run(() =>
            {
                Parallel.ForEach(recordingServices, services =>
                {
                    services.StartRecording(file);
                });
                ConfigHelper.SaveConfig(bus, Path.Combine(file, RadarConstaint.BUSFile));
            });
            _flushTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _flushTimer.Tick += FlushBufferToUi;
            _flushTimer?.Start();
        }
        private long Timestap = 0;
        private void FlushBufferToUi(object sender, EventArgs e)
        {
            Timestap++;
            var minte = Timestap % 3600 / 60;
            var ms = Timestap % 3600 % 60;
            RecodingTimeStr = $"{Timestap/3600:D2}:{minte:D2}:{ms:D2}";
        }

        private async Task StartRecvMsg()
        {
            if (IsRecvi) return;
            ClearTargets();
            var bus = ViewModelLocator.GetService<SensorConfigurationViewModel>().BusConfig;
            this.IsRecvi = true;
            await Task.Run(() =>
            {
                Parallel.ForEach(liveServices, services =>
                {
                    services.Init(bus);
                    services.StartReceive(0);
                });
                dataMediator.Open();
                ViewModelLocator.GetService<CanCommService>().Open();
            });
        }

        private async Task StopRecvMsg()
        {
            if (!this.IsRecvi) return;
            if (this.IsRecoding)
            {
                await StopRecoding();
                await Task.Delay(1000);
            }
            this.IsRecvi = false;
            await Task.Run(() =>
            {
                Parallel.ForEach(liveServices, services => { services.StopReceive(0); });
                dataMediator.Close();
                ViewModelLocator.GetService<CanCommService>().Open();
            });
        }

        #endregion

        #region 方法
        private void ClearTargets()
        {
            IsRecvi = false;
            foreach (var item in this.Targets)
            {
                item.IsVisibled = false;
            }
            SelectedTarget = null;
            TargetCount = 0;
            ObjId = 0;
            SelectedTargetInfo = "";
            StatusText = "Targets Cleared";
        }

        private void ResetZoom()
        {
            ZoomLevel = 1.0;
            ZoomCenter = new Point(0, 0);
            ZoomInfoText = "Zoom: 1.0x";
        }

        private void OnCanvasSizeChanged(object param)
        {
            if (param is Size size)
            {
                CanvasWidth = size.Width;
                CanvasHeight = size.Height;
            }
        }

        private void OnTargetSelected(object param)
        {
            if (param is RadarTarget target)
            {
                SelectedTarget = target;
                StatusText = $"Selected Target: ID={target.Id:00} | Type: {target.Type}";
            }
            else
            {
                SelectedTarget = null;
                StatusText = "Radar Ready";
            }
        }

        private void OnCanvasMouseWheel(object param)
        {
            if (param is Tuple<Point, int> data && CoordinateSystem != null)
            {
                Point mousePos = data.Item1;
                int delta = data.Item2;

                var worldPos = CoordinateSystem.ScreenToWorld(mousePos.X, mousePos.Y);
                double zoomDelta = delta > 0 ? 1.15 : 0.87;
                double newZoom = ZoomLevel * zoomDelta;
                newZoom = Math.Max(0.3, Math.Min(8.0, newZoom));

                ZoomLevel = newZoom;
                ZoomCenter = new Point(
                    ZoomCenter.X + (worldPos.X - ZoomCenter.X) * (1 - 1 / zoomDelta),
                    ZoomCenter.Y + (worldPos.Y - ZoomCenter.Y) * (1 - 1 / zoomDelta)
                );
                ZoomInfoText = $"Zoom: {newZoom:F1}x";
            }
        }

        private void OnCanvasDrag(object param)
        {
            if (param is Tuple<Point, Point> data && CoordinateSystem != null)
            {
                Point delta = data.Item1;      // 鼠标位移
                Point dragStart = data.Item2;  // 拖拽起始中心

                double ppmX = CoordinateSystem.PixelPerMeterX;
                double ppmY = CoordinateSystem.PixelPerMeterY;

                ZoomCenter = new Point(
                    dragStart.X - delta.X / ppmX,
                    dragStart.Y + delta.Y / ppmY
                );
            }
        }

        private void UpdateDetailInfo()
        {
            if (SelectedTarget != null)
            {
                var t = SelectedTarget;
                SelectedTargetInfo = SelectedTarget.Desc;
            }
            else
            {
                SelectedTargetInfo = "UnSelected Target";
            }
        }

        public void UpdateCoordinateText(Point worldPos)
        {
            CoordinateText = $"O({worldPos.X:F1}m, {worldPos.Y:F1}m)";
        }

        #endregion

        #region 航迹目标更新
        private void DataBirdsView_DataComed(RadarTargetDto tagertobj)
        {
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var radar in this.Targets)
                {
                    if (tagertobj.RadarTargets.TryGetValue(radar.Id, out BaseRadarTarget target))
                    {
                        radar.Id = target.Id;
                        radar.VY = target.Vy;
                        radar.VX = target.Vx;
                        radar.X = target.X;
                        radar.Y = target.Y;
                        radar.PolarRadius = target.PolarRadius;
                        radar.VR = target.Vr;
                        radar.PolarAngle = target.PolarAngle;
                        radar.Desc = target.Desc;
                        radar.IsVisibled = true;
                        if (tagertobj.GenType == GenType.Gen2Type)
                        {
                            var gen2obj = target as RadarGen2Obj;
                            radar.ClassType = gen2obj.Classification;
                            radar.MotionType = gen2obj.SportAttribut;
                        }
                        else
                        {
                            radar.ClassType = -1;
                            radar.MotionType = 0;
                        }
                    }
                    else
                    {
                        radar.IsVisibled = false;
                        radar.Desc = string.Empty;
                    }
                    if (SelectedTarget != null && SelectedTarget.Id == radar.Id) UpdateDetailInfo();
                }
            });
        }
        private void DataMediator_DataComed(RadarTargetDto tagertobj)
        {
            byte selectId = this.ObjId;
            Application.Current.Dispatcher.Invoke(() => {

                if (tagertobj.GenType == Brigade.Models.EnumEntitys.GenType.Gen2Type)
                {
                    this.FrameId = tagertobj.FrameID;
                    this.SensorId = tagertobj.CanID;
                }
                this.TargetCount = tagertobj.RadarTargets.Count;
                this.RadarTargets.Clear();
                foreach (var target in tagertobj.RadarTargets.Values)
                {
                    var obj = new RadarTarget()
                    {
                        Id = target.Id,
                        VY = target.Vy,
                        VX = target.Vx,
                        X = target.X,
                        Y = target.Y,
                        PolarRadius = target.PolarRadius,
                        VR = target.Vr,
                        IsSelected = selectId == target.Id,
                        PolarAngle = target.PolarAngle
                    };
                    obj.Status = $"0x{tagertobj.CanID:x}";
                    this.RadarTargets.Add(obj);
                }
            });
        }
        #endregion
    }
}
