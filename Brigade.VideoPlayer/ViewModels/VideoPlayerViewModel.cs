using Brigade.VideoPlayer.Commands;
using Brigade.VideoPlayer.Models;
using Brigade.VideoPlayer.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Brigade.VideoPlayer.ViewModels
{
    public class VideoPlayerViewModel : ViewModelBase
    {
        private readonly FFmpegService _ffmpegService;
        private ObservableCollection<string> _playlist;
        private string _currentVideoPath;
        private string _currentVideoName;
        private bool _isLoaded;
        private bool _isPlaying;
        private bool _isPaused;
        private TimeSpan _currentPosition;
        private TimeSpan _totalDuration;
        private double _playProgress;
        private double _volume = 0.8;
        private bool _isMuted;
        private int _currentIndex;
        private DispatcherTimer _positionTimer;

        // 命令
        public ICommand OpenFileCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand RemoveFromListCommand { get; }
        public ICommand ClearListCommand { get; }

        // 播放器控制委托
        public Action<string> LoadVideoAction { get; set; }
        public Action PlayAction { get; set; }
        public Action PauseAction { get; set; }
        public Action StopAction { get; set; }
        public Action<TimeSpan> SetPositionAction { get; set; }
        public Action<double> SetVolumeAction { get; set; }
        public Action SetMuteAction { get; set; }
        public Func<TimeSpan> GetPositionFunc { get; set; }
        public Func<TimeSpan> GetDurationFunc { get; set; }

        // 事件
        public Action OpenFileDialogRequested;

        public ObservableCollection<string> Playlist
        {
            get => _playlist;
            set => Set(ref _playlist, value);
        }

        public string CurrentVideoPath
        {
            get => _currentVideoPath;
            set => Set(ref _currentVideoPath, value);
        }

        public string CurrentVideoName
        {
            get => _currentVideoName;
            set => Set(ref _currentVideoName, value);
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (Set(ref _isLoaded, value))
                {
                    OnPropertyChanged(nameof(CanPlay));
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => Set(ref _isPaused, value);
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set
            {
                Set(ref _currentPosition, value);
                OnPropertyChanged(nameof(CurrentPositionStr));
                if (_totalDuration.TotalSeconds > 0)
                    PlayProgress = (value.TotalSeconds / _totalDuration.TotalSeconds) * 100;
            }
        }

        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            set
            {
                Set(ref _totalDuration, value);
                OnPropertyChanged(nameof(TotalDurationStr));
            }
        }

        public double PlayProgress
        {
            get => _playProgress;
            set => Set(ref _playProgress, value);
        }

        public double Volume
        {
            get => _volume;
            set
            {
                if (Set(ref _volume, value))
                {
                    SetVolumeAction?.Invoke(value);
                }
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (Set(ref _isMuted, value))
                {
                    SetMuteAction?.Invoke();
                }
            }
        }

        public string CurrentPositionStr => _currentPosition.ToString(@"mm\:ss");
        public string TotalDurationStr => _totalDuration.ToString(@"mm\:ss");

        public bool CanPlay => _isLoaded && !_isPlaying;
        public bool CanPause => _isPlaying;
        public bool CanNext => _playlist.Count > 0 && _currentIndex < _playlist.Count - 1;
        public bool CanPrevious => _playlist.Count > 0 && _currentIndex > 0;

        public VideoPlayerViewModel(FFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
            _playlist = new ObservableCollection<string>();

            _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _positionTimer.Tick += PositionTimer_Tick;

            OpenFileCommand = new RelayCommand(OpenFile);
            PlayCommand = new RelayCommand(Play, () => CanPlay);
            PauseCommand = new RelayCommand(Pause, () => CanPause);
            StopCommand = new RelayCommand(Stop, () => _isLoaded);
            NextCommand = new RelayCommand(Next, () => CanNext);
            PreviousCommand = new RelayCommand(Previous, () => CanPrevious);
            RemoveFromListCommand = new RelayCommand<string>(RemoveFromList);
            ClearListCommand = new RelayCommand(ClearList, () => _playlist.Count > 0);
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (GetPositionFunc != null)
            {
                CurrentPosition = GetPositionFunc();
            }
        }

        private void OpenFile()
        {
            OpenFileDialogRequested?.Invoke();
        }

        public void AddFiles(string[] files)
        {
            var videoExts = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".3gp" };
            foreach (var file in files)
            {
                if (videoExts.Contains(Path.GetExtension(file).ToLower()))
                    _playlist.Add(file);
            }
        }

        private void Play()
        {
            PlayAction?.Invoke();
            IsPlaying = true;
            IsPaused = false;
            _positionTimer.Start();
        }

        private void Pause()
        {
            PauseAction?.Invoke();
            IsPlaying = false;
            IsPaused = true;
            _positionTimer.Stop();
        }

        private void Stop()
        {
            StopAction?.Invoke();
            IsPlaying = false;
            IsPaused = false;
            _positionTimer.Stop();
            CurrentPosition = TimeSpan.Zero;
        }

        private void Next()
        {
            if (_currentIndex < _playlist.Count - 1)
            {
                _currentIndex++;
                LoadVideo(_playlist[_currentIndex]);
            }
        }

        private void Previous()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                LoadVideo(_playlist[_currentIndex]);
            }
        }

        private void RemoveFromList(string path)
        {
            if (_playlist.Contains(path))
            {
                _playlist.Remove(path);
                if (path == _currentVideoPath)
                    Stop();
            }
        }

        private void ClearList()
        {
            _playlist.Clear();
            Stop();
            _currentIndex = 0;
            IsLoaded = false;
        }

        public void LoadVideo(string path)
        {
            CurrentVideoPath = path;
            CurrentVideoName = Path.GetFileName(path);
            IsLoaded = true;
            IsPlaying = false;
            IsPaused = false;
            CurrentPosition = TimeSpan.Zero;
            _currentIndex = _playlist.IndexOf(path);

            LoadVideoAction?.Invoke(path);
        }

        public void OnMediaOpened(TimeSpan duration)
        {
            TotalDuration = duration;
        }

        public void OnMediaEnded()
        {
            IsPlaying = false;
            IsPaused = false;
            _positionTimer.Stop();
            CurrentPosition = TimeSpan.Zero;

            // 自动播放下一个
            if (_currentIndex < _playlist.Count - 1)
                Next();
        }

        public void SeekToProgress(double progress)
        {
            if (_totalDuration.TotalSeconds > 0)
            {
                var pos = TimeSpan.FromTicks((long)(_totalDuration.Ticks * progress / 100.0));
                SetPositionAction?.Invoke(pos);
                CurrentPosition = pos;
            }
        }
    }
}
