using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rxbit.Brigade.HalconVision.Mvvm;

namespace Rxbit.Brigade.HalconVision.ViewModels
{
    public class HalconMainViewModel : ObservableObject
    {
        private ObservableCollection<HalconTabItem> _tabs;
        private HalconTabItem _selectedTab;
        private int _tabCounter;

        public ObservableCollection<HalconTabItem> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public HalconTabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                    OnPropertyChanged(nameof(HasSelectedTab));
            }
        }

        public bool HasSelectedTab => SelectedTab != null;

        public ICommand NewGraphicsWindowCommand { get; }
        public ICommand NewProgramWindowCommand { get; }
        public ICommand NewVariablesWindowCommand { get; }
        public ICommand CloseTabCommand { get; }

        public HalconMainViewModel()
        {
            _tabs = new ObservableCollection<HalconTabItem>();
            _tabCounter = 0;

            NewGraphicsWindowCommand = new RelayCommand(() => AddTab(HalconTabType.Graphics));
            NewProgramWindowCommand = new RelayCommand(() => AddTab(HalconTabType.Program));
            NewVariablesWindowCommand = new RelayCommand(() => AddTab(HalconTabType.Variables));
            CloseTabCommand = new RelayCommand<HalconTabItem>(CloseTab);

            AddTab(HalconTabType.Graphics);
            AddTab(HalconTabType.Program);
            AddTab(HalconTabType.Variables);
        }

        private void AddTab(HalconTabType type)
        {
            _tabCounter++;
            var tab = new HalconTabItem
            {
                Id = _tabCounter,
                TabType = type
            };

            switch (type)
            {
                case HalconTabType.Graphics:
                    tab.Title = $"图形窗口 [HALCON{tab.Id}]";
                    tab.Content = new GraphicsWindowViewModel(tab.Id);
                    break;
                case HalconTabType.Program:
                    tab.Title = $"程序窗口 - main()";
                    tab.Content = new ProgramWindowViewModel();
                    break;
                case HalconTabType.Variables:
                    tab.Title = $"变量窗口 - main()";
                    tab.Content = new VariablesWindowViewModel();
                    break;
            }

            Tabs.Add(tab);
            SelectedTab = tab;
        }

        private void CloseTab(HalconTabItem tab)
        {
            if (tab == null) return;
            Tabs.Remove(tab);
            if (SelectedTab == tab)
                SelectedTab = Tabs.Count > 0 ? Tabs[Tabs.Count - 1] : null;
        }
    }

    public enum HalconTabType
    {
        Graphics,
        Program,
        Variables
    }

    public class HalconTabItem : ObservableObject
    {
        private int _id;
        private string _title;
        private HalconTabType _tabType;
        private object _content;
        private bool _isClosable = true;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public HalconTabType TabType { get => _tabType; set => SetProperty(ref _tabType, value); }
        public object Content { get => _content; set => SetProperty(ref _content, value); }
        public bool IsClosable { get => _isClosable; set => SetProperty(ref _isClosable, value); }
    }
}