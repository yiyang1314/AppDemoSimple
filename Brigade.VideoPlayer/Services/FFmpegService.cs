using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Brigade.VideoPlayer.Services
{
    /// <summary>
    /// FFmpeg 服务：检测、执行命令、视频生成
    /// </summary>
    public class FFmpegService
    {
        private string _ffmpegPath;
        private string _ffprobePath;

        public bool IsAvailable { get; private set; }
        public string FFmpegPath => _ffmpegPath ?? "未找到";
        public string FFprobePath => _ffprobePath ?? "未找到";

        public event Action<double> ProgressChanged;
        public event Action<string> StatusChanged;
        public event Action<string>? ErrorOccurred;

        /// <summary>
        /// 检测 FFmpeg 是否可用
        /// </summary>
        public bool DetectFFmpeg()
        {
            StatusChanged?.Invoke("正在检测 FFmpeg...");

            var ffmpegCandidates = new List<string> { "ffmpeg" };
            var ffprobeCandidates = new List<string> { "ffprobe" };

            // 常见安装路径
            string[] commonDirs =
            {
                @"C:\ffmpeg\bin",
                @"C:\Program Files\ffmpeg\bin",
                @"C:\Program Files (x86)\ffmpeg\bin",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"ffmpeg\bin"),
            };

            foreach (var dir in commonDirs)
            {
                if (Directory.Exists(dir))
                {
                    ffmpegCandidates.Insert(0, Path.Combine(dir, "ffmpeg.exe"));
                    ffprobeCandidates.Insert(0, Path.Combine(dir, "ffprobe.exe"));
                }
            }

            // 应用程序同级目录
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            ffmpegCandidates.Add(Path.Combine(appDir, "ffmpeg.exe"));
            ffmpegCandidates.Add(Path.Combine(appDir, "ffmpeg", "ffmpeg.exe"));
            ffmpegCandidates.Add(Path.Combine(appDir, "Tools", "ffmpeg.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "ffprobe.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "ffmpeg", "ffprobe.exe"));
            ffprobeCandidates.Add(Path.Combine(appDir, "Tools", "ffprobe.exe"));

            // 检测
            foreach (var path in ffmpegCandidates)
            {
                if (TestExecutable(path))
                {
                    _ffmpegPath = path;
                    break;
                }
            }

            foreach (var path in ffprobeCandidates)
            {
                if (TestExecutable(path))
                {
                    _ffprobePath = path;
                    break;
                }
            }

            IsAvailable = _ffmpegPath != null;

            if (IsAvailable)
            {
                StatusChanged?.Invoke("✓ FFmpeg 已就绪");
            }
            else
            {
                StatusChanged?.Invoke("✗ FFmpeg 未安装");
            }

            return IsAvailable;
        }

        /// <summary>
        /// 测试可执行文件
        /// </summary>
        private bool TestExecutable(string path)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
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
        /// 手动设置 FFmpeg 路径
        /// </summary>
        public void SetFFmpegPath(string ffmpegExePath)
        {
            if (string.IsNullOrEmpty(ffmpegExePath) || !File.Exists(ffmpegExePath))
                throw new FileNotFoundException("FFmpeg 可执行文件未找到", ffmpegExePath);

            var dir = Path.GetDirectoryName(ffmpegExePath);
            _ffmpegPath = ffmpegExePath;
            var probePath = Path.Combine(dir, "ffprobe.exe");
            _ffprobePath = File.Exists(probePath) ? probePath : ffmpegExePath;
            IsAvailable = TestExecutable(ffmpegExePath);
        }

        /// <summary>
        /// 获取媒体时长（秒）
        /// </summary>
        public double GetMediaDuration(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return 0;

            try
            {
                // 优先使用 ffprobe
                if (!string.IsNullOrEmpty(_ffprobePath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = _ffprobePath,
                        Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    using (var proc = Process.Start(psi))
                    {
                        var output = proc.StandardOutput.ReadToEnd().Trim();
                        proc.WaitForExit(5000);
                        if (double.TryParse(output, out var dur))
                            return dur;
                    }
                }
                // 回退到 ffmpeg
                else if (!string.IsNullOrEmpty(_ffmpegPath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = _ffmpegPath,
                        Arguments = $"-i \"{filePath}\" 2>&1 | findstr /i \"Duration\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    using (var proc = Process.Start(psi))
                    {
                        var output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit(5000);
                        // 解析 Duration: 00:05:23.45
                        var match = System.Text.RegularExpressions.Regex.Match(output, @"Duration:\s*(\d+):(\d+):(\d+)\.(\d+)");
                        if (match.Success)
                        {
                            int h = int.Parse(match.Groups[1].Value);
                            int m = int.Parse(match.Groups[2].Value);
                            int s = int.Parse(match.Groups[3].Value);
                            int ms = int.Parse(match.Groups[4].Value);
                            return h * 3600 + m * 60 + s + ms / 100.0;
                        }
                    }
                }
            }
            catch { }

            return 0;
        }

        /// <summary>
        /// 生成视频：图片序列 + MP3 → MP4
        /// </summary>
        public async Task GenerateVideoAsync(
            List<(string imagePath, double duration)> images,
            string mp3Path,
            string outputPath,
            int width = 1920,
            int height = 1080,
            int frameRate = 25)
        {
            if (!IsAvailable)
                throw new InvalidOperationException("FFmpeg 未安装");

            if (images == null || images.Count == 0)
                throw new ArgumentException("图片列表不能为空");

            // 创建临时目录
            var tempDir = Path.Combine(Path.GetTempPath(), "VideoMaker_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                StatusChanged?.Invoke("准备生成...");

                // 生成 concat 文件
                var concatFile = Path.Combine(tempDir, "images.txt");
                using (var sw = new StreamWriter(concatFile))
                {
                    foreach (var (imagePath, duration) in images)
                    {
                        var d = duration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
                        sw.WriteLine($"file '{imagePath}'");
                        sw.WriteLine($"duration {d}");
                    }
                    // FFmpeg concat demuxer 需要最后一张图重复
                    sw.WriteLine($"file '{images.Last().imagePath}'");
                }

                var totalDuration = images.Sum(x => x.duration);

                StatusChanged?.Invoke("正在生成视频...");

                // FFmpeg 命令
                var args = $"-y -f concat -safe 0 -i \"{concatFile}\" " +
                           $"-i \"{mp3Path}\" " +
                           $"-c:v libx264 -pix_fmt yuv420p " +
                           $"-r {frameRate} " +
                           $"-vf \"scale={width}:{height},fps={frameRate}\" " +
                           $"-c:a aac -b:a 192k " +
                           $"-t {totalDuration.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)} " +
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

                    string line;
                    while ((line = proc.StandardError.ReadLine()) != null)
                    {
                        var timeIdx = line.IndexOf("time=");
                        if (timeIdx >= 0)
                        {
                            var timeStr = line.Substring(timeIdx + 5);
                            var spaceIdx = timeStr.IndexOf(' ');
                            if (spaceIdx > 0) timeStr = timeStr.Substring(0, spaceIdx);

                            if (TimeSpan.TryParse(timeStr, out var elapsed))
                            {
                                var pct = (elapsed.TotalSeconds / totalDuration) * 100;
                                ProgressChanged?.Invoke(pct);
                                StatusChanged?.Invoke($"编码中... {elapsed:mm\\:ss} / {TimeSpan.FromSeconds(totalDuration):mm\\:ss}");
                            }
                        }
                    }

                    proc.WaitForExit();

                    if (proc.ExitCode != 0)
                        throw new Exception("FFmpeg 编码失败");
                }

                StatusChanged?.Invoke("✓ 生成完成！");
                ProgressChanged?.Invoke(100);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }
        }

        /// <summary>
        /// 获取视频文件信息
        /// </summary>
        public (double duration, int width, int height, double frameRate) GetVideoInfo(string videoPath)
        {
            if (string.IsNullOrEmpty(_ffprobePath))
                return (0, 0, 0, 0);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = _ffprobePath,
                    Arguments = $"-v error -show_entries format=duration:stream=width,height,r_frame_rate -of default=noprint_wrappers=1 \"{videoPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    var output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(5000);

                    double duration = 0;
                    int width = 0, height = 0;
                    double fps = 0;

                    var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Split('=');
                        if (parts.Length != 2) continue;

                        switch (parts[0].Trim())
                        {
                            case "duration":
                                double.TryParse(parts[1].Trim(), out duration);
                                break;
                            case "width":
                                int.TryParse(parts[1].Trim(), out width);
                                break;
                            case "height":
                                int.TryParse(parts[1].Trim(), out height);
                                break;
                            case "r_frame_rate":
                                var fpsParts = parts[1].Trim().Split('/');
                                if (fpsParts.Length == 2 &&
                                    double.TryParse(fpsParts[0], out var num) &&
                                    double.TryParse(fpsParts[1], out var den) && den > 0)
                                    fps = num / den;
                                break;
                        }
                    }

                    return (duration, width, height, fps);
                }
            }
            catch
            {
                return (0, 0, 0, 0);
            }
        }
    }
}
