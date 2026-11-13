using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// AboutView.xaml 的交互逻辑
    /// </summary>
    public partial class AboutView : WindowBase
    {
        public AboutView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetService<AboutViewModel>();
        }


        /// <summary>
        /// 菜单打开时处理：左对齐 + 防溢出
        /// </summary>
        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is  MenuItem) return;
            MenuItem menuItem = sender as MenuItem;
            // 延迟到布局完成后执行
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // 查找内部Popup
                var popup = FindVisualChild<Popup>(menuItem);
                if (popup == null) return;

                // 1. 强制左对齐：HorizontalOffset设为0
                popup.HorizontalOffset = 0;
                popup.VerticalOffset = 0;
                popup.Placement = PlacementMode.Bottom;

                // 2. 防溢出检测
                PreventOverflow(popup);
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        /// <summary>
        /// 防止子菜单溢出窗口边界
        /// </summary>
        private void PreventOverflow(Popup popup)
        {
            if (popup.Child == null) return;

            // 获取Popup实际尺寸
            popup.Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var popupWidth = popup.Child.DesiredSize.Width;
            var popupHeight = popup.Child.DesiredSize.Height;

            // 获取菜单项在屏幕上的位置
            var menuItem = popup.PlacementTarget as FrameworkElement;
            if (menuItem == null) return;

            var menuItemScreenPos = menuItem.PointToScreen(new Point(0, 0));
            var windowScreenPos = this.PointToScreen(new Point(0, 0));

            // 计算相对窗口的位置
            var relativeLeft = menuItemScreenPos.X - windowScreenPos.X;
            var relativeTop = menuItemScreenPos.Y - windowScreenPos.Y + menuItem.ActualHeight;

            // 检查右侧溢出
            if (relativeLeft + popupWidth > this.ActualWidth)
            {
                // 向右溢出，调整偏移使右边缘对齐窗口右边缘
                var overflow = (relativeLeft + popupWidth) - this.ActualWidth;
                popup.HorizontalOffset = -overflow - 8; // 留8px边距
            }

            // 检查底部溢出
            if (relativeTop + popupHeight > this.ActualHeight)
            {
                // 向上展开
                popup.Placement = PlacementMode.Top;
                popup.VerticalOffset = -menuItem.ActualHeight;
            }

            // 检查左侧溢出（当窗口靠近屏幕左边缘时）
            var screenLeft = windowScreenPos.X + relativeLeft + popup.HorizontalOffset;
            if (screenLeft < 0)
            {
                popup.HorizontalOffset = -relativeLeft + 8; // 最小8px边距
            }
            else
            {
                popup.HorizontalOffset = 0;
            }
        }

        /// <summary>
        /// 查找视觉树中的子元素
        /// </summary>
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
