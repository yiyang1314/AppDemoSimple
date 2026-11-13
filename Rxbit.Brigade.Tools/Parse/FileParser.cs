using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace Rxbit.Brigade.Models.Helper
{
    public enum PlaySpeed 
    {
        [Description("0.5X")]
        Speed_0_5X,
        [Description("1X")]
        Speed_1X,
        [Description("1.5X")]
        Speed_1_5X,
        [Description("2X")]
        Speed_2X,
        [Description("3X")]
        Speed_3X,
    }

    public class FileParser
    {
        public static ConcurrentDictionary<ulong, List<Frame>> ParseBlfFile(string file)
        {
            var dircts = new BlfParser().ParseBlfFile(file);
            string outPuth = file.Replace(".blf", "222.asc");
            FileStream fs = new FileStream(outPuth, FileMode.Create, FileAccess.Write);
            double temp = 0;
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine("BLF TO Asc start");
                foreach (var key in dircts.Keys)
                {
                    List<Frame> frames = dircts[key];
                    foreach (Frame f in frames)
                    {
                        sw.WriteLine(f);
                    }
                    temp = frames[frames.Count -1].TimeStmp;
                }
                sw.WriteLine("BLF TO Asc end");
            }
            fs.Close();
            //System.Windows.Forms.MessageBox.Show("共加载数据：" + dircts.Count + " 条，共 " + temp + " 秒" );
            return dircts;
        }

        public static ConcurrentDictionary<ulong, List<Frame>> ParseAscFile(string file)
        {
            var lines = File.ReadAllLines(file);
            ConcurrentDictionary<ulong, List<Frame>> ASCS = new ConcurrentDictionary<ulong, List<Frame>>();
            long currline = 0;
            long errorLine = -1;
            try
            {
                for (int i = 0; i < lines.LongLength; i++)
                {
                    if (currline == errorLine)
                    {
                        Console.WriteLine(lines[currline]);
                    }
                    var Fileds = lines[i].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (Fileds.Length < 8) continue;
                    ulong key = (ulong)(Convert.ToDouble(Fileds[0]) * 1000);
                    if (!ASCS.ContainsKey(key))
                    {
                        ASCS.TryAdd(key, new List<Frame>());
                    }
                    if (ASCS.TryGetValue(key, out List<Frame> frameList))
                    {
                        int vaildLen = 6;
                        Frame f = new Frame();
                        if (Fileds[1].StartsWith("CANFD"))
                        {
                            vaildLen = 10;
                            f.CanType = Fileds[1];
                            f.ID = Convert.ToUInt32(Fileds[3], 16);
                            f.Channel = Convert.ToUInt32(Fileds[2]);
                        }
                        else
                        {
                            f.ID = Convert.ToUInt32(Fileds[2], 16);
                            f.Channel = Convert.ToUInt32(Fileds[1]);
                        }
                        f.TimeStmp = Convert.ToDouble(Fileds[0]);
                        f.DLC = Convert.ToInt32(Fileds[vaildLen - 1]);
                        f.Data = new byte[f.DLC];
                        f.DataHex = "";
                        for (int j = 0; j < f.DLC; j++)
                        {
                            f.Data[j] = Convert.ToByte(Fileds[j + vaildLen], 16);
                            f.DataHex = f.DataHex + Fileds[j + vaildLen] + " ";
                        }
                        frameList.Add(f);
                        currline++;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"报文格式解析异常，错误行{currline}：{lines[currline]}");
                errorLine = currline;
            }
            finally
            {

            }

            return ASCS;
        }
    }
    public class Frame 
    {
        public double TimeStmp { get; set; }
        public uint ID { get; set; }
        public uint Channel { get; set; }
        public string CanType { get; set; } = string.Empty;
        public int DLC { get; set; }
        private string _DataHex = string.Empty;

        public string DataHex
        {
            get { return _DataHex ; }
            set { _DataHex  = value; }
        }

        public Byte[] Data { get; set; }

        public override string ToString()
        {
            if (CanType != string.Empty)
            {
                return $"{TimeStmp:F6} {Channel} {ID:X} Rx 1 0 d {DataConverter.DataLen2DLC(DLC)} {DLC} {DataHex}";
            }
            return $"{TimeStmp:F6} {CanType} {Channel} {ID:X} Rx d {DLC} {DataHex}";
        }
    }

    public class DataConverter
    {
        public static uint Length2DLC(uint length)
        {
            int dlc = 0;
            dlc = DLCList.IndexOf(length);
            return dlc < 0 ? 0 : (uint)dlc;
        }
        static List<uint> DLCList = new List<uint>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };

        public static byte DataLen2DLC(int length)
        {
            var inde = 0;

            if (length >= DLCList[DLCList.Count - 1])
            {
                inde = 15;
            }

            for (int i = 0; i < DLCList.Count - 1; i++)
            {
                if (length > DLCList[i] && length < DLCList[i + 1])
                {
                    inde = i + 1;
                    break;
                }
                else if (length == DLCList[i])
                {
                    inde = i;
                }
            }

            return (byte)inde;
        }
    }
}
