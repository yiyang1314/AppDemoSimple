using Microsoft.Win32;
using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.CanDevices.Structs;
using Rxbit.Brigade.EcuServices.Services;
using Rxbit.Brigade.EcuServices.Utils;
using Rxbit.Brigade.EcuServices.ZflashItems;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class SensorConfigurationViewModel : ViewModelBase
    {
        private readonly CanCommService service;
        private readonly BrigadeConfigBus bus;

        public ObservableCollection<SensorInfo> Sensors
        {
            get => _sensors;
            set
            {
                SetProperty(ref _sensors, value);
                OnPropertyChanged();
            }
        }
        private ObservableCollection<SensorInfo> _sensors;

        public event Action<SensorInfo> SensorChangeEvevt;

        public ObservableCollection<GenType> Types => new ObservableCollection<GenType> { GenType.Gen1None, GenType.Gen1Type, GenType.Gen2Type };

        private string _FirmWareVersion = " --- ";
        public string FirmWareVersion
        {
            get { return _FirmWareVersion; }
            set { SetProperty(ref _FirmWareVersion, value); }
        }

        public GenType CurrGenType
        {
            get => _currGenType;
            set
            {
                SetProperty(ref _currGenType, value);
            }
        }
        private GenType _currGenType = GenType.Gen1Type;
        private string _TpMsg = "";
        public string TipMsg
        {
            get { return _TpMsg; }
            set { SetProperty(ref _TpMsg, value); }
        }
        public SensorInfo CurrSensor
        {
            get => bus.Sensor;
            set
            {
                bus.Sensor = value;
                OnPropertyChanged(nameof(CurrSensor.Desc));
                OnPropertyChanged(nameof(CurrSensor));
                OnPropertyChanged(nameof(bus.Sensor));
            }
        }

        public SensorInfo SelectSensor
        {
            get => _selectSensor;
            set
            {
                SetProperty(ref _selectSensor, value);
                OnPropertyChanged(nameof(SelectSensor.Desc));
            }
        }
        private SensorInfo _selectSensor;

        private volatile bool EnbaledChangeSensor;
        private volatile int SensorCount;
        public AsyncRelayCommand ChangeConfigCommand { get; }
        public AsyncRelayCommand UpdateCommand { get; }
        public AsyncRelayCommand ChangeSensorCammand { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand<Window> CloseWinCommand { get; }
        private AutoResetEvent ResetEvent = new AutoResetEvent(false);
        public ChannelConfig Cfg
        {
            get => bus.ChannelConfig;
            set
            {
                bus.ChannelConfig = value;
                OnPropertyChanged(nameof(Cfg));
            }
        }
        public SensorConfigurationViewModel()
        {
            Sensors = new ObservableCollection<SensorInfo>(SensorInfo.CreateData());
            bus = ViewModelLocator.GetService<BrigadeConfigBus>();
            CurrSensor = Sensors.Where(u=>u.Type == GenType.GenNoInit).LastOrDefault();
            SelectSensor = CurrSensor;
            ChangeConfigCommand = new AsyncRelayCommand(CheckFireWare);
            UpdateCommand = new AsyncRelayCommand(UpdateFireWare);
            SaveCommand = new AsyncRelayCommand(SaveConfig);
            ChangeSensorCammand = new AsyncRelayCommand(SwitchSensorInfo);
            CloseWinCommand = new RelayCommand<Window>(CloseWindow);
            service = ViewModelLocator.GetService<CanCommService>();
        }
        private void CloseWindow(Window window)
        {
            service.CanDataSent -= Service_CanDataSent;
            service.CanDataComed -= Service_CanDataComed;
            window?.Close();
            this.TipMsg = "";
        }
        private async Task SwitchSensorInfo()
        {
            this.TipMsg = "";
            if (CurrSensor == null) return;
            if (SelectSensor == null) return;
            uint currID = CurrSensor.CanID;
            uint afterID = SelectSensor.CanID;
            if (currID == afterID) return;
            await SwitchSensorInfo(currID, afterID);
        }
        private async Task SwitchSensorInfo(uint currID, uint afterID)
        {
            if (CurrSensor == null) return;
            if (SelectSensor == null) return;
            if (currID == afterID) return;
            var mst = await Task.Run(() => {
                ResetEvent.Reset();
                var data = new byte[8];
                uint toggleID = 0;
                byte type = 0;
                if (!BrigadeHeler.GetSensor(currID, afterID, ref toggleID, ref type))
                {
                    return "this select SensorID is Not Exist!";
                }
                service.CanDataSent -= Service_CanDataSent;
                service.CanDataComed -= Service_CanDataComed;
                service.CanDataSent += Service_CanDataSent;
                service.CanDataComed += Service_CanDataComed;
                data[0] = type;
                data[7] = 0xff;
                if (!service.Send(Cfg, toggleID, data))
                {
                    return "this can Devices is not Open!";
                }

                if (!WaitOne(3000))
                {
                    return "this sensorID is switch faild!";
                }
                return string.Empty;
            });
            if (string.IsNullOrEmpty(mst))
            {
                IsSuccess = true;
                this.TipMsg = "this sensorID is switch sucessfully!";
                ChangeSensorId();
            }
            else
            {
                this.TipMsg = mst;
                IsSuccess = false;
            }
        }

        private void ToggleSensorID()
        {
            var data = new byte[8];
            uint toggleID = 0;
            byte type = 0;
            if (CurrSensor != null && BrigadeHeler.GetSensor(CurrSensor.CanID, 0x390, ref toggleID, ref type))
            {
                data[0] = type;
                data[7] = 0xff;
                service.Send(Cfg, toggleID, data);
            }
        }

        private void ChangeSensorId()
        {
            if (this.CurrSensor.CanID != this.SelectSensor.CanID)
            {
                Parallel.ForEach(SensorChangeEvevt.GetInvocationList(), e =>
                {
                    e.DynamicInvoke(this.SelectSensor);
                });
            }
            this.CurrSensor = this.SelectSensor;
        }
        public void NotifySensorChange(uint SensorId)
        {
            if (SensorId == this.CurrSensor.CanID) return;
            this.CurrSensor = this.Sensors.Where(u=>u.CanID == SensorId).LastOrDefault();
            Parallel.ForEach(SensorChangeEvevt.GetInvocationList(), e =>
            {
                e.DynamicInvoke(this.CurrSensor);
            });
        }
        private bool WaitOne(int timeout = 3000)
        {
            this.EnbaledChangeSensor = true;
            SensorCount = 0;
            Task.Run(() =>
            {
                var stop = new Stopwatch();
                stop.Start();
                while (stop.ElapsedMilliseconds < timeout)
                {
                    if (SensorCount > 0)
                    {
                        break;
                    }
                    else
                    {
                        Task.Delay(100);
                    }
                }
                stop.Stop();
            });
            ResetEvent.WaitOne(5000);
            this.EnbaledChangeSensor = false;
            return SensorCount > 0;
        }

        private async Task SaveConfig()
        {
            await Task.Run(new Action(() => {
                ConfigHelper.SaveConfig(bus, RadarConstaint.SettingFile);
            }));
            await Task.Delay(10);
        }

        private async Task UpdateFireWare()
        {
            var cfg = ViewModelLocator.GetService<BrigadeConfigBus>();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = cfg.AppPath.AppHexDefaultPath;
            dialog.DefaultExt = ".hex"; // Default file extension
            dialog.Filter = "App File(.hex)|*.hex"; // Filter files by extension
            bool? result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }
            ToggleSensorID();
            string appHexh = dialog.FileName;
            cfg.AppPath.AppHexDefaultPath = Path.GetDirectoryName(appHexh);
            var ret = await UpgradeApp(appHexh);
            if (ret)
            {
                await SwitchSensorInfo(0x390, CurrSensor.CanID);
            }
            this.CurrSensor = this.Sensors.Where(u => u.CanID == 0x390).First();
        }

        private async Task<bool> UpgradeApp(string hexFile)
        {
            SimpleEcuTool.DirsKey_DataComed = RadarConstaint.GetInfo;
            var list = SimpleEcuTool.ParseHexPackage(EcuRadar.Brigade_Radar, hexFile, out string msg);
            if (!string.IsNullOrEmpty(msg) || list.Count == 0)
            {
                this.TipMsg = msg;
                return false;
            }
            UpgradeFirmWareWin upgade = new UpgradeFirmWareWin();
            UpgradeFirmWareWinModel vm = upgade.DataContext as UpgradeFirmWareWinModel;
            vm.TotalFlows = list.Count;
            UdsService udsServices = new UdsService(service, 0);
            udsServices.DirsKey_DataComed = RadarConstaint.GetInfo;
            udsServices.channelConfig = Cfg;
            vm.UdsV1Service = udsServices;
            vm.UdsV1Service.SourceId = bus.UdsTx;
            vm.UdsV1Service.TargetId = bus.UdsRx;
            vm.ZFlashItems = new ObservableCollection<ZFlashItem>(list);
            upgade.Owner = Application.Current.MainWindow;
            var ret = upgade.ShowDialog() == true;
            if (!ret)
            {
                if (vm.IsExeCateStatus)
                {
                    if (vm.UdsV1Service.IsCheckVersion)
                    {
                        this.TipMsg = vm.ErrorMsg;
                    }
                    else
                    {
                        this.TipMsg = "Upgrade App Version is Faild! please switch geateset Version";
                    }
                    this.IsSuccess = vm.UdsV1Service.IsCheckVersion;
                }
                else
                {
                    this.IsSuccess = false;
                    this.TipMsg = vm.ErrorMsg;
                }
            }
            return ret == true;
        }
        private async Task CheckFireWare()
        {
            this.TipMsg = "";
            ResetEvent.Reset();
            service.CanDataSent -= Service_CanDataSent;
            service.CanDataComed -= Service_CanDataComed;
            service.CanDataSent += Service_CanDataSent;
            service.CanDataComed += Service_CanDataComed;
            Cfg = ViewModelLocator.GetService<CanDeviceViewModel>().Config;
            var data = new byte[8];

            FirmWareVersion = "";
            if (!SendUDS(new byte[] {0x02,0x10,0x03 }))
            {
                this.TipMsg = $"this UDS[10] service is NPC[{recsp[1]:x} {recsp[2]:x} {recsp[3]:x}]";
                IsSuccess = false;
                return;
            }
            if (!SendUDS(new byte[] { 0x02, 0x11, 0x01 }))
            {
                this.TipMsg = $"this UDS[11] service is NPC[{recsp[1]:x} {recsp[2]:x} {recsp[3]:x}]";
                IsSuccess = false;
            }
            await Task.Delay(1000);
            service.CanDataSent -= Service_CanDataSent;
            service.CanDataComed -= Service_CanDataComed;
        }

        public bool SendUDS(byte[] txdata,int timeout = 3000)
        {
            Array.Clear(recsp,0,8);
            Buffer.BlockCopy(txdata, 0, recsp, 0, txdata.Length);
            if (!service.Send(Cfg, bus.UdsTx, recsp)) 
            {
                return false;
            }
            ResetEvent.WaitOne(timeout);
            return (txdata[1]+0x40) == recsp[1];
        }
        private async Task CheckFireWare1()
        {
            this.TipMsg = "";
            ResetEvent.Reset();
            service.CanDataSent -= Service_CanDataSent;
            service.CanDataComed -= Service_CanDataComed;
            service.CanDataSent += Service_CanDataSent;
            service.CanDataComed += Service_CanDataComed;
            Cfg = ViewModelLocator.GetService<CanDeviceViewModel>().Config;
            IsSuccess = SendUDS(new byte[] { 0x03, 0x22, 0xf1, 0x95 });
            if (!IsSuccess)
            {
                this.TipMsg = "this can devices is Not Opened!";
            }
            await Task.Delay(10);
            service.CanDataSent -= Service_CanDataSent;
            service.CanDataComed -= Service_CanDataComed;
        }
        private byte[] recsp = new byte[8];
        private void Service_CanDataComed(ICanChannelService arg1, List<RXCanMessage> messages)
        {
            if (this.EnbaledChangeSensor)
            {
                SensorCount = messages.Where(u => u.ID == this.SelectSensor.CanID).Count();
                if (SensorCount > 0)
                {
                    this.EnbaledChangeSensor = false;
                    ResetEvent.Set();
                }
            }

            foreach (var item in messages)
            {
                if (item.ID == 0x410 )
                {
                    FirmWareVersion = $"V{item.DATA[1]}.{item.DATA[3]}.{item.DATA[5]}";
                    return;
                }
                if (item.ID != bus.UdsRx) continue;
                if ((item.DATA[0] & 0x10) == 0x10)
                {
                    length = ((item.DATA[0] & 0x0F) << 8) + item.DATA[1];
                    var data = new byte[8];
                    data[0] = 0x30;
                    if (service.Send(Cfg, bus.UdsTx, data))
                    {
                        Console.WriteLine("-------------");
                    }
                }
                else if (item.DATA[0] < 8)
                {
                    Buffer.BlockCopy(item.DATA, 0, recsp, 0, 8);
                    ResetEvent.Set();
                }
                else if (item.DATA[0] >= 0x20)
                {
                    flowframe.AddRange(item.DATA.Skip(1).ToList());
                    if (flowframe.Count == length)
                    {
                        ResetEvent.Set();
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            FirmWareVersion = Encoding.ASCII.GetString(flowframe.ToArray(), 0, length).Trim();
                            flowframe.Clear();
                            length = 0;
                        }));
                    }
                }
            }
        }
        private List<byte> flowframe = new List<byte>();
        private int length = 0;
        private void Service_CanDataSent(ICanChannelService arg1, List<RXCanMessage> messages)
        {
        }
        private bool _IsSuccess = false;

        public bool IsSuccess
        {
            get { return _IsSuccess; }
            set { SetProperty(ref _IsSuccess, value); }
        }

        public BrigadeConfigBus BusConfig => bus;
    }
}
