using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Dashboard1View : UserControl
    {
        public Dashboard1View()
        {
            InitializeComponent();
            DataContext = new Dashboard1ViewModel();
        }

        private void Item_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(43, 58, 75));
            }
        }

        private void Item_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(38, 52, 69));
            }
        }
    }
}
