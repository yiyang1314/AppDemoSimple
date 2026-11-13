using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Plc1View : UserControl
    {
        public Plc1View()
        {
            InitializeComponent();
            DataContext = new Plc1ViewModel();
        }
    }
}
