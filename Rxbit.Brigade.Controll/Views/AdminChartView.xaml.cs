using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class AdminChartView : UserControl
    {
        private AdminChartViewModel _vm;
        private const double MaxY = 3500;
        private const double PaddingLeft = 10;
        private const double PaddingRight = 10;
        private const double PaddingTop = 10;
        private const double PaddingBottom = 10;

        public AdminChartView()
        {
            InitializeComponent();
            _vm = new AdminChartViewModel();
            DataContext = _vm;
            Loaded += AdminChartView_Loaded;
        }

        private void AdminChartView_Loaded(object sender, RoutedEventArgs e)
        {
            DrawChart();
        }

        private void ChartGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded) DrawChart();
        }

        private void DrawChart()
        {
            if (_vm == null || _vm.Series == null) return;

            ChartCanvas.Children.Clear();
            YAxisPanel.Children.Clear();
            XAxisPanel.Children.Clear();

            double width = ChartCanvas.ActualWidth;
            double height = ChartCanvas.ActualHeight;

            if (width < 50 || height < 50)
            {
                width = 600;
                height = 300;
            }

            double plotWidth = width - PaddingLeft - PaddingRight;
            double plotHeight = height - PaddingTop - PaddingBottom;
            int pointCount = _vm.XAxisLabels.Count;
            double stepX = plotWidth / (pointCount - 1);

            for (int i = 0; i < _vm.YAxisTicks.Count; i++)
            {
                double tick = _vm.YAxisTicks[i];
                double y = PaddingTop + plotHeight - (tick / MaxY) * plotHeight;

                var line = new Line
                {
                    X1 = PaddingLeft,
                    Y1 = y,
                    X2 = width - PaddingRight,
                    Y2 = y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection { 4, 4 }
                };
                ChartCanvas.Children.Add(line);

                var label = new TextBlock
                {
                    Text = tick.ToString("N0"),
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var canvas = new Canvas { Width = 35, Height = 16 };
                Canvas.SetLeft(label, 2);
                Canvas.SetTop(label, y - 8);
                canvas.Children.Add(label);
                YAxisPanel.Children.Add(canvas);
            }

            var itemsControl = new ItemsControl();
            var panel = new WrapPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < pointCount; i++)
            {
                var tb = new TextBlock
                {
                    Text = _vm.XAxisLabels[i],
                    FontSize = 11,
                    Foreground = Brushes.Gray,
                    Width = stepX,
                    TextAlignment = TextAlignment.Center
                };
                panel.Children.Add(tb);
            }
            XAxisPanel.Children.Add(panel);

            foreach (var series in _vm.Series)
            {
                var color = ParseColor(series.Color);

                var polyline = new Polyline
                {
                    Stroke = color,
                    StrokeThickness = 2,
                    StrokeLineJoin = PenLineJoin.Round
                };

                var points = new PointCollection();
                for (int i = 0; i < series.Data.Count && i < pointCount; i++)
                {
                    double x = PaddingLeft + i * stepX;
                    double yVal = Math.Min(series.Data[i], MaxY);
                    double y = PaddingTop + plotHeight - (yVal / MaxY) * plotHeight;
                    points.Add(new Point(x, y));

                    var dot = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = color,
                        Stroke = Brushes.White,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(dot, x - 3);
                    Canvas.SetTop(dot, y - 3);
                    ChartCanvas.Children.Add(dot);
                }
                polyline.Points = points;
                ChartCanvas.Children.Add(polyline);
            }
        }

        private SolidColorBrush ParseColor(string hex)
        {
            if (hex.StartsWith("#") && hex.Length == 7)
            {
                byte r = Convert.ToByte(hex.Substring(1, 2), 16);
                byte g = Convert.ToByte(hex.Substring(3, 2), 16);
                byte b = Convert.ToByte(hex.Substring(5, 2), 16);
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            return Brushes.Blue;
        }
    }
}
