using Rxbit.Brigade.CanDevices.Structs;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rxbit.Brigade.CanDevices.Config
{
    /// <summary>
    /// CAN 设备通道配置信息
    /// </summary>
    public class ChannelConfig
    {
        /// <summary>
        /// CAN 设备通道唯一 ID
        /// </summary>
        public UInt32 Id { get; set; }

        /// <summary>
        /// CAN 设备类型，具体见 CanCategory 枚举
        /// </summary>
        public CanCategory CCategroy { get; set; } = CanCategory.ZLG_USBCAN_II;

        /// <summary>
        /// CAN 设备索引,根据 CCategroy 判断 CAN 设备索引是否有效
        /// 例如：ZLG_USBCAN_2 类型设备，CanIndex 有效；
        /// PEAK_USBCAN 类型设备，则 CanIndex 无效，仅关注 ChannelIndex 即可。
        /// </summary>
        public Byte CanIndex { get; set; }

        /// <summary>
        /// 通道索引
        /// </summary>
        public Byte ChannelIndex { get; set; }

        /// <summary>
        /// 数据协议
        /// </summary>
        public DataProtocol DataProto { get; set; } = DataProtocol.CAN;

        /// <summary>
        /// 通道波特率
        /// </summary>
        public ChannelBaudRate BaudRate { get; set; } = ChannelBaudRate._1000Kbps;

        /// <summary>
        /// 数据波特率
        /// </summary>
        public ChannelDataBaudRate DataBaudRate { get; set; } = ChannelDataBaudRate.None;
        /// <summary>
        /// 启用终端电阻
        /// </summary>
        public ChannelResistanceEnable IsResistanceEnable { get; set; } = ChannelResistanceEnable.Disable;
        /// <summary>
        /// key：DeviceCategory 左移 32 位 + DeviceLocation，value：设备编号
        /// </summary>
        [JsonIgnore]
        private readonly ConcurrentDictionary<Int64, DeviceConfig> devices = new ConcurrentDictionary<Int64, DeviceConfig>();
        public void AddDevice(DeviceConfig dc)
        {
            //var key = (((Int64)dc.DCategory) << 32) + (Int64)dc.Location;
            //if (!devices.ContainsKey(key))
            //{
            //    devices.TryAdd(key, dc);
            //}
            //else
            //{
            //    devices[key] = dc;
            //}
        }

        //public DeviceConfig GetDevice(DeviceCategory category, DeviceLocation location)
        //{
        //    var key = (((Int64)category) << 32) + (Int64)location;
        //    DeviceConfig value;
        //    if (devices.TryGetValue(key, out value))
        //    {
        //        return value;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public void ClearDevice()
        {
            this.devices.Clear();
        }

        public IEnumerable<DeviceConfig> GetAllDevice()
        {
            return this.devices.Values;
        }

        //public bool HasDevice(DeviceCategory category)
        //{
        //    bool found = false;
        //    foreach (var item in devices)
        //    {
        //        if (((item.Key >> 32) & 0xFFFFFFFF) == (long)category)
        //        {
        //            found = true;
        //            break;
        //        }
        //    }
        //    return found;
        //}

        public bool IsSupportCanFD()
        {
            return (CCategroy == CanCategory.GCAN_USBCAN_FD
                || CCategroy == CanCategory.CXKJ_USBCANFD
                || CCategroy == CanCategory.ZLG_USBCANFD_100U
                || CCategroy == CanCategory.ZLG_USBCANFD_200U);
        }
    }
}
