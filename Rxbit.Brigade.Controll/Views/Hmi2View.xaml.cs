using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Hmi2View : UserControl
    {
        public Hmi2View()
        {
            InitializeComponent();
            DataContext = new Hmi2ViewModel();
        }
    }
}
