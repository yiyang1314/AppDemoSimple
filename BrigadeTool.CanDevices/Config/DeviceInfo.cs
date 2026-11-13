using Rxbit.Brigade.CanDevices.Converters;
using System.ComponentModel;

namespace Rxbit.Brigade.CanDevices.Config
{
    public class DeviceInfo
    {
        public string SeriaNumber { get; set; }
        public string SoftwareVersion { get; set; }
        public string FirmwareVersion { get; set; }
        public string HardwareVersion { get; set; }

        public string SDKVersion { get; set; }
        public int CanCount { get; set; }
        public string HardwareType { get; set; }

        /**
        硬件版本: 3.01	Hardware Version: 3.01
        固件版本: 2.23	Firmware Version: 2.23
        驱动版本: 1.00	Driver Version: 1.00
        动态库版本: 0.00	Dynamic Library Version: 0.00
        CAN 路数: 2	CAN Channels: 2
        序列号: 7A41B5CA00460CF4F190 Serial Number: 7A41B5CA00460CF4F190
        硬件类型: USBCANFD-200U	Hardware Type: USBCANFD-200U
        */
    }

    /// <summary>
    /// 项目类型
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EcuRadar
    {
        [Description("前雷达公版")]
        AMFRRadar = 0,
        [Description("侧盲雷达公版")]
        BissRadar = 1,
        [Description("时代新安雷达")]
        SDXARadar = 2,
        [Description("角雷达公版")]
        AngleRadar = 3,
        [Description("壁虎角雷达")]
        BiHu_Radar = 4,
        [Description("蘑菇前雷达")]
        MG_Radar = 5,
        [Description("Brigade雷达")]
        Brigade_Radar = 7,
    }
}
