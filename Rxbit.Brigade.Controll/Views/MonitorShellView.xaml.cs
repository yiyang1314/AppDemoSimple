using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class MonitorShellView : UserControl
    {
        private MonitorShellViewModel _vm;

        public MonitorShellView()
        {
            InitializeComponent();
            _vm = new MonitorShellViewModel();
            DataContext = _vm;
            Loaded += MonitorShellView_Loaded;
        }

        private void MonitorShellView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMenu();
            Navigate(_vm.Current);
        }

        private void BuildMenu()
        {
            MonNavPanel.Children.Clear();

            foreach (var m in _vm.NavItems)
            {
                var bd = new Border
                {
                    Background = m.IsActive
                        ? new SolidColorBrush(Color.FromRgb(0, 120, 212))
                        : Brushes.Transparent,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(12, 7, 12, 7),
                    Margin = new Thickness(0, 1, 0, 1),
                    Cursor = Cursors.Hand
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };

                var iconTb = new TextBlock
                {
                    Text = m.Icon,
                    FontSize = 14,
                    Width = 24,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var labelTb = new TextBlock
                {
                    Text = m.Label,
                    FontSize = 13,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                sp.Children.Add(iconTb);
                sp.Children.Add(labelTb);

                bd.Child = sp;
                string target = m.Target;
                bd.MouseLeftButtonDown += (s, ee) => Navigate(target);
                MonNavPanel.Children.Add(bd);
            }
        }

        private void Navigate(string target)
        {
            _vm.Navigate(target);
            BuildMenu();
            MonContentArea.Children.Clear();
            UserControl view;
            switch (target)
            {
                case "process":
                    view = new MonitorProcessView(); break;
                case "performance":
                    view = new MonitorPerformanceView(); break;
                default:
                    view = new MonitorPerformanceView(); break;
            }
            MonContentArea.Children.Add(view);
        }
    }
}
