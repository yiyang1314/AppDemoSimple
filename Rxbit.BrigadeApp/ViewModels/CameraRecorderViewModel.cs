using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Services.Media;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    /// <summary>
    /// USB 摄像头预览/录制窗口的视图模型：枚举设备、预览画面、录制为 AVI(MJPEG)。
    /// </summary>
    public class CameraRecorderViewModel : ViewModelBase, IDisposable
    {
        private readonly object _sync = new object();
        private readonly DispatcherTimer _statusTimer;
        private readonly DispatcherTimer _previewWatchdog;
        private int _receivedFrames;

        private CameraCaptureSession _session;
        private Thread _captureThread;
        private AviMjpegWriter _writer;

        private volatile bool _stopRequested;
        private volatile bool _recording;
        private bool _isPreviewing;
        private bool _disposed;

        private int _frameCount;
        private Stopwatch _recordWatch;
        private int _recordFrameRate = 10;
        private DateTime _lastPreviewUpdate;
        private int _nullFrameCount;

        private CameraDeviceInfo _selectedDevice;
        private ImageSource _previewImage;
        private string _statusMessage = "请选择 USB 摄像头开始预览。";
        private string _outputPath;
        private string _elapsedText = "00:00:00";
        private string _frameCountText = "0";

        public CameraRecorderViewModel()
        {
            Devices = new ObservableCollection<CameraDeviceInfo>();
            FrameRateOptions = new[] { 5, 10, 15 };
            OutputFolder = Path.Combine(RadarConstaint.RecondingPath, "Media");

            RefreshDevicesCommand = new RelayCommand(RefreshDevices);
            StartCommand = new RelayCommand(StartRecording, () => !IsRecording);
            StopCommand = new RelayCommand(StopRecording, () => IsRecording);
            StopPreviewCommand = new RelayCommand(StopPreview, () => IsPreviewing);
            OpenFolderCommand = new RelayCommand(OpenOutputFolder);

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _statusTimer.Tick += (s, e) => RefreshStatus();

            // 首帧看门狗：部分摄像头/驱动会在 ReadSample 上无限阻塞，5 秒无画面则主动停止并提示
            _previewWatchdog = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _previewWatchdog.Tick += (s, e) =>
            {
                _previewWatchdog.Stop();
                if (_disposed || !IsPreviewing || _receivedFrames > 0) return;
                StopCapture();
                StatusMessage = "摄像头 5 秒内没有返回画面，已停止预览。请检查设备是否被其它程序占用或重新插拔后再试。";
            };

            RefreshDevices();
        }

        public ObservableCollection<CameraDeviceInfo> Devices { get; }
        public int[] FrameRateOptions { get; }
        public string OutputFolder { get; }

        public CameraDeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (!SetProperty(ref _selectedDevice, value)) return;
                if (_disposed) return;

                // 切换设备时先停止当前录制/预览，确保释放摄像头句柄
                StopCapture();
                if (value != null)
                {
                    StartPreview();
                }
                else
                {
                    PreviewImage = null;
                    StatusMessage = "未选择摄像头。";
                }
            }
        }

        public int SelectedFrameRate
        {
            get => _recordFrameRate;
            set => SetProperty(ref _recordFrameRate, value);
        }

        public ImageSource PreviewImage
        {
            get => _previewImage;
            private set => SetProperty(ref _previewImage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string OutputPath
        {
            get => _outputPath;
            private set => SetProperty(ref _outputPath, value);
        }

        public bool IsRecording => _recording;

        public bool CanSwitchDevice => !_recording;

        public bool IsPreviewing
        {
            get => _isPreviewing;
            private set
            {
                if (SetProperty(ref _isPreviewing, value))
                {
                    OnPropertyChanged(nameof(RecordingStateText));
                }
            }
        }

        public string RecordingStateText
        {
            get
            {
                if (IsRecording) return "● REC";
                return IsPreviewing ? "● LIVE" : "IDLE";
            }
        }

        public string ElapsedText
        {
            get => _elapsedText;
            private set => SetProperty(ref _elapsedText, value);
        }

        public string FrameCountText
        {
            get => _frameCountText;
            private set => SetProperty(ref _frameCountText, value);
        }

        public ICommand RefreshDevicesCommand { get; }
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand StopPreviewCommand { get; }
        public ICommand OpenFolderCommand { get; }

        private void RefreshDevices()
        {
            var previousLink = SelectedDevice?.SymbolicLink;

            var list = CameraCaptureSession.EnumerateDevices();
            Devices.Clear();
            foreach (var device in list)
            {
                Devices.Add(device);
            }

            if (Devices.Count == 0)
            {
                SelectedDevice = null;
                StatusMessage = "未检测到 USB 摄像头，请确认设备已连接并允许应用访问摄像头。";
                return;
            }

            var match = previousLink == null
                ? null
                : Devices.FirstOrDefault(d => d.SymbolicLink == previousLink);
            SelectedDevice = match ?? Devices[0];
        }

        private void StartPreview()
        {
            CameraCaptureSession session;
            try
            {
                session = new CameraCaptureSession();
                session.Open(SelectedDevice.SymbolicLink, 640, 480);
            }
            catch (Exception ex)
            {
                StatusMessage = "打开摄像头失败：" + ex.Message;
                IsPreviewing = false;
                return;
            }

            lock (_sync)
            {
                _session = session;
                _stopRequested = false;
                _nullFrameCount = 0;
                _receivedFrames = 0;
                _captureThread = new Thread(CaptureLoop)
                {
                    IsBackground = true,
                    Name = "CameraPreview"
                };
                _captureThread.Start();
            }

            IsPreviewing = true;
            _previewWatchdog.Stop();
            _previewWatchdog.Start();
            StatusMessage = $"正在预览：{SelectedDevice.Name}（{session.Width}×{session.Height}）";
        }

        private void CaptureLoop()
        {
            var clock = Stopwatch.StartNew();
            long lastPreviewTick = -1;
            long lastWriteTick = -1;

            var readClock = Stopwatch.StartNew();
            LogHelper.Info("CameraRecorder: 采集线程已启动");
            while (!_stopRequested)
            {
                CameraCaptureSession session;
                lock (_sync)
                {
                    session = _session;
                }
                if (session == null) break;

                BitmapSource frame;
                try
                {
                    var before = readClock.ElapsedMilliseconds;
                    frame = session.ReadFrame();
                    var cost = readClock.ElapsedMilliseconds - before;
                    if (cost > 1500)
                    {
                        LogHelper.Info($"CameraRecorder: ReadFrame 阻塞 {cost}ms");
                    }
                }
                catch (Exception ex)
                {
                    PostStatus("读取摄像头画面失败：" + ex.Message);
                    break;
                }

                if (frame == null)
                {
                    _nullFrameCount++;
                    if (_nullFrameCount > 300)
                    {
                        PostStatus("摄像头没有返回画面，请检查设备是否被其它程序占用。");
                        break;
                    }
                    Thread.Sleep(10);
                    continue;
                }
                _nullFrameCount = 0;
                if (_receivedFrames == 0)
                {
                    _receivedFrames = 1;
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() => _previewWatchdog.Stop()));
                }

                long now = clock.ElapsedMilliseconds;
                if (lastPreviewTick < 0 || now - lastPreviewTick >= 60)
                {
                    lastPreviewTick = now;
                    var preview = frame;
                    Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (!_disposed) PreviewImage = preview;
                    }));
                }

                if (_recording && (lastWriteTick < 0 || now - lastWriteTick >= 1000 / Math.Max(1, _recordFrameRate)))
                {
                    lastWriteTick = now;
                    WriteFrame(frame);
                }
            }

            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_disposed) return;
                IsPreviewing = false;
                OnPropertyChanged(nameof(RecordingStateText));
            }));
        }

        private void WriteFrame(BitmapSource frame)
        {
            try
            {
                int width = frame.PixelWidth - (frame.PixelWidth & 1);
                int height = frame.PixelHeight - (frame.PixelHeight & 1);
                BitmapSource source = frame;
                if (width != frame.PixelWidth || height != frame.PixelHeight)
                {
                    var cropped = new CroppedBitmap(frame, new Int32Rect(0, 0, width, height));
                    cropped.Freeze();
                    source = cropped;
                }

                var bytes = MjpegFrameEncoder.Encode(source, 75);
                if (bytes == null) return;

                lock (_sync)
                {
                    if (_writer != null)
                    {
                        _writer.AddFrame(bytes);
                        _frameCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                PostStatus("写入视频帧失败：" + ex.Message);
            }
        }

        private void StartRecording()
        {
            if (SelectedDevice == null)
            {
                StatusMessage = "请先选择 USB 摄像头。";
                return;
            }

            if (!IsPreviewing)
            {
                StartPreview();
            }
            if (!IsPreviewing)
            {
                return;
            }

            int width;
            int height;
            lock (_sync)
            {
                if (_session == null)
                {
                    StatusMessage = "摄像头未就绪，无法开始录制。";
                    return;
                }
                width = _session.Width - (_session.Width & 1);
                height = _session.Height - (_session.Height & 1);
            }

            try
            {
                Directory.CreateDirectory(OutputFolder);
                string safeName = MakeSafeFileName(SelectedDevice.Name);
                string path = Path.Combine(OutputFolder,
                    $"Camera_{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.avi");

                var writer = new AviMjpegWriter(path, width, height, SelectedFrameRate);
                lock (_sync)
                {
                    _writer = writer;
                    _frameCount = 0;
                }

                OutputPath = path;
                _recordWatch = Stopwatch.StartNew();
                _recording = true;
                OnPropertyChanged(nameof(IsRecording));
                OnPropertyChanged(nameof(CanSwitchDevice));
                OnPropertyChanged(nameof(RecordingStateText));
                StatusMessage = "正在录制：" + Path.GetFileName(path);
                _statusTimer.Start();
                RefreshStatus();
            }
            catch (Exception ex)
            {
                StatusMessage = "开始录制失败：" + ex.Message;
            }
        }

        private void StopRecording()
        {
            if (!_recording) return;

            _recording = false;
            lock (_sync)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = "保存视频文件失败：" + ex.Message;
                    }
                    _writer = null;
                }
            }

            if (_recordWatch != null)
            {
                _recordWatch.Stop();
            }

            OnPropertyChanged(nameof(IsRecording));
            OnPropertyChanged(nameof(CanSwitchDevice));
            OnPropertyChanged(nameof(RecordingStateText));
            RefreshStatus();
            StatusMessage = $"录制完成：{OutputPath}";
        }

        /// <summary>
        /// 停止录制与预览，释放摄像头设备与文件句柄。
        /// </summary>
        private void StopCapture()
        {
            _previewWatchdog.Stop();
            _recording = false;
            lock (_sync)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.Dispose();
                    }
                    catch
                    {
                        // 停止/切换设备时忽略文件收尾异常
                    }
                    _writer = null;
                }
            }

            Thread thread;
            lock (_sync)
            {
                _stopRequested = true;
                thread = _captureThread;
                _captureThread = null;
            }

            if (thread != null && thread != Thread.CurrentThread)
            {
                thread.Join(1500);
            }

            CameraCaptureSession session;
            lock (_sync)
            {
                session = _session;
                _session = null;
            }

            if (session != null)
            {
                if (thread == null || !thread.IsAlive)
                {
                    session.Dispose();
                }
                else
                {
                    // 采集线程仍阻塞在原生 ReadSample 中：等它退出后再释放设备，避免并发释放导致崩溃
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        thread.Join(3000);
                        try
                        {
                            session.Dispose();
                        }
                        catch
                        {
                            // 释放失败时忽略，由进程退出兜底
                        }
                    });
                }
            }

            if (_recordWatch != null)
            {
                _recordWatch.Stop();
            }
            _statusTimer.Stop();

            PreviewImage = null;
            IsPreviewing = false;
            OnPropertyChanged(nameof(IsRecording));
            OnPropertyChanged(nameof(CanSwitchDevice));
            OnPropertyChanged(nameof(RecordingStateText));
            ElapsedText = "00:00:00";
            FrameCountText = "0";
        }

        private void StopPreview()
        {
            StopCapture();
            StatusMessage = "预览已停止。";
        }

        private void RefreshStatus()
        {
            if (_recording)
            {
                if (_recordWatch != null)
                {
                    ElapsedText = _recordWatch.Elapsed.ToString(@"hh\:mm\:ss");
                }
                FrameCountText = _frameCount.ToString();
            }
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

        private static string MakeSafeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Camera";
            var chars = name.Select(c => Path.GetInvalidFileNameChars().Contains(c) || char.IsWhiteSpace(c) ? '_' : c)
                            .ToArray();
            return new string(chars).Trim('_');
        }

        private void PostStatus(string message)
        {
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_disposed) StatusMessage = message;
            }));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _statusTimer.Stop();
            StopCapture();
        }
    }
}
