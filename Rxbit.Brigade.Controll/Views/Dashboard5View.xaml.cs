using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Dashboard5View : UserControl
    {
        public Dashboard5View()
        {
            InitializeComponent();
            DataContext = new Dashboard5ViewModel();
        }
    }
}
