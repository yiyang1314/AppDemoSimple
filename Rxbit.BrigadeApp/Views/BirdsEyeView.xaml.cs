using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Rxbit.BrigadeApp.Views
{
    public partial class BirdsEyeView : WindowBase
    {
        public BirdsEyeView()
        {
            InitializeComponent();
        }

        private void Ellipse_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ellipse = sender as Ellipse;
            if (ellipse != null)
            {
                var dataContext = ellipse.DataContext as DetectionPoint;
                if (dataContext != null)
                {
                    var vm = DataContext as BirdsEyeViewModel;
                    vm?.SelectPoint(dataContext.Id);
                }
            }
        }
    }
}
