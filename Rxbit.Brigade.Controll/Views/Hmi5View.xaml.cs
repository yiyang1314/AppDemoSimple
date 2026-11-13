using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Rxbit.Brigade.Controll.ViewModels;

namespace Rxbit.Brigade.Controll.Views
{
    public partial class Hmi5View : UserControl
    {
        private readonly Hmi5ViewModel _vm;

        public Hmi5View()
        {
            InitializeComponent();
            _vm = new Hmi5ViewModel();
            DataContext = _vm;
        }

        private void RadarCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawRadar();
        }

        private void DrawRadar()
        {
            if (_vm == null || _vm.RadarLabels == null || _vm.RadarValues == null) return;

            RadarCanvas.Children.Clear();

            var w = RadarCanvas.ActualWidth;
            var h = RadarCanvas.ActualHeight;
            if (w < 20 || h < 20) return;

            double cx = w / 2;
            double cy = h / 2;
            double radius = Math.Min(w, h) / 2 - 50;
            int n = _vm.RadarLabels.Count;
            int levels = 5;

            if (n < 3) n = 5;

            var labels = new[] { "设置1", "设置5", "设置4", "设置3", "设置2" };

            // 绘制多层多边形背景
            for (int l = levels; l >= 1; l--)
            {
                double r = radius * l / levels;
                Polygon poly = new Polygon
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(60, 100, 150, 255)),
                    StrokeThickness = 1,
                    Fill = l == levels
                        ? new SolidColorBrush(Color.FromArgb(8, 100, 150, 255))
                        : null
                };
                for (int i = 0; i < n; i++)
                {
                    double angle = -Math.PI / 2 + (2 * Math.PI * i) / n;
                    double x = cx + r * Math.Cos(angle);
                    double y = cy + r * Math.Sin(angle);
                    poly.Points.Add(new Point(x, y));
                }
                RadarCanvas.Children.Add(poly);
            }

            // 绘制轴线
            for (int i = 0; i < n; i++)
            {
                double angle = -Math.PI / 2 + (2 * Math.PI * i) / n;
                Line line = new Line
                {
                    X1 = cx,
                    Y1 = cy,
                    X2 = cx + radius * Math.Cos(angle),
                    Y2 = cy + radius * Math.Sin(angle),
                    Stroke = new SolidColorBrush(Color.FromArgb(80, 100, 150, 255)),
                    StrokeThickness = 1
                };
                RadarCanvas.Children.Add(line);
            }

            // 绘制数值刻度
            string[] ticks = { "50", "100", "140", "180", "220" };
            for (int i = 0; i < n; i++)
            {
                double angle = -Math.PI / 2 + (2 * Math.PI * i) / n;
                // 主刻度
                double xOut = cx + (radius + 22) * Math.Cos(angle);
                double yOut = cy + (radius + 22) * Math.Sin(angle);
                TextBlock lbl = new TextBlock
                {
                    Text = labels[i % labels.Length],
                    Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(lbl, xOut - 24);
                Canvas.SetTop(lbl, yOut - 8);
                RadarCanvas.Children.Add(lbl);

                // 中间刻度（设置轴边上的数值：150, 130, 140, 200, 140）
                var axisVals = new[] { "150", "130", "140", "200", "140" };
                double xMid = cx + (radius * 0.82) * Math.Cos(angle);
                double yMid = cy + (radius * 0.82) * Math.Sin(angle);
                TextBlock tv = new TextBlock
                {
                    Text = axisVals[i % axisVals.Length],
                    Foreground = new SolidColorBrush(Color.FromArgb(220, 120, 200, 255)),
                    FontSize = 10
                };
                Canvas.SetLeft(tv, xMid - 14);
                Canvas.SetTop(tv, yMid - 7);
                RadarCanvas.Children.Add(tv);

                // 轴上的小刻度：100, 90, 130, 160
                var innerVals = new[] { "100", "90", "100", "160", "100" };
                double xMid2 = cx + (radius * 0.58) * Math.Cos(angle);
                double yMid2 = cy + (radius * 0.58) * Math.Sin(angle);
                TextBlock tv2 = new TextBlock
                {
                    Text = innerVals[i % innerVals.Length],
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 120, 200, 255)),
                    FontSize = 9
                };
                Canvas.SetLeft(tv2, xMid2 - 10);
                Canvas.SetTop(tv2, yMid2 - 6);
                RadarCanvas.Children.Add(tv2);
            }

            // 绘制数据多边形 (外层蓝色：120,130,140,200,140)
            double[] outerVals = { 120, 130, 140, 200, 140 };
            double max = 220;
            Polygon dataOuter = new Polygon
            {
                Stroke = new SolidColorBrush(Color.FromArgb(200, 80, 180, 255)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(40, 80, 180, 255))
            };
            for (int i = 0; i < n; i++)
            {
                double angle = -Math.PI / 2 + (2 * Math.PI * i) / n;
                double r = radius * (outerVals[i] / max);
                double x = cx + r * Math.Cos(angle);
                double y = cy + r * Math.Sin(angle);
                dataOuter.Points.Add(new Point(x, y));
                // 数据点
                Ellipse dot = new Ellipse
                {
                    Width = 6,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(120, 200, 255))
                };
                Canvas.SetLeft(dot, x - 3);
                Canvas.SetTop(dot, y - 3);
                RadarCanvas.Children.Add(dot);
            }
            RadarCanvas.Children.Add(dataOuter);

            // 内层数据多边形 (青色：100,90,130,160,100)
            double[] innerValsData = { 100, 90, 130, 160, 100 };
            Polygon dataInner = new Polygon
            {
                Stroke = new SolidColorBrush(Color.FromArgb(220, 100, 220, 220)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(30, 100, 220, 220))
            };
            for (int i = 0; i < n; i++)
            {
                double angle = -Math.PI / 2 + (2 * Math.PI * i) / n;
                double r = radius * (innerValsData[i] / max);
                double x = cx + r * Math.Cos(angle);
                double y = cy + r * Math.Sin(angle);
                dataInner.Points.Add(new Point(x, y));
                Ellipse dot = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = new SolidColorBrush(Color.FromRgb(100, 220, 220))
                };
                Canvas.SetLeft(dot, x - 2.5);
                Canvas.SetTop(dot, y - 2.5);
                RadarCanvas.Children.Add(dot);
            }
            RadarCanvas.Children.Add(dataInner);

            // 中心
            Ellipse center = new Ellipse
            {
                Width = 4,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255))
            };
            Canvas.SetLeft(center, cx - 2);
            Canvas.SetTop(center, cy - 2);
            RadarCanvas.Children.Add(center);
        }
    }
}
