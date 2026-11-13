using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.Models;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class CanPlaybackViewModel : ViewModelBase
    {
        // 回放数据参数
        private const int TotalFrames = 1000000;      // 20万帧
        private const double TotalDurationMs = 20000; // 200秒 (每帧1ms)
        private int step = 1;
        // 播放速度档位
        private static readonly double[] SpeedLevels = { 0.5,1.0, 2.0, 5.0, 10.0, 20.0 };
        private int _speedLevelIndex;

        // 播放状态
        private bool _isPlaying;
        private bool _isPaused;
        private double _currentTimeMs;     // 当前时间(ms)
        private double _progress;          // 进度百分比 (0-100)
        private double _playbackSpeed;     // 播放速度
        private int _currentFrame;         // 当前帧索引
        private string _currentTimestamp;  // 当前时间戳显示
        private string _totalDurationStr;  // 总时长显示
        private string _currentDurationStr; // 当前时长显示
        private string _fileName;          // 当前加载的文件名
        private bool _isFileLoaded;        // 是否已加载文件
        private bool _isDragging;         // 是否正在拖拽进度条
        private bool _wasPlayingBeforeDrag; // 拖拽前是否在播放

        // 数据显示
        private ObservableCollection<CanFrameData> _visibleFrames;
        private int _frameDisplayCount = 50;  // 显示最近50帧

        private DispatcherTimer _playbackTimer;
        private Random _randomGenerator;

        #region 播放状态属性

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (Set(ref _isPlaying, value))
                {
                    OnPropertyChanged(nameof(PlayPauseButtonText));
                    OnPropertyChanged(nameof(CanPlay));
                }
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (Set(ref _isPaused, value))
                {
                    OnPropertyChanged(nameof(PlayPauseButtonText));
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
                // 拖拽过程中直接更新时间显示，避免触发 Set 导致循环更新
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
                        // 开始拖拽：暂停播放但记住状态
                        _wasPlayingBeforeDrag = IsPlaying;
                        if (IsPlaying)
                        {
                            _playbackTimer.Stop();
                            IsPlaying = false;
                            IsPaused = true;
                        }
                    }
                    else
                    {
                        // 结束拖拽：如果之前在播放则恢复
                        if (_wasPlayingBeforeDrag && IsFileLoaded)
                        {
                            _playbackTimer.Start();
                            IsPlaying = true;
                            IsPaused = false;
                        }
                    }
                }
            }
        }

        public double SliderValue
        {
            get => Progress;
            set
            {
                if (_isDragging)
                {
                    Progress = value; // 通过 Progress 的 setter 触发时间更新
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
                    RestartTimer();
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

        public string FileNameDisplay => string.IsNullOrEmpty(FileName) ? "未加载文件" : Path.GetFileName(FileName);

        public string SpeedDisplay => $"{PlaybackSpeed:F1}×";

        public string SpeedButtonText => $"{PlaybackSpeed:F0}×";

        public bool CanPlay => IsFileLoaded;

        public ObservableCollection<CanFrameData> VisibleFrames
        {
            get => _visibleFrames;
            set => Set(ref _visibleFrames, value);
        }

        #endregion

        #region 按钮文本 (Unicode符号)

        // ▶ 播放 / ⏸ 暂停
        public string PlayPauseButtonText => IsPlaying ? "\u23F8" : "\u25B6";

        // ⏮ 后退10秒
        public string RewindButtonText => "\u23EE";

        // ⏭ 前进10秒
        public string ForwardButtonText => "\u23ED";

        // ⏪ 上一帧
        public string PrevFrameButtonText => "\u23EA";

        // ⏩ 下一帧
        public string NextFrameButtonText => "\u23E9";

        // ⏹ 停止
        public string StopButtonText => "\u23F9";

        // 📂 文件选择
        public string FileSelectButtonText => "\uD83D\uDCC2"; // 📂

        #endregion

        #region 命令

        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand RewindCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand PrevFrameCommand { get; }
        public ICommand NextFrameCommand { get; }
        public ICommand CycleSpeedCommand { get; }
        public ICommand SelectFileCommand { get; }

        #endregion

        public CanPlaybackViewModel()
        {
            _randomGenerator = new Random();
            VisibleFrames = new ObservableCollection<CanFrameData>();
            _speedLevelIndex = 3;
            PlaybackSpeed = SpeedLevels[_speedLevelIndex];
            IsFileLoaded = false;
            step = (int)(PlaybackSpeed * (TotalFrames / TotalDurationMs));
            // 初始化时间显示
            TotalDurationStr = FormatTimeSpan(TimeSpan.FromMilliseconds(TotalDurationMs));
            CurrentDurationStr = FormatTimeSpan(TimeSpan.Zero);
            CurrentTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            // 创建播放计时器
            _playbackTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60fps
            };
            _playbackTimer.Tick += PlaybackTimer_Tick;

            // 绑定命令
            PlayPauseCommand = new RelayCommand(TogglePlayPause, () => CanPlay);
            StopCommand = new RelayCommand(StopPlayback);
            RewindCommand = new RelayCommand(Rewind, () => CanPlay);
            ForwardCommand = new RelayCommand(Forward, () => CanPlay);
            PrevFrameCommand = new RelayCommand(PrevFrame, () => CanPlay);
            NextFrameCommand = new RelayCommand(NextFrame, () => CanPlay);
            CycleSpeedCommand = new RelayCommand(CycleSpeed);
            SelectFileCommand = new RelayCommand(SelectAscFile);
        }

        #region 文件选择

        private void SelectAscFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ASC Files (*.asc)|*.asc|All Files (*.*)|*.*",
                Title = "选择 ASC 回放文件",
                Multiselect = false
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
            if (VisibleFrames.Count == 0)
            {
                GenerateInitialFrames();
                IsFileLoaded = true;
            }
        }

        private void LoadAscFile(string filePath)
        {
            try
            {
                FileName = filePath;
                IsFileLoaded = true;
                StopPlayback();

                // 加载并解析ASC文件
                var frames = ParseAscFile(filePath);
                if (frames.Count > 0)
                {
                    VisibleFrames.Clear();
                    // 显示前50帧
                    foreach (var frame in frames.Take(_frameDisplayCount))
                    {
                        VisibleFrames.Add(frame);
                    }

                    // 更新总时长和帧数显示
                    TotalDurationStr = FormatTimeSpan(frames[frames.Count - 1].Timestamp - frames[0].Timestamp);
                    CurrentDurationStr = FormatTimeSpan(TimeSpan.Zero);
                    CurrentTimestamp = frames[0].Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
                }
                else
                {
                    // 如果没有解析到帧，生成模拟数据
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<CanFrameData> ParseAscFile(string filePath)
        {
            var frames = new ObservableCollection<CanFrameData>();
            try
            {
                var lines = File.ReadAllLines(filePath);
                int index = 0;
                foreach (var line in lines)
                {
                    // ASC文件格式: timestamp Channel ID Rx/Tx DLC Data
                    // 例如: 0.000000 1 123x Rx d8 01 02 03 04 05 06 07 08
                    var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 6 && double.TryParse(parts[0], out double timestamp))
                    {
                        var frame = new CanFrameData
                        {
                            FrameIndex = index++,
                            Timestamp = DateTime.FromBinary((long)(timestamp * 10000000)),
                            CanId = ParseCanId(parts[2]),
                            Direction = parts[3].Equals("Rx", StringComparison.OrdinalIgnoreCase) ? "Rx" : "Tx",
                            Dlc = int.TryParse(parts[4], out int dlc) ? dlc : 8,
                            Message = "CAN Message",
                            Data = string.Join(" ", parts.Skip(5).Take(8).Select(p => "0x" + p.ToUpper()))
                        };
                        frames.Add(frame);
                    }
                }
            }
            catch
            {
                // 解析失败时返回空集合，让调用方生成模拟数据
            }
            return frames;
        }

        private byte ParseCanId(string idStr)
        {
            if (idStr.EndsWith("x"))
                idStr = idStr.TrimEnd('x');
            if (ushort.TryParse(idStr, System.Globalization.NumberStyles.HexNumber, null, out ushort id))
                return (byte)(id & 0xFF);
            return 0;
        }

        #endregion

        #region 速度切换

        private void CycleSpeed()
        {
            _speedLevelIndex = (_speedLevelIndex + 1) % SpeedLevels.Length;
            PlaybackSpeed = SpeedLevels[_speedLevelIndex];
            step = (int)(PlaybackSpeed * (TotalFrames / TotalDurationMs));
        }

        #endregion

        #region 播放控制

        private void TogglePlayPause()
        {
            if (!CanPlay) return;

            if (IsPlaying)
            {
                // 暂停
                _playbackTimer.Stop();
                IsPlaying = false;
                IsPaused = true;
            }
            else
            {
                // 播放
                if (CurrentTimeMs >= TotalDurationMs)
                {
                    CurrentTimeMs = 0; // 从头开始
                }
                _playbackTimer.Start();
                IsPlaying = true;
                IsPaused = false;
            }
        }

        private void StopPlayback()
        {
            _playbackTimer.Stop();
            IsPlaying = false;
            IsPaused = false;
            CurrentTimeMs = 0;
        }

        private void Rewind()
        {
            CurrentTimeMs = Math.Max(0, CurrentTimeMs - step); // 后退10秒
            UpdateVisibleFrames();
        }

        private void Forward()
        {
            CurrentTimeMs = Math.Min(TotalDurationMs, CurrentTimeMs + step); // 前进10秒
            UpdateVisibleFrames();
        }

        private void PrevFrame()
        {
            CurrentTimeMs = Math.Max(0, CurrentTimeMs - 1); // 后退1ms (1帧)
            UpdateVisibleFrames();
        }

        private void NextFrame()
        {
            CurrentTimeMs = Math.Min(TotalDurationMs, CurrentTimeMs + 1); // 前进1ms (1帧)
            UpdateVisibleFrames();
        }

        /// <summary>
        /// 跳转到指定进度位置 (0-100)
        /// </summary>
        public void SeekToProgress(double progress)
        {
            var targetTime = (progress / 100.0) * TotalDurationMs;
            CurrentTimeMs = Math.Max(0, Math.Min(TotalDurationMs, targetTime));
            UpdateVisibleFrames();
        }

        /// <summary>
        /// 获取指定时间对应的 DateTime
        /// </summary>
        private DateTime GetTimeAt(double timeMs)
        {
            var baseTime = DateTime.Now.AddMilliseconds(-TotalDurationMs);
            return baseTime.AddMilliseconds(timeMs);
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            // 如果正在拖拽进度条，跳过自动更新
            if (_isDragging) return;

            // 每次tick推进的时间量 (基于播放速度)
            double advance = 16 * PlaybackSpeed; // 16ms * speed
            CurrentTimeMs += advance;

            if (CurrentTimeMs >= TotalDurationMs)
            {
                CurrentTimeMs = TotalDurationMs;
                _playbackTimer.Stop();
                IsPlaying = false;
                IsPaused = false;
            }

            UpdateVisibleFrames();
        }

        private void RestartTimer()
        {
            if (_playbackTimer != null &&_playbackTimer.IsEnabled)
            {
                _playbackTimer.Stop();
                _playbackTimer.Start();
            }
        }

        #endregion

        #region 数据帧生成

        private void GenerateInitialFrames()
        {
            VisibleFrames.Clear();
            var baseTime = DateTime.Now.AddMilliseconds(-TotalDurationMs);

            for (int i = 0; i < _frameDisplayCount && i < TotalFrames; i++)
            {
                var frameTime = baseTime.AddMilliseconds(i * (TotalDurationMs / TotalFrames));
                VisibleFrames.Add(GenerateFrameData(i, frameTime));
            }
        }

        private void UpdateVisibleFrames()
        {
            var currentFrameIndex = (int)(CurrentTimeMs / (TotalDurationMs / TotalFrames));
            var baseTime = DateTime.Now.AddMilliseconds(-TotalDurationMs);
            var startFrame = Math.Max(0, currentFrameIndex - _frameDisplayCount + 1);

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                VisibleFrames.Clear();
                for (int i = startFrame; i < startFrame + _frameDisplayCount && i < TotalFrames; i++)
                {
                    var frameTime = baseTime.AddMilliseconds(i * (TotalDurationMs / TotalFrames));
                    VisibleFrames.Add(GenerateFrameData(i, frameTime));
                }
            }));
        }

        private CanFrameData GenerateFrameData(int index, DateTime time)
        {
            // 模拟CAN消息数据
            byte canId = (byte)(0x100 + (index % 256));
            string direction = index % 20 == 0 ? "Tx" : "Rx";
            int dlc = 8;
            string[] messageTypes = { "Engine Data", "Vehicle Speed", "Transmission", "ABS Status", "Airbag", "Body Control", "Instrument", "Chassis" };
            string message = messageTypes[index % messageTypes.Length];
            string data = GenerateRandomHex(dlc);

            return new CanFrameData
            {
                FrameIndex = index,
                Timestamp = time,
                CanId = canId,
                Direction = direction,
                Dlc = dlc,
                Message = message,
                Data = data
            };
        }

        private string GenerateRandomHex(int length)
        {
            var bytes = new byte[length];
            _randomGenerator.NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", " 0x").Insert(0, "0x");
        }

        #endregion

        #region 时间/进度更新

        private void UpdateTimestampDisplay()
        {
            var baseTime = DateTime.Now.AddMilliseconds(-TotalDurationMs);
            var currentTime = baseTime.AddMilliseconds(CurrentTimeMs);
            CurrentTimestamp = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            CurrentDurationStr = FormatTimeSpan(TimeSpan.FromMilliseconds(CurrentTimeMs));
        }

        private void UpdateProgress()
        {
            Progress = (CurrentTimeMs / TotalDurationMs) * 100;
        }

        private void UpdateCurrentFrame()
        {
            CurrentFrame = (int)(CurrentTimeMs / (TotalDurationMs / TotalFrames));
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds:D3}";
        }

        #endregion

        #region INotifyPropertyChanged

        #endregion
    }

    /// <summary>
    /// CAN帧数据模型
    /// </summary>
    public class CanFrameData : ViewModelBase
    {
        private int _frameIndex;
        private DateTime _timestamp;
        private byte _canId;
        private string _direction;
        private int _dlc;
        private string _message;
        private string _data;

        public int FrameIndex { get => _frameIndex; set => Set(ref _frameIndex, value); }
        public DateTime Timestamp { get => _timestamp; set => Set(ref _timestamp, value); }
        public byte CanId { get => _canId; set => Set(ref _canId, value); }
        public string Direction { get => _direction; set => Set(ref _direction, value); }
        public int Dlc { get => _dlc; set => Set(ref _dlc, value); }
        public string Message { get => _message; set => Set(ref _message, value); }
        public string Data { get => _data; set => Set(ref _data, value); }

        public string TimeDisplay => Timestamp.ToString("HH:mm:ss.fff");
        public string CanIdDisplay => $"0x{CanId:X2}";
    }
}