using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class AdminShellView : UserControl
    {
        private AdminShellViewModel _vm;
        private SolidColorBrush NormalBg;
        private SolidColorBrush ActiveBg;
        private SolidColorBrush NormalFg;
        private SolidColorBrush ActiveFg;
        private SolidColorBrush NormalSubBg;
        private SolidColorBrush ActiveSubBg;

        public AdminShellView()
        {
            InitializeComponent();
            _vm = new AdminShellViewModel();
            DataContext = _vm;

            NormalBg = new SolidColorBrush(Color.FromRgb(48, 65, 86));
            ActiveBg = new SolidColorBrush(Color.FromRgb(38, 52, 69));
            NormalFg = new SolidColorBrush(Color.FromRgb(192, 204, 218));
            ActiveFg = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            NormalSubBg = new SolidColorBrush(Color.FromRgb(42, 57, 75));
            ActiveSubBg = new SolidColorBrush(Color.FromRgb(38, 52, 69));

            Loaded += AdminShellView_Loaded;
        }

        private void AdminShellView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildSidebar();
            NavigateTo("dashboard");
        }

        private void BuildSidebar()
        {
            SidebarPanel.Children.Clear();

            foreach (var item in _vm.NavItems)
            {
                SidebarPanel.Children.Add(CreateMenuBorder(item));
                if (item.Children.Count > 0 && item.IsExpanded)
                {
                    foreach (var child in item.Children)
                    {
                        SidebarPanel.Children.Add(CreateSubMenuBorder(child));
                    }
                }
            }
        }

        private Border CreateMenuBorder(AdminNavItem item)
        {
            var border = new Border
            {
                Padding = new Thickness(16, 10, 12, 10),
                Background = item.IsActive ? ActiveBg : NormalBg,
                Cursor = Cursors.Hand
            };

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            if (!string.IsNullOrEmpty(item.Icon))
            {
                stack.Children.Add(new TextBlock
                {
                    Text = item.Icon,
                    FontSize = 14,
                    Width = 24,
                    Foreground = item.IsActive ? ActiveFg : NormalFg,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }

            stack.Children.Add(new TextBlock
            {
                Text = item.Label,
                FontSize = 13,
                Foreground = item.IsActive ? ActiveFg : NormalFg,
                VerticalAlignment = VerticalAlignment.Center
            });

            if (item.Children.Count > 0)
            {
                stack.Children.Add(new TextBlock
                {
                    Text = item.IsExpanded ? "▾" : "▸",
                    FontSize = 10,
                    Foreground = item.IsActive ? ActiveFg : NormalFg,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(12, 0, 0, 0)
                });
            }

            border.Child = stack;
            border.MouseLeftButtonDown += (s, e) =>
            {
                if (item.Children.Count > 0)
                {
                    item.IsExpanded = !item.IsExpanded;
                    BuildSidebar();
                }
                if (!string.IsNullOrEmpty(item.TargetView))
                {
                    NavigateTo(item.TargetView);
                }
            };

            return border;
        }

        private Border CreateSubMenuBorder(AdminNavItem item)
        {
            var border = new Border
            {
                Padding = new Thickness(40, 8, 12, 8),
                Background = item.IsActive ? ActiveSubBg : NormalSubBg,
                Cursor = Cursors.Hand
            };

            border.Child = new TextBlock
            {
                Text = item.Label,
                FontSize = 12,
                Foreground = item.IsActive ? ActiveFg : NormalFg
            };

            border.MouseLeftButtonDown += (s, e) =>
            {
                if (!string.IsNullOrEmpty(item.TargetView))
                {
                    NavigateTo(item.TargetView);
                }
            };

            return border;
        }

        private void NavigateTo(string target)
        {
            _vm.NavigateTo(target);
            BuildSidebar();

            switch (target)
            {
                case "dashboard":
                    AdminContent.Content = new AdminDashboardView();
                    break;
                case "chart":
                    AdminContent.Content = new AdminChartView();
                    break;
                case "list":
                    AdminContent.Content = new AdminListView();
                    break;
                case "iconfont":
                    AdminContent.Content = new AdminIconFontView();
                    break;
                default:
                    AdminContent.Content = new AdminDashboardView();
                    break;
            }
        }
    }
}
