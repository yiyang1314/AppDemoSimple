using Microsoft.Win32;
using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.Models.Commands;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class Brigade2RaddarViewModel : ViewModelBase
    {
        private string _sourceFilePath;
        public string SourceFilePath
        {
            get { return _sourceFilePath; }
            set { SetProperty(ref _sourceFilePath, value); }
        }
        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public AsyncRelayCommand SelectFileCommand { get; }
        public AsyncRelayCommand ConvertorCommand { get; }
        public Brigade2RaddarViewModel()
        {
            SelectFileCommand = new AsyncRelayCommand(SelectFile);
            ConvertorCommand = new AsyncRelayCommand(ConvertBlfToAsc);
        }

        private async Task SelectFile()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BLF File (*.blf)|*.blf|ASC File (*.asc)|*.asc";
            openFileDialog.Title = "Select File";
            if (openFileDialog.ShowDialog() == true)
            {
                SourceFilePath = openFileDialog.FileName;
                StatusMessage = $"已选择文件: {SourceFilePath}";
            }
        }

        private async Task ConvertBlfToAsc()
        {
            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
            {
                StatusMessage = "请先选择有效的源文件";
                return;
            }

            if (!SourceFilePath.ToLower().EndsWith(".asc"))
            {
                StatusMessage = "请选择 BLF 文件";
                return;
            }

            try
            {
                StatusMessage = "正在转换 ASC 到 CSV...";
                var frames = FileParser.ParseAscFile(SourceFilePath);
                var OutputFilePath = SourceFilePath.Replace(".asc", "_radar.csv");
                Brigade.Models.Models.BrigadeCanProtocol.DefectInfo defect = new Brigade.Models.Models.BrigadeCanProtocol.DefectInfo();
                Brigade.Models.Models.BrigadeCanProtocol.ObjStatus n0 = new Brigade.Models.Models.BrigadeCanProtocol.ObjStatus();
                Brigade.Models.Models.BrigadeCanProtocol.ObjN1 n1 = new Brigade.Models.Models.BrigadeCanProtocol.ObjN1();
                Brigade.Models.Models.BrigadeCanProtocol.ObjN2 n2 = new Brigade.Models.Models.BrigadeCanProtocol.ObjN2();
                int count = 0;
                using StreamWriter streamWriter = new StreamWriter(OutputFilePath);

                bool mIs2xxFirst = false;
                bool mIs3xxFirst = false;
                StringBuilder sb = new StringBuilder();
                foreach (var item in frames)
                {
                    foreach (var frame in item.Value)
                    {
                        if (frame.ID >= 0x310 && frame.ID <= 0x38f)
                        {
                            if (!mIs3xxFirst)
                            {
                                sb.AppendLine(RadarConstaint.GetInfo("SensorParamStr"));
                                mIs3xxFirst = true;
                                mIs2xxFirst = false;
                            }
                            defect.Init(frame.Data);
                            sb.Append($"{frame.ID:x}");
                            defect.ToCsv(sb);
                            streamWriter.WriteLine(sb);
                            sb.Clear();
                        }
                        else if (frame.ID >= 0x210 && frame.ID <= 0x282)
                        {
                            if (!mIs2xxFirst)
                            {
                                sb.AppendLine(RadarConstaint.GetInfo("ObjectMsgParamStr"));
                                mIs3xxFirst = false;
                                mIs2xxFirst = true;
                            }
                            var cid =  frame.ID & 0x03;
                            if (cid == 0)
                            {
                                n0.Init(frame.Data);
                                count++;
                                sb.Append($"{frame.ID:x},");
                                n0.ToCsv(sb);
                            }

                            if (cid == 1)
                            {
                                n1.Init(frame.Data);
                                if (n1.Id > 1)
                                {
                                    sb.Append(",,,,,,,,,,,");
                                }
                                count++;
                                n1.ToCsv(sb);
                            }
                            if (cid == 2)
                            {
                                n2.Init(frame.Data);
                                n2.ToCsv(sb);
                                streamWriter.WriteLine(sb);
                                sb.Clear();
                            }
                        }
                    }
                }
                if (File.Exists(OutputFilePath))
                {
                    StatusMessage = $"转换成功！输出文件: {OutputFilePath}";
                }
                else
                {
                    StatusMessage = "转换失败";
                }
                streamWriter.Close();
            }
            catch (Exception ex)
            {
                StatusMessage = $"转换失败: {ex.Message}";
            }
        }

    }
}
