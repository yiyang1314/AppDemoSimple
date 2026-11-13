using Rxbit.Brigade.CanDevices.Converters;
using System.ComponentModel;

namespace Rxbit.Brigade.CanDevices.Parsers
{
    /// <summary>
    /// 设备类型
    /// </summary>
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DeviceCategory
    {
        Vehicle = 6,
        ARS408 = 11,
        ARS410 = 20,
        ZKHY403 = 21,
        TC404 = 25,
        MG403 = 28,
        BSIS403T3 = 39,
        QZ_TC404 = 40,
        KM_BSIS403 = 41,
        SQ_BSIS = 42,
        SQ404T1 = 43,
        Brigade = 44,
        ZT_BSIS = 45,
    }
}
