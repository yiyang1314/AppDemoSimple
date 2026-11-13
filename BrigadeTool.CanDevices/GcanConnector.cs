using System;
using System.Runtime.InteropServices;
using static Rxbit.Brigade.CanDevices.Structs.Gcan;

namespace Rxbit.Brigade.CanDevices
{
    public class GcanConnector
    {
        [DllImport("gcan/ECanVci.dll", EntryPoint = "OpenDevice")]
        public static extern UInt32 OpenDevice(
           UInt32 DeviceType,
           UInt32 DeviceInd,
           UInt32 Reserved);

        [DllImport("gcan/ECanVci.dll", EntryPoint = "CloseDevice")]
        public static extern UInt32 CloseDevice(
            UInt32 DeviceType,
            UInt32 DeviceInd);


        [DllImport("gcan/ECanVci.dll", EntryPoint = "InitCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 InitCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref INIT_CONFIG InitConfig);


        [DllImport("gcan/ECanVci.dll", EntryPoint = "StartCAN")]
        public static extern UInt32 StartCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);


        [DllImport("gcan/ECanVci.dll", EntryPoint = "ResetCAN")]
        public static extern UInt32 ResetCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);


        [DllImport("gcan/ECanVci.dll", EntryPoint = "Transmit")]
        public static extern UInt32 Transmit(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            IntPtr Send,
            UInt16 length);


        [DllImport("gcan/ECanVci.dll", EntryPoint = "Receive", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Receive(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            IntPtr Receive,
            UInt32 length,
            UInt32 WaitTime);

        [DllImport("gcan/ECanVci.dll", EntryPoint = "ReadErrInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ReadErrInfo(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            out CAN_ERR_INFO ReadErrInfo);



        [DllImport("gcan/ECanVci.dll", EntryPoint = "ReadBoardInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ReadBoardInfo(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            out BOARD_INFO ReadErrInfo);

        [DllImport("gcan/ECanVci.dll", EntryPoint = "GetReceiveNum")]
        public static extern UInt32 GetReceiveNum(
        UInt32 DeviceType,
        UInt32 DeviceInd,
        UInt32 CANInd);

        [DllImport("gcan/ECanVci.dll", EntryPoint = "ClearBuffer")]
        public static extern UInt32 ClearBuffer(
        UInt32 DeviceType,
        UInt32 DeviceInd,
        UInt32 CANInd);

    }
}
