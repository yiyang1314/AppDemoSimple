using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Rxbit.BrigadeChm.Models
{
    public class HelpTopic : ObservableObject
    {
        private string _id;
        private string _title;
        private string _fileName;
        private string _htmlContent;
        private string _keywords;
        private int _sortOrder;
        private HelpSection _parentSection;

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string FileName { get => _fileName; set => SetProperty(ref _fileName, value); }
        public string HtmlContent { get => _htmlContent; set => SetProperty(ref _htmlContent, value); }
        public string Keywords { get => _keywords; set => SetProperty(ref _keywords, value); }
        public int SortOrder { get => _sortOrder; set => SetProperty(ref _sortOrder, value); }
        public HelpSection ParentSection { get => _parentSection; set => SetProperty(ref _parentSection, value); }
    }
}
