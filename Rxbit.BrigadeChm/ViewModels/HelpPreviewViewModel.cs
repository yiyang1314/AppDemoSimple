using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Rxbit.BrigadeChm.Models;
using Rxbit.BrigadeChm.Services;

namespace Rxbit.BrigadeChm.ViewModels
{
    public class HelpPreviewViewModel : ObservableObject
    {
        private readonly HelpContentModel _contentModel;
        private readonly ChmCompiler _chmCompiler;
        private readonly HtmlGenerator _htmlGenerator;

        private string _currentHtmlPath;
        private string _currentTopicTitle;
        private bool _isCompiling;
        private string _statusMessage;
        private string _searchText;
        private ObservableCollection<HelpTopic> _searchResults;
        private HelpTopic _selectedTopic;
        private string _outputDirectory;
        private string _lastCompiledChmPath;

        public ObservableCollection<HelpSection> Sections => _contentModel.Sections;

        public string CurrentHtmlPath
        {
            get => _currentHtmlPath;
            set => SetProperty(ref _currentHtmlPath, value);
        }

        public string CurrentTopicTitle
        {
            get => _currentTopicTitle;
            set => SetProperty(ref _currentTopicTitle, value);
        }

        public bool IsCompiling
        {
            get => _isCompiling;
            set
            {
                SetProperty(ref _isCompiling, value);
                OnPropertyChanged(nameof(IsNotCompiling));
            }
        }

        public bool IsNotCompiling => !_isCompiling;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                if (!string.IsNullOrEmpty(value) && value.Length >= 1)
                    PerformSearch(value);
                else
                    SearchResults = new ObservableCollection<HelpTopic>();
            }
        }

        public ObservableCollection<HelpTopic> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
        }

        public HelpTopic SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (SetProperty(ref _selectedTopic, value) && value != null)
                    NavigateToTopic(value);
            }
        }

        public string OutputDirectory
        {
            get => _outputDirectory;
            set => SetProperty(ref _outputDirectory, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand CompileChmCommand { get; }
        public ICommand PreviewChmCommand { get; }
        public ICommand OpenOutputFolderCommand { get; }
        public ICommand HomeCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ImportHtmlFileCommand { get; }
        public ICommand ImportFolderCommand { get; }
        public ICommand CompileSingleHtmlCommand { get; }
        public ICommand CompileFolderCommand { get; }

        public event EventHandler<string> NavigateRequest;

        public string GetHtmlForTopic(HelpTopic topic)
        {
            if (topic == null) return "";
            if (topic.ParentSection == null)
            {
                foreach (var s in _contentModel.Sections)
                {
                    if (s.Topics.Contains(topic))
                    {
                        topic.ParentSection = s;
                        break;
                    }
                }
            }
            return _htmlGenerator.GenerateTopicHtml(topic, _contentModel);
        }

        public HelpPreviewViewModel()
        {
            _contentModel = new HelpContentModel();
            _chmCompiler = new ChmCompiler();
            _htmlGenerator = new HtmlGenerator();

            _contentModel.AddDefaultContent();
            _outputDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "BrigadeHelp");

            NavigateCommand = new RelayCommand<HelpTopic>(NavigateToTopic);
            CompileChmCommand = new RelayCommand(async () => await CompileChmAsync());
            PreviewChmCommand = new RelayCommand(PreviewChm);
            OpenOutputFolderCommand = new RelayCommand(OpenOutputFolder);
            HomeCommand = new RelayCommand(GoHome);
            BackCommand = new RelayCommand(() => NavigateRequest?.Invoke(this, "back"));
            ForwardCommand = new RelayCommand(() => NavigateRequest?.Invoke(this, "forward"));
            RefreshCommand = new RelayCommand(() => NavigateRequest?.Invoke(this, "refresh"));
            ImportHtmlFileCommand = new RelayCommand(ImportHtmlFile);
            ImportFolderCommand = new RelayCommand(ImportFolder);
            CompileSingleHtmlCommand = new RelayCommand(async () => await CompileSingleHtmlAsync());
            CompileFolderCommand = new RelayCommand(async () => await CompileFolderAsync());

            SearchResults = new ObservableCollection<HelpTopic>();
            CurrentTopicTitle = _contentModel.Title;
            StatusMessage = "就绪";

            GoHome();
        }

        private void NavigateToTopic(HelpTopic topic)
        {
            if (topic == null) return;

            var htmlDir = Path.Combine(_outputDirectory, "html");
            Directory.CreateDirectory(htmlDir);

            var htmlPath = Path.Combine(htmlDir, topic.FileName);
            if (!File.Exists(htmlPath))
            {
                var html = _htmlGenerator.GenerateTopicHtml(topic, _contentModel);
                File.WriteAllText(htmlPath, html);
            }

            CurrentHtmlPath = htmlPath;
            CurrentTopicTitle = topic.Title;
            NavigateRequest?.Invoke(this, htmlPath);
        }

        private void GoHome()
        {
            var htmlDir = Path.Combine(_outputDirectory, "html");
            Directory.CreateDirectory(htmlDir);

            var indexPath = Path.Combine(htmlDir, "index.html");
            if (!File.Exists(indexPath))
            {
                var html = _htmlGenerator.GenerateIndexHtml(_contentModel);
                File.WriteAllText(indexPath, html);
            }

            CurrentHtmlPath = indexPath;
            CurrentTopicTitle = _contentModel.Title;
            NavigateRequest?.Invoke(this, indexPath);
        }

        private void PerformSearch(string query)
        {
            query = query.ToLower();
            var results = new ObservableCollection<HelpTopic>();

            foreach (var section in _contentModel.Sections)
            {
                foreach (var topic in section.Topics)
                {
                    if (topic.Title.ToLower().Contains(query) ||
                        (topic.Keywords != null && topic.Keywords.ToLower().Contains(query)) ||
                        (topic.HtmlContent != null && topic.HtmlContent.ToLower().Contains(query)))
                    {
                        results.Add(topic);
                    }
                }
            }

            SearchResults = results;
        }

        private async System.Threading.Tasks.Task CompileChmAsync()
        {
            if (_isCompiling) return;

            IsCompiling = true;
            StatusMessage = "正在编译 CHM 帮助文件...";

            try
            {
                var result = await _chmCompiler.CompileAsync(_contentModel, _outputDirectory);

                if (result.Success)
                {
                    StatusMessage = $"编译成功：{result.ChmFilePath}";
                    MessageBox.Show(
                        $"CHM 文件编译成功！\n\n路径：{result.ChmFilePath}\n\n点击【预览 CHM】查看生成的帮助文档。",
                        "编译成功",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = $"编译失败：{result.ErrorMessage}";
                    MessageBox.Show(
                        $"CHM 文件编译失败！\n\n错误：{result.ErrorMessage}",
                        "编译失败",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"异常：{ex.Message}";
                MessageBox.Show(
                    $"编译过程发生异常：{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsCompiling = false;
            }
        }

        private void PreviewChm()
        {
            var chmPath = _contentModel.ChmFilePath;
            if (string.IsNullOrEmpty(chmPath) || !File.Exists(chmPath))
                chmPath = _lastCompiledChmPath;

            if (!string.IsNullOrEmpty(chmPath) && File.Exists(chmPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = chmPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"无法打开 CHM 文件：{ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(
                    "请先编译 CHM 文件，然后再预览。\n\n点击【编译 CHM】或【HTML→CHM】按钮开始编译。",
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void ImportHtmlFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HTML 文件 (*.html;*.htm)|*.html;*.htm|所有文件 (*.*)|*.*",
                Title = "选择 HTML 文件"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                var section = _contentModel.LoadFromHtmlFile(dialog.FileName);
                StatusMessage = $"已导入 HTML 文件：{section.Topics[0].Title}";

                if (section.Topics.Count > 0)
                {
                    NavigateToTopic(section.Topics[0]);
                }

                MessageBox.Show(
                    $"导入成功！\n\n文件：{dialog.FileName}\n主题：{section.Topics[0].Title}",
                    "导入成功",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"导入失败：{ex.Message}";
                MessageBox.Show(
                    $"导入 HTML 文件失败：{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ImportFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择包含 HTML 文件的文件夹",
                ShowNewFolderButton = false
            })
            {
                dialog.RootFolder = Environment.SpecialFolder.Desktop;
                if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                try
                {
                    var count = _contentModel.LoadFromDirectory(dialog.SelectedPath);
                    StatusMessage = $"已批量导入 {count} 个 HTML 文件";

                    MessageBox.Show(
                        $"批量导入成功！\n\n文件夹：{dialog.SelectedPath}\n导入文件数：{count}",
                        "导入成功",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"导入失败：{ex.Message}";
                    MessageBox.Show(
                        $"批量导入失败：{ex.Message}",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void OpenOutputFolder()
        {
            try
            {
                Directory.CreateDirectory(_outputDirectory);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select, \"{_outputDirectory}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开文件夹：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task CompileSingleHtmlAsync()
        {
            if (_isCompiling) return;

            var fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HTML 文件 (*.html;*.htm)|*.html;*.htm|所有文件 (*.*)|*.*",
                Title = "选择要编译为 CHM 的 HTML 文件"
            };

            if (fileDialog.ShowDialog() != true) return;

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CHM 帮助文件 (*.chm)|*.chm",
                FileName = Path.GetFileNameWithoutExtension(fileDialog.FileName) + ".chm",
                Title = "保存 CHM 文件"
            };

            if (saveDialog.ShowDialog() != true) return;

            var outputDir = Path.Combine(
                Path.GetDirectoryName(saveDialog.FileName) ?? _outputDirectory,
                "chm_temp_" + Guid.NewGuid().ToString("N").Substring(0, 8));

            IsCompiling = true;
            StatusMessage = "正在编译 HTML → CHM...";

            try
            {
                var result = await _chmCompiler.CompileFromHtmlFilesAsync(
                    new System.Collections.Generic.List<string> { fileDialog.FileName },
                    outputDir,
                    Path.GetFileNameWithoutExtension(fileDialog.FileName));

                if (result.Success)
                {
                    var destChm = saveDialog.FileName;
                    if (File.Exists(result.ChmFilePath) && result.ChmFilePath != destChm)
                    {
                        if (File.Exists(destChm)) try { File.Delete(destChm); } catch { }
                        File.Move(result.ChmFilePath, destChm);
                    }

                    _lastCompiledChmPath = destChm;
                    StatusMessage = $"编译成功：{destChm}";

                    MessageBox.Show(
                        $"HTML → CHM 编译成功！\n\n源文件：{fileDialog.FileName}\nCHM 文件：{destChm}",
                        "编译成功",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = $"编译失败：{result.ErrorMessage}";
                    MessageBox.Show(
                        $"编译失败！\n\n错误：{result.ErrorMessage}",
                        "编译失败",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"异常：{ex.Message}";
                MessageBox.Show(
                    $"编译过程发生异常：{ex.Message}",
                    "错误",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsCompiling = false;

                try
                {
                    if (Directory.Exists(outputDir))
                        Directory.Delete(outputDir, true);
                }
                catch { }
            }
        }

        private async System.Threading.Tasks.Task CompileFolderAsync()
        {
            if (_isCompiling) return;

            using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "选择包含 HTML 文件的文件夹（将批量编译为单个 CHM）",
                ShowNewFolderButton = false
            })
            {
                if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                var htmlFiles = Directory.GetFiles(folderDialog.SelectedPath, "*.html", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(folderDialog.SelectedPath, "*.htm", SearchOption.AllDirectories))
                    .OrderBy(f => f)
                    .ToList();

                if (htmlFiles.Count == 0)
                {
                    MessageBox.Show(
                        "所选文件夹中未找到 HTML 文件。",
                        "提示",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CHM 帮助文件 (*.chm)|*.chm",
                    FileName = Path.GetFileName(folderDialog.SelectedPath) + ".chm",
                    Title = "保存 CHM 文件"
                };

                if (saveDialog.ShowDialog() != true) return;

                var outputDir = Path.Combine(
                    Path.GetDirectoryName(saveDialog.FileName) ?? _outputDirectory,
                    "chm_temp_" + Guid.NewGuid().ToString("N").Substring(0, 8));

                IsCompiling = true;
                StatusMessage = $"正在批量编译 {htmlFiles.Count} 个 HTML 文件 → CHM...";

                try
                {
                    var result = await _chmCompiler.CompileFromHtmlFilesAsync(
                        htmlFiles,
                        outputDir,
                        Path.GetFileNameWithoutExtension(saveDialog.FileName));

                    if (result.Success)
                    {
                        var destChm = saveDialog.FileName;
                        if (File.Exists(result.ChmFilePath) && result.ChmFilePath != destChm)
                        {
                            if (File.Exists(destChm)) try { File.Delete(destChm); } catch { }
                            File.Move(result.ChmFilePath, destChm);
                        }

                        _lastCompiledChmPath = destChm;
                        StatusMessage = $"编译成功：{destChm}";

                        MessageBox.Show(
                            $"批量编译成功！\n\n文件夹：{folderDialog.SelectedPath}\nHTML 文件数：{htmlFiles.Count}\nCHM 文件：{destChm}",
                            "编译成功",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = $"编译失败：{result.ErrorMessage}";
                        MessageBox.Show(
                            $"编译失败！\n\n错误：{result.ErrorMessage}",
                            "编译失败",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"异常：{ex.Message}";
                    MessageBox.Show(
                        $"编译过程发生异常：{ex.Message}",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    IsCompiling = false;

                    try
                    {
                        if (Directory.Exists(outputDir))
                            Directory.Delete(outputDir, true);
                    }
                    catch { }
                }
            }
        }
    }
}
