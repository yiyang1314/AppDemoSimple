using Rxbit.Brigade.EcuServices.Models;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rxbit.Brigade.EcuServices.Utils
{
    /// <summary>
    /// 文件Flash解析类
    /// </summary>
    public static class FlashParseFile
    {
        /// <summary>
        /// 解析烧录文件，支持bin，S19，hex格式
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="block">解析结果</param>
        /// <returns>返回状态</returns>
        /// <exception cref="RuntimeBinderException">异常处理</exception>
        public static bool ParseFlashFile(string file, out DataBlock block)
        {
            List<DataBlock> blocklist = new List<DataBlock>();
            if (!File.Exists(file))
            {
                throw new RuntimeBinderException("文件不存在");
            }

            if (file.ToLower().EndsWith(".hex"))
            {
                blocklist.AddRange(ParseHexFile(file));
            }
            else if (file.ToLower().EndsWith(".bin"))
            {
                blocklist.AddRange(ParseBinFile(file));
            }
            else if (file.ToLower().EndsWith(".s19"))
            {
                blocklist.AddRange(ParseS19File(file));
            }
            else
            {
                throw new RuntimeBinderException("文件类型错误，ECU烧录不支持此类文件类型");
            }
            block = blocklist.FirstOrDefault();
            return block == null || block.Data.Count == 0;
        }

        /// <summary>
        /// BIN文件解析
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="blockSize">块长度</param>
        /// <returns>数据集合</returns>
        public static List<DataBlock> ParseBinFile(string filePath, uint startAddress = 0, int blockSize = 1024)
        {
            var blocks = new List<DataBlock>();
            byte[] data = File.ReadAllBytes(filePath);
            if (blockSize == 0)
            {
                blockSize = data.Length;
            }
            for (int i = 0; i < data.Length; i += blockSize)
            {
                int size = Math.Min(blockSize, data.Length - i);
                byte[] blockData = new byte[size];
                Array.Copy(data, i, blockData, 0, size);
                blocks.Add(new DataBlock
                {
                    Address = (((int)(startAddress + i))),
                    Data = blockData.ToList()
                });
            }
            return blocks;
        }
        /// <summary>
        ///  单个HEX块合并
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>数据集合</returns>
        public static List<DataBlock> ParseHexFile(string file)
        {
            List<DataBlock> blocks = new List<DataBlock>();
            ParseHexFile parseHexFile = new ParseHexFile();
            DataBlock block = new DataBlock();
            parseHexFile.ParseSigleHexfile(file, ref block);
            blocks.Add(block);
            return blocks;
        }
        /// <summary>
        /// 多个hex-block块合并
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="block">输出块</param>
        /// <returns>数据集合</returns>
        public static bool ParseHexFileEx(string file, out DataBlock block)
        {
            block = null;
            ushort extendedAddr = 0; // 扩展地址（用于超过16位的地址）
            foreach (var line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith(":")) continue;

                // 解析长度、地址、类型
                int length = Convert.ToInt32(line.Substring(1, 2), 16);
                ushort addr = Convert.ToUInt16(line.Substring(3, 4), 16);
                int type = Convert.ToInt32(line.Substring(7, 2), 16);

                if (type == 0x04) // 扩展线性地址（高16位）
                {
                    extendedAddr = Convert.ToUInt16(line.Substring(9, 4), 16);
                    continue;
                }
                if (type != 0x00) continue; // 只处理数据记录
                // 计算绝对地址（扩展地址 << 16 + 偏移地址）
                long address = ((long)extendedAddr << 16) + addr;
                // 解析数据
                byte[] data = Enumerable.Range(0, length)
                    .Select(i => Convert.ToByte(line.Substring(9 + i * 2, 2), 16))
                    .ToArray();
                if (block == null)
                {
                    block = new DataBlock { Address = (int)address };
                }
                block.Data.AddRange(data);
            }
            return block != null;
        }
        /// <summary>
        /// S19文件
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <returns>数据集合</returns>
        public static List<DataBlock> ParseS19File(string file)
        {
            var blocks = new List<DataBlock>();
            foreach (var line in File.ReadAllLines(file))
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("S")) continue;

                var type = line[1];
                if (type != '1' && type != '2' && type != '3') continue; // 只处理数据记录

                // 解析长度（1字节，十六进制）
                int length = Convert.ToInt32(line.Substring(2, 2), 16);
                // 地址长度：S1=2字节，S2=3字节，S3=4字节
                int addrLength = 0;
                switch (type)
                {
                    case '1':
                        addrLength = 4; break;
                    case '2':
                        addrLength = 6; break;
                    case '3':
                        addrLength = 8; break;
                    default:
                        addrLength = 0; break;
                }
                ;
                if (addrLength == 0) continue;

                // 解析地址
                long address = Convert.ToInt64(line.Substring(4, addrLength), 16);
                // 解析数据（总长度-地址长度/2-1字节校验和）
                int dataLength = (length - (addrLength / 2) - 1) * 2;
                byte[] data = Enumerable.Range(0, dataLength / 2)
                    .Select(i => Convert.ToByte(line.Substring(4 + addrLength + i * 2, 2), 16))
                    .ToArray();

                blocks.Add(new DataBlock { Address = (int)address, Data = data.ToList() });
            }
            return blocks.OrderBy(b => b.Address).ToList(); // 按地址排序
        }
    }
}
