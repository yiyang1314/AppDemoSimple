using Microsoft.Win32;
using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.Utils;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.Helper;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class WinHelperViewModel : ViewModelBase
    {
        private string _binFilePath;
        public string BinFilePath
        {
            get { return _binFilePath; }
            set { SetProperty(ref _binFilePath, value); }
        }

        private string _StartAddres = "00000000";
        public string StartAddress
        {
            get { return _StartAddres; }
            set
            {
                if (value.StartsWith("0x"))
                {
                    SetProperty(ref _StartAddres, value);
                }
                else
                {
                    OnPropertyChanged(nameof(StartAddress));
                }
            }
        }

        private string _hexFilePath;
        public string HexFilePath
        {
            get { return _hexFilePath; }
            set { SetProperty(ref _hexFilePath, value); }
        }

        private string _byteInput;
        public string ByteInput
        {
            get { return _byteInput; }
            set { SetProperty(ref _byteInput, value); }
        }

        private string _stringInput;
        public string StringInput
        {
            get { return _stringInput; }
            set { SetProperty(ref _stringInput, value); }
        }

        private string _hexInput;
        public string HexInput
        {
            get { return _hexInput; }
            set { SetProperty(ref _hexInput, value); }
        }

        private string _crcFilePath;
        public string CrcFilePath
        {
            get { return _crcFilePath; }
            set { SetProperty(ref _crcFilePath, value); }
        }

        private string _crcResult;
        public string CrcResult
        {
            get { return _crcResult; }
            set { SetProperty(ref _crcResult, value); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }


        private int _CrcIndex = 0;
        public int SelectCrcIndex
        {
            get { return _CrcIndex; }
            set { SetProperty(ref _CrcIndex, value); }
        }
        private bool _IsRunning;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set { SetProperty(ref _IsRunning, value); }
        }
        public AsyncRelayCommand SelectBinFileCommand { get; }
        public AsyncRelayCommand SelectHexFileCommand { get; }
        public AsyncRelayCommand BinToHexCommand { get; }
        public AsyncRelayCommand HexToBinCommand { get; }

        public AsyncRelayCommand ByteToStringCommand { get; }
        public AsyncRelayCommand StringToHexCommand { get; }
        public AsyncRelayCommand HexToStringCommand { get; }
        public AsyncRelayCommand SelectCrcFileCommand { get; }
        public AsyncRelayCommand CalculateCrcCommand { get; }

        public WinHelperViewModel()
        {
            SelectBinFileCommand = new AsyncRelayCommand(SelectBinFile);
            SelectHexFileCommand = new AsyncRelayCommand(SelectHexFile);
            BinToHexCommand = new AsyncRelayCommand(ConvertBinToHex, () => !this.IsRunning);
            HexToBinCommand = new AsyncRelayCommand(ConvertHexToBin, () => !this.IsRunning);
            ByteToStringCommand = new AsyncRelayCommand(ConvertByteToString, () => !this.IsRunning);
            StringToHexCommand = new AsyncRelayCommand(ConvertStringToHex, () => !this.IsRunning);
            HexToStringCommand = new AsyncRelayCommand(ConvertHexToString, () => !this.IsRunning);
            SelectCrcFileCommand = new AsyncRelayCommand(SelectCrcFile, () => !this.IsRunning);
            CalculateCrcCommand = new AsyncRelayCommand(CalculateCrc, () => !this.IsRunning);
        }

        private async Task SelectBinFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BIN 文件 (*.bin)|*.bin|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择 BIN 文件";
            if (openFileDialog.ShowDialog() == true)
            {
                BinFilePath = openFileDialog.FileName;
                StatusMessage = $"已选择 BIN 文件: {Path.GetFileName(BinFilePath)}";
            }
        }

        private async Task SelectHexFile()
        {
            //string srcico = $@"E:\tang\ai-workspace\LOGO.png";
            //string tico = $@"E:\tang\ai-workspace\LOGO.ico";
            // await PngToIcoConverter.ConvertIco(srcico, tico);
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "HEX 文件 (*.hex)|*.hex|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择 HEX 文件";
            if (openFileDialog.ShowDialog() == true)
            {
                HexFilePath = openFileDialog.FileName;
                StatusMessage = $"已选择 HEX 文件: {Path.GetFileName(HexFilePath)}";
            }
        }

        private async Task ConvertBinToHex()
        {
            if (string.IsNullOrEmpty(BinFilePath) || !File.Exists(BinFilePath))
            {
                StatusMessage = "请先选择有效的 BIN 文件";
                return;
            }

            try
            {
                StatusMessage = "正在转换 BIN 到 HEX...";
                this.IsRunning = true;
                var outputPath = Path.ChangeExtension(BinFilePath, ".hex");
                string args = $@"{outputPath} 0x{this.StartAddress}";
                if (!CrcCalculator.InvokeExe(SimpleEcuTool.Bin2Hex_EXE, out string result1, args))
                {

                }
                this.HexFilePath = outputPath;
                StatusMessage = $"转换成功！输出文件: {Path.GetFileName(outputPath)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task ConvertHexToBin()
        {
            if (string.IsNullOrEmpty(HexFilePath) || !File.Exists(HexFilePath))
            {
                StatusMessage = "请先选择有效的 HEX 文件";
                return;
            }

            try
            {
                this.IsRunning = true;
                var outputPath = Path.ChangeExtension(HexFilePath, ".bin");
                StatusMessage = "正在转换 HEX 到 BIN...";
                if (CrcCalculator.InvokeExe(SimpleEcuTool.Hex2Bin_EXE, out string result2, HexFilePath))
                {
                    this.BinFilePath = outputPath;
                }
                StatusMessage = $"转换成功！info:\r\n {result2}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task ConvertByteToString()
        {
            if (string.IsNullOrEmpty(ByteInput))
            {
                StatusMessage = "请输入 Byte 数组";
                return;
            }

            try
            {
                this.IsRunning = true;
                var bytes = ParseByteInput(ByteInput);
                StringInput = Encoding.UTF8.GetString(bytes);
                HexInput = BitConverter.ToString(bytes).Replace("-", " ");
                StatusMessage = "转换成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task ConvertStringToHex()
        {
            if (string.IsNullOrEmpty(StringInput))
            {
                StatusMessage = "请输入字符串";
                return;
            }

            try
            {
                this.IsRunning = true;
                var bytes = Encoding.UTF8.GetBytes(StringInput);
                HexInput = BitConverter.ToString(bytes).Replace("-", " ");
                ByteInput = $"new byte[] {{ {string.Join(", ", bytes)} }}";
                StatusMessage = "转换成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task ConvertHexToString()
        {
            if (string.IsNullOrEmpty(HexInput))
            {
                StatusMessage = "请输入 Hex 字符串";
                return;
            }

            try
            {
                this.IsRunning = true;
                var bytes = ParseHexInput(HexInput);
                StringInput = Encoding.UTF8.GetString(bytes);
                ByteInput = $"new byte[] {{ {string.Join(", ", bytes)} }}";
                StatusMessage = "转换成功";
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private async Task SelectCrcFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "HEX 文件 (*.hex)|*.hex|BIN 文件 (*.bin)|*.bin|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择要计算 CRC 的文件";
            if (openFileDialog.ShowDialog() == true)
            {
                CrcFilePath = openFileDialog.FileName;
                StatusMessage = $"已选择文件: {Path.GetFileName(CrcFilePath)}";
            }
        }

        private async Task CalculateCrc()
        {
            if (string.IsNullOrEmpty(CrcFilePath) || !File.Exists(CrcFilePath))
            {
                StatusMessage = "请先选择有效的文件";
                return;
            }

            try
            {
                this.IsRunning = true;
                StatusMessage = "正在计算 CRC...";
                DataBlock block = new DataBlock();
                if (CrcFilePath.EndsWith(".hex"))
                {
                    if (!FlashParseFile.ParseHexFileEx(CrcFilePath, out block) || block.Data.Count == 0)
                    {
                        ParseHexFile parseHexFile = new ParseHexFile();
                        parseHexFile.ParseSigleHexfile(CrcFilePath, ref block);
                    }
                }
                else
                {
                    var list = FlashParseFile.ParseBinFile(CrcFilePath);
                    block = list.FirstOrDefault();
                }
                if (block == null || block.Data.Count == 0)
                {
                    StatusMessage = $"read File :{CrcFilePath} File Faild！";
                    return;
                }
                var crcs = new CrcConfig[] { CrcCalculator.CRC32, CrcCalculator.CRC16, CrcCalculator.CRC8 };
                if (!CrcCalculator.CalculateCrc(block.Data.ToArray(), crcs[SelectCrcIndex], out string result))
                {
                    StatusMessage = "计算失败！";
                    return;
                }
                CrcResult = $"0x{result} ";
                StatusMessage = "计算完成";
            }
            catch (Exception ex)
            {
                StatusMessage = $"计算失败: {ex.Message}";
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private byte[] ParseByteInput(string input)
        {
            var cleaned = input.Replace("new byte[] {", "").Replace("}", "").Replace(" ", "");
            var parts = cleaned.Split(',');
            var bytes = new byte[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                bytes[i] = byte.Parse(parts[i].Trim());
            }
            return bytes;
        }

        private byte[] ParseHexInput(string input)
        {
            var cleaned = input.Replace(" ", "").Replace("-", "");
            var bytes = new byte[cleaned.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(cleaned.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }
    }
}
