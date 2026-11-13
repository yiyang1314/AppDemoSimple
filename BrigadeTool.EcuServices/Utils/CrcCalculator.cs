using Rxbit.Brigade.EcuServices.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Rxbit.Brigade.EcuServices.Utils
{
    public static class CrcCalculator
    {
        // CRC8 - 多项式 0x07
        public static readonly CrcConfig CRC8 = new CrcConfig
        {
            Type = CrcType.Crc8,
            Polynomial = 0x07,
            InitialValue = 0x00,
            XorOutput = 0x00,
            InputReverse = true,
            OutputReverse = true
        };

        // CRC16 - 多项式 0x1021
        public static readonly CrcConfig CRC16 = new CrcConfig
        {
            Type = CrcType.Crc16,
            Polynomial = 0x1021,
            InitialValue = 0xFFFF,
            XorOutput = 0x0000,
            InputReverse = false,
            OutputReverse = false
        };

        // CRC32 - 多项式 0x04C11DB7
        public static readonly CrcConfig CRC32 = new CrcConfig
        {
            Type = CrcType.Crc32,
            Polynomial = 0x04C11DB7,
            InitialValue = 0xFFFFFFFF,
            XorOutput = 0xFFFFFFFF,
            InputReverse = true,
            OutputReverse = true
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte BitRev8(byte v)
        {
            v = (byte)((v & 0xF0) >> 4 | (v & 0x0F) << 4);
            v = (byte)((v & 0xCC) >> 2 | (v & 0x33) << 2);
            v = (byte)((v & 0xAA) >> 1 | (v & 0x55) << 1);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort BitRev16(ushort v)
        {
            v = (ushort)((v & 0xFF00) >> 8 | (v & 0x00FF) << 8);
            v = (ushort)((v & 0xF0F0) >> 4 | (v & 0x0F0F) << 4);
            v = (ushort)((v & 0xCCCC) >> 2 | (v & 0x3333) << 2);
            v = (ushort)((v & 0xAAAA) >> 1 | (v & 0x5555) << 1);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint BitRev32(uint v)
        {
            v = ((v & 0xFF00FF00) >> 8) | ((v & 0x00FF00FF) << 8);
            v = ((v & 0xF0F0F0F0) >> 4) | ((v & 0x0F0F0F0F) << 4);
            v = ((v & 0xCCCCCCCC) >> 2) | ((v & 0x33333333) << 2);
            v = ((v & 0xAAAAAAAA) >> 1) | ((v & 0x55555555) << 1);
            return (v >> 16) | (v << 16);   // 交换高低字
        }
        public static uint Compute(ReadOnlySpan<byte> data)
        {
            uint Poly = 0x04C11DB7;   // 正常多项式
            uint Init = 0xFFFFFFFF;
            uint XorOut = 0xFFFFFFFF;
            uint crc = Init;                 // ① 初值
            foreach (byte b in data)
            {
                byte rev = BitRev8(b);       // ② 输入反转（字节级）
                crc ^= (uint)rev << 24;      // 移到 MSB
                for (int i = 0; i < 8; i++)
                    crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ Poly : crc << 1;
            }
            crc = BitRev32(crc);             // ③ 输出反转
            return crc ^ XorOut;             // ④ 最终异或
        }

        /// <summary>
        /// CRC32校验（多项式0x04C11DB7）
        /// </summary>
        public static uint Crc32(byte[] data, uint initv = 0xFFFFFFFF, bool inputreser = false, bool outputreser = false, uint ploy = 0x04C11DB7, uint xor = 0)
        {
            uint crc = initv;
            foreach (byte b in data)
            {
                byte rev = inputreser ? BitRev8(b) : b;
                crc ^= (uint)rev << 24;
                for (int i = 0; i < 8; i++)
                    crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ ploy : crc << 1;
            }
            crc = outputreser ? BitRev32(crc) : crc;
            return crc ^ xor;
        }

        /// <summary>
        /// CRC8校验（多项式0x07）
        /// </summary>
        public static byte Crc8(byte[] data, byte initv = 0, bool inputreser = false, bool outputreser = false, byte ploy = 0x07, byte xor = 0)
        {
            byte crc = initv;
            foreach (byte b in data)
            {
                byte rev = inputreser ? BitRev8(b) : b;
                crc ^= rev;                      // 先异或
                for (int i = 0; i < 8; i++)
                    crc = (byte)((crc & 0x80) != 0 ? (crc << 1) ^ ploy : crc << 1);
            }
            crc = outputreser ? BitRev8(crc) : crc;                  // 输出反转
            return (byte)(crc ^ xor);
        }

        /// <summary>
        /// CRC16校验（Modbus，多项式0x8005）
        /// </summary>
        public static ushort Crc16(byte[] data, ushort initv = 0xFFFF, bool inputreser = false, bool outputreser = false, ushort ploy = 0x8005, ushort xor = 0)
        {
            ushort crc = initv;
            foreach (byte b in data)
            {
                byte rev = inputreser ? BitRev8(b) : b;
                crc ^= (ushort)(rev << 8);       // 移到 MSB
                for (int i = 0; i < 8; i++)
                    crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ ploy : crc << 1);
            }
            crc = outputreser ? BitRev16(crc) : crc; // 输出反转
            return (ushort)(crc ^ xor);
        }

        #region 核心计算方法
        /// <summary>
        /// 计算指定数据的CRC校验值
        /// </summary>
        /// <param name="data">待校验的字节数组</param>
        /// <param name="config">CRC配置参数</param>
        /// <returns>CRC校验值（根据类型返回8/16/32位结果）</returns>
        public static bool CalculateCrc(byte[] data, CrcConfig config, out string varifyresult)
        {
            varifyresult = "";
            if (data == null || data.Length == 0)
            {
                varifyresult = "data is empty!";
                return false;
            }


            uint crcResult = config.InitialValue;
            switch (config.Type)
            {
                case CrcType.Crc8:
                    crcResult = Crc8(data, (byte)config.InitialValue, config.InputReverse, config.OutputReverse, (byte)config.Polynomial, (byte)config.XorOutput);
                    varifyresult = (crcResult & 0xff).ToString("X2");
                    return true;
                case CrcType.Crc16:
                    ushort crc16 = Crc16(data, (ushort)config.InitialValue, config.InputReverse, config.OutputReverse, (ushort)config.Polynomial, (ushort)config.XorOutput);
                    varifyresult = ((crc16 >> 8) & 0xff).ToString("X2") + " " + (crc16 & 0xff).ToString("X2");
                    return true;
                case CrcType.Crc32:
                    crcResult = Crc32(data, (uint)config.InitialValue, config.InputReverse, config.OutputReverse, (uint)config.Polynomial, (uint)config.XorOutput);
                    varifyresult = string.Format("{0:x2} {1:x2} {2:x2} {3:x2}", (crcResult >> 24) & 0xff,
                        (crcResult >> 16) & 0xff, (crcResult >> 8) & 0xff, crcResult & 0xff);
                    return true;
            }
            varifyresult = "类型错误";
            return false;
        }

        /// <summary>
        /// 调用外部EXE，不显示窗口，并返回退出码
        /// </summary>
        /// <param name="exePath">EXE文件路径</param>
        /// <param name="arguments">命令行参数（可选）</param>
        /// <param name="output">输出信息（标准输出+错误输出，可选）</param>
        /// <returns>EXE的退出码（返回值）</returns>
        public static bool InvokeExe(string exePath, out string output, string arguments = "")
        {
            output = string.Empty;
            if (!File.Exists(exePath))
            {
                output = "Exe程序路径不存在.";
                return false;
            }
            if (!File.Exists(arguments))
            {
                output = "Hex文件不存在";
                return false;
            }
            int exitCode = -1;
            using (Process process = new Process())
            {
                try
                {
                    // 配置进程启动信息
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = exePath,               // 外部EXE路径
                        Arguments = arguments,            // 命令行参数
                        UseShellExecute = false,          // 不使用操作系统外壳程序启动
                        CreateNoWindow = true,            // 不创建新窗口（关键：隐藏控制台）
                        RedirectStandardOutput = true,    // 重定向标准输出
                        RedirectStandardError = true,     // 重定向错误输出
                        StandardOutputEncoding = System.Text.Encoding.Default, // 输出编码（根据实际情况调整）
                        StandardErrorEncoding = System.Text.Encoding.Default
                    };
                    process.StartInfo = startInfo;
                    // 启动进程
                    process.Start();
                    // 异步读取输出（避免死锁，尤其是输出内容较多时）
                    string stdOutput = process.StandardOutput.ReadToEnd();
                    string stdError = process.StandardError.ReadToEnd();
                    // 实时读取输出（关键：避免缓冲区满）
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            stdOutput = $"输出: {e.Data}";
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            stdError = $"错误: {e.Data}";
                    };

                    // 等待进程退出（设置超时时间，单位：毫秒，-1表示无限等待）
                    process.WaitForExit(-1);
                    // 获取退出码（返回值）
                    exitCode = process.ExitCode;
                    // 合并输出信息
                    output = $"{stdOutput}\r\n";
                    return exitCode == 0;
                }
                catch (Exception ex)
                {
                    output = $"调用失败: {ex.Message}";
                }
            }
            return false;
        }
        #endregion
    }
}
