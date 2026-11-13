using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Shop5View : UserControl
    {
        public Shop5View()
        {
            InitializeComponent();
            DataContext = new Shop5ViewModel();
        }
    }
}
