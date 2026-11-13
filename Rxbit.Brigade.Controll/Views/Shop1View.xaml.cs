using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Shop1View : UserControl
    {
        public Shop1View()
        {
            InitializeComponent();
            DataContext = new Shop1ViewModel();
        }
    }
}
