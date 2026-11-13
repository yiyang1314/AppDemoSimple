using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Rxbit.Brigade.CanDevices.Config
{
    public class CanMessageWrapper
    {
        public ChannelConfig ChannelConfig { get; set; }
        /// <summary>
        /// 开始录制时间
        /// </summary>
        public DateTime BeginDt { get; set; }
        /// <summary>
        /// 接收数据时间
        /// </summary>
        public DateTime ReceiveDt { get; set; }
        public Double Timestamp { get; set; }
        /// <summary>
        /// 数据方向
        /// </summary>
        public Direction direction { get; set; } = Direction.Rx;
        public List<RXCanMessage> Messages { get; set; }
    }

    public enum Direction
    {
        Rx,
        Tx,
    }
}
