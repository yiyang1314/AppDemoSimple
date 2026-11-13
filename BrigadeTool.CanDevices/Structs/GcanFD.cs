using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Rxbit.Brigade.CanDevices.Structs.GcanFD;

namespace Rxbit.Brigade.CanDevices.Structs
{
    public struct GcanFD
    {
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct BYTE_TYPE
        {
            [FieldOffset(0)]
            public byte Bit0;
            [FieldOffset(1)]
            public byte Bit1;
            [FieldOffset(2)]
            public byte Bit2;
            [FieldOffset(3)]
            public byte Bit3;
            [FieldOffset(4)]
            public byte Bit4;
            [FieldOffset(5)]
            public byte Bit5;
            [FieldOffset(6)]
            public byte Bit6;
            [FieldOffset(7)]
            public byte Bit7;
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct CANFDFRAME_TYPE
        {
            [FieldOffset(0)]
            public byte proto;// CAN/CANFD 0-1
            [FieldOffset(1)]
            public byte format;// STD/EXD 0-1
            [FieldOffset(2)]
            public byte type;// DATA/RTR 0-1
            [FieldOffset(3)]
            public byte bitratemode;// bitrateswitch on/off 0-1
            [FieldOffset(4)]
            public byte reserved;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct CANFD_ECR_TYPE
        {
            [FieldOffset(0)]
            public uint TEC;
            [FieldOffset(8)]
            public uint REC;
            [FieldOffset(15)]
            public uint RP;
            [FieldOffset(16)]
            public uint CEL;
            [FieldOffset(24)]
            public uint reserved;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct CANFD_PSR_TYPE
        {
            [FieldOffset(0)]
            public uint LEC;
            [FieldOffset(3)]
            public uint ACT;
            [FieldOffset(5)]
            public uint EP;
            [FieldOffset(6)]
            public uint EW;
            [FieldOffset(7)]
            public uint BO;
            [FieldOffset(8)]
            public uint DLEC;
            [FieldOffset(11)]
            public uint RESI;
            [FieldOffset(12)]
            public uint RBRS;
            [FieldOffset(13)]
            public uint RFDF;
            [FieldOffset(14)]
            public uint PXE;
            [FieldOffset(15)]
            public uint reserved;
            [FieldOffset(16)]
            public uint TDCV;
            [FieldOffset(23)]
            public uint reserved2;
        }

        public struct ERR_FRAME
        {
            public TIMESTAMP_TYPE can_timestamp;
            public CANFD_ECR_TYPE can_ecr_register;
            public CANFD_PSR_TYPE can_psr_register;
        }

        public struct CANFD_STATUS
        {
            public ushort LeftSendBufferNum;
            public TIMESTAMP_TYPE can0_timestamp;
            public CANFD_ECR_TYPE can0_ecr_register;
            public CANFD_PSR_TYPE can0_psr_register;
            public uint can0_RxLost_Cnt;
            public uint can0_TxFail_Cnt;
            public float can0_Load_Rate;
            public TIMESTAMP_TYPE can1_timestamp;
            public CANFD_ECR_TYPE can1_ecr_register;
            public CANFD_PSR_TYPE can1_psr_register;
            public uint can1_RxLost_Cnt;
            public uint can1_TxFail_Cnt;
            public float can1_Load_Rate;
        }

        public struct INIT_CONFIG
        {
            public byte CanReceMode;
            public byte CanSendMode;
            public uint NominalBitRate;
            public uint DataBitRate;
            public byte FilterUsedBits;
            public byte StdOrExdBits;
            public byte NominalBitRateSelect;
            public byte DataBitRateSelect;
            public uint StandardORExtendedfilter1;
            public uint StandardORExtendedfilter1Mask;
            public uint StandardORExtendedfilter2;
            public uint StandardORExtendedfilter2Mask;
            public uint StandardORExtendedfilter3;
            public uint StandardORExtendedfilter3Mask;
            public uint StandardORExtendedfilter4;
            public uint StandardORExtendedfilter4Mask;
            public uint StandardORExtendedfilter5;
            public uint StandardORExtendedfilter5Mask;
            public uint StandardORExtendedfilter6;
            public uint StandardORExtendedfilter6Mask;
            public uint StandardORExtendedfilter7;
            public uint StandardORExtendedfilter7Mask;
            public uint StandardORExtendedfilter8;
            public uint StandardORExtendedfilter8Mask;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TIMESTAMP_TYPE
        {
            public byte mday;
            public byte hour;
            public byte minute;
            public byte second;
            public ushort millisecond;
            public ushort microsecond;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ulong ToUnixTimestampMicroseconds()
            {
                return ((ulong)hour * 3_600_000_000UL) +
                       ((ulong)minute * 60_000_000UL) +
                       ((ulong)second * 1_000_000UL) +
                       ((ulong)millisecond * 1_000UL) +
                       microsecond;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CANFD_OBJ
        {
            public byte CanORCanfdType;
            public byte DataLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] Reserved;
            public uint ID;
            public TIMESTAMP_TYPE TimeStamp;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Data;

            public bool IsCanfdProto()
            {
                return (CanORCanfdType & 0x01) == 1;
            }
            public bool IsStdFrame()
            {
                return ((CanORCanfdType >> 1) & 0x01) == 0;
            }
            public bool IsDataFrame()
            {
                return ((CanORCanfdType >> 2) & 0x01) == 0;
            }
            // 判断是否为扩展帧
            public bool IsExtFrame()
            {
                return ((CanORCanfdType >> 1) & 0x01) == 1;
            }
            public bool IsBRS()
            {
                return ((CanORCanfdType >> 3) & 0x01) == 1;
            }
            public void Set_CanORCanfdType(byte proto, byte Format, byte Type, byte BRS)
            {
                byte tmp = 0;
                checked
                {
                    tmp += (byte)((BRS % 2) * 8);//brs
                    tmp += (byte)((Type % 8) * 4);//data - rtr
                    tmp += (byte)((Format % 2) * 2); //std-exd
                    CanORCanfdType = (byte)(tmp + proto % 2);//can-canfd
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BOARD_INFO
        {
            public ushort hw_Version;
            public ushort fw_Version;
            public ushort dr_Version;
            public ushort in_Version;
            public ushort irq_Num;
            public byte can_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public char[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] Reserved;
        }

        [Flags]
        // 定义发送模式选择
        public enum SEND_MODE : UInt32
        {
            POSITIVE_SEND = 0,
            PASSIVE_SEND = 1,
        }

        // 定义接收模式选择
        public enum RECEIVE_MODE : UInt32
        {
            SPECIFIED_RECEIVE = 0,
            GLOBAL_STANDARD_RECEIVE = 1,
            GLOBAL_EXTENDED_RECEIVE = 2,
            GLOBAL_STANDARD_AND_EXTENDED_RECEIVE = 3,
        }
        /// <summary>
        /// 状态码
        /// </summary>
        public enum ResultStatus : uint
        {
            STATUS_OK = 0,
            /// <summary>
            /// 设备状态码
            /// </summary>
            ERR_CAN_NOINIT = 0x0001,	// CAN未初始化就被用于发送
            ERR_CAN_DISABLE = 0x0002,	// CAN未使能就被用于发送
            ERR_CAN_BUSOFF = 0x0004,	// CAN已经离线

            /// <summary>
            /// CAN数据状态码
            /// </summary>
            ERR_DATA_LEN = 0x0010,	// 数据长度错误
            ERR_USB_WRITE = 0x0020,	// USB写数据失败
            ERR_USB_READ = 0x0040,	// USB读数据失败

            //通用错误码
            ERR_DEVICEOPENED = 0x0100,	//设备已经打开
            ERR_DEVICEOPEN = 0x0200,	//打开设备错误
            ERR_DEVICENOTOPEN = 0x0400,	//设备没有打开
            ERR_BUFFEROVERFLOW = 0x0800, //缓冲区溢出
            ERR_DEVICENOTEXIST = 0x1000, //此设备不存在
            ERR_DEVICECLOSE = 0x2000	//关闭设备失败
        }
        // 定义CAN波特率的选择类型
        //public enum BAUD_RATE : uint
        //{
        //    BAUDRATE_1M = 0,
        //    BAUDRATE_800K = 1,
        //    BAUDRATE_500K = 2,
        //    BAUDRATE_400K = 3,
        //    BAUDRATE_250K = 4,
        //    BAUDRATE_200K = 5,
        //    BAUDRATE_125K = 6,
        //    BAUDRATE_100K = 7,
        //    BAUDRATE_80K = 8,
        //    BAUDRATE_62500 = 9,
        //    BAUDRATE_50K = 10,
        //    BAUDRATE_40K = 11,
        //    BAUDRATE_25K = 12,
        //    BAUDRATE_20K = 13,
        //    BAUDRATE_10K = 14,
        //    BAUDRATE_5K = 15,
        //}
        //public enum DATA_RATE : uint
        //{
        //    DATARATE_5M = 0,
        //    DATARATE_4M = 1,
        //    DATARATE_2M = 2,
        //    DATARATE_1M = 3,
        //    DATARATE_800K = 4,
        //    DATARATE_500K = 5,
        //    DATARATE_400K = 6,
        //    DATARATE_250K = 7,
        //    DATARATE_200K = 8,
        //    DATARATE_125K = 9,
        //    DATARATE_100K = 10,
        //    DATARATE_80K = 11,
        //    DATARATE_62500 = 12,
        //    DATARATE_50K = 13,
        //    DATARATE_40K = 14,
        //    DATARATE_25K = 15,
        //    DATARATE_20K = 16,
        //    DATARATE_10K = 17,
        //    DATARATE_5K = 18,

        //}
    }

    /// <summary>
    /// TIMESTAMP_TYPE 转时间戳扩展方法
    /// </summary>
    public static class TimestampTypeExtensions
    {
        /// <summary>
        /// 1. 先将 TIMESTAMP_TYPE 转为 DateTime（需补全年/月）
        /// </summary>
        private static DateTime ToDateTime(this TIMESTAMP_TYPE ts, int? year = null, int? month = null)
        {
            var now = DateTime.Now;
            // 未指定年/月时，使用当前日期的年/月
            int targetYear = year ?? now.Year;
            int targetMonth = month ?? now.Month;

            // 构造基础DateTime（包含毫秒），再叠加微秒（1 Tick = 100ns = 0.1微秒）
            return new DateTime(
                targetYear, targetMonth, ts.mday,
                ts.hour, ts.minute, ts.second, ts.millisecond
            ).AddTicks(ts.microsecond * 10);
        }

        /// <summary>
        /// 2. 转换为【秒级Unix时间戳】（double，带微秒精度）
        /// 格式：1707123456.123456（秒.微秒）
        /// </summary>
        public static double ToUnixTimestampSeconds(this TIMESTAMP_TYPE ts, int? year = null, int? month = null)
        {
            DateTime dt = ts.ToDateTime(year, month);
            DateTime utcDt = TimeZoneInfo.ConvertTimeToUtc(dt); // Unix时间戳基于UTC
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 总秒数 + 微秒部分（微秒/1e6）
            double totalSeconds = (utcDt - epoch).TotalSeconds;
            return totalSeconds + ts.microsecond / 1_000_000.0;
        }

        /// <summary>
        /// 3. 转换为【毫秒级Unix时间戳】（long，舍去微秒或四舍五入）
        /// 格式：1707123456123（毫秒）
        /// </summary>
        public static long ToUnixTimestampMilliseconds(this TIMESTAMP_TYPE ts, int? year = null, int? month = null, bool roundMicro = false)
        {
            DateTime dt = ts.ToDateTime(year, month);
            DateTime utcDt = TimeZoneInfo.ConvertTimeToUtc(dt);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            double totalMs = (utcDt - epoch).TotalMilliseconds;
            // 可选：四舍五入微秒到毫秒
            if (roundMicro) totalMs += ts.microsecond / 1000.0;

            return (long)Math.Floor(totalMs);
        }

        /// <summary>
        /// 4. 转换为【微秒级Unix时间戳】（long，完整精度）
        /// 格式：1707123456123456（微秒）
        /// </summary>
        public static long ToUnixTimestampMicroseconds(this TIMESTAMP_TYPE ts, int? year = null, int? month = null)
        {
            DateTime dt = ts.ToDateTime(year, month);
            DateTime utcDt = TimeZoneInfo.ConvertTimeToUtc(dt);
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Ticks 转微秒：1 Tick = 100ns = 0.1微秒 → 总微秒 = Ticks / 10
            long totalTicks = (utcDt - epoch).Ticks;
            return totalTicks / 10;
        }
    }
}
