using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Rxbit.BrigadeApp.Services.Media
{
    public enum ScreenCaptureTarget
    {
        FullScreen,
        ApplicationWindow
    }

    /// <summary>
    /// 屏幕/窗口录制：GDI 采帧 + AVI(MJPEG) 写入，离线可用，无需额外驱动。
    /// </summary>
    public sealed class ScreenRecorder : IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private readonly object _sync = new object();
        private AviMjpegWriter _writer;
        private Thread _thread;
        private volatile bool _stopRequested;
        private volatile bool _recording;
        private Stopwatch _watch;
        private int _frameCount;
        private Rectangle _captureBounds;
        private int _outputWidth;
        private int _outputHeight;
        private int _frameRate;
        private IntPtr _windowHandle;
        private ScreenCaptureTarget _target;

        public bool IsRecording => _recording;
        public string OutputPath { get; private set; }
        public string LastError { get; private set; }
        public int FrameCount => _frameCount;
        public TimeSpan Elapsed => _watch == null ? TimeSpan.Zero : _watch.Elapsed;

        public bool Start(ScreenCaptureTarget target, IntPtr windowHandle, int frameRate, int maxWidth, string outputPath)
        {
            lock (_sync)
            {
                if (_recording)
                {
                    return false;
                }

                LastError = null;
                _target = target;
                _windowHandle = windowHandle;
                _frameRate = Math.Max(1, Math.Min(frameRate, 30));

                try
                {
                    _captureBounds = GetCaptureBounds();
                    if (_captureBounds.Width <= 0 || _captureBounds.Height <= 0)
                    {
                        LastError = "无法获取录制区域（屏幕可能不可用）。";
                        return false;
                    }

                    using (var first = CaptureBitmap(_captureBounds))
                    {
                        if (first == null)
                        {
                            LastError = "屏幕采集失败。";
                            return false;
                        }

                        int width = first.Width;
                        int height = first.Height;
                        if (maxWidth > 0 && width > maxWidth)
                        {
                            width = maxWidth;
                            height = (int)Math.Round(first.Height * (double)maxWidth / first.Width);
                        }
                        _outputWidth = width - (width & 1);
                        _outputHeight = height - (height & 1);

                        var scaled = ScaleBitmap(first, _outputWidth, _outputHeight);
                        OutputPath = outputPath;
                        _writer = new AviMjpegWriter(outputPath, _outputWidth, _outputHeight, _frameRate);
                        var bytes = MjpegFrameEncoder.Encode(scaled, 70);
                        if (scaled != first)
                        {
                            scaled.Dispose();
                        }
                        _writer.AddFrame(bytes);
                        _frameCount = 1;
                    }

                    _stopRequested = false;
                    _recording = true;
                    _watch = Stopwatch.StartNew();
                    _thread = new Thread(CaptureLoop) { IsBackground = true, Name = "ScreenRecorder" };
                    _thread.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    Cleanup();
                    return false;
                }
            }
        }

        private Rectangle GetCaptureBounds()
        {
            if (_target == ScreenCaptureTarget.ApplicationWindow && _windowHandle != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(_windowHandle, out rect))
                {
                    return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }
            }

            var screen = Screen.PrimaryScreen;
            return screen == null ? Rectangle.Empty : screen.Bounds;
        }

        private static Bitmap CaptureBitmap(Rectangle bounds)
        {
            try
            {
                var bitmap = new Bitmap(Math.Max(bounds.Width, 1), Math.Max(bounds.Height, 1), PixelFormat.Format24bppRgb);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap ScaleBitmap(Bitmap source, int width, int height)
        {
            if (source.Width == width && source.Height == height)
            {
                return source;
            }

            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(source, new Rectangle(0, 0, width, height));
            }
            return bitmap;
        }

        private void CaptureLoop()
        {
            int interval = 1000 / _frameRate;
            var lastTick = Stopwatch.StartNew();

            while (!_stopRequested)
            {
                try
                {
                    bool windowMinimized = _target == ScreenCaptureTarget.ApplicationWindow
                                           && _windowHandle != IntPtr.Zero
                                           && IsIconic(_windowHandle);

                    if (!windowMinimized)
                    {
                        var bounds = GetCaptureBounds();
                        if (bounds.Width > 0 && bounds.Height > 0)
                        {
                            using (var source = CaptureBitmap(bounds))
                            {
                                if (source != null)
                                {
                                    var scaled = ScaleBitmap(source, _outputWidth, _outputHeight);
                                    try
                                    {
                                        var bytes = MjpegFrameEncoder.Encode(scaled, 70);
                                        lock (_sync)
                                        {
                                            if (_writer != null)
                                            {
                                                _writer.AddFrame(bytes);
                                                _frameCount++;
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (scaled != source)
                                        {
                                            scaled.Dispose();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    long elapsed = lastTick.ElapsedMilliseconds;
                    int sleep = interval - (int)elapsed;
                    if (sleep > 0)
                    {
                        Thread.Sleep(sleep);
                    }
                    lastTick.Restart();
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    _stopRequested = true;
                }
            }
        }

        public void Stop()
        {
            Thread thread;
            lock (_sync)
            {
                if (!_recording && _thread == null)
                {
                    return;
                }

                _recording = false;
                _stopRequested = true;
                thread = _thread;
                _thread = null;
            }

            if (thread != null)
            {
                thread.Join(2000);
            }

            lock (_sync)
            {
                if (_watch != null)
                {
                    _watch.Stop();
                }
                Cleanup();
            }
        }

        private void Cleanup()
        {
            if (_writer != null)
            {
                try
                {
                    _writer.Dispose();
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                }
                _writer = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
