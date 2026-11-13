using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.EcuServices.ZflashItems;
using System.Collections.Generic;

namespace Rxbit.Brigade.EcuServices.Models
{
    public class UdsMessage
    {
        public uint TxID { get; set; }

        public uint RxID { get; set; }
        public ChannelConfig Channel { get; set; }

        public Direction Direction { get; set; } = Direction.Rx;

        public byte[] Data { get; set; } = new byte[8];
        public List<byte> FlowData { get; set; }=new List<byte>();

        public int FlowIndex { get; set; }

        public int FlowCount { get; set; }

        public EcuMessage MsgType = EcuMessage.Nonamal;

        public override string ToString()
        {
            if (Direction == Direction.Rx)
            {
                return $"{RxID:X},{MsgType},{Direction}";
            }

            return $"{TxID:X},{MsgType},{Direction}";
        }

        public void ClearFlow()
        {
            FlowIndex = 0;
            FlowCount = 0;
            FlowData.Clear();
        }
    }
}
