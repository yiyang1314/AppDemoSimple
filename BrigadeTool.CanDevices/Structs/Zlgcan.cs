using System;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Zlgcan
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_Receive_Data
        {
            public CAN_Frame canFrame;
            public UInt64 timestamp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CAN_Frame
        {
            public UInt32 ID;
            public Byte canDlc;
            public Byte pad;
            public Byte res0;
            public Byte res1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public Byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_Transmit_Data
        {
            public CAN_Frame canFrame;
            public UInt32 type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_TransmitFD_Data
        {
            public CANFD_Frame frame;
            public uint transmit_type;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_ReceiveFD_Data
        {
            public CANFD_Frame canfdFrame;
            public UInt64 timestamp;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CANFD_Frame
        {
            public UInt32 ID;
            public Byte len;
            public Byte flags;
            public Byte res0;
            public Byte res1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public Byte[] data;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_CHANNEL_ERROR_INFO
        {
            public UInt32 errorCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public Byte[] passiveErrData;
            public Byte arLostErrData;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct ZCAN_CHANNEL_INIT_CONFIG
        {
            [FieldOffset(0)]
            public UInt32 canVersion;

            [FieldOffset(4)]
            public ZCAN can;

            [FieldOffset(4)]
            public CANFD canfd;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN
        {
            public UInt32 accCode;
            public UInt32 accMask;
            public UInt32 reserved;
            public Byte filter;
            public Byte timing0;
            public Byte timing1;
            public Byte mode;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CANFD
        {
            public UInt32 accCode;
            public UInt32 accMask;
            public UInt32 abitTiming;
            public UInt32 dbitTiming;
            public UInt32 brp;
            public Byte filter;
            public Byte mode;
            public UInt16 pad;
            public UInt32 reserved;
        }

        public enum CanVersion
        {
            CAN = 0,
            CanFD = 1,
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Int32 SetValueFunc(String path, String value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate String GetValueFunc(String path, String value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr GetPropertyFunc(String path, String value);
        public struct IProperty
        {
            public SetValueFunc SetValue;
            public GetValueFunc GetValue;
            public GetPropertyFunc GetProperty;
        }
    }
}
