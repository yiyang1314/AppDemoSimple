using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class AdminMenuItem : ObservableObject
    {
        private string _icon;
        private string _label;
        private bool _isExpanded;
        private bool _isActive;
        private ObservableCollection<AdminMenuItem> _children;
        private string _targetView;

        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsExpanded { get => _isExpanded; set { SetProperty(ref _isExpanded, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public ObservableCollection<AdminMenuItem> Children { get => _children; set { SetProperty(ref _children, value); } }
        public string TargetView { get => _targetView; set { SetProperty(ref _targetView, value); } }

        public AdminMenuItem()
        {
            Children = new ObservableCollection<AdminMenuItem>();
        }
    }

    public class AdminStatItem : ObservableObject
    {
        private string _label;
        private string _value;
        private string _color;

        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public string Value { get => _value; set { SetProperty(ref _value, value); } }
        public string Color { get => _color; set { SetProperty(ref _color, value); } }
    }

    public class AdminDownloadItem : ObservableObject
    {
        private string _title;
        private string _count;
        private string _percent;
        private string _color;

        public string Title { get => _title; set { SetProperty(ref _title, value); } }
        public string Count { get => _count; set { SetProperty(ref _count, value); } }
        public string Percent { get => _percent; set { SetProperty(ref _percent, value); } }
        public string Color { get => _color; set { SetProperty(ref _color, value); } }
    }

    public class AdminSystemInfoItem : ObservableObject
    {
        private string _name;
        private string _value;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Value { get => _value; set { SetProperty(ref _value, value); } }
    }

    public class AdminDashboardViewModel : ObservableObject
    {
        private string _currentTime;
        private ObservableCollection<AdminStatItem> _stats;
        private ObservableCollection<AdminDownloadItem> _downloads;
        private ObservableCollection<AdminSystemInfoItem> _systemInfos;

        public string CurrentTime { get => _currentTime; set { SetProperty(ref _currentTime, value); } }
        public ObservableCollection<AdminStatItem> Stats { get => _stats; set { SetProperty(ref _stats, value); } }
        public ObservableCollection<AdminDownloadItem> Downloads { get => _downloads; set { SetProperty(ref _downloads, value); } }
        public ObservableCollection<AdminSystemInfoItem> SystemInfos { get => _systemInfos; set { SetProperty(ref _systemInfos, value); } }

        public AdminDashboardViewModel()
        {
            CurrentTime = "2018-04-25 20:50:53";

            Stats = new ObservableCollection<AdminStatItem>
            {
                new AdminStatItem { Label = "文章数", Value = "66", Color = "#1E9FFF" },
                new AdminStatItem { Label = "会员数", Value = "12", Color = "#5FB878" },
                new AdminStatItem { Label = "回复数", Value = "99", Color = "#FFB800" },
                new AdminStatItem { Label = "商品数", Value = "67", Color = "#E91E63" },
                new AdminStatItem { Label = "文章数", Value = "67", Color = "#1E9FFF" },
                new AdminStatItem { Label = "文章数", Value = "6766", Color = "#5FB878" },
            };

            Downloads = new ObservableCollection<AdminDownloadItem>
            {
                new AdminDownloadItem { Title = "下载月", Count = "33,555", Percent = "新下载 10%", Color = "#1E9FFF" },
                new AdminDownloadItem { Title = "下载月", Count = "33,555", Percent = "新下载 10%", Color = "#5FB878" },
                new AdminDownloadItem { Title = "下载月", Count = "33,555", Percent = "新下载 10%", Color = "#FFB800" },
                new AdminDownloadItem { Title = "下载月", Count = "33,555", Percent = "新下载 10%", Color = "#E91E63" },
            };

            SystemInfos = new ObservableCollection<AdminSystemInfoItem>
            {
                new AdminSystemInfoItem { Name = "版本", Value = "1.0.180420" },
                new AdminSystemInfoItem { Name = "服务器地址", Value = "x.xuebingsi.com" },
                new AdminSystemInfoItem { Name = "操作系统", Value = "WINNT" },
            };
        }
    }
}
