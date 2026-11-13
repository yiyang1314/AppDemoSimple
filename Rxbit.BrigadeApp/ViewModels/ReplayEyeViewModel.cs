using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.IServices;
using Rxbit.Brigade.CanDevices.Structs;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using Rxbit.BrigadeApp.Models;
using Rxbit.BrigadeApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class ReplayEyeViewModel : ViewModelBase
    {
        #region 私有字段

        private readonly CanReplayService canReplayService;
        private readonly DataMediatorService<RadarTargetDto> dataMediator;
        private readonly List<UInt64> sortedTimestamps;
        private int _currentTimestampIndex;

        // 帧率驱动的播放时钟
        private Stopwatch _playbackStopwatch;
        private int _playStartFrameIndex;
        private double _avgFrameIntervalMs;
        private bool _mediatorOpen;

        private double _totalDurationMs;
        private double _currentTimeMs;
        private double _progress;
        private double _playbackSpeed;
        private int _currentFrame;
        private string _currentTimestamp;
        private string _totalDurationStr;
        private string _currentDurationStr;
        private string _fileName;
        private bool _isFileLoaded;
        private bool _isDragging;
        private bool _wasPlayingBeforeDrag;

        private DispatcherTimer _playbackTimer;
        private const double TickIntervalMs = 16.0;

        private static readonly double[] SpeedLevels = { 0.5, 1.0, 1.5, 2.0, 3.0, 5.0, 10.0 };
        private int _speedLevelIndex;

        #endregion

        #region 播放状态属性
        public SensorInfo SavePreSensor
        {
            get => _preSensor;
            set
            {
                SetProperty(ref _preSensor, value);
            }
        }
        private SensorInfo _preSensor;
        private bool _isPlaying;
        private bool _isPaused;

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (Set(ref _isPlaying, value))
                {
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (Set(ref _isPaused, value))
                {
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                }
            }
        }

        public double TotalDurationMs
        {
            get => _totalDurationMs;
            private set
            {
                if (Set(ref _totalDurationMs, value))
                {
                    OnPropertyChanged(nameof(TotalDurationStr));
                }
            }
        }

        public double CurrentTimeMs
        {
            get => _currentTimeMs;
            set
            {
                if (Set(ref _currentTimeMs, value))
                {
                    UpdateTimestampDisplay();
                    UpdateProgress();
                    UpdateCurrentFrame();
                }
            }
        }

        public double Progress
        {
            get => _progress;
            set
            {
                if (_isDragging)
                {
                    _progress = value;
                    var targetTime = (value / 100.0) * TotalDurationMs;
                    CurrentTimestamp = GetTimeAt(targetTime).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    CurrentDurationStr = FormatTimeSpan(TimeSpan.FromMilliseconds(targetTime));
                    OnPropertyChanged(nameof(CurrentTimestamp));
                    OnPropertyChanged(nameof(CurrentDurationStr));
                }
                else
                {
                    Set(ref _progress, value);
                }
            }
        }

        public bool IsDragging
        {
            get => _isDragging;
            set
            {
                if (Set(ref _isDragging, value))
                {
                    if (value)
                    {
                        _wasPlayingBeforeDrag = IsPlaying;
                        if (IsPlaying)
                        {
                            _playbackTimer?.Stop();
                            _playbackStopwatch?.Stop();
                            IsPlaying = false;
                            IsPaused = true;
                            OnPropertyChanged(nameof(PlayPauseButtonText));
                        }
                    }
                    else
                    {
                        if (_wasPlayingBeforeDrag && IsFileLoaded && _currentTimestampIndex < sortedTimestamps.Count - 1)
                        {
                            StartPlaybackClock();
                            RestartTimer();
                            IsPlaying = true;
                            IsPaused = false;
                            OnPropertyChanged(nameof(PlayPauseButtonText));
                        }
                    }
                }
            }
        }

        public double PlaybackSpeed
        {
            get => _playbackSpeed;
            set
            {
                if (Set(ref _playbackSpeed, value))
                {
                    OnPropertyChanged(nameof(SpeedDisplay));
                    OnPropertyChanged(nameof(SpeedButtonText));
                    // 速度变化时重置播放时钟，避免时间漂移
                    if (IsPlaying)
                    {
                        StartPlaybackClock();
                    }
                }
            }
        }

        public int CurrentFrame
        {
            get => _currentFrame;
            set => Set(ref _currentFrame, value);
        }

        public string CurrentTimestamp
        {
            get => _currentTimestamp;
            set => Set(ref _currentTimestamp, value);
        }

        public string TotalDurationStr
        {
            get => _totalDurationStr;
            set => Set(ref _totalDurationStr, value);
        }

        public string CurrentDurationStr
        {
            get => _currentDurationStr;
            set => Set(ref _currentDurationStr, value);
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (Set(ref _fileName, value))
                {
                    OnPropertyChanged(nameof(FileNameDisplay));
                }
            }
        }

        public bool IsFileLoaded
        {
            get => _isFileLoaded;
            set
            {
                if (Set(ref _isFileLoaded, value))
                {
                    OnPropertyChanged(nameof(CanPlay));
                    OnPropertyChanged(nameof(StatusIndicatorColor));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public Brush StatusIndicatorColor => IsFileLoaded ? Brushes.LimeGreen : Brushes.Gray;

        public string FileNameDisplay => string.IsNullOrEmpty(FileName) ? "Load data Failed!" : Path.GetFileName(FileName);

        public string SpeedDisplay => $"{PlaybackSpeed:F1}X";

        public string SpeedButtonText => $"{PlaybackSpeed:F1}X";

        public bool CanPlay => IsFileLoaded;

        #endregion

        #region 按钮文本

        public string PlayPauseButtonText => IsPlaying ? "\u23F8" : "\u25B6";

        #endregion

        #region 命令

        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RewindCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand PrevFrameCommand { get; }
        public ICommand NextFrameCommand { get; }
        public ICommand CycleSpeedCommand { get; }
        public AsyncRelayCommand SelectFileCommand { get; }

        #endregion

        public ReplayEyeViewModel()
        {
            _speedLevelIndex = 1;
            PlaybackSpeed = SpeedLevels[_speedLevelIndex];
            IsFileLoaded = false;

            TotalDurationStr = "00:00:00";
            CurrentDurationStr = "00:00:00";
            CurrentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            PlayPauseCommand = new RelayCommand(TogglePlayPause, () => CanPlay);
            StopCommand = new RelayCommand(StopPlayback);
            RewindCommand = new RelayCommand(Rewind, () => CanPlay);
            ForwardCommand = new RelayCommand(Forward, () => CanPlay);
            PrevFrameCommand = new RelayCommand(PrevFrame, () => CanPlay);
            NextFrameCommand = new RelayCommand(NextFrame, () => CanPlay);
            CycleSpeedCommand = new RelayCommand(CycleSpeed);
            SelectFileCommand = new AsyncRelayCommand(SelectAscFile);

            canReplayService = ViewModelLocator.GetService<CanReplayService>();
            var parser = ViewModelLocator.GetService<BrigadeCanMessageParser>();
            canReplayService.AddParseService(parser);
            canReplayService.CanDataComed -= parser.Enqueue;
            canReplayService.CanDataComed += parser.Enqueue;
            dataMediator = ViewModelLocator.GetService<DataMediatorService<RadarTargetDto>>();

            sortedTimestamps = new List<UInt64>();
            _playbackStopwatch = new Stopwatch();
        }

        #region 文件选择

        private async Task SelectAscFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Config Files (*.config)|*.config|All Files (*.*)|*.*",
                Title = "Select Config Playback File",
                Multiselect = false,
                InitialDirectory = RadarConstaint.RecondingPath
            };

            if (!string.IsNullOrEmpty(FileName))
            {
                try
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(FileName);
                }
                catch { }
            }

            if (dialog.ShowDialog() == true)
            {
                LoadAscFile(dialog.FileName);
            }
        }

        private void LoadAscFile(string filePath)
        {
            try
            {
                
                IsFileLoaded = true;
                StopPlayback();
                var bus = ConfigHelper.ReadConfig<BrigadeConfigBus>(Path.Combine(Path.GetDirectoryName(filePath),RadarConstaint.BUSFile));
                if (bus == null) return;
                var config = canReplayService.Init(filePath, bus);
                if (config != null)
                {
                    FileName = config.File;
                    var sensor = ViewModelLocator.GetService<SensorConfigurationViewModel>();
                    if (this.SavePreSensor == null)
                    {
                        this.SavePreSensor = sensor.CurrSensor;
                    }
                    sensor.NotifySensorChange(bus.Sensor.CanID);
                    sortedTimestamps.Clear();
                    sortedTimestamps.AddRange(canReplayService.GetAvailableTimestamps());
                    SeekStepMs = _speedLevelIndex == 1?1:(config.LastFrameTime - config.FirstFrameTime) / 10;
                    if (sortedTimestamps.Count > 0)
                    {
                        UInt64 firstTs = sortedTimestamps[0];
                        UInt64 lastTs = sortedTimestamps[sortedTimestamps.Count - 1];
                        TotalDurationMs = (double)(lastTs - firstTs);
                        _avgFrameIntervalMs = TotalDurationMs / sortedTimestamps.Count;
                    }
                    else
                    {
                        TotalDurationMs = 0;
                        _avgFrameIntervalMs = 0;
                    }

                    TotalDurationStr = FormatTimeSpan(TimeSpan.FromMilliseconds(TotalDurationMs));
                    _currentTimestampIndex = 0;
                    CurrentTimeMs = 0;
                    CurrentDurationStr = "00:00:00";
                    CurrentTimestamp = File.GetCreationTime(filePath).ToString("yyyy/MM/dd HH:mm:ss.fff");
                    Progress = 0;
                    CurrentFrame = 0;

                    // 显示首帧
                    if (sortedTimestamps.Count > 0)
                    {
                        EnsureMediatorOpen();
                        canReplayService.GetFrame(sortedTimestamps[0]);
                    }
                }
                else
                {
                    MessageBox.Show("File load failed: Unable to parse CAN data", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    IsFileLoaded = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IsFileLoaded = false;
            }
        }

        #endregion

        #region 速度切换

        private void CycleSpeed()
        {
            int pos  = (_speedLevelIndex+1) % SpeedLevels.Length;
            PlaybackSpeed = SpeedLevels[pos];
            _speedLevelIndex = pos;
        }

        #endregion

        #region 播放控制

        /// <summary>
        /// 启动播放时钟：以当前帧为起点，Stopwatch重新计时
        /// </summary>
        private void StartPlaybackClock()
        {
            _playStartFrameIndex = _currentTimestampIndex;
            _playbackStopwatch.Restart();
        }

        /// <summary>
        /// 确保mediator推送循环已启动（仅首次调用时启动，避免重复Open清空队列）
        /// </summary>
        private void EnsureMediatorOpen()
        {
            if (!_mediatorOpen)
            {
                dataMediator.Open();
                _mediatorOpen = true;
            }
        }

        /// <summary>
        /// 重新打开mediator（清空旧帧队列，用于seek场景）
        /// </summary>
        private void ReopenMediatorForSeek()
        {
            dataMediator.Open();
            _mediatorOpen = true;
        }

        private void TogglePlayPause()
        {
            if (!CanPlay) return;

            if (IsPlaying)
            {
                _playbackTimer?.Stop();
                _playbackStopwatch?.Stop();
                IsPlaying = false;
                IsPaused = true;
                OnPropertyChanged(nameof(PlayPauseButtonText));
            }
            else
            {
                // 到达末尾则从头开始
                if (_currentTimestampIndex >= sortedTimestamps.Count - 1)
                {
                    _currentTimestampIndex = 0;
                    CurrentTimeMs = 0;
                    Progress = 0;
                }

                StartPlaybackClock();
                RestartTimer();
                IsPlaying = true;
                IsPaused = false;
                OnPropertyChanged(nameof(PlayPauseButtonText));
            }
        }

        private void StopPlayback()
        {
            _playbackTimer?.Stop();
            _playbackStopwatch?.Stop();
            IsPlaying = false;
            IsPaused = false;
            OnPropertyChanged(nameof(PlayPauseButtonText));
            _currentTimestampIndex = 0;
            CurrentTimeMs = 0;
            Progress = 0;
            CurrentDurationStr = "00:00:00";
            if (_mediatorOpen)
            {
                dataMediator.Close();
                _mediatorOpen = false;
            }
        }

        private double SeekStepMs = 1.0; // 快进/后退10秒

        private void Rewind()
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            double targetTime = Math.Max(0, CurrentTimeMs - PlaybackSpeed * 1000);
            SeekToTime(targetTime);
        }

        private void Forward()
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            double targetTime = Math.Min(TotalDurationMs, CurrentTimeMs + PlaybackSpeed * 1000);
            SeekToTime(targetTime);
        }

        private void PrevFrame()
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            _currentTimestampIndex = Math.Max(0, _currentTimestampIndex - 1);
            var ts = sortedTimestamps[_currentTimestampIndex];
            UInt64 firstTs = sortedTimestamps[0];
            CurrentTimeMs = (double)(ts - firstTs);

            ReopenMediatorForSeek();
            canReplayService.GetFrame(ts);

            if (IsPlaying)
            {
                StartPlaybackClock();
            }
        }

        private void NextFrame()
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            _currentTimestampIndex = Math.Min(sortedTimestamps.Count - 1, _currentTimestampIndex + 1);
            var ts = sortedTimestamps[_currentTimestampIndex];
            UInt64 firstTs = sortedTimestamps[0];
            CurrentTimeMs = (double)(ts - firstTs);

            ReopenMediatorForSeek();
            canReplayService.GetFrame(ts);

            if (IsPlaying)
            {
                StartPlaybackClock();
            }
        }

        private void SeekToTime(double relTimeMs)
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            UInt64 firstTs = sortedTimestamps[0];
            UInt64 targetAbs = firstTs + (UInt64)relTimeMs;

            int index = sortedTimestamps.BinarySearch(targetAbs);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= sortedTimestamps.Count)
            {
                index = sortedTimestamps.Count - 1;
            }

            _currentTimestampIndex = index;
            var actualTs = sortedTimestamps[index];
            CurrentTimeMs = (double)(actualTs - firstTs);

            ReopenMediatorForSeek();
            canReplayService.GetFrame(actualTs);

            if (!IsPaused)
            {
                StartPlaybackClock();
            }
        }

        public void SeekToProgress(double progress)
        {
            if (!CanPlay || sortedTimestamps.Count == 0) return;

            var targetTime = (progress / 100.0) * TotalDurationMs;
            SeekToTime(targetTime);
        }

        /// <summary>
        /// 帧号驱动的播放Tick：基于Stopwatch权威时钟计算目标帧号
        /// </summary>
        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (_isDragging || !IsPlaying || sortedTimestamps.Count == 0 || _avgFrameIntervalMs <= 0) return;
            if (_isDragging && !IsPaused) 
            {
                return;
            }
            try
            {
                // 权威时钟：实际经过的时间
                double elapsedMs = _playbackStopwatch.Elapsed.TotalMilliseconds;

                // 帧号驱动：目标帧 = 起始帧 + (经过时间 × 播放速度 / 平均帧间隔)
                int targetIndex = _playStartFrameIndex + (int)((elapsedMs * PlaybackSpeed) / _avgFrameIntervalMs);

                if (targetIndex >= sortedTimestamps.Count)
                {
                    targetIndex = sortedTimestamps.Count - 1;
                }

                // 仅当目标帧超过当前帧时才推进（避免回退）
                if (targetIndex > _currentTimestampIndex)
                {
                    _currentTimestampIndex = targetIndex;
                    var ts = sortedTimestamps[_currentTimestampIndex];
                    UInt64 firstTs = sortedTimestamps[0];
                    CurrentTimeMs = (double)(ts - firstTs);
                    // 不调用dataMediator.Open()——推送循环已在播放开始时启动
                    // 每次Open()会Clear()队列，导致正在处理的帧被丢弃
                    canReplayService.GetFrame(ts);
                }

                // 播放完成
                if (_currentTimestampIndex >= sortedTimestamps.Count - 1)
                {
                    _playbackTimer?.Stop();
                    _playbackStopwatch?.Stop();
                    IsPlaying = false;
                    IsPaused = false;
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PlaybackTimer_Tick error: {ex.Message}");
            }
        }

        private void RestartTimer()
        {
            if (string.IsNullOrEmpty(FileName)) return;
            _playbackTimer?.Stop();
            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TickIntervalMs)
            };
            _playbackTimer.Tick += PlaybackTimer_Tick;
            _playbackTimer.Start();
        }

        #endregion

        #region Time/Progress Updates

        private void UpdateTimestampDisplay()
        {
            var currentTime = GetTimeAt(CurrentTimeMs);
            CurrentTimestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            CurrentDurationStr = FormatTimeSpan(TimeSpan.FromMilliseconds(CurrentTimeMs));
        }

        private void UpdateProgress()
        {
            Progress = TotalDurationMs > 0 ? (CurrentTimeMs / TotalDurationMs) * 100 : 0;
        }

        private void UpdateCurrentFrame()
        {
            if (sortedTimestamps.Count > 0)
            {
                CurrentFrame = _currentTimestampIndex + 1;
            }
        }

        private DateTime GetTimeAt(double timeMs)
        {
            var baseTime = DateTime.Now.AddMilliseconds(-TotalDurationMs);
            return baseTime.AddMilliseconds(timeMs);
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        #endregion
    }
}
