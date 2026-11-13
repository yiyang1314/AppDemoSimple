using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Dashboard2View : UserControl
    {
        public Dashboard2View()
        {
            InitializeComponent();
            DataContext = new Dashboard2ViewModel();
        }
    }
}
