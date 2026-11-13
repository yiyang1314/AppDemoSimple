using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Plc3View : UserControl
    {
        public Plc3View()
        {
            InitializeComponent();
            DataContext = new Plc3ViewModel();
        }
    }
}
