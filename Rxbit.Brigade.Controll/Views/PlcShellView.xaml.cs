using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class PlcShellView : UserControl
    {
        private PlcShellViewModel _vm;

        public PlcShellView()
        {
            InitializeComponent();
            _vm = new PlcShellViewModel();
            DataContext = _vm;
            Loaded += PlcShellView_Loaded;
        }

        private void PlcShellView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMenu();
            Navigate(_vm.Current);
        }

        private void BuildMenu()
        {
            PlcMenuPanel.Children.Clear();

            var header = new TextBlock
            {
                Text = "PLC 界面列表",
                Foreground = new SolidColorBrush(Color.FromRgb(90, 120, 160)),
                FontSize = 11,
                Margin = new Thickness(12, 4, 0, 8)
            };
            PlcMenuPanel.Children.Add(header);

            foreach (var m in _vm.Menus)
            {
                var bd = new Border
                {
                    Background = m.IsActive
                        ? new SolidColorBrush(Color.FromRgb(28, 165, 255))
                        : Brushes.Transparent,
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 4, 0, 4),
                    Cursor = Cursors.Hand
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock
                {
                    Text = m.Icon,
                    FontSize = 16,
                    Width = 24,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(180, 200, 220)),
                    VerticalAlignment = VerticalAlignment.Center
                });
                sp.Children.Add(new TextBlock
                {
                    Text = m.Label,
                    FontSize = 13,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(180, 200, 220)),
                    VerticalAlignment = VerticalAlignment.Center
                });

                bd.Child = sp;
                string target = m.Target;
                bd.MouseLeftButtonDown += (s, ee) => Navigate(target);
                PlcMenuPanel.Children.Add(bd);
            }
        }

        private void Navigate(string target)
        {
            _vm.Navigate(target);
            BuildMenu();
            PlcContentArea.Children.Clear();
            UserControl view;
            switch (target)
            {
                case "plc1": view = new Plc1View(); break;
                case "plc2": view = new Plc2View(); break;
                case "plc3": view = new Plc3View(); break;
                case "plc4": view = new Plc4View(); break;
                case "plc5": view = new Plc5View(); break;
                default: view = new Plc1View(); break;
            }
            PlcContentArea.Children.Add(view);
        }
    }
}
