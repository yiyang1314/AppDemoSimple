using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rxbit.BrigadeChm.Models;

namespace Rxbit.BrigadeChm.Services
{
    public class ChmCompiler
    {
        private readonly HtmlGenerator _htmlGenerator;
        public string LastError { get; private set; }
        public string LastOutput { get; private set; }
        public bool IsCompiling { get; private set; }

        public ChmCompiler()
        {
            _htmlGenerator = new HtmlGenerator();
        }

        public async Task<ChmCompileResult> CompileAsync(HelpContentModel model, string outputDir)
        {
            var result = new ChmCompileResult();

            try
            {
                IsCompiling = true;
                LastError = null;
                LastOutput = null;

                model.RebuildIndex();

                var htmlDir = Path.Combine(outputDir, "html");
                Directory.CreateDirectory(outputDir);

                _htmlGenerator.WriteAllFiles(model, outputDir);

                var existingChm = Path.Combine(outputDir, "brigade_help.chm");
                if (File.Exists(existingChm))
                {
                    try { File.Delete(existingChm); } catch { }
                }

                GenerateHhpFile(model, outputDir);
                GenerateHhcFile(model, outputDir);
                GenerateHhkFile(model, outputDir);

                var chmFilePath = Path.Combine(outputDir, "brigade_help.chm");
                model.ChmFilePath = chmFilePath;

                await RunHhcAsync(model, outputDir);

                result.Success = true;
                result.ChmFilePath = chmFilePath;
                result.OutputLog = LastOutput;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                LastError = ex.Message;
            }
            finally
            {
                IsCompiling = false;
            }

            return result;
        }

        public async Task<ChmCompileResult> CompileFromHtmlFilesAsync(List<string> htmlFiles, string outputDir, string title = "帮助文档")
        {
            var result = new ChmCompileResult();

            try
            {
                IsCompiling = true;
                LastError = null;
                LastOutput = null;

                if (htmlFiles == null || htmlFiles.Count == 0)
                    throw new ArgumentException("至少需要一个 HTML 文件");

                Directory.CreateDirectory(outputDir);

                var htmlDir = Path.Combine(outputDir, "html");
                if (Directory.Exists(htmlDir))
                {
                    try { Directory.Delete(htmlDir, true); } catch { }
                }
                Directory.CreateDirectory(htmlDir);

                var fileInfos = new List<(string sourcePath, string fileName, string title)>();

                for (int i = 0; i < htmlFiles.Count; i++)
                {
                    var src = htmlFiles[i];
                    if (!File.Exists(src)) continue;

                    var safeName = SanitizeFileName(Path.GetFileNameWithoutExtension(src));
                    var ext = Path.GetExtension(src).ToLower();
                    var uniqueName = i == 0
                        ? safeName + ext
                        : $"{safeName}_{i}{ext}";

                    var destPath = Path.Combine(htmlDir, uniqueName);
                    File.Copy(src, destPath, true);

                    var extractedTitle = ExtractHtmlTitle(src) ?? Path.GetFileNameWithoutExtension(src);
                    fileInfos.Add((src, uniqueName, extractedTitle));
                }

                GenerateSimpleIndexHtml(htmlDir, fileInfos, title);

                var chmFileName = SanitizeFileName(title) + ".chm";
                var chmFilePath = Path.Combine(outputDir, chmFileName);

                if (File.Exists(chmFilePath))
                {
                    try { File.Delete(chmFilePath); } catch { }
                }

                GenerateHhpFileForFiles(fileInfos, outputDir, title, chmFileName);
                GenerateHhcFileForFiles(fileInfos, outputDir, title);
                GenerateHhkFileForFiles(fileInfos, outputDir);

                await RunHhcAsync(outputDir, chmFilePath);

                result.Success = true;
                result.ChmFilePath = chmFilePath;
                result.OutputLog = LastOutput;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                LastError = ex.Message;
            }
            finally
            {
                IsCompiling = false;
            }

            return result;
        }

        private void GenerateSimpleIndexHtml(string htmlDir, List<(string sourcePath, string fileName, string title)> fileInfos, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine($"<title>{title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body{font-family:'Segoe UI','Microsoft YaHei',sans-serif;margin:20px;background:#1a1b1d;color:#d0d0d0;}");
            sb.AppendLine(".container{max-width:900px;margin:0 auto;background:#1e2a3d;padding:30px 40px;border-radius:8px;}");
            sb.AppendLine("h1{color:#00d9ff;border-bottom:2px solid #0f3460;padding-bottom:10px;}");
            sb.AppendLine("a{color:#00d9ff;text-decoration:none;}");
            sb.AppendLine("a:hover{text-decoration:underline;}");
            sb.AppendLine("li{margin:8px 0;}");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<div class='container'>");
            sb.AppendLine($"<h1>{title}</h1>");
            sb.AppendLine("<h2>目录</h2>");
            sb.AppendLine("<ul>");
            foreach (var fi in fileInfos)
            {
                sb.AppendLine($"<li><a href='{fi.fileName}'>{fi.title}</a></li>");
            }
            sb.AppendLine("</ul>");
            sb.AppendLine("</div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(Path.Combine(htmlDir, "index.html"), sb.ToString(), Encoding.UTF8);
        }

        private void GenerateHhpFileForFiles(List<(string sourcePath, string fileName, string title)> fileInfos, string outputDir, string title, string chmFileName)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[OPTIONS]");
            sb.AppendLine($"Title={title}");
            sb.AppendLine($"CHM={chmFileName}");
            sb.AppendLine("DefaultTopic=html/index.html");
            sb.AppendLine("BinaryIndex=Yes");
            sb.AppendLine("Full-text search=Yes");
            sb.AppendLine("LCID=0x0804");
            sb.AppendLine("Contents=contents.hhc");
            sb.AppendLine("Index=index.hhk");
            sb.AppendLine();
            sb.AppendLine("[FILES]");
            sb.AppendLine("html/index.html");
            foreach (var fi in fileInfos)
            {
                sb.AppendLine($"html/{fi.fileName}");
            }

            File.WriteAllText(Path.Combine(outputDir, "project.hhp"), sb.ToString(), Encoding.GetEncoding(936));
        }

        private void GenerateHhcFileForFiles(List<(string sourcePath, string fileName, string title)> fileInfos, string outputDir, string title)
        {
            var enc = Encoding.GetEncoding(936);
            var sb = new StringBuilder();
            sb.AppendLine("[CONTENTS]");
            sb.AppendLine("DefaultTopic=html/index.html");
            sb.AppendLine("");
            sb.AppendLine("<UL>");
            sb.AppendLine("<LI> <OBJECT type=\"TEXT\">");
            sb.AppendLine("<param name=\"ItemName\" value=\"" + title + "\">");
            sb.AppendLine("</OBJECT>");

            sb.AppendLine("<UL>");
            foreach (var fi in fileInfos)
            {
                sb.AppendLine("<LI> <OBJECT type=\"TEXT\">");
                sb.AppendLine("<param name=\"ItemName\" value=\"" + fi.title + "\">");
                sb.AppendLine("</OBJECT>");
                sb.AppendLine("<OBJECT type=\"HTMLTOPIC\">");
                sb.AppendLine("<param name=\"Local\" value=\"html/" + fi.fileName + "\">");
                sb.AppendLine("</OBJECT>");
            }
            sb.AppendLine("</UL>");

            sb.AppendLine("</UL>");

            File.WriteAllText(Path.Combine(outputDir, "contents.hhc"), sb.ToString(), enc);
        }

        private void GenerateHhkFileForFiles(List<(string sourcePath, string fileName, string title)> fileInfos, string outputDir)
        {
            var enc = Encoding.GetEncoding(936);
            var sb = new StringBuilder();
            sb.AppendLine("[INDEX]");

            foreach (var fi in fileInfos)
            {
                sb.AppendLine(fi.title);
                sb.AppendLine("    html/" + fi.fileName);
            }

            File.WriteAllText(Path.Combine(outputDir, "index.hhk"), sb.ToString(), enc);
        }

        private string ExtractHtmlTitle(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var match = Regex.Match(content, @"<title[^>]*>(.*?)</title>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                return match.Success ? match.Groups[1].Value.Trim() : null;
            }
            catch
            {
                return null;
            }
        }

        private async Task RunHhcAsync(string outputDir, string chmFilePath)
        {
            var hhcPath = FindHhcExePath();
            var hhpPath = Path.Combine(outputDir, "project.hhp");

            if (string.IsNullOrEmpty(hhcPath))
            {
                LastError = "未找到 HTML Help Workshop 编译器 (hhc.exe)。请安装 HTML Help Workshop。";
                throw new InvalidOperationException(LastError);
            }

            using (var process = new Process())
            {
                process.StartInfo.FileName = hhcPath;
                process.StartInfo.Arguments = $"\"{hhpPath}\"";
                process.StartInfo.WorkingDirectory = outputDir;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null) outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit(30000));

                LastOutput = outputBuilder.ToString();
                LastError = errorBuilder.ToString();

                if (!process.HasExited)
                {
                    process.Kill();
                    throw new TimeoutException("CHM 编译超时（超过 30 秒）");
                }

                if (process.ExitCode >= 2)
                {
                    throw new InvalidOperationException($"CHM 编译失败 (退出码 {process.ExitCode}): {LastError}");
                }

                if (process.ExitCode == 1)
                {
                    LastError = $"编译完成（有警告）: {LastOutput}";
                }
            }
        }

        private void GenerateHhpFile(HelpContentModel model, string outputDir)
        {
            var chmFileName = "brigade_help.chm";
            var sb = new StringBuilder();
            sb.AppendLine("[OPTIONS]");
            sb.AppendLine($"Title={model.Title}");
            sb.AppendLine($"CHM={chmFileName}");
            sb.AppendLine("DefaultTopic=html/index.html");
            sb.AppendLine("BinaryIndex=Yes");
            sb.AppendLine("Full-text search=Yes");
            sb.AppendLine("LCID=0x0804");
            sb.AppendLine("Contents=contents.hhc");
            sb.AppendLine("Index=index.hhk");
            sb.AppendLine();
            sb.AppendLine("[FILES]");

            foreach (var section in model.Sections)
            {
                foreach (var topic in section.Topics)
                {
                    sb.AppendLine($"html/{topic.FileName}");
                }
            }
            sb.AppendLine("html/index.html");
            sb.AppendLine("html/search.html");

            var hhpPath = Path.Combine(outputDir, "project.hhp");
            File.WriteAllText(hhpPath, sb.ToString(), Encoding.GetEncoding(936));
        }

        private void GenerateHhcFile(HelpContentModel model, string outputDir)
        {
            var enc = Encoding.GetEncoding(936);
            var sb = new StringBuilder();
            sb.AppendLine("[CONTENTS]");
            sb.AppendLine("DefaultTopic=html/index.html");
            sb.AppendLine("");
            sb.AppendLine("<UL>");

            foreach (var section in model.Sections.OrderBy(s => s.SortOrder))
            {
                sb.AppendLine("<LI> <OBJECT type=\"TEXT\">");
                sb.AppendLine("<param name=\"ItemName\" value=\"" + section.Title + "\">");
                sb.AppendLine("</OBJECT>");

                if (section.Topics.Count > 0)
                {
                    sb.AppendLine("<UL>");
                    foreach (var topic in section.Topics.OrderBy(t => t.SortOrder))
                    {
                        sb.AppendLine("<LI> <OBJECT type=\"TEXT\">");
                        sb.AppendLine("<param name=\"ItemName\" value=\"" + topic.Title + "\">");
                        sb.AppendLine("</OBJECT>");
                        sb.AppendLine("<OBJECT type=\"HTMLTOPIC\">");
                        sb.AppendLine("<param name=\"Local\" value=\"html/" + topic.FileName + "\">");
                        sb.AppendLine("</OBJECT>");
                    }
                    sb.AppendLine("</UL>");
                }
            }

            sb.AppendLine("</UL>");

            var hhcPath = Path.Combine(outputDir, "contents.hhc");
            File.WriteAllText(hhcPath, sb.ToString(), enc);
        }

        private void GenerateHhkFile(HelpContentModel model, string outputDir)
        {
            var enc = Encoding.GetEncoding(936);
            var sb = new StringBuilder();
            sb.AppendLine("[INDEX]");

            var topicIndex = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var section in model.Sections.OrderBy(s => s.SortOrder))
            {
                foreach (var topic in section.Topics.OrderBy(t => t.SortOrder))
                {
                    if (!topicIndex.Contains(topic.Title))
                    {
                        topicIndex.Add(topic.Title);
                        sb.AppendLine(topic.Title);
                        sb.AppendLine("    html/" + topic.FileName);
                    }

                    if (!string.IsNullOrEmpty(topic.Keywords))
                    {
                        var keywords = topic.Keywords.Split(',');
                        foreach (var kw in keywords)
                        {
                            var trimmed = kw.Trim();
                            if (!string.IsNullOrEmpty(trimmed) && !topicIndex.Contains(trimmed))
                            {
                                topicIndex.Add(trimmed);
                                sb.AppendLine(trimmed);
                                sb.AppendLine("    html/" + topic.FileName);
                            }
                        }
                    }
                }
            }

            var hhkPath = Path.Combine(outputDir, "index.hhk");
            File.WriteAllText(hhkPath, sb.ToString(), enc);
        }

        private string FindTopicFile(string topicId, HelpContentModel model)
        {
            var topic = model.FindTopicById(topicId);
            return topic?.FileName ?? "index.html";
        }

        private async Task RunHhcAsync(HelpContentModel model, string outputDir)
        {
            var hhcPath = FindHhcExePath();
            var hhpPath = Path.Combine(outputDir, "project.hhp");

            if (string.IsNullOrEmpty(hhcPath))
            {
                LastError = "未找到 HTML Help Workshop 编译器 (hhc.exe)。请安装 HTML Help Workshop。";
                throw new InvalidOperationException(LastError);
            }

            using (var process = new Process())
            {
                process.StartInfo.FileName = hhcPath;
                process.StartInfo.Arguments = $"\"{hhpPath}\"";
                process.StartInfo.WorkingDirectory = outputDir;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null) outputBuilder.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) errorBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit(30000));

                LastOutput = outputBuilder.ToString();
                LastError = errorBuilder.ToString();

                if (!process.HasExited)
                {
                    process.Kill();
                    throw new TimeoutException("CHM 编译超时（超过 30 秒）");
                }

                if (process.ExitCode >= 2)
                {
                    throw new InvalidOperationException($"CHM 编译失败 (退出码 {process.ExitCode}): {LastError}");
                }

                if (process.ExitCode == 1)
                {
                    LastError = $"编译完成（有警告）: {LastOutput}";
                }
            }
        }

        private string FindHhcExePath()
        {
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var possiblePaths = new[]
            {
                Path.Combine(appDir, "hhc.exe"),
                Path.Combine(appDir, "Tools", "hhc.exe"),
                @"C:\Program Files (x86)\HTML Help Workshop\hhc.exe",
                @"C:\Program Files\HTML Help Workshop\hhc.exe",
                @"C:\Program Files (x86)\Microsoft Help Workshop\hhc.exe",
                @"C:\Program Files\Microsoft Help Workshop\hhc.exe",
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            var envPath = Environment.GetEnvironmentVariable("HTML_HELP_WORKSHOP");
            if (!string.IsNullOrEmpty(envPath))
            {
                var candidate = Path.Combine(envPath, "hhc.exe");
                if (File.Exists(candidate))
                    return candidate;
            }

            try
            {
                using (var result = Process.Start(new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "hhc",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }))
                {
                    if (result != null)
                    {
                        var output = result.StandardOutput.ReadToEnd().Trim();
                        result.WaitForExit();
                        if (result.ExitCode == 0 && File.Exists(output))
                            return output;
                    }
                }
            }
            catch { }

            return null;
        }

        private string SanitizeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
            return sanitized.Replace(" ", "_");
        }
    }

    public class ChmCompileResult
    {
        public bool Success { get; set; }
        public string ChmFilePath { get; set; }
        public string ErrorMessage { get; set; }
        public string OutputLog { get; set; }
    }
}
