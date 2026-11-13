using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Rxbit.Brigade.ZcanPro.Mvvm;

namespace Rxbit.Brigade.ZcanPro.ViewModels
{
    public class ZcanMainViewModel : ObservableObject
    {
        private ObservableCollection<ZcanTabItem> _tabs;
        private ZcanTabItem _selectedTab;
        private int _tabCounter;

        public ObservableCollection<ZcanTabItem> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public ZcanTabItem SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                    OnPropertyChanged(nameof(HasSelectedTab));
            }
        }

        public bool HasSelectedTab => SelectedTab != null;

        public ICommand NewOscilloscopeCommand { get; }
        public ICommand NewDataPanelCommand { get; }

        public ZcanMainViewModel()
        {
            _tabs = new ObservableCollection<ZcanTabItem>();
            _tabCounter = 0;

            NewOscilloscopeCommand = new RelayCommand(() => AddTab(ZcanTabType.Oscilloscope));
            NewDataPanelCommand = new RelayCommand(() => AddTab(ZcanTabType.DataPanel));
        }

        private void AddTab(ZcanTabType type)
        {
            _tabCounter++;
            var tab = new ZcanTabItem
            {
                Id = _tabCounter,
                TabType = type
            };

            switch (type)
            {
                case ZcanTabType.Oscilloscope:
                    tab.Title = $"示波器视图 {_tabCounter}";
                    tab.Content = new OscilloscopeViewModel(_tabCounter);
                    break;
                case ZcanTabType.DataPanel:
                    tab.Title = $"数据监控面板 {_tabCounter}";
                    tab.Content = new DataPanelViewModel(_tabCounter);
                    break;
            }

            Tabs.Add(tab);
            SelectedTab = tab;
        }

        public void CloseTab(ZcanTabItem tab)
        {
            if (tab == null) return;
            Tabs.Remove(tab);
            if (SelectedTab == tab)
                SelectedTab = Tabs.Count > 0 ? Tabs[Tabs.Count - 1] : null;
        }
    }

    public enum ZcanTabType
    {
        Oscilloscope,
        DataPanel
    }

    public class ZcanTabItem : ObservableObject
    {
        private int _id;
        private string _title;
        private ZcanTabType _tabType;
        private object _content;
        private bool _isClosable = true;

        public int Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ZcanTabType TabType { get => _tabType; set => SetProperty(ref _tabType, value); }
        public object Content { get => _content; set => SetProperty(ref _content, value); }
        public bool IsClosable { get => _isClosable; set => SetProperty(ref _isClosable, value); }
    }
}