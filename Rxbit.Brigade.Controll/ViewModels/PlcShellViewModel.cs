using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class PlcShellViewModel : ObservableObject
    {
        private ObservableCollection<PlcNavItem> _menus;
        private string _current;

        public ObservableCollection<PlcNavItem> Menus { get => _menus; set { SetProperty(ref _menus, value); } }
        public string Current { get => _current; set { SetProperty(ref _current, value); } }

        public PlcShellViewModel()
        {
            Menus = new ObservableCollection<PlcNavItem>
            {
                new PlcNavItem { Icon = "●", Label = "VISA 4工位", Target = "plc1", IsActive = true },
                new PlcNavItem { Icon = "◉", Label = "MOTEC 自动化", Target = "plc2", IsActive = false },
                new PlcNavItem { Icon = "⚡", Label = "智能控制机监控", Target = "plc3", IsActive = false },
                new PlcNavItem { Icon = "⚗", Label = "配方编辑", Target = "plc4", IsActive = false },
                new PlcNavItem { Icon = "👷", Label = "操作员界面", Target = "plc5", IsActive = false },
            };
            Current = "plc1";
        }

        public void Navigate(string target)
        {
            Current = target;
            foreach (var m in Menus)
                m.IsActive = m.Target == target;
        }
    }
}
