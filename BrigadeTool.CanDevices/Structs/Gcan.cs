using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Gcan
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CAN_OBJ
        {
            public uint ID;
            public uint TimeStamp;
            public byte TimeFlag;
            public byte SendType;
            public byte RemoteFlag;
            public byte ExternFlag;
            public byte DataLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] data;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CAN_ERR_INFO
        {
            public uint ErrCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Passive_ErrData;
            public byte ArLost_ErrData;

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct INIT_CONFIG
        {

            public uint AccCode;
            public uint AccMask;
            public uint Reserved;
            public byte Filter;
            public byte Timing0;
            public byte Timing1;
            public byte Mode;
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
            public byte[] str_Serial_Num;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] str_hw_Type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] Reserved;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CAN_STATUS
        {
            byte ErrInterrupt;
            byte regMode;
            byte regStatus;
            byte regALCapture;
            byte regECCapture;
            byte regEWLimit;
            byte regRECounter;
            byte regTECounter;
            uint Reserved;
        }
    }
}
