using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rxbit.Brigade.Models.Helper
{
    // DbcParser.cs
    public class DbcParser
    {
        public ObservableCollection<DbcMessage> Messages { get; } = new ObservableCollection<DbcMessage>();
        private readonly Dictionary<uint, DbcMessage> _messageDict = new Dictionary<uint, DbcMessage>();
        private readonly Dictionary<string, DbcSignal> _signalDict = new Dictionary<string, DbcSignal>();

        public void ParseFile(string filePath)
        {
            Messages.Clear();
            _messageDict.Clear();
            _signalDict.Clear();

            var lines = File.ReadAllLines(filePath, Encoding.GetEncoding("GB2312"));
            DbcMessage currentMessage = null;
            long currline = 0;
            long errorline = 1259;
            StringBuilder cmsb = new StringBuilder();
            try
            {
                bool mIsParseCm = false;
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (currline == errorline)
                    {
                        Console.WriteLine(errorline);
                    }
                    if (trimmed.StartsWith("BO_ "))
                    {
                        currentMessage = ParseMessage(trimmed);
                        if (currentMessage.DLC > 0)
                        {
                            Messages.Add(currentMessage);
                        }
                        _messageDict[currentMessage.ID] = currentMessage;
                    }
                    else if (trimmed.StartsWith("SG_ ") && currentMessage != null)
                    {
                        var signal = ParseSignal(trimmed, currentMessage.ID);
                        if (signal != null)
                        {
                            currentMessage.Signals.Add(signal);
                            _signalDict[signal.Name] = signal;
                        }
                    }
                    else if (trimmed.StartsWith("CM_ SG_ "))
                    {
                        if (trimmed.EndsWith("\";"))
                        {
                            ParseComment(trimmed);
                            cmsb.Clear();
                        }
                        else 
                        {
                            mIsParseCm = true;
                            cmsb.Append(trimmed);
                        }
                    }
                    else if (mIsParseCm)
                    {
                        cmsb.Append(";" + trimmed);
                        if (trimmed.EndsWith("\";"))
                        {
                            mIsParseCm = false;
                            ParseComment(cmsb.ToString());
                            dbcSignal.Comment = String.Join("; ", dbcSignal.ValueDescriptions);
                            cmsb.Clear();
                        }
                    }
                    else if (trimmed.StartsWith("VAL_ "))
                    {
                        ParseValueDescription(trimmed);
                    }
                    currline++;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"当前行解析错误，行{currline},line:{lines[currline]}");
                errorline = currline;
                currline = 0;

            }
            finally
            {

            }
        }

        private DbcMessage ParseMessage(string line)
        {
            // BO_ 2032 ServiceDriverInstallation: 8 Vector__XXX
            var match = Regex.Match(line, @"BO_\s+(\d+)\s+(\w+):\s+(\d+)\s+(\w+)");
            if (match.Success)
            {
                return new DbcMessage
                {
                    ID = uint.Parse(match.Groups[1].Value),
                    Name = match.Groups[2].Value,
                    DLC = byte.Parse(match.Groups[3].Value)
                };
            }
            return null;
        }

        private DbcSignal ParseSignal(string line, uint messageId)
        {
            // SG_ ServiceDriverInstallationCmd : 3|04@0+ (1,0) [0|0] "" Vector__XXX
            var match = Regex.Match(line,
                @"SG_\s+(\w+)\s*:?\s*(\d+)\|(\d+)@(\d+)([+-])\s*\(([\d.]+),\s*([-\d.]+)\)\s*\[([-\d.]+)\|([-\d.]+)\]\s*""([^""]*)""");

            if (match.Success)
            {
                List<string> strs = new List<string>();
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    strs.Add(match.Groups[i].Value);
                }
                return new DbcSignal
                {
                    Name = match.Groups[1].Value,
                    StartBit = int.Parse(match.Groups[2].Value),
                    Length = int.Parse(match.Groups[3].Value),
                    ByteOrder = byte.Parse(match.Groups[4].Value) == 0 ? ByteOrder.Motorola: ByteOrder.Intel,
                    ValueType = match.Groups[5].Value == "+" ? SignedType.Unsigned : SignedType.Signed,
                    Factor = double.Parse(match.Groups[6].Value),
                    Offset = double.Parse(match.Groups[7].Value),
                    Min = double.Parse(match.Groups[8].Value),
                    Max = double.Parse(match.Groups[9].Value),
                    Unit = match.Groups[10].Value
                };
            }
            else
            {
                var str = line.Split(new string[] { " ",":","|","@","\"","(",")","[","]",","}, StringSplitOptions.RemoveEmptyEntries);
                var Signal =  new DbcSignal
                {
                    Name = str[1],
                    StartBit = int.Parse(str[2]),
                    Length = int.Parse(str[3]),

                    Factor = double.Parse(str[5]),
                    Offset = double.Parse(str[6]),
                    Min = double.Parse(str[7]),
                    Max = double.Parse(str[8])
                };
                var chs = str[4].ToCharArray();
                Signal.ByteOrder = chs[0] == '0' ? ByteOrder.Motorola : ByteOrder.Intel;
                Signal.ValueType = chs[1] == '+' ? SignedType.Unsigned : SignedType.Signed;
                if (str.Length > 10)
                {
                    Signal.Unit = str[1];
                }
                return Signal;
            }

            return null;
        }

        private DbcSignal dbcSignal;
        private void ParseComment(string line)
        {
            // CM_ SG_ 2032 ServiceDriverInstallationCmd "注释内容";
            var str = line.Split(new string[] { " ", "\"", ";",":", "：" }, StringSplitOptions.RemoveEmptyEntries);
            if (str.Length > 2)
            {
                var msgId = uint.Parse(str[2]);
                var sigName = str[3];
                if (_messageDict.TryGetValue(msgId, out var msg))
                {
                    dbcSignal = msg.Signals.FirstOrDefault(s => s.Name == sigName);
                    if (dbcSignal == null) return;
                    var cms = line.Split(new string[] { "\"", ";", "；"}, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var cm in cms)
                    {
                        if (!cm.StartsWith("0x")) continue;
                        var valuedesc = new ValueDescription();
                        var cplit = cm.Split(new string[] { "0x", ":", "：", "=" }, StringSplitOptions.RemoveEmptyEntries);
                        if (cm.Contains("：") || cm.Contains(":"))
                        {
                            if (int.TryParse(cplit[0].Trim(), System.Globalization.NumberStyles.HexNumber, null, out int v))
                            {
                                valuedesc.Description = cplit[1];
                                valuedesc.Value = v;
                            }
                            else 
                            {
                                var cplit1 = cplit[0].Split(new string[] { "-", "~" }, StringSplitOptions.RemoveEmptyEntries);
                                valuedesc.IsRange = true;
                                valuedesc.Description = cm;
                                valuedesc.Value = Convert.ToInt32(cplit1[0].Trim(), 16);
                            }
                        }
                        else 
                        {
                            int index = cm.IndexOf(" ");
                            string prefix = cm.Substring(0, index);
                            string sufix = cm.Substring(index, cm.Length - index);
                            if (int.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out int v))
                            {
                                valuedesc.Description = sufix;
                                valuedesc.Value = v;
                            }
                            else
                            {
                                var cplit1 = prefix.Split(new string[] { "-", "~"}, StringSplitOptions.RemoveEmptyEntries);
                                valuedesc.IsRange = true;
                                valuedesc.Description = cm;
                                valuedesc.Value = Convert.ToInt32(cplit1[0], 16);
                            }
                        }
                        dbcSignal.ValueDescriptions.Add(valuedesc);
                    }
                    if (dbcSignal.ValueDescriptions.Count > 0) 
                    {
                        dbcSignal.Comment = String.Join("; ", dbcSignal.ValueDescriptions);
                    }
                    else 
                    {
                        str = cms[1].Split(new string[] { "\"", ";" ,"；"}, StringSplitOptions.RemoveEmptyEntries) ;
                        if (str != null && str.Length == 1) 
                        {
                            dbcSignal.Comment = str[0];
                        }
                    }
                }
            }
        }

        private void ParseValueDescription(string line)
        {
            // VAL_ 2032 ServiceDriverInstallationCmd 0 "Idle" 1 "Running";
            var match = Regex.Match(line, @"VAL_\s+(\d+)\s+(\w+)\s+(.+);");
            if (match.Success)
            {
                var msgId = uint.Parse(match.Groups[1].Value);
                var sigName = match.Groups[2].Value;
                var values = match.Groups[3].Value;

                if (_messageDict.TryGetValue(msgId, out var msg))
                {
                    var sig = msg.Signals.FirstOrDefault(s => s.Name == sigName);
                    if (sig != null)
                    {
                        var valMatches = Regex.Matches(values, @"(\d+)\s+""([^""]+)""");
                        foreach (Match vm in valMatches)
                        {
                            sig.ValueDescriptions.Add(new ValueDescription
                            {
                                Value = int.Parse(vm.Groups[1].Value, System.Globalization.NumberStyles.HexNumber,null),
                                Description = vm.Groups[2].Value
                            });
                        }
                        sig.Comment = String.Join("; ", sig.ValueDescriptions);
                       // sig.SelectValueDesc = sig.ValueDescriptions.FirstOrDefault();
                    }
                }
            }
        }

        public byte[] BuildCanFrame(DbcMessage message)
        {
            var data = new byte[message.DLC];
            ulong rawData = 0;

            foreach (var signal in message.Signals)
            {
                // 物理值转原始值
                var rawValue = (ulong)((signal.PhysicalValue - signal.Offset) / signal.Factor);

                if (signal.ByteOrder ==  ByteOrder.Intel) // Intel (Little Endian)
                {
                    rawData |= rawValue << (int)signal.StartBit;
                }
                else // Motorola (Big Endian)
                {
                    // Motorola格式需要特殊处理位序
                    var startByte = signal.StartBit / 8;
                    var startBitInByte = 7 - (signal.StartBit % 8);
                    var actualStart = startByte * 8 + startBitInByte;
                    rawData |= rawValue << (int)(actualStart - signal.Length + 1);
                }
            }

            // 填充字节
            for (int i = 0; i < message.DLC; i++)
            {
                data[i] = (byte)((rawData >> (i * 8)) & 0xFF);
            }

            return data;
        }

        public void BuildSignalsFrame(DbcMessage message, byte[] data)
        {

            ulong rawData = 0;

            foreach (var signal in message.Signals)
            {
                
                
                // 物理值转原始值
                var rawValue = (ulong)((signal.PhysicalValue - signal.Offset) / signal.Factor);
      
            }


        }
    }
}
