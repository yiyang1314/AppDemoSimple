using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class AdminChartSeries : ObservableObject
    {
        private string _name;
        private string _color;
        private ObservableCollection<double> _data;

        public string Name { get => _name; set { SetProperty(ref _name, value); } }
        public string Color { get => _color; set { SetProperty(ref _color, value); } }
        public ObservableCollection<double> Data { get => _data; set { SetProperty(ref _data, value); } }
    }

    public class AdminChartViewModel : ObservableObject
    {
        private ObservableCollection<string> _xAxisLabels;
        private ObservableCollection<AdminChartSeries> _series;
        private ObservableCollection<double> _yAxisTicks;

        public ObservableCollection<string> XAxisLabels { get => _xAxisLabels; set { SetProperty(ref _xAxisLabels, value); } }
        public ObservableCollection<AdminChartSeries> Series { get => _series; set { SetProperty(ref _series, value); } }
        public ObservableCollection<double> YAxisTicks { get => _yAxisTicks; set { SetProperty(ref _yAxisTicks, value); } }

        public AdminChartViewModel()
        {
            XAxisLabels = new ObservableCollection<string> { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            YAxisTicks = new ObservableCollection<double> { 0, 500, 1000, 1500, 2000, 2500, 3000 };

            Series = new ObservableCollection<AdminChartSeries>
            {
                new AdminChartSeries
                {
                    Name = "邮件营销",
                    Color = "#5B8FF9",
                    Data = new ObservableCollection<double> { 820, 932, 901, 934, 1290, 1330, 1320 }
                },
                new AdminChartSeries
                {
                    Name = "联盟广告",
                    Color = "#5AD8A6",
                    Data = new ObservableCollection<double> { 620, 732, 791, 834, 1090, 1330, 1220 }
                },
                new AdminChartSeries
                {
                    Name = "视频广告",
                    Color = "#FFAB2B",
                    Data = new ObservableCollection<double> { 420, 532, 601, 634, 790, 830, 920 }
                },
                new AdminChartSeries
                {
                    Name = "直接访问",
                    Color = "#5D7092",
                    Data = new ObservableCollection<double> { 320, 432, 501, 534, 690, 730, 820 }
                },
                new AdminChartSeries
                {
                    Name = "搜索引擎",
                    Color = "#F6BD16",
                    Data = new ObservableCollection<double> { 220, 332, 401, 434, 590, 630, 720 }
                }
            };
        }
    }
}
