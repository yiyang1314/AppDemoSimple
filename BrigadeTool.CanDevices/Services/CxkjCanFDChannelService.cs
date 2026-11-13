using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Services
{
    internal class CxkjCanFDChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }
        private Byte dataProtoValue;
        private IntPtr channelHandle;
        public const uint CAN_EFF_FLAG = 0x80000000;  // 扩展帧
        /// <summary>
        /// 判断是否为扩展帧
        /// </summary>
        public bool IsExtendedFrame(uint canId)
        {
            return (canId & CAN_EFF_FLAG) != 0;
        }
        static CxkjCanFDChannelService()
        {
            baudRates.Add(10, (0x31, 0x1C));
            baudRates.Add(50, (0x09, 0x1C));
            baudRates.Add(100, (0x04, 0x1C));
            baudRates.Add(125, (0x03, 0x1C));
            baudRates.Add(250, (0x01, 0x1C));
            baudRates.Add(500, (0x00, 0x1C));
            baudRates.Add(1000, (0x00, 0x14));
            InitArbitrationBaudRates();
            InitDataBaudRate();
        }

        #region 1 --- CAN卡参数适配
        public Boolean InitChannelBaudRate(IntPtr canHandle, ChannelConfig ccc)
        {
            var ptr = CxkjCanFDConnector.GetIProperty(canHandle);
            if (ptr == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                var property = (Zlgcan.IProperty)Marshal.PtrToStructure(ptr, typeof(Zlgcan.IProperty));
                if (ccc.DataProto == DataProtocol.CANFD)
                {
                    setBaudrateFD(property, 'A', ((uint)ccc.BaudRate) * 1000, ccc.ChannelIndex);
                    setBaudrateFD(property, 'D', ((uint)ccc.DataBaudRate) * 1000, ccc.ChannelIndex);
                    return SetChannelStandard(property, ccc.ChannelIndex, 0);
                }
                else
                {
                    return SetChannelBaudRate(property, ccc.ChannelIndex, (Int32)ccc.BaudRate);
                }
            }
        }

        public Boolean InitResistanceEnable(IntPtr canHandle, ChannelConfig ccc)
        {
            var ptr = CxkjCanFDConnector.GetIProperty(canHandle);
            if (ptr == IntPtr.Zero)
            {
                return false;
            }
            else
            {
                var property = (Zlgcan.IProperty)Marshal.PtrToStructure(ptr, typeof(Zlgcan.IProperty));
                return setResistanceEnable(property, ccc.ChannelIndex, (Int32)ccc.IsResistanceEnable);
            }
        }

        private Boolean SetChannelStandard(Zlgcan.IProperty property, Byte index, Int32 standard)
        {
            string path = index + "/canfd_standard";
            string value = standard.ToString();
            return 1 == property.SetValue(path, value);
        }

        private Boolean SetChannelBaudRate(Zlgcan.IProperty property, Byte index, Int32 baudRate)
        {
            String path = index + "/baud_rate";
            String value = (baudRate * 1000).ToString();
            return property.SetValue(path, value) == 1;
        }
        private bool setBaudrateFD(Zlgcan.IProperty property, char ad, UInt32 baud, int channel_index)
        {
            string path = "";
            if (ad == 'A')
            {
                path = channel_index + "/canfd_abit_baud_rate";
            }
            else if (ad == 'D')
            {
                path = channel_index + "/canfd_dbit_baud_rate";
            }
            return 1 == property.SetValue(path, baud.ToString());
        }
        //设置终端电阻使能
        private bool setResistanceEnable(Zlgcan.IProperty property, Byte index, Int32 isEnabled)
        {
            string path = index + "/initenal_resistance";
            string value = isEnabled.ToString();
            return property.SetValue(path, value) == 1;
        }
        private Int64 GenaralKey(int devtype, byte devId, uint channelindex)
        {
            return devtype * 1011 + devId * 100;
        }
        #endregion

        #region 2 --- 驱动函数封装
        /// <summary>
        /// CAN只能解析CAN数据 CANFD支持解析CAN和CANFD数据
        /// </summary>
        /// <returns></returns>
        public List<RXCanMessage> Read()
        {
            List<RXCanMessage> datas = new List<RXCanMessage>();
            if (dataProtoValue == 0)//CAN
            {
                var length = CxkjCanFDConnector.ZCAN_GetReceiveNum(channelHandle, 0);
                if (length > 0)
                {
                    Int32 size = Marshal.SizeOf(typeof(Zlgcan.ZCAN_Receive_Data));
                    IntPtr receivePtr = Marshal.AllocHGlobal((Int32)length * size);
                    var receiveLength = CxkjCanFDConnector.ZCAN_Receive(channelHandle, receivePtr, length);
                    for (int i = 0; i < receiveLength; i++)
                    {
                        IntPtr offsetPtr = IntPtr.Add(receivePtr, i * size);
                        var canData = (Zlgcan.ZCAN_Receive_Data)Marshal.PtrToStructure(
                          offsetPtr, typeof(Zlgcan.ZCAN_Receive_Data));

                        var message = new RXCanMessage()
                        {
                            DLC = canData.canFrame.canDlc,
                            TIMESTAMP = canData.timestamp,
                            ID = canData.canFrame.ID & 0x7FFFFFFF,
                            IsExdFrame = IsExtendedFrame(canData.canFrame.ID),
                            DATA = new Byte[8],
                        };

                        Array.Copy(canData.canFrame.data, message.DATA, 8);
                        datas.Add(message);
                    }
                    Marshal.FreeHGlobal(receivePtr);
                }
            }
            else//CANFD
            {
                var CANLength = CxkjCanFDConnector.ZCAN_GetReceiveNum(channelHandle, 0);
                if (CANLength > 0)
                {
                    Int32 size = Marshal.SizeOf(typeof(Zlgcan.ZCAN_Receive_Data));
                    IntPtr receivePtr = Marshal.AllocHGlobal((Int32)CANLength * size);
                    var receiveLength = CxkjCanFDConnector.ZCAN_Receive(channelHandle, receivePtr, CANLength);
                    for (int i = 0; i < receiveLength; i++)
                    {
                        IntPtr offsetPtr = IntPtr.Add(receivePtr, i * size);
                        var canData = (Zlgcan.ZCAN_Receive_Data)Marshal.PtrToStructure(
                          offsetPtr, typeof(Zlgcan.ZCAN_Receive_Data));
                        var message = new RXCanMessage()
                        {
                            DLC = canData.canFrame.canDlc,
                            TIMESTAMP = canData.timestamp,
                            ID = canData.canFrame.ID & 0x7FFFFFFF,
                            IsExdFrame = IsExtendedFrame(canData.canFrame.ID),
                            DATA = new Byte[8],
                        };

                        Array.Copy(canData.canFrame.data, message.DATA, 8);
                        datas.Add(message);
                    }
                    Marshal.FreeHGlobal(receivePtr);
                }
                var CANFDLength = CxkjCanFDConnector.ZCAN_GetReceiveNum(channelHandle, 1);
                if (CANFDLength > 0)
                {
                    Int32 size = Marshal.SizeOf(typeof(Zlgcan.ZCAN_ReceiveFD_Data));
                    IntPtr receivePtr = Marshal.AllocHGlobal((Int32)CANFDLength * size);
                    var receiveLength = CxkjCanFDConnector.ZCAN_ReceiveFD(channelHandle, receivePtr, CANFDLength);
                    for (int i = 0; i < receiveLength; i++)
                    {
                        IntPtr offsetPtr = IntPtr.Add(receivePtr, i * size);
                        var canData = (Zlgcan.ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(
                            offsetPtr, typeof(Zlgcan.ZCAN_ReceiveFD_Data));
                        var len = canData.canfdFrame.len > 8 ? 64 : 8;
                        var message = new RXCanMessage()
                        {
                            DLC = canData.canfdFrame.len,
                            TIMESTAMP = canData.timestamp,
                            ID = canData.canfdFrame.ID & 0x7FFFFFFF,
                            IsExdFrame = IsExtendedFrame(canData.canfdFrame.ID),
                            DATA = new Byte[len],
                        };

                        Array.Copy(canData.canfdFrame.data, message.DATA, len);
                        datas.Add(message);
                    }
                    Marshal.FreeHGlobal(receivePtr);
                }

            }
            return datas;
        }

        public Boolean Close()
        {
            Int64 canKey = GenaralKey((Int32)ChannelConfig.CCategroy, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                CxkjCanFDConnector.ZCAN_ResetCAN(channelHandle);
                can.channelHandles.Remove(this.channelHandle);
                if (can.channelHandles.Count == 0)
                {
                    CxkjCanFDConnector.ZCAN_CloseDevice(can.canHandle);
                    OpenedCans.TryRemove(canKey, out can);
                }

                return true;
            }
            return false;
        }

        public Boolean Open(ChannelConfig ccc)
        {
            ChannelConfig = ccc;
            this.dataProtoValue = (Byte)(ccc.DataProto == DataProtocol.CANFD ? 1 : 0);
            var can = OpenCan(ccc.CCategroy, ccc.CanIndex, ccc.ChannelIndex);
            if (can.canHandle != IntPtr.Zero)
            {
                Int32 baudRate = (Int32)ccc.BaudRate;
                Int32 dataBaudRate = (Int32)ccc.DataBaudRate;
                if (InitChannelBaudRate(can.canHandle, ccc))
                {
                    Zlgcan.ZCAN_CHANNEL_INIT_CONFIG channelConfig = new Zlgcan.ZCAN_CHANNEL_INIT_CONFIG();
                    if (ccc.DataProto == DataProtocol.CANFD)
                    {
                        channelConfig.canVersion = (UInt32)Zlgcan.CanVersion.CanFD;
                        channelConfig.canfd = new Zlgcan.CANFD()
                        {
                            mode = 0,
                            filter = 0,
                            accCode = 0,
                            accMask = 0xFFFFFFFF
                        };
                    }
                    else
                    {
                        channelConfig.canVersion = (UInt32)Zlgcan.CanVersion.CAN;
                        channelConfig.can = new Zlgcan.ZCAN()
                        {
                            mode = 0,
                            filter = 0,
                            accCode = 0,
                            timing0 = baudRates[baudRate].timing0,
                            timing1 = baudRates[baudRate].timing1,
                            accMask = 0xFFFFFFFF
                        };
                    }

                    IntPtr channelConfigPtr = Marshal.AllocHGlobal(Marshal.SizeOf(channelConfig));
                    Marshal.StructureToPtr(channelConfig, channelConfigPtr, true);
                    channelHandle = CxkjCanFDConnector.ZCAN_InitCAN(can.canHandle, ccc.ChannelIndex, channelConfigPtr);
                    Marshal.FreeHGlobal(channelConfigPtr);
                    if (channelHandle != IntPtr.Zero)
                    {
                        InitResistanceEnable(can.canHandle, ccc);
                        // CxkjCanFDConnector.ZCAN_ResetCAN(channelHandle);
                        var startResult = CxkjCanFDConnector.ZCAN_StartCAN(channelHandle);
                        if (startResult == 1)
                        {
                            can.channelHandles.Add(channelHandle);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private (IntPtr canHandle, List<IntPtr> channelHandles) OpenCan(CanCategory cc, Byte index, uint channelIndex)
        {
            Int64 canKey = GenaralKey(((int)cc), index, channelIndex);
            if (!OpenedCans.TryGetValue(canKey, out var can))
            {
                IntPtr canHandle = IntPtr.Zero;
                try
                {
                    canHandle = CxkjCanFDConnector.ZCAN_OpenDevice(CxkjCanFDConnector.ZCAN_USBCANFD_200U, index, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {

                }
                if (canHandle != IntPtr.Zero)
                {
                    var ncan = (canHandle, new List<IntPtr>());
                    if (OpenedCans.TryAdd(canKey, ncan))
                    {
                        return ncan;
                    }
                }

                return (IntPtr.Zero, new List<IntPtr>());
            }
            else
            {
                return can;
            }
        }


        private static void InitArbitrationBaudRates()
        {
            ArbitrationBaudRates.Clear();
            ArbitrationBaudRates.Add(10, 0x0041BBEE);
            ArbitrationBaudRates.Add(50, 0x0041BBEE);
            ArbitrationBaudRates.Add(100, 0x0041BBEE);
            ArbitrationBaudRates.Add(125, 0x0001AFBE);
            ArbitrationBaudRates.Add(250, 0x0001AFBE);
            ArbitrationBaudRates.Add(500, 0x0001975E);
            ArbitrationBaudRates.Add(1000, 0x00018B2E);
        }

        private static void InitDataBaudRate()
        {
            DataBaudRates.Clear();
            DataBaudRates.Add(0, 0x00010207);
            DataBaudRates.Add(1000, 0x0081830E);
            DataBaudRates.Add(2000, 0x0041020A);
            DataBaudRates.Add(4000, 0x0001020A);
            DataBaudRates.Add(5000, 0x00010207);
        }

        public Boolean Send(UInt32 protocolId, Byte[] data)
        {
            if (data.Length == 64 || CanType > 0)
            {
                return SendFD(protocolId, data);
            }
            var canData = new Zlgcan.ZCAN_Transmit_Data();
            canData.canFrame.ID = protocolId;
            canData.canFrame.data = new Byte[data.Length];
            Array.Copy(data, canData.canFrame.data, data.Length);
            canData.canFrame.canDlc = (byte)data.Length;
            IntPtr sendPtr = Marshal.AllocHGlobal(Marshal.SizeOf(canData));
            Marshal.StructureToPtr(canData, sendPtr, true);
            var len = CxkjCanFDConnector.ZCAN_Transmit(channelHandle, sendPtr, 1);
            Marshal.FreeHGlobal(sendPtr);
            return len > 0;
        }

        /// <summary>
        /// 使用FD加速
        /// </summary>
        /// <param name="protocolId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean SendFD(UInt32 protocolId, Byte[] data)
        {
            var canData = new Zlgcan.ZCAN_TransmitFD_Data();
            canData.frame.ID = protocolId;
            canData.frame.data = new Byte[64];
            Array.Copy(data, canData.frame.data, data.Length);
            canData.frame.len = (byte)data.Length;
            if (UseCanfd)
            {
                canData.frame.flags = (byte)(CanType == 2 ? 3 : 1);
            }
            IntPtr sendPtr = Marshal.AllocHGlobal(Marshal.SizeOf(canData));
            Marshal.StructureToPtr(canData, sendPtr, true);
            var len = CxkjCanFDConnector.ZCAN_TransmitFD(channelHandle, sendPtr, 1);
            Marshal.FreeHGlobal(sendPtr);
            return len > 0;
        }
        /// <summary>
        /// 多帧发送：将List<byte[]>转化为ZCAN_Transmit_Data结构体数组指针，调用ZCAN_Transmit
        /// </summary>
        /// <param name="protocolId">CAN帧ID（如0x7E0）</param>
        /// <param name="frames">多帧数据（每个元素是1个CAN帧的8字节数据）</param>
        /// <param name="channelHandle">CAN通道句柄（从驱动获取）</param>
        /// <returns>是否全部发送成功</returns>
        public Boolean SendMuiltFrame(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            // 合法性检查
            if (frames == null || frames.Count == 0 || bs == 0)
                throw new ArgumentNullException(nameof(frames), "帧集合不能为空");
            if (channelHandle == IntPtr.Zero)
                throw new ArgumentException("CAN通道句柄无效");
            if (frames[0].Data.Length > 8)
            {
                return SendMuiltFrameFD(protocolId, frames, offset, bs);
            }
            uint len = (uint)bs;
            // 1. 创建ZCAN_Transmit_Data数组（数量=帧数量）
            Zlgcan.ZCAN_Transmit_Data[] transmitDataArray = new Zlgcan.ZCAN_Transmit_Data[bs];
            // 2. 逐个填充每个帧的结构体数据
            for (int i = 0; i < bs; i++)
            {
                byte[] currentFrame = frames[i + offset].Data;
                if (currentFrame == null || currentFrame.Length > 8)
                    throw new ArgumentException($"第{i}帧数据长度非法（需≤8字节）");
                var canData = new Zlgcan.ZCAN_Transmit_Data();
                canData.canFrame.ID = IsContius ? protocolId : frames[i + offset].CanID;
                canData.canFrame.data = new Byte[8];
                Array.Copy(frames[i + offset].Data, canData.canFrame.data, frames[i + offset].Data.Length);
                canData.canFrame.canDlc = (byte)frames[i + offset].Data.Length;
                transmitDataArray[i] = canData;
            }

            // 3. 计算非托管内存大小：单个ZCAN_Transmit_Data的大小 × 数组长度
            int singleStructSize = Marshal.SizeOf(typeof(Zlgcan.ZCAN_Transmit_Data));
            int totalBufferSize = singleStructSize * transmitDataArray.Length;
            // 4. 分配非托管内存，用于存放结构体数组
            IntPtr transmitDataPtr = Marshal.AllocHGlobal(totalBufferSize);
            try
            {
                // 5. 将结构体数组逐个封送到非托管内存
                for (int i = 0; i < transmitDataArray.Length; i++)
                {
                    // 计算当前结构体在非托管内存中的偏移地址
                    IntPtr currentStructPtr = IntPtr.Add(transmitDataPtr, i * singleStructSize);
                    // 将托管结构体封送到非托管内存
                    Marshal.StructureToPtr(transmitDataArray[i], currentStructPtr, false);
                }

                // 6. 调用周立功ZCAN_Transmit接口发送多帧
                // 注意：ZCAN_Transmit的第三个参数是“发送的帧数量”
                uint sentCount = CxkjCanFDConnector.ZCAN_Transmit(channelHandle, transmitDataPtr, len);
                // 返回是否全部帧发送成功
                return sentCount == transmitDataArray.Length;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                // 7. 释放非托管内存（必须执行，避免内存泄漏）
                Marshal.FreeHGlobal(transmitDataPtr);
            }
            return false;
        }

        public Boolean SendMuiltFrameFD(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            uint len = (uint)bs;
            // 1. 创建ZCAN_Transmit_Data数组（数量=帧数量）
            Zlgcan.ZCAN_TransmitFD_Data[] transmitDataArray = new Zlgcan.ZCAN_TransmitFD_Data[bs];
            byte flag = 0;
            if (UseCanfd)
            {
                flag = (byte)(CanType == 2 ? 3 : 1);
            }
            // 2. 逐个填充每个帧的结构体数据
            for (int i = 0; i < bs; i++)
            {
                byte[] currentFrame = frames[i + offset].Data;
                var canData = new Zlgcan.ZCAN_TransmitFD_Data();
                canData.frame.ID = IsContius ? protocolId : frames[i + offset].CanID;
                canData.frame.data = new Byte[64];
                canData.frame.flags = flag;
                Array.Copy(frames[i + offset].Data, canData.frame.data, frames[i + offset].Data.Length);
                canData.frame.len = (byte)CanTool.GetCanFdValidLength(frames[i + offset].Data.Length);
                transmitDataArray[i] = canData;
            }

            // 3. 计算非托管内存大小：单个ZCAN_Transmit_Data的大小 × 数组长度
            int singleStructSize = Marshal.SizeOf(typeof(Zlgcan.ZCAN_TransmitFD_Data));
            int totalBufferSize = singleStructSize * transmitDataArray.Length;
            // 4. 分配非托管内存，用于存放结构体数组
            IntPtr transmitDataPtr = Marshal.AllocHGlobal(totalBufferSize);
            try
            {
                // 5. 将结构体数组逐个封送到非托管内存
                for (int i = 0; i < transmitDataArray.Length; i++)
                {
                    // 计算当前结构体在非托管内存中的偏移地址
                    IntPtr currentStructPtr = IntPtr.Add(transmitDataPtr, i * singleStructSize);
                    // 将托管结构体封送到非托管内存
                    Marshal.StructureToPtr(transmitDataArray[i], currentStructPtr, false);
                }

                // 6. 调用周立功ZCAN_Transmit接口发送多帧
                // 注意：ZCAN_Transmit的第三个参数是“发送的帧数量”
                uint sentCount = CxkjCanFDConnector.ZCAN_TransmitFD(channelHandle, transmitDataPtr, len);
                // 返回是否全部帧发送成功
                return sentCount == transmitDataArray.Length;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
            finally
            {
                // 7. 释放非托管内存（必须执行，避免内存泄漏）
                Marshal.FreeHGlobal(transmitDataPtr);
            }
            return false;
        }

        public Boolean GetOnlineState(ChannelConfig cc)
        {
            Int64 canKey = GenaralKey((Int32)cc.CCategroy, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (OpenedCans.TryGetValue(canKey, out var can))
            {// 2 online -3 -offline
                return CxkjCanFDConnector.ZCAN_IsDeviceOnLine(can.canHandle) == 2;
            }
            return false;
        }

        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            return false;
        }
        #endregion

        #region 3 --- 参数配置集合
        private static readonly ConcurrentDictionary<Int64, (IntPtr canHandle, List<IntPtr> channelHandles)> OpenedCans
            = new ConcurrentDictionary<Int64, (IntPtr can, List<IntPtr> channels)>();
        private static readonly Dictionary<Int32, (Byte timing0, Byte timing1)> baudRates
            = new Dictionary<Int32, (Byte timing0, Byte timing1)>();
        /// <summary>
        /// 仲裁波特率
        /// </summary>
        private static readonly Dictionary<Int32, UInt32> ArbitrationBaudRates =
            new Dictionary<Int32, UInt32>();
        /// <summary>
        /// 数据波特率
        /// </summary>
        private static readonly Dictionary<Int32, UInt32> DataBaudRates =
            new Dictionary<Int32, UInt32>();

        #endregion
    }
}
