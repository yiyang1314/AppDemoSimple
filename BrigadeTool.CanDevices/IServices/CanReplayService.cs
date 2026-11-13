using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Parsers;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.IServices
{
    public class CanReplayService : IReplayService
    {
        public FrameTimeConfig FrameConfig { get; set; }
        public FrameTimeConfig Init(String directory, ConfigBus configBus)
        {
            this.channelMessages.Clear();
            _sortedTimestamps = null;
            FrameConfig = null;
            String fileName = Path.Combine(Path.GetDirectoryName(directory), UniformFileNaming.GetFileName(configBus.ChannelConfig));
            if (configBus != null && File.Exists(directory))
            {
                Dictionary<String, ChannelConfig> fncs = new Dictionary<String, ChannelConfig>();
                if (!fncs.ContainsKey(fileName))
                {
                    fncs.Add(fileName, configBus.ChannelConfig);
                }

                Parallel.ForEach(fncs, (fnc) =>
                {
                    String filePath = fnc.Key;
                    if (File.Exists(filePath))
                    {
                        var messages = new ConcurrentDictionary<UInt64, List<RXCanMessage>>();
                        this.channelMessages.TryAdd(fnc.Value.Id, (fnc.Value, messages));
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            String line;
                            while (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                if (!String.IsNullOrEmpty(line))
                                {
                                    if (line.Contains("CANFD "))
                                    {
                                        line = line.Replace("CANFD ", "");
                                    }
                                    if (!line.Contains("Rx d"))
                                    {
                                        string pattern = @"Rx \d{1,2} \d{1,2} d \d{1,2}";
                                        line = Regex.Replace(line, pattern, "Rx d");
                                    }
                                    var fields = line.Trim().Split(' ');
                                    var len = fields.Length - 6;
                                    if (len == 8 || len == 64)
                                    {
                                        Double dtimestamp;
                                        if (Double.TryParse(fields[0], out dtimestamp))
                                        {
                                            var timestamp = ((UInt64)(dtimestamp * 1000) / 10) * 10;
                                            string s = fields[2];
                                            if (fields[2].EndsWith("x"))
                                            {
                                                s = s.Substring(0, fields[2].Length - 1);
                                            }
                                            RXCanMessage message = new RXCanMessage()
                                            {
                                                ID = Convert.ToUInt32(s, 16),
                                                DLC = Convert.ToByte(fields[5], 16),
                                                DATA = new Byte[len],
                                                TIMESTAMP = timestamp
                                            };
                                            for (int i = 0; i < len; i++)
                                            {
                                                message.DATA[i] = Convert.ToByte(fields[6 + i], 16);
                                            }
                                            if (!messages.ContainsKey(timestamp))
                                            {
                                                messages.TryAdd(timestamp, new List<RXCanMessage>());
                                            }
                                            messages[timestamp].Add(message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            if (this.channelMessages.Count > 0)
            {
                List<UInt64> minTimes = new List<UInt64>();
                List<UInt64> maxTimes = new List<UInt64>();
                foreach (var item in this.channelMessages)
                {
                    if (item.Value.messages.Count > 0)
                    {
                        minTimes.Add(item.Value.messages.Keys.Min());
                        maxTimes.Add(item.Value.messages.Keys.Max());
                    }
                }
                if (minTimes.Count > 0)
                {
                    Parallel.ForEach(this.cmsgParsers, p =>
                    {
                        p.Init(configBus);
                    });
                    Start();
                    FrameConfig= new FrameTimeConfig()
                    {
                        FirstFrameTime = minTimes.Min(),
                        LastFrameTime = maxTimes.Max(),
                        File = fileName
                    };
                    return FrameConfig;
                }
            }
            return null;
        }

        public void GetFrame(UInt64 timestamp)
        {
            Parallel.ForEach(this.channelMessages, cmsg =>
            {
                if (cmsg.Value.messages.ContainsKey(timestamp))
                {
                    var message = cmsg.Value.messages[timestamp];
                    Parallel.ForEach(this.CanDataComed.GetInvocationList(), p =>
                    {
                        p.DynamicInvoke(new CanMessageWrapper()
                        {
                            Timestamp = timestamp,
                            ChannelConfig = cmsg.Value.channelConfig,
                            Messages = message,
                        });
                    });
                }
            });
        }

        public void Start()
        {
            Parallel.ForEach(this.cmsgParsers, p =>
            {
                p.StartParser();
            });
        }

        public void Stop()
        {
            Parallel.ForEach(this.cmsgParsers, p =>
            {
                p.StopParser();
            });
        }

        public CanReplayService()
        {
            
        }

        private List<UInt64> _sortedTimestamps;

        public List<UInt64> GetAvailableTimestamps()
        {
            if (_sortedTimestamps != null) return _sortedTimestamps;

            var allTimestamps = new HashSet<UInt64>();
            foreach (var item in this.channelMessages)
            {
                foreach (var key in item.Value.messages.Keys)
                {
                    allTimestamps.Add(key);
                }
            }
            _sortedTimestamps = allTimestamps.OrderBy(t => t).ToList();
            return _sortedTimestamps;
        }

        public UInt64 GetNextTimestamp(UInt64 currentTimestamp)
        {
            var timestamps = GetAvailableTimestamps();
            for (int i = 0; i < timestamps.Count; i++)
            {
                if (timestamps[i] > currentTimestamp)
                    return timestamps[i];
            }
            return currentTimestamp;
        }

        public UInt64 GetPrevTimestamp(UInt64 currentTimestamp)
        {
            var timestamps = GetAvailableTimestamps();
            for (int i = timestamps.Count - 1; i >= 0; i--)
            {
                if (timestamps[i] < currentTimestamp)
                    return timestamps[i];
            }
            return currentTimestamp;
        }

        public bool HasTimestamp(UInt64 timestamp)
        {
            foreach (var item in this.channelMessages)
            {
                if (item.Value.messages.ContainsKey(timestamp))
                    return true;
            }
            return false;
        }

        public bool AddParseService(CanMessageParser parser)
        {
            if (!this.cmsgParsers.Contains(parser))
            {
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

        public event Action<CanMessageWrapper> CanDataComed;
        private ConcurrentDictionary<Int64, (ChannelConfig channelConfig, ConcurrentDictionary<UInt64, List<RXCanMessage>> messages)> channelMessages =
            new ConcurrentDictionary<Int64, (ChannelConfig channelConfig, ConcurrentDictionary<UInt64, List<RXCanMessage>> messages)>();
        private readonly List<CanMessageParser> cmsgParsers = new List<CanMessageParser>();
    }
}
