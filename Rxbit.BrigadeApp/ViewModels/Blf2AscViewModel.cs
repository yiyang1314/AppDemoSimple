using Microsoft.Win32;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.Helper;
using Rxbit.Brigade.Models.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class Blf2AscViewModel : ViewModelBase
    {
        private string _sourceFilePath;
        public string SourceFilePath
        {
            get { return _sourceFilePath; }
            set { SetProperty(ref _sourceFilePath, value); }
        }

        private string _outputFilePath;
        public string OutputFilePath
        {
            get { return _outputFilePath; }
            set { SetProperty(ref _outputFilePath, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public AsyncRelayCommand SelectFileCommand { get; }
        public AsyncRelayCommand BlfToAscCommand { get; }
        public AsyncRelayCommand BlfToCsvCommand { get; }
        public AsyncRelayCommand AscToCsvCommand { get; }

        public Blf2AscViewModel()
        {
            SelectFileCommand = new AsyncRelayCommand(SelectFile);
            BlfToAscCommand = new AsyncRelayCommand(ConvertBlfToAsc);
            BlfToCsvCommand = new AsyncRelayCommand(ConvertBlfToCsv);
            AscToCsvCommand = new AsyncRelayCommand(ConvertAscToCsv);
        }

        private async Task SelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BLF 文件 (*.blf)|*.blf|ASC 文件 (*.asc)|*.asc|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择源文件";
            if (openFileDialog.ShowDialog() == true)
            {
                SourceFilePath = openFileDialog.FileName;
                StatusMessage = $"已选择文件: {Path.GetFileName(SourceFilePath)}";
            }
        }

        private async Task ConvertBlfToAsc()
        {
            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
            {
                StatusMessage = "请先选择有效的源文件";
                return;
            }

            if (!SourceFilePath.ToLower().EndsWith(".blf"))
            {
                StatusMessage = "请选择 BLF 文件";
                return;
            }

            try
            {
                StatusMessage = "正在转换 BLF 到 ASC...";
                var blfParser = new BlfParser();
                int result = blfParser.Parse(SourceFilePath);

                OutputFilePath = SourceFilePath.Replace(".blf", ".asc");
                if (result == 0 && File.Exists(OutputFilePath))
                {
                    StatusMessage = $"转换成功！输出文件: {Path.GetFileName(OutputFilePath)}";
                }
                else
                {
                    StatusMessage = "转换失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
        }

        private async Task ConvertBlfToCsv()
        {
            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
            {
                StatusMessage = "请先选择有效的源文件";
                return;
            }

            if (!SourceFilePath.ToLower().EndsWith(".blf"))
            {
                StatusMessage = "请选择 BLF 文件";
                return;
            }

            try
            {
                StatusMessage = "正在转换 BLF 到 CSV...";
                var blfParser = new BlfParser();
                var frames = blfParser.ParseBlfFile(SourceFilePath);

                OutputFilePath = SourceFilePath.Replace(".blf", ".csv");
                // SaveFramesToCsv(frames, OutputFilePath);
                FileParser.ParseCsvFile(frames, OutputFilePath);
                if (File.Exists(OutputFilePath))
                {
                    StatusMessage = $"转换成功！输出文件: {Path.GetFileName(OutputFilePath)}";
                }
                else
                {
                    StatusMessage = "转换失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
        }

        private async Task ConvertAscToCsv()
        {
            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
            {
                StatusMessage = "请先选择有效的源文件";
                return;
            }

            if (!SourceFilePath.ToLower().EndsWith(".asc"))
            {
                StatusMessage = "请选择 ASC 文件";
                return;
            }

            try
            {
                StatusMessage = "正在转换 ASC 到 CSV...";
                var frames = FileParser.ParseAscFile(SourceFilePath);
                OutputFilePath = SourceFilePath.Replace(".asc", ".csv");
                FileParser.ParseCsvFile(frames, OutputFilePath);
                if (File.Exists(OutputFilePath))
                {
                    StatusMessage = $"转换成功！输出文件: {Path.GetFileName(OutputFilePath)}";
                }
                else
                {
                    StatusMessage = "转换失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
        }
    }
}