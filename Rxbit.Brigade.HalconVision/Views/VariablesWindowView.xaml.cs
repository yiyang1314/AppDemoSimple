using System.Windows;
using System.Windows.Controls;

namespace Rxbit.Brigade.HalconVision.Views
{
    public partial class VariablesWindowView : UserControl
    {
        public VariablesWindowView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
        }
    }
}