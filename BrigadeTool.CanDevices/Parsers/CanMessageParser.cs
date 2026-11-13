using Rxbit.Brigade.CanDevices.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.Parsers
{
    public abstract class CanMessageParser
    {
        protected CanMessageParser(Int32 argc, params DeviceCategory[] dcs)
        {
            this.deviceCategories.Clear();
            if (dcs != null)
            {
                this.deviceCategories.AddRange(dcs);
            }
        }

        public virtual void Init(ConfigBus configBus)
        {
            this.channelIds.Clear();
            this.channelIds.Add(configBus.ChannelConfig.Id);
            this.needParser = this.channelIds.Count > 0;
        }

        public abstract void Parse();

        public void StartParser()
        {
            if (needParser && !isParsering)
            {
                isParsering = true;
                Task.Run(async () => { await MessageParserAsync(); });
            }
        }

        private async Task MessageParserAsync()
        {
            while (this.isParsering)
            {
                if (this.canMessages.IsEmpty)
                {
                    await Task.Delay(10);
                }
                else
                {
                    Parse();
                }
            }

            parserAre.Set();
        }

        public void StopParser()
        {
            if (isParsering)
            {
                parserAre.Reset();
                isParsering = false;
                parserAre.WaitOne(5000);
            }
        }

        public void Enqueue(CanMessageWrapper obj)
        {
            if (needParser && this.channelIds.Contains(obj.ChannelConfig.Id))
            {
                this.canMessages.Enqueue(obj);
            }
        }

        private readonly AutoResetEvent parserAre = new AutoResetEvent(false);
        private Boolean isParsering = false;
        private Boolean needParser = false;
        private readonly List<UInt32> channelIds = new List<UInt32>();
        private readonly List<DeviceCategory> deviceCategories = new List<DeviceCategory>();
        protected readonly ConcurrentQueue<CanMessageWrapper> canMessages = new ConcurrentQueue<CanMessageWrapper>();
    }
}
