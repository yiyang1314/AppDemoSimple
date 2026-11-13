using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Rxbit.BrigadeApp.Views
{
    public partial class VideoMakerView : Controls.WindowBase
    {
        private VideoMakerViewModel _viewModel;
        private bool _isSliderDragging;

        public VideoMakerView()
        {
            InitializeComponent();
            _viewModel = new VideoMakerViewModel();
            DataContext = _viewModel;

            // 订阅文件选择事件
            _viewModel.SelectImageFolderRequested += OnSelectImagesRequested;
            _viewModel.SelectMp3FileRequested += OnSelectMp3Requested;
            _viewModel.SelectOutputFolderRequested += OnSelectOutputRequested;
            _viewModel.SelectFFmpegFileRequested += OnSelectFFmpegRequested;

            // 绑定播放器控制
            _viewModel.PlayVideoAction = () => VideoPlayer.Play();
            _viewModel.PauseVideoAction = () => VideoPlayer.Pause();
            _viewModel.StopVideoAction = () => { VideoPlayer.Stop(); };
            _viewModel.LoadVideoAction = (path) => VideoPlayer.Source = new Uri(path);
            _viewModel.UpdatePositionAction = () =>
            {
                if (!_isSliderDragging && VideoPlayer.NaturalDuration.HasTimeSpan)
                {
                    _viewModel.SetPosition(VideoPlayer.Position);
                }
            };
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel.SelectImageFolderRequested -= OnSelectImagesRequested;
            _viewModel.SelectMp3FileRequested -= OnSelectMp3Requested;
            _viewModel.SelectOutputFolderRequested -= OnSelectOutputRequested;
            _viewModel.SelectFFmpegFileRequested -= OnSelectFFmpegRequested;
            base.OnClosed(e);
        }

        /// <summary>
        /// 选择图片文件或文件夹
        /// </summary>
        private void OnSelectImagesRequested(string prompt)
        {
            // 提供两种选择方式：文件夹或文件
            var result = System.Windows.MessageBox.Show(
                "是 = 选择整个文件夹\n否 = 选择多个图片文件",
                prompt,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dialog.Description = "选择包含图片的文件夹";
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _viewModel.AddImageFolder(dialog.SelectedPath);
                    }
                }
            }
            else if (result == MessageBoxResult.No)
            {
                using (var dialog = new System.Windows.Forms.OpenFileDialog())
                {
                    dialog.Title = "选择图片文件";
                    dialog.Multiselect = true;
                    dialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*";
                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        _viewModel.AddImageFiles(dialog.FileNames);
                    }
                }
            }
        }

        /// <summary>
        /// 选择MP3文件
        /// </summary>
        private void OnSelectMp3Requested()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "选择MP3配乐文件";
                dialog.Filter = "MP3音频|*.mp3|所有音频|*.mp3;*.wav;*.aac;*.m4a|所有文件|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.SetMp3File(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// 选择输出目录
        /// </summary>
        private void OnSelectOutputRequested()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "选择视频输出目录";
                if (!string.IsNullOrEmpty(_viewModel.OutputPath) && System.IO.Directory.Exists(_viewModel.OutputPath))
                    dialog.SelectedPath = _viewModel.OutputPath;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.SetOutputPath(dialog.SelectedPath);
                }
            }
        }

        /// <summary>
        /// 手动选择 ffmpeg.exe 路径
        /// </summary>
        private void OnSelectFFmpegRequested()
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Title = "选择 ffmpeg.exe";
                dialog.Filter = "FFmpeg 可执行文件|ffmpeg.exe|所有可执行文件|*.exe|所有文件|*.*";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.SetFFmpegPath(dialog.FileName);
                }
            }
        }

        /// <summary>
        /// 媒体打开
        /// </summary>
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                _viewModel.SetTotalDuration(VideoPlayer.NaturalDuration.TimeSpan);
            }
        }

        /// <summary>
        /// 媒体播放结束
        /// </summary>
        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _viewModel.IsPlaying = false;
            VideoPlayer.Position = TimeSpan.Zero;
        }

        /// <summary>
        /// 进度条拖拽开始
        /// </summary>
        private void VideoSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isSliderDragging = true;
        }

        /// <summary>
        /// 进度条拖拽完成
        /// </summary>
        private void VideoSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                var pos = TimeSpan.FromTicks((long)(VideoPlayer.NaturalDuration.TimeSpan.Ticks * (_viewModel.PlayProgress / 100.0)));
                VideoPlayer.Position = pos;
            }
            _isSliderDragging = false;
        }
    }
}
