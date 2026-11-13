using CommunityToolkit.Mvvm.ComponentModel;
using Rxbit.Brigade.CanDevices.Config;
using System;
namespace Rxbit.BrigadeApp.Models
{
    public class CanMessage: ObservableObject
    {
        public DateTime Time { get; set; }
        public Direction Direction { get; set; }
        public UInt32 CanId { get; set; }
        public int Dlc { get; set; }
        public string Data { get; set; }
        public string Message { get; set; }
    }
}
