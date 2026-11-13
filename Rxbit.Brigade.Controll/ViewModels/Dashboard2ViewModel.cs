using System.Collections.ObjectModel;
using Rxbit.Brigade.Controll.Mvvm;

namespace Rxbit.Brigade.Controll.ViewModels
{
    public class ChartBarItem : ObservableObject
    {
        private string _label;
        private double _value;
        private string _color;

        public string Label { get { return _label; } set { SetProperty(ref _label, value); } }
        public double Value { get { return _value; } set { SetProperty(ref _value, value); } }
        public string Color { get { return _color; } set { SetProperty(ref _color, value); } }
    }

    public class ChartLinePoint : ObservableObject
    {
        private double _x;
        private double _y;

        public double X { get { return _x; } set { SetProperty(ref _x, value); } }
        public double Y { get { return _y; } set { SetProperty(ref _y, value); } }
    }

    public class StatCardItem : ObservableObject
    {
        private string _title;
        private string _value;
        private string _change;
        private bool _isUp;
        private string _color;
        private ObservableCollection<ChartLinePoint> _sparkPoints;

        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        public string Value { get { return _value; } set { SetProperty(ref _value, value); } }
        public string Change { get { return _change; } set { SetProperty(ref _change, value); } }
        public bool IsUp { get { return _isUp; } set { SetProperty(ref _isUp, value); } }
        public string Color { get { return _color; } set { SetProperty(ref _color, value); } }
        public ObservableCollection<ChartLinePoint> SparkPoints { get { return _sparkPoints; } set { SetProperty(ref _sparkPoints, value); } }
    }

    public class Dashboard2ViewModel : ObservableObject
    {
        private ObservableCollection<ChartBarItem> _salesBarData;
        private ObservableCollection<StatCardItem> _statCards;
        private ObservableCollection<ChartLinePoint> _trendLineData;
        private ObservableCollection<ChartBarItem> _annualBarData;
        private ObservableCollection<ChartBarItem> _recentActivity;
        private double _donutValue;

        public ObservableCollection<ChartBarItem> SalesBarData
        {
            get { return _salesBarData; }
            set { SetProperty(ref _salesBarData, value); }
        }

        public ObservableCollection<StatCardItem> StatCards
        {
            get { return _statCards; }
            set { SetProperty(ref _statCards, value); }
        }

        public ObservableCollection<ChartLinePoint> TrendLineData
        {
            get { return _trendLineData; }
            set { SetProperty(ref _trendLineData, value); }
        }

        public ObservableCollection<ChartBarItem> AnnualBarData
        {
            get { return _annualBarData; }
            set { SetProperty(ref _annualBarData, value); }
        }

        public ObservableCollection<ChartBarItem> RecentActivity
        {
            get { return _recentActivity; }
            set { SetProperty(ref _recentActivity, value); }
        }

        public double DonutValue
        {
            get { return _donutValue; }
            set { SetProperty(ref _donutValue, value); }
        }

        public Dashboard2ViewModel()
        {
            SalesBarData = new ObservableCollection<ChartBarItem>
            {
                new ChartBarItem { Label = "一月", Value = 40, Color = "#1890FF" },
                new ChartBarItem { Label = "二月", Value = 55, Color = "#1890FF" },
                new ChartBarItem { Label = "三月", Value = 45, Color = "#1890FF" },
                new ChartBarItem { Label = "四月", Value = 70, Color = "#1890FF" },
                new ChartBarItem { Label = "五月", Value = 90, Color = "#1890FF" },
            };

            StatCards = new ObservableCollection<StatCardItem>
            {
                new StatCardItem { Title = "销售额", Value = "¥500,458", Change = "+12.3%", IsUp = true, Color = "#F5A623",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 30 }, new ChartLinePoint { X = 1, Y = 50 },
                        new ChartLinePoint { X = 2, Y = 40 }, new ChartLinePoint { X = 3, Y = 60 },
                        new ChartLinePoint { X = 4, Y = 55 }, new ChartLinePoint { X = 5, Y = 80 }
                    } },
                new StatCardItem { Title = "净利润", Value = "¥130,580", Change = "+8.7%", IsUp = true, Color = "#7CC7DA",
                    SparkPoints = new ObservableCollection<ChartLinePoint>
                    {
                        new ChartLinePoint { X = 0, Y = 20 }, new ChartLinePoint { X = 1, Y = 35 },
                        new ChartLinePoint { X = 2, Y = 30 }, new ChartLinePoint { X = 3, Y = 45 },
                        new ChartLinePoint { X = 4, Y = 40 }, new ChartLinePoint { X = 5, Y = 60 }
                    } },
            };

            TrendLineData = new ObservableCollection<ChartLinePoint>
            {
                new ChartLinePoint { X = 0, Y = 20 }, new ChartLinePoint { X = 1, Y = 45 },
                new ChartLinePoint { X = 2, Y = 35 }, new ChartLinePoint { X = 3, Y = 60 },
                new ChartLinePoint { X = 4, Y = 50 }, new ChartLinePoint { X = 5, Y = 75 },
                new ChartLinePoint { X = 6, Y = 65 }
            };

            AnnualBarData = new ObservableCollection<ChartBarItem>
            {
                new ChartBarItem { Label = "1月", Value = 60, Color = "#2F54EB" },
                new ChartBarItem { Label = "2月", Value = 75, Color = "#2F54EB" },
                new ChartBarItem { Label = "3月", Value = 55, Color = "#2F54EB" },
                new ChartBarItem { Label = "4月", Value = 85, Color = "#2F54EB" },
                new ChartBarItem { Label = "5月", Value = 70, Color = "#2F54EB" },
                new ChartBarItem { Label = "6月", Value = 95, Color = "#2F54EB" },
            };

            RecentActivity = new ObservableCollection<ChartBarItem>
            {
                new ChartBarItem { Label = "新订单 #30291", Value = 1, Color = "#1890FF" },
                new ChartBarItem { Label = "退款申请 #12945", Value = 2, Color = "#FA8C16" },
                new ChartBarItem { Label = "库存更新", Value = 3, Color = "#52C41A" },
            };

            DonutValue = 75;
        }
    }
}
