using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static Rxbit.Brigade.CanDevices.Structs.GcanFD;

namespace Rxbit.Brigade.CanDevices.Services
{
    internal class GcCanfdChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        private int ptSize = Marshal.SizeOf(typeof(GcanFD.CANFD_OBJ));
        private int maxlen = 900;
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }
        private GcanFD.ResultStatus status;

        public const uint CAN_EFF_FLAG = 0x80000000;  // 扩展帧
        /// <summary>
        /// 判断是否为扩展帧
        /// </summary>
        public bool IsExtendedFrame(uint canId)
        {
            return (canId & CAN_EFF_FLAG) != 0;
        }
        /// <summary>
        /// CAN只能解析CAN数据 CANFD支持解析CAN和CANFD数据
        /// </summary>
        /// <returns></returns>
        public List<RXCanMessage> Read()
        {
            IntPtr pt = Marshal.AllocHGlobal(ptSize * maxlen);
            uint mLen = 800u;
            status = GcanFdConnector.ReceiveFD(GcanFdConnector.USBCANFD, ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, pt, ref mLen);
            List<RXCanMessage> datas = new List<RXCanMessage>((int)mLen);
            if (mLen > 0)
            {
                for (int i = 0; i < mLen; i++)
                {
                    GcanFD.CANFD_OBJ data = (GcanFD.CANFD_OBJ)Marshal.PtrToStructure((IntPtr)(pt + i * ptSize), typeof(GcanFD.CANFD_OBJ));
                    var message = new RXCanMessage()
                    {
                        DLC = data.DataLen,
                        TIMESTAMP = data.TimeStamp.ToUnixTimestampMicroseconds(),
                        ID = data.ID,
                        IsExdFrame = data.IsExtFrame(),
                        DATA = new Byte[data.DataLen > 8 ? 64 : 8],
                    };
                    Buffer.BlockCopy(data.Data, 0, message.DATA, 0, data.DataLen);
                    datas.Add(message);
                }
            }
            Marshal.FreeHGlobal(pt);
            return datas;
        }



        public Boolean Close()
        {
            Int64 canKey = (Int32)this.ChannelConfig.CCategroy * 1000 + this.ChannelConfig.CanIndex * 100;
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                status = GcanFdConnector.StartCANFD(GcanFdConnector.USBCANFD, this.ChannelConfig.CanIndex, this.ChannelConfig.ChannelIndex);
                status = GcanFdConnector.ResetCANFD(GcanFdConnector.USBCANFD, this.ChannelConfig.CanIndex, this.ChannelConfig.ChannelIndex);
                can.channels.Remove(this.ChannelConfig.ChannelIndex);
                if (can.channels.Count == 0)
                {
                    status = GcanFdConnector.CloseDeviceFD(GcanFdConnector.USBCANFD, this.ChannelConfig.CanIndex);
                    OpenedCans.TryRemove(canKey, out can);
                }
                return status == ResultStatus.STATUS_OK;
            }
            return false;
        }

        public Boolean Open(ChannelConfig ccc)
        {
            ChannelConfig = ccc;
            var can = OpenCan(ccc.CCategroy, ccc.CanIndex);
            Int32 baudRate = (Int32)ccc.BaudRate;
            Int32 dataBaudRate = (Int32)ccc.DataBaudRate;
            GcanFD.INIT_CONFIG[] Canfd_InitConfig_CAN1 = new GcanFD.INIT_CONFIG[2];
            Canfd_InitConfig_CAN1[0].NominalBitRateSelect = ArbitrationBaudRates[baudRate];
            Canfd_InitConfig_CAN1[0].DataBitRateSelect = DataBaudRates[dataBaudRate];
            Canfd_InitConfig_CAN1[0].DataBitRate = 0u;
            Canfd_InitConfig_CAN1[0].CanReceMode = ((byte)GcanFD.RECEIVE_MODE.GLOBAL_STANDARD_AND_EXTENDED_RECEIVE);
            Canfd_InitConfig_CAN1[0].CanSendMode = ((byte)GcanFD.SEND_MODE.POSITIVE_SEND);
            if (status != ResultStatus.STATUS_OK) return false;
            status = GcanFdConnector.InitCANFD(GcanFdConnector.USBCANFD, ccc.CanIndex, ccc.ChannelIndex, Canfd_InitConfig_CAN1);
            if (status != ResultStatus.STATUS_OK)
            {
                Close();
                return false;
            }
            status = GcanFdConnector.StartCANFD(GcanFdConnector.USBCANFD, ccc.CanIndex, ccc.ChannelIndex);
            if (status == ResultStatus.STATUS_OK)
            {
                can.channelindex.Add(ccc.ChannelIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        private (uint devIndex, List<uint> channelindex) OpenCan(CanCategory cc, Byte index)
        {
            Int64 canKey = (Int32)cc * 1000 + index * 100;
            if (!OpenedCans.TryGetValue(canKey, out var can))
            {
                status = GcanFdConnector.OpenDeviceFD(GcanFdConnector.USBCANFD, index);
                if (status == ResultStatus.STATUS_OK)
                {
                    var ncan = (index, new List<uint>());
                    if (OpenedCans.TryAdd(canKey, ncan))
                    {
                        return ncan;
                    }
                }
                else
                {
                    GcanFdConnector.CloseDeviceFD(GcanFdConnector.USBCANFD, index);
                }
                return (0, new List<uint>());
            }
            else
            {
                return can;
            }
        }

        static GcCanfdChannelService()
        {
            InitArbitrationBaudRates();
            InitDataBaudRate();
        }

        private static void InitArbitrationBaudRates()
        {
            ArbitrationBaudRates.Clear();
            ArbitrationBaudRates.Add(10, 14);
            ArbitrationBaudRates.Add(50, 10);
            ArbitrationBaudRates.Add(100, 7);
            ArbitrationBaudRates.Add(125, 6);
            ArbitrationBaudRates.Add(250, 5);
            ArbitrationBaudRates.Add(500, 2);
            ArbitrationBaudRates.Add(1000, 0);
        }

        private static void InitDataBaudRate()
        {
            DataBaudRates.Clear();
            DataBaudRates.Add(0, 18);
            DataBaudRates.Add(1000, 3);
            DataBaudRates.Add(2000, 2);
            DataBaudRates.Add(4000, 1);
            DataBaudRates.Add(5000, 0);
        }

        public Boolean Send(UInt32 protocolId, Byte[] data)
        {
            GcanFD.CANFD_OBJ[] frames = new GcanFD.CANFD_OBJ[1];
            GcanFD.CANFD_OBJ obj = new GcanFD.CANFD_OBJ() { Reserved = new byte[2], };
            obj.Data = new byte[64];
            obj.ID = protocolId;
            obj.DataLen = (byte)(data.Length);
            if (UseCanfd)
            {
                obj.Set_CanORCanfdType(1, 0, 0, 1);
            }
            else
            {
                obj.Set_CanORCanfdType((byte)(CanType > 0 ? 1 : 0), 0, 0, 0);
            }
            Buffer.BlockCopy(data, 0, obj.Data, 0, data.Length);
            frames[0] = obj;
            status = GcanFdConnector.TransmitFD(GcanFdConnector.USBCANFD,
                ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, frames, 1u);
            return status == ResultStatus.STATUS_OK;
        }
        /// <summary>
        /// 获取 CAN FD 合法长度（自动向上取整到标准值）
        /// </summary>
        public static byte GetCanFdValidLength(int len)
        {
            if (len <= 8) return 8;
            else if (len <= 12) return 12;
            else if (len <= 16) return 16;
            else if (len <= 20) return 20;
            else if (len <= 24) return 24;
            else if (len <= 32) return 32;
            else if (len <= 48) return 48;
            else if (len <= 64) return 64;
            else
            {
                throw new ArgumentException("CAN FD 最大 64 字节");
            }
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
            if (OpenedCans.Count == 0)
                throw new ArgumentException("CAN通道句柄无效");
            uint len = (uint)bs;
            GcanFD.CANFD_OBJ[] list = new GcanFD.CANFD_OBJ[bs];
            byte proto = (byte)(CanType > 0 ? 1 : 0);
            byte BRS = 0;
            if (UseCanfd)
            {
                BRS = 1;
            }


            // 1. 创建ZCAN_Transmit_Data数组（数量=帧数量）
            // 2. 逐个填充每个帧的结构体数据
            for (int i = 0; i < bs; i++)
            {
                byte[] currentFrame = frames[i + offset].Data;
                GcanFD.CANFD_OBJ obj = new GcanFD.CANFD_OBJ() { Reserved = new byte[2] };
                obj.ID = IsContius ? protocolId : frames[i + offset].CanID;
                obj.DataLen = (byte)(frames[i + offset].Data.Length <= 8 ? 8 : 64);
                obj.Data = new byte[64];
                obj.Set_CanORCanfdType(proto, 0, 0, BRS);
                Array.Copy(frames[i + offset].Data, obj.Data, frames[i + offset].Data.Length);
                list[i] = obj;
            }
            int step = bs > 40 ? 40 : bs;
            for (int i = 0; i < bs; i += step)
            {
                int minLen = Math.Min(step, bs - i);
                GcanFD.CANFD_OBJ[] cs = new GcanFD.CANFD_OBJ[minLen];
                Array.Copy(list, i, cs, 0, minLen);
                status = GcanFdConnector.TransmitFD(GcanFdConnector.USBCANFD,
                    ChannelConfig.CanIndex, ChannelConfig.ChannelIndex, cs, (uint)minLen);
                if (status != ResultStatus.STATUS_OK)
                {
                    throw new ArgumentException($"第{i}帧发送失败");
                }
            }
            return status == ResultStatus.STATUS_OK;
        }

        public Boolean GetOnlineState(ChannelConfig cc)
        {
            Int64 canKey = (Int32)cc.CCategroy * 1000 + cc.CanIndex * 100;
            if (OpenedCans.TryGetValue(canKey, out var can))
            {
                GcanFD.CANFD_STATUS[] devstatus = new CANFD_STATUS[2];
                status = GcanFdConnector.GetCanfdBusStatus(GcanFdConnector.USBCANFD, cc.CanIndex, devstatus);
                return status == ResultStatus.STATUS_OK;
            }
            return false;
        }

        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            return false;
        }

        private static readonly ConcurrentDictionary<Int64, (uint devindex, List<uint> channels)> OpenedCans
            = new ConcurrentDictionary<Int64, (uint devindex, List<uint> channels)>();

        /// <summary>
        /// 仲裁波特率
        /// </summary>
        private static readonly Dictionary<Int32, byte> ArbitrationBaudRates =
            new Dictionary<Int32, byte>();
        /// <summary>
        /// 数据波特率
        /// </summary>
        private static readonly Dictionary<Int32, byte> DataBaudRates =
            new Dictionary<Int32, byte>();
    }
}
