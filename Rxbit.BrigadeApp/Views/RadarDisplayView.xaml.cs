using Rxbit.Brigade.Models.Helper;
using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Rxbit.BrigadeApp.Views
{
    public partial class RadarDisplayView : UserControl
    {
        public RadarDisplayView()
        {
            InitializeComponent();
            Loaded += RadarDisplayView_Loaded;
            SizeChanged += RadarDisplayView_SizeChanged;
        }

        private void RadarDisplayView_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
        }

        private void RadarDisplayView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            if (CanvasCorss == null) return;
            CanvasCorss.Children.Clear();
            DrawHelper.DrawArrowLine(CanvasCorss, "+X", new Point(60, 90),
                new Point(140, 90), 8, new SolidColorBrush(Color.FromRgb(255, 104, 126)), 2);

            DrawHelper.DrawArrowLine(CanvasCorss, "+Y", new Point(70, 100),
                new Point(70, 40), 8, new SolidColorBrush(Color.FromRgb(255, 104, 126)), 2);
        }

        private RadarDisplayViewModel GetViewModel()
        {
            return DataContext as RadarDisplayViewModel;
        }

        private void RadarCanvas_TargetSelected(object sender, RoutedEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null)
            {
                vm.TargetSelectedCommand.Execute(e.OriginalSource);
            }
        }

        private void RadarCanvas_CoordinateChanged(object sender, RoutedEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null && e.OriginalSource is System.Windows.Point worldPos)
            {
                vm.UpdateCoordinateText(worldPos);
            }
        }

        private void RadarCanvas_CanvasZoom(object sender, RoutedEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null && e.OriginalSource is Tuple<System.Windows.Point, int> data)
            {
                vm.CanvasMouseWheelCommand.Execute(data);
            }
        }

        private void RadarCanvas_CanvasDrag(object sender, RoutedEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null && e.OriginalSource is Tuple<System.Windows.Point, System.Windows.Point> data)
            {
                vm.CanvasMouseDragCommand.Execute(data);
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null && vm.ReplayVM != null && vm.ReplayVM.IsFileLoaded)
            {
                vm.ReplayVM.IsDragging = true;
            }
        }

        private void ProgressSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = GetViewModel();
            if (vm != null && vm.ReplayVM != null && vm.ReplayVM.IsDragging)
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
            var vm = GetViewModel();
            if (vm != null && vm.ReplayVM != null && vm.ReplayVM.IsDragging)
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
    public static class WatermarkHelper
    {
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached(
                "Watermark",
                typeof(string),
                typeof(WatermarkHelper),
                new PropertyMetadata(null, OnWatermarkChanged));

        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);
        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                // 移除旧事件
                tb.GotFocus -= OnGotFocus;
                tb.LostFocus -= OnLostFocus;
                tb.Loaded -= OnLoaded; // 确保已加载时刷新

                if (e.NewValue != null)
                {
                    tb.GotFocus += OnGotFocus;
                    tb.LostFocus += OnLostFocus;
                    tb.Loaded += OnLoaded;
                    // 初始设置水印
                    SetWatermarkText(tb);
                }
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            SetWatermarkText(tb);
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            string watermark = GetWatermark(tb);
            // 如果当前文本等于水印，清空并移除绑定（防止数据源恢复）
            if (tb.Text == watermark)
            {
                // 清除绑定，只修改本地值
                tb.SetCurrentValue(TextBox.TextProperty, string.Empty);
                tb.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private static void OnLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            SetWatermarkText(tb);
        }

        private static void SetWatermarkText(TextBox tb)
        {
            string watermark = GetWatermark(tb);
            // 当 Text 为空或 null 或仅空白时显示水印
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                // 使用 SetCurrentValue 避免破坏绑定
                tb.SetCurrentValue(TextBox.TextProperty, watermark);
                tb.Foreground = Brushes.Gray;
            }
            else
            {
                // 如果有实际内容，确保前景色正常
                if (tb.Foreground != SystemColors.ControlTextBrush)
                    tb.Foreground = SystemColors.ControlTextBrush;
            }
        }
    }
}