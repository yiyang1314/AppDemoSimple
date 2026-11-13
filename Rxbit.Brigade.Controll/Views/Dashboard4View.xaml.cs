using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Dashboard4View : UserControl
    {
        public Dashboard4View()
        {
            InitializeComponent();
            DataContext = new Dashboard4ViewModel();
        }
    }
}
