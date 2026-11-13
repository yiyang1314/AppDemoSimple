using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Rxbit.BrigadeApp.Views
{
    public partial class CanPlaybackView : WindowBase
    {
        private CanPlaybackViewModel _viewModel;
        private bool _isSliderDragging;

        public CanPlaybackView()
        {
            InitializeComponent();
            _viewModel = new ViewModels.CanPlaybackViewModel();
            DataContext = _viewModel;
        }

        /// <summary>
        /// 鼠标按下：开始拖拽进度条
        /// </summary>
        private void PlaybackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;
            _isSliderDragging = true;
            _viewModel.IsDragging = true;
            e.Handled = false; // 允许事件继续传递给 Slider
        }

        /// <summary>
        /// 鼠标释放：结束拖拽并跳转
        /// </summary>
        private void PlaybackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel == null) return;

            // 先获取当前滑块值
            double progress = PlaybackSlider.Value;

            // 结束拖拽状态
            _isSliderDragging = false;
            _viewModel.IsDragging = false;

            // 跳转到目标位置
            _viewModel.SeekToProgress(progress);

            e.Handled = false; // 允许事件继续传递给 Slider
        }

        /// <summary>
        /// 鼠标移动：拖拽过程中实时更新显示
        /// </summary>
        private void PlaybackSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null || !_isSliderDragging) return;

            var slider = sender as Slider;
            if (slider != null)
            {
                _viewModel.SliderValue = slider.Value;
            }
        }

        /// <summary>
        /// Thumb 拖拽开始
        /// </summary>
        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (_viewModel == null) return;
            _isSliderDragging = true;
            _viewModel.IsDragging = true;
        }

        /// <summary>
        /// Thumb 拖拽完成
        /// </summary>
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_viewModel == null) return;

            double progress = PlaybackSlider.Value;
            _isSliderDragging = false;
            _viewModel.IsDragging = false;
            _viewModel.SeekToProgress(progress);
        }

        /// <summary>
        /// Thumb 拖拽中：实时更新时间显示
        /// </summary>
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_viewModel == null) return;
            _viewModel.SliderValue = PlaybackSlider.Value;
        }
    }
}