using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    // PLC 通用导航项
    public class PlcNavItem : ObservableObject
    {
        private string _icon;
        private string _label;
        private bool _isActive;
        private string _target;

        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public bool IsActive { get => _isActive; set { SetProperty(ref _isActive, value); } }
        public string Target { get => _target; set { SetProperty(ref _target, value); } }
    }

    // PLC1 - VISA 4工位轴控制
    public class PlcAxisItem : ObservableObject
    {
        private string _name;
        private string _position;
        private string _speed;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Position { get => _position; set { SetProperty(ref _position, value); } }
        public string Speed { get => _speed; set { SetProperty(ref _speed, value); } }
    }

    public class PlcStationItem : ObservableObject
    {
        private string _name;
        private ObservableCollection<PlcAxisItem> _axes;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public ObservableCollection<PlcAxisItem> Axes { get => _axes; set { SetProperty(ref _axes, value); } }
    }

    public class Plc1ViewModel : ObservableObject
    {
        private string _account;
        private string _dateText;
        private ObservableCollection<PlcNavItem> _leftNav;
        private ObservableCollection<PlcNavItem> _rightNav;
        private ObservableCollection<PlcStationItem> _stations;

        public string Account { get => _account; set { SetProperty(ref _account, value); } }
        public string DateText { get => _dateText; set { SetProperty(ref _dateText, value); } }
        public ObservableCollection<PlcNavItem> LeftNav { get => _leftNav; set { SetProperty(ref _leftNav, value); } }
        public ObservableCollection<PlcNavItem> RightNav { get => _rightNav; set { SetProperty(ref _rightNav, value); } }
        public ObservableCollection<PlcStationItem> Stations { get => _stations; set { SetProperty(ref _stations, value); } }

        public Plc1ViewModel()
        {
            Account = "登录账号";
            DateText = "2020.03.31  12:30:45";

            LeftNav = new ObservableCollection<PlcNavItem>
            {
                new PlcNavItem { Icon = "●", Label = "手动模式", IsActive = true, Target = "manual" },
                new PlcNavItem { Icon = "●", Label = "自动模式", Target = "auto" },
                new PlcNavItem { Icon = "●", Label = "报警", Target = "alarm" },
                new PlcNavItem { Icon = "●", Label = "设置", Target = "setting" },
                new PlcNavItem { Icon = "●", Label = "登录", Target = "login" },
                new PlcNavItem { Icon = "●", Label = "IO设置", Target = "io" },
            };

            RightNav = new ObservableCollection<PlcNavItem>
            {
                new PlcNavItem { Label = "型号选择", Target = "model" },
                new PlcNavItem { Label = "辊道", Target = "roller" },
                new PlcNavItem { Label = "AGV", Target = "agv" },
                new PlcNavItem { Label = "上一页", Target = "prev" },
                new PlcNavItem { Label = "下一页", Target = "next" },
                new PlcNavItem { Label = "返回", Target = "back" },
            };

            Stations = new ObservableCollection<PlcStationItem>();
            for (int i = 1; i <= 4; i++)
            {
                Stations.Add(new PlcStationItem
                {
                    Name = $"工位{i}",
                    Axes = new ObservableCollection<PlcAxisItem>
                    {
                        new PlcAxisItem { Name = "X轴", Position = "0.00", Speed = "0.00" },
                        new PlcAxisItem { Name = "Y轴", Position = "0.00", Speed = "0.00" },
                        new PlcAxisItem { Name = "Z轴", Position = "0.00", Speed = "0.00" },
                        new PlcAxisItem { Name = "C轴", Position = "0.00", Speed = "0.00" },
                    }
                });
            }
        }
    }

    // PLC2 - MOTEC 自动化设备
    public class Plc2ViewModel : ObservableObject
    {
        private string _dateText;
        private string _userName;
        private string _statusText;

        public string DateText { get => _dateText; set { SetProperty(ref _dateText, value); } }
        public string UserName { get => _userName; set { SetProperty(ref _userName, value); } }
        public string StatusText { get => _statusText; set { SetProperty(ref _statusText, value); } }

        public Plc2ViewModel()
        {
            DateText = "2025/7/1  10:32:59";
            UserName = "用户";
            StatusText = "运行中";
        }
    }

    // PLC3 - 智能控制机监控
    public class Plc3SensorItem : ObservableObject
    {
        private string _label;
        private string _value;
        private string _unit;
        private string _icon;
        private bool _isAlert;

        public string Label { get => _label; set { SetProperty(ref _label, value); } }
        public string Value { get => _value; set { SetProperty(ref _value, value); } }
        public string Unit { get => _unit; set { SetProperty(ref _unit, value); } }
        public string Icon { get => _icon; set { SetProperty(ref _icon, value); } }
        public bool IsAlert { get => _isAlert; set { SetProperty(ref _isAlert, value); } }
    }

    public class Plc3ViewModel : ObservableObject
    {
        private string _currentTime;
        private ObservableCollection<Plc3SensorItem> _leftSensors;
        private ObservableCollection<Plc3SensorItem> _rightSensors;
        private ObservableCollection<Plc3SensorItem> _topSensors;
        private ObservableCollection<Plc3SensorItem> _bottomSensors;

        public string CurrentTime { get => _currentTime; set { SetProperty(ref _currentTime, value); } }
        public ObservableCollection<Plc3SensorItem> LeftSensors { get => _leftSensors; set { SetProperty(ref _leftSensors, value); } }
        public ObservableCollection<Plc3SensorItem> RightSensors { get => _rightSensors; set { SetProperty(ref _rightSensors, value); } }
        public ObservableCollection<Plc3SensorItem> TopSensors { get => _topSensors; set { SetProperty(ref _topSensors, value); } }
        public ObservableCollection<Plc3SensorItem> BottomSensors { get => _bottomSensors; set { SetProperty(ref _bottomSensors, value); } }

        public Plc3ViewModel()
        {
            CurrentTime = "2019-08-08  08:08:08";

            LeftSensors = new ObservableCollection<Plc3SensorItem>
            {
                new Plc3SensorItem { Label = "电流", Value = "22.0", Unit = "A", Icon = "⚡" },
                new Plc3SensorItem { Label = "电压", Value = "220.0", Unit = "V", Icon = "⚡" },
                new Plc3SensorItem { Label = "功率", Value = "200.0", Unit = "W", Icon = "⚡" },
            };

            RightSensors = new ObservableCollection<Plc3SensorItem>
            {
                new Plc3SensorItem { Label = "电流", Value = "22.0", Unit = "A", Icon = "⚡" },
                new Plc3SensorItem { Label = "电压", Value = "220.0", Unit = "V", Icon = "⚡" },
                new Plc3SensorItem { Label = "功率", Value = "200.0", Unit = "W", Icon = "⚡" },
            };

            TopSensors = new ObservableCollection<Plc3SensorItem>
            {
                new Plc3SensorItem { Label = "左上门温度", Value = "22.8℃", Unit = "57.7%", Icon = "🌡" },
                new Plc3SensorItem { Label = "右上门温度", Value = "22.8℃", Unit = "57.7%", Icon = "🌡" },
            };

            BottomSensors = new ObservableCollection<Plc3SensorItem>
            {
                new Plc3SensorItem { Label = "左上门温度", Value = "22.8℃", Unit = "57.7%", Icon = "🌡" },
                new Plc3SensorItem { Label = "右上门温度", Value = "37.8℃", Unit = "57.7%", Icon = "🌡", IsAlert = true },
            };
        }
    }

    // PLC4 - 配方编辑
    public class PlcRecipeStep : ObservableObject
    {
        private int _stepNo;
        private string _name;
        private string _param1;
        private string _param2;

        public int StepNo { get => _stepNo; set { SetProperty(ref _stepNo, value); } }
        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Param1 { get => _param1; set { SetProperty(ref _param1, value); } }
        public string Param2 { get => _param2; set { SetProperty(ref _param2, value); } }
    }

    public class PlcRecipeItem : ObservableObject
    {
        private string _name;
        private string _desc;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Desc { get => _desc; set { SetProperty(ref _desc, value); } }
    }

    public class Plc4ViewModel : ObservableObject
    {
        private string _currentTime;
        private ObservableCollection<PlcNavItem> _leftNav;
        private ObservableCollection<PlcRecipeStep> _steps;
        private ObservableCollection<PlcRecipeItem> _recipes;
        private string _recipeName;
        private string _recipeCode;

        public string CurrentTime { get => _currentTime; set { SetProperty(ref _currentTime, value); } }
        public ObservableCollection<PlcNavItem> LeftNav { get => _leftNav; set { SetProperty(ref _leftNav, value); } }
        public ObservableCollection<PlcRecipeStep> Steps { get => _steps; set { SetProperty(ref _steps, value); } }
        public ObservableCollection<PlcRecipeItem> Recipes { get => _recipes; set { SetProperty(ref _recipes, value); } }
        public string RecipeName { get => _recipeName; set { SetProperty(ref _recipeName, value); } }
        public string RecipeCode { get => _recipeCode; set { SetProperty(ref _recipeCode, value); } }

        public Plc4ViewModel()
        {
            CurrentTime = "2024/08/05  15:53:47";
            RecipeName = "";
            RecipeCode = "";

            LeftNav = new ObservableCollection<PlcNavItem>
            {
                new PlcNavItem { Icon = "☰", Label = "菜单", Target = "menu" },
                new PlcNavItem { Icon = "🏠", Label = "主页面", Target = "home" },
                new PlcNavItem { Icon = "✎", Label = "配方编辑", IsActive = true, Target = "recipe" },
                new PlcNavItem { Icon = "⚠", Label = "警报记录", Target = "alarm" },
                new PlcNavItem { Icon = "📋", Label = "操作记录", Target = "oplog" },
                new PlcNavItem { Icon = "📈", Label = "历史趋势", Target = "trend" },
                new PlcNavItem { Icon = "👤", Label = "用户管理", Target = "user" },
            };

            Steps = new ObservableCollection<PlcRecipeStep>
            {
                new PlcRecipeStep { StepNo = 1, Name = "水（WFI）注入", Param1 = "温度：25±2℃", Param2 = "流速：50 L/min" },
                new PlcRecipeStep { StepNo = 2, Name = "基础培养基溶解", Param1 = "搅拌：100 rpm", Param2 = "时间：30 min" },
                new PlcRecipeStep { StepNo = 3, Name = "葡萄糖添加", Param1 = "温度：25±2℃", Param2 = "流速：50 L/min" },
                new PlcRecipeStep { StepNo = 4, Name = "氨基酸补充", Param1 = "温度：25±2℃", Param2 = "流速：50 L/min" },
            };

            Recipes = new ObservableCollection<PlcRecipeItem>
            {
                new PlcRecipeItem { Name = "细胞培养基配制", Desc = "用于细菌/生物反应器" },
                new PlcRecipeItem { Name = "无菌注射液配方", Desc = "大输液或疫苗佐剂" },
                new PlcRecipeItem { Name = "静脉输液", Desc = "生理盐水、葡萄糖注射液" },
                new PlcRecipeItem { Name = "化工/精细化工配方", Desc = "催化剂或纳米颗粒浊度" },
            };
        }
    }

    // PLC5 - 操作员界面
    public class PlcAlarmItem : ObservableObject
    {
        private string _time;
        private string _content;

        public string Time { get => _time; set { SetProperty(ref _time, value); } }
        public string Content { get => _content; set { SetProperty(ref _content, value); } }
    }

    public class Plc5ViewModel : ObservableObject
    {
        private string _operatorName;
        private string _currentTime;
        private string _currentOutput;
        private string _currentCycle;
        private string _currentStatus;
        private string _cycleTime;
        private string _operatorGroup;
        private ObservableCollection<PlcNavItem> _leftNav;
        private ObservableCollection<PlcAlarmItem> _alarms;

        public string OperatorName { get => _operatorName; set { SetProperty(ref _operatorName, value); } }
        public string CurrentTime { get => _currentTime; set { SetProperty(ref _currentTime, value); } }
        public string CurrentOutput { get => _currentOutput; set { SetProperty(ref _currentOutput, value); } }
        public string CurrentCycle { get => _currentCycle; set { SetProperty(ref _currentCycle, value); } }
        public string CurrentStatus { get => _currentStatus; set { SetProperty(ref _currentStatus, value); } }
        public string CycleTime { get => _cycleTime; set { SetProperty(ref _cycleTime, value); } }
        public string OperatorGroup { get => _operatorGroup; set { SetProperty(ref _operatorGroup, value); } }
        public ObservableCollection<PlcNavItem> LeftNav { get => _leftNav; set { SetProperty(ref _leftNav, value); } }
        public ObservableCollection<PlcAlarmItem> Alarms { get => _alarms; set { SetProperty(ref _alarms, value); } }

        public Plc5ViewModel()
        {
            OperatorName = "操作员1008";
            OperatorGroup = "组 (6) 03";
            CurrentTime = "2024/7/9  星期二  10:35AM";
            CurrentOutput = "0";
            CurrentCycle = "0.0";
            CurrentStatus = "紧急停止中";
            CycleTime = "10:35AM";

            LeftNav = new ObservableCollection<PlcNavItem>
            {
                new PlcNavItem { Icon = "🏃", Label = "运行界面", IsActive = true, Target = "run" },
                new PlcNavItem { Icon = "⚙", Label = "工作站面", Target = "station" },
                new PlcNavItem { Icon = "🔧", Label = "气缸页面", Target = "cylinder" },
                new PlcNavItem { Icon = "🎯", Label = "伺服页面", Target = "servo" },
                new PlcNavItem { Icon = "🔢", Label = "参数设置", Target = "param" },
                new PlcNavItem { Icon = "⚠", Label = "报警记录", Target = "alarm" },
                new PlcNavItem { Icon = "📡", Label = "IO监视", Target = "io" },
                new PlcNavItem { Icon = "🧩", Label = "其他功能", Target = "other" },
            };

            Alarms = new ObservableCollection<PlcAlarmItem>
            {
                new PlcAlarmItem { Time = "09:57:31", Content = "设备尚未运转准备" },
                new PlcAlarmItem { Time = "09:57:31", Content = "M01未激磁" },
                new PlcAlarmItem { Time = "09:57:31", Content = "M01未激磁" },
                new PlcAlarmItem { Time = "09:57:31", Content = "M01未激磁" },
                new PlcAlarmItem { Time = "09:57:31", Content = "M01未激磁" },
                new PlcAlarmItem { Time = "09:57:31", Content = "M01未激磁" },
            };
        }
    }
}
