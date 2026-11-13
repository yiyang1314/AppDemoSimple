using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Rxbit.BrigadeChm.Models
{
    public class HelpSection : ObservableObject
    {
        private string _id;
        private string _title;
        private string _description;
        private int _sortOrder;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public int SortOrder { get => _sortOrder; set => SetProperty(ref _sortOrder, value); }
        public ObservableCollection<HelpTopic> Topics { get; set; } = new ObservableCollection<HelpTopic>();
        public ObservableCollection<HelpSection> SubSections { get; set; } = new ObservableCollection<HelpSection>();
    }
}
