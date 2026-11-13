using Kvaser.CanLib;
using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.Services
{
    public class KvaserChannelService : ICanChannelService
    {
        public ChannelConfig ChannelConfig { get; private set; }
        private Int32 channelHandle;
        private Byte dataProtoValue;
        public byte CanType { get; set; }
        public bool UseCanfd { get; set; }
        public Int64 Id { get { return this.ChannelConfig.Id; } }
        public DateTime BeginRecordDt { get; set; }
        Canlib.canStatus status = Canlib.canStatus.canOK;
        private volatile bool IsInit;

        /// <summary>
        /// 仲裁波特率
        /// </summary>
        private static readonly Dictionary<ChannelBaudRate, int> ArbitrationBaudRates =
            new Dictionary<ChannelBaudRate, int>();
        /// <summary>
        /// 数据波特率
        /// </summary>
        private static readonly Dictionary<ChannelDataBaudRate, int> DataBaudRates =
            new Dictionary<ChannelDataBaudRate, int>();

        static KvaserChannelService()
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
            ArbitrationBaudRates.Add(ChannelBaudRate._10Kbps, Canlib.canBITRATE_10K);
            ArbitrationBaudRates.Add(ChannelBaudRate._50Kbps, Canlib.canBITRATE_50K);
            ArbitrationBaudRates.Add(ChannelBaudRate._100Kbps, Canlib.canBITRATE_100K);
            ArbitrationBaudRates.Add(ChannelBaudRate._125Kbps, Canlib.canBITRATE_125K);
            ArbitrationBaudRates.Add(ChannelBaudRate._250Kbps, Canlib.canBITRATE_250K);
            ArbitrationBaudRates.Add(ChannelBaudRate._500Kbps, Canlib.canBITRATE_500K);
            ArbitrationBaudRates.Add(ChannelBaudRate._1000Kbps, Canlib.canBITRATE_1M);
        }
        /// <summary>
        /// 数据域波特率，除5m采样率为75%外，其余采样率均为80%
        /// </summary>
        private static void InitDataBaudRate()
        {
            DataBaudRates.Clear();
            DataBaudRates.Add(ChannelDataBaudRate.None, Canlib.canFD_BITRATE_1M_80P);
            DataBaudRates.Add(ChannelDataBaudRate._1000Kbps, Canlib.canFD_BITRATE_1M_80P);
            DataBaudRates.Add(ChannelDataBaudRate._2000Kbps, Canlib.canFD_BITRATE_2M_60P);
            DataBaudRates.Add(ChannelDataBaudRate._4000Kbps, Canlib.canFD_BITRATE_4M_80P);
            DataBaudRates.Add(ChannelDataBaudRate._5000Kbps, Canlib.canFD_BITRATE_8M_60P);
        }
        public Boolean Close()
        {
            tokenSource?.Cancel();
            Task.Delay(1000).Wait();
            status = Canlib.canBusOff(channelHandle);
            status = Canlib.canClose(channelHandle);
            this.IsInit = false;
            return status == Canlib.canStatus.canOK;
        }
        private int Canlen = 8;
        public Boolean Open(ChannelConfig ccc)
        {
            ChannelConfig = ccc;
            this.dataProtoValue = (Byte)(ccc.DataProto == DataProtocol.CANFD ? 1 : 0);
            int flag = Canlib.canOPEN_ACCEPT_VIRTUAL;
            if (this.dataProtoValue == 1)//CAN
            {
                flag = Canlib.canOPEN_CAN_FD;
            }
            Canlib.canInitializeLibrary();
            DeviceInfo device = new DeviceInfo();
            GetDeviceInfo(ccc, ref device);
            this.channelHandle = Canlib.canOpenChannel(ccc.ChannelIndex, flag);
            if (this.channelHandle <0) 
            {
                Close();
                CheckStatus((Canlib.canStatus)channelHandle, "OpenChannel");
                return false;
            }

            int baute = ArbitrationBaudRates[ccc.BaudRate];
            status = Canlib.canSetBusParams(channelHandle, baute, 0, 0, 0, 0);
            if (!CheckStatus(status, "OpenChannel")) return false;
            if (this.dataProtoValue != 0)
            {
                int databaute = DataBaudRates[ccc.DataBaudRate];
                status = Canlib.canSetBusParamsFd(channelHandle, databaute, 0, 0, 0);
            }
            if (!CheckStatus(status, "OpenChannel")) return false;
            Canlen = ChannelConfig.DataProto == DataProtocol.CAN ? 8 : 64;
            status = Canlib.canBusOn(channelHandle);
            IsInit = CheckStatus(status, "OpenChannel");
            return IsInit;
        }
        private CancellationTokenSource tokenSource;
        public void RecvitAsync()
        {
            tokenSource = new CancellationTokenSource();
            Task.Run(() => LoopMessgae(this.channelHandle, true, tokenSource), tokenSource.Token);

        }
        private void LoopMessgae(int channecl, bool Canfd, CancellationTokenSource tokenSource)
        {
            byte[] msg = new byte[64];
            string EX = "";
            Canlib.canStatus status = Canlib.canStatus.canOK;
            Console.WriteLine("----------------------RX Message-------------------------");
            while (!tokenSource.IsCancellationRequested)
            {
                status = Canlib.canReadWait(channecl, out int CanID, msg, out int dlc, out int flag, out long time, 50);
                if (!CheckStatus(status, "canReadWait") || dlc == 0)
                {
                    //Console.WriteLine($"rec Message channelHandle:{channecl} ,channel: {ChannelConfig.ChannelIndex}, status； {status}" );
                    continue;
                }

                bool IsExTended = (flag & Canlib.canMSG_EXT) != 0;
                bool IsCanfd = (flag & Canlib.canFDMSG_FDF) != 0;
                int BRS = (flag & Canlib.canFDMSG_FDF);
                bool IsRemote = (flag & Canlib.canMSG_RTR) != 0;
                EX = IsExTended ? "x" : "";
                byte[] data = new byte[dlc];
                Buffer.BlockCopy(msg, 0, data, 0, dlc);
                if (IsCanfd)
                {
                    Console.WriteLine($"{time} CANFD {ChannelConfig.ChannelIndex} {CanID:x}{EX} Rx 0 0 d {CanTool.GetCanFdDlc(dlc)} {dlc} {BitConverter.ToString(data)}");
                }
                else
                {
                    Console.WriteLine($"{time} {ChannelConfig.ChannelIndex} {CanID:x}{EX} Rx d {dlc} {BitConverter.ToString(data)}");
                }
            }
        }

        public List<RXCanMessage> Read()
        {
            List<RXCanMessage> datas = new List<RXCanMessage>();
            byte[] msg = new byte[Canlen];
            status = Canlib.canReadWait(channelHandle, out int CanID, msg, out int dlc, out int flag, out long time, 50);
            if (status != Canlib.canStatus.canOK || status == Canlib.canStatus.canERR_NOMSG)
            {
                return datas;
            }

            bool IsExTended = (flag & Canlib.canMSG_EXT) != 0;
            bool IsCanfd = (flag & Canlib.canFDMSG_FDF) != 0;
            int BRS = (flag & Canlib.canFDMSG_FDF);
            bool IsRemote = (flag & Canlib.canMSG_RTR) != 0;
            var message = new RXCanMessage()
            {
                DLC = (byte)dlc,
                TIMESTAMP = (ulong)time,
                ID = (uint)CanID,
                IsExdFrame = IsExTended,
                DATA = new Byte[dlc > 8 ? 64 : 8],
            };
            Array.Copy(msg, 0, message.DATA, 0, dlc);
            datas.Add(message);
            return datas;
        }

        public Boolean Send(UInt32 protocolId, Byte[] data)
        {

            int len = CanTool.GetCanFdValidLength(data.Length);
            byte[] msg = new byte[len];
            Array.Copy(data, 0, msg, 0, data.Length);
            int flag = 0;
            if (len > 8)//can
            {
                flag |= Canlib.canFDMSG_FDF;
            }
            status = Canlib.canWrite(this.channelHandle, (int)protocolId, msg, len, flag);
            return status == Canlib.canStatus.canOK;
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
            if (!CheckStatus(status, "GetOnlineState")) return false;
            DeviceInfo device = new DeviceInfo();
            GetDeviceInfo(cc, ref device);
            return true;
        }

        public bool GetDeviceInfo(ChannelConfig cc, ref DeviceInfo device)
        {
            var stat = Canlib.canGetChannelData(cc.ChannelIndex, Canlib.canCHANNELDATA_DEVDESCR_ASCII, out object device_name);
            if (CheckForError("GetDeviceInfo", stat)) return false;
            device.SeriaNumber = device_name.ToString();
            stat = Canlib.canGetChannelData(cc.ChannelIndex, Canlib.canCHANNELDATA_CHAN_NO_ON_CARD, out object device_channel);
            if (CheckForError("GetDeviceInfo", stat)) return false;
            int channel = (int)((uint)device_channel + 1);
            device.CanCount = channel;
            return true;
        }

        private bool CheckStatus(Canlib.canStatus status, string method)
        {
            if (status != Canlib.canStatus.canOK && status != Canlib.canStatus.canERR_NOMSG)
            {
                string errorText;
                Canlib.canGetErrorText(status, out errorText);
                return false;
            }
            return true;
        }

        private bool CheckForError(string cmd, Canlib.canStatus stat)
        {
            if (stat != Canlib.canStatus.canOK)
            {
                Canlib.canGetErrorText(stat, out string buf);

                Console.WriteLine("[{0}] {1}: failed, stat={2}", cmd, buf, (int)stat);
                return true;
            }
            return false;
        }

        public static List<DeviceInfo> FindDevices()
        {
            Canlib.canInitializeLibrary();
            List<DeviceInfo> list = new List<DeviceInfo>();
            var stat = Canlib.canGetNumberOfChannels(out int number_of_channels);
            for (int i = 0; i < number_of_channels; i++)
            {
                stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_DEVDESCR_ASCII, out object device_name);
                if (stat != Canlib.canStatus.canOK)
                    return list;

                stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CHAN_NO_ON_CARD, out object device_channel);
                if (stat != Canlib.canStatus.canOK)
                    return list;
                UInt32 cc = (UInt32)device_channel + 1;
                list.Add(new DeviceInfo()
                {
                    SeriaNumber = device_name.ToString(),
                    CanCount = (int)cc,
                });
                string devin = string.Format("Found channel: {0} {1} {2}", i, device_name, cc);
            }
            return list;
        }
    }
}
