
using Rxbit.Brigade.CanDevices.Parsers;

namespace Rxbit.Brigade.CanDevices.Config
{
    public class DeviceConfig
    {
        public uint DeviceIndex { get; set; }

        public string Desc { get; set; }

        public DeviceCategory DCategory { get; set; }
    }
}
