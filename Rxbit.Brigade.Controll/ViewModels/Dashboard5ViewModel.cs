using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class Dashboard5ViewModel : ObservableObject
    {
        private ObservableCollection<StatCardItem> _statCards;
        private ObservableCollection<ChartLinePoint> _areaChartData;
        private ObservableCollection<ChartBarItem> _pieChartData;

        public ObservableCollection<StatCardItem> StatCards
        {
            get { return _statCards; }
            set { SetProperty(ref _statCards, value); }
        }

        public ObservableCollection<ChartLinePoint> AreaChartData
        {
            get { return _areaChartData; }
            set { SetProperty(ref _areaChartData, value); }
        }

        public ObservableCollection<ChartBarItem> PieChartData
        {
            get { return _pieChartData; }
            set { SetProperty(ref _pieChartData, value); }
        }

        public Dashboard5ViewModel()
        {
            StatCards = new ObservableCollection<StatCardItem>
            {
                new StatCardItem { Title = "用户量", Value = "2,000", Change = "8.2%", IsUp = true, Color = "#1890FF",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 10 }, new ChartLinePoint { X = 1, Y = 25 },
                        new ChartLinePoint { X = 2, Y = 20 }, new ChartLinePoint { X = 3, Y = 45 },
                        new ChartLinePoint { X = 4, Y = 40 }, new ChartLinePoint { X = 5, Y = 60 }
                    } },
                new StatCardItem { Title = "访问量", Value = "20,000", Change = "12.5%", IsUp = true, Color = "#52C41A",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 15 }, new ChartLinePoint { X = 1, Y = 30 },
                        new ChartLinePoint { X = 2, Y = 25 }, new ChartLinePoint { X = 3, Y = 50 },
                        new ChartLinePoint { X = 4, Y = 55 }, new ChartLinePoint { X = 5, Y = 75 }
                    } },
                new StatCardItem { Title = "下载量", Value = "8,000", Change = "-5.3%", IsUp = false, Color = "#FAAD14",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 40 }, new ChartLinePoint { X = 1, Y = 30 },
                        new ChartLinePoint { X = 2, Y = 35 }, new ChartLinePoint { X = 3, Y = 25 },
                        new ChartLinePoint { X = 4, Y = 20 }, new ChartLinePoint { X = 5, Y = 15 }
                    } },
                new StatCardItem { Title = "使用量", Value = "5,000", Change = "+3.8%", IsUp = true, Color = "#722ED1",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 20 }, new ChartLinePoint { X = 1, Y = 35 },
                        new ChartLinePoint { X = 2, Y = 45 }, new ChartLinePoint { X = 3, Y = 30 },
                        new ChartLinePoint { X = 4, Y = 50 }, new ChartLinePoint { X = 5, Y = 55 }
                    } },
            };

            AreaChartData = new ObservableCollection<ChartLinePoint>
            {
                new ChartLinePoint { X = 0, Y = 5 },
                new ChartLinePoint { X = 1, Y = 15 },
                new ChartLinePoint { X = 2, Y = 30 },
                new ChartLinePoint { X = 3, Y = 50 },
                new ChartLinePoint { X = 4, Y = 70 },
                new ChartLinePoint { X = 5, Y = 85 },
                new ChartLinePoint { X = 6, Y = 65 },
                new ChartLinePoint { X = 7, Y = 45 },
                new ChartLinePoint { X = 8, Y = 30 },
                new ChartLinePoint { X = 9, Y = 20 },
                new ChartLinePoint { X = 10, Y = 15 },
                new ChartLinePoint { X = 11, Y = 10 },
            };

            PieChartData = new ObservableCollection<ChartBarItem>
            {
                new ChartBarItem { Label = "直接", Value = 35, Color = "#1890FF" },
                new ChartBarItem { Label = "邮件", Value = 25, Color = "#52C41A" },
                new ChartBarItem { Label = "广告", Value = 20, Color = "#FAAD14" },
                new ChartBarItem { Label = "视频", Value = 12, Color = "#722ED1" },
                new ChartBarItem { Label = "其它", Value = 8, Color = "#13C2C2" },
            };
        }
    }
}
