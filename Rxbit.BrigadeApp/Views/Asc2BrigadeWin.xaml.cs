using Rxbit.BrigadeApp.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// Asc2BrigadeWin.xaml 的交互逻辑
    /// </summary>
    public partial class Asc2BrigadeWin : WindowBase
    {
        public Asc2BrigadeWin()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
