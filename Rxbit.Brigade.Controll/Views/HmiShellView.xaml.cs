using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class HmiShellView : UserControl
    {
        private HmiShellViewModel _vm;

        public HmiShellView()
        {
            InitializeComponent();
            _vm = new HmiShellViewModel();
            DataContext = _vm;
            Loaded += HmiShellView_Loaded;
        }

        private void HmiShellView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMenu();
            Navigate(_vm.Current);
        }

        private void BuildMenu()
        {
            HmiMenuPanel.Children.Clear();
            TextBlock header = new TextBlock
            {
                Text = "HMI 界面列表",
                Foreground = new SolidColorBrush(Color.FromRgb(90, 120, 160)),
                FontSize = 11,
                Margin = new Thickness(12, 4, 0, 8)
            };
            HmiMenuPanel.Children.Add(header);

            foreach (var m in _vm.Menus)
            {
                var bd = new Border
                {
                    Background = m.IsActive
                        ? new SolidColorBrush(Color.FromRgb(28, 156, 255))
                        : Brushes.Transparent,
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(14, 10, 14, 10),
                    Margin = new Thickness(0, 2, 0, 2),
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
                HmiMenuPanel.Children.Add(bd);
            }
        }

        private void Navigate(string target)
        {
            _vm.Navigate(target);
            BuildMenu();
            HmiContentArea.Children.Clear();
            UserControl view;
            switch (target)
            {
                case "hmi1": view = new Hmi1View(); break;
                case "hmi2": view = new Hmi2View(); break;
                case "hmi3": view = new Hmi3View(); break;
                case "hmi4": view = new Hmi4View(); break;
                case "hmi5": view = new Hmi5View(); break;
                default: view = new Hmi1View(); break;
            }
            HmiContentArea.Children.Add(view);
        }
    }
}
