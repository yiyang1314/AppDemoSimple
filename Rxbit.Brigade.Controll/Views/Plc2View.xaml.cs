using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Plc2View : UserControl
    {
        public Plc2View()
        {
            InitializeComponent();
            DataContext = new Plc2ViewModel();
        }
    }
}
