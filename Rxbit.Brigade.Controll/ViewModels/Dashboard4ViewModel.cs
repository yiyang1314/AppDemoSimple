using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class WorkbenchItem : ObservableObject
    {
        private string _name;
        private string _description;
        private string _date;
        private string _color;
        private string _icon;

        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        public string Description { get { return _description; } set { SetProperty(ref _description, value); } }
        public string Date { get { return _date; } set { SetProperty(ref _date, value); } }
        public string Color { get { return _color; } set { SetProperty(ref _color, value); } }
        public string Icon { get { return _icon; } set { SetProperty(ref _icon, value); } }
    }

    public class ActivityItem : ObservableObject
    {
        private string _text;
        private string _time;
        private string _color;

        public string Text { get { return _text; } set { SetProperty(ref _text, value); } }
        public string Time { get { return _time; } set { SetProperty(ref _time, value); } }
        public string Color { get { return _color; } set { SetProperty(ref _color, value); } }
    }

    public class Dashboard4ViewModel : ObservableObject
    {
        private ObservableCollection<WorkbenchItem> _items;
        private ObservableCollection<ActivityItem> _activities;

        public ObservableCollection<WorkbenchItem> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        public ObservableCollection<ActivityItem> Activities
        {
            get { return _activities; }
            set { SetProperty(ref _activities, value); }
        }

        public Dashboard4ViewModel()
        {
            Items = new ObservableCollection<WorkbenchItem>
            {
                new WorkbenchItem { Name = "Github", Description = "是一个开源及私有软件项目的托管平台。", Date = "2021-07-04", Color = "#24292E", Icon = "G" },
                new WorkbenchItem { Name = "Vue", Description = "渐进式 JavaScript 框架。", Date = "2021-07-04", Color = "#4FC08D", Icon = "V" },
                new WorkbenchItem { Name = "Html5", Description = "HTML5 是网页的下一代标准。", Date = "2021-07-04", Color = "#E44D26", Icon = "H" },
                new WorkbenchItem { Name = "Angular", Description = "现代 Web 平台，可自助编译。", Date = "2021-07-04", Color = "#DD0031", Icon = "A" },
                new WorkbenchItem { Name = "React", Description = "用于构建用户界面的 JavaScript 库。", Date = "2021-07-04", Color = "#61DAFB", Icon = "R" },
                new WorkbenchItem { Name = "Js", Description = "JavaScript 是一门动态语言，而不是想出来的。", Date = "2021-07-04", Color = "#F7DF1E", Icon = "JS" },
            };

            Activities = new ObservableCollection<ActivityItem>
            {
                new ActivityItem { Text = "Ah Jung 刚才工作台页面憧憬了一些，谈论能赢！", Time = "07-04 23:13:10", Color = "#1890FF" },
                new ActivityItem { Text = "Ah Jung 在开源项目 naive-ui-admin 中创建了项目", Time = "07-04 23:08:08", Color = "#52C41A" },
                new ActivityItem { Text = "#赞美风情， naive-ui-admin 触发了一个 bug，抽时间看看吧！", Time = "07-04 22:38:16", Color = "#FAAD14" },
                new ActivityItem { Text = "技术部那位童鞋，再次警告，不要摸鱼，不要摸鱼啦！", Time = "07-04 06:16:04", Color = "#722ED1" },
                new ActivityItem { Text = "上班不摸鱼，和咸鱼有什么区别（这话真不是我说的呢）！", Time = "07-03 22:44:03", Color = "#FA8C16" },
                new ActivityItem { Text = "页面切换其实也支持缓存，只加了过渡效果，看起来像是重新渲染了", Time = "07-03 22:30:00", Color = "#13C2C2" },
            };
        }
    }
}
