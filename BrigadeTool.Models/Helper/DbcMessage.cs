using Rxbit.Brigade.Models.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rxbit.Brigade.Models.Helper
{
    /// <summary>
    /// DbcMessage.cs
    /// </summary>
    public class DbcMessage : INotifyPropertyChanged
    {
        public uint ID { get; set; }
        public string Name { get; set; }
        public byte DLC { get; set; }
        public string Comment { get; set; }
        public ObservableCollection<DbcSignal> Signals { get; set; } = new ObservableCollection<DbcSignal>();

        // 运行时属性
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public string IDHex => $"{ID:X}";
        public string IDDec => $"({ID})";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override string ToString()
        {
            return $"{Name}[{IDHex}]\r\n" ;
        }
    }
    public enum SignedType 
    {
        Unsigned, Signed
    }
    public enum ByteOrder
    {
        Motorola,
        Intel
    }
    /// <summary>
    /// DbcSignal.cs
    /// </summary>
    public class DbcSignal : ViewModelBase
    {
        public string Name { get; set; }
        public int StartBit { get; set; }
        public int Length { get; set; }
        public ByteOrder ByteOrder { get; set; } // 1=Intel, 0=Motorola
        public SignedType ValueType { get; set; } // 0=Unsigned, 1=Signed
        public double Factor { get; set; }
        public double Offset { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Unit { get; set; }
        public string Comment { get; set; }

        public ObservableCollection<ValueDescription> ValueDescriptions { get; set; } = new ObservableCollection<ValueDescription>();

        // 运行时编辑值
        private double _physicalValue;
        public double PhysicalValue
        {
            get => _physicalValue;
            set
            {
                if (_physicalValue != value)
                {
                    SetProperty(ref _physicalValue, value);
                    OnPropertyChanged(nameof(RawValueHex));
                    RawValue = (ulong)((_physicalValue - Offset) / Factor);
                }
            }
        }

        private ulong _rawValue;
        public ulong RawValue
        {
            get => _rawValue;
            set
            {
                if (_rawValue != value)
                {
                    //PhysicalValue = (value * Factor) + Offset;
                    SetProperty(ref _rawValue, value);
                    OnPropertyChanged(nameof(RawValueHex));
                }
            }
        }

        public string RawValueHex => $"{RawValue:X}";

        public override string ToString()
        {
            return $"{Name} - {PhysicalValue}[{RawValueHex}]";
        }
    }

    public class ValueDescription : ViewModelBase
    {
        public int Value { get; set; }
        public string Description { get; set; }

        public bool IsRange { get; set; }
        public override string ToString()
        {
            if (IsRange) 
            {
                return  Description;
            }
            return "0x" + Value.ToString("X") + " - " + Description;
        }
    }

    /// <summary>
    /// DBC信号解析器 - 支持Motorola和Intel格式
    /// </summary>
    public static class DbcSignalParser
    {
        #region 核心解析接口
        /// <summary>
        /// 解析信号物理值（自动根据ByteOrder选择解析方式）
        /// </summary>
        public static double ParsePhysicalValue(byte[] data, DbcSignal signal)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("CAN数据必须为8字节");

            long rawValue = signal.ByteOrder == ByteOrder.Motorola
                ? ParseMotorolaRaw(data, signal.StartBit, signal.Length, signal.ValueType)
                : ParseIntelRaw(data, signal.StartBit, signal.Length, signal.ValueType);

            return rawValue * signal.Factor + signal.Offset;
        }

        /// <summary>
        /// 解析信号原始值（long类型，未应用factor和offset）
        /// </summary>
        public static long ParseRawValue(byte[] data, DbcSignal signal)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("CAN数据必须为8字节");

            return signal.ByteOrder == ByteOrder.Motorola
                ? ParseMotorolaRaw(data, signal.StartBit, signal.Length, signal.ValueType)
                : ParseIntelRaw(data, signal.StartBit, signal.Length, signal.ValueType);
        }


        /// <summary>
        /// 解析信号物理值（自动根据ByteOrder选择解析方式）
        /// </summary>
        public static double WritePhysicalValue(byte[] data, DbcSignal signal,double value)
        {
            

            return (value - signal.Offset) / signal.Factor;
        }



        #endregion

        #region Intel模式解析 (Little Endian)

        /// <summary>
        /// Intel格式解析（小端序，LSB在前）
        /// DBC StartBit定义为LSB（最低有效位）的位置
        /// </summary>
        public static long ParseIntelRaw(byte[] data, int startBit, int length, SignedType sign)
        {
            if (startBit < 0 || startBit > 63)
                throw new ArgumentOutOfRangeException(nameof(startBit));
            if (length < 1 || length > 64)
                throw new ArgumentOutOfRangeException(nameof(length));

            // Intel: StartBit是LSB的位置，向高位增长
            int startByte = startBit / 8;
            int startBitInByte = startBit % 8;

            long value = 0;
            int bitsRemaining = length;
            int currentBit = startBit;

            while (bitsRemaining > 0)
            {
                int byteIndex = currentBit / 8;
                int bitIndex = currentBit % 8;

                // 计算当前字节能取多少位
                int bitsInThisByte = Math.Min(bitsRemaining, 8 - bitIndex);

                // 提取位（从bitIndex开始，取bitsInThisByte位）
                byte mask = (byte)((1 << bitsInThisByte) - 1);
                byte bits = (byte)((data[byteIndex] >> bitIndex) & mask);

                // 放到结果的对应位置
                value |= ((long)bits << (currentBit - startBit));

                bitsRemaining -= bitsInThisByte;
                currentBit += bitsInThisByte;
            }

            // 处理有符号数
            if (sign == SignedType.Signed && length < 64)
            {
                long signBit = 1L << (length - 1);
                if ((value & signBit) != 0)
                {
                    // 负数，进行符号扩展
                    long signExtendMask = ~((1L << length) - 1);
                    value |= signExtendMask;
                }
            }

            return value;
        }

        /// <summary>
        /// Intel格式编码（将值写入字节数组）
        /// </summary>
        public static void EncodeIntel(byte[] data, int startBit, int length, long value)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("数据数组必须至少8字节");

            // 掩码处理（确保值在有效位宽内）
            if (length < 64)
            {
                long mask = (1L << length) - 1;
                value &= mask;
            }

            int bitsRemaining = length;
            int currentBit = startBit;

            while (bitsRemaining > 0)
            {
                int byteIndex = currentBit / 8;
                int bitIndex = currentBit % 8;
                int bitsInThisByte = Math.Min(bitsRemaining, 8 - bitIndex);

                // 提取要写入的位
                long bits = (value >> (currentBit - startBit)) & ((1L << bitsInThisByte) - 1);

                // 清零目标位，然后写入
                byte clearMask = (byte)~(((1 << bitsInThisByte) - 1) << bitIndex);
                data[byteIndex] &= clearMask;
                data[byteIndex] |= (byte)(bits << bitIndex);

                bitsRemaining -= bitsInThisByte;
                currentBit += bitsInThisByte;
            }
        }

        #endregion

        #region Motorola模式解析 (Big Endian)

        /// <summary>
        /// Motorola格式解析（大端序，MSB在前）
        /// DBC StartBit定义为MSB（最高有效位）的位置
        /// 位序：行优先，每行从左上到右下（7->0, 15->8...）
        /// </summary>
        public static long ParseMotorolaRaw(byte[] data, int startBit, int length, SignedType sign)
        {
            if (startBit < 0 || startBit > 63)
                throw new ArgumentOutOfRangeException(nameof(startBit));
            if (length < 1 || length > 64)
                throw new ArgumentOutOfRangeException(nameof(length));

            // Motorola: StartBit是MSB的位置，向低位增长（跨字节时跳到下一个字节的高位）
            long value = 0;
            int bitsCollected = 0;
            int currentBit = startBit;

            while (bitsCollected < length)
            {
                int byteIndex = currentBit / 8;
                int bitIndex = 7 - (currentBit % 8); // Motorola位序反转（7->0）

                // 计算当前字节还能取多少位（向低位取）
                int bitsInThisByte = Math.Min(length - bitsCollected, bitIndex + 1);

                // 提取位（从bitIndex开始向低位取）
                byte mask = (byte)((1 << bitsInThisByte) - 1);
                byte bits = (byte)((data[byteIndex] >> (bitIndex - bitsInThisByte + 1)) & mask);

                // 放到结果的高位（Motorola是大端，先取的位是高位）
                value = (value << bitsInThisByte) | bits;

                bitsCollected += bitsInThisByte;

                // 移动到下一个位：跨字节处理
                // 如果当前字节取完了，跳到下一个字节（+8），并从该字节的高位（bit 7）开始
                currentBit = ((currentBit / 8) + 1) * 8 + 7;
            }

            // 处理有符号数
            if (sign == SignedType.Signed && length < 64)
            {
                long signBit = 1L << (length - 1);
                if ((value & signBit) != 0)
                {
                    long signExtendMask = ~((1L << length) - 1);
                    value |= signExtendMask;
                }
            }

            return value;
        }

        /// <summary>
        /// 更精确的Motorola解析（基于DBC标准，考虑跨字节情况）
        /// 使用位索引转换方法
        /// </summary>
        public static long ParseMotorolaRawV2(byte[] data, int startBit, int length, SignedType sign)
        {
            // 将Motorola的StartBit（MSB）转换为LSB位置，然后用类似Intel的方式处理
            int msb = startBit;
            int lsb = MotorolaMsbToLsb(msb, length);

            // 现在从lsb开始，向高位取length位（Intel方式，但位序是反的）
            long value = 0;
            int bitsRemaining = length;
            int currentBit = lsb;

            while (bitsRemaining > 0)
            {
                int byteIndex = currentBit / 8;
                int bitIndex = currentBit % 8;
                int bitsInThisByte = Math.Min(bitsRemaining, 8 - bitIndex);

                byte mask = (byte)((1 << bitsInThisByte) - 1);
                byte bits = (byte)((data[byteIndex] >> bitIndex) & mask);

                // Motorola：位是反向存储的，需要反转
                value = (value << bitsInThisByte) | bits;

                bitsRemaining -= bitsInThisByte;
                currentBit += bitsInThisByte;
            }

            if (sign == SignedType.Signed && length < 64)
            {
                long signBit = 1L << (length - 1);
                if ((value & signBit) != 0)
                {
                    long signExtendMask = ~((1L << length) - 1);
                    value |= signExtendMask;
                }
            }

            return value;
        }

        /// <summary>
        /// 将Motorola的MSB转换为LSB位置
        /// </summary>
        private static int MotorolaMsbToLsb(int msb, int length)
        {
            int msbRow = msb / 8;
            int msbCol = 7 - (msb % 8); // 列位置（0-7，0是最高位）

            // 计算LSB位置
            int totalBits = msbRow * 8 + msbCol + 1; // MSB之前的位数（包括MSB）
            int lsbTotalBits = totalBits - length;   // LSB之前的位数

            int lsbRow = lsbTotalBits / 8;
            int lsbCol = 7 - (lsbTotalBits % 8);

            return lsbRow * 8 + (7 - lsbCol);
        }

        /// <summary>
        /// Motorola格式编码
        /// </summary>
        public static void EncodeMotorola(byte[] data, int startBit, int length, long value)
        {
            if (data == null || data.Length < 8)
                throw new ArgumentException("数据数组必须至少8字节");

            if (length < 64)
            {
                long mask = (1L << length) - 1;
                value &= mask;
            }

            // 从MSB开始，向低位写入
            int bitsRemaining = length;
            int currentBit = startBit;
            int valueShift = length;

            while (bitsRemaining > 0)
            {
                int byteIndex = currentBit / 8;
                int bitIndex = 7 - (currentBit % 8); // 列索引（7->0）
                int bitsInThisByte = Math.Min(bitsRemaining, bitIndex + 1);

                valueShift -= bitsInThisByte;
                long bits = (value >> valueShift) & ((1L << bitsInThisByte) - 1);

                // 写入位置：从(bitIndex - bitsInThisByte + 1)到bitIndex
                int startPos = bitIndex - bitsInThisByte + 1;
                byte clearMask = (byte)~(((1 << bitsInThisByte) - 1) << startPos);
                data[byteIndex] &= clearMask;
                data[byteIndex] |= (byte)(bits << startPos);

                bitsRemaining -= bitsInThisByte;
                currentBit = ((currentBit / 8) + 1) * 8 + 7; // 跳到下一个字节
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 打印CAN数据的位布局（用于调试）
        /// </summary>
        public static void PrintCanDataBits(byte[] data)
        {
            Console.WriteLine("CAN Data Bit Layout (Row: Byte index, Col: Bit 7->0):");
            Console.WriteLine("     7  6  5  4  3  2  1  0");

            for (int i = 0; i < data.Length; i++)
            {
                Console.Write($"[{i}] ");
                for (int j = 7; j >= 0; j--)
                {
                    int bit = (data[i] >> j) & 1;
                    Console.Write($" {bit} ");
                }
                Console.WriteLine($"  0x{data[i]:X2}");
            }
        }

        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        public static string ToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", " ");
        }


        public static void Test()
        {
            Console.WriteLine("=== DBC Signal Parser Demo ===\n");

            // 测试数据：0x12 0x34 0x56 0x78 0x9A 0xBC 0xDE 0xF0
            byte[] testData = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 };

            Console.WriteLine("Test Data: " + DbcSignalParser.ToHexString(testData));
            DbcSignalParser.PrintCanDataBits(testData);
            Console.WriteLine();

            // ===== Intel模式测试 =====
            Console.WriteLine("=== Intel (Little Endian) Tests ===");

            // 测试1: Intel, StartBit=0, Length=8 (单字节)
            var sigIntel1 = new DbcSignal
            {
                Name = "Intel_Byte0",
                StartBit = 0,
                Length = 8,
                ByteOrder = ByteOrder.Intel,
                ValueType = SignedType.Unsigned,
                Factor = 1,
                Offset = 0
            };
            long val1 = DbcSignalParser.ParseIntelRaw(testData, 0, 8, SignedType.Unsigned);
            Console.WriteLine($"Intel Raw [Start:0, Len:8]: 0x{val1:X2} ({val1}) - Expected: 0x12");

            // 测试2: Intel, StartBit=0, Length=16 (跨字节)
            var sigIntel2 = new DbcSignal
            {
                Name = "Intel_Word",
                StartBit = 0,
                Length = 16,
                ByteOrder = ByteOrder.Intel,
                ValueType = SignedType.Unsigned,
                Factor = 0.1,
                Offset = 0
            };
            long val2 = DbcSignalParser.ParseIntelRaw(testData, 0, 16, SignedType.Unsigned);
            double phy2 = DbcSignalParser.ParsePhysicalValue(testData, sigIntel2);
            Console.WriteLine($"Intel Raw [Start:0, Len:16]: 0x{val2:X4} ({val2}) - Expected: 0x3412 (13330)");
            Console.WriteLine($"Intel Physical: {phy2} (Expected: 1333.0)");

            // 测试3: Intel, StartBit=4, Length=12 (非对齐)
            long val3 = DbcSignalParser.ParseIntelRaw(testData, 4, 12, SignedType.Unsigned);
            Console.WriteLine($"Intel Raw [Start:4, Len:12]: 0x{val3:X3} ({val3}) - Expected: 0x412 (1042)");

            // 测试4: Intel有符号数
            byte[] negData = new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            long val4 = DbcSignalParser.ParseIntelRaw(negData, 0, 16, SignedType.Signed);
            Console.WriteLine($"Intel Signed [Start:0, Len:16]: {val4} (Expected: -1)");

            Console.WriteLine();

            // ===== Motorola模式测试 =====
            Console.WriteLine("=== Motorola (Big Endian) Tests ===");

            // 测试1: Motorola, StartBit=7, Length=8 (单字节，Byte0的Bit7是MSB)
            var sigMot1 = new DbcSignal
            {
                Name = "Motorola_Byte0",
                StartBit = 7,
                Length = 8,
                ByteOrder = ByteOrder.Motorola,
                ValueType = SignedType.Unsigned,
                Factor = 1,
                Offset = 0
            };
            long valM1 = DbcSignalParser.ParseMotorolaRaw(testData, 7, 8, SignedType.Unsigned);
            Console.WriteLine($"Motorola Raw [Start:7, Len:8]: 0x{valM1:X2} ({valM1}) - Expected: 0x12");

            // 测试2: Motorola, StartBit=7, Length=16 (跨字节，MSB在Byte0 Bit7)
            var sigMot2 = new DbcSignal
            {
                Name = "Motorola_Word",
                StartBit = 7,
                Length = 16,
                ByteOrder = ByteOrder.Motorola,
                ValueType = SignedType.Unsigned,
                Factor = 0.01,
                Offset = 0
            };
            long valM2 = DbcSignalParser.ParseMotorolaRaw(testData, 7, 16, SignedType.Unsigned);
            double phyM2 = DbcSignalParser.ParsePhysicalValue(testData, sigMot2);
            Console.WriteLine($"Motorola Raw [Start:7, Len:16]: 0x{valM2:X4} ({valM2}) - Expected: 0x1234 (4660)");
            Console.WriteLine($"Motorola Physical: {phyM2} (Expected: 46.60)");

            // 测试3: Motorola, StartBit=11, Length=12 (非对齐，MSB在Byte1 Bit3)
            long valM3 = DbcSignalParser.ParseMotorolaRaw(testData, 11, 12, SignedType.Unsigned);
            Console.WriteLine($"Motorola Raw [Start:11, Len:12]: 0x{valM3:X3} ({valM3})");

            // 测试4: Motorola有符号数
            byte[] motNegData = new byte[] { 0xFF, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            long valM4 = DbcSignalParser.ParseMotorolaRaw(motNegData, 7, 12, SignedType.Signed);
            Console.WriteLine($"Motorola Signed [Start:7, Len:12]: {valM4}");

            Console.WriteLine();

            // ===== 编码测试 =====
            Console.WriteLine("=== Encoding Tests ===");

            byte[] encodeBuffer = new byte[8];

            // Intel编码
            DbcSignalParser.EncodeIntel(encodeBuffer, 0, 16, 0x1234);
            Console.WriteLine($"Intel Encode 0x1234 at 0: {DbcSignalParser.ToHexString(encodeBuffer)}");

            Array.Clear(encodeBuffer, 0, encodeBuffer.Length);

            // Motorola编码
            DbcSignalParser.EncodeMotorola(encodeBuffer, 7, 16, 0x1234);
            Console.WriteLine($"Motorola Encode 0x1234 at 7: {DbcSignalParser.ToHexString(encodeBuffer)}");

            Console.WriteLine();
            Console.WriteLine("=== 实际DBC信号示例 ===");

            // 模拟实际DBC信号：车速信号
            // 假设：Message ID 0x123, 信号名 VehicleSpeed
            // StartBit=24, Length=16, Factor=0.01, Offset=0, Range=0-250, Unit=km/h
            var vehicleSpeed = new DbcSignal
            {
                Name = "VehicleSpeed",
                StartBit = 6,
                Length = 11,
                ByteOrder = ByteOrder.Motorola,
                ValueType = SignedType.Unsigned,
                Factor = 0.125,
                Offset = 0,
                Unit = "km/h"
            };

            // 模拟CAN帧：0x00 0x00 0x00 0x86 0x0B 0x00 0x00 0x00
            // 期望速度：35.0 - 118 km/h (0x0B86 = 2950 * 0.01)
            byte[] speedFrame = new byte[] { 0x91, 0x81, 0x00, 0x18, 0x01, 0x00, 0x00, 0x00 };
            double speed = DbcSignalParser.ParsePhysicalValue(speedFrame, vehicleSpeed);
            Console.WriteLine($"Vehicle Speed: {speed} {vehicleSpeed.Unit} (Expected: 295.0)");
        }
        #endregion
    }

    public static class DbcSignalConverter
    {
        #region 信号转字节
        /// <summary>
        /// 将物理值写入指定位置的 8 字节 CAN 数据
        /// </summary>
        /// <param name="signal">DBC 信号定义</param>
        /// <param name="physicalValue">物理值（如车速 60.5 km/h）</param>
        /// <param name="canData">8 字节 CAN 数据（会被修改）</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void SignalToBytes(DbcSignal signal, double physicalValue, byte[] canData)
        {
            if (canData == null || canData.Length != 8)
                throw new ArgumentException("CAN 数据必须是 8 字节数组", nameof(canData));

            // 1. 物理值 → 原始值（raw = (physical - offset) / factor）
            double rawValue = (physicalValue - signal.Offset) / signal.Factor;

            // 2. 限制原始值范围（防止溢出）
            ulong raw;
            if (signal.ValueType == 0) // 无符号
            {
                ulong maxRaw = (1UL << (int)signal.Length) -1;
                //rawValue = Math.Clamp(rawValue, 0, (1UL << (int)signal.Length) - 1);
                if (rawValue < 0 || rawValue > maxRaw)
                {
                    rawValue = signal.Min;
                }
                raw = (ulong)rawValue;
            }
            else // 有符号
            {
                long minRaw = -(1L << ((int)signal.Length - 1));
                long maxRaw = (1L << ((int)signal.Length - 1)) - 1;
                //rawValue = Math.Clamp(rawValue, minRaw, maxRaw);
                if (rawValue < minRaw || rawValue > maxRaw)
                {
                    rawValue = signal.Min;
                }
                long signedRaw = (long)rawValue;
                // 转为补码
                raw = (ulong)(signedRaw & ((1L << (int)signal.Length) - 1));
            }

            // 3. 按字节序写入位
            if (signal.ByteOrder ==  ByteOrder.Intel) // Intel (小端, 低位在前)
                WriteBitsIntel(canData, signal.StartBit, signal.Length, raw);
            else // Motorola (大端, 高位在前)
                WriteBitsMotorola(canData, signal.StartBit, signal.Length, raw);
        }

        /// <summary>
        /// 小端(Intel)方式写入位
        /// </summary>
        private static void WriteBitsIntel(byte[] data, int startBit, int length, ulong value)
        {
            uint bitPos = (uint)startBit;
            ulong mask = 1;
            for (uint i = 0; i < length; i++)
            {
                uint byteIdx = bitPos / 8;
                uint bitInByte = bitPos % 8;

                if ((value & mask) != 0)
                    data[byteIdx] |= (byte)(1 << (int)bitInByte);
                else
                    data[byteIdx] &= (byte)~(1 << (int)bitInByte);

                mask <<= 1;
                bitPos++;
            }
        }

        /// <summary>
        /// 大端(Motorola)方式写入位（CAN 标准：startBit 是最高位）
        /// </summary>
        private static void WriteBitsMotorola(byte[] data, int startBit, int length, ulong value)
        {
            // Motorola: startBit 是信号最高位，依次向低字节、低位方向排布
            uint currentBit = (uint)startBit;
            ulong mask = 1UL << ((int)length - 1); // 从最高位开始

            for (uint i = 0; i < length; i++)
            {
                uint byteIdx = currentBit / 8;
                uint bitInByte = currentBit % 8;

                if ((value & mask) != 0)
                    data[byteIdx] |= (byte)(1 << (int)bitInByte);
                else
                    data[byteIdx] &= (byte)~(1 << (int)bitInByte);

                mask >>= 1;
                currentBit--; // 大端：从高位向低位写
            }
        }

        #endregion

        #region 字节转信号
        /// <summary>
        /// 从 8 字节 CAN 数据中解析出信号的物理值
        /// </summary>
        /// <param name="signal">DBC 信号定义</param>
        /// <param name="canData">8 字节 CAN 数据</param>
        /// <returns>物理值</returns>
        /// <exception cref="ArgumentException"></exception>
        public static double BytesToSignal(DbcSignal signal, byte[] canData)
        {
            if (canData == null )
                throw new ArgumentException("CAN 数据必须是 8 字节数组", nameof(canData));

            // 1. 读取原始值（raw）
            ulong raw;
            if (signal.ByteOrder ==  ByteOrder.Intel) // Intel
                raw = ReadBitsIntel(canData, signal.StartBit, signal.Length);
            else // Motorola
                raw = ReadBitsMotorola(canData, signal.StartBit, signal.Length);

            // 2. 处理有符号/无符号
            if (signal.ValueType ==  SignedType.Signed) // 有符号
            {
                long signedRaw = (long)raw;
                long signBit = 1L << ((int)signal.Length - 1);
                if ((signedRaw & signBit) != 0)
                    signedRaw -= (1L << (int)signal.Length); // 补码转负数
                raw = (ulong)signedRaw;
            }
            //signal.RawValue = raw;
            // 3. 原始值 → 物理值（physical = raw * factor + offset）
            double physical = (double)raw * signal.Factor + signal.Offset;
            if (physical < signal.Min || physical > signal.Max) 
            {
                physical = signal.Min;
            }
            signal.PhysicalValue = physical;
            // 4. 限制到 Min/Max 范围
            return physical;
        }

        /// <summary>
        /// 小端(Intel)方式读取位
        /// </summary>
        private static ulong ReadBitsIntel(byte[] data, int startBit, int length)
        {
            ulong result = 0;
            uint bitPos = (uint)startBit;
            for (uint i = 0; i < length; i++)
            {
                uint byteIdx = bitPos / 8;
                uint bitInByte = bitPos % 8;
                ulong bitValue = (ulong)(data[byteIdx] >> (int)bitInByte) & 1;
                result |= bitValue << (int)i;
                bitPos++;
            }
            return result;
        }

        /// <summary>
        /// 大端(Motorola)方式读取位
        /// </summary>
        private static ulong ReadBitsMotorola(byte[] data, int startBit, int length)
        {

            ulong result = 0;
            uint currentBit = (uint)startBit;
            for (uint i = 0; i < length; i++)
            {
                uint byteIdx = currentBit / 8;
                uint bitInByte = currentBit % 8;
                ulong bitValue = (ulong)(data[byteIdx] >> (int)bitInByte) & 1;
                result = (result << 1) | bitValue;
                currentBit--;
            }
            return result;
        }
        #endregion
    }
}