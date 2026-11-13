using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Dashboard3View : UserControl
    {
        public Dashboard3View()
        {
            InitializeComponent();
            DataContext = new Dashboard3ViewModel();
        }
    }
}
