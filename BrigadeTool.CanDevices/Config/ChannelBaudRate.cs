using Rxbit.Brigade.CanDevices.Converters;
using System;
using System.ComponentModel;

namespace Rxbit.Brigade.CanDevices.Config
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ChannelBaudRate : UInt32
    {
        [Description("10K")]
        _10Kbps = 10,
        [Description("50K")]
        _50Kbps = 50,
        [Description("100K")]
        _100Kbps = 100,
        [Description("125K")]
        _125Kbps = 125,
        [Description("250k")]
        _250Kbps = 250,
        [Description("500k")]
        _500Kbps = 500,
        [Description("1M")]
        _1000Kbps = 1000,
    };

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ChannelDataBaudRate : UInt32
    {
        [Description("None")]
        None = 0,
        [Description("1M")]
        _1000Kbps = 1000,
        [Description("2M")]
        _2000Kbps = 2000,
        [Description("4M")]
        _4000Kbps = 4000,
        [Description("5M")]
        _5000Kbps = 5000,
    };

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DataProtocol
    {
        [Description("CAN")]
        CAN,
        [Description("CANFD")]
        CANFD
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ChannelResistanceEnable : UInt32
    {
        [Description("Disable")]
        Disable = 0,
        [Description("Enable")]
        Enable = 1,
    };
}
