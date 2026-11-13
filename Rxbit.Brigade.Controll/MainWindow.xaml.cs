using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.Views;

namespace Rxbit.Brigade.Controll
{
    public partial class MainWindow : Window
    {
        private UserControl _currentView;

        public MainWindow()
        {
            InitializeComponent();
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            NavigateTo(1);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Alt) return;

            int index = -1;
            if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                index = e.Key - Key.D0;
            }
            else if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                index = e.Key - Key.NumPad0;
            }

            if (index >= 1 && index <= 9)
            {
                NavigateTo(index);
                e.Handled = true;
            }
        }

        private void SetActiveNav(int index)
        {
            var normal = (Style)FindResource("ControllNavStyle");
            var active = (Style)FindResource("ControllNavActiveStyle");

            foreach (var child in NavPanel.Children)
            {
                var button = child as Button;
                if (button == null) continue;

                int tag;
                if (!int.TryParse(button.Tag?.ToString(), out tag)) continue;
                button.Style = tag == index ? active : normal;
            }
        }

        private void BtnNav_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            int tag;
            if (!int.TryParse(btn.Tag.ToString(), out tag)) return;

            NavigateTo(tag);
        }

        private void NavigateTo(int index)
        {
            if (index == 11 || index == 12)
            {
                LaunchExternalModule(index);
                return;
            }

            SetActiveNav(index);

            if (_currentView != null)
            {
                ContentArea.Children.Clear();
            }

            switch (index)
            {
                case 1:
                    _currentView = new Dashboard1View();
                    break;
                case 2:
                    _currentView = new Dashboard2View();
                    break;
                case 3:
                    _currentView = new Dashboard3View();
                    break;
                case 4:
                    _currentView = new Dashboard4View();
                    break;
                case 5:
                    _currentView = new Dashboard5View();
                    break;
                case 6:
                    _currentView = new AdminShellView();
                    break;
                case 7:
                    _currentView = new HmiShellView();
                    break;
                case 8:
                    _currentView = new PlcShellView();
                    break;
                case 9:
                    _currentView = new ShopShellView();
                    break;
                case 10:
                    _currentView = new MonitorShellView();
                    break;
                default:
                    _currentView = new Dashboard1View();
                    break;
            }

            ContentArea.Children.Add(_currentView);
            UpdateNavButtons(index);
        }

        private void LaunchExternalModule(int index)
        {
            string moduleDir = "";
            string exeName = "";
            string moduleName = "";

            switch (index)
            {
                case 11:
                    moduleDir = "Rxbit.Brigade.HalconVision";
                    exeName = "Rxbit.Brigade.HalconVision.exe";
                    moduleName = "HalconVision 机器视觉开发环境";
                    break;
                case 12:
                    moduleDir = "Rxbit.Brigade.ZcanPro";
                    exeName = "Rxbit.Brigade.ZcanPro.exe";
                    moduleName = "ZCAN Pro 总线分析仪";
                    break;
            }

            string exePath = ResolveModuleExe(moduleDir, exeName);
            if (exePath != null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = System.IO.Path.GetDirectoryName(exePath),
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show($"找不到 {moduleName} 可执行文件。\n\n已依次查找：\n{DescribeModuleSearchPaths(moduleDir, exeName)}\n\n请先编译对应模块，或将发布产物放到上述任一目录。", "启动失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 在「源码树运行」与「独立目录发布运行」两种布局下探测模块 exe。
        /// </summary>
        private static string ResolveModuleExe(string moduleDir, string exeName)
        {
            foreach (var candidate in GetModuleSearchPaths(moduleDir, exeName))
            {
                if (System.IO.File.Exists(candidate)) return candidate;
            }
            return null;
        }

        private static string[] GetModuleSearchPaths(string moduleDir, string exeName)
        {
            var paths = new System.Collections.Generic.List<string>();
            var dir = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'));
#if DEBUG
            // 当前以 Debug 运行时优先命中 Debug 产物，找不到再回落到 Release
            string firstConfig = "Debug";
            string secondConfig = "Release";
#else
            // 当前以 Release 运行时优先命中 Release 产物，找不到再回落到 Debug
            string firstConfig = "Release";
            string secondConfig = "Debug";
#endif
            // 从当前 exe 所在目录逐级向上探测：兼容 publish 同级目录、合并发布、源码树 bin\Debug|Release 与 bin\x86\Debug|Release
            for (int depth = 0; depth < 6 && dir != null; depth++, dir = dir.Parent)
            {
                string root = dir.FullName;
                paths.Add(System.IO.Path.Combine(root, exeName));
                paths.Add(System.IO.Path.Combine(root, moduleDir, exeName));
                paths.Add(System.IO.Path.Combine(root, moduleDir, "bin", firstConfig, "net48", exeName));
                paths.Add(System.IO.Path.Combine(root, moduleDir, "bin", secondConfig, "net48", exeName));
                paths.Add(System.IO.Path.Combine(root, moduleDir, "bin", "x86", firstConfig, "net48", exeName));
                paths.Add(System.IO.Path.Combine(root, moduleDir, "bin", "x86", secondConfig, "net48", exeName));
            }

            var result = new System.Collections.Generic.List<string>();
            foreach (var p in paths)
            {
                string full = System.IO.Path.GetFullPath(p);
                if (!result.Contains(full)) result.Add(full);
            }
            return result.ToArray();
        }

        private static string DescribeModuleSearchPaths(string moduleDir, string exeName)
        {
            return string.Join("\n", GetModuleSearchPaths(moduleDir, exeName));
        }

        private void UpdateNavButtons(int activeIndex)
        {
            var buttons = new[] { BtnNav1, BtnNav2, BtnNav3, BtnNav4, BtnNav5, BtnNav6, BtnNav7, BtnNav8, BtnNav9, BtnNav10, BtnNav11, BtnNav12 };

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = buttons[i];
                if (i + 1 == activeIndex)
                {
                    btn.Style = TryFindResource("SideNavButtonActiveStyle") as Style;
                }
                else
                {
                    btn.Style = TryFindResource("SideNavButtonStyle") as Style;
                }
            }
        }
    }
}
