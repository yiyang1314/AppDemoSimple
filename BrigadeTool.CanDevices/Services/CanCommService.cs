using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.Services
{
    public class CanCommService
    {
        public event Action<ChannelConfig> ChannelDisconned;
        public event Action<ICanChannelService, List<RXCanMessage>> CanDataComed;
        public event Action<ICanChannelService, List<RXCanMessage>> CanDataSent;
        public ulong LastRecCanFrameTIMESTAMP = 0;

        public bool IsOpen { get; set; }
        public ChannelConfig CurrChannelConfig { get; set; }


        public bool Open() 
        {
            if (IsOpen && CurrChannelConfig != null) CloseAll();
            else 
            {
                if (CurrChannelConfig != null) Open(CurrChannelConfig);
            }
            return IsOpen;
        }
        public Boolean Open(ChannelConfig ccc)
        {
            LastRecCanFrameTIMESTAMP = 0;
            Boolean result;
            if (OpenedChannels.ContainsKey(ccc.Id))
            {
                result = true;
            }
            else
            {
                ICanChannelService channelService;
                switch (ccc.CCategroy)
                {
                    case CanCategory.ZLG_USBCAN_I:
                    case CanCategory.ZLG_USBCAN_II:
                    case CanCategory.ZLG_USBCAN_2EU:
                    case CanCategory.ZLG_USBCANFD_100U:
                    case CanCategory.ZLG_USBCANFD_200U:
                        {
                            channelService = new ZlgCanChannelService();
                        }
                        break;
                    case CanCategory.PEAK_CAN:
                        {
                            channelService = new PeakCanChannelService();
                        }
                        break;
                    case CanCategory.GCAN_USBCAN_V4:
                    case CanCategory.GCAN_USBCAN_V5:
                        {
                            channelService = new GCanChannelService();
                        }
                        break;
                    case CanCategory.GCAN_USBCAN_FD:
                        {
                            channelService = new GcCanfdChannelService();
                        }
                        break;
                    case CanCategory.CXKJ_CANalyst_II:
                        {
                            channelService = new CxkjCanChannelService();
                        }
                        break;
                    case CanCategory.CXKJ_USBCANFD:
                        {
                            channelService = new CxkjCanFDChannelService();
                        }
                        break;
                    case CanCategory.Kvaser_USBCAN:
                        {
                            channelService = new KvaserChannelService();
                        }
                        break;
                    default:
                        channelService = null;
                        break;
                }

                if (channelService != null && channelService.Open(ccc))
                {
                    result = OpenedChannels.TryAdd(ccc.Id, channelService);
                }
                else
                {
                    result = false;
                }
            }

            if (result && !isReceiving)
            {
                isReceiving = true;
                Task.Run(async () => { await ReceiveCanFrame(); });
                CurrChannelConfig = ccc;
            }
            IsOpen = result;
            return result;
        }

        public Boolean Close(UInt32 id)
        {
            ICanChannelService channelService;
            if (OpenedChannels.TryRemove(id, out channelService))
            {
                channelService.Close();
            }
            if (isReceiving &&
                OpenedChannels.Count == 0)
            {
                stopReceiveAre.Reset();
                isReceiving = false;
                stopReceiveAre.WaitOne(5000);
            }
            IsOpen = false;
            return true;
        }

        public async Task ReceiveCanFrame()
        {
            Boolean readedCanFrame;
            while (isReceiving)
            {
                readedCanFrame = false;
                Parallel.ForEach(OpenedChannels.Values, channel =>
                {
                    var data = channel.Read();
                    if (data.Count > 0)
                    {
                        readedCanFrame = true;
                        LastRecCanFrameTIMESTAMP = data.Last().TIMESTAMP;
                        this.CanDataComed?.Invoke(channel, data);
                    }
                    else
                    {
                        var result = channel.GetOnlineState(channel.ChannelConfig);
                        if (!result)
                        {
                            channel.Close();
                            OpenedChannels.TryRemove(channel.ChannelConfig.Id, out var cc);
                            this.ChannelDisconned.Invoke(channel.ChannelConfig);
                        }
                    }
                });

                if (!readedCanFrame)
                {
                    await Task.Delay(10);
                }
            }

            this.stopReceiveAre.Set();
        }

        public void CloseAll()
        {
            foreach (var channelKey in OpenedChannels.Keys)
            {
                ICanChannelService channelService;
                if (OpenedChannels.TryRemove(channelKey, out channelService))
                {
                    channelService.Close();
                }
            }

            if (isReceiving)
            {
                stopReceiveAre.Reset();
                isReceiving = false;
                stopReceiveAre.WaitOne(5000);
            }
        }

        public Boolean Send(ChannelConfig ccc, UInt32 protocolId, Byte[] data)
        {
            ICanChannelService channelService;
            if (OpenedChannels.TryGetValue(ccc.Id, out channelService))
            {
                    bool result = channelService.Send(protocolId, data);
                if (result)
                {
                    List<RXCanMessage> datas = new List<RXCanMessage>();
                    RXCanMessage sendFrame = new RXCanMessage()
                    {
                        ID = protocolId,
                        DLC = (byte)data.Length,
                        TIMESTAMP = LastRecCanFrameTIMESTAMP,
                        DATA = data,
                    };
                    Array.Copy(data, 0, sendFrame.DATA, 0, data.Length);
                    datas.Add(sendFrame);
                    this.CanDataSent?.Invoke(channelService, datas);
                }
                return result;
            }
            else
            {
                return false;
            }
        }

        public void SetCantype(ChannelConfig ccc, byte cantype, bool useCanfd = false)
        {
            if (OpenedChannels.Count == 0 || ccc == null) return;
            ICanChannelService channelService;
            if (OpenedChannels.TryGetValue(ccc.Id, out channelService))
            {
                channelService.CanType = cantype;
                channelService.UseCanfd = useCanfd;
            }
        }
        public Boolean SendMuiltFrame(ChannelConfig ccc, UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true)
        {
            ICanChannelService channelService;
            if (OpenedChannels.TryGetValue(ccc.Id, out channelService))
            {
                List<RXCanMessage> datas = new List<RXCanMessage>();
                for (int i = 0; i < bs && (i + offset) < frames.Count; i++)
                {
                    var frame = frames[i + offset];
                    RXCanMessage sendFrame = new RXCanMessage()
                    {
                        ID = IsContius ? protocolId : frame.CanID,
                        DLC = (byte)frame.Data.Length,
                        TIMESTAMP = LastRecCanFrameTIMESTAMP,
                        DATA = frame.Data,

                    };
                    Array.Copy(frame.Data, 0, sendFrame.DATA, 0, frame.Data.Length);
                    datas.Add(sendFrame);
                }
                bool result = channelService.SendMuiltFrame(protocolId, frames, offset, bs, IsContius);
                if (!result)
                {
                    this.CanDataSent?.Invoke(channelService, datas);
                }
                return result;
            }
            else
            {
                return false;
            }
        }
        private Boolean isReceiving = false;
        private AutoResetEvent stopReceiveAre = new AutoResetEvent(false);
        public static readonly ConcurrentDictionary<UInt32, ICanChannelService> OpenedChannels =
            new ConcurrentDictionary<UInt32, ICanChannelService>();
    }
}
