using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.CanDevices.Structs;
using Rxbit.Brigade.EcuServices.Utils;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class CanDeviceViewModel : ViewModelBase, IDisposable
    {
        private readonly CanCommService service;
        private uint SensorId = 0;
        public Action<bool> OpenSuccess { get; set; }
        public ObservableCollection<DataProtocol> DataProtocols { get; private set; } = new ObservableCollection<DataProtocol>();
        public ObservableCollection<ChannelBaudRate> BaudRates { get; private set; } = new ObservableCollection<ChannelBaudRate>();
        public ObservableCollection<ChannelDataBaudRate> DataBaudRates { get; private set; } = new ObservableCollection<ChannelDataBaudRate>();
        public ObservableCollection<ChannelResistanceEnable> ResistanceEnableStates { get; private set; } = new ObservableCollection<ChannelResistanceEnable>();
        public ObservableCollection<CanCategory> CanCategorys { get; private set; } = new ObservableCollection<CanCategory>();
        public ObservableCollection<Byte> CanIndexs { get; private set; } = new ObservableCollection<Byte>();
        public ObservableCollection<Byte> ChannelIndexs { get; private set; } = new ObservableCollection<Byte>();
        public ObservableCollection<CanDeviceType> DeviceTypes { get; } = new ObservableCollection<CanDeviceType>();

        private CanDeviceType _selectedDeviceType = CanDeviceType.PeakCan;
        public CanDeviceType SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (value != _selectedDeviceType)
                {
                    SetProperty(ref _selectedDeviceType, value);
                    ValueChanged(value);
                }
            }
        }

        public CanCategory SelectCategory
        {
            get { return Config.CCategroy; }
            set
            {
                if (Config.CCategroy != value)
                {
                    Config.CCategroy = value;
                    OnPropertyChanged(nameof(SelectCategory));
                    ValueChangeCate(value);
                }
            }
        }

        public DataProtocol SelectProtocol
        {
            get { return Config.DataProto; }
            set
            {
                if (Config.DataProto != value)
                {
                    Config.DataProto = value;
                    if (value == DataProtocol.CANFD)
                    {
                        IsCanfdShow = Visibility.Visible;
                    }
                    else
                    {
                        IsCanfdShow = Visibility.Collapsed;
                    }
                }
                OnPropertyChanged(nameof(SelectProtocol));
            }
        }
        public Visibility IsCanfdShow
        {
            get => _IsCanfdShow;
            set => SetProperty(ref _IsCanfdShow, value);
        }
        private Visibility _IsCanfdShow = Visibility.Collapsed;
        public Visibility IsEnabledResistance
        {
            get => _IsEnabledResistance;
            set => SetProperty(ref _IsEnabledResistance, value);
        }
        private Visibility _IsEnabledResistance = Visibility.Collapsed;

        public Visibility IsDevIndexShow
        {
            get => _IsDevIndexShow;
            set => SetProperty(ref _IsDevIndexShow, value);
        }
        private Visibility _IsDevIndexShow = Visibility.Collapsed;

        public bool IsCanfdEnabled
        {
            get => _IsCanfdEnabled;
            set => SetProperty(ref _IsCanfdEnabled, value);
        }
        private bool _IsCanfdEnabled = true;

        private bool _isDeviceOpen = true;
        public bool IsDeviceBtn
        {
            get => _isDeviceOpen;
            set => SetProperty(ref _isDeviceOpen, value);
        }

        private bool _IsCanConnected = false;
        public bool IsCanConnected
        {
            get => _IsCanConnected;
            set => SetProperty(ref _IsCanConnected, value);
        }

        private string _ErrorMsg = "";
        public string ErrorMsg
        {
            get => _ErrorMsg;
            set => SetProperty(ref _ErrorMsg, value);
        }

        private string version = "V1.0.0";
        public string Version
        {
            get { return version; }
            set => SetProperty(ref version, value);
        }

        private bool IsEnabled;

        public ChannelConfig Config
        {
            get { return ConfigBus.ChannelConfig; }
            set
            {
                ConfigBus.ChannelConfig = value;
                OnPropertyChanged(nameof(Config));
            }
        }
        public AsyncRelayCommand ToggleDeviceCommand { get; }
        public AsyncRelayCommand ConnectCanCommand { get; }
        public AsyncRelayCommand DisConnectCanCommand { get; }
        public RelayCommand LoadConmand { get; }
        public SensorConfigurationViewModel SensorVM { get; set; }
        private readonly BrigadeConfigBus ConfigBus;
        public CanDeviceViewModel()
        {
            SensorVM = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            ConfigBus = SensorVM.BusConfig;
            ViewModelLocator.GetService<NetworkSummaryViewModel>();
            ViewModelLocator.GetService<DetectionSensorInfoViewModel>();
            ViewModelLocator.GetService<MainWindowViewModel>();
            Config = new ChannelConfig()
            {
                CCategroy = CanCategory.PEAK_CAN,
                CanIndex = 0,
                ChannelIndex = 1,
                BaudRate = ChannelBaudRate._500Kbps,
                DataBaudRate = ChannelDataBaudRate._2000Kbps,
                DataProto = DataProtocol.CAN,
                Id = 1,
                IsResistanceEnable = ChannelResistanceEnable.Disable,
            };
            ConfigBus.ChannelConfig = Config;
            IsCanfdShow = Visibility.Collapsed;
            IsDevIndexShow = Visibility.Collapsed;
            IsCanfdEnabled = true;
            Init();
            ToggleDeviceCommand = new AsyncRelayCommand(OnToggleDevice);
            ConnectCanCommand = new AsyncRelayCommand(Connect, () => IsDeviceBtn);
            DisConnectCanCommand = new AsyncRelayCommand(DisConnect, () => !IsDeviceBtn);
            LoadConmand = new RelayCommand(LoadCondig);
            service = ViewModelLocator.GetService<CanCommService>();
            RadarConstaint.Init();
        }

        private void LoadCondig()
        {
            if (this.SensorId != 0) return;
            if (File.Exists(RadarConstaint.SettingFile))
            {
                var bus = ConfigHelper.ReadConfig<BrigadeConfigBus>(RadarConstaint.SettingFile);
                if (bus != null)
                {
                    CanDevCategoryAttribute devCategoryAttr = EnumAttributeHelper.GetEnumMemberAttribute<CanDevCategoryAttribute>(bus.ChannelConfig.CCategroy);
                    this.SelectedDeviceType = devCategoryAttr.Type;
                    this.SelectCategory = bus.ChannelConfig.CCategroy;
                    this.SelectProtocol = bus.ChannelConfig.DataProto;
                    Config = bus.ChannelConfig;
                    //if (bus.Sensor != null) 
                    //{
                    //    ConfigBus.Sensor.CanID = bus.Sensor.CanID;
                    //    ConfigBus.Sensor.Type = bus.Sensor.Type;
                    //}
                }
                ConfigBus.UdsRx = bus.UdsRx;
                ConfigBus.UdsTx = bus.UdsTx;
            }
            if (ConfigBus.AppPath == null)
            {
                ConfigBus.AppPath = new AppPathInfo()
                {
                    RecodingPath = RadarConstaint.RecondingPath,
                    AppHexDefaultPath = RadarConstaint.RecondingPath,
                    SettingPath = RadarConstaint.AppWorkPath
                };
            }
            if (!File.Exists(RadarConstaint.PCANPath))
            {
                ZipHelper.ExtractZip(RadarConstaint.BrigaeSdkPath, RadarConstaint.AppWorkPath, overwriteFile: true);
            }
            
        }

        private void ValueChanged(CanDeviceType Categroy)
        {
            var list = EnumAttributeHelper.GetEnumMemberAttributes<CanCategory, CanDevCategoryAttribute>();
            this.CanCategorys.Clear();
            foreach (var item in list)
            {
                if (item.Value.Type == Categroy)
                {
                    this.CanCategorys.Add(item.Key);
                }
            }
            if (Categroy == CanDeviceType.ZLGCAN)
            {
                this.SelectCategory = this.CanCategorys.Where(u => u == CanCategory.ZLG_USBCANFD_200U).FirstOrDefault();
            }
            else 
            {
                this.SelectCategory = this.CanCategorys.First();
            }
            
            ChannelIndexs.Clear();
            byte i = 0;
            if (Categroy == CanDeviceType.PeakCan || Categroy == CanDeviceType.Kvaser)
            {
                this.IsDevIndexShow = Visibility.Collapsed;
                if (Categroy == CanDeviceType.PeakCan)
                {
                    i = 1;
                }
            }
            else
            {
                this.IsDevIndexShow = Visibility.Visible;
            }
            for (byte j = i; j <= 8; j++)
            {
                ChannelIndexs.Add(j);
            }
            this.Config.ChannelIndex = ChannelIndexs.FirstOrDefault();
            OnPropertyChanged(nameof(this.Config));
        }

        private void ValueChangeCate(CanCategory category)
        {
            switch (category)
            {
                case CanCategory.ZLG_USBCANFD_100U:
                case CanCategory.ZLG_USBCANFD_200U:
                case CanCategory.GCAN_USBCAN_FD:
                case CanCategory.PEAK_CAN:
                case CanCategory.CXKJ_USBCANFD:
                    {
                        IsCanfdShow = Visibility.Visible;
                        IsCanfdEnabled = true;
                        Config.IsResistanceEnable = ChannelResistanceEnable.Enable;
                    }
                    break;
                case CanCategory.CXKJ_USBCAN_I:
                case CanCategory.CXKJ_CANalyst_II:
                case CanCategory.ZLG_USBCAN_I:
                case CanCategory.ZLG_USBCAN_II:
                case CanCategory.ZLG_USBCAN_2EU:
                case CanCategory.GCAN_USBCAN_V4:
                case CanCategory.Kvaser_USBCAN:
                case CanCategory.GCAN_USBCAN_V5:
                    {
                        IsCanfdShow = Visibility.Collapsed;
                        IsCanfdEnabled = false;
                        SelectProtocol = DataProtocol.CAN;
                    }
                    break;
                default:
                    break;
            }
            SelectProtocol = DataProtocol.CAN;
            Config.BaudRate = ChannelBaudRate._500Kbps;
        }
        private void Init()
        {
            //foreach (CanDeviceType cc in Enum.GetValues(typeof(CanDeviceType)))
            //{
            //    this.DeviceTypes.Add(cc);
            //}
            this.DeviceTypes.Add(CanDeviceType.ZLGCAN);
            this.DeviceTypes.Add(CanDeviceType.PeakCan);
            this.DeviceTypes.Add(CanDeviceType.Kvaser);
            // 1. 获取枚举所有成员的特性
            var list = EnumAttributeHelper.GetEnumMemberAttributes<CanCategory, CanDevCategoryAttribute>();
            CanDevCategoryAttribute devCategoryAttr = EnumAttributeHelper.GetEnumMemberAttribute<CanDevCategoryAttribute>(Config.CCategroy);
            this.SelectedDeviceType = devCategoryAttr.Type;
            this.CanCategorys.Clear();
            foreach (var item in list)
            {
                if (item.Value.Type == this.SelectedDeviceType)
                {
                    this.CanCategorys.Add(item.Key);
                }
            }

            for (Byte i = 0; i < 8; i++)
            {
                this.CanIndexs.Add(i);
            }

            byte j = (byte)(this.SelectedDeviceType == CanDeviceType.PeakCan ? 1 : 0);
            for (Byte i = j; i < 16; i++)
            {
                this.ChannelIndexs.Add(i);
            }

            foreach (DataProtocol item in Enum.GetValues(typeof(DataProtocol)))
            {
                this.DataProtocols.Add(item);
            }
            foreach (ChannelBaudRate item in Enum.GetValues(typeof(ChannelBaudRate)))
            {
                this.BaudRates.Add(item);
            }
            foreach (ChannelDataBaudRate item in Enum.GetValues(typeof(ChannelDataBaudRate)))
            {
                this.DataBaudRates.Add(item);
            }
            foreach (ChannelResistanceEnable item in Enum.GetValues(typeof(ChannelResistanceEnable)))
            {
                this.ResistanceEnableStates.Add(item);
            }
        }
        private async Task OnToggleDevice()
        {
            var vm = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            if (this.SensorId == 0)
            {
                this.ErrorMsg = "No checked SensorID!";
                this.IsCanConnected = true;
                this.SensorId = 0x220;
                //return;
            }
            vm.NotifySensorChange(this.SensorId);
            service.CloseAll();
            service.CanDataComed -= Service_CanDataComed;
            OpenSuccess?.Invoke(true);
        }

        private async Task Connect()
        {
            IsEnabled = false;
            service.ChannelDisconned += Service_ChannelDisconned;
            service.CanDataComed += Service_CanDataComed;
            await Task.Run(new Action(() => {
                if (service.Open(Config)) IsEnabled = true;
            }));
            await Task.Delay(1000);
            if (!IsEnabled)
            {
                MessageBox.Show("this CAN Device is Not Opened!");
                return;
            }
            this.IsCanConnected = service.IsOpen;
            this.IsDeviceBtn = false;
            await OnToggleDevice();
        }

        private void Service_CanDataComed(ICanChannelService channel, List<RXCanMessage> messages)
        {
            var frame = messages.FirstOrDefault();
            this.ErrorMsg = $"Recvive CAN ID:{frame.ID:x}, data:{BitConverter.ToString(frame.DATA)}";
            this.IsEnabled = true;
            if (this.SensorId == 0)
            {
                foreach (var item in messages)
                {
                    var cid = item.ID >> 4;
                    if (cid >= 0x31 && cid <= 0x39)
                    {
                        service.CanDataComed -= Service_CanDataComed;
                        SensorId = item.ID & 0x3F0;
                        this.ErrorMsg = $"Current SendorId:{SensorId:x}";
                        return;
                    }
                    if (cid >= 0x21 && cid <= 0x28)
                    {
                        service.CanDataComed -= Service_CanDataComed;
                        SensorId = item.ID & 0x2F0;
                        this.ErrorMsg = $"Current SendorId:{SensorId:x}";
                        return;
                    }
                }
            }
        }

        private void Service_ChannelDisconned(ChannelConfig cfg)
        {
            this.ErrorMsg = $"this Can【{cfg.CCategroy}】channel【{cfg.ChannelIndex}】is already Disconnect!";
            this.IsCanConnected = false;
            service.ChannelDisconned -= Service_ChannelDisconned;
            service.CanDataComed -= Service_CanDataComed;
        }

        private async Task DisConnect()
        {
            service.CloseAll();
            await Task.Delay(50);
            this.IsDeviceBtn = true;
            this.IsCanConnected = false;
            this.ErrorMsg = string.Empty;
        }

        public void Dispose()
        {
            service.CloseAll();
            service.ChannelDisconned -= Service_ChannelDisconned;
            service.CanDataComed -= Service_CanDataComed;
        }
    }
}
