using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Hmi3View : UserControl
    {
        public Hmi3View()
        {
            InitializeComponent();
            DataContext = new Hmi3ViewModel();
        }
    }
}
