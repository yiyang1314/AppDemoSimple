using Rxbit.Brigade.Models.Helper;
using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// RadarDisplayWin.xaml 的交互逻辑
    /// </summary>
    public partial class RadarDisplayWin : WindowBase
    {

        public RadarDisplayWin()
        {
            InitializeComponent();
            Update();
            SizeChanged += RadarDisplayWin_SizeChanged;
        }

        private void RadarDisplayWin_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }
        private void Update()
        {
            CanvasCorss.Children.Clear();
            DrawHelper.DrawArrowLine(CanvasCorss, "+X", new Point(60, 90),
                new Point(140, 90), 8, new SolidColorBrush(Color.FromRgb(255, 104, 126)), 2);

            DrawHelper.DrawArrowLine(CanvasCorss, "+Y", new Point(70, 100),
                new Point(70, 40), 8, new SolidColorBrush(Color.FromRgb(255, 104, 126)), 2);
        }

        /// <summary>
        /// 目标选中事件 - 转发给ViewModel
        /// </summary>
        private void RadarCanvas_TargetSelected(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm)
            {
                vm.TargetSelectedCommand.Execute(e.OriginalSource);
            }
        }

        /// <summary>
        /// 坐标变化事件 - 转发给ViewModel
        /// </summary>
        private void RadarCanvas_CoordinateChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && e.OriginalSource is System.Windows.Point worldPos)
            {
                vm.UpdateCoordinateText(worldPos);
            }
        }

        /// <summary>
        /// 画布缩放事件 - 转发给ViewModel
        /// </summary>
        private void RadarCanvas_CanvasZoom(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && e.OriginalSource is Tuple<System.Windows.Point, int> data)
            {
                vm.CanvasMouseWheelCommand.Execute(data);
            }
        }

        /// <summary>
        /// 画布拖拽事件 - 转发给ViewModel
        /// </summary>
        private void RadarCanvas_CanvasDrag(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && e.OriginalSource is Tuple<System.Windows.Point, System.Windows.Point> data)
            {
                vm.CanvasMouseDragCommand.Execute(data);
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && vm.ReplayVM != null && vm.ReplayVM.IsFileLoaded)
            {
                vm.ReplayVM.IsDragging = true;
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && vm.ReplayVM != null && vm.ReplayVM.IsDragging)
            {
                var slider = sender as Slider;
                if (slider != null)
                {
                    vm.ReplayVM.SeekToProgress(slider.Value);
                }
                vm.ReplayVM.IsDragging = false;
            }
        }

        private void ProgressSlider_LostMouseCapture(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarDisplayViewModel vm && vm.ReplayVM != null && vm.ReplayVM.IsDragging)
            {
                var slider = sender as Slider;
                if (slider != null)
                {
                    vm.ReplayVM.SeekToProgress(slider.Value);
                }
                vm.ReplayVM.IsDragging = false;
            }
        }
    }
}
