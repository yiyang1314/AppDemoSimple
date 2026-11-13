using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class MonitorProcessView : UserControl
    {
        private MonitorProcessViewModel _vm;

        public MonitorProcessView()
        {
            InitializeComponent();
            _vm = new MonitorProcessViewModel();
            DataContext = _vm;
        }
    }
}