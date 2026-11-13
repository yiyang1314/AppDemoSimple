using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class MonitorPerformanceView : UserControl
    {
        private MonitorPerformanceViewModel _vm;
        private DispatcherTimer _updateTimer;
        private int _currentMaxX = 60;

        public MonitorPerformanceView()
        {
            InitializeComponent();
            _vm = new MonitorPerformanceViewModel();
            DataContext = _vm;
            Loaded += MonitorPerformanceView_Loaded;
            Unloaded += MonitorPerformanceView_Unloaded;
        }

        private void MonitorPerformanceView_Loaded(object sender, RoutedEventArgs e)
        {
            SetupChart();
            UpdateSparkLines();

            _vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(MonitorPerformanceViewModel.SelectedItem))
                {
                    UpdateChart();
                    UpdateSparkLines();
                }
            };

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _updateTimer.Tick += (s, args) =>
            {
                _vm.UpdateData();
                UpdateChart();
                UpdateSparkLines();
            };
            _updateTimer.Start();
        }

        private void ItemBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border == null) return;
            var item = border.DataContext as MonitorItem;
            if (item != null)
            {
                _vm.SelectItem(item);
                UpdateChart();
            }
        }

        private void MonitorPerformanceView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_updateTimer != null)
            {
                _updateTimer.Stop();
                _updateTimer = null;
            }
        }

        private void ConfigureChartAxes(int dataCount)
        {
            if (DetailChart == null) return;

            // Use Chinese-compatible font for axis name
            var chineseNamePaint = new SolidColorPaint
            {
                FontFamily = "Microsoft YaHei",
                Color = new SKColor(51, 51, 51)
            };

            // Dynamic X axis: show last 60 data points
            _currentMaxX = Math.Max(60, dataCount);
            int minX = Math.Max(0, _currentMaxX - 60);

            DetailChart.XAxes = new List<Axis>
            {
                new Axis
                {
                    MinLimit = minX,
                    MaxLimit = _currentMaxX,
                    MinStep = 10,
                    ShowSeparatorLines = true,
                    TextSize = 10,
                    Labeler = value =>
                    {
                        int secondsAgo = (int)((_currentMaxX - value) * 2);
                        return secondsAgo <= 0 ? "0秒" : $"{secondsAgo}秒";
                    }
                }
            };

            // Y axis with percentage range
            DetailChart.YAxes = new List<Axis>
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                    MinStep = 20,
                    ShowSeparatorLines = true,
                    TextSize = 10,
                    Name = "% 利用率",
                    NamePaint = chineseNamePaint
                }
            };
        }

        private void SetupChart()
        {
            if (DetailChart == null) return;

            int dataCount = _vm.SelectedItem?.DetailValues?.Count ?? 60;
            ConfigureChartAxes(dataCount);

            DetailChart.Series = new List<ISeries>();

            if (_vm.SelectedItem?.DetailSeries == null) return;

            _vm.SelectedItem.DetailSeries.Name = _vm.SelectedItem.Name;
            DetailChart.Series = new List<ISeries> { _vm.SelectedItem.DetailSeries };
        }

        private void UpdateChart()
        {
            if (DetailChart == null) return;

            int dataCount = _vm.SelectedItem?.DetailValues?.Count ?? 60;
            ConfigureChartAxes(dataCount);

            DetailChart.Series = new List<ISeries>();

            if (_vm.SelectedItem?.DetailSeries == null) return;

            _vm.SelectedItem.DetailSeries.Name = _vm.SelectedItem.Name;
            DetailChart.Series = new List<ISeries> { _vm.SelectedItem.DetailSeries };
        }

        private void UpdateSparkLines()
        {
            foreach (var item in _vm.MonitorItems)
            {
                item.NotifySparkChanged();
            }
        }
    }
}