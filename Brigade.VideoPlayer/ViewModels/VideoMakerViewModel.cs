using Brigade.VideoPlayer.Commands;
using Brigade.VideoPlayer.Models;
using Brigade.VideoPlayer.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Brigade.VideoPlayer.ViewModels
{
    public class ImageItem : ViewModelBase
    {
        private string _filePath;
        private string _fileName;
        private double _duration = 2.0;
        private System.Windows.Media.ImageSource _thumbnail;

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
        public System.Windows.Media.ImageSource Thumbnail { get => _thumbnail; set => Set(ref _thumbnail, value); }
        public Action TotalDurationChanged { get; set; }
    }

    public class VideoMakerViewModel : ViewModelBase
    {
        private readonly FFmpegService _ffmpegService;
        private ObservableCollection<ImageItem> _imageItems;
        private ImageItem _selectedImage;
        private double _defaultImageDuration = 2.0;
        private string _mp3FilePath;
        private string _mp3FileName;
        private double _mp3DurationSec;
        private string _outputPath;
        private string _outputFileName = "output.mp4";
        private int _videoWidth = 1920;
        private int _videoHeight = 1080;
        private int _frameRate = 25;
        private bool _isGenerating;
        private int _generateProgress;
        private string _generateStatus;

        // 命令
        public ICommand ImportImagesCommand { get; }
        public ICommand ImportFolderCommand { get; }
        public ICommand RemoveImageCommand { get; }
        public ICommand ClearImagesCommand { get; }
        public ICommand BrowseMp3Command { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand GenerateVideoCommand { get; }
        public ICommand DetectFFmpegCommand { get; }

        // 事件
        public Action SelectImageFilesRequested;
        public Action SelectImageFolderRequested;
        public Action SelectMp3FileRequested;
        public Action SelectOutputFolderRequested;

        public ObservableCollection<ImageItem> ImageItems
        {
            get => _imageItems;
            set
            {
                if (Set(ref _imageItems, value))
                {
                    UpdateTotalDuration();
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
                    foreach (var item in _imageItems)
                        item.Duration = value;
                    UpdateTotalDuration();
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
                        _mp3DurationSec = _ffmpegService.GetMediaDuration(value);
                        OnPropertyChanged(nameof(Mp3DurationSec));
                        OnPropertyChanged(nameof(Mp3DurationStr));
                        OnPropertyChanged(nameof(AudioTrimInfo));
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

        public double TotalVideoDurationSec => _imageItems?.Sum(x => x.Duration) ?? 0;

        public string TotalDurationStr => TotalVideoDurationSec > 0
            ? TimeSpan.FromSeconds(TotalVideoDurationSec).ToString(@"mm\:ss\.fff")
            : "00:00.000";

        public string AudioTrimInfo
        {
            get
            {
                if (_mp3DurationSec <= 0 || TotalVideoDurationSec <= 0)
                    return "待选择配乐";
                if (_mp3DurationSec > TotalVideoDurationSec)
                    return $"配乐将截取至视频时长 {TimeSpan.FromSeconds(TotalVideoDurationSec):mm\\:ss\\.fff}";
                return $"视频时长将匹配配乐时长 {TimeSpan.FromSeconds(_mp3DurationSec):mm\\:ss\\.fff}";
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

        public bool CanGenerate => _imageItems != null && _imageItems.Count > 0
            && !string.IsNullOrEmpty(_mp3FilePath)
            && !string.IsNullOrEmpty(_outputPath)
            && !IsGenerating
            && _ffmpegService.IsAvailable;

        public VideoMakerViewModel(FFmpegService ffmpegService)
        {
            _ffmpegService = ffmpegService;
            _imageItems = new ObservableCollection<ImageItem>();
            _outputPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            ImportImagesCommand = new RelayCommand(ImportImages);
            ImportFolderCommand = new RelayCommand(ImportFolder);
            RemoveImageCommand = new RelayCommand<ImageItem>(RemoveImage);
            ClearImagesCommand = new RelayCommand(ClearImages, () => _imageItems.Count > 0);
            BrowseMp3Command = new RelayCommand(BrowseMp3);
            BrowseOutputCommand = new RelayCommand(BrowseOutput);
            GenerateVideoCommand = new RelayCommand(GenerateVideo, () => CanGenerate);
            DetectFFmpegCommand = new RelayCommand(() => _ffmpegService.DetectFFmpeg());

            _ffmpegService.ProgressChanged += OnProgressChanged;
            _ffmpegService.StatusChanged += OnStatusChanged;
        }

        private void UpdateTotalDuration()
        {
            OnPropertyChanged(nameof(TotalVideoDurationSec));
            OnPropertyChanged(nameof(TotalDurationStr));
            OnPropertyChanged(nameof(AudioTrimInfo));
            OnPropertyChanged(nameof(CanGenerate));
        }

        private void OnProgressChanged(double progress)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() => GenerateProgress = (int)progress);
        }

        private void OnStatusChanged(string status)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() => GenerateStatus = status);
        }

        private void ImportImages()
        {
            SelectImageFilesRequested?.Invoke();
        }

        private void ImportFolder()
        {
            SelectImageFolderRequested?.Invoke();
        }

        public void AddImageFiles(string[] files)
        {
            var imageExts = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            foreach (var file in files.OrderBy(f => f))
            {
                if (!imageExts.Contains(Path.GetExtension(file).ToLower())) continue;

                var item = new ImageItem
                {
                    FilePath = file,
                    FileName = Path.GetFileName(file),
                    Duration = _defaultImageDuration,
                };
                item.TotalDurationChanged = UpdateTotalDuration;

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

            UpdateTotalDuration();
        }

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

        private void RemoveImage(ImageItem item)
        {
            if (item != null && _imageItems.Contains(item))
            {
                _imageItems.Remove(item);
                UpdateTotalDuration();
            }
        }

        private void ClearImages()
        {
            _imageItems.Clear();
            UpdateTotalDuration();
        }

        private void BrowseMp3()
        {
            SelectMp3FileRequested?.Invoke();
        }

        public void SetMp3File(string path)
        {
            Mp3FilePath = path;
        }

        private void BrowseOutput()
        {
            SelectOutputFolderRequested?.Invoke();
        }

        public void SetOutputPath(string path)
        {
            OutputPath = path;
            OnPropertyChanged(nameof(CanGenerate));
        }

        private async void GenerateVideo()
        {
            if (!_ffmpegService.IsAvailable)
            {
                System.Windows.MessageBox.Show("FFmpeg 未检测到！请先安装 FFmpeg。", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsGenerating = true;
                GenerateProgress = 0;

                var outputFile = Path.Combine(_outputPath, _outputFileName);
                var images = _imageItems.Select(x => (x.FilePath, x.Duration)).ToList();

                await _ffmpegService.GenerateVideoAsync(
                    images,
                    _mp3FilePath,
                    outputFile,
                    _videoWidth,
                    _videoHeight,
                    _frameRate);

                System.Windows.MessageBox.Show($"视频生成成功！\n保存路径：{outputFile}", "完成",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"生成失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }
}
