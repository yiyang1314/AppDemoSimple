using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Shop2View : UserControl
    {
        public Shop2View()
        {
            InitializeComponent();
            DataContext = new Shop2ViewModel();
        }
    }
}
