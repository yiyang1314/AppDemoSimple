using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Rxbit.Brigade.CanDevices.Structs.CxkjCan;

namespace Rxbit.Brigade.CanDevices.Services
{
    internal class CxkjCanChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }
        private Byte dataProtoValue;
        private volatile bool mIsVaild;
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        const int ZCAN_USBCANFD_200U = 2;
        private int DevType = DEV_USBCAN2;
        /// <summary>
        /// CAN只能解析CAN数据 CANFD支持解析CAN和CANFD数据
        /// </summary>
        /// <returns></returns>
        public unsafe List<RXCanMessage> Read()
        {
            List<RXCanMessage> datas = new List<RXCanMessage>();
            var len = CxkjCanConnector.VCI_GetReceiveNum(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (len > 0)
            {
                CxkjCan.VCI_CAN_OBJ[] msvnf = new CxkjCan.VCI_CAN_OBJ[len];
                var length = CxkjCanConnector.VCI_Receive(DEV_USBCAN2,
                    ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, ref msvnf[0], len, -1);
                if (length == 0xFFFFFFFF) return datas;
                for (uint i = 0; i < length; i++)
                {
                    byte dlen = msvnf[i].DataLen;
                    if (msvnf[i].DataLen > 8)
                    {
                        dlen = 8;
                    }
                    //bool exframe = msvnf[i].ExternFlag != 0;//=0 标准帧，扩展帧1
                    //bool remoteframe = msvnf[i].RemoteFlag != 0; /// =0 数据帧， 1 远程帧
                    var message = new RXCanMessage()
                    {
                        DLC = dlen,
                        TIMESTAMP = msvnf[i].TimeStamp,
                        ID = msvnf[i].ID,
                        IsExdFrame = msvnf[i].ExternFlag == 1,
                        DATA = new Byte[8],
                    };
                    message.DATA = DataToArray(ref msvnf[i]);
                    datas.Add(message);
                }

            }
            return datas;
        }

        public Boolean Close()
        {
            mIsVaild = false;
            Int64 canKey = GenaralKey((Int32)ChannelConfig.CCategroy, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                CxkjCanConnector.VCI_ResetCAN(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
                can.channelHandles.Remove(ChannelConfig.ChannelIndex);
                if (can.channelHandles.Count == 0)
                {
                    CxkjCanConnector.VCI_CloseDevice(DEV_USBCAN2, ChannelConfig.CanIndex);
                    OpenedCans.TryRemove(canKey, out can);
                }
                mIsVaild = false;
                return true;
            }
            return false;
        }

        public Boolean Open(ChannelConfig ccc)
        {
            ChannelConfig = ccc;
            this.dataProtoValue = (Byte)(ccc.DataProto == DataProtocol.CANFD ? 1 : 0);

            DevType = ((int)ccc.CCategroy) % 1000;
            var can = OpenCan(ccc.CCategroy, ccc.CanIndex, ccc.ChannelIndex);
            if (can.canHandle == 1)
            {
                Int32 baudRate = (Int32)ccc.BaudRate;
                VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
                config.AccCode = 0;
                config.AccMask = 0xFFFFFFFF;
                config.Timing0 = baudRates[baudRate].timing0;
                config.Timing1 = baudRates[baudRate].timing1;
                config.Filter = 1;
                config.Mode = 0;
                var ret = CxkjCanConnector.VCI_InitCAN(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, ref config);
                if (ret != 1)
                {
                    return false;
                }
                ret = CxkjCanConnector.VCI_StartCAN(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
                if (ret != 1)
                {
                    return false;
                }
                CxkjCanConnector.VCI_ClearBuffer(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
                ret = CxkjCanConnector.VCI_StartCAN(DEV_USBCAN2, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
                if (ret == 1)
                {
                    can.channelHandles.Add(ChannelConfig.ChannelIndex);
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
        private Int64 GenaralKey(int devtype, byte devId, uint channelindex)
        {
            return devtype * 1000 + devId * 100;
        }
        private (UInt32 canHandle, List<UInt32> channelHandles) OpenCan(CanCategory cc, Byte index, uint channelIndex)
        {
            Int64 canKey = GenaralKey((Int32)cc, index, channelIndex);
            if (!OpenedCans.TryGetValue(canKey, out var can))
            {
                UInt32 category = (UInt32)CanCategory.CXKJ_CANalyst_II % 1000;
                var canHandle = CxkjCanConnector.VCI_OpenDevice(category, index, 0);
                if (canHandle != 0)
                {
                    mIsVaild = true;
                    var ncan = (canHandle, new List<uint>());
                    if (OpenedCans.TryAdd(canKey, ncan))
                    {
                        return ncan;
                    }
                }

                return (99, new List<uint>());
            }
            else
            {
                mIsVaild = true;
                return can;
            }
        }

        static CxkjCanChannelService()
        {
            baudRates.Add(50, (0x09, 0x1C));
            baudRates.Add(100, (0x04, 0x1C));
            baudRates.Add(125, (0x03, 0x1C));
            baudRates.Add(200, (0x81, 0xFA));
            baudRates.Add(250, (0x01, 0x1C));
            baudRates.Add(500, (0x00, 0x1C));
            baudRates.Add(1000, (0x00, 0x14));
        }

        unsafe public Boolean Send(UInt32 protocolId, Byte[] data)
        {
            if (data.Length > 8)
            {
                throw new ArgumentException("不支持CANFD加速..");
            }
            if (!mIsVaild)
                throw new ArgumentException("CAN通道句柄无效");
            CxkjCan.VCI_CAN_OBJ sendobj = new CxkjCan.VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.RemoteFlag = 0;
            sendobj.ExternFlag = 0;
            sendobj.ID = protocolId;
            sendobj.DataLen = (byte)data.Length;
            ArrayToData(data, ref sendobj);
            return CxkjCanConnector.VCI_Transmit(DEV_USBCAN2,
                    ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, ref sendobj, 1) > 0;
        }

        /// <summary>
        /// 多帧发送：将List<byte[]>转化为ZCAN_Transmit_Data结构体数组指针，调用ZCAN_Transmit
        /// </summary>
        /// <param name="protocolId">CAN帧ID（如0x7E0）</param>
        /// <param name="frames">多帧数据（每个元素是1个CAN帧的8字节数据）</param>
        /// <param name="channelHandle">CAN通道句柄（从驱动获取）</param>
        /// <returns>是否全部发送成功</returns>
        unsafe public Boolean SendMuiltFrame(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            // 合法性检查
            if (frames == null || bs == 0)
                throw new ArgumentNullException(nameof(frames), "帧集合不能为空");
            if (!mIsVaild)
                throw new ArgumentException("CAN通道句柄无效");

            uint len = (uint)bs;
            // 1. 创建ZCAN_Transmit_Data数组（数量=帧数量）
            CxkjCan.VCI_CAN_OBJ[] transmitDataArray = new CxkjCan.VCI_CAN_OBJ[bs];
            // 2. 逐个填充每个帧的结构体数据
            for (int i = 0; i < bs; i++)
            {
                byte[] currentFrame = frames[i + offset].Data;
                if (currentFrame == null || currentFrame.Length > 8)
                    throw new ArgumentException($"第{i}帧数据长度非法（需≤8字节）");
                transmitDataArray[i].ID = IsContius ? protocolId : frames[i + offset].CanID;
                transmitDataArray[i].DataLen = (byte)currentFrame.Length;
                ArrayToData(currentFrame, ref transmitDataArray[i]);
            }
            return CxkjCanConnector.VCI_Transmit(DEV_USBCAN2,
             ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, ref transmitDataArray[0], len) > 0;
        }

        /// <summary>
        /// 使用FD加速
        /// </summary>
        /// <param name="protocolId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Boolean SendFD(UInt32 protocolId, Byte[] data)
        {
            //var canData = new Zlgcan.ZCAN_TransmitFD_Data();
            //canData.frame.ID = protocolId;
            //canData.frame.data = new Byte[data.Length];
            //Array.Copy(data, canData.frame.data, data.Length);
            //canData.frame.len = (byte)data.Length;
            //IntPtr sendPtr = Marshal.AllocHGlobal(Marshal.SizeOf(canData));
            //Marshal.StructureToPtr(canData, sendPtr, true);
            //var len = ZlgcanConnector.ZCAN_TransmitFD(channelHandle, sendPtr, 1);
            //Marshal.FreeHGlobal(sendPtr);
            return false;
        }

        public Boolean GetOnlineState(ChannelConfig cc)
        {
            Int64 canKey = GenaralKey((Int32)cc.CCategroy, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex);
            if (OpenedCans.TryGetValue(canKey, out var can) && mIsVaild)
            {
                CxkjCan.VCI_BOARD_INFO pInfo = new CxkjCan.VCI_BOARD_INFO();
                return CxkjCanConnector.VCI_ReadBoardInfo(DEV_USBCAN2,
             ChannelConfig.CanIndex, ref pInfo) == 1;
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
        public static unsafe byte[] DataToArray(ref VCI_CAN_OBJ obj)
        {
            byte[] buf = new byte[8];   // 只拿有效长度
            fixed (byte* p = obj.Data)            // fixed 住嵌入式数组
            {
                Marshal.Copy((IntPtr)p, buf, 0, buf.Length);
            }
            return buf;
        }
        public static unsafe void ArrayToData(byte[] source, ref VCI_CAN_OBJ obj)
        {
            if (source == null || source.Length > 8)
                throw new ArgumentException("Length max 8");

            obj.DataLen = (byte)source.Length;
            fixed (byte* p = obj.Data)            // 直接拿到指针
            {
                Marshal.Copy(source, 0, (IntPtr)p, source.Length);
            }
        }

        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            return true;
        }
    }
}
