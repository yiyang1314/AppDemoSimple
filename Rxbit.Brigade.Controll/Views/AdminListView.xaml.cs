using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class AdminListView : UserControl
    {
        public AdminListView()
        {
            InitializeComponent();
            DataContext = new AdminListViewModel();
        }
    }
}
