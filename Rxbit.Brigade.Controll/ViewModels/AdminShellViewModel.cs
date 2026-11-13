using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class AdminNavItem : ObservableObject
    {
        private string _icon;
        private string _label;
        private bool _isExpanded;
        private bool _isActive;
        private string _targetView;
        private ObservableCollection<AdminNavItem> _children;

        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsExpanded { get => _isExpanded; set { SetProperty(ref _isExpanded, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public string TargetView { get => _targetView; set { SetProperty(ref _targetView, value); } }
        public ObservableCollection<AdminNavItem> Children { get => _children; set { SetProperty(ref _children, value); } }

        public AdminNavItem()
        {
            Children = new ObservableCollection<AdminNavItem>();
        }
    }

    public class AdminShellViewModel : ObservableObject
    {
        private ObservableCollection<AdminNavItem> _navItems;
        private string _currentView;

        public ObservableCollection<AdminNavItem> NavItems { get => _navItems; set { SetProperty(ref _navItems, value); } }
        public string CurrentView { get => _currentView; set { SetProperty(ref _currentView, value); } }

        public AdminShellViewModel()
        {
            NavItems = new ObservableCollection<AdminNavItem>
            {
                new AdminNavItem { Icon = "👥", Label = "会员管理", TargetView = "member" },
                new AdminNavItem { Icon = "📋", Label = "订单管理", TargetView = "order" },
                new AdminNavItem { Icon = "📁", Label = "分类管理", TargetView = "category" },
                new AdminNavItem { Icon = "🏙️", Label = "城市联动", TargetView = "city" },
                new AdminNavItem
                {
                    Icon = "⚙️", Label = "管理员管理", IsExpanded = true,
                    Children = new ObservableCollection<AdminNavItem>
                    {
                        new AdminNavItem { Label = "管理员列表", TargetView = "list" },
                        new AdminNavItem { Label = "角色管理", TargetView = "dashboard", IsActive = true },
                        new AdminNavItem { Label = "权限分类", TargetView = "permission-cat" },
                        new AdminNavItem { Label = "权限管理", TargetView = "permission" },
                    }
                },
                new AdminNavItem
                {
                    Icon = "📊", Label = "系统统计", IsExpanded = true,
                    Children = new ObservableCollection<AdminNavItem>
                    {
                        new AdminNavItem { Label = "折线图", TargetView = "chart" },
                        new AdminNavItem { Label = "柱状图", TargetView = "barchart" },
                        new AdminNavItem { Label = "地图", TargetView = "map" },
                        new AdminNavItem { Label = "饼图", TargetView = "pie" },
                        new AdminNavItem { Label = "雷达图", TargetView = "radar" },
                        new AdminNavItem { Label = "热力图", TargetView = "heatmap" },
                    }
                },
                new AdminNavItem
                {
                    Icon = "🔤", Label = "图标字体", IsExpanded = true,
                    Children = new ObservableCollection<AdminNavItem>
                    {
                        new AdminNavItem { Label = "图标对应字体", TargetView = "iconfont" },
                    }
                },
                new AdminNavItem { Icon = "📄", Label = "其它页面", TargetView = "other" },
                new AdminNavItem { Icon = "🧩", Label = "layui第三方组件", TargetView = "layui" },
            };

            CurrentView = "dashboard";
        }

        public void NavigateTo(string target)
        {
            CurrentView = target;
            UpdateActiveState(NavItems, target);
        }

        private void UpdateActiveState(ObservableCollection<AdminNavItem> items, string target)
        {
            foreach (var item in items)
            {
                if (item.TargetView == target)
                {
                    item.IsActive = true;
                }
                else
                {
                    item.IsActive = false;
                }
                if (item.Children.Count > 0)
                {
                    UpdateActiveState(item.Children, target);
                }
            }
        }
    }
}
