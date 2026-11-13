using System.Collections.ObjectModel;
using Rxbit.Brigade.HalconVision.Mvvm;

namespace Rxbit.Brigade.HalconVision.ViewModels
{
    public class VariablesWindowViewModel : ObservableObject
    {
        private string _title;
        private string _selectedGroup = "Iconics";
        private ObservableCollection<VariableGroup> _groups;
        private VariableItem _selectedVariable;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string SelectedGroup { get => _selectedGroup; set => SetProperty(ref _selectedGroup, value); }
        public ObservableCollection<VariableGroup> Groups { get => _groups; set => SetProperty(ref _groups, value); }
        public VariableItem SelectedVariable { get => _selectedVariable; set => SetProperty(ref _selectedVariable, value); }

        public VariablesWindowViewModel()
        {
            _title = "变量窗口 - main()";
            _groups = new ObservableCollection<VariableGroup>();

            // Iconics 组
            var iconicsGroup = new VariableGroup { Name = "Iconics", IsExpanded = true };
            iconicsGroup.Variables.Add(new VariableItem { Name = "Contours", Value = "XLD[460,4090]", Type = "XLD", IsArray = true, ElementCount = 460 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "Countours1", Value = "Region[8537,8186]", Type = "Region", IsArray = true, ElementCount = 8537 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "Region", Value = "Region[8537,8186]", Type = "Region", IsArray = true, ElementCount = 8537 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "ImaAmp", Value = "byte[512,512]", Type = "Image", IsArray = true, ElementCount = 262144 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "ImaAmp2", Value = "byte[512,512]", Type = "Image", IsArray = true, ElementCount = 262144 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "ImaDir", Value = "byte[512,512]", Type = "Image", IsArray = true, ElementCount = 262144 });
            iconicsGroup.Variables.Add(new VariableItem { Name = "Image", Value = "byte[512,512]", Type = "Image", IsArray = true, ElementCount = 262144 });
            _groups.Add(iconicsGroup);

            // Control 组
            var controlGroup = new VariableGroup { Name = "Control", IsExpanded = false };
            controlGroup.Variables.Add(new VariableItem { Name = "WindowID", Value = "200000", Type = "control", IsArray = false });
            _groups.Add(controlGroup);

            // Values 组
            var valuesGroup = new VariableGroup { Name = "Values", IsExpanded = false };
            valuesGroup.Variables.Add(new VariableItem { Name = "ImaAmp", Value = "0.000135522", Type = "real", IsArray = false });
            valuesGroup.Variables.Add(new VariableItem { Name = "ImaAmp2", Value = "0.000007931", Type = "real", IsArray = false });
            valuesGroup.Variables.Add(new VariableItem { Name = "Seconds1", Value = "0.000004821", Type = "real", IsArray = false });
            valuesGroup.Variables.Add(new VariableItem { Name = "Seconds2", Value = "0.000007931", Type = "real", IsArray = false });
            _groups.Add(valuesGroup);
        }
    }

    public class VariableGroup : ObservableObject
    {
        private string _name;
        private bool _isExpanded;
        private ObservableCollection<VariableItem> _variables;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
        public ObservableCollection<VariableItem> Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }

        public VariableGroup()
        {
            _variables = new ObservableCollection<VariableItem>();
        }
    }

    public class VariableItem : ObservableObject
    {
        private string _name;
        private string _value;
        private string _type;
        private bool _isArray;
        private int _elementCount;
        private bool _isSelected;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        public bool IsArray { get => _isArray; set => SetProperty(ref _isArray, value); }
        public int ElementCount { get => _elementCount; set => SetProperty(ref _elementCount, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }
}