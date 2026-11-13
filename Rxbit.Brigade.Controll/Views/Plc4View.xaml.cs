using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Plc4View : UserControl
    {
        public Plc4View()
        {
            InitializeComponent();
            DataContext = new Plc4ViewModel();
        }
    }
}
