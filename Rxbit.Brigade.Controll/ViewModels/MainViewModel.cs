using System.Collections.ObjectModel;
using System.Windows.Input;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string _title = "仪表板";
        private object _currentView;

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public object CurrentView
        {
            get { return _currentView; }
            set { SetProperty(ref _currentView, value); }
        }
    }
}
