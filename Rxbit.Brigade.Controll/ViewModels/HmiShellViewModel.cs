using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class HmiShellMenu : ObservableObject
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

    public class HmiShellViewModel : ObservableObject
    {
        private ObservableCollection<HmiShellMenu> _menus;
        private string _current;

        public ObservableCollection<HmiShellMenu> Menus { get => _menus; set { SetProperty(ref _menus, value); } }
        public string Current { get => _current; set { SetProperty(ref _current, value); } }

        public HmiShellViewModel()
        {
            Menus = new ObservableCollection<HmiShellMenu>
            {
                new HmiShellMenu { Icon = "🌡", Label = "地铁机房监测", Target = "hmi1", IsActive = true },
                new HmiShellMenu { Icon = "✨", Label = "美容导入仪", Target = "hmi2", IsActive = false },
                new HmiShellMenu { Icon = "⚛", Label = "核应用服务端", Target = "hmi3", IsActive = false },
                new HmiShellMenu { Icon = "🤖", Label = "自动化设备", Target = "hmi4", IsActive = false },
                new HmiShellMenu { Icon = "📊", Label = "参数设置雷达", Target = "hmi5", IsActive = false },
            };
            Current = "hmi1";
        }

        public void Navigate(string target)
        {
            Current = target;
            foreach (var m in Menus)
            {
                m.IsActive = m.Target == target;
            }
        }
    }
}
