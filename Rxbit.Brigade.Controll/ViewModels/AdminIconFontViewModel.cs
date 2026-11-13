using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class AdminIconItem : ObservableObject
    {
        private string _unicode;
        private string _name;
        private string _hex;

        public string Unicode { get => _unicode; set { SetProperty(ref _unicode, value); } }
        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Hex { get => _hex; set { SetProperty(ref _hex, value); } }
    }

    public class AdminIconFontViewModel : ObservableObject
    {
        private ObservableCollection<AdminIconItem> _icons;

        public ObservableCollection<AdminIconItem> Icons { get => _icons; set { SetProperty(ref _icons, value); } }

        public AdminIconFontViewModel()
        {
            Icons = new ObservableCollection<AdminIconItem>
            {
                new AdminIconItem { Unicode = "&#xe696;", Name = "all", Hex = "&#xe696;" },
                new AdminIconItem { Unicode = "&#xe697;", Name = "back", Hex = "&#xe697;" },
                new AdminIconItem { Unicode = "&#xe698;", Name = "cart", Hex = "&#xe698;" },
                new AdminIconItem { Unicode = "&#xe699;", Name = "Category", Hex = "&#xe699;" },
                new AdminIconItem { Unicode = "&#xe69a;", Name = "close", Hex = "&#xe69a;" },
                new AdminIconItem { Unicode = "&#xe69b;", Name = "comments", Hex = "&#xe69b;" },
                new AdminIconItem { Unicode = "&#xe69c;", Name = "cry", Hex = "&#xe69c;" },
                new AdminIconItem { Unicode = "&#xe69d;", Name = "delete", Hex = "&#xe69d;" },
                new AdminIconItem { Unicode = "&#xe69e;", Name = "edit", Hex = "&#xe69e;" },
                new AdminIconItem { Unicode = "&#xe69f;", Name = "email", Hex = "&#xe69f;" },
                new AdminIconItem { Unicode = "&#xe6a0;", Name = "favorite", Hex = "&#xe6a0;" },
                new AdminIconItem { Unicode = "&#xe6a2;", Name = "form", Hex = "&#xe6a2;" },
                new AdminIconItem { Unicode = "&#xe6a3;", Name = "help", Hex = "&#xe6a3;" },
                new AdminIconItem { Unicode = "&#xe6a4;", Name = "information", Hex = "&#xe6a4;" },
                new AdminIconItem { Unicode = "&#xe6a5;", Name = "less", Hex = "&#xe6a5;" },
                new AdminIconItem { Unicode = "&#xe6a6;", Name = "more_unfold", Hex = "&#xe6a6;" },
                new AdminIconItem { Unicode = "&#xe6a7;", Name = "more", Hex = "&#xe6a7;" },
                new AdminIconItem { Unicode = "&#xe6a8;", Name = "pic", Hex = "&#xe6a8;" },
                new AdminIconItem { Unicode = "&#xe6aa;", Name = "QRCode", Hex = "&#xe6aa;" },
                new AdminIconItem { Unicode = "&#xe6ab;", Name = "refresh", Hex = "&#xe6ab;" },
                new AdminIconItem { Unicode = "&#xe6ac;", Name = "RFQ", Hex = "&#xe6ac;" },
            };
        }
    }
}
