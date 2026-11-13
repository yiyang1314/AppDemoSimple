using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Services.Media;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.ViewModels
{
    /// <summary>
    /// 屏幕录制窗口的视图模型。
    /// </summary>
    public class ScreenRecorderViewModel : ViewModelBase, IDisposable
    {
        public class CaptureTargetOption
        {
            public string Name { get; set; }
            public ScreenCaptureTarget Value { get; set; }
            public override string ToString() => Name;
        }

        private readonly ScreenRecorder _recorder = new ScreenRecorder();
        private readonly DispatcherTimer _statusTimer;

        private CaptureTargetOption _selectedTarget;
        private int _selectedFrameRate = 10;
        private string _statusMessage = "准备就绪，点击「开始录制」开始采集屏幕画面。";
        private string _outputPath;
        private bool _isRecording;
        private string _elapsedText = "00:00:00";
        private string _frameCountText = "0";
        private bool _disposed;

        public ScreenRecorderViewModel()
        {
            TargetOptions = new[]
            {
                new CaptureTargetOption { Name = "整个屏幕", Value = ScreenCaptureTarget.FullScreen },
                new CaptureTargetOption { Name = "本软件窗口", Value = ScreenCaptureTarget.ApplicationWindow }
            };
            FrameRateOptions = new[] { 5, 10, 15 };
            _selectedTarget = TargetOptions[0];

            OutputFolder = Path.Combine(RadarConstaint.RecondingPath, "Media");

            StartCommand = new RelayCommand(StartRecording, () => !IsRecording);
            StopCommand = new RelayCommand(StopRecording, () => IsRecording);
            OpenFolderCommand = new RelayCommand(OpenOutputFolder);

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _statusTimer.Tick += (s, e) => RefreshStatus();
        }

        public CaptureTargetOption[] TargetOptions { get; }
        public int[] FrameRateOptions { get; }

        public CaptureTargetOption SelectedTarget
        {
            get => _selectedTarget;
            set => SetProperty(ref _selectedTarget, value);
        }

        public int SelectedFrameRate
        {
            get => _selectedFrameRate;
            set => SetProperty(ref _selectedFrameRate, value);
        }

        public string OutputFolder { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (SetProperty(ref _isRecording, value))
                {
                    OnPropertyChanged(nameof(IsIdle));
                    OnPropertyChanged(nameof(RecordingStateText));
                }
            }
        }

        public bool IsIdle => !IsRecording;

        public string RecordingStateText => IsRecording ? "● REC" : "IDLE";

        public string ElapsedText
        {
            get => _elapsedText;
            set => SetProperty(ref _elapsedText, value);
        }

        public string FrameCountText
        {
            get => _frameCountText;
            set => SetProperty(ref _frameCountText, value);
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenFolderCommand { get; }

        private void StartRecording()
        {
            try
            {
                Directory.CreateDirectory(OutputFolder);
                string fileName = $"Screen_{DateTime.Now:yyyyMMdd_HHmmss}.avi";
                string path = Path.Combine(OutputFolder, fileName);

                IntPtr windowHandle = IntPtr.Zero;
                if (SelectedTarget != null && SelectedTarget.Value == ScreenCaptureTarget.ApplicationWindow)
                {
                    var mainWindow = Application.Current?.MainWindow;
                    if (mainWindow == null)
                    {
                        StatusMessage = "未找到主窗口，无法录制软件窗口。";
                        return;
                    }
                    windowHandle = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
                }

                bool started = _recorder.Start(
                    SelectedTarget?.Value ?? ScreenCaptureTarget.FullScreen,
                    windowHandle,
                    SelectedFrameRate,
                    1280,
                    path);

                if (!started)
                {
                    StatusMessage = "开始录制失败：" + (_recorder.LastError ?? "未知错误");
                    return;
                }

                OutputPath = path;
                IsRecording = true;
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
            try
            {
                _recorder.Stop();
                _statusTimer.Stop();
                IsRecording = false;
                RefreshStatus();

                if (!string.IsNullOrEmpty(_recorder.LastError))
                {
                    StatusMessage = "录制结束，但写入文件时出现异常：" + _recorder.LastError;
                }
                else
                {
                    StatusMessage = $"录制完成：{OutputPath}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "停止录制失败：" + ex.Message;
            }
        }

        private void RefreshStatus()
        {
            if (_recorder.IsRecording || IsRecording)
            {
                ElapsedText = _recorder.Elapsed.ToString(@"hh\:mm\:ss");
                FrameCountText = _recorder.FrameCount.ToString();
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _statusTimer.Stop();
            _recorder.Dispose();
        }
    }
}
