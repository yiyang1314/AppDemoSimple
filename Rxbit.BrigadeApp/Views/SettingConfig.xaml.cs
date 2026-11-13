using Rxbit.BrigadeApp.Controls;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// SettingSensor.xaml 的交互逻辑
    /// </summary>
    public partial class SettingSensor : WindowBase
    {
        public SettingSensor()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
