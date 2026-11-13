using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;

namespace Rxbit.BrigadeApp.Controls
{
    public enum CoordinateSystemType
    {
        Cartesian,
        Radar,
        Grid,
        BirdsEye
    }

    public class TargetInfo
    {
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Distance { get; set; }
        public double Angle { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public Brush FillColor { get; set; } = Brushes.Red;
    }

    public class CoordinateCanvas : Canvas
    {
        #region 依赖属性

        public static readonly DependencyProperty CoordinateTypeProperty =
            DependencyProperty.Register("CoordinateType", typeof(CoordinateSystemType), typeof(CoordinateCanvas),
                new PropertyMetadata(CoordinateSystemType.Cartesian, OnCoordinateTypeChanged));

        public CoordinateSystemType CoordinateType
        {
            get => (CoordinateSystemType)GetValue(CoordinateTypeProperty);
            set => SetValue(CoordinateTypeProperty, value);
        }

        public static readonly DependencyProperty XRangeProperty =
            DependencyProperty.Register("XRange", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(8.0, OnRangeChanged));

        public double XRange
        {
            get => (double)GetValue(XRangeProperty);
            set => SetValue(XRangeProperty, value);
        }

        public static readonly DependencyProperty YRangeProperty =
            DependencyProperty.Register("YRange", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(60.0, OnRangeChanged));

        public double YRange
        {
            get => (double)GetValue(YRangeProperty);
            set => SetValue(YRangeProperty, value);
        }

        public static readonly DependencyProperty XScaleProperty =
            DependencyProperty.Register("XScale", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(30.0, OnScaleChanged));

        public double XScale
        {
            get => (double)GetValue(XScaleProperty);
            set => SetValue(XScaleProperty, value);
        }

        public static readonly DependencyProperty YScaleProperty =
            DependencyProperty.Register("YScale", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(5.0, OnScaleChanged));

        public double YScale
        {
            get => (double)GetValue(YScaleProperty);
            set => SetValue(YScaleProperty, value);
        }

        public static readonly DependencyProperty RadarRadiusProperty =
            DependencyProperty.Register("RadarRadius", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(50.0, OnRadarChanged));

        public double RadarRadius
        {
            get => (double)GetValue(RadarRadiusProperty);
            set => SetValue(RadarRadiusProperty, value);
        }

        public static readonly DependencyProperty RadarSectorsProperty =
            DependencyProperty.Register("RadarSectors", typeof(int), typeof(CoordinateCanvas),
                new PropertyMetadata(12, OnRadarChanged));

        public int RadarSectors
        {
            get => (int)GetValue(RadarSectorsProperty);
            set => SetValue(RadarSectorsProperty, value);
        }

        public static readonly DependencyProperty GridSizeProperty =
            DependencyProperty.Register("GridSize", typeof(double), typeof(CoordinateCanvas),
                new PropertyMetadata(4.0, OnGridChanged));

        public double GridSize
        {
            get => (double)GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly DependencyProperty TargetsProperty =
            DependencyProperty.Register("Targets", typeof(List<TargetInfo>), typeof(CoordinateCanvas),
                new PropertyMetadata(new List<TargetInfo>(), OnTargetsChanged));

        public List<TargetInfo> Targets
        {
            get => (List<TargetInfo>)GetValue(TargetsProperty);
            set => SetValue(TargetsProperty, value);
        }

        public static readonly DependencyProperty IsScanningProperty =
            DependencyProperty.Register("IsScanning", typeof(bool), typeof(CoordinateCanvas),
                new PropertyMetadata(false, OnScanningChanged));

        public bool IsScanning
        {
            get => (bool)GetValue(IsScanningProperty);
            set => SetValue(IsScanningProperty, value);
        }

        public static readonly DependencyProperty SelectedTargetProperty =
            DependencyProperty.Register("SelectedTarget", typeof(TargetInfo), typeof(CoordinateCanvas),
                new PropertyMetadata(null));

        public TargetInfo SelectedTarget
        {
            get => (TargetInfo)GetValue(SelectedTargetProperty);
            set => SetValue(SelectedTargetProperty, value);
        }

        public static readonly DependencyProperty SensorIdProperty =
            DependencyProperty.Register("SensorId", typeof(string), typeof(CoordinateCanvas),
                new PropertyMetadata("0x310"));

        public string SensorId
        {
            get => (string)GetValue(SensorIdProperty);
            set => SetValue(SensorIdProperty, value);
        }

        #endregion

        #region 私有字段

        private double _scale = 1.0;
        private Point _panOffset = new Point(0, 0);
        private Point _lastMousePos;
        private bool _isDragging;
        private bool _isPanning;
        private TargetInfo _hoverTarget;
        private const double InnerPadding = 50;

        #endregion

        #region 构造函数

        public CoordinateCanvas()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
            MouseLeave += OnMouseLeave;
            MouseWheel += OnMouseWheel;
            Background = Brushes.Black;
        }

        #endregion

        #region 事件处理

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Draw();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Draw();
        }

        private static void OnCoordinateTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnRadarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnTargetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        private static void OnScanningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CoordinateCanvas)d).Draw();
        }

        #endregion

        #region 鼠标事件

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            _lastMousePos = pos;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _hoverTarget = FindTargetAt(pos);
                if (_hoverTarget != null)
                {
                    SelectedTarget = _hoverTarget;
                    _isDragging = true;
                }
                else
                {
                    _isPanning = true;
                    SelectedTarget = null;
                }
                CaptureMouse();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            if (_isDragging && SelectedTarget != null)
            {
                var deltaX = (pos.X - _lastMousePos.X) / XScale / _scale;
                var deltaY = (pos.Y - _lastMousePos.Y) / YScale / _scale;

                SelectedTarget.X += deltaX;
                SelectedTarget.Y -= deltaY;
                SelectedTarget.Distance = Math.Sqrt(SelectedTarget.X * SelectedTarget.X + SelectedTarget.Y * SelectedTarget.Y);
                SelectedTarget.Angle = Math.Atan2(SelectedTarget.Y, SelectedTarget.X) * 180 / Math.PI;

                _lastMousePos = pos;
                Draw();
            }
            else if (_isPanning)
            {
                _panOffset.X += pos.X - _lastMousePos.X;
                _panOffset.Y += pos.Y - _lastMousePos.Y;
                _lastMousePos = pos;
                Draw();
            }
            else
            {
                _hoverTarget = FindTargetAt(pos);
                Cursor = _hoverTarget != null ? Cursors.Hand : Cursors.Arrow;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _isPanning = false;
            ReleaseMouseCapture();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isPanning = false;
            _hoverTarget = null;
            Cursor = Cursors.Arrow;
            ReleaseMouseCapture();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            _scale = Math.Max(0.1, Math.Min(5.0, _scale * zoomFactor));
            Draw();
            e.Handled = true;
        }

        #endregion

        #region 目标查找

        private TargetInfo FindTargetAt(Point pos)
        {
            foreach (var target in Targets)
            {
                var screenPos = WorldToScreen(target.X, target.Y);
                var distance = Math.Sqrt(Math.Pow(pos.X - screenPos.X, 2) + Math.Pow(pos.Y - screenPos.Y, 2));
                if (distance < 15)
                    return target;
            }
            return null;
        }

        #endregion

        #region 坐标转换

        private Point WorldToScreen(double x, double y)
        {
            switch (CoordinateType)
            {
                case CoordinateSystemType.Cartesian:
                case CoordinateSystemType.Grid:
                    double centerX = ActualWidth / 2 + _panOffset.X;
                    double centerY = ActualHeight / 2 + _panOffset.Y;
                    return new Point(
                        centerX + x * XScale * _scale,
                        centerY - y * YScale * _scale
                    );
                case CoordinateSystemType.Radar:
                    double centerXRadar = ActualWidth / 2 + _panOffset.X;
                    double centerYRadar = ActualHeight / 2 + _panOffset.Y;
                    double angleRad = y * Math.PI / 180;
                    double radius = x * XScale * _scale;
                    return new Point(
                        centerXRadar + radius * Math.Cos(angleRad),
                        centerYRadar - radius * Math.Sin(angleRad)
                    );
                case CoordinateSystemType.BirdsEye:
                    double originX = InnerPadding + _panOffset.X;
                    double originY = ActualHeight - InnerPadding + _panOffset.Y;
                    return new Point(
                        originX + x * XScale * _scale,
                        originY - y * YScale * _scale
                    );
                default:
                    return new Point(x, y);
            }
        }

        #endregion

        #region 绘制方法

        public void Draw()
        {
            Children.Clear();

            switch (CoordinateType)
            {
                case CoordinateSystemType.Cartesian:
                    DrawCartesianCoordinate();
                    break;
                case CoordinateSystemType.Radar:
                    DrawRadarCoordinate();
                    break;
                case CoordinateSystemType.Grid:
                    DrawGridCoordinate();
                    break;
                case CoordinateSystemType.BirdsEye:
                    DrawBirdsEyeCoordinate();
                    break;
            }

            DrawTargets();
        }

        private void DrawCartesianCoordinate()
        {
            double centerX = ActualWidth / 2 + _panOffset.X;
            double centerY = ActualHeight / 2 + _panOffset.Y;

            // 绘制网格
            double gridStep = 5;
            double gridOpacity = 0.3;

            for (double x = -XRange; x <= XRange; x += gridStep)
            {
                double screenX = centerX + x * XScale * _scale;
                if (screenX > 0 && screenX < ActualWidth)
                {
                    var line = new Line
                    {
                        X1 = screenX,
                        Y1 = 0,
                        X2 = screenX,
                        Y2 = ActualHeight,
                        Stroke = Brushes.Green,
                        StrokeThickness = 1,
                        Opacity = gridOpacity
                    };
                    Children.Add(line);
                }
            }

            for (double y = -YRange; y <= YRange; y += gridStep)
            {
                double screenY = centerY - y * YScale * _scale;
                if (screenY > 0 && screenY < ActualHeight)
                {
                    var line = new Line
                    {
                        X1 = 0,
                        Y1 = screenY,
                        X2 = ActualWidth,
                        Y2 = screenY,
                        Stroke = Brushes.Green,
                        StrokeThickness = 1,
                        Opacity = gridOpacity
                    };
                    Children.Add(line);
                }
            }

            // 绘制X轴
            var xAxis = new Line
            {
                X1 = 0,
                Y1 = centerY,
                X2 = ActualWidth,
                Y2 = centerY,
                Stroke = Brushes.Lime,
                StrokeThickness = 2
            };
            Children.Add(xAxis);

            // 绘制Y轴
            var yAxis = new Line
            {
                X1 = centerX,
                Y1 = 0,
                X2 = centerX,
                Y2 = ActualHeight,
                Stroke = Brushes.Lime,
                StrokeThickness = 2
            };
            Children.Add(yAxis);

            // X轴箭头
            DrawArrow(centerX + XRange * XScale * _scale, centerY, 10, 0, Brushes.Lime);
            // Y轴箭头
            DrawArrow(centerX, centerY - YRange * YScale * _scale, 10, 90, Brushes.Lime);

            // X轴刻度和标签
            for (double x = -XRange; x <= XRange; x += XRange / 4)
            {
                double screenX = centerX + x * XScale * _scale;
                if (screenX > 0 && screenX < ActualWidth)
                {
                    var tick = new Line
                    {
                        X1 = screenX,
                        Y1 = centerY - 6,
                        X2 = screenX,
                        Y2 = centerY + 6,
                        Stroke = Brushes.Lime,
                        StrokeThickness = 2
                    };
                    Children.Add(tick);

                    if (Math.Abs(x) > 0.001)
                    {
                        var label = new TextBlock
                        {
                            Text = $"{x}m",
                            Foreground = Brushes.Lime,
                            FontSize = 10
                        };
                        SetLeft(label, screenX - 15);
                        SetTop(label, centerY + 8);
                        Children.Add(label);
                    }
                }
            }

            // Y轴刻度和标签
            for (double y = -YRange; y <= YRange; y += YRange / 6)
            {
                double screenY = centerY - y * YScale * _scale;
                if (screenY > 0 && screenY < ActualHeight)
                {
                    var tick = new Line
                    {
                        X1 = centerX - 6,
                        Y1 = screenY,
                        X2 = centerX + 6,
                        Y2 = screenY,
                        Stroke = Brushes.Lime,
                        StrokeThickness = 2
                    };
                    Children.Add(tick);

                    if (Math.Abs(y) > 0.001)
                    {
                        var label = new TextBlock
                        {
                            Text = $"{y}m",
                            Foreground = Brushes.Lime,
                            FontSize = 10
                        };
                        SetLeft(label, centerX + 8);
                        SetTop(label, screenY - 8);
                        Children.Add(label);
                    }
                }
            }

            // 轴标签
            var xLabel = new TextBlock { Text = "+X", Foreground = Brushes.Lime, FontSize = 10 };
            SetLeft(xLabel, ActualWidth - 25);
            SetTop(xLabel, centerY + 10);
            Children.Add(xLabel);

            var xLabelNeg = new TextBlock { Text = "-X", Foreground = Brushes.Lime, FontSize = 10 };
            SetLeft(xLabelNeg, 5);
            SetTop(xLabelNeg, centerY + 10);
            Children.Add(xLabelNeg);

            var yLabel = new TextBlock { Text = "+Y", Foreground = Brushes.Lime, FontSize = 10 };
            SetLeft(yLabel, centerX + 8);
            SetTop(yLabel, 5);
            Children.Add(yLabel);

            var yLabelNeg = new TextBlock { Text = "-Y", Foreground = Brushes.Lime, FontSize = 10 };
            SetLeft(yLabelNeg, centerX + 8);
            SetTop(yLabelNeg, ActualHeight - 20);
            Children.Add(yLabelNeg);
        }

        private void DrawRadarCoordinate()
        {
            double centerX = ActualWidth / 2 + _panOffset.X;
            double centerY = ActualHeight / 2 + _panOffset.Y;

            // 绘制同心圆
            for (int i = 1; i <= 5; i++)
            {
                double radius = (RadarRadius / 5) * i * XScale * _scale;
                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };
                SetLeft(circle, centerX - radius);
                SetTop(circle, centerY - radius);
                Children.Add(circle);

                var label = new TextBlock
                {
                    Text = $"{(RadarRadius / 5) * i}m",
                    Foreground = Brushes.Gray,
                    FontSize = 10
                };
                SetLeft(label, centerX + radius + 5);
                SetTop(label, centerY - 6);
                Children.Add(label);
            }

            // 绘制径向线
            double angleStep = 360.0 / RadarSectors;
            for (int i = 0; i < RadarSectors; i++)
            {
                double angleRad = i * angleStep * Math.PI / 180;
                double radius = RadarRadius * XScale * _scale;

                var line = new Line
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = centerX + radius * Math.Cos(angleRad),
                    Y2 = centerY - radius * Math.Sin(angleRad),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1
                };
                Children.Add(line);
            }

            if (IsScanning)
            {
                var scanLine = new Line
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = centerX + RadarRadius * XScale * _scale,
                    Y2 = centerY,
                    Stroke = Brushes.Cyan,
                    StrokeThickness = 2,
                    Opacity = 0.8
                };
                Children.Add(scanLine);
            }
        }

        private void DrawGridCoordinate()
        {
            double centerX = ActualWidth / 2 + _panOffset.X;
            double centerY = ActualHeight / 2 + _panOffset.Y;

            double gridWidth = XRange * 2 * XScale * _scale;
            double gridHeight = YRange * 2 * YScale * _scale;

            // 绘制垂直线
            double xStart = centerX - gridWidth / 2;
            double xEnd = centerX + gridWidth / 2;
            double xStep = GridSize * XScale * _scale;

            for (double x = xStart; x <= xEnd; x += xStep)
            {
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = ActualHeight,
                    Stroke = (Math.Abs(x - centerX) < 0.001) ? Brushes.DarkGray : Brushes.LightGray,
                    StrokeThickness = (Math.Abs(x - centerX) < 0.001) ? 2 : 1,
                    Opacity = 0.5
                };
                Children.Add(line);
            }

            // 绘制水平线
            double yStart = centerY - gridHeight / 2;
            double yEnd = centerY + gridHeight / 2;
            double yStep = GridSize * YScale * _scale;

            for (double y = yStart; y <= yEnd; y += yStep)
            {
                var line = new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = ActualWidth,
                    Y2 = y,
                    Stroke = (Math.Abs(y - centerY) < 0.001) ? Brushes.DarkGray : Brushes.LightGray,
                    StrokeThickness = (Math.Abs(y - centerY) < 0.001) ? 2 : 1,
                    Opacity = 0.5
                };
                Children.Add(line);
            }

            // 绘制刻度标签
            double xMax = XRange;
            double xMin = -XRange;
            double xLabelStep = XRange / 4;

            for (double x = xMin; x <= xMax; x += xLabelStep)
            {
                double screenX = centerX + x * XScale * _scale;
                var label = new TextBlock
                {
                    Text = $"{x}m",
                    Foreground = Brushes.Gray,
                    FontSize = 10
                };
                SetLeft(label, screenX - 15);
                SetTop(label, centerY + 10);
                Children.Add(label);
            }

            double yMax = YRange;
            double yMin = -YRange;
            double yLabelStep = YRange / 6;

            for (double y = yMin; y <= yMax; y += yLabelStep)
            {
                double screenY = centerY - y * YScale * _scale;
                var label = new TextBlock
                {
                    Text = $"{y}m",
                    Foreground = Brushes.Gray,
                    FontSize = 10
                };
                SetLeft(label, centerX + 10);
                SetTop(label, screenY - 8);
                Children.Add(label);
            }
        }

        private void DrawBirdsEyeCoordinate()
        {
            double originX = InnerPadding + _panOffset.X;
            double originY = ActualHeight - InnerPadding + _panOffset.Y;

            double drawableWidth = ActualWidth - InnerPadding * 2;
            double drawableHeight = ActualHeight - InnerPadding * 2;

            // 绘制边框
            var border = new Rectangle
            {
                Width = drawableWidth,
                Height = drawableHeight,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            SetLeft(border, InnerPadding);
            SetTop(border, InnerPadding);
            Children.Add(border);

            // 绘制垂直网格线
            double xGridInterval = 4; // 每4米一格
            for (double x = -XRange; x <= XRange; x += xGridInterval)
            {
                double screenX = originX + x * XScale * _scale;
                if (screenX >= InnerPadding && screenX <= ActualWidth - InnerPadding)
                {
                    var line = new Line
                    {
                        X1 = screenX,
                        Y1 = InnerPadding,
                        X2 = screenX,
                        Y2 = ActualHeight - InnerPadding,
                        Stroke = (x == 0) ? Brushes.Gray : Brushes.DarkGray,
                        StrokeThickness = (x == 0) ? 2 : 1,
                        Opacity = (x == 0) ? 1 : 0.5
                    };
                    Children.Add(line);
                }
            }

            // 绘制水平网格线
            double yGridInterval = 12; // 每12米一格
            for (double y = 0; y <= YRange; y += yGridInterval)
            {
                double screenY = originY - y * YScale * _scale;
                if (screenY >= InnerPadding && screenY <= ActualHeight - InnerPadding)
                {
                    var line = new Line
                    {
                        X1 = InnerPadding,
                        Y1 = screenY,
                        X2 = ActualWidth - InnerPadding,
                        Y2 = screenY,
                        Stroke = (y == 0) ? Brushes.Gray : Brushes.DarkGray,
                        StrokeThickness = (y == 0) ? 2 : 1,
                        Opacity = (y == 0) ? 1 : 0.5
                    };
                    Children.Add(line);
                }
            }

            // 绘制X轴刻度标签（顶部）
            for (double x = -XRange; x <= XRange; x += xGridInterval)
            {
                double screenX = originX + x * XScale * _scale;
                if (screenX >= InnerPadding && screenX <= ActualWidth - InnerPadding)
                {
                    var label = new TextBlock
                    {
                        Text = $"{x}m",
                        Foreground = Brushes.LightGray,
                        FontSize = 10
                    };
                    SetLeft(label, screenX - label.ActualWidth / 2);
                    SetTop(label, InnerPadding - 20);
                    Children.Add(label);
                }
            }

            // 绘制Y轴刻度标签（左侧）
            for (double y = 0; y <= YRange; y += yGridInterval)
            {
                double screenY = originY - y * YScale * _scale;
                if (screenY >= InnerPadding && screenY <= ActualHeight - InnerPadding)
                {
                    var label = new TextBlock
                    {
                        Text = $"{y}m",
                        Foreground = Brushes.LightGray,
                        FontSize = 10
                    };
                    SetLeft(label, 5);
                    SetTop(label, screenY - label.ActualHeight / 2);
                    Children.Add(label);
                }
            }

            // 绘制原点标记（红色点）
            var originMarker = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red
            };
            SetLeft(originMarker, originX - 5);
            SetTop(originMarker, originY - 5);
            Children.Add(originMarker);

            // 绘制传感器信息
            var sensorText = new TextBlock
            {
                Text = $"Sensor ID: {SensorId}",
                Foreground = new SolidColorBrush(Color.FromArgb(180, 120, 160, 200)),
                FontSize = 11,
                FontFamily = new FontFamily("Consolas")
            };
            SetLeft(sensorText, ActualWidth / 2 - 60);
            SetTop(sensorText, ActualHeight - 25);
            Children.Add(sensorText);
        }

        private void DrawArrow(double x, double y, double size, double angle, Brush brush)
        {
            double angleRad = angle * Math.PI / 180;
            double x1 = x - size * Math.Cos(angleRad);
            double y1 = y - size * Math.Sin(angleRad);
            double x2 = x - size * Math.Cos(angleRad - 0.5);
            double y2 = y - size * Math.Sin(angleRad - 0.5);
            double x3 = x - size * Math.Cos(angleRad + 0.5);
            double y3 = y - size * Math.Sin(angleRad + 0.5);

            var arrow1 = new Line { X1 = x, Y1 = y, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = 2 };
            var arrow2 = new Line { X1 = x, Y1 = y, X2 = x3, Y2 = y3, Stroke = brush, StrokeThickness = 2 };
            Children.Add(arrow1);
            Children.Add(arrow2);
        }

        private void DrawTargets()
        {
            foreach (var target in Targets)
            {
                var screenPos = WorldToScreen(target.X, target.Y);

                // 绘制目标点
                var ellipse = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = target.FillColor,
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Opacity = target == SelectedTarget ? 1.0 : 0.7
                };

                if (target == SelectedTarget)
                {
                    var glow = new Ellipse
                    {
                        Width = 24,
                        Height = 24,
                        Fill = target.FillColor,
                        Opacity = 0.3
                    };
                    SetLeft(glow, screenPos.X - 12);
                    SetTop(glow, screenPos.Y - 12);
                    Children.Add(glow);
                }

                SetLeft(ellipse, screenPos.X - 8);
                SetTop(ellipse, screenPos.Y - 8);
                Children.Add(ellipse);

                // 绘制目标ID标签
                var label = new TextBlock
                {
                    Text = target.Id,
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                SetLeft(label, screenPos.X - label.ActualWidth / 2);
                SetTop(label, screenPos.Y - 20);
                Children.Add(label);
            }
        }

        #endregion
    }
}
