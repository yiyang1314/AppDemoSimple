using Rxbit.BrigadeApp.Controls;
using System.Windows;

namespace Rxbit.BrigadeApp.Views
{
    public partial class Blf2AscWindow : WindowBase
    {
        public Blf2AscWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
