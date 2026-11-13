using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Runtime.InteropServices;
using static Rxbit.Brigade.CanDevices.Structs.GcanFD;

namespace Rxbit.Brigade.CanDevices
{
    public static class GcanFdConnector
    {
        #region 状态码
        public const int USBCANFD = 6;
        public const int CANFD_DEVICE_MAX_NUMBER = 16;
        public const int CANFD_ERRFRAME_MAX_NUMBER = 1000;
        public const int CANFD_RECEFBUFFER_MAX_NUMBER = 10000;
        public const int CAN_0 = 0;
        public const int CAN_1 = 1;
        #endregion

        #region CANFDFRAME_TYPE 类型值
        /// <summary>
        /// 标准帧-扩展帧
        /// </summary>
        public const int STANDARD_FORMAT = 0;
        public const int EXTENDED_FORMAT = 1;
        /// <summary>
        /// 数据帧-远程帧
        /// </summary>
        public const int DATA_TYPE = 0;
        public const int RTR_TYPE = 1;
        /// <summary>
        /// can协议类型
        /// </summary>
        public const int CAN_TYPE = 0;
        public const int CANFD_TYPE = 1;
        /// <summary>
        /// 可变波特率
        /// </summary>
        public const int BITRATESITCH_ON = 1;
        public const int BITRATESITCH_OFF = 0;
        #endregion

        #region 函数接口
        [DllImport("ECANFDVCI.dll", EntryPoint = "OpenDeviceFD")]
        public static extern ResultStatus OpenDeviceFD(uint DeviceType, uint DeviceInd);
        [DllImport("ECANFDVCI.dll", EntryPoint = "CloseDeviceFD")]
        public static extern ResultStatus CloseDeviceFD(uint DeviceType, uint DeviceInd);
        [DllImport("ECANFDVCI.dll", EntryPoint = "InitCANFD", CallingConvention = CallingConvention.StdCall)]
        public static extern ResultStatus InitCANFD(uint DeviceType, uint DeviceInd, byte CANInd, GcanFD.INIT_CONFIG[] InitConfig);
        [DllImport("ECANFDVCI.dll", EntryPoint = "StartCANFD")]
        public static extern ResultStatus StartCANFD(uint DeviceType, uint DeviceInd, byte CANInd);
        [DllImport("ECANFDVCI.dll", EntryPoint = "StopCANFD")]
        public static extern ResultStatus StopCANFD(uint DeviceType, uint DeviceInd, byte CANInd);
        [DllImport("ECANFDVCI.dll", EntryPoint = "ResetCANFD")]
        public static extern ResultStatus ResetCANFD(uint DeviceType, uint DeviceInd, byte CANInd);
        [DllImport("ECANFDVCI.dll", EntryPoint = "TransmitFD", CallingConvention = CallingConvention.StdCall)]
        public static extern ResultStatus TransmitFD(uint DeviceType, uint DeviceInd, byte CANInd, GcanFD.CANFD_OBJ[] Send, uint length);
        [DllImport("ECANFDVCI.dll", EntryPoint = "ReceiveFD", CallingConvention = CallingConvention.StdCall)]
        public static extern ResultStatus ReceiveFD(uint DeviceType, uint DeviceInd, byte CANInd, IntPtr mReceive, ref uint length);
        [DllImport("ECANFDVCI.dll", EntryPoint = "Receive_buffer_thread")]
        public static extern ResultStatus Receive_buffer_thread(uint DeviceType, uint DeviceInd, uint WaitTime);
        [DllImport("ECANFDVCI.dll", EntryPoint = "GetErrFrame", CallingConvention = CallingConvention.StdCall)]
        public static extern ResultStatus GetErrFrame(uint DeviceType, uint DeviceInd, byte CANInd, GcanFD.ERR_FRAME[] pCanfdErrbuffer, ref uint length);
        [DllImport("ECANFDVCI.dll", EntryPoint = "GetCanfdBusStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern ResultStatus GetCanfdBusStatus(uint DeviceType, uint DeviceInd, GcanFD.CANFD_STATUS[] p_canfd_status);
        [DllImport("ECANFDVCI.dll", EntryPoint = "GetReference")]
        public static extern ResultStatus GetReference(uint DeviceType, uint DeviceInd, byte CANInd, uint RefType, byte[] pData);
        [DllImport("ECANFDVCI.dll", EntryPoint = "SetReference")]
        public static extern ResultStatus SetReference(uint DeviceType, uint DeviceInd, byte CANInd, uint RefType, object pData);
        #endregion
    }
}
