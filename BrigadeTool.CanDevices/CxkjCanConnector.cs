using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices
{
    /// <summary>
    /// 创新科技CAN卡适配
    /// </summary>
    public class CxkjCanConnector
    {
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        #region CAN卡驱动适配
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("cxkjcan/controlcan.dll", EntryPoint = "VCI_InitCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref CxkjCan.VCI_INIT_CONFIG pInitConfig);

        [DllImport("cxkjcan/controlcan.dll", EntryPoint = "VCI_ReadBoardInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref CxkjCan.VCI_BOARD_INFO pInfo);

        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("cxkjcan/controlcan.dll", EntryPoint = "VCI_Transmit", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref CxkjCan.VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("cxkjcan/controlcan.dll", EntryPoint = "VCI_Receive", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref CxkjCan.VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [DllImport("cxkjcan/controlcan.dll")]
        public static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("cxkjcan/controlcan.dll", EntryPoint = "VCI_FindUsbDevice2", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 VCI_FindUsbDevice2(ref CxkjCan.VCI_BOARD_INFO pInfo);
        /*------------函数描述结束---------------------------------*/
        #endregion
    }
}
