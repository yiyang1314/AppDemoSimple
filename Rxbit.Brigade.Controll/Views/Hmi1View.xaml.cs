using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Hmi1View : UserControl
    {
        public Hmi1View()
        {
            InitializeComponent();
            DataContext = new Hmi1ViewModel();
        }
    }
}
