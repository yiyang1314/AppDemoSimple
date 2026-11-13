using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.CanDevices.Parsers
{
    public class CanMessageRecorder
    {
        public void Enqueue(CanMessageWrapper data)
        {
            if (this.isRecording)
            {
                this.canFrames.Enqueue(data);
            }
        }
        public Boolean StartRecording(String directory)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            StopRecording();
            if (!this.isRecording)
            {
                DateTime dt = DateTime.Now;
                foreach (var item in CanCommService.OpenedChannels.Values)
                {
                    item.BeginRecordDt = dt;
                    var filePath = Path.Combine(directory, UniformFileNaming.GetFileName(new ChannelConfig()
                    {
                        CCategroy = item.ChannelConfig.CCategroy,
                        CanIndex = item.ChannelConfig.CanIndex,
                        ChannelIndex = item.ChannelConfig.ChannelIndex
                    }));
                    var writer = new StreamWriter(filePath);
                    var item1 = (writer, GetUnit(item.ChannelConfig.CCategroy));
                    this.fsWriters.TryAdd(item.Id, item1);
                    writer.WriteLine($"date {dt.ToString("ddd MMM dd hh:mm:ss tt yyyy", cultureInfo)}");
                    writer.WriteLine("base hex timestamps absolute");
                    writer.WriteLine("// version 7.0.0");
                }
                this.isRecording = true;
                Task.Run(async () => { await WriteMessage(); });
            }

            return true;
        }
        private UInt64 GetUnit(CanCategory category)
        {
            if (category == CanCategory.CXKJ_CANalyst_II)
            {
                return 10000;
            }
            else if (category == CanCategory.Kvaser_USBCAN)
            {
                return 1000;
            }
            else
            {
                return 1000000;
            }
        }
        private async Task WriteMessage()
        {
            UInt64 mFirstTimeSpan = 0;
            Double Timestamp = 0;
            while (isRecording)
            {
                if (this.canFrames.IsEmpty)
                {
                    await Task.Delay(10);
                }
                else
                {
                    while (!this.canFrames.IsEmpty)
                    {
                        CanMessageWrapper canFrame;
                        if (this.canFrames.TryDequeue(out canFrame))
                        {
                            if (canFrame.ReceiveDt >= canFrame.BeginDt)
                            {
                                if (mFirstTimeSpan == 0)
                                {
                                    mFirstTimeSpan = canFrame.Messages.First().TIMESTAMP;
                                }
                                if (this.fsWriters.TryGetValue(canFrame.ChannelConfig.Id, out (StreamWriter, UInt64) streamItems))
                                {
                                    StreamWriter writer = streamItems.Item1;
                                    foreach (var data in canFrame.Messages)
                                    {
                                        if (data.TIMESTAMP > mFirstTimeSpan)
                                        {
                                            Timestamp = 1.0 * (data.TIMESTAMP - mFirstTimeSpan) / streamItems.Item2;
                                        }
                                        if (canFrame.ChannelConfig.CCategroy == CanCategory.GCAN_USBCAN_V4
                                            || canFrame.ChannelConfig.CCategroy == CanCategory.GCAN_USBCAN_V5)
                                        {
                                            Timestamp = (canFrame.ReceiveDt - canFrame.BeginDt).TotalSeconds;
                                        }
                                        byte ChannelIndex = canFrame.ChannelConfig.ChannelIndex;
                                        ChannelIndex += 1;
                                        string frameIdex = data.IsExdFrame ? "x" : "";
                                        if ((canFrame.ChannelConfig.DataProto == DataProtocol.CANFD || data.DLC > 8))
                                        {
                                            writer.WriteLine($"{Timestamp:F6} CANFD {ChannelIndex} {data.ID.ToString("X")}{frameIdex} {canFrame.direction} 0 0 d {DataConverter.DataLen2DLC(data.DLC)} " +
                                           $"{data.DLC} {BitConverter.ToString(data.DATA, 0).Replace("-", " ")}");
                                        }
                                        else
                                        {
                                            writer.WriteLine($"{Timestamp:F6} {ChannelIndex} {data.ID.ToString("X")}{frameIdex} {canFrame.direction} d " +
                                            $"{data.DLC} {BitConverter.ToString(data.DATA, 0).Replace("-", " ")}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.stopRecordingAre.Set();
        }

        public Boolean StopRecording()
        {
            if (this.isRecording)
            {
                this.stopRecordingAre.Reset();

                this.isRecording = false;
                this.stopRecordingAre.WaitOne(5000);
                foreach (var item in this.fsWriters)
                {
                    item.Value.Item1.WriteLine("End Triggerblock");
                    item.Value.Item1.Close();
                }
                this.fsWriters.Clear();
            }
            ClearMessage();
            return true;
        }

        private void ClearMessage()
        {
            while (!canFrames.IsEmpty)
            {
                canFrames.TryDequeue(out var frame);
            }
        }

        private readonly AutoResetEvent stopRecordingAre = new AutoResetEvent(false);
        private ConcurrentQueue<CanMessageWrapper> canFrames = new ConcurrentQueue<CanMessageWrapper>();
        private readonly CultureInfo cultureInfo = new CultureInfo("en-US");
        private Boolean isRecording = false;
        private readonly ConcurrentDictionary<Int64, (StreamWriter, UInt64)> fsWriters = new ConcurrentDictionary<Int64, (StreamWriter, UInt64)>();
    }

    public class DataConverter
    {
        public static uint Length2DLC(uint length)
        {
            int dlc = 0;
            dlc = DLCList.IndexOf(length);
            return dlc < 0 ? 0 : (uint)dlc;
        }
        static List<uint> DLCList = new List<uint>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 12, 16, 20, 24, 32, 48, 64 };

        public static byte DataLen2DLC(int length)
        {
            var inde = 0;

            if (length >= DLCList[DLCList.Count - 1])
            {
                inde = 15;
            }

            for (int i = 0; i < DLCList.Count - 1; i++)
            {
                if (length > DLCList[i] && length < DLCList[i + 1])
                {
                    inde = i + 1;
                    break;
                }
                else if (length == DLCList[i])
                {
                    inde = i;
                }
            }

            return (byte)inde;
        }
    }
}
