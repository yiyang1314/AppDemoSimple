using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.Models.Models;

namespace Rxbit.BrigadeApp.Models
{
    public class BrigadeConfigBus : ConfigBus 
    {
        public SensorInfo Sensor { get; set; } = new SensorInfo() { Type = Brigade.Models.EnumEntitys.GenType.GenNoInit};

        public uint UdsTx { get; set; } = 0x793;

        public uint UdsRx { get; set; } = 0x79B;
        public AppPathInfo AppPath { get; set; }
        public BrigadeConfigBus()
        {
        }
    }
    public class AppPathInfo
    {
        public string RecodingPath { get; set; } = string.Empty;

        public string SettingPath { get; set; } = string.Empty;

        public string AppHexDefaultPath { get; set; } = string.Empty;

        public string LogsPath { get; set; } = string.Empty;

        public AppPathInfo() { }
    }
}
