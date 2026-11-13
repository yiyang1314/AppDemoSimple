using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Rxbit.Brigade.Models.EnumEntitys
{
    /// <summary>
    /// 常量自定义
    /// </summary>
    public static class RadarConstaint
    {
        #region 路径常量定义
        public static string AppWorkPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
        public static string RecondingPath = @"C:\Data";
        public static string FlashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Brigade_flash.hex");
        public static string PCANPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "peakcan", "PCANBasic.dll");
        public static string BrigaeSdkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrigadeSdk.zip");
        public static string SettingFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Brigade", "BrigadeSetting.config");
        public static string BUSFile = "BrigadeSetting.config";

        #endregion

        #region 雷达协议常量自定义
        public static string[] MoveString = { "未知", "径向远离", "径向靠近", "向左横穿", "向右横穿", "静态", "停止" };
        public static string[] Cel2xx = { "未知", "径向远离", "径向靠近", "向左横穿", "向右横穿", "静态", "停止" };
        public static string[] Cel3xx = { "未知", "径向远离", "径向靠近", "向左横穿", "向右横穿", "静态", "停止" };
        public static string[] Alivedecs = { "维持存活", "有跟踪目标", "传感器未配置", "预留" };
        public static string[] ClsassCate = { "Unknown", "VRU", "CAR" };
        public static string[] RepeatArrs = { "重新检测", "新检测" };
        public static string[] VaildArrs = { "有效", "无效" };
        public static string[] IdentArrs = { "BS-9000", "77GHz Radar" };
        public static string[] NGArrs = { "Ok", "NG" };
        public static string[] YesArrs = { "No", "Yes" };
        public static string PolarRadiusTip = "经向距离";
        public static string PolarAngleTip = "经向角度";
        public static string LongitudeTip = "纵向距离";
        public static string LatgitudeTip = "横向距离";
        public static string RelativeVTip = "相对速度";
        public static string SignalTip = "信号功率";
        public static string AppearTip = "外观状态";
        public static string DetectionTip = "检测状态";
        public static string TriggerTip = "触发事件";
        public static string RadarTypeTip = "雷达类型";
        public static string MMICTip = "射频状态";
        public static string TemperaTip = "温度状态";
        public static string VoltageTip = "电压状态";
        public static string CanTip = "遮挡状态";
        public static string LatVxTip = "横向速度";
        public static string LongVyTip = "纵向速度";
        public static string ProbExistTip = "置信度";
        public static string LatAccVxTip = "横向加速度";
        public static string LongAccVyTip = "纵向加速度";
        public static string RCSTip = "RCS";
        public static string SNRTip = "信噪比";
        public static string MAGTip = "信号幅度";
        public static string LiveTip = "存活时间";
        public static string SportTip = "运动状态";
        public static string ClassTip = "分类状态";
        public static string Reflection = "高反射目标";
        #endregion

        public static string[] GetArrsInfo(string key)
        {
            var list = Application.Current.TryFindResource(key) as string[];
            return list;
        }

        public static string GetInfo(string key)
        {
            var str = Application.Current.TryFindResource(key).ToString();
            return str;
        }
        public static void Init()
        {
            MoveString = GetArrsInfo("MoveInfo");
            Alivedecs = GetArrsInfo("Alivedecs");
            ClsassCate = GetArrsInfo("ClsassCate");
            RepeatArrs = GetArrsInfo("RepeatInfo");
            VaildArrs = GetArrsInfo("VaildInfo");
            Cel2xx = GetArrsInfo("Header2xx");
            Cel3xx = GetArrsInfo("Header3xx");
            PolarRadiusTip = GetInfo("PolarRadiusTip");
            PolarAngleTip = GetInfo("PolarAngleTip");
            LongitudeTip = GetInfo("LongitudeTip");
            LatgitudeTip = GetInfo("LatgitudeTip");
            RelativeVTip = GetInfo("RelativeVTip");
            SignalTip = GetInfo("SignalTip");
            AppearTip = GetInfo("AppearTip");
            DetectionTip = GetInfo("DetectionTip");
            TriggerTip = GetInfo("TriggerTip");
            RadarTypeTip = GetInfo("RadarTypeTip");
            MMICTip = GetInfo("MMICTip");
            TemperaTip = GetInfo("TemperaTip");
            VoltageTip = GetInfo("VoltageTip");
            CanTip = GetInfo("CanTip");
            LatVxTip = GetInfo("LatVxTip");
            LongVyTip = GetInfo("LongVyTip");
            ProbExistTip = GetInfo("ProbExistTip");
            LatAccVxTip = GetInfo("LatAccVxTip");
            LongAccVyTip = GetInfo("LongAccVyTip");
            RCSTip = GetInfo("RCSTip");
            SNRTip = GetInfo("SNRTip");
            MAGTip = GetInfo("MAGTip");
            LiveTip = GetInfo("LiveTip");
            SportTip = GetInfo("SportTip");
            ClassTip = GetInfo("ClassTip");
            Reflection = GetInfo("Detection_ReflectTime");
        }
    }
}