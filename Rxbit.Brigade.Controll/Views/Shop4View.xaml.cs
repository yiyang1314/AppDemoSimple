using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Shop4View : UserControl
    {
        public Shop4View()
        {
            InitializeComponent();
            DataContext = new Shop4ViewModel();
        }
    }
}
