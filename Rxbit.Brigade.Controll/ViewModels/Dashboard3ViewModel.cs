using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class Dashboard3ViewModel : ObservableObject
    {
        private ObservableCollection<StatCardItem> _statCards;
        private ObservableCollection<ChartLinePoint> _areaChartData;
        private ObservableCollection<ChartLinePoint> _areaChartData2;

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

        public ObservableCollection<ChartLinePoint> AreaChartData2
        {
            get { return _areaChartData2; }
            set { SetProperty(ref _areaChartData2, value); }
        }

        public Dashboard3ViewModel()
        {
            StatCards = new ObservableCollection<StatCardItem>
            {
                new StatCardItem { Title = "访问量", Value = "49,917", Change = "65.83%", IsUp = true, Color = "#1890FF",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 20 }, new ChartLinePoint { X = 1, Y = 35 },
                        new ChartLinePoint { X = 2, Y = 45 }, new ChartLinePoint { X = 3, Y = 30 },
                        new ChartLinePoint { X = 4, Y = 50 }, new ChartLinePoint { X = 5, Y = 60 }
                    } },
                new StatCardItem { Title = "销售额", Value = "¥57,127", Change = "17.60%", IsUp = true, Color = "#52C41A",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 15 }, new ChartLinePoint { X = 1, Y = 30 },
                        new ChartLinePoint { X = 2, Y = 25 }, new ChartLinePoint { X = 3, Y = 40 },
                        new ChartLinePoint { X = 4, Y = 35 }, new ChartLinePoint { X = 5, Y = 55 }
                    } },
                new StatCardItem { Title = "订单量", Value = "91,740", Change = "76.54%", IsUp = true, Color = "#FAAD14",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 25 }, new ChartLinePoint { X = 1, Y = 40 },
                        new ChartLinePoint { X = 2, Y = 35 }, new ChartLinePoint { X = 3, Y = 55 },
                        new ChartLinePoint { X = 4, Y = 48 }, new ChartLinePoint { X = 5, Y = 70 }
                    } },
                new StatCardItem { Title = "成交额", Value = "¥93,114", Change = "88.05%", IsUp = true, Color = "#FF4D4F",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 30 }, new ChartLinePoint { X = 1, Y = 50 },
                        new ChartLinePoint { X = 2, Y = 40 }, new ChartLinePoint { X = 3, Y = 60 },
                        new ChartLinePoint { X = 4, Y = 55 }, new ChartLinePoint { X = 5, Y = 80 }
                    } },
            };

            AreaChartData = new ObservableCollection<ChartLinePoint>
            {
                new ChartLinePoint { X = 0, Y = 5 },
                new ChartLinePoint { X = 1, Y = 8 },
                new ChartLinePoint { X = 2, Y = 15 },
                new ChartLinePoint { X = 3, Y = 25 },
                new ChartLinePoint { X = 4, Y = 45 },
                new ChartLinePoint { X = 5, Y = 60 },
                new ChartLinePoint { X = 6, Y = 75 },
                new ChartLinePoint { X = 7, Y = 85 },
                new ChartLinePoint { X = 8, Y = 70 },
                new ChartLinePoint { X = 9, Y = 55 },
                new ChartLinePoint { X = 10, Y = 40 },
                new ChartLinePoint { X = 11, Y = 30 },
                new ChartLinePoint { X = 12, Y = 20 },
            };

            AreaChartData2 = new ObservableCollection<ChartLinePoint>
            {
                new ChartLinePoint { X = 0, Y = 3 },
                new ChartLinePoint { X = 1, Y = 5 },
                new ChartLinePoint { X = 2, Y = 10 },
                new ChartLinePoint { X = 3, Y = 20 },
                new ChartLinePoint { X = 4, Y = 35 },
                new ChartLinePoint { X = 5, Y = 55 },
                new ChartLinePoint { X = 6, Y = 70 },
                new ChartLinePoint { X = 7, Y = 80 },
                new ChartLinePoint { X = 8, Y = 65 },
                new ChartLinePoint { X = 9, Y = 48 },
                new ChartLinePoint { X = 10, Y = 32 },
                new ChartLinePoint { X = 11, Y = 22 },
                new ChartLinePoint { X = 12, Y = 12 },
            };
        }
    }
}
