using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Plc5View : UserControl
    {
        public Plc5View()
        {
            InitializeComponent();
            DataContext = new Plc5ViewModel();
        }
    }
}
