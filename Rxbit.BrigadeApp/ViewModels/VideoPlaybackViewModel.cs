using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Services.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    /// <summary>
    /// 界面视频回放窗口的视图模型：打开录制生成的 AVI(MJPEG)，播放/暂停/停止与进度拖动。
    /// </summary>
    public class VideoPlaybackViewModel : ViewModelBase, IDisposable
    {
        private readonly DispatcherTimer _playTimer;

        private AviMjpegReader _reader;
        private BitmapSource _currentFrame;
        private bool _isPlaying;
        private bool _suppressSeek;
        private bool _disposed;
        private int _currentIndex;
        private double _positionValue;
        private string _filePath = string.Empty;
        private string _infoText = "--";
        private string _positionText = "00:00:00";
        private string _durationText = "00:00:00";
        private string _statusMessage = "选择已录制的视频文件（AVI / MJPEG）开始回放。";

        public VideoPlaybackViewModel()
        {
            OutputFolder = Path.Combine(RadarConstaint.RecondingPath, "Media");

            OpenCommand = new RelayCommand(OpenFile);
            PlayCommand = new RelayCommand(Play, () => HasFile && !IsPlaying);
            PauseCommand = new RelayCommand(Pause, () => IsPlaying);
            StopCommand = new RelayCommand(StopPlayback, () => HasFile);
            OpenFolderCommand = new RelayCommand(OpenOutputFolder);

            _playTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(100) };
            _playTimer.Tick += PlayTimer_Tick;
        }

        public string OutputFolder { get; }

        public bool HasFile => _reader != null;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (SetProperty(ref _isPlaying, value))
                {
                    OnPropertyChanged(nameof(PlaybackStateText));
                    // 播放状态变化后立即刷新按钮可用状态，避免依赖输入事件才重新求值
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string PlaybackStateText => IsPlaying ? "● PLAYING" : (HasFile ? "PAUSED" : "IDLE");

        public BitmapSource CurrentFrame
        {
            get => _currentFrame;
            private set => SetProperty(ref _currentFrame, value);
        }

        public string FilePath
        {
            get => _filePath;
            private set => SetProperty(ref _filePath, value);
        }

        public string InfoText
        {
            get => _infoText;
            private set => SetProperty(ref _infoText, value);
        }

        public string PositionText
        {
            get => _positionText;
            private set => SetProperty(ref _positionText, value);
        }

        public string DurationText
        {
            get => _durationText;
            private set => SetProperty(ref _durationText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public double PositionMaximum
        {
            get => _reader == null ? 0 : Math.Max(0, _reader.FrameCount - 1);
        }

        /// <summary>
        /// 进度条位置（以帧索引为单位），拖动时直接定位到对应帧。
        /// </summary>
        public double PositionValue
        {
            get => _positionValue;
            set
            {
                if (!SetProperty(ref _positionValue, value)) return;
                if (_suppressSeek || _reader == null) return;
                ShowFrame((int)Math.Round(value));
            }
        }

        public ICommand OpenCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenFolderCommand { get; }

        private void OpenFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择录制视频",
                Filter = "AVI 视频 (*.avi)|*.avi|所有文件 (*.*)|*.*",
                CheckFileExists = true
            };

            if (Directory.Exists(OutputFolder))
            {
                dialog.InitialDirectory = OutputFolder;
            }

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            LoadFile(dialog.FileName);
        }

        /// <summary>
        /// 载入视频文件并显示首帧；失败时保留当前状态并给出提示。
        /// </summary>
        public void LoadFile(string path)
        {
            StopPlayback();
            ReleaseReader();

            try
            {
                var reader = new AviMjpegReader(path);
                if (reader.FrameCount <= 0)
                {
                    reader.Dispose();
                    StatusMessage = "该视频文件不包含可播放的画面帧。";
                    return;
                }

                _reader = reader;
                _currentIndex = 0;
                FilePath = path;
                InfoText = $"{reader.Width}×{reader.Height} · {reader.FrameRate:0.##} fps · {reader.FrameCount} frames";
                DurationText = FormatTime(reader.FrameCount, reader.FrameRate);
                PositionMaximumRefresh();
                ShowFrame(0);
                StatusMessage = "已载入：" + Path.GetFileName(path);
                OnPropertyChanged(nameof(HasFile));
                OnPropertyChanged(nameof(PlaybackStateText));
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                ReleaseReader();
                StatusMessage = "打开视频失败：" + ex.Message;
            }
        }

        private void PositionMaximumRefresh()
        {
            OnPropertyChanged(nameof(PositionMaximum));
            _suppressSeek = true;
            PositionValue = 0;
            _suppressSeek = false;
        }

        private void Play()
        {
            if (_reader == null) return;

            if (_currentIndex >= _reader.FrameCount - 1)
            {
                ShowFrame(0);
            }

            double fps = _reader.FrameRate > 0 ? _reader.FrameRate : 10;
            _playTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(30, 1000 / fps));
            IsPlaying = true;
            _playTimer.Start();
            StatusMessage = "正在播放：" + Path.GetFileName(FilePath);
        }

        private void Pause()
        {
            _playTimer.Stop();
            if (!IsPlaying) return;
            IsPlaying = false;
            StatusMessage = "已暂停。";
        }

        private void StopPlayback()
        {
            _playTimer.Stop();
            IsPlaying = false;
            if (_reader == null) return;
            ShowFrame(0);
            StatusMessage = "已停止。";
        }

        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            if (_reader == null)
            {
                Pause();
                return;
            }

            int next = _currentIndex + 1;
            if (next >= _reader.FrameCount)
            {
                _playTimer.Stop();
                IsPlaying = false;
                StatusMessage = "播放结束。";
                return;
            }

            ShowFrame(next);
        }

        private void ShowFrame(int index)
        {
            if (_reader == null) return;
            if (index < 0) index = 0;
            if (index >= _reader.FrameCount) index = _reader.FrameCount - 1;

            var frame = _reader.GetFrame(index);
            if (frame != null)
            {
                CurrentFrame = frame;
            }

            _currentIndex = index;
            _suppressSeek = true;
            PositionValue = index;
            _suppressSeek = false;
            PositionText = FormatTime(index, _reader.FrameRate);
        }

        private static string FormatTime(int frames, double frameRate)
        {
            if (frameRate <= 0) return "00:00:00";
            var seconds = (int)Math.Round(frames / frameRate);
            var span = TimeSpan.FromSeconds(seconds);
            return span.ToString(@"hh\:mm\:ss");
        }

        private void OpenOutputFolder()
        {
            try
            {
                Directory.CreateDirectory(OutputFolder);
                Process.Start("explorer.exe", OutputFolder);
            }
            catch (Exception ex)
            {
                StatusMessage = "打开目录失败：" + ex.Message;
            }
        }

        private void ReleaseReader()
        {
            if (_reader == null) return;
            try
            {
                _reader.Dispose();
            }
            catch
            {
                // 释放失败时忽略，避免影响窗口关闭
            }
            _reader = null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _playTimer.Stop();
            _playTimer.Tick -= PlayTimer_Tick;
            ReleaseReader();
        }
    }
}
