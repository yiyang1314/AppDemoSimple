using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class DashboardIconItem : ObservableObject
    {
        private string _icon;
        private string _name;

        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }

    public class Dashboard1ViewModel : ObservableObject
    {
        private string _searchText;
        private int _selectedTab; // 0=Icons, 1=Elements, 2=UI Icons

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterIcons();
                }
            }
        }

        public int SelectedTab
        {
            get { return _selectedTab; }
            set { SetProperty(ref _selectedTab, value); }
        }

        public ObservableCollection<DashboardIconItem> AllIcons { get; set; }
        public ObservableCollection<DashboardIconItem> FilteredIcons { get; set; }

        public Dashboard1ViewModel()
        {
            AllIcons = new ObservableCollection<DashboardIconItem>
            {
                new DashboardIconItem { Icon = "ⓘ", Name = "404" },
                new DashboardIconItem { Icon = "🐛", Name = "bug" },
                new DashboardIconItem { Icon = "📊", Name = "chart" },
                new DashboardIconItem { Icon = "📋", Name = "clipboard" },
                new DashboardIconItem { Icon = "📦", Name = "component" },
                new DashboardIconItem { Icon = "🎯", Name = "dashboard" },
                new DashboardIconItem { Icon = "📄", Name = "documentation" },
                new DashboardIconItem { Icon = "❤️", Name = "donate" },
                new DashboardIconItem { Icon = "🏃", Name = "drag" },
                new DashboardIconItem { Icon = "🚪", Name = "exit" },
                new DashboardIconItem { Icon = "📚", Name = "education" },
                new DashboardIconItem { Icon = "✉️", Name = "email" },
                new DashboardIconItem { Icon = "⟲", Name = "expand" },
                new DashboardIconItem { Icon = "🖼️", Name = "excel" },
                new DashboardIconItem { Icon = "🖥️", Name = "fullscreen" },
                new DashboardIconItem { Icon = "👁️", Name = "eye-open" },
                new DashboardIconItem { Icon = "👁️‍🗨️", Name = "eye" },
                new DashboardIconItem { Icon = "📝", Name = "form" },
                new DashboardIconItem { Icon = "⤢", Name = "fullscreen" },
                new DashboardIconItem { Icon = "🧭", Name = "guide" },
                new DashboardIconItem { Icon = "ℹ️", Name = "icon" },
                new DashboardIconItem { Icon = "🌐", Name = "international" },
                new DashboardIconItem { Icon = "🗣️", Name = "language" },
                new DashboardIconItem { Icon = "🔗", Name = "link" },
                new DashboardIconItem { Icon = "📋", Name = "list" },
                new DashboardIconItem { Icon = "🔒", Name = "lock" },
                new DashboardIconItem { Icon = "💬", Name = "message" },
                new DashboardIconItem { Icon = "💰", Name = "money" },
                new DashboardIconItem { Icon = "🗂️", Name = "nested" },
                new DashboardIconItem { Icon = "🔐", Name = "password" },
                new DashboardIconItem { Icon = "📕", Name = "pdf" },
                new DashboardIconItem { Icon = "👤", Name = "people" },
                new DashboardIconItem { Icon = "👥", Name = "peoples" },
                new DashboardIconItem { Icon = "🔍", Name = "qq" },
                new DashboardIconItem { Icon = "🔎", Name = "search" },
                new DashboardIconItem { Icon = "🛒", Name = "shopping" },
                new DashboardIconItem { Icon = "📏", Name = "size" },
                new DashboardIconItem { Icon = "🎓", Name = "skill" },
                new DashboardIconItem { Icon = "⭐", Name = "star" },
                new DashboardIconItem { Icon = "🪟", Name = "tab" },
                new DashboardIconItem { Icon = "📑", Name = "table" },
                new DashboardIconItem { Icon = "🎨", Name = "theme" },
                new DashboardIconItem { Icon = "🌳", Name = "tree-table" },
                new DashboardIconItem { Icon = "🌲", Name = "tree" },
                new DashboardIconItem { Icon = "👤", Name = "user" },
                new DashboardIconItem { Icon = "💬", Name = "wechat" },
                new DashboardIconItem { Icon = "🤐", Name = "zip" },
            };

            FilteredIcons = new ObservableCollection<DashboardIconItem>(AllIcons);
        }

        private void FilterIcons()
        {
            FilteredIcons.Clear();
            if (string.IsNullOrEmpty(_searchText))
            {
                foreach (var item in AllIcons)
                    FilteredIcons.Add(item);
            }
            else
            {
                foreach (var item in AllIcons)
                {
                    if (item.Name.IndexOf(_searchText, System.StringComparison.OrdinalIgnoreCase) >= 0)
                        FilteredIcons.Add(item);
                }
            }
            OnPropertyChanged("FilteredIcons");
        }
    }
}
