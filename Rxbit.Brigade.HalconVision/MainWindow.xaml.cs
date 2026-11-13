using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rxbit.Brigade.HalconVision.ViewModels;
using Rxbit.Brigade.HalconVision.Views;

namespace Rxbit.Brigade.HalconVision
{
    public partial class MainWindow : Window
    {
        private readonly HalconMainViewModel _viewModel;
        private int _windowCounter;
        private MdiChildWindow _activeWindow;

        public ObservableCollection<MdiChildWindow> ChildWindows { get; } = new ObservableCollection<MdiChildWindow>();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new HalconMainViewModel();
            _windowCounter = 0;

            Loaded += MainWindow_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            AddGraphicsWindow();
            AddProgramWindow();
            AddVariablesWindow();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure canvas fills the grid
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

        /// <summary>
        /// 按行按列平均分配界面大小，默认按左右上下排列子窗口。
        /// 1 个 → 1×1；2 个 → 1×2（左右）；3+ 个 → 列数=⌈√n⌉，行数自动向上取整。
        /// </summary>
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
                child.Margin = new Thickness(0);
                Canvas.SetLeft(child, spacing + col * (childWidth + spacing));
                Canvas.SetTop(child, spacing + row * (childHeight + spacing));
            }
        }

        private void AddGraphicsWindow()
        {
            _windowCounter++;
            var vm = new GraphicsWindowViewModel(_windowCounter);
            var view = new GraphicsWindowView { DataContext = vm };

            var child = CreateMdiChild($"图形窗口 [HALCON{_windowCounter}]", "◫", view, 480, 360);
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
        }

        private void AddProgramWindow()
        {
            _windowCounter++;
            var vm = new ProgramWindowViewModel();
            var view = new ProgramWindowView { DataContext = vm };

            var child = CreateMdiChild($"程序窗口 - main() - 主线程: {42548 + _windowCounter}", "▣", view, 560, 420);
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
        }

        private void AddVariablesWindow()
        {
            _windowCounter++;
            var vm = new VariablesWindowViewModel();
            var view = new VariablesWindowView { DataContext = vm };

            var child = CreateMdiChild($"变量窗口 [Variables{_windowCounter}]", "▤", view, 420, 340);
            MdiCanvas.Children.Add(child);
            ChildWindows.Add(child);
            SetActiveWindow(child);
            ArrangeChildWindows();
        }

        private MdiChildWindow CreateMdiChild(string title, string icon, object content, double width, double height)
        {
            var child = new MdiChildWindow
            {
                Title = title,
                Icon = icon,
                ChildContent = content,
                Width = width,
                Height = height,
                MinWidth = 280,
                MinHeight = 180
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
            _activeWindow.IsActive = true;

            // Update status bar
            if (window != null)
            {
                StatusWindowInfo.Content = $"活动窗口: {window.Title}";
            }
        }

        private void CloseWindow(MdiChildWindow window)
        {
            MdiCanvas.Children.Remove(window);
            ChildWindows.Remove(window);

            if (_activeWindow == window)
            {
                _activeWindow = null;
                StatusWindowInfo.Content = "活动窗口: 无";
            }

            ArrangeChildWindows();
        }

        private void NewGraphicsWindow_Click(object sender, RoutedEventArgs e) => AddGraphicsWindow();
        private void NewProgramWindow_Click(object sender, RoutedEventArgs e) => AddProgramWindow();
        private void NewVariablesWindow_Click(object sender, RoutedEventArgs e) => AddVariablesWindow();

        private void CloseCurrentWindow_Click(object sender, RoutedEventArgs e)
        {
            if (_activeWindow != null)
            {
                CloseWindow(_activeWindow);
            }
        }

        private void CloseAllWindows_Click(object sender, RoutedEventArgs e)
        {
            var windowsToClose = new List<MdiChildWindow>(ChildWindows);
            foreach (var w in windowsToClose)
            {
                MdiCanvas.Children.Remove(w);
            }
            ChildWindows.Clear();
            _activeWindow = null;
            StatusWindowInfo.Content = "活动窗口: 无";
        }

        private void ResetLayout_Click(object sender, RoutedEventArgs e)
        {
            ArrangeChildWindows();
        }

        private void RunProgram_Click(object sender, RoutedEventArgs e)
        {
            StatusWindowInfo.Content = "运行程序...";
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HALCON 文件|*.hdev;*.hdvp|所有文件|*.*",
                Title = "打开"
            };
            if (dlg.ShowDialog() == true)
            {
                StatusWindowInfo.Content = $"已打开: {dlg.FileName}";
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            StatusWindowInfo.Content = "文件已保存";
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "HALCON 文件|*.hdev|所有文件|*.*",
                Title = "另存为"
            };
            if (dlg.ShowDialog() == true)
            {
                StatusWindowInfo.Content = $"另存为: {dlg.FileName}";
            }
        }

        private void ImportImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "图像文件|*.png;*.jpg;*.bmp;*.tiff;*.tif|所有文件|*.*",
                Title = "导入图像"
            };
            if (dlg.ShowDialog() == true)
            {
                StatusWindowInfo.Content = $"已导入: {dlg.FileName}";
            }
        }

        private void OpenFunctionBrowser_Click(object sender, RoutedEventArgs e)
        {
            AddVariablesWindow();
            StatusWindowInfo.Content = "函数浏览器已打开";
        }

        private void TestNotification_Click(object sender, RoutedEventArgs e)
        {
            var nm = NotificationManager.Instance;
            nm.ShowNotification(
                "HALCON 程序运行完成",
                "图像处理任务已成功完成。耗时 2.35 秒，处理了 128 张图像。",
                "✅",
                "查看结果",
                () =>
                {
                    nm.ShowNotification(
                        "打开结果",
                        "正在跳转到结果窗口...",
                        "🔔",
                        null,
                        null,
                        3000);
                },
                5000);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                RunProgram_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers != ModifierKeys.Control) return;

            switch (e.Key)
            {
                case Key.S:
                    SaveFile_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.O:
                    OpenFile_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.N:
                    NewGraphicsWindow_Click(sender, e);
                    e.Handled = true;
                    break;
                case Key.H:
                    OpenFunctionBrowser_Click(sender, e);
                    e.Handled = true;
                    break;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}