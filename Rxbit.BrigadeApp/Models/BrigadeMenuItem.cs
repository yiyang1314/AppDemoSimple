using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Controls;
using System;

namespace Rxbit.BrigadeApp.Models
{
    public class BrigadeMenuItem : ViewModelBase
    {
        public Type WinType { get; set; }
        public WindowBase Win { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsVisible { get; set; }

        public bool IsEnabled { get; set; }
    }
}
