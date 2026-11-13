using System;

namespace Rxbit.Brigade.CanDevices.Config
{
    public class FrameTimeConfig
    {
        public UInt64 FirstFrameTime { get; set; }
        public UInt64 LastFrameTime { get; set; }

        public string File { get; set; } = string.Empty;
    }
}
