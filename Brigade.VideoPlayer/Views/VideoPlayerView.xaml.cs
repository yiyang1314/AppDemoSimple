using Brigade.VideoPlayer.Services;
using Brigade.VideoPlayer.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Brigade.VideoPlayer.Views
{
    public partial class VideoPlayerView : System.Windows.Controls.UserControl
    {
        private VideoPlayerViewModel _viewModel;
        private FFmpegService _ffmpegService;
        private bool _isSliderDragging;
        private bool _initialized;

        public VideoPlayerView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                _ffmpegService = new FFmpegService();
                _ffmpegService.DetectFFmpeg();

                _viewModel = new VideoPlayerViewModel(_ffmpegService);
                DataContext = _viewModel;

                _viewModel.LoadVideoAction = (path) => VideoPlayer.Source = new Uri(path);
                _viewModel.PlayAction = () => VideoPlayer.Play();
                _viewModel.PauseAction = () => VideoPlayer.Pause();
                _viewModel.StopAction = () => VideoPlayer.Stop();
                _viewModel.SetPositionAction = (pos) => VideoPlayer.Position = pos;
                _viewModel.SetVolumeAction = (v) => VideoPlayer.Volume = v;
                _viewModel.SetMuteAction = () => VideoPlayer.IsMuted = !VideoPlayer.IsMuted;
                _viewModel.GetPositionFunc = () => VideoPlayer.Position;
                _viewModel.GetDurationFunc = () =>
                    VideoPlayer.NaturalDuration.HasTimeSpan ? VideoPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero;

                _viewModel.OpenFileDialogRequested += OnOpenFileDialog;
                Focus();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"视频播放器初始化失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.OpenFileDialogRequested -= OnOpenFileDialog;
            Unloaded -= OnUnloaded;
        }

        private void OnOpenFileDialog()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "选择视频文件";
            dialog.Multiselect = true;
            dialog.Filter = "视频文件|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm;*.m4v;*.3gp|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                _viewModel.AddFiles(dialog.FileNames);
                if (_viewModel.Playlist.Count > 0 && string.IsNullOrEmpty(_viewModel.CurrentVideoPath))
                {
                    _viewModel.LoadVideo(_viewModel.Playlist[0]);
                }
            }
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                _viewModel.OnMediaOpened(VideoPlayer.NaturalDuration.TimeSpan);
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            _viewModel?.OnMediaEnded();
        }

        private void PlayPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            if (_viewModel.IsPlaying)
            {
                _viewModel.PauseCommand.Execute(null);
                PlayPauseBtn.Content = "播放";
                PlayPauseBtn.ToolTip = "播放 (空格)";
            }
            else
            {
                _viewModel.PlayCommand.Execute(null);
                PlayPauseBtn.Content = "暂停";
                PlayPauseBtn.ToolTip = "暂停 (空格)";
            }
        }

        private void VideoPlayerView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_viewModel == null) return;

            if (e.Key == Key.Space)
            {
                PlayPauseBtn_Click(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                OnOpenFileDialog();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp && _viewModel.PreviousCommand.CanExecute(null))
            {
                _viewModel.PreviousCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown && _viewModel.NextCommand.CanExecute(null))
            {
                _viewModel.NextCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isSliderDragging = true;
        }

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _viewModel?.SeekToProgress(ProgressSlider.Value);
            _isSliderDragging = false;
        }

        private void Playlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel?.CurrentVideoPath != null)
            {
                _viewModel.LoadVideo(_viewModel.CurrentVideoPath);
                _viewModel.PlayCommand.Execute(null);
                PlayPauseBtn.Content = "暂停";
            }
        }
    }
}
