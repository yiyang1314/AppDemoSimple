using Rxbit.Brigade.Models.EnumEntitys;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Rxbit.BrigadeApp.Models
{
    public class MenuItemModel : INotifyPropertyChanged
    {
        private bool _isChecked;
        private bool _isEnabled = true;

        public string Header { get; set; }
        public MenuType MenuType { get; set; }
        public string Icon { get; set; }
        public ICommand Command { get; set; }
        public bool IsCheckable { get; set; }
        public bool IsSeparator { get; set; } = false;
        public List<MenuItemModel> Children { get; set; } = new List<MenuItemModel>();
        public Action<MenuItemModel> ClickAction { get; set; }
        public string Shortcut { get; set; }
        public Type WindowType { get; set; }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
