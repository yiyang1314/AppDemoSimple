using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    // Monitor navigation item
    public class MonitorNavItem : ObservableObject
    {
        private string _icon;
        private string _label;
        private bool _isActive;
        private string _target;

        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public string Label { get => _label; set => SetProperty(ref _label, value); }
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
        public string Target { get => _target; set => SetProperty(ref _target, value); }
    }

    // Performance monitor item (CPU, Memory, Disks, Wi-Fi, GPUs)
    public class MonitorItem : ObservableObject
    {
        private string _name;
        private string _subLabel;
        private double _value;
        private string _valueUnit;
        private double _percentage;
        private string _icon;
        private Color _color;
        private LineSeries<double> _miniSeries;
        private LineSeries<double> _detailSeries;
        private ObservableCollection<double> _sparkValues;
        private ObservableCollection<double> _detailValues;
        private string _detailTitle;
        private string _detailSubtitle;
        private bool _isSelected;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string SubLabel { get => _subLabel; set => SetProperty(ref _subLabel, value); }
        public double Value { get => _value; set => SetProperty(ref _value, value); }
        public string ValueUnit { get => _valueUnit; set => SetProperty(ref _valueUnit, value); }
        public double Percentage { get => _percentage; set => SetProperty(ref _percentage, value); }
        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public Color Color { get => _color; set => SetProperty(ref _color, value); }
        public LineSeries<double> MiniSeries { get => _miniSeries; set => SetProperty(ref _miniSeries, value); }
        public LineSeries<double> DetailSeries { get => _detailSeries; set => SetProperty(ref _detailSeries, value); }
        public ObservableCollection<double> SparkValues { get => _sparkValues; set => SetProperty(ref _sparkValues, value); }
        public ObservableCollection<double> DetailValues { get => _detailValues; set => SetProperty(ref _detailValues, value); }
        public bool IsSelected { get => _isSelected; set { SetProperty(ref _isSelected, value); } }
        public void NotifySparkChanged() { OnPropertyChanged(nameof(SparkPoints)); }
        public void NotifyDetailChanged() { OnPropertyChanged(nameof(DetailTitle)); OnPropertyChanged(nameof(DetailSubtitle)); }
        public PointCollection SparkPoints
        {
            get
            {
                if (SparkValues == null || SparkValues.Count < 2) return null;
                var points = new PointCollection();
                double min = SparkValues.Min();
                double max = SparkValues.Max();
                double range = max - min;
                if (range < 0.001) range = 1;
                double width = 60;
                for (int i = 0; i < SparkValues.Count; i++)
                {
                    double x = (double)i / (SparkValues.Count - 1) * width;
                    double y = 30 - ((SparkValues[i] - min) / range) * 28;
                    points.Add(new Point(x, y));
                }
                return points;
            }
        }
        public string DetailTitle { get => _detailTitle; set => SetProperty(ref _detailTitle, value); }
        public string DetailSubtitle { get => _detailSubtitle; set => SetProperty(ref _detailSubtitle, value); }

        // Detail info properties (for bottom info panel)
        public string InfoLine1 { get; set; }
        public string InfoLine2 { get; set; }
        public string InfoLine3 { get; set; }
        public string InfoLine4 { get; set; }
        public string InfoLabel1 { get; set; }
        public string InfoLabel2 { get; set; }
        public string InfoLabel3 { get; set; }
        public string InfoLabel4 { get; set; }
        public string InfoLabel5 { get; set; }
        public string InfoLabel6 { get; set; }
        public string InfoValue1 { get; set; }
        public string InfoValue2 { get; set; }
        public string InfoValue3 { get; set; }
        public string InfoValue4 { get; set; }
        public string InfoValue5 { get; set; }
        public string InfoValue6 { get; set; }
    }

    // Process item for Process page
    public class ProcessItem : ObservableObject
    {
        private string _name;
        private string _icon;
        private double _cpu;
        private string _cpuText;
        private double _memory;
        private string _memoryText;
        private double _disk;
        private string _diskText;
        private double _network;
        private string _networkText;
        private string _status;
        private bool _isSuspended;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Icon { get => _icon; set => SetProperty(ref _icon, value); }
        public double CPU { get => _cpu; set => SetProperty(ref _cpu, value); }
        public string CpuText { get => _cpuText; set => SetProperty(ref _cpuText, value); }
        public double Memory { get => _memory; set => SetProperty(ref _memory, value); }
        public string MemoryText { get => _memoryText; set => SetProperty(ref _memoryText, value); }
        public double Disk { get => _disk; set => SetProperty(ref _disk, value); }
        public string DiskText { get => _diskText; set => SetProperty(ref _diskText, value); }
        public double Network { get => _network; set => SetProperty(ref _network, value); }
        public string NetworkText { get => _networkText; set => SetProperty(ref _networkText, value); }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public bool IsSuspended { get => _isSuspended; set => SetProperty(ref _isSuspended, value); }
    }

    // Monitor Shell ViewModel
    public class MonitorShellViewModel : ObservableObject
    {
        private ObservableCollection<MonitorNavItem> _navItems;
        private string _current;
        private string _searchText;

        public ObservableCollection<MonitorNavItem> NavItems { get => _navItems; set => SetProperty(ref _navItems, value); }
        public string Current { get => _current; set => SetProperty(ref _current, value); }
        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }

        public MonitorShellViewModel()
        {
            NavItems = new ObservableCollection<MonitorNavItem>
            {
                new MonitorNavItem { Icon = "◨", Label = "进程", Target = "process" },
                new MonitorNavItem { Icon = "📊", Label = "性能", Target = "performance", IsActive = true },
                new MonitorNavItem { Icon = "📜", Label = "应用历史记录", Target = "history" },
                new MonitorNavItem { Icon = "🚀", Label = "启动应用", Target = "startup" },
                new MonitorNavItem { Icon = "👤", Label = "用户", Target = "users" },
                new MonitorNavItem { Icon = "🔍", Label = "详细信息", Target = "details" },
                new MonitorNavItem { Icon = "⚙", Label = "服务", Target = "services" },
            };
            Current = "performance";
        }

        public void Navigate(string target)
        {
            Current = target;
            foreach (var n in NavItems)
                n.IsActive = n.Target == target;
        }
    }

    // Monitor Performance ViewModel
    public class MonitorPerformanceViewModel : ObservableObject
    {
        private ObservableCollection<MonitorItem> _monitorItems;
        private MonitorItem _selectedItem;
        private string _pageTitle;
        private string _pageSubtitle;

        public ObservableCollection<MonitorItem> MonitorItems { get => _monitorItems; set => SetProperty(ref _monitorItems, value); }
        public MonitorItem SelectedItem { get => _selectedItem; set { SetProperty(ref _selectedItem, value); OnPropertyChanged(nameof(SelectedItemDetail)); } }
        public string PageTitle { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }
        public string PageSubtitle { get => _pageSubtitle; set => SetProperty(ref _pageSubtitle, value); }

        // The detail view model for the selected item
        public MonitorItem SelectedItemDetail => _selectedItem;

        public MonitorPerformanceViewModel()
        {
            PageTitle = "性能";
            MonitorItems = new ObservableCollection<MonitorItem>
            {
                new MonitorItem { Name = "CPU", SubLabel = "12th Gen Intel(R) Core(TM) i7-12650H", Value = 6, ValueUnit = "%", Percentage = 6, Icon = "🖥", Color = Color.FromRgb(24, 144, 255), DetailTitle = "CPU", DetailSubtitle = "12th Gen Intel(R) Core(TM) i7-12650H" },
                new MonitorItem { Name = "内存", SubLabel = "23.5/31.7 GB (74%)", Value = 74, ValueUnit = "%", Percentage = 74, Icon = "💾", Color = Color.FromRgb(82, 196, 26), DetailTitle = "内存", DetailSubtitle = "DDR4" },
                new MonitorItem { Name = "磁盘 0 (C: D: E:)", SubLabel = "SSD (NVMe)", Value = 3, ValueUnit = "%", Percentage = 3, Icon = "💽", Color = Color.FromRgb(250, 173, 20), DetailTitle = "磁盘 0 (C: D: E:)", DetailSubtitle = "SSD (NVMe)" },
                new MonitorItem { Name = "磁盘 1 (H:)", SubLabel = "USB (USB)", Value = 0, ValueUnit = "%", Percentage = 0, Icon = "💿", Color = Color.FromRgb(114, 46, 209), DetailTitle = "磁盘 1 (H:)", DetailSubtitle = "TOSHIBA EXTERNAL_USB USB Device" },
                new MonitorItem { Name = "磁盘 2 (F:)", SubLabel = "可移动 (USB)", Value = 0, ValueUnit = "%", Percentage = 0, Icon = "💿", Color = Color.FromRgb(24, 144, 255), DetailTitle = "磁盘 2 (F:)", DetailSubtitle = "可移动 (USB)" },
                new MonitorItem { Name = "磁盘 3 (I:)", SubLabel = "可移动 (USB)", Value = 0, ValueUnit = "%", Percentage = 0, Icon = "💿", Color = Color.FromRgb(24, 144, 255), DetailTitle = "磁盘 3 (I:)", DetailSubtitle = "可移动 (USB)" },
                new MonitorItem { Name = "Wi-Fi", SubLabel = "WLAN", Value = 5, ValueUnit = "%", Percentage = 5, Icon = "📶", Color = Color.FromRgb(245, 34, 45), DetailTitle = "Wi-Fi", DetailSubtitle = "Intel(R) Wi-Fi 6E AX211 160MHz" },
                new MonitorItem { Name = "GPU 0", SubLabel = "Intel(R) UHD Graphics", Value = 4, ValueUnit = "%", Percentage = 4, Icon = "🎮", Color = Color.FromRgb(19, 194, 194), DetailTitle = "GPU 0", DetailSubtitle = "Intel(R) UHD Graphics" },
                new MonitorItem { Name = "GPU 1", SubLabel = "NVIDIA GeForce R...", Value = 0, ValueUnit = "%", Percentage = 0, Icon = "🎮", Color = Color.FromRgb(250, 173, 20), DetailTitle = "GPU 1", DetailSubtitle = "NVIDIA GeForce RTX" },
            };

            // Set first item as selected
            SelectedItem = MonitorItems[0];
            MonitorItems[0].IsSelected = true;

            // Create initial data series for each monitor item
            var random = new Random();
            foreach (var item in MonitorItems)
            {
                item.SparkValues = new ObservableCollection<double>();
                item.MiniSeries = new LineSeries<double> { Values = item.SparkValues, Name = item.Name };
                item.DetailValues = new ObservableCollection<double>();
                item.DetailSeries = new LineSeries<double> { Values = item.DetailValues, Name = item.Name };

                double baseValue = item.Percentage;
                for (int i = 0; i < 60; i++)
                {
                    double val = baseValue + (random.NextDouble() - 0.5) * baseValue * 0.6;
                    if (val < 0) val = 0;
                    item.SparkValues.Add(val);
                    item.DetailValues.Add(val);
                }
            }

            // Set detail info for selected item (CPU)
            UpdateDetailInfo(SelectedItem);
        }

        public void SelectItem(MonitorItem item)
        {
            foreach (var m in MonitorItems)
                m.IsSelected = (m == item);
            SelectedItem = item;
            UpdateDetailInfo(item);
        }

        private void UpdateDetailInfo(MonitorItem item)
        {
            if (item == null) return;

            // Set bottom panel info based on item type
            if (item.Name.Contains("CPU"))
            {
                item.InfoLabel1 = "利用率"; item.InfoValue1 = "6%";
                item.InfoLabel2 = "速度"; item.InfoValue2 = "1.33 GHz";
                item.InfoLabel3 = "基准速度"; item.InfoValue3 = "2.30 GHz";
                item.InfoLabel4 = "插槽"; item.InfoValue4 = "1";
                item.InfoLabel5 = "内核"; item.InfoValue5 = "10";
                item.InfoLabel6 = "逻辑处理器"; item.InfoValue6 = "16";
                item.InfoLine1 = "进程"; item.InfoLine2 = "线程"; item.InfoLine3 = "句柄"; item.InfoLine4 = "正常运行时间";
            }
            else if (item.Name.Contains("内存"))
            {
                item.InfoLabel1 = "可用"; item.InfoValue1 = "8.2 GB";
                item.InfoLabel2 = "已提交"; item.InfoValue2 = "23.5 GB";
                item.InfoLabel3 = "缓存"; item.InfoValue3 = "15.8 GB";
                item.InfoLabel4 = "分页"; item.InfoValue4 = "0";
                item.InfoLabel5 = "分页峰值"; item.InfoValue5 = "0";
                item.InfoLabel6 = ""; item.InfoValue6 = "";
                item.InfoLine1 = "速度"; item.InfoLine2 = "插槽"; item.InfoLine3 = "形状因子"; item.InfoLine4 = "类型";
            }
            else if (item.Name.Contains("磁盘") && !item.Name.Contains("Wi-Fi"))
            {
                item.InfoLabel1 = "读取速度"; item.InfoValue1 = "0 KB/秒";
                item.InfoLabel2 = "写入速度"; item.InfoValue2 = "0 KB/秒";
                item.InfoLabel3 = "响应时间"; item.InfoValue3 = "0 毫秒";
                item.InfoLabel4 = "磁盘类型"; item.InfoValue4 = item.SubLabel.Contains("SSD") ? "SSD" : "HDD";
                item.InfoLabel5 = "容量"; item.InfoValue5 = "1.8 TB";
                item.InfoLabel6 = "文件系统"; item.InfoValue6 = "NTFS";
                item.InfoLine1 = "已格式化"; item.InfoLine2 = "已压缩"; item.InfoLine3 = "磁盘"; item.InfoLine4 = "";
            }
            else if (item.Name.Contains("Wi-Fi"))
            {
                item.InfoLabel1 = "发送"; item.InfoValue1 = "112 Kbps";
                item.InfoLabel2 = "接收"; item.InfoValue2 = "624 Kbps";
                item.InfoLabel3 = "适配器名称"; item.InfoValue3 = "WLAN";
                item.InfoLabel4 = "SSID"; item.InfoValue4 = "yiyangyu";
                item.InfoLabel5 = "连接类型"; item.InfoValue5 = "802.11ax";
                item.InfoLabel6 = "IPv4 地址"; item.InfoValue6 = "192.168.100.101";
                item.InfoLine1 = "IPv6 地址"; item.InfoLine2 = "信号强度"; item.InfoLine3 = ""; item.InfoLine4 = "";
            }
            else if (item.Name.Contains("GPU"))
            {
                item.InfoLabel1 = "GPU 利用率"; item.InfoValue1 = "4%";
                item.InfoLabel2 = "专用 GPU 内存"; item.InfoValue2 = "0.9/15.8 GB";
                item.InfoLabel3 = "共享 GPU 内存"; item.InfoValue3 = "0.9/15.8 GB";
                item.InfoLabel4 = "驱动程序版本"; item.InfoValue4 = "31.01.101.586";
                item.InfoLabel5 = "驱动程序日期"; item.InfoValue5 = "2024/1/18";
                item.InfoLabel6 = "DirectX 版本"; item.InfoValue6 = "12 (FL 12.1)";
                item.InfoLine1 = "物理位置"; item.InfoLine2 = ""; item.InfoLine3 = ""; item.InfoLine4 = "";
            }
            else
            {
                item.InfoLabel1 = "利用率"; item.InfoValue1 = item.Percentage + "%";
                item.InfoLabel2 = "速度"; item.InfoValue2 = item.Value + " GHz";
                item.InfoLabel3 = ""; item.InfoValue3 = "";
                item.InfoLabel4 = ""; item.InfoValue4 = "";
                item.InfoLabel5 = ""; item.InfoValue5 = "";
                item.InfoLabel6 = ""; item.InfoValue6 = "";
                item.InfoLine1 = ""; item.InfoLine2 = ""; item.InfoLine3 = ""; item.InfoLine4 = "";
            }

            OnPropertyChanged(nameof(SelectedItemDetail));
        }

        // Update data for real-time simulation
        public void UpdateData()
        {
            var random = new Random();
            foreach (var item in MonitorItems)
            {
                // Simulate data change
                double change = (random.NextDouble() - 0.5) * 2;
                double newValue = Math.Max(0, Math.Min(100, item.Value + change));
                item.Value = Math.Round(newValue, 1);
                item.Percentage = item.Value;

                // Update data series
                if (item.DetailValues != null)
                {
                    item.DetailValues.Add(newValue);
                    if (item.DetailValues.Count > 600)
                        item.DetailValues.RemoveAt(0);
                }
                if (item.SparkValues != null)
                {
                    item.SparkValues.Add(newValue);
                    if (item.SparkValues.Count > 60)
                        item.SparkValues.RemoveAt(0);
                }

                OnPropertyChanged(nameof(SelectedItemDetail));
            }
        }
    }

    // Monitor Process ViewModel
    public class MonitorProcessViewModel : ObservableObject
    {
        private ObservableCollection<ProcessItem> _processes;
        private string _searchText;
        private ObservableCollection<ProcessItem> _filteredProcesses;

        public ObservableCollection<ProcessItem> Processes { get => _processes; set => SetProperty(ref _processes, value); }
        public string SearchText { get => _searchText; set { SetProperty(ref _searchText, value); FilterProcesses(); } }
        public ObservableCollection<ProcessItem> FilteredProcesses { get => _filteredProcesses; set => SetProperty(ref _filteredProcesses, value); }

        public MonitorProcessViewModel()
        {
            Processes = new ObservableCollection<ProcessItem>
            {
                new ProcessItem { Name = "System Settings Broker", Icon = "⚙", CPU = 0, CpuText = "0%", Memory = 90.5, MemoryText = "90.5 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "WebView2 管理器", Icon = "🌐", CPU = 0, CpuText = "0%", Memory = 22.7, MemoryText = "22.7 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "服务主机: Network Connect...", Icon = "🔌", CPU = 0.1, CpuText = "0.1%", Memory = 1.7, MemoryText = "1.7 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "服务主机 (2)", Icon = "🖥", CPU = 0, CpuText = "0%", Memory = 1.2, MemoryText = "1.2 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "服务主机: Time Broker", Icon = "⏰", CPU = 0, CpuText = "0%", Memory = 1.0, MemoryText = "1.0 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "User OOZE Broker", Icon = "📊", CPU = 0, CpuText = "0%", Memory = 1.0, MemoryText = "1.0 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "Runtime Broker", Icon = "🔷", CPU = 0, CpuText = "0%", Memory = 1.0, MemoryText = "1.0 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "蓝图管家 (HP Support Assis...", Icon = "🔧", CPU = 0, CpuText = "0%", Memory = 0.8, MemoryText = "0.8 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "Windows 安全中心", Icon = "🛡", CPU = 0, CpuText = "0%", Memory = 15.2, MemoryText = "15.2 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "Windows 资源管理器", Icon = "📁", CPU = 0.5, CpuText = "0.5%", Memory = 85.3, MemoryText = "85.3 MB", Disk = 0.1, DiskText = "0.1 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "Microsoft Visual C++ ...", Icon = "📦", CPU = 0, CpuText = "0%", Memory = 2.1, MemoryText = "2.1 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "KernelBase", Icon = "🔩", CPU = 0, CpuText = "0%", Memory = 0.5, MemoryText = "0.5 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
                new ProcessItem { Name = "Windows 终端", Icon = "⌨", CPU = 1.2, CpuText = "1.2%", Memory = 45.6, MemoryText = "45.6 MB", Disk = 0.3, DiskText = "0.3 MB/秒", Network = 0.1, NetworkText = "0.1 Mbps" },
                new ProcessItem { Name = "Microsoft Store", Icon = "🛒", CPU = 0.8, CpuText = "0.8%", Memory = 32.4, MemoryText = "32.4 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0.5, NetworkText = "0.5 Mbps" },
                new ProcessItem { Name = "设置", Icon = "⚙", CPU = 0.2, CpuText = "0.2%", Memory = 28.9, MemoryText = "28.9 MB", Disk = 0, DiskText = "0 MB/秒", Network = 0, NetworkText = "0 Mbps" },
            };
            FilteredProcesses = new ObservableCollection<ProcessItem>(Processes);
        }

        private void FilterProcesses()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProcesses = new ObservableCollection<ProcessItem>(Processes);
            }
            else
            {
                var filtered = Processes.Where(p => p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                FilteredProcesses = new ObservableCollection<ProcessItem>(filtered);
            }
        }
    }
}