using System.Windows.Controls;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class AdminIconFontView : UserControl
    {
        public AdminIconFontView()
        {
            InitializeComponent();
            DataContext = new AdminIconFontViewModel();
        }
    }
}
