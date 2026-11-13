using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Rxbit.Brigade.ZcanPro.ViewModels;
using Rxbit.Brigade.ZcanPro.Views;

namespace Rxbit.Brigade.ZcanPro
{
    public partial class MainWindow : Window
    {
        private int _windowCounter;
        private MdiChildWindow _activeWindow;
        private MdiChildWindow _ecuFlashChild;
        private int _zCounter;

        public ObservableCollection<MdiChildWindow> ChildWindows { get; } = new ObservableCollection<MdiChildWindow>();

        public MainWindow()
        {
            InitializeComponent();
            _windowCounter = 0;

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;

            AddOscilloscopeView();
            AddDataPanelView();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MdiCanvas.Width = ContentGrid.ActualWidth;
            MdiCanvas.Height = ContentGrid.ActualHeight;
            Canvas.SetLeft(MdiCanvas, 0);
            Canvas.SetTop(MdiCanvas, 0);
            ArrangeChildWindows();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MdiCanvas.Width = ContentGrid.ActualWidth;
            MdiCanvas.Height = ContentGrid.ActualHeight;
            ArrangeChildWindows();
        }

        private void ArrangeChildWindows()
        {
            if (ChildWindows.Count == 0) return;

            double canvasWidth = MdiCanvas.ActualWidth > 0 ? MdiCanvas.ActualWidth : MdiCanvas.Width;
            double canvasHeight = MdiCanvas.ActualHeight > 0 ? MdiCanvas.ActualHeight : MdiCanvas.Height;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            int count = ChildWindows.Count;
            int cols, rows;

            if (count == 1)
            {
                cols = 1; rows = 1;
            }
            else if (count == 2)
            {
                cols = 2; rows = 1;
            }
            else
            {
                cols = (int)Math.Ceiling(Math.Sqrt(count));
                rows = (int)Math.Ceiling((double)count / cols);
            }

            double spacing = 6;
            double childWidth = (canvasWidth - (cols + 1) * spacing) / cols;
            double childHeight = (canvasHeight - (rows + 1) * spacing) / rows;

            childWidth = Math.Max(280, childWidth);
            childHeight = Math.Max(200, childHeight);

            for (int i = 0; i < ChildWindows.Count; i++)
            {
                var child = ChildWindows[i];
                int col = i % cols;
                int row = i / cols;

                child.Width = childWidth;
                child.Height = childHeight;
                Canvas.SetLeft(child, spacing + col * (childWidth + spacing));
                Canvas.SetTop(child, spacing + row * (childHeight + spacing));
            }
        }

        private void AddOscilloscopeView()
        {
            _windowCounter++;
            var vm = new OscilloscopeViewModel(_windowCounter);
            var view = new OscilloscopeView { DataContext = vm };

            var child = CreateMdiChild($"示波器视图 {_windowCounter}", "📊", view);
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
        }

        private void AddDataPanelView()
        {
            _windowCounter++;
            var vm = new DataPanelViewModel(_windowCounter);
            var view = new DataPanelView { DataContext = vm };

            var child = CreateMdiChild($"数据面板 {_windowCounter}", "📋", view);
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
        }

        private void AddEcuFlashView()
        {
            // 单例：已存在则直接弹出并置顶，不重复创建
            if (_ecuFlashChild != null && ChildWindows.Contains(_ecuFlashChild))
            {
                BringToFront(_ecuFlashChild);
                return;
            }

            _windowCounter++;
            var view = new EcuFlashView();

            var child = CreateMdiChild($"ECU Flash 刷写 {_windowCounter}", "💾", view);
            child.MinWidth = 960;
            child.MinHeight = 600;
            _ecuFlashChild = child;
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
            BringToFront(child);
        }

        /// <summary>
        /// 将指定子窗口置顶显示：恢复可见、激活、设为最高 ZIndex、移入可视区域。
        /// </summary>
        private void BringToFront(MdiChildWindow window)
        {
            if (window == null) return;

            // 若被最小化则恢复显示
            if (window.Visibility != Visibility.Visible)
            {
                window.Visibility = Visibility.Visible;
            }

            // 提升到最高 ZIndex
            _zCounter++;
            Panel.SetZIndex(window, _zCounter);

            // 确保位于画布可视区域内
            double left = Canvas.GetLeft(window);
            double top = Canvas.GetTop(window);
            double w = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
            double h = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
            double canvasW = MdiCanvas.ActualWidth > 0 ? MdiCanvas.ActualWidth : MdiCanvas.Width;
            double canvasH = MdiCanvas.ActualHeight > 0 ? MdiCanvas.ActualHeight : MdiCanvas.Height;

            if (canvasW > 0 && left + w > canvasW)
                Canvas.SetLeft(window, Math.Max(0, canvasW - w - 4));
            else if (left < 0)
                Canvas.SetLeft(window, 0);

            if (canvasH > 0 && top + h > canvasH)
                Canvas.SetTop(window, Math.Max(0, canvasH - h - 4));
            else if (top < 0)
                Canvas.SetTop(window, 0);

            SetActiveWindow(window);
            window.Focus();
        }

        private MdiChildWindow CreateMdiChild(string title, string icon, object content)
        {
            var child = new MdiChildWindow
            {
                Title = title,
                Icon = icon,
                ChildContent = content,
                MinWidth = 280,
                MinHeight = 200
            };

            child.CloseRequested += (s, e) => CloseWindow(child);
            child.Activated += (s, e) => SetActiveWindow(child);

            return child;
        }

        private void SetActiveWindow(MdiChildWindow window)
        {
            if (_activeWindow != null && _activeWindow != window)
            {
                _activeWindow.IsActive = false;
            }
            _activeWindow = window;
            if (window != null)
            {
                _activeWindow.IsActive = true;
                _zCounter++;
                Panel.SetZIndex(window, _zCounter);
                StatusActiveView.Text = $"活动视图: {window.Title}";
            }
        }

        private void CloseWindow(MdiChildWindow window)
        {
            MdiCanvas.Children.Remove(window);
            ChildWindows.Remove(window);

            if (ReferenceEquals(_ecuFlashChild, window))
            {
                _ecuFlashChild = null;
            }

            if (_activeWindow == window)
            {
                _activeWindow = null;
                StatusActiveView.Text = "活动视图: 无";
            }

            ArrangeChildWindows();
        }

        private void NewOscilloscope_Click(object sender, RoutedEventArgs e) => AddOscilloscopeView();
        private void NewDataPanel_Click(object sender, RoutedEventArgs e) => AddDataPanelView();
        private void NewEcuFlash_Click(object sender, RoutedEventArgs e) => AddEcuFlashView();

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                switch (button.Tag?.ToString())
                {
                    case "devices":
                        MessageBox.Show("设备管理 - 此功能模块正在开发中", "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "send":
                        MessageBox.Show("发送数据 - 此功能模块正在开发中", "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "usage":
                        MessageBox.Show("通道利用率 - 此功能模块正在开发中", "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "advanced":
                        AddEcuFlashView();
                        break;
                    case "tools":
                        MessageBox.Show("工具 - 此功能模块正在开发中", "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    case "settings":
                        MessageBox.Show("设置&帮助 - 此功能模块正在开发中", "ZCAN Pro", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }
            }
        }
    }
}