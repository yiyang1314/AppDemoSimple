using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Shop3View : UserControl
    {
        public Shop3View()
        {
            InitializeComponent();
            DataContext = new Shop3ViewModel();
        }
    }
}
