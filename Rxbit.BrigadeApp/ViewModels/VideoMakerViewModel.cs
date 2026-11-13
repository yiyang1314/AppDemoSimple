using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.Models;
using System;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// 图片项模型
    /// </summary>
    public class ImageItem : ViewModelBase
    {
        private string _filePath;
        private string _fileName;
        private double _duration = 2.0; // 默认每张图2秒
        private ImageSource _thumbnail;

        public string FilePath { get => _filePath; set => Set(ref _filePath, value); }
        public string FileName { get => _fileName; set => Set(ref _fileName, value); }
        public double Duration
        {
            get => _duration;
            set
            {
                if (Set(ref _duration, value))
                    TotalDurationChanged?.Invoke();
            }
        }
        public ImageSource Thumbnail { get => _thumbnail; set => Set(ref _thumbnail, value); }

        public Action TotalDurationChanged { get; set; }
    }

    /// <summary>
    /// 视频制作 ViewModel：批量导入图片+MP3配乐→生成MP4视频
    /// </summary>
    public class VideoMakerViewModel : ViewModelBase
    {
        // 图片列表
        private ObservableCollection<ImageItem> _imageItems;
        private ImageItem _selectedImage;
        private double _defaultImageDuration = 2.0;

        // MP3配乐
        private string _mp3FilePath;
        private string _mp3FileName;
        private double _mp3DurationSec; // MP3时长（秒）

        // 输出
        private string _outputPath;
        private string _outputFileName = "output.mp4";

        // 视频参数
        private int _videoWidth = 1920;
        private int _videoHeight = 1080;
        private int _frameRate = 25;

        // 生成状态
        private bool _isGenerating;
        private int _generateProgress;
        private string _generateStatus;
        private string _ffmpegPath;
        private string _ffprobePath;
        private bool _isFFmpegAvailable;
        private string _ffmpegStatusText;

        // 视频播放
        private string _videoFilePath;
        private bool _isVideoLoaded;
        private bool _isPlaying;
        private TimeSpan _currentPosition;
        private TimeSpan _totalDuration;
        private double _playProgress;
        private DispatcherTimer _playTimer;

        // 命令
        public ICommand ImportImagesCommand { get; }
        public ICommand ImportFolderCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ClearImagesCommand { get; }
        public ICommand BrowseMp3Command { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand GenerateVideoCommand { get; }
        public ICommand PlayPauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand BrowseFFmpegCommand { get; }
        public ICommand DetectFFmpegCommand { get; }
        public ICommand DownloadFFmpegCommand { get; }

        // 事件
        public Action<string> SelectImageFolderRequested;
        public Action SelectMp3FileRequested;
        public Action SelectOutputFolderRequested;
        public Action SelectFFmpegFileRequested;

        /// <summary>
        /// 总时长（秒）= 所有图片时长之和
        /// </summary>
        public double TotalVideoDurationSec => _imageItems?.Sum(x => x.Duration) ?? 0;

        public ObservableCollection<ImageItem> ImageItems
        {
            get => _imageItems;
            set
            {
                if (Set(ref _imageItems, value))
                {
                    OnPropertyChanged(nameof(TotalVideoDurationSec));
                    OnPropertyChanged(nameof(TotalDurationStr));
                    OnPropertyChanged(nameof(CanGenerate));
                }
            }
        }

        public ImageItem SelectedImage
        {
            get => _selectedImage;
            set => Set(ref _selectedImage, value);
        }

        public double DefaultImageDuration
        {
            get => _defaultImageDuration;
            set
            {
                if (Set(ref _defaultImageDuration, value))
                {
                    // 批量更新所有图片时长
                    foreach (var item in _imageItems)
                        item.Duration = value;
                    OnPropertyChanged(nameof(TotalVideoDurationSec));
                    OnPropertyChanged(nameof(TotalDurationStr));
                    OnPropertyChanged(nameof(CanGenerate));
                }
            }
        }

        public string Mp3FilePath
        {
            get => _mp3FilePath;
            set
            {
                if (Set(ref _mp3FilePath, value))
                {
                    if (!string.IsNullOrEmpty(value) && File.Exists(value))
                    {
                        Mp3FileName = Path.GetFileName(value);
                        GetMp3Duration(value);
                    }
                }
            }
        }

        public string Mp3FileName
        {
            get => _mp3FileName;
            set => Set(ref _mp3FileName, value);
        }

        public double Mp3DurationSec
        {
            get => _mp3DurationSec;
            set
            {
                Set(ref _mp3DurationSec, value);
                OnPropertyChanged(nameof(Mp3DurationStr));
                OnPropertyChanged(nameof(AudioTrimInfo));
            }
        }

        public string Mp3DurationStr => _mp3DurationSec > 0
            ? TimeSpan.FromSeconds(_mp3DurationSec).ToString(@"mm\:ss\.fff")
            : "--";

        /// <summary>
        /// 音频剪辑信息：MP3时长 vs 视频时长
        /// </summary>
        public string AudioTrimInfo
        {
            get
            {
                if (_mp3DurationSec <= 0 || TotalVideoDurationSec <= 0)
                    return "待选择配乐";
                if (_mp3DurationSec > TotalVideoDurationSec)
                    return $"配乐将被截取至视频时长 {TimeSpan.FromSeconds(TotalVideoDurationSec):mm\\:ss\\.fff}";
                return $"配乐时长 {TimeSpan.FromSeconds(_mp3DurationSec):mm\\:ss\\.fff}（视频将匹配配乐时长）";
            }
        }

        public string OutputPath
        {
            get => _outputPath;
            set => Set(ref _outputPath, value);
        }

        public string OutputFileName
        {
            get => _outputFileName;
            set => Set(ref _outputFileName, value);
        }

        public int VideoWidth
        {
            get => _videoWidth;
            set => Set(ref _videoWidth, value);
        }

        public int VideoHeight
        {
            get => _videoHeight;
            set => Set(ref _videoHeight, value);
        }

        public int FrameRate
        {
            get => _frameRate;
            set => Set(ref _frameRate, value);
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                Set(ref _isGenerating, value);
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        public int GenerateProgress
        {
            get => _generateProgress;
            set => Set(ref _generateProgress, value);
        }

        public string GenerateStatus
        {
            get => _generateStatus;
            set => Set(ref _generateStatus, value);
        }

        public string VideoFilePath
        {
            get => _videoFilePath;
            set
            {
                Set(ref _videoFilePath, value);
                IsVideoLoaded = !string.IsNullOrEmpty(value) && File.Exists(value);
            }
        }

        public bool IsVideoLoaded
        {
            get => _isVideoLoaded;
            set => Set(ref _isVideoLoaded, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => Set(ref _isPlaying, value);
        }

        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            set
            {
                Set(ref _currentPosition, value);
                OnPropertyChanged(nameof(CurrentPositionStr));
                if (_totalDuration.TotalSeconds > 0)
                {
                    PlayProgress = (_currentPosition.TotalSeconds / _totalDuration.TotalSeconds) * 100;
                }
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

        public string CurrentPositionStr => _currentPosition.ToString(@"mm\:ss\.fff");
        public string TotalDurationStr => TotalVideoDurationSec > 0
            ? TimeSpan.FromSeconds(TotalVideoDurationSec).ToString(@"mm\:ss\.fff")
            : "00:00.000";

        public double PlayProgress
        {
            get => _playProgress;
            set => Set(ref _playProgress, value);
        }

        public bool CanGenerate => _imageItems != null && _imageItems.Count > 0
            && !string.IsNullOrEmpty(_mp3FilePath)
            && !IsGenerating
            && IsFFmpegAvailable;

        /// <summary>
        /// FFmpeg 是否可用
        /// </summary>
        public bool IsFFmpegAvailable
        {
            get => _isFFmpegAvailable;
            set
            {
                Set(ref _isFFmpegAvailable, value);
                OnPropertyChanged(nameof(CanGenerate));
                OnPropertyChanged(nameof(FFmpegStatusColor));
            }
        }

        /// <summary>
        /// FFmpeg 状态文字
        /// </summary>
        public string FFmpegStatusText
        {
            get => _ffmpegStatusText;
            set => Set(ref _ffmpegStatusText, value);
        }

        /// <summary>
        /// FFmpeg 路径（用于显示）
        /// </summary>
        public string FFmpegPath
        {
            get => _ffmpegPath;
            set => Set(ref _ffmpegPath, value);
        }

        /// <summary>
        /// FFmpeg 状态颜色（绿色=已找到，红色=未找到）
        /// </summary>
        public string FFmpegStatusColor => _isFFmpegAvailable ? "#00d9ff" : "#ff6b6b";

        /// <summary>
        /// 播放器位置跳转
        /// </summary>
        public Action<double> SeekVideoAction { get; set; }

        /// <summary>
        /// 播放器控制
        /// </summary>
        public Action PlayVideoAction { get; set; }
        public Action PauseVideoAction { get; set; }
        public Action StopVideoAction { get; set; }
        public Action<string> LoadVideoAction { get; set; }

        public VideoMakerViewModel()
        {
            ImageItems = new ObservableCollection<ImageItem>();
            OutputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _playTimer.Tick += PlayTimer_Tick;

            ImportImagesCommand = new RelayCommand(ImportImages);
            ImportFolderCommand = new RelayCommand(ImportFolder);
            RemoveImageCommand = new RelayCommand<ImageItem>(RemoveImage);
            ClearImagesCommand = new RelayCommand(ClearImages, () => _imageItems.Count > 0);
            BrowseMp3Command = new RelayCommand(BrowseMp3);
            BrowseOutputCommand = new RelayCommand(BrowseOutput);
            GenerateVideoCommand = new RelayCommand(GenerateVideo, () => CanGenerate);
            PlayPauseCommand = new RelayCommand(PlayPause, () => _isVideoLoaded);
            StopCommand = new RelayCommand(StopPlay, () => _isVideoLoaded);
            BrowseFFmpegCommand = new RelayCommand(BrowseFFmpeg);
            DetectFFmpegCommand = new RelayCommand(() => DetectFFmpeg());
            DownloadFFmpegCommand = new RelayCommand(DownloadFFmpeg);

            // 启动时自动检测 FFmpeg
            DetectFFmpeg();
        }

        /// <summary>
        /// 导入多张图片
        /// </summary>
        private void ImportImages()
        {
            SelectImageFolderRequested?.Invoke("选择图片文件");
        }

        /// <summary>
        /// 导入整个文件夹的图片
        /// </summary>
        private void ImportFolder()
        {
            SelectImageFolderRequested?.Invoke("选择图片文件夹");
        }

        /// <summary>
        /// 添加图片文件到列表
        /// </summary>
        public void AddImageFiles(string[] files)
        {
            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            foreach (var file in files.OrderBy(f => f))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (!imageExts.Contains(ext)) continue;

                var item = new ImageItem
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Duration = _defaultImageDuration,
                };
                item.TotalDurationChanged = () =>
                {
                    OnPropertyChanged(nameof(TotalVideoDurationSec));
                    OnPropertyChanged(nameof(TotalDurationStr));
                    OnPropertyChanged(nameof(AudioTrimInfo));
                    OnPropertyChanged(nameof(CanGenerate));
                };

                // 加载缩略图
                try
                {
                    var img = new System.Windows.Media.Imaging.BitmapImage();
                    img.BeginInit();
                    img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    img.UriSource = new Uri(file, UriKind.Absolute);
                    img.DecodePixelWidth = 160;
                    img.EndInit();
                    img.Freeze();
                    item.Thumbnail = img;
                }
                catch { }

                _imageItems.Add(item);
            }

            OnPropertyChanged(nameof(TotalVideoDurationSec));
            OnPropertyChanged(nameof(TotalDurationStr));
            OnPropertyChanged(nameof(AudioTrimInfo));
            OnPropertyChanged(nameof(CanGenerate));
        }

        /// <summary>
        /// 添加文件夹内所有图片
        /// </summary>
        public void AddImageFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;
            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var files = Directory.GetFiles(folderPath)
                .Where(f => imageExts.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(f => f)
                .ToArray();
            AddImageFiles(files);
        }

        /// <summary>
        /// 删除指定图片
        /// </summary>
        private void RemoveImage(ImageItem item)
        {
            if (item != null && _imageItems.Contains(item))
            {
                _imageItems.Remove(item);
                OnPropertyChanged(nameof(TotalVideoDurationSec));
                OnPropertyChanged(nameof(TotalDurationStr));
                OnPropertyChanged(nameof(AudioTrimInfo));
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        /// <summary>
        /// 清空图片列表
        /// </summary>
        private void ClearImages()
        {
            _imageItems.Clear();
            OnPropertyChanged(nameof(TotalVideoDurationSec));
            OnPropertyChanged(nameof(TotalDurationStr));
            OnPropertyChanged(nameof(AudioTrimInfo));
            OnPropertyChanged(nameof(CanGenerate));
        }

        /// <summary>
        /// 选择MP3文件
        /// </summary>
        private void BrowseMp3()
        {
            SelectMp3FileRequested?.Invoke();
        }

        /// <summary>
        /// 设置MP3文件路径
        /// </summary>
        public void SetMp3File(string path)
        {
            Mp3FilePath = path;
        }

        /// <summary>
        /// 选择输出目录
        /// </summary>
        private void BrowseOutput()
        {
            SelectOutputFolderRequested?.Invoke();
        }

        /// <summary>
        /// 设置输出路径
        /// </summary>
        public void SetOutputPath(string path)
        {
            OutputPath = path;
        }

        #region FFmpeg 检测与管理

        /// <summary>
        /// 自动检测 FFmpeg/ffprobe 是否可用
        /// 检测顺序：PATH → 常见安装目录 → 应用程序同级目录
        /// </summary>
        public void DetectFFmpeg()
        {
            FFmpegStatusText = "正在检测 FFmpeg...";

            // 候选路径列表
            var ffmpegCandidates = new System.Collections.Generic.List<string>();
            var ffprobeCandidates = new System.Collections.Generic.List<string>();

            // 1. PATH 环境变量中的 ffmpeg/ffprobe
            ffmpegCandidates.Add("ffmpeg");
            ffprobeCandidates.Add("ffprobe");

            // 2. 常见 Windows 安装路径
            string[] commonDirs = {
                @"C:\ffmpeg\bin",
                @"C:\Program Files\ffmpeg\bin",
                @"C:\Program Files (x86)\ffmpeg\bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ffmpeg\bin"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"ffmpeg\bin"),
            };

            foreach (var dir in commonDirs)
            {
                if (Directory.Exists(dir))
                {
                    ffmpegCandidates.Add(Path.Combine(dir, "ffmpeg.exe"));
                    ffprobeCandidates.Add(Path.Combine(dir, "ffprobe.exe"));
                }
            }

            // 3. 应用程序同级目录 (bundled)
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            ffmpegCandidates.Add(Path.Combine(appDir, "ffmpeg.exe"));
            ffmpegCandidates.Add(Path.Combine(appDir, "ffmpeg", "ffmpeg.exe"));
            ffmpegCandidates.Add(Path.Combine(appDir, "Tools", "ffmpeg.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "ffprobe.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "ffmpeg", "ffprobe.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "Tools", "ffprobe.exe"));

            // 4. 用户上次手动指定的路径（持久化在 Settings 中）
            try
            {
                var savedPath = Rxbit.BrigadeApp.Properties.Settings.Default.FFmpegPath;
                if (!string.IsNullOrEmpty(savedPath))
                {
                    var dir = Path.GetDirectoryName(savedPath);
                    if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    {
                        ffmpegCandidates.Insert(0, Path.Combine(dir, "ffmpeg.exe"));
                        ffprobeCandidates.Insert(0, Path.Combine(dir, "ffprobe.exe"));
                    }
                }
            }
            catch { }

            // 逐个检测
            string foundFFmpeg = null;
            string foundFFprobe = null;

            foreach (var path in ffmpegCandidates)
            {
                if (TryTestFFmpeg(path))
                {
                    foundFFmpeg = path == "ffmpeg" ? "ffmpeg" : path;
                    break;
                }
            }

            foreach (var path in ffprobeCandidates)
            {
                if (TryTestFFprobe(path))
                {
                    foundFFprobe = path == "ffprobe" ? "ffprobe" : path;
                    break;
                }
            }

            // 更新状态
            if (foundFFmpeg != null)
            {
                _ffmpegPath = foundFFmpeg;
                _ffprobePath = foundFFprobe ?? foundFFmpeg; // ffprobe 没找到时退回用 ffmpeg
                IsFFmpegAvailable = true;
                FFmpegPath = foundFFmpeg == "ffmpeg" ? "PATH (系统环境变量)" : foundFFmpeg;
                FFmpegStatusText = foundFFprobe != null
                    ? "✓ FFmpeg 已就绪"
                    : "✓ FFmpeg 可用（ffprobe 未检测到，部分功能受限）";
            }
            else
            {
                _ffmpegPath = null;
                _ffprobePath = null;
                IsFFmpegAvailable = false;
                FFmpegPath = "未找到";
                FFmpegStatusText = "✗ FFmpeg 未安装，请点击右侧按钮安装或手动指定路径";
            }

            OnPropertyChanged(nameof(FFmpegPath));
            OnPropertyChanged(nameof(FFmpegStatusText));
            OnPropertyChanged(nameof(IsFFmpegAvailable));
            OnPropertyChanged(nameof(CanGenerate));
        }

        /// <summary>
        /// 测试 ffmpeg 是否可执行
        /// </summary>
        private bool TryTestFFmpeg(string path)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(3000);
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 测试 ffprobe 是否可执行
        /// </summary>
        private bool TryTestFFprobe(string path)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit(3000);
                    return proc.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 手动浏览选择 ffmpeg.exe
        /// </summary>
        private void BrowseFFmpeg()
        {
            SelectFFmpegFileRequested?.Invoke();
        }

        /// <summary>
        /// 设置 FFmpeg 路径（用户手动指定后）
        /// </summary>
        public void SetFFmpegPath(string ffmpegExePath)
        {
            if (string.IsNullOrEmpty(ffmpegExePath) || !File.Exists(ffmpegExePath))
                return;

            var dir = Path.GetDirectoryName(ffmpegExePath);
            _ffmpegPath = ffmpegExePath;
            _ffprobePath = Path.Combine(dir, "ffprobe.exe");

            // ffprobe 不存在时退回用 ffmpeg
            if (!File.Exists(_ffprobePath))
                _ffprobePath = ffmpegExePath;

            IsFFmpegAvailable = TryTestFFmpeg(ffmpegExePath);
            FFmpegPath = ffmpegExePath;
            FFmpegStatusText = IsFFmpegAvailable
                ? "✓ FFmpeg 已配置（手动指定）"
                : "✗ 指定的路径无效";

            // 持久化保存
            try
            {
                Rxbit.BrigadeApp.Properties.Settings.Default.FFmpegPath = ffmpegExePath;
                Rxbit.BrigadeApp.Properties.Settings.Default.Save();
            }
            catch { }

            OnPropertyChanged(nameof(FFmpegPath));
            OnPropertyChanged(nameof(FFmpegStatusText));
            OnPropertyChanged(nameof(IsFFmpegAvailable));
            OnPropertyChanged(nameof(CanGenerate));
        }

        /// <summary>
        /// 打开 FFmpeg 下载页面
        /// </summary>
        private void DownloadFFmpeg()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip",
                    UseShellExecute = true
                });
            }
            catch
            {
                try
                {
                    // 备用下载地址
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/BtbN/FFmpeg-Builds/releases",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show(
                        "无法自动打开浏览器，请手动访问以下地址下载 FFmpeg：\n\n" +
                        "Windows 版下载：https://www.gyan.dev/ffmpeg/builds/\n" +
                        "GitHub 构建：https://github.com/BtbN/FFmpeg-Builds/releases\n\n" +
                        "下载后解压，将 bin 文件夹中的 ffmpeg.exe 和 ffprobe.exe\n" +
                        "放入系统 PATH 或通过本界面「手动指定」按钮选择路径。",
                        "FFmpeg 下载指引",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        #endregion

        /// <summary>
        /// 获取MP3时长（通过FFmpeg ffprobe）
        /// </summary>
        private void GetMp3Duration(string mp3Path)
        {
            if (string.IsNullOrEmpty(_ffprobePath) || !File.Exists(_ffprobePath))
            {
                Mp3DurationSec = 0;
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{mp3Path}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    var output = proc.StandardOutput.ReadToEnd().Trim();
                    proc.WaitForExit(5000);
                    if (double.TryParse(output, out var dur))
                    {
                        Mp3DurationSec = dur;
                    }
                }
            }
            catch
            {
                Mp3DurationSec = 0;
            }
            OnPropertyChanged(nameof(AudioTrimInfo));
        }

        /// <summary>
        /// 生成MP4视频
        /// </summary>
        private void GenerateVideo()
        {
            if (!IsFFmpegAvailable || string.IsNullOrEmpty(_ffmpegPath))
            {
                MessageBox.Show(
                    "FFmpeg 未检测到！请先安装 FFmpeg 或手动指定路径。\n\n" +
                    "点击界面底部的「下载FFmpeg」或「手动指定」按钮。",
                    "FFmpeg 未安装",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (_imageItems.Count == 0 || string.IsNullOrEmpty(_mp3FilePath))
            {
                MessageBox.Show("请先添加图片和选择配乐", "提示");
                return;
            }

            IsGenerating = true;
            GenerateProgress = 0;
            GenerateStatus = "准备生成...";

            Task.Run(() =>
            {
                try
                {
                    var outputPath = Path.Combine(OutputPath, OutputFileName);
                    var tempDir = Path.Combine(Path.GetTempPath(), "VideoMaker_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(tempDir);

                    // 1. 生成 concat 文件（FFmpeg concat demuxer 格式）
                    GenerateStatus = "生成图片序列...";
                    GenerateProgress = 10;

                    var concatFile = Path.Combine(tempDir, "images.txt");
                    using (var sw = new StreamWriter(concatFile))
                    {
                        foreach (var img in _imageItems)
                        {
                            // 每个图片持续 img.Duration 秒
                            var d = img.Duration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                            sw.WriteLine($"file '{img.FilePath}'");
                            sw.WriteLine($"duration {d}");
                        }
                        // 最后一张图重复一次（FFmpeg concat demuxer 需要）
                        sw.WriteLine($"file '{_imageItems.Last().FilePath}'");
                    }

                    // 2. 计算视频总时长
                    var videoDuration = _imageItems.Sum(x => x.Duration);

                    GenerateStatus = "正在生成视频（FFmpeg处理中）...";
                    GenerateProgress = 30;

                    // 3. 调用 FFmpeg 合成
                    // 使用 concat demuxer + 音频截取到视频时长
                    var args = $"-y -f concat -safe 0 -i \"{concatFile}\" " +
                               $"-i \"{_mp3FilePath}\" " +
                               $"-c:v libx264 -pix_fmt yuv420p " +
                               $"-r {_frameRate} " +
                               $"-vf \"scale={_videoWidth}:{_videoHeight},fps={_frameRate}\" " +
                               $"-c:a aac -b:a 192k " +
                               $"-t {videoDuration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} " +
                               $"-shortest " +
                               $"\"{outputPath}\"";

                    var psi = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var proc = new Process { StartInfo = psi })
                    {
                        proc.Start();

                        // 读取FFmpeg输出解析进度
                        string line;
                        while ((line = proc.StandardError.ReadLine()) != null)
                        {
                            // 解析 time=00:01:23.45 格式的进度
                            var timeIdx = line.IndexOf("time=");
                            if (timeIdx >= 0)
                            {
                                var timeStr = line.Substring(timeIdx + 5);
                                var spaceIdx = timeStr.IndexOf(' ');
                                if (spaceIdx > 0) timeStr = timeStr.Substring(0, spaceIdx);

                                if (TimeSpan.TryParse(timeStr, out var elapsed))
                                {
                                    var pct = (elapsed.TotalSeconds / videoDuration) * 60 + 30; // 30%~90%
                                    if (pct > 90) pct = 90;
                                    GenerateProgress = (int)pct;
                                    GenerateStatus = $"正在编码... {elapsed:mm\\:ss} / {TimeSpan.FromSeconds(videoDuration):mm\\:ss}";
                                }
                            }
                        }
                        proc.WaitForExit();
                    }

                    GenerateProgress = 95;
                    GenerateStatus = "清理临时文件...";

                    // 清理临时文件
                    try { Directory.Delete(tempDir, true); } catch { }

                    GenerateProgress = 100;
                    GenerateStatus = "生成完成！";

                    // 在UI线程加载视频
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        VideoFilePath = outputPath;
                        LoadVideoAction?.Invoke(outputPath);
                    });
                }
                catch (Exception ex)
                {
                    GenerateStatus = $"生成失败: {ex.Message}";
                }
                finally
                {
                    Application.Current?.Dispatcher.Invoke(() => IsGenerating = false);
                }
            });
        }

        /// <summary>
        /// 播放/暂停
        /// </summary>
        private void PlayPause()
        {
            if (_isPlaying)
            {
                PauseVideoAction?.Invoke();
                IsPlaying = false;
                _playTimer.Stop();
            }
            else
            {
                PlayVideoAction?.Invoke();
                IsPlaying = true;
                _playTimer.Start();
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        private void StopPlay()
        {
            StopVideoAction?.Invoke();
            IsPlaying = false;
            _playTimer.Stop();
            CurrentPosition = TimeSpan.Zero;
        }

        /// <summary>
        /// 播放计时器
        /// </summary>
        private void PlayTimer_Tick(object sender, EventArgs e)
        {
            // 由 View 中的 MediaElement 更新 CurrentPosition
            UpdatePositionAction?.Invoke();
        }

        public Action UpdatePositionAction { get; set; }

        /// <summary>
        /// 设置播放位置
        /// </summary>
        public void SetPosition(TimeSpan position)
        {
            CurrentPosition = position;
        }

        /// <summary>
        /// 设置总时长
        /// </summary>
        public void SetTotalDuration(TimeSpan duration)
        {
            TotalDuration = duration;
        }
    }
}
