using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Services
{
    internal class GCanChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }
        private Byte dataProtoValue;
        private uint deviceType = 1;

        /// <summary>
        /// CAN只能解析CAN数据 CANFD支持解析CAN和CANFD数据
        /// </summary>
        /// <returns></returns>
        public List<RXCanMessage> Read()
        {
            List<RXCanMessage> datas = new List<RXCanMessage>();

            var length = GcanConnector.GetReceiveNum(deviceType, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (length > 0)
            {
                Int32 size = Marshal.SizeOf(typeof(Gcan.CAN_OBJ));
                IntPtr receivePtr = Marshal.AllocHGlobal((Int32)length * size);
                var receiveLength = GcanConnector.Receive(deviceType, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, receivePtr, length, 1);
                for (int i = 0; i < receiveLength; i++)
                {
                    IntPtr offsetPtr = IntPtr.Add(receivePtr, i * size);
                    var canData = (Gcan.CAN_OBJ)Marshal.PtrToStructure(
                      offsetPtr, typeof(Gcan.CAN_OBJ));
                    var message = new RXCanMessage()
                    {
                        DLC = canData.DataLen,
                        TIMESTAMP = canData.TimeStamp,
                        ID = canData.ID,
                        IsExdFrame = canData.ExternFlag == 1,
                        DATA = new Byte[8],
                    };
                    if (canData.DataLen > 8)
                    {
                        message.DLC = 8;
                    }
                    Array.Copy(canData.data, message.DATA, 8);
                    datas.Add(message);
                }
                Marshal.FreeHGlobal(receivePtr);
            }

            return datas;
        }

        public Boolean Close()
        {
            Int64 canKey = (Int32)this.ChannelConfig.CCategroy * 1000 + this.ChannelConfig.CanIndex * 100;
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                GcanConnector.ResetCAN(deviceType, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
                can.channelHandles.Remove(ChannelConfig.CanIndex);
                if (can.channelHandles.Count == 0)
                {
                    uint ret = GcanConnector.CloseDevice(deviceType, ChannelConfig.CanIndex);
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
            var can = OpenCan(ccc.CCategroy, ccc.CanIndex);
            if (can.canHandle != 99)
            {
                Int32 baudRate = (Int32)ccc.BaudRate;
                Int32 dataBaudRate = (Int32)ccc.DataBaudRate;

                Gcan.INIT_CONFIG channelConfig = new Gcan.INIT_CONFIG()
                {
                    Mode = 0,
                    Filter = 0,
                    AccCode = 0,
                    AccMask = 0xFFFFFFFF,
                    Timing0 = baudRates[baudRate].timing0,
                    Timing1 = baudRates[baudRate].timing1,
                };

                var initStatus = GcanConnector.InitCAN(deviceType, can.canHandle, ccc.ChannelIndex, ref channelConfig);
                if (initStatus != 0)
                {
                    GcanConnector.ClearBuffer(deviceType, can.canHandle, ccc.ChannelIndex);
                    GcanConnector.ResetCAN(deviceType, can.canHandle, ccc.ChannelIndex);
                    var startResult = GcanConnector.StartCAN(deviceType, can.canHandle, ccc.ChannelIndex);
                    if (startResult == 1)
                    {
                        can.channelHandles.Add(ccc.ChannelIndex);
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

        private (UInt32 canHandle, List<UInt32> channelHandles) OpenCan(CanCategory cc, Byte index)
        {
            Int64 canKey = (Int32)cc * 1000 + index * 100;
            if (!OpenedCans.TryGetValue(canKey, out var can))
            {
                //if (cc == CanCategory.G_USBCAN_I_PRO)
                //{
                //    deviceType = ((uint)CanCategory.G_USBCAN_I_PRO);
                //}
                //if (cc == CanCategory.G_USBCAN2)
                //{
                //    deviceType = ((uint)CanCategory.G_USBCAN2);
                //}
                uint deviceType = ((uint)cc) % 1000;
                var canHandle = GcanConnector.OpenDevice(deviceType, index, 0);
                if (canHandle != 0)
                {
                    var ncan = (index, new List<uint>());
                    if (OpenedCans.TryAdd(canKey, ncan))
                    {
                        return ncan;
                    }
                }

                return (99, new List<uint>());
            }
            else
            {
                return can;
            }
        }
        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            return false;
        }
        static GCanChannelService()
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
            var canData = new Gcan.CAN_OBJ();
            canData.ID = protocolId;
            canData.data = new Byte[data.Length];
            Array.Copy(data, canData.data, data.Length);
            canData.DataLen = (byte)data.Length;
            IntPtr sendPtr = Marshal.AllocHGlobal(Marshal.SizeOf(canData));
            Marshal.StructureToPtr(canData, sendPtr, true);
            var len = GcanConnector.Transmit(deviceType, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, sendPtr, 1);
            Marshal.FreeHGlobal(sendPtr);

            return len > 0;
        }

        /// <summary>
        /// 适配多帧发送
        /// </summary>
        /// <param name="protocolId"></param>
        /// <param name="frames"></param>
        /// <param name="offset"></param>
        /// <param name="bs"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public Boolean SendMuiltFrame(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            // 合法性检查
            if (frames == null || bs == 0)
                throw new ArgumentNullException(nameof(frames), "帧集合不能为空");
            if (deviceType < 0)
                throw new ArgumentException("CAN通道句柄无效");

            ushort len = (ushort)bs;
            // 1. 创建ZCAN_Transmit_Data数组（数量=帧数量）
            Gcan.CAN_OBJ[] transmitDataArray = new Gcan.CAN_OBJ[bs];
            // 2. 逐个填充每个帧的结构体数据
            for (int i = 0; i < bs; i++)
            {
                byte[] currentFrame = frames[i + offset].Data;
                if (currentFrame == null || currentFrame.Length > 8)
                    throw new ArgumentException($"第{i}帧数据长度非法（需≤8字节）");
                var canData = new Gcan.CAN_OBJ();
                canData.ID = IsContius ? protocolId : frames[i + offset].CanID;
                canData.data = new Byte[frames[i + offset].Data.Length];
                Array.Copy(frames[i + offset].Data, canData.data, frames[i + offset].Data.Length);
                canData.DataLen = (byte)frames[i + offset].Data.Length;
                IntPtr sendPtr = Marshal.AllocHGlobal(Marshal.SizeOf(canData));
                Marshal.StructureToPtr(canData, sendPtr, true);
                transmitDataArray[i] = canData;
            }

            // 3. 计算非托管内存大小：单个ZCAN_Transmit_Data的大小 × 数组长度
            int singleStructSize = Marshal.SizeOf(typeof(Gcan.CAN_OBJ));
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
                uint sentCount = GcanConnector.Transmit(deviceType, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, transmitDataPtr, len);
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
            Int64 canKey = (Int32)cc.CCategroy * 1000 + cc.CanIndex * 100;
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                Gcan.BOARD_INFO bOARD_INFO = new Gcan.BOARD_INFO();
                return GcanConnector.ReadBoardInfo(deviceType, cc.CanIndex, out bOARD_INFO) == 1;
            }
            return false;
        }

        private static readonly ConcurrentDictionary<Int64, (UInt32 canHandle, List<UInt32> channelHandles)> OpenedCans
            = new ConcurrentDictionary<Int64, (UInt32 can, List<UInt32> channels)>();
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
    }
}
