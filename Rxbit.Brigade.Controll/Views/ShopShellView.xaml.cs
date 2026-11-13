using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class ShopShellView : UserControl
    {
        private ShopShellViewModel _vm;

        public ShopShellView()
        {
            InitializeComponent();
            _vm = new ShopShellViewModel();
            DataContext = _vm;
            Loaded += ShopShellView_Loaded;
        }

        private void ShopShellView_Loaded(object sender, RoutedEventArgs e)
        {
            BuildMenu();
            Navigate(_vm.Current);
        }

        private void BuildMenu()
        {
            ShopMenuPanel.Children.Clear();

            foreach (var m in _vm.Menus)
            {
                var bd = new Border
                {
                    Background = m.IsActive
                        ? new SolidColorBrush(Color.FromRgb(0, 181, 120))
                        : Brushes.Transparent,
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(12, 9, 12, 9),
                    Margin = new Thickness(0, 2, 0, 2),
                    Cursor = Cursors.Hand
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };

                var iconTb = new TextBlock
                {
                    Text = m.Icon,
                    FontSize = 16,
                    Width = 24,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(96, 98, 102)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var labelTb = new TextBlock
                {
                    Text = m.Label,
                    FontSize = 13,
                    Foreground = m.IsActive ? Brushes.White : new SolidColorBrush(Color.FromRgb(96, 98, 102)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                sp.Children.Add(iconTb);
                sp.Children.Add(labelTb);

                if (!string.IsNullOrEmpty(m.Badge))
                {
                    var badge = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(255, 77, 79)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(5, 1, 5, 1),
                        Margin = new Thickness(6, 0, 0, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        Child = new TextBlock
                        {
                            Text = m.Badge,
                            FontSize = 10,
                            Foreground = Brushes.White,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    };
                    sp.Children.Add(badge);
                }

                bd.Child = sp;
                string target = m.Target;
                bd.MouseLeftButtonDown += (s, ee) => Navigate(target);
                ShopMenuPanel.Children.Add(bd);
            }
        }

        private void Navigate(string target)
        {
            _vm.Navigate(target);
            BuildMenu();
            ShopContentArea.Children.Clear();
            UserControl view;
            switch (target)
            {
                case "shop1":
                case "game":
                case "wxgame":
                case "category":
                    view = new Shop1View(); break;
                case "shop2":
                    view = new Shop2View(); break;
                case "shop3":
                    view = new Shop3View(); break;
                case "shop4":
                    view = new Shop4View(); break;
                case "shop5":
                    view = new Shop5View(); break;
                default:
                    view = new Shop1View(); break;
            }
            ShopContentArea.Children.Add(view);
        }
    }
}
