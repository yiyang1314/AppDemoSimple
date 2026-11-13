using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Parsers;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.IServices
{
    public class CanLiveService : ILiveService, IRecordingService
    {
        public event Action<CanMessageWrapper> CanDataComed;
        private readonly CanCommService canCommService;

        public CanLiveService(CanCommService ccs)
        {
            this.canCommService = ccs;
            this.canCommService.CanDataComed += CanCommService_CanDataComed;
            this.canCommService.CanDataSent += CanCommService_CanDataSent;
            this.CanDataComed += canMessageRecorder.Enqueue;
        }

        public bool AddParseService(CanMessageParser parser)
        {
            if (!this.cmsgParsers.Contains(parser)) 
            {
                this.CanDataComed += parser.Enqueue;
                this.cmsgParsers.Add(parser);
            }
            return true;
        }
        public bool RemoveParseService(CanMessageParser parser)
        {
            if (this.cmsgParsers.Contains(parser))
            {
                this.cmsgParsers.Remove(parser);
            }
            return true;
        }

        private void CanCommService_CanDataComed(ICanChannelService sender, List<RXCanMessage> objs)
        {
            var messages = new CanMessageWrapper()
            {
                ChannelConfig = sender.ChannelConfig,
                Messages = objs,
                BeginDt = sender.BeginRecordDt,
                ReceiveDt = DateTime.Now,
                direction = Direction.Rx,
            };

            Parallel.ForEach(this.CanDataComed.GetInvocationList(), notify =>
            {
                notify.DynamicInvoke(messages);
            });
        }
        private void CanCommService_CanDataSent(ICanChannelService sender, List<RXCanMessage> objs)
        {
            var messages = new CanMessageWrapper()
            {
                ChannelConfig = sender.ChannelConfig,
                Messages = objs,
                BeginDt = sender.BeginRecordDt,
                ReceiveDt = DateTime.Now,
                direction = Direction.Tx,
            };

            Parallel.ForEach(this.CanDataComed.GetInvocationList(), notify =>
            {
                notify.DynamicInvoke(messages);
            });
        }
        public void Init(ConfigBus configBus)
        {
            Parallel.ForEach(this.cmsgParsers, p =>
            {
                p.Init(configBus);
            });
        }

        public void StartReceive(int deviceIds)
        {
            Parallel.ForEach(this.cmsgParsers, p =>
            {
                p.StartParser();
            });
        }

        public void StopReceive(int deviceIds)
        {
            Parallel.ForEach(this.cmsgParsers, p =>
            {
                p.StopParser();
            });
        }

        public Boolean StartRecording(String directory)
        {
            canMessageRecorder.StartRecording(directory);
            return true;
        }

        public Boolean StopRecording()
        {
            canMessageRecorder.StopRecording();
            return true;
        }

        private readonly CanMessageRecorder canMessageRecorder = new CanMessageRecorder();
        private readonly List<CanMessageParser> cmsgParsers = new List<CanMessageParser>();
    }
}
