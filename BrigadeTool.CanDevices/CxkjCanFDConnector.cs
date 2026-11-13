using System;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices
{
    /// <summary>
    /// CANFD can卡适配
    /// </summary>
    public class CxkjCanFDConnector
    {
        public const int ZCAN_USBCANFD_200U = 41;
        public const int ZCAN_USBCANFD_100U = 42;
        public const int STATUS_ERR = 0;
        public const int STATUS_OK = 1;

        public const int STATUS_ONLINE = 2;
        public const int STATUS_OFFLINE = 3;

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_OpenDevice(uint device_type, uint device_index, uint reserved);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_CloseDevice(IntPtr device_handle);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        // pInitConfig -> ZCAN_CHANNEL_INIT_CONFIG
        public static extern IntPtr ZCAN_InitCAN(IntPtr device_handle, uint can_index, IntPtr pInitConfig);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_StartCAN(IntPtr channel_handle);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ResetCAN(IntPtr channel_handle);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        // pTransmit -> ZCAN_Transmit_Data
        public static extern uint ZCAN_Transmit(IntPtr channel_handle, IntPtr pTransmit, uint len);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        // pTransmit -> ZCAN_TransmitFD_Data
        public static extern uint ZCAN_TransmitFD(IntPtr channel_handle, IntPtr pTransmit, uint len);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_GetReceiveNum(IntPtr channel_handle, byte type);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_Receive(IntPtr channel_handle, IntPtr data, uint len, int wait_time = -1);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ReceiveFD(IntPtr channel_handle, IntPtr data, uint len, int wait_time = -1);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        // pErrInfo -> ZCAN_CHANNEL_ERROR_INFO
        public static extern uint ZCAN_ReadChannelErrInfo(IntPtr channel_handle, IntPtr pErrInfo);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetIProperty(IntPtr device_handle);

        [DllImport("cxkjcan/ControlCANFD.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_IsDeviceOnLine(IntPtr device_handle);
    }
}
