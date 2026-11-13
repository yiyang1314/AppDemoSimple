using Brigade.VideoPlayer.Models;
using Brigade.VideoPlayer.Services;
using Brigade.VideoPlayer.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;

namespace Brigade.VideoPlayer.Views
{
    public partial class VideoMakerView : System.Windows.Controls.UserControl
    {
        private VideoMakerViewModel _viewModel;
        private FFmpegService _ffmpegService;
        private bool _initialized;

        public VideoMakerView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                _ffmpegService = new FFmpegService();
                _ffmpegService.DetectFFmpeg();

                _viewModel = new VideoMakerViewModel(_ffmpegService);
                DataContext = _viewModel;

                _viewModel.SelectImageFilesRequested += OnSelectImageFiles;
                _viewModel.SelectImageFolderRequested += OnSelectImageFolder;
                _viewModel.SelectMp3FileRequested += OnSelectMp3;
                _viewModel.SelectOutputFolderRequested += OnSelectOutput;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"视频制作模块初始化失败：{ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectImageFilesRequested -= OnSelectImageFiles;
                _viewModel.SelectImageFolderRequested -= OnSelectImageFolder;
                _viewModel.SelectMp3FileRequested -= OnSelectMp3;
                _viewModel.SelectOutputFolderRequested -= OnSelectOutput;
            }
            Unloaded -= OnUnloaded;
        }

        private void OnSelectImageFiles()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "选择图片文件";
            dialog.Multiselect = true;
            dialog.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                _viewModel.AddImageFiles(dialog.FileNames);
            }
        }

        private void OnSelectImageFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "选择包含图片的文件夹";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.AddImageFolder(dialog.SelectedPath);
                }
            }
        }

        private void OnSelectMp3()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Title = "选择MP3配乐文件";
            dialog.Filter = "MP3音频|*.mp3|所有音频|*.mp3;*.wav;*.aac;*.m4a|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                _viewModel.SetMp3File(dialog.FileName);
            }
        }

        private void OnSelectOutput()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = "选择视频输出目录";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.SetOutputPath(dialog.SelectedPath);
                }
            }
        }
    }
}
