using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class HmiNavItem : ObservableObject
    {
        private string _label;
        private bool _isActive;
        private string _target;

        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public string Target { get => _target; set { SetProperty(ref _target, value); } }
    }

    // HMI1 - 地铁机房监测
    public class HmiEnvironmentSensor : ObservableObject
    {
        private string _name;
        private bool _isActive;
        private string _temp;
        private string _humidity;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public string Temp { get => _temp; set { SetProperty(ref _temp, value); } }
        public string Humidity { get => _humidity; set { SetProperty(ref _humidity, value); } }
    }

    public class Hmi1ViewModel : ObservableObject
    {
        private ObservableCollection<HmiNavItem> _leftMenu;
        private ObservableCollection<string> _channelList;
        private string _selectedChannel;
        private string _deviceId;
        private string _currentTemp;
        private string _currentHumidity;
        private string _alarmHigh;
        private string _alarmLow;
        private string _tempUpper;
        private string _tempLower;

        public ObservableCollection<HmiNavItem> LeftMenu { get => _leftMenu; set { SetProperty(ref _leftMenu, value); } }
        public ObservableCollection<string> ChannelList { get => _channelList; set { SetProperty(ref _channelList, value); } }
        public string SelectedChannel { get => _selectedChannel; set { SetProperty(ref _selectedChannel, value); } }
        public string DeviceId { get => _deviceId; set { SetProperty(ref _deviceId, value); } }
        public string CurrentTemp { get => _currentTemp; set { SetProperty(ref _currentTemp, value); } }
        public string CurrentHumidity { get => _currentHumidity; set { SetProperty(ref _currentHumidity, value); } }
        public string AlarmHigh { get => _alarmHigh; set { SetProperty(ref _alarmHigh, value); } }
        public string AlarmLow { get => _alarmLow; set { SetProperty(ref _alarmLow, value); } }
        public string TempUpper { get => _tempUpper; set { SetProperty(ref _tempUpper, value); } }
        public string TempLower { get => _tempLower; set { SetProperty(ref _tempLower, value); } }

        public Hmi1ViewModel()
        {
            LeftMenu = new ObservableCollection<HmiNavItem>
            {
                new HmiNavItem { Label = "温湿度传感器", IsActive = true, Target = "1" },
                new HmiNavItem { Label = "烟感传感器", IsActive = false, Target = "2" },
                new HmiNavItem { Label = "多模空气监测", IsActive = false, Target = "3" },
                new HmiNavItem { Label = "UPS5传感器", IsActive = false, Target = "4" },
                new HmiNavItem { Label = "线缆监测温监测", IsActive = false, Target = "5" },
            };

            ChannelList = new ObservableCollection<string> { "1号柱", "2号柱", "3号柱", "4号柱", "5号柱", "6号柱", "7号柱", "8号柱" };
            SelectedChannel = "1号柱";

            DeviceId = "DJHGHS0450199NT";
            CurrentTemp = "32.1℃";
            CurrentHumidity = "11%";
            AlarmHigh = "1 开启";
            AlarmLow = "1 开启";
            TempUpper = "22 - 25 ℃";
            TempLower = "23 - 25 ℃";
        }
    }

    // HMI2 - 美容导入仪
    public class BeautyStageItem : ObservableObject
    {
        private string _stageName;
        private string _time;
        private int _intensity;
        private bool _isActive;

        public string StageName { get => _stageName; set { SetProperty(ref _stageName, value); } }
        public string Time { get => _time; set { SetProperty(ref _time, value); } }
        public int Intensity { get => _intensity; set { SetProperty(ref _intensity, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
    }

    public class Hmi2ViewModel : ObservableObject
    {
        private ObservableCollection<BeautyStageItem> _stages;
        private string _currentMode;
        private bool _isRunning;

        public ObservableCollection<BeautyStageItem> Stages { get => _stages; set { SetProperty(ref _stages, value); } }
        public string CurrentMode { get => _currentMode; set { SetProperty(ref _currentMode, value); } }
        public bool IsRunning { get => _isRunning; set { SetProperty(ref _isRunning, value); } }

        public Hmi2ViewModel()
        {
            CurrentMode = "自动模式";
            IsRunning = false;

            Stages = new ObservableCollection<BeautyStageItem>
            {
                new BeautyStageItem { StageName = "阶段一", Time = "05:00", Intensity = 4, IsActive = true },
                new BeautyStageItem { StageName = "阶段二", Time = "05:00", Intensity = 3, IsActive = false },
                new BeautyStageItem { StageName = "阶段三", Time = "05:00", Intensity = 4, IsActive = false },
                new BeautyStageItem { StageName = "阶段四", Time = "05:00", Intensity = 3, IsActive = false },
                new BeautyStageItem { StageName = "阶段五", Time = "05:00", Intensity = 4, IsActive = false },
            };
        }
    }

    // HMI3 - 核应用程序管理
    public class CoreAppItem : ObservableObject
    {
        private string _name;
        private string _status;
        private bool _isActive;
        private bool _isAlert;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Status { get => _status; set { SetProperty(ref _status, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public bool IsAlert { get => _isAlert; set { SetProperty(ref _isAlert, value); } }
    }

    public class Hmi3ViewModel : ObservableObject
    {
        private string _operatorName;
        private string _operatorTitle;
        private ObservableCollection<HmiNavItem> _sideMenu;
        private ObservableCollection<CoreAppItem> _apps;

        public string OperatorName { get => _operatorName; set { SetProperty(ref _operatorName, value); } }
        public string OperatorTitle { get => _operatorTitle; set { SetProperty(ref _operatorTitle, value); } }
        public ObservableCollection<HmiNavItem> SideMenu { get => _sideMenu; set { SetProperty(ref _sideMenu, value); } }
        public ObservableCollection<CoreAppItem> Apps { get => _apps; set { SetProperty(ref _apps, value); } }

        public Hmi3ViewModel()
        {
            OperatorName = "操作员";
            OperatorTitle = "仪控工程师";

            SideMenu = new ObservableCollection<HmiNavItem>
            {
                new HmiNavItem { Label = "应用程序管理", IsActive = true, Target = "mgr" },
                new HmiNavItem { Label = "应用程序维护", IsActive = false, Target = "mtn" },
                new HmiNavItem { Label = "数据点管理", IsActive = false, Target = "data" },
                new HmiNavItem { Label = "权限管理", IsActive = false, Target = "perm" },
                new HmiNavItem { Label = "日志", IsActive = false, Target = "log" },
                new HmiNavItem { Label = "运行状态", IsActive = false, Target = "state" },
            };

            Apps = new ObservableCollection<CoreAppItem>
            {
                new CoreAppItem { Name = "TAP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "ROC", Status = "正在运行", IsActive = true, IsAlert = true },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
                new CoreAppItem { Name = "TAP", Status = "未运行", IsActive = false, IsAlert = false },
                new CoreAppItem { Name = "ADP", Status = "正在运行", IsActive = true, IsAlert = false },
            };
        }
    }

    // HMI4 - 自动化设备
    public class AutoSwitchItem : ObservableObject
    {
        private string _label;
        private bool _value;
        private string _onLabel;
        private string _offLabel;

        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool Value { get => _value; set { SetProperty(ref _value, value); } }
        public string OnLabel { get => _onLabel; set { SetProperty(ref _onLabel, value); } }
        public string OffLabel { get => _offLabel; set { SetProperty(ref _offLabel, value); } }
    }

    public class Hmi4ViewModel : ObservableObject
    {
        private string _dateText;
        private ObservableCollection<AutoSwitchItem> _gripperSwitches;
        private ObservableCollection<AutoSwitchItem> _traySwitches;

        public string DateText { get => _dateText; set { SetProperty(ref _dateText, value); } }
        public ObservableCollection<AutoSwitchItem> GripperSwitches { get => _gripperSwitches; set { SetProperty(ref _gripperSwitches, value); } }
        public ObservableCollection<AutoSwitchItem> TraySwitches { get => _traySwitches; set { SetProperty(ref _traySwitches, value); } }

        public Hmi4ViewModel()
        {
            DateText = "2025/7/1  10:32:59";

            GripperSwitches = new ObservableCollection<AutoSwitchItem>
            {
                new AutoSwitchItem { Label = "转角缸", OffLabel = "松开", OnLabel = "夹紧", Value = false },
                new AutoSwitchItem { Label = "防坠落", OffLabel = "松开", OnLabel = "夹紧", Value = false },
                new AutoSwitchItem { Label = "插销缸", OffLabel = "松开", OnLabel = "夹紧", Value = true },
            };

            TraySwitches = new ObservableCollection<AutoSwitchItem>
            {
                new AutoSwitchItem { Label = "上提", OffLabel = "缩回", OnLabel = "伸出", Value = true },
                new AutoSwitchItem { Label = "下提", OffLabel = "缩回", OnLabel = "伸出", Value = true },
                new AutoSwitchItem { Label = "托举", OffLabel = "缩回", OnLabel = "伸出", Value = true },
                new AutoSwitchItem { Label = "挡停", OffLabel = "缩回", OnLabel = "伸出", Value = true },
            };
        }
    }

    // HMI5 - 参数设置 + 雷达图
    public class ParamSettingItem : ObservableObject
    {
        private string _name;
        private string _value;
        private string _unit;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Value { get => _value; set { SetProperty(ref _value, value); } }
        public string Unit { get => _unit; set { SetProperty(ref _unit, value); } }
    }

    public class Hmi5ViewModel : ObservableObject
    {
        private ObservableCollection<HmiNavItem> _sideMenu;
        private ObservableCollection<ParamSettingItem> _params;
        private ObservableCollection<double> _radarValues;
        private ObservableCollection<string> _radarLabels;

        public ObservableCollection<HmiNavItem> SideMenu { get => _sideMenu; set { SetProperty(ref _sideMenu, value); } }
        public ObservableCollection<ParamSettingItem> Params { get => _params; set { SetProperty(ref _params, value); } }
        public ObservableCollection<double> RadarValues { get => _radarValues; set { SetProperty(ref _radarValues, value); } }
        public ObservableCollection<string> RadarLabels { get => _radarLabels; set { SetProperty(ref _radarLabels, value); } }

        public Hmi5ViewModel()
        {
            SideMenu = new ObservableCollection<HmiNavItem>
            {
                new HmiNavItem { Label = "用户登录", IsActive = false, Target = "login" },
                new HmiNavItem { Label = "自动画面", IsActive = false, Target = "auto" },
                new HmiNavItem { Label = "手动画面", IsActive = false, Target = "manual" },
                new HmiNavItem { Label = "参数设置", IsActive = true, Target = "param" },
                new HmiNavItem { Label = "报警查询", IsActive = false, Target = "alarm" },
                new HmiNavItem { Label = "IO画面", IsActive = false, Target = "io" },
            };

            Params = new ObservableCollection<ParamSettingItem>
            {
                new ParamSettingItem { Name = "参数设置D0", Value = "012.3", Unit = "Nm" },
                new ParamSettingItem { Name = "参数设置D1", Value = "012.3", Unit = "Nm" },
                new ParamSettingItem { Name = "参数设置D2", Value = "012.3", Unit = "Nm" },
                new ParamSettingItem { Name = "参数设置D3", Value = "012.3", Unit = "Nm" },
                new ParamSettingItem { Name = "参数设置D4", Value = "012.3", Unit = "Nm" },
            };

            RadarLabels = new ObservableCollection<string> { "设置1", "设置5", "设置4", "设置3", "设置2" };
            RadarValues = new ObservableCollection<double> { 120, 130, 140, 200, 140 };
        }
    }
}
