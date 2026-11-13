using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Generic;
using System.Threading;
using static Rxbit.Brigade.CanDevices.Structs.PCAN;

namespace Rxbit.Brigade.CanDevices.Services
{
    public class PeakCanChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        private UInt16 channelHandle;
        private Byte dataProtoValue;
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }

        private volatile bool IsInit;

        /// <summary>
        /// 仲裁波特率
        /// </summary>
        private static readonly Dictionary<Int32, string> ArbitrationBaudRates =
            new Dictionary<Int32, string>();
        /// <summary>
        /// 数据波特率
        /// </summary>
        private static readonly Dictionary<Int32, string> DataBaudRates =
            new Dictionary<Int32, string>();

        static PeakCanChannelService()
        {
            InitArbitrationBaudRates();
            InitDataBaudRate();
        }
        /// <summary>
        /// 仲裁域波特率，采样率80%
        /// </summary>
        private static void InitArbitrationBaudRates()
        {
            ArbitrationBaudRates.Clear();
            ArbitrationBaudRates.Add(10, "nom_brp=24, nom_tseg1=199, nom_tseg2=50, nom_sjw=1,");
            ArbitrationBaudRates.Add(50, "nom_brp=4, nom_tseg1=239, nom_tseg2=60, nom_sjw=1,");
            ArbitrationBaudRates.Add(100, "nom_brp=2, nom_tseg1=239, nom_tseg2=60, nom_sjw=1,");
            ArbitrationBaudRates.Add(125, "nom_brp=2, nom_tseg1=191, nom_tseg2=48, nom_sjw=1,");
            ArbitrationBaudRates.Add(250, "nom_brp=1, nom_tseg1=191, nom_tseg2=48, nom_sjw=1,");
            ArbitrationBaudRates.Add(500, "nom_brp=1, nom_tseg1=95, nom_tseg2=24, nom_sjw=1,");
            ArbitrationBaudRates.Add(1000, "nom_brp=1, nom_tseg1=47, nom_tseg2=12, nom_sjw=1,");
        }
        /// <summary>
        /// 数据域波特率，除5m采样率为75%外，其余采样率均为80%
        /// </summary>
        private static void InitDataBaudRate()
        {
            DataBaudRates.Clear();
            DataBaudRates.Add(0, "data_brp=12, data_tseg1=3, data_tseg2=1, data_sjw=1");
            DataBaudRates.Add(1000, "data_brp=12, data_tseg1=3, data_tseg2=1, data_sjw=1");
            DataBaudRates.Add(2000, "data_brp=6, data_tseg1=3, data_tseg2=1, data_sjw=1");
            DataBaudRates.Add(4000, "data_brp=3, data_tseg1=3, data_tseg2=1, data_sjw=1");
            DataBaudRates.Add(5000, "data_brp=3, data_tseg1=2, data_tseg2=1, data_sjw=1");
        }
        public Boolean Close()
        {
            var result = PeakCanConnector.Uninitialize(this.channelHandle);
            this.IsInit = false;
            return result == TPCANStatus.PCAN_ERROR_OK;
        }

        public Boolean Open(ChannelConfig ccc)
        {
            ChannelConfig = ccc;
            this.dataProtoValue = (Byte)(ccc.DataProto == DataProtocol.CANFD ? 1 : 0);
            if (this.dataProtoValue == 0)//CAN
            {
                TPCANBaudrate baudRate;
                switch (ccc.BaudRate)
                {
                    case ChannelBaudRate._10Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_10K;
                        break;
                    case ChannelBaudRate._50Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_50K;
                        break;
                    case ChannelBaudRate._100Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_100K;
                        break;
                    case ChannelBaudRate._125Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_125K;
                        break;
                    case ChannelBaudRate._250Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_250K;
                        break;
                    case ChannelBaudRate._500Kbps:
                        baudRate = TPCANBaudrate.PCAN_BAUD_500K;
                        break;
                    case ChannelBaudRate._1000Kbps:
                    default:
                        baudRate = TPCANBaudrate.PCAN_BAUD_1M;
                        break;
                }
                this.channelHandle = ConvertChannelIndex(ccc.ChannelIndex);
                var result = PeakCanConnector.Initialize(channelHandle, baudRate);
                this.IsInit = result == TPCANStatus.PCAN_ERROR_OK;
                return IsInit;
            }
            else//CANFD
            {
                //baudRate str "f_clock_mhz=20, nom_brp=5, nom_tseg1=2, nom_tseg2=1, nom_sjw=1, data_brp=2, data_tseg1=3, data_tseg2=1, data_sjw=1"
                string baudRateStr = $"f_clock_mhz=60, {ArbitrationBaudRates[(Int32)ccc.BaudRate]} {DataBaudRates[(Int32)ccc.DataBaudRate]}";
                this.channelHandle = ConvertChannelIndex(ccc.ChannelIndex);
                var result = PeakCanConnector.InitializeFD(channelHandle, baudRateStr);
                this.IsInit = result == TPCANStatus.PCAN_ERROR_OK;
                return IsInit;
            }
        }

        /// <summary>
        /// 注意：PEAK CAN 通道索引的起始值为 1
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <returns></returns>
        private UInt16 ConvertChannelIndex(Byte channelIndex)
        {
            if (channelIndex >= 1 && channelIndex <= 8)
            {
                return (UInt16)(0x50 + channelIndex);
            }
            else if (channelIndex >= 9 && channelIndex <= 16)
            {
                return (UInt16)(0x500 + channelIndex);
            }
            else
            {
                return 0;
            }
        }

        public List<RXCanMessage> Read()
        {
            List<RXCanMessage> datas = new List<RXCanMessage>();
            TPCANMsg canMsg;
            TPCANTimestamp canTimestamp;
            TPCANStatus stsResult;
            TPCANMsgFD canMsgFD;
            ulong canTimestampFD;

            if (this.dataProtoValue == 0)//CAN
            {
                stsResult = PeakCanConnector.Read(channelHandle, out canMsg, out canTimestamp);
                if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                {
                    var message = new RXCanMessage()
                    {
                        DLC = canMsg.LEN,
                        TIMESTAMP = canTimestamp.millis,
                        ID = canMsg.ID,
                        DATA = new Byte[8],
                    };
                    Array.Copy(canMsg.DATA, message.DATA, 8);
                    datas.Add(message);
                }
                else
                {
                    return datas;
                }
            }
            else//CANFD
            {
                stsResult = PeakCanConnector.Read(channelHandle, out canMsg, out canTimestamp);
                if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                {
                    if (canMsg.LEN == 8)
                    {
                        var message = new RXCanMessage()
                        {
                            DLC = canMsg.LEN,
                            TIMESTAMP = canTimestamp.millis,
                            ID = canMsg.ID,
                            DATA = new Byte[8],
                        };
                        Array.Copy(canMsg.DATA, message.DATA, 8);

                        datas.Add(message);
                    }
                }
                else
                {
                    return datas;
                }
                stsResult = PeakCanConnector.ReadFD(channelHandle, out canMsgFD, out canTimestampFD);
                if (stsResult != TPCANStatus.PCAN_ERROR_QRCVEMPTY)
                {
                    int dataLength = GetLengthFromDLC(canMsgFD.DLC, (canMsgFD.MSGTYPE & TPCANMessageType.PCAN_MESSAGE_FD) == 0);
                    var message = new RXCanMessage()
                    {
                        DLC = (byte)dataLength,
                        TIMESTAMP = canTimestampFD,
                        ID = canMsgFD.ID,
                        DATA = new Byte[dataLength <= 8 ? 8 : 64],
                    };
                    Array.Copy(canMsgFD.DATA, 0, message.DATA, 0, dataLength);
                    datas.Add(message);
                }
                else
                {
                    return datas;
                }
            }
            return datas;
        }
        private int GetLengthFromDLC(int dlc, bool isSTD)
        {
            if (dlc <= 8)
                return dlc;

            if (isSTD)
                return 8;

            switch (dlc)
            {
                case 9: return 12;
                case 10: return 16;
                case 11: return 20;
                case 12: return 24;
                case 13: return 32;
                case 14: return 48;
                case 15: return 64;
                default: return dlc;
            }
        }
        private int GetLength2DLC(int dlc)
        {
            if (dlc <= 8)
                return dlc;
            switch (dlc)
            {
                case 12: return 9;
                case 16: return 10;
                case 20: return 11;
                case 24: return 12;
                case 32: return 13;
                case 48: return 14;
                case 64: return 15;
                default: return dlc;
            }
        }
        private Byte[] hearbeats = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        public Boolean Send(UInt32 protocolId, Byte[] data)
        {
            if (this.dataProtoValue == 0)//can
            {
                TPCANMsg canMsg = new TPCANMsg();
                canMsg.DATA = new Byte[data.Length];
                canMsg.ID = protocolId;
                canMsg.LEN = (Byte)data.Length;
                canMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
                Array.Copy(data, canMsg.DATA, data.Length);
                var status = PeakCanConnector.Write(channelHandle, ref canMsg);
                return status == TPCANStatus.PCAN_ERROR_OK;
            }
            else
            {
                TPCANMsgFD canMsg = new TPCANMsgFD();
                canMsg.DATA = new Byte[64];
                canMsg.ID = protocolId;
                canMsg.DLC = (Byte)GetLength2DLC(data.Length);
                canMsg.MSGTYPE = canMsg.DLC > 8 ? TPCANMessageType.PCAN_MESSAGE_FD : TPCANMessageType.PCAN_MESSAGE_STANDARD;//TPCANMessageType.PCAN_MESSAGE_STANDARD;
                Array.Copy(data, 0, canMsg.DATA, 0, data.Length);
                var status = PeakCanConnector.WriteFD(channelHandle, ref canMsg);
                return status == TPCANStatus.PCAN_ERROR_OK;
            }
        }

        public Boolean SendMuiltFrame(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            int len = 0;
            for (int i = 0; i < bs; i++)
            {
                if (!Send(protocolId, frames[offset + i].Data))
                {
                    return false;
                }
                Thread.SpinWait(500);
                len++;
            }
            return len == bs;
        }

        public Boolean GetOnlineState(ChannelConfig cc)
        {
            if (!this.IsInit) return true;
            var status = PeakCanConnector.GetStatus(channelHandle);
            if (status == TPCANStatus.PCAN_ERROR_OK || status == TPCANStatus.PCAN_ERROR_BUSOFF)
            {
                return true;
            }
            // 警告：总线重度干扰，检查接线、波特率，即将离线 PCAN_ERROR_BUSHEAVY
            //弹窗/状态栏提示：总线存在轻度干扰  PCAN_ERROR_BUSHEAVY
            return false;
        }

        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            if (!this.IsInit) return false;

            return true;
        }
    }
}
