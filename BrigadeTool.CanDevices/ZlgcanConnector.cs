using System;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices
{
    public class ZlgcanConnector
    {
        /// <summary>
        /// 该函数用于检测设备是否在线，仅支持USB系列设备。
        /// </summary>
        /// <param name="canHandle">CAN 设备句柄值</param>
        /// <returns>设备在线=2(STATUS_ONLINE)，不在线=3(STATUS_OFFLINE)</returns>
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_IsDeviceOnLine(IntPtr canHandle);
        /// <summary>
        /// 此函数用于打开 CAN 设备，注意一个设备只能打开一次。
        /// </summary>
        /// <param name="canType">CAN 设备类型号</param>
        /// <param name="canIndex">CAN 设备索引号</param>
        /// <param name="reserved">保留参数，通常为 0</param>
        /// <returns>打开的 CAN 设备句柄, IntPtr.Zero 表示失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_OpenDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_OpenDevice(UInt32 canType, UInt32 canIndex, UInt32 reserved);

        /// <summary>
        /// 此函数用于关闭设备
        /// </summary>
        /// <param name="canHandle">打开 CAN 设备成功时返回的 CAN 设备句柄<</param>
        /// <returns>1-表示操作成功，0-表示操作失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_CloseDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_CloseDevice(IntPtr canHandle);

        /// <summary>
        /// 此函数用于初始化指定的 CAN 通道
        /// </summary>
        /// <param name="canHandle">打开 CAN 设备成功时返回的 CAN 设备句柄</param>
        /// <param name="canIndex">CAN 设备索引</param>
        /// <param name="pInitConfig">CAN 通道初始化结构体 ZCAN_CHANNEL_INIT_CONFIG 指针句柄</param>
        /// <returns>CAN 通道句柄，IntPtr.Zero 表示初始化失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_InitCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_InitCAN(IntPtr canHandle, uint channelIndex, IntPtr pInitConfig);

        /// <summary>
        /// 此函数用于启动 CAN 的某一个通道。有多个 CAN 通道时，需要多次调用
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <returns>1-表示操作成功，0-表示操作失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_StartCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_StartCAN(IntPtr channelHandle);

        /// <summary>
        /// 此函数用于复位 CAN 通道，主要与 ZCAN_StartCAN 函数配合使用，无需再初始化，即可恢复 CAN 通道的正常状态；
        /// 可理解为关闭 CAN 通道，但是初始化配置信息仍然存在
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <returns>1-表示操作成功，0-表示操作失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_ResetCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_ResetCAN(IntPtr channelHandle);

        /// <summary>
        /// 此函数用于清空 CAN 通道的接收缓冲区数据帧
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <returns>1-表示成功，0-表示失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_ClearBuffer", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_ClearBuffer(IntPtr channelHandle);

        /// <summary>
        /// 此函数用于向 CAN 通道发送数据
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="pTransmit">帧结构体 ZCAN_Transmit_Data 指针句柄</param>
        /// <param name="len">要发送的帧结构体 ZCAN_Transmit_Data 数组长度</param>
        /// <returns>返回实际发送成功的帧数</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_Transmit", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_Transmit(IntPtr channelHandle, IntPtr pTransmit, UInt32 len);

        /// <summary>
        /// 此函数用于向 CAN 通道发送数据
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="pTransmit">帧结构体 ZCAN_TransmitFD_Data 指针句柄</param>
        /// <param name="len">要发送的帧结构体 ZCAN_TransmitFD_Data 数组长度</param>
        /// <returns>返回实际发送成功的帧数</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_TransmitFD", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_TransmitFD(IntPtr channelHandle, IntPtr pTransmit, UInt32 len);

        /// <summary>
        /// 此函数用于获取指定 CAN 通道的接收缓冲区中，尚未被读取的帧数量。
        /// 主要用途是配合 ZCAN_Receive 或 ZCAN_ReceiveFD 使用，即缓冲区有数据，再接收。
        /// 用户无需一直调用 ZCAN_Receive 或 ZCAN_ReceiveFD，可以节约 PC 系统资源，提高程序的效率
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="type">0-CAN，1-CANFD</param>
        /// <returns>返回尚未被读取的帧数</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_GetReceiveNum", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_GetReceiveNum(IntPtr channelHandle, Byte type);

        /// <summary>
        /// 此函数用于从指定的 CAN 通道的接收缓冲区中读取数据。
        /// 建议再调用之前，先调用 ZCAN_GetReceiveNum 函数获知缓冲区中有多少帧，然后对应地去接收。
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="data">接收帧结构体 ZCAN_Receive_Data 数组的首指针</param>
        /// <param name="len">接收帧结构体 ZCAN_Receive_Data 数组的最大帧数，实际返回值小于等于这个值</param>
        /// <param name="wait_time">缓冲区无数据，函数堵塞等待时间，以毫秒为单位，-1 表示无超时一直等待</param>
        /// <returns>返回实际读取到的帧数</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_Receive", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_Receive(IntPtr channelHandle, IntPtr data, UInt32 len, Int32 wait_time = -1);

        /// <summary>
        /// 此函数用于从指定的 CAN 通道的接收缓冲区中读取数据。
        /// 建议再调用之前，先调用 ZCAN_GetReceiveNum 函数获知缓冲区中有多少帧，然后对应地去接收。
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="data">接收帧结构体 ZCAN_ReceiveFD_Data 数组的首指针</param>
        /// <param name="len">接收帧结构体 ZCAN_ReceiveFD_Data 数组的最大帧数，实际返回值小于等于这个值</param>
        /// <param name="wait_time">缓冲区无数据，函数堵塞等待时间，以毫秒为单位，-1 表示无超时一直等待</param>
        /// <returns>返回实际读取到的帧数</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_ReceiveFD", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_ReceiveFD(IntPtr channelHandle, IntPtr data, UInt32 len, Int32 wait_time = -1);

        /// <summary>
        /// 此函数用于获取 CAN 通道最近一次错误信息
        /// </summary>
        /// <param name="channelHandle">CAN 通道句柄</param>
        /// <param name="pErrInfo">存储错误结构体 ZCAN_CHANNEL_ERR_INFO 的首指针</param>
        /// <returns>1-表示操作成功，0-表示操作失败</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "ZCAN_ReadChannelErrInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZCAN_ReadChannelErrInfo(IntPtr channelHandle, IntPtr pErrInfo);

        /// <summary>
        /// 获取 CAN 设备属性接口
        /// </summary>
        /// <param name="canHandle">打开 CAN 设备成功时返回的 CAN 设备句柄</param>
        /// <returns>IPropery 指针句柄</returns>
        [DllImport("zlgcan/zlgcan.dll", EntryPoint = "GetIProperty", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetIProperty(IntPtr canHandle);

        // ═══════════════════════════════════════════
        // 标志位定义（与 zlgcan.h 一致）
        // ═══════════════════════════════════════════
        public const uint CAN_EFF_FLAG = 0x80000000;  // 扩展帧
        public const uint CAN_RTR_FLAG = 0x40000000;  // 远程帧
        public const uint CAN_ERR_FLAG = 0x20000000;  // 错误帧
        public const uint CAN_ID_MASK = 0x1FFFFFFF;  // 29位ID掩码

        // ═══════════════════════════════════════════
        // 判断方法
        // ═══════════════════════════════════════════

        /// <summary>
        /// 判断是否为扩展帧
        /// </summary>
        public static bool IsExtendedFrame1(uint canId)
        {
            return (canId & CAN_EFF_FLAG) != 0;
        }

        /// <summary>
        /// 判断是否为远程帧
        /// </summary>
        public static bool IsRemoteFrame(uint canId)
        {
            return (canId & CAN_RTR_FLAG) != 0;
        }

        /// <summary>
        /// 判断是否为错误帧
        /// </summary>
        public static bool IsErrorFrame(uint canId)
        {
            return (canId & CAN_ERR_FLAG) != 0;
        }

        /// <summary>
        /// 提取纯 CAN ID（去除标志位）
        /// </summary>
        public static uint GetRawId(uint canId)
        {
            return canId & CAN_ID_MASK;
        }
    }
}
