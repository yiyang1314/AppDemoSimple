using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Rxbit.Brigade.CanDevices.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RXCanMessage
    {
        /// <summary>
        /// 报文 ID
        /// </summary>
        public UInt32 ID;
        /// <summary>
        /// 报文数据内容长度
        /// </summary>
        public Byte DLC;

        public bool IsCanfd;

        public bool IsBRS;

        public bool IsExdFrame;
        /// <summary>
        /// CAN 通道接收时间戳，相对于 CAN 打开时间，单位微秒
        /// </summary>
        public UInt64 TIMESTAMP;
        /// <summary>
        /// 报文数据内容，根据 DLC 来判断真实有效数据内容
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public Byte[] DATA;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Frame  // 12 字节
    {
        public ushort BlockIndex;
        public byte FrameIndex;  // 0=首帧
        public byte[] Data;        // 长度恒=8，不足补0
        public uint CanID;
    }
    public class CanDevCategoryAttribute : Attribute
    {
        public CanDeviceType Type { get; set; }

        public string Title { get; set; }
        public CanDevCategoryAttribute(CanDeviceType type,string desc)
        {
            this.Type = type;
            this.Title = desc;
        }
    }
    public enum CanDeviceType
    {
        [Description("周立功")]
        ZLGCAN = 0,
        [Description("广成")]
        Gcan = 1,
        [Description("PCAN")]
        PeakCan = 2,
        [Description("创新科技")]
        CXKJ = 3,
        [Description("同星智能")]
        TXZN = 4,
        [Description("Kvaser")]
        Kvaser = 6,
        [Description("NONE")]
        NONE = 5,
    }

    public enum CanCategory
    {
        [CanDevCategory(CanDeviceType.ZLGCAN, "USBCAN_II")]
        ZLG_USBCAN_I = 1003,
        [CanDevCategory(CanDeviceType.ZLGCAN, "USBCAN_II")]
        ZLG_USBCAN_II = 1004,
        [CanDevCategory(CanDeviceType.ZLGCAN, "USBCAN_2EU")]
        ZLG_USBCAN_2EU = 1021,
        [CanDevCategory(CanDeviceType.ZLGCAN, "USBCANFD_100U")]
        ZLG_USBCANFD_100U = 1042,
        [CanDevCategory(CanDeviceType.ZLGCAN, "USBCANFD_200U")]
        ZLG_USBCANFD_200U = 1041,
        [CanDevCategory(CanDeviceType.Gcan, "USBCAN_I")]
        GCAN_USBCAN_V4 = 2003,
        [CanDevCategory(CanDeviceType.Gcan, "USBCAN_II")]
        GCAN_USBCAN_V5 = 2004,
        [CanDevCategory(CanDeviceType.Gcan, "USBCANFD")]
        GCAN_USBCAN_FD = 2006,
        [CanDevCategory(CanDeviceType.PeakCan, "USBCAN")]
        PEAK_CAN = 2,
        [CanDevCategory(CanDeviceType.CXKJ, "USBCAN_I")]
        CXKJ_USBCAN_I = 3003,
        [CanDevCategory(CanDeviceType.CXKJ, "CANalyst_II")]
        CXKJ_CANalyst_II = 3004,
        [CanDevCategory(CanDeviceType.CXKJ, "USBCANFD")]
        CXKJ_USBCANFD = 3041,
        [CanDevCategory(CanDeviceType.Kvaser, "Kvaser_USBCAN")]
        Kvaser_USBCAN = 4100,
        [CanDevCategory(CanDeviceType.NONE, "VirtualCAN")]
        VirtualCAN = 5,
    }

    public static class EnumCanHelper
    {
        /// <summary>
        /// 根据枚举值读取 CanDevCategory 特性
        /// </summary>
        public static CanDeviceType GetCanDevAttr(this CanCategory canCategory)
        {
            // 获取枚举字段信息
            var field = canCategory.GetType().GetField(canCategory.ToString());
            if (field == null) return  CanDeviceType.PeakCan;
            // 读取自定义特性
            var dev = field.GetCustomAttributes(typeof(CanDevCategoryAttribute), false)
                        .FirstOrDefault() as CanDevCategoryAttribute;
            if (dev == null) return CanDeviceType.PeakCan;
            return dev.Type;
        }

        /// <summary>
        /// 获取 CanCategory 枚举中【全部带有 CanDevCategory 特性】的枚举项列表
        /// 每条包含：枚举值、枚举名称、厂商类型、设备名称
        /// </summary>
        public static List<CanCategory> GetAllCanCategoryWithAttr(this CanCategory canCategory)
        {
            var result = new List<CanCategory>();
            var enumType = typeof(CanCategory);
            CanDeviceType type = canCategory.GetCanDevAttr();
            foreach (CanCategory cc in Enum.GetValues(typeof(CanCategory)))
            {
                var cantype = canCategory.GetCanDevAttr();
                if (cantype == type)
                {
                    result.Add(cc);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// 枚举特性提取工具类
    /// </summary>
    public static class EnumAttributeHelper
    {
        /// <summary>
        /// 获取枚举类型上的特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        // 获取枚举类型上的特性
        // 获取枚举成员上的特性
        public static T GetEnumMemberAttribute<T>(Enum enumValue) where T : Attribute
        {
            // 获取枚举成员的FieldInfo
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            if (fieldInfo == null)
                return null;

            // 获取成员上的指定特性
            return fieldInfo.GetCustomAttributes(typeof(T), inherit: false)
                           .FirstOrDefault() as T;
        }

        /// <summary>
        /// 获取枚举类型所有成员及其特性
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <typeparam name="TAttribute">要获取的特性类型</typeparam>
        /// <returns>包含枚举成员和特性的字典</returns>
        public static Dictionary<TEnum, TAttribute> GetEnumMemberAttributes<TEnum, TAttribute>()
            where TEnum : struct, Enum
            where TAttribute : Attribute
        {
            var result = new Dictionary<TEnum, TAttribute>();

            // 获取枚举的所有成员
            var enumMembers = typeof(TEnum).GetMembers(BindingFlags.Public | BindingFlags.Static);

            foreach (var member in enumMembers)
            {
                // 解析枚举值
                if (Enum.TryParse<TEnum>(member.Name, out TEnum enumValue))
                {
                    // 获取成员上的指定特性
                    var attribute = member.GetCustomAttribute<TAttribute>();
                    if (attribute != null)
                    {
                        result[enumValue] = attribute;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取单个枚举值上的特性
        /// </summary>
        /// <typeparam name="TAttribute">要获取的特性类型</typeparam>
        /// <param name="enumValue">枚举值</param>
        /// <returns>枚举值上的特性，如果没有则返回null</returns>
        public static TAttribute GetEnumAttribute<TAttribute>(this Enum enumValue)
            where TAttribute : Attribute
        {
            if (enumValue == null)
                return null;

            // 获取枚举值对应的成员信息
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
            if (memberInfo == null)
                return null;

            // 获取并返回特性
            return memberInfo.GetCustomAttribute<TAttribute>();
        }
    }
}
