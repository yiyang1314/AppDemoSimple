using Microsoft.Win32;
using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.Services;
using Rxbit.Brigade.EcuServices.Utils;
using Rxbit.Brigade.EcuServices.ZflashItems;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class UpgradeFirmWareWinModel : ViewModelBase
    {
        private readonly CanCommService service;
        private readonly ConcurrentQueue<string> FirmWareLogs = new ConcurrentQueue<string>();
        private CancellationTokenSource CTS;
        private bool mIsRunning = false;
        public UpgradeFirmWareWinModel()
        {
            SelectedCommand = new AsyncRelayCommand(UpdateFireWare);
            UpdataFlashCommand = new AsyncRelayCommand(StartSimpleEcu);
            CloseCommand = new RelayCommand<Window>(CloseWindows);
            LoadedCommand = new AsyncRelayCommand(Clear);
            service = ViewModelLocator.GetService<CanCommService>();
            UdsV1Service = new UdsService(service, 0);
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
            string appHexh = dialog.FileName;
            cfg.AppPath.AppHexDefaultPath = Path.GetDirectoryName(appHexh);
            var ret = await UpgradeApp(appHexh);
        }

        private async Task<bool> UpgradeApp(string hexFile)
        {
            SimpleEcuTool.DirsKey_DataComed = RadarConstaint.GetInfo;
            var list = SimpleEcuTool.ParseHexPackage(EcuRadar.Brigade_Radar, hexFile, out string msg);
            if (!string.IsNullOrEmpty(msg) || list.Count == 0)
            {
                this.ErrorMsg = msg;
                return false;
            }
            await this.Clear();
            ZFlashItems = new ObservableCollection<ZFlashItem>(list);
            TotalFlows = ZFlashItems.Count;
            return true;
        }

        private async Task StartSimpleEcu()
        {
            if (this.UdsV1Service.FlowProgress != null)
            {
                this.ErrorMsg = "Flashing program in progress...";
                return;
            }

            var bus = ViewModelLocator.GetService<BrigadeConfigBus>();
            var Cfg = ViewModelLocator.GetService<CanDeviceViewModel>().Config;
            UdsV1Service.DirsKey_DataComed = RadarConstaint.GetInfo;
            UdsV1Service.channelConfig = Cfg;
            UdsV1Service.SourceId = bus.UdsTx;
            UdsV1Service.TargetId = bus.UdsRx;
            this.UdsV1Service.ZflashDir = "";
            this.ErrorMsg = "Starting flashing...";
            this.UdsV1Service.FlowProgress -= ReceviFlow;
            this.UdsV1Service.FlowProgress += ReceviFlow;
            this.TotalFlows = ZFlashItems.Count;
            this.FlashDriverStepId = ZFlashItems.Where(u => u.ZFlashUdsType == ZFlashUdsEnum.FileDown).First().NumID;
            var stopwatch = new Stopwatch();
            await Task.Delay(1000);
            bool IsSupport2004 = false;
            RecevieasyncLog();
            this.UdsV1Service.LoadedCanEvent(0, IsSupport2004);
            stopwatch.Restart();
            this.IsRunning = true;
            this.IsExeCateStatus = await this.UdsV1Service.StartEcu(ZFlashItems.ToList(), ReceiveDownBlock);
            stopwatch.Stop();
            this.TestStatus = true;
            this.UdsV1Service.FlowProgress -= ReceviFlow;
            this.UdsV1Service.FlowProgress = null;
            await UdsV1Service.StopBootloader();
            this.UdsV1Service.UnLoadedCanEvent();
            CTS?.Cancel();
            if (this.IsExeCateStatus)
            {
                this.ErrorMsg = $"App Upgrade Elapsed Time: {stopwatch.ElapsedMilliseconds / 60000}min {stopwatch.ElapsedMilliseconds / 1000 % 60}s {stopwatch.ElapsedMilliseconds % 1000}ms";
            }
            this.IsRunning = false;
        }

        private void RecevieasyncLog()
        {
            if (CTS != null) return;
            CTS = new CancellationTokenSource();
            Task.Run(() => {
                while (!CTS.IsCancellationRequested)
                {
                    if (FirmWareLogs.IsEmpty)
                    {
                        Task.Delay(1000);
                    }
                    else
                    {
                        while (FirmWareLogs.TryDequeue(out string msg))
                        {
                            LogHelper.Error(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff\t") + msg);
                        }
                    }

                }
            }, CTS.Token);
        }

        private async Task Clear()
        {
            this.BlockIndex = 0;
            this.FileBlocks = 20;
            this.FlowIndex = 0;
            this.TotalFlows = 0;
            this.FlashBarProgress = 0;
            this.AppBarProgress = 0;
            this.TestStatus = false;
            this.UdsCode = "";
            this.TestStatus = false;
            await Task.Delay(100);     
        }


        private async Task StartUpdateFireWare()
        {
            UdsServices.NotifyMsg += ReceviMsg;
            UdsServices.FlowProgress += ReceviFlow;
            try
            {
                UdsServices.RegisterBoot();
                var ret = await UdsServices.StartEcu(this.ZFlashItems.ToList(), ReceiveDownBlock);
                if (!ret)
                {
                    this.UdsCode = BitConverter.ToString(UdsServices.Respones).Replace("-", " ");
                    this.ErrorMsg = UdsServices.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            UdsServices.NotifyMsg -= ReceviMsg;
            UdsServices.FlowProgress -= ReceviFlow;
            UdsServices.DisposibleBoot();

        }
        private void CloseWindows(Window window)
        {
            window?.Close();
        }

        private async Task UpdataFlash()
        {
            UdsServices.NotifyMsg += ReceviMsg;
            UdsServices.FlowProgress += ReceviFlow;
            try
            {
                UdsServices.RegisterBoot();
                var ret = await UdsServices.StartEcu(this.ZFlashItems.ToList(), ReceiveDownBlock);
                if (!ret)
                {
                    this.UdsCode = BitConverter.ToString(UdsServices.Respones).Replace("-", " ");
                    this.ErrorMsg = UdsServices.ErrorMsg;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            UdsServices.NotifyMsg -= ReceviMsg;
            UdsServices.FlowProgress -= ReceviFlow;
            UdsServices.DisposibleBoot();
        }

        private void ReceiveDownBlock(int NumId, string desc, int blocks, int progress)
        {
            if (NumId == 0)
            {
                this.FileBlocks = blocks;
                return;
            }
            if (this.FlashDriverStepId == NumId)
            {
                this.FlashBarProgress = progress;
            }
            else
            {
                this.AppBarProgress = progress;
            }
            this.BlockIndex = blocks;
            this.ErrorMsg = NumId + "." +desc;
        }

        private void ReceviFlow(int NumId, string title, byte[] rsp, string errormsg)
        {
            if (NumId == -1)
            {
                if (rsp != null)
                {
                    this.UdsCode = BitConverter.ToString(rsp).Replace("-", " ");
                    this.ErrorMsg = rsp[0] == 0 ?"this UDS Service Request is No sesponse": errormsg;
                }
                else
                    this.ErrorMsg = errormsg;
            }
            else
            {
                this.FlowIndex = NumId;
                this.ErrorMsg = NumId + "." + title + "," + errormsg;
            }
        }

        private void ReceviMsg(EcuMessage ecuMessage, string timestemp, byte[] data, bool IsTx, string desc)
        {
            this.ErrorMsg = desc;
            if (CTS != null && !CTS.IsCancellationRequested)
            {
                FirmWareLogs.Enqueue(desc);
            }
        }

        public RxBitUdsServices UdsServices { get; set; }

        public UdsService UdsV1Service { get; set; }
        public ObservableCollection<ZFlashItem> ZFlashItems { get; set; }

        public ICommand UpdataFlashCommand { get; set; }

        public AsyncRelayCommand LoadedCommand { get; set; }
        public AsyncRelayCommand SelectedCommand { get; set; }
        public RelayCommand<Window> CloseCommand { get; set; }

        public bool IsExeCateStatus
        {
            get { return _CateStatus; }
            set { Set(ref _CateStatus, value); }
        }
        private bool _CateStatus = false;

        public bool IsRunning
        {
            get { return !_IsRunning; }
            set { Set(ref _IsRunning, value); }
        }
        private bool _IsRunning = false;

        public double FlashBarProgress
        {
            get { return flashBarProgress; }
            set { Set(ref flashBarProgress, value); }
        }
        private double flashBarProgress = 0;

        public int TotalFlows
        {
            get { return _TotalFlows; }
            set { Set(ref _TotalFlows, value); }
        }
        private int _TotalFlows = 0;

        /// <summary>
        /// Flash Hex
        /// </summary>
        public int FlashDriverStepId
        {
            get { return _FlashDriverStepId; }
            set { Set(ref _FlashDriverStepId, value); }
        }
        private int _FlashDriverStepId = 0;
        /// <summary>
        /// 流程块
        /// </summary>
        public int FlowIndex
        {
            get { return _FlowIndex; }
            set { Set(ref _FlowIndex, value); }
        }
        private int _FlowIndex = 0;

        /// <summary>
        /// 文件总块数
        /// </summary>
        public int FileBlocks
        {
            get { return _FileBlocks; }
            set { Set(ref _FileBlocks, value); }
        }
        private int _FileBlocks = 20;

        /// <summary>
        /// 块索引
        /// </summary>
        public int BlockIndex
        {
            get { return _BlockIndex; }
            set { Set(ref _BlockIndex, value); }
        }
        private int _BlockIndex = 0;

        /// <summary>
        /// 流程块
        /// </summary>
        public double AppBarProgress
        {
            get { return appProgress; }
            set { Set(ref appProgress, value); }
        }
        private double appProgress = 0;

        public string ErrorMsg
        {
            get { return errorMsg; }
            set { Set(ref errorMsg, value); }
        }
        private string errorMsg = string.Empty;


        public string FlowTitle
        {
            get { return _FlowTitle; }
            set { Set(ref _FlowTitle, value); }
        }
        private string _FlowTitle = string.Empty;

        public string UdsCode
        {
            get { return _UdsCode; }
            set { Set(ref _UdsCode, value); }
        }
        private string _UdsCode = string.Empty;

        public bool TestStatus
        {
            get { return mTestStatus; }
            set { Set(ref mTestStatus, value); }
        }
        private bool mTestStatus;
    }
}
