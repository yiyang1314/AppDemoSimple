using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Hmi4View : UserControl
    {
        public Hmi4View()
        {
            InitializeComponent();
            DataContext = new Hmi4ViewModel();
        }
    }
}
