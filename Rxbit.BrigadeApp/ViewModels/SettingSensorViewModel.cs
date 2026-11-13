using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class SettingSensorViewModel : ViewModelBase
    {
        public RelayCommand LoadedCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand<string> SelectFileCommand { get; }
        private bool _IsEditable;
        public bool IsEditable
        {
            get => _IsEditable;
            set
            {
                _IsEditable = value;
                OnPropertyChanged(nameof(IsEditable));
            }
        }

        public string RecodingPath
        {
            get { return recodingPath; }
            set { Set(ref recodingPath, value); }
        }
        private string recodingPath = string.Empty;

        public string LogPath
        {
            get { return logPath; }
            set { Set(ref logPath, value); }
        }
        private string logPath = string.Empty;

        public string BrigadePath { get { return RadarConstaint.ConfigPath; } }
        public string PCANPath
        {
            get { return mPCANPath; }
            set { SetProperty(ref mPCANPath, value); }
        }
        private string mPCANPath;
        public string FlashPath
        {
            get { return flash; }
            set { SetProperty(ref flash, value); }
        }
        private string flash = "";
        public SettingSensorViewModel()
        {
            LoadedCommand = new RelayCommand(Loaded);
            SaveCommand = new RelayCommand(SaveConfig);
            SelectFileCommand = new RelayCommand<string>(SelectFile);
        }

        private void SelectFile(string type)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = "Select Folder";
            folderDlg.SelectedPath = type == "recoding" ? this.recodingPath : this.logPath;
            if (folderDlg.ShowDialog() == DialogResult.OK)
            {
                if (type == "recoding")
                {
                    this.RecodingPath = folderDlg.SelectedPath;
                }
                else if (type == "log")
                {
                    this.LogPath = folderDlg.SelectedPath;
                }
            }
        }

        public string ReqTx
        {
            get { return reqTx; }
            set { SetProperty(ref reqTx, value); }
        }
        private string reqTx = "794";


        public string RspRx
        {
            get { return rspRx; }
            set { SetProperty(ref rspRx, value); }
        }
        private string rspRx = "79A";

        private void SaveConfig()
        {
            RadarConstaint.RecondingPath = this.RecodingPath;
            RadarConstaint.LogPath = this.LogPath;
            var bus = ViewModelLocator.GetService<SensorConfigurationViewModel>().BusConfig;
            var pinfo = new Models.AppPathInfo()
            {
                RecodingPath = this.RecodingPath,
                AppHexDefaultPath = RadarConstaint.FlashPath,
                LogsPath = this.LogPath,
                SettingPath = RadarConstaint.SettingFile
            };

            if (!uint.TryParse(this.ReqTx, System.Globalization.NumberStyles.HexNumber, null, out uint UdsTx))
            {
                MessageBox.Show("The current value is not a valid hexadecimal!");
                return;
            }
            if (!uint.TryParse(this.RspRx, System.Globalization.NumberStyles.HexNumber, null, out uint UdsRx))
            {
                MessageBox.Show("The current value is not a valid hexadecimal!");
                return;
            }
            bus.UdsTx = UdsTx;
            bus.UdsRx = UdsRx;
            bus.AppPath = pinfo;
            ConfigHelper.SaveConfig(bus, RadarConstaint.SettingFile);
        }

        private void Loaded()
        {
            this.RecodingPath = RadarConstaint.RecondingPath;
            this.LogPath = RadarConstaint.LogPath;
            this.FlashPath = RadarConstaint.FlashPath;
            var bus = ViewModelLocator.GetService<SensorConfigurationViewModel>().BusConfig;
            this.ReqTx = bus.UdsTx.ToString("x");
            this.RspRx = bus.UdsRx.ToString("x");
            this.PCANPath = RadarConstaint.PCANPath;
            this.IsEditable = true;
        }
    }
}
