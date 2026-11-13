using Rxbit.Brigade.CanDevices.IServices;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase,IDisposable
    {
        private readonly List<IRecordingService> recordingServices = new List<IRecordingService>();
        private bool IsInitVM = false;
        private bool _isSensorConnected = true;
        private string _statusText;
        private string _statusColor;
        private Brush _statusColorBrush;
        private string _sensorTypeInfo;
        private List<MenuItemModel> _mainMenuItems;
        private Window _mainWindow;
        private readonly Dictionary<Type, Window> _openWindows = new Dictionary<Type, Window>();
        private readonly BrigadeConfigBus configBus;
        public SensorConfigurationViewModel SensorVM { get; set; }

        public uint CurrSensorId { get; set; }
        public Brush StatusColorBrush
        {
            get => _statusColorBrush;
            private set
            {
                SetProperty(ref _statusColorBrush, value);
            }
        }

        public bool IsSensorConnected
        {
            get => _isSensorConnected;
            set
            {
                SetProperty(ref _isSensorConnected, value);
                UpdateMenuEnabledStates();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                SetProperty(ref _statusText, value);
            }
        }

        public string StatusColor
        {
            get => _statusColor;
            set
            {
                SetProperty(ref _statusColor, value);
            }
        }

        public string SensorTypeInfo
        {
            get => _sensorTypeInfo;
            set
            {
                SetProperty(ref _sensorTypeInfo, value);
            }
        }
        public List<MenuItemModel> MainMenuItems
        {
            get => _mainMenuItems;
            set
            {
                _mainMenuItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand MenuItemClickCommand { get; }
        public ICommand OpenSensorConfigCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand LoadedCommand { get; }

        private readonly DispatcherTimer _flushTimer;
        public MainWindowViewModel(Window mainWindow = null)
        {
            _mainWindow = mainWindow ?? Application.Current.MainWindow;
            InitializeSensors();
            InitializeMenuItems();
            SensorVM = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            SensorVM.SensorChangeEvevt -= SensorVM_PropertyChanged;
            SensorVM.SensorChangeEvevt += SensorVM_PropertyChanged;
            StatusText = "No sensor selected — choose a Sensor ID to begin receiving data";
            StatusColor = "#ff4444";
            StatusColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff4444"));
            IsSensorConnected = false;
            MenuItemClickCommand = new RelayCommand<MenuItemModel>(ExecuteMenuItemClick);
            ExitCommand = new RelayCommand(ExitApplication);
            LoadedCommand = new RelayCommand(LoadedConfig);
            OpenSensorConfigCommand = new  RelayCommand(OpenConfigSensor);
            _flushTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _flushTimer.Tick += FlushBufferToUi;
            _flushTimer?.Start();
        }

        private void FlushBufferToUi(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.CurrentTime = DateTime.Now;
            });
        }
        private void LoadedConfig()
        {
            var sensor = ViewModelLocator.GetService<SensorConfigurationViewModel>();
            if (CurrSensorId != 0 && CurrSensorId == sensor.CurrSensor.CanID)
            {
                return;
            }
            this.IsSensorConnected = sensor.CurrSensor.IsVaild;
            OnSensorSelected(sensor.CurrSensor);
            IsInitVM = true;
        }

        private void SensorVM_PropertyChanged(SensorInfo info)
        {
            if (info == null) return;
            if (CurrSensorId != 0 && CurrSensorId == info.CanID)
            {
                return;
            }
            var sensor = info;
            StatusText = $"selected SensorID 0x{info.CanID:x} {sensor.Type}";
            SensorTypeInfo = $"0x{info.CanID:x} {sensor.Type}";
            this.IsSensorConnected = info.IsVaild;
            OnSensorSelected(info);
            CurrSensorId = info.CanID;
        }
        private void OpenConfigSensor()
        {
            SensorConfigurationView view =new SensorConfigurationView();
            view.Owner = Application.Current.MainWindow;
            view.ShowDialog();
        }

        private void InitializeMenuItems()
        {
            MainMenuItems = new List<MenuItemModel>
            {
                // File Menu
                new MenuItemModel
                {
                    Header = "Files",
                    Children = new List<MenuItemModel>
                    {
                        //new MenuItemModel
                        //{
                        //    Header = "Save Config",
                        //    Shortcut = "Ctrl+S",
                        //    Command = ViewModelLocator.GetService<SensorConfigurationViewModel>().SaveCommand,
                        //},
                        //new MenuItemModel { IsSeparator = true },
                        new MenuItemModel
                        {
                            Header = "Open Software WorkDir",
                            Shortcut = "Ctrl+O",
                            ClickAction =(item)=>OpenOptionFolder(0),
                        },
                        //new MenuItemModel
                        //{
                        //    Header = "Open FlashDrivar",
                        //    Shortcut = "Ctrl+F",
                        //    ClickAction =(item)=>OpenOptionFolder(1),
                        //},
                        new MenuItemModel
                        {
                            Header = "Open Recording Path",
                            Shortcut = "Ctrl+R",
                            ClickAction =(item)=>OpenOptionFolder(2),
                        },
                        new MenuItemModel
                        {
                            Header = "Open Logs Path",
                            Shortcut = "Ctrl+L",
                            ClickAction =(item)=>OpenOptionFolder(4),
                        }
                        //new MenuItemModel
                        //{
                        //    Header = "Open Setting Path",
                        //    ClickAction =(item)=>OpenOptionFolder(3),
                        //},
                        //new MenuItemModel { IsSeparator = true },
                        //new MenuItemModel
                        //{
                        //    Header = "_Exit",
                        //    Command = ExitCommand,
                        //    Shortcut = "Ctrl+Q",
                        //}
                    }
                },
                new MenuItemModel
                {
                    Header = "Devices",
                    Children = new List<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Header = "CAN Device Configuration",
                            WindowType = typeof(CanDeviceView),
                            Shortcut = "Ctrl+O"
                        },
                        new MenuItemModel
                        {
                            WindowType = typeof(SettingSensor),
                            Header = "Path Configuration",
                            Shortcut = "Ctrl+Q"
                        },
                        new MenuItemModel
                        {
                            WindowType = typeof(SensorConfigurationView),
                            Header = "Sensor Configuration",
                            Shortcut = "Ctrl+Q"
                        }
                    }
                },
                // View Menu
                new MenuItemModel
                {
                    Header = "View",
                    Children = new List<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Header = "Detection & Sensor Info",
                            MenuType = MenuType.View,
                            WindowType = typeof(DetectionSensorInfoView),
                            IsCheckable = true,
                            IsEnabled = false
                        },
                        new MenuItemModel
                        {
                            Header = "Network Summary",
                            MenuType = MenuType.View,
                            WindowType = typeof(NetworkSummaryView),
                            IsCheckable = true,
                            IsEnabled = false
                        },
                        //new MenuItemModel
                        //{
                        //    Header = "Replay Mode",
                        //    MenuType = MenuType.View,
                        //    WindowType = typeof(CanPlaybackView),
                        //    IsCheckable = true,
                        //    IsEnabled = false
                        //},
                        new MenuItemModel
                        {
                            Header = "CAN Live Data",
                            MenuType = MenuType.View,
                            WindowType = typeof(CanLiveDataView),
                            IsCheckable = false,
                            IsEnabled = false
                        },
                        new MenuItemModel { IsSeparator = true },
                        new MenuItemModel
                        {
                            Header = "Screen Recording",
                            MenuType = MenuType.View,
                            WindowType = typeof(ScreenRecorderView),
                            IsCheckable = false,
                            IsEnabled = false
                        },
                        new MenuItemModel
                        {
                            Header = "USB Camera Recording",
                            MenuType = MenuType.View,
                            WindowType = typeof(CameraRecorderView),
                            IsCheckable = false,
                            IsEnabled = false
                        },
                        new MenuItemModel
                        {
                            Header = "Video Playback",
                            MenuType = MenuType.View,
                            WindowType = typeof(VideoPlaybackView),
                            IsCheckable = false,
                            IsEnabled = false
                        },
                    }
                },
                // Tools Menu
                new MenuItemModel
                {
                    Header = "Tools",
                    Children = new List<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Header = "Update FirmWare",
                            MenuType = MenuType.Advinces,
                            WindowType = typeof(UpgradeFirmWareWin)
                        },
                        new MenuItemModel
                        {
                            Header = "CRC Tools",
                            MenuType = MenuType.Advinces,
                            WindowType = typeof(WinHelper)
                        },
                        new MenuItemModel
                        {
                            Header = "Asc to Brigade",
                            MenuType = MenuType.Advinces,
                            WindowType = typeof(Asc2BrigadeWin)
                        },
                        new MenuItemModel
                        {
                            Header = "Blf2Asc Tools",
                            MenuType = MenuType.Advinces,
                            WindowType = typeof(Blf2AscWindow)
                        },
                    }
                },
                // Help Menu
                new MenuItemModel
                {
                    Header = "Help",
                    Children = new List<MenuItemModel>
                    {
                        new MenuItemModel
                        {
                            Header = "About",
                            MenuType = MenuType.Aoubt,
                            WindowType = typeof(AboutView)
                        },
                        new MenuItemModel
                        {
                            Header = "Docs",
                            MenuType = MenuType.Aoubt,
                            WindowType = typeof(AboutView)
                        },
                    }
                }
            };
        }

        private void OpenOptionFolder(int type)
        {
            string dirPath = string.Empty;
            switch (type)
            {
                case 0:
                    dirPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    break;
                case 1:
                    dirPath = RadarConstaint.FlashPath;
                    break;
                case 2:
                    dirPath = RadarConstaint.RecondingPath;
                    break;
                case 3:
                    dirPath = RadarConstaint.SettingFile;
                    break;
                case 4:
                    dirPath = RadarConstaint.LogPath;
                    break;
                default:
                    dirPath = RadarConstaint.AppWorkPath;
                    break;
            }
            try
            {
                // 构建explorer命令：/select, 后跟文件的完整路径
                string arguments = $"/select, \"{dirPath}\"";
                // 启动资源管理器进程
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = arguments,
                    UseShellExecute = true // 必须为true才能正确启动资源管理器
                });
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex);
            }
        }

        private void UpdateMenuEnabledStates()
        {
            if (MainMenuItems == null) return;
            foreach (var menu in MainMenuItems)
            {
                if (menu.Header == "View")
                {
                    foreach (var child in menu.Children)
                    {
                        if (child.IsSeparator) continue;
                        // 数据窗口不依赖传感器是否在线，始终允许打开
                        child.IsEnabled = true;
                    }
                }
            }
        }

        private void ExecuteMenuItemClick(MenuItemModel item)
        {
            if (item == null) return;
            LogHelper.Info($"MenuItemClick: {item.Header} (WindowType={(item.WindowType == null ? "null" : item.WindowType.Name)})");

            // 优先执行命令
            if (item.Command != null)
            {
                item.Command.Execute(null);
                return;
            }

            // 然后尝试通过反射创建窗口
            if (item.WindowType != null)
            {
                if (item.IsCheckable)
                {
                    // 勾选型菜单项与窗口打开状态保持同步：已打开则关闭，未打开则打开
                    if (_openWindows.TryGetValue(item.WindowType, out Window openedWindow))
                    {
                        openedWindow.Close();
                        item.IsChecked = false;
                        return;
                    }

                    item.IsChecked = OpenWindowByType(item.WindowType, item);
                    return;
                }

                OpenWindowByType(item.WindowType, item);
                return;
            }

            // 最后执行点击动作
            item.ClickAction?.Invoke(item);
        }

        private bool OpenWindowByType(Type windowType, MenuItemModel item = null)
        {
            // 检查窗口是否已经打开
            if (_openWindows.TryGetValue(windowType, out Window existingWindow))
            {
                if (existingWindow.IsVisible)
                {
                    existingWindow.Activate();
                    return true;
                }
                _openWindows.Remove(windowType);
            }

            try
            {
                Window newWindow;

                // 特殊处理 CoordinateView，它是 UserControl，需要包装在 Window 中
                if (windowType == typeof(CoordinateView))
                {
                    newWindow = new Window
                    {
                        Title = "Coordinate System Viewer",
                        Width = 900,
                        Height = 700,
                        WindowStyle = WindowStyle.SingleBorderWindow,
                        ResizeMode = ResizeMode.CanResize,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1a1a2e"))
                    };
                    newWindow.Content = Activator.CreateInstance(windowType);
                }
                else
                {
                    // 使用反射创建窗口实例
                    newWindow = (Window)Activator.CreateInstance(windowType);
                }

                // 设置窗口位置在主窗口中心
                CenterWindowOnMainWindow(newWindow);

                // 添加到已打开窗口字典
                _openWindows[windowType] = newWindow;

                // 窗口关闭时从字典中移除，并同步菜单勾选状态
                newWindow.Closed += (s, e) =>
                {
                    _openWindows.Remove(windowType);
                    if (item != null && item.IsCheckable)
                    {
                        item.IsChecked = false;
                    }
                };

                // 显示窗口
                newWindow.Show();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Failed to open window {windowType.Name}: {ex.Message}", ex);
                MessageBox.Show($"Failed to open window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void CenterWindowOnMainWindow(Window window)
        {
            if (_mainWindow == null)
            {
                _mainWindow = Application.Current.MainWindow;
            }

            if (_mainWindow != null)
            {
                window.Owner = _mainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private void InitializeSensors()
        {
           
        }

        private void OnSensorSelected(SensorInfo info)
        {
            if (info.Type == GenType.Gen1Type || info.Type == GenType.Gen2Type)
            {
                StatusText = $"selected sensor 0x{info.CanID:x}";
                StatusColor = "#00ff88";
                StatusColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00ff88"));
                SensorTypeInfo = $"Gen2Type {((uint)info.Type)} | 50 ms cycle";
            }
            else
            {
                StatusText = "No sensor selected — choose a Sensor ID to begin receiving data";
                StatusColor = "#ff4444";
                StatusColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff4444"));
                SensorTypeInfo = "";
               // CloseAllViews();
            }
        }

        private void ExitApplication()
        {
            Dispose();
            Application.Current.Shutdown();
        }

        private void CloseAllViews()
        {
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var window in _openWindows.Values.ToList())
                {
                    //window.Close();
                }
                _openWindows.Clear();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _openWindows.Clear();
            _flushTimer.Stop();
            _flushTimer.Tick -= FlushBufferToUi;
        }

        public DateTime CurrentTime 
        { 
            get { return _CurrentTime; }
            set { SetProperty(ref _CurrentTime, value); }
        }
        private DateTime _CurrentTime = DateTime.Now;
    }
}
