using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace Rxbit.BrigadeApp.Controls
{
    public class WindowBase : Window
    {
        private DateTime _lastClickTime;
        private const double DoubleClickMs = 300;

        public WindowBase()
        {
            Loaded += Window_Loaded;
            Initialized += WindowBase_Initialized;
        }

        private void WindowBase_Initialized(object sender, EventArgs e)
        {
            // 从应用资源中获取 WindowBase 的样式
            var style = Application.Current.TryFindResource(typeof(WindowBase)) as Style;
            if (style != null)
            {
                this.Style = style;
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 从全局模板中查找按钮并绑定事件
            var btnMin = Template.FindName("BtnMin", this) as Button;
            var btnMax = Template.FindName("BtnMax", this) as Button;
            var btnClose = Template.FindName("BtnClose", this) as Button;
            var titleBar = Template.FindName("TitleBar", this) as Grid;

            if (btnMin != null)
                btnMin.Click += BtnMin_Click;
            if (btnMax != null)
                btnMax.Click += BtnMax_Click;
            if (btnClose != null)
                btnClose.Click += BtnClose_Click;
            if (titleBar != null)
                titleBar.MouseDown += TitleBar_MouseDown;

            StateChanged += Window_StateChanged;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // 双击标题栏最大化/还原窗口
                var currentTime = DateTime.Now;
                var elapsed = currentTime - _lastClickTime;
                if (elapsed.TotalMilliseconds <= DoubleClickMs)
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    DragMove();
                }
                _lastClickTime = currentTime;
            }
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var btnMax = Template.FindName("BtnMax", this) as Button;
            if (btnMax == null) return;
            
            btnMax.Tag = WindowState == WindowState.Maximized ? "Restored" : "Maximized";
        }
    }
}
