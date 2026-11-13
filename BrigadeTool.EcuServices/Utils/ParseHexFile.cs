using Rxbit.Brigade.EcuServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rxbit.Brigade.EcuServices.Utils
{
    public class ParseHexFile
    {
        List<DataLine> Hex;
        public const byte DataRecord = 0x00;                                    //  用来记录数据，HEX文件的大部分记录都是数据记录
        public const byte EndOfFileRecord = 0x01;                               //  用来标识文件结束，放在文件的最后，标识HEX文件的结尾
        public const byte ExtendedSegmentAddressRecord = 0x02;                  //  用来标识扩展段地址的记录
        public const byte StartSegmentAddressRecord = 0x03;                     //  开始段地址记录
        public const byte ExtendedLinearAddressRecord = 0x04;                   //  用来标识扩展线性地址的记录
        public const byte StartLinearAddressRecord = 0x05;                      //  开始线性地址记录
        public const byte Unknown = 0x06;
        private int BaseAddress = 0x0;
        public ParseHexFile()
        {
            Hex = new List<DataLine>();
        }
        public bool ParseSigleHexfile(string filename, ref DataBlock dataBlock)
        {
            List<DataBlock> HexBlockList = new List<DataBlock>();
            if (ParseHexfile(filename, ref HexBlockList))
            {
                dataBlock = HexBlockList.FirstOrDefault();
                return true;
            }
            ;
            return false;
        }

        public bool ParseHexfile(string filename, ref List<DataBlock> HexBlockList)
        {
            try
            {
                StreamReader read = new StreamReader(filename);
                string str = "";
                int ncount = 0;
                //检查hex文件错误
                do
                {
                    str = read.ReadLine();
                    ncount++;
                    if (str == null)
                        continue;
                    else
                    {
                        if (!CheckCorrectness(str, ncount))
                        {
                            //Console.Write("Hex文件出错\r\n");
                            return false;
                        }
                    }
                } while (!read.EndOfStream);
                //while (str != null);

                read.BaseStream.Position = 0;
                //提取hex文件每行的数据
                do
                {
                    str = read.ReadLine();
                    //ncount++;
                    if (str == null)
                        continue;
                    else
                    {
                        switch (Convert.ToByte(str.Substring(7, 2), 16))
                        {
                            //DataLine
                            case DataRecord://00
                                DataLine HexData = new DataLine();
                                HexData.address = BaseAddress + Convert.ToInt32(str.Substring(3, 4), 16);
                                HexData.data = str.Substring(9, Convert.ToInt32(str.Substring(1, 2), 16) * 2);
                                HexData.count = Convert.ToInt32(str.Substring(1, 2), 16);
                                Hex.Add(HexData);
                                break;
                            case EndOfFileRecord://01
                                break;
                            case ExtendedSegmentAddressRecord://02
                                BaseAddress = Convert.ToInt32(str.Substring(9, 4), 16) << 4;
                                break;
                            case StartSegmentAddressRecord://03
                                //BaseAddress = Convert.ToInt32(str.Substring(9, 4), 16) << 4;
                                break;
                            case StartLinearAddressRecord://05
                                break;
                            case ExtendedLinearAddressRecord://04
                                BaseAddress = Convert.ToInt32(str.Substring(9, 4), 16) << 16;
                                break;
                            default:
                                break;
                        }
                    }
                } while (!read.EndOfStream);
                //将每行的数据打包成逻辑块
                int nextAddress = 0xFFFFFFF;
                DataBlock HexBlock = null;
                for (int i = 0; i < Hex.Count; i++)
                {
                    if (i == 0)
                    {
                        HexBlock = new DataBlock();
                        nextAddress = getNextAddress(HexBlock, Hex, i);
                        if (i == Hex.Count - 1)//数据量很少，只用一块s19Block
                        {
                            //已经循环到底，只有一个block
                            HexBlockList.Add(HexBlock);
                        }
                    }
                    else
                    {
                        if (nextAddress == Hex[i].address)//计算地址=读取地址，表示数据是连续的
                        {
                            joinHexBlockData(HexBlock.Data, Hex[i].data);//继续给Block增加数据
                            nextAddress = Hex[i].address + Hex[i].count;
                            if (i == Hex.Count - 1)
                            {
                                //已经循环到底，只有一个block
                                HexBlockList.Add(HexBlock);
                            }
                        }
                        else//计算地址!=读取地址，表示数据是非连续，需要将数据放入新的块
                        {
                            HexBlockList.Add(HexBlock);//将前面更新Block的加入BlockList
                            HexBlock = new DataBlock();
                            nextAddress = getNextAddress(HexBlock, Hex, i);
                            if (i == Hex.Count - 1)
                            {
                                //已经循环到底，只有一个block
                                HexBlockList.Add(HexBlock);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.Write(ex);
                return false;
            }
            return true;
        }
        private int getNextAddress(DataBlock HexBlock, List<DataLine> Hex, int i)
        {
            HexBlock.Address = Hex[i].address;
            joinHexBlockData(HexBlock.Data, Hex[i].data);
            return Hex[i].address + Hex[i].count;
        }

        private void joinHexBlockData(List<byte> list, string dataLine)
        {
            for (int i = 0; i < dataLine.Length; i = i + 2)
            {
                byte value = byte.Parse(dataLine.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                list.Add(value);
            }
        }
        private static bool CheckCorrectness(string strText, int ncount)
        {
            try
            {
                //检查起始引号
                if (strText.Substring(0, 1) != ":")
                {
                    Console.WriteLine(String.Format("第" + ncount.ToString() + "行起始非引号"));
                    return false;
                }
                //检查行长度与格式
                int datalength = Convert.ToByte(strText.Substring(1, 2), 16);
                if (datalength * 2 != (strText.Length - 1 - 2 - 4 - 2 - 2))
                {
                    Console.WriteLine(String.Format("第" + ncount.ToString() + "行长度与格式不匹配"));
                    return false;
                }
                //检查CRC
                UInt32 u32 = 0;
                for (int i = 0; i < (strText.Length - 1) / 2; i++)
                {
                    u32 += Convert.ToByte(strText.Substring(1 + 2 * i, 2), 16);
                }
                if (u32 % 0x100 != 0)
                {
                    Console.WriteLine(String.Format("第" + ncount.ToString() + "行校验错误"));
                    return false;
                }
                byte datatype = Convert.ToByte(strText.Substring(7, 2), 16);

                switch (datatype)
                {
                    case DataRecord:
                        return true;
                    case EndOfFileRecord:
                        if (strText != ":00000001FF")
                        {
                            Console.WriteLine(String.Format("第" + ncount.ToString() + "行不是正确文件结束记录"));
                            return false;
                        }
                        else return true;
                    case ExtendedLinearAddressRecord:
                        if (datalength != 2)
                        {
                            Console.WriteLine(String.Format("第" + ncount.ToString() + "行是扩展段地址记录(02),但长度不是2"));

                            return false;
                        }
                        return true;
                    case ExtendedSegmentAddressRecord:
                        if (datalength != 2)
                        {
                            Console.WriteLine(String.Format("第" + ncount.ToString() + "行是扩展段地址记录(04),但长度不是2"));

                            return false;
                        }
                        return true;
                    case StartSegmentAddressRecord:
                        //HEX.strFault = "第" + ncount.ToString() + "行是开始段地址记录(03)";
                        //return false;
                        return true;
                    case StartLinearAddressRecord:
                        //HEX.strFault = "第" + ncount.ToString() + "行是开始线性地址记录(05)";
                        //return false;
                        return true;
                    default:
                        Console.WriteLine(String.Format("第" + ncount.ToString() + "行是未知无效记录"));
                        return false;
                }
            }
            catch
            {
                Console.WriteLine(String.Format("第" + ncount.ToString() + "行格式错误"));
                return false;
            }
        }
    }
}
