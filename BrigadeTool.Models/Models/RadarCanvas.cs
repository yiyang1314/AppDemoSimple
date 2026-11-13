using Rxbit.Brigade.Models.EnumEntitys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rxbit.Brigade.Models.Models
{
    /// <summary>
    /// 雷达坐标系统 - 纯数据/工具类，无UI逻辑
    /// 支持自定义网格坐标系参数
    /// </summary>
    public class RadarCoordinateSystem
    {
        public double MinX { get; set; } = -8;
        public double MaxX { get; set; } = 8;
        public double MinY { get; set; } = 0;
        public double MaxY { get; set; } = 60;
        public double TickIntervalX { get; set; } = 4;
        public double TickIntervalY { get; set; } = 12;
        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public double ZoomLevel { get; set; } = 1.0;
        public Point ZoomCenter { get; set; } = new Point(0, 0);
        public double ActualWidth { get; set; } = 800;
        public double ActualHeight { get; set; } = 600;
        public double MarginLeft { get; set; } = 50;
        public double MarginRight { get; set; } = 50;
        public double MarginTop { get; set; } = 30;
        public double MarginBottom { get; set; } = 30;

        public double PlotWidth => Math.Max(100, ActualWidth - MarginLeft - MarginRight);
        public double PlotHeight => Math.Max(100, ActualHeight - MarginTop - MarginBottom);
        public Rect PlotRect => new Rect(MarginLeft, MarginTop, PlotWidth, PlotHeight);

        public double PixelPerMeterX => (PlotWidth / (MaxX - MinX)) * ScaleX * ZoomLevel;
        public double PixelPerMeterY => (PlotHeight / (MaxY - MinY)) * ScaleY * ZoomLevel;

        public Point OriginScreen
        {
            get
            {
                double originX = MarginLeft + (-MinX) * PixelPerMeterX;
                double originY = MarginTop + PlotHeight;
                return new Point(originX, originY);
            }
        }

        public Point WorldToScreen(double x, double y)
        {
            var origin = OriginScreen;
            double screenX = origin.X + x * PixelPerMeterX;
            double screenY = origin.Y - y * PixelPerMeterY;
            return new Point(screenX, screenY);
        }

        public Point ScreenToWorld(double screenX, double screenY)
        {
            var origin = OriginScreen;
            return new Point((screenX - origin.X) / PixelPerMeterX, (origin.Y - screenY) / PixelPerMeterY);
        }

        public IEnumerable<double> GetXAxisTicks()
        {
            double startX = Math.Ceiling(MinX / TickIntervalX) * TickIntervalX;
            for (double x = startX; x <= MaxX + 0.001; x += TickIntervalX)
                yield return Math.Round(x, 2);
        }

        public IEnumerable<double> GetYAxisTicks()
        {
            double startY = Math.Ceiling(MinY / TickIntervalY) * TickIntervalY;
            for (double y = startY; y <= MaxY + 0.001; y += TickIntervalY)
                yield return Math.Round(y, 2);
        }
    }

    /// <summary>
    /// 雷达画布 - 纯View层，只负责渲染和交互事件转发
    /// 所有业务逻辑在ViewModel中处理
    /// </summary>
    public class RadarCanvas : Canvas
    {
        #region 依赖属性

        public RadarCoordinateSystem CoordinateSystem
        {
            get => (RadarCoordinateSystem)GetValue(CoordinateSystemProperty);
            set => SetValue(CoordinateSystemProperty, value);
        }
        public static readonly DependencyProperty CoordinateSystemProperty =
            DependencyProperty.Register(nameof(CoordinateSystem), typeof(RadarCoordinateSystem), typeof(RadarCanvas),
                new PropertyMetadata(null, (d, e) => ((RadarCanvas)d).InvalidateVisual()));

        public ObservableCollection<Models.RadarTarget> Targets
        {
            get => (ObservableCollection<Models.RadarTarget>)GetValue(TargetsProperty);
            set => SetValue(TargetsProperty, value);
        }
        public static readonly DependencyProperty TargetsProperty =
            DependencyProperty.Register(nameof(Targets), typeof(ObservableCollection<Models.RadarTarget>), typeof(RadarCanvas),
                new PropertyMetadata(null, OnTargetsChanged));

        private static void OnTargetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = (RadarCanvas)d;
            if (e.OldValue is ObservableCollection<Models.RadarTarget> oldTargets)
            {
                oldTargets.CollectionChanged -= canvas.Targets_CollectionChanged;
                foreach (var target in oldTargets)
                {
                    ((System.ComponentModel.INotifyPropertyChanged)target).PropertyChanged -= canvas.Target_PropertyChanged;
                }
            }
            if (e.NewValue is ObservableCollection<Models.RadarTarget> newTargets)
            {
                newTargets.CollectionChanged += canvas.Targets_CollectionChanged;
                foreach (var target in newTargets)
                {
                    ((System.ComponentModel.INotifyPropertyChanged)target).PropertyChanged += canvas.Target_PropertyChanged;
                }
            }
            canvas.InvalidateVisual();
        }

        private void Targets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    ((System.ComponentModel.INotifyPropertyChanged)item).PropertyChanged += Target_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    ((System.ComponentModel.INotifyPropertyChanged)item).PropertyChanged -= Target_PropertyChanged;
                }
            }
            InvalidateVisual();
        }

        private void Target_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public uint SensorID
        {
            get => (uint)GetValue(SensorIDProperty);
            set => SetValue(SensorIDProperty, value);
        }
        public static readonly DependencyProperty SensorIDProperty =
            DependencyProperty.Register(nameof(SensorID), typeof(uint), typeof(RadarCanvas),
                new PropertyMetadata(0u, (d, e) => ((RadarCanvas)d).InvalidateVisual()));

        public double ScanAngle
        {
            get => (double)GetValue(ScanAngleProperty);
            set => SetValue(ScanAngleProperty, value);
        }
        public static readonly DependencyProperty ScanAngleProperty =
            DependencyProperty.Register(nameof(ScanAngle), typeof(double), typeof(RadarCanvas),
                new PropertyMetadata(0.0, (d, e) => ((RadarCanvas)d).InvalidateVisual()));

        public bool ShowScanLine
        {
            get => (bool)GetValue(ShowScanLineProperty);
            set => SetValue(ShowScanLineProperty, value);
        }
        public static readonly DependencyProperty ShowScanLineProperty =
            DependencyProperty.Register(nameof(ShowScanLine), typeof(bool), typeof(RadarCanvas), new PropertyMetadata(true));

        public bool ShowGrid
        {
            get => (bool)GetValue(ShowGridProperty);
            set => SetValue(ShowGridProperty, value);
        }
        public static readonly DependencyProperty ShowGridProperty =
            DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(RadarCanvas), new PropertyMetadata(true));

        public bool ShowBorder
        {
            get => (bool)GetValue(ShowBorderProperty);
            set => SetValue(ShowBorderProperty, value);
        }
        public static readonly DependencyProperty ShowBorderProperty =
            DependencyProperty.Register(nameof(ShowBorder), typeof(bool), typeof(RadarCanvas), new PropertyMetadata(true));

        public Models.RadarTarget SelectedTarget
        {
            get => (Models.RadarTarget)GetValue(SelectedTargetProperty);
            set => SetValue(SelectedTargetProperty, value);
        }
        public static readonly DependencyProperty SelectedTargetProperty =
            DependencyProperty.Register(nameof(SelectedTarget), typeof(Models.RadarTarget), typeof(RadarCanvas),
                new PropertyMetadata(null, (d, e) => ((RadarCanvas)d).InvalidateVisual()));

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }
        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(nameof(GridLineBrush), typeof(Brush), typeof(RadarCanvas),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255))));

        public Brush BorderLineBrush
        {
            get => (Brush)GetValue(BorderLineBrushProperty);
            set => SetValue(BorderLineBrushProperty, value);
        }
        public static readonly DependencyProperty BorderLineBrushProperty =
            DependencyProperty.Register(nameof(BorderLineBrush), typeof(Brush), typeof(RadarCanvas),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 255, 255, 255))));

        public Brush AxisLabelBrush
        {
            get => (Brush)GetValue(AxisLabelBrushProperty);
            set => SetValue(AxisLabelBrushProperty, value);
        }
        public static readonly DependencyProperty AxisLabelBrushProperty =
            DependencyProperty.Register(nameof(AxisLabelBrush), typeof(Brush), typeof(RadarCanvas),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(160, 200, 200, 200))));

        public Brush OriginMarkerBrush
        {
            get => (Brush)GetValue(OriginMarkerBrushProperty);
            set => SetValue(OriginMarkerBrushProperty, value);
        }
        public static readonly DependencyProperty OriginMarkerBrushProperty =
            DependencyProperty.Register(nameof(OriginMarkerBrush), typeof(Brush), typeof(RadarCanvas),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 68, 68))));

        public Brush ScanLineBrush
        {
            get => (Brush)GetValue(ScanLineBrushProperty);
            set => SetValue(ScanLineBrushProperty, value);
        }
        public static readonly DependencyProperty ScanLineBrushProperty =
            DependencyProperty.Register(nameof(ScanLineBrush), typeof(Brush), typeof(RadarCanvas),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(120, 0, 188, 212))));

        public double TargetDefaultSize
        {
            get => (double)GetValue(TargetDefaultSizeProperty);
            set => SetValue(TargetDefaultSizeProperty, value);
        }
        public static readonly DependencyProperty TargetDefaultSizeProperty =
            DependencyProperty.Register(nameof(TargetDefaultSize), typeof(double), typeof(RadarCanvas), new PropertyMetadata(14.0));

        public double TargetSelectedSize
        {
            get => (double)GetValue(TargetSelectedSizeProperty);
            set => SetValue(TargetSelectedSizeProperty, value);
        }
        public static readonly DependencyProperty TargetSelectedSizeProperty =
            DependencyProperty.Register(nameof(TargetSelectedSize), typeof(double), typeof(RadarCanvas), new PropertyMetadata(18.0));

        #endregion

        #region 路由事件 - 交互事件通过路由事件转发给ViewModel处理

        public static readonly RoutedEvent TargetSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(TargetSelected), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RadarCanvas));

        public event RoutedEventHandler TargetSelected
        {
            add { AddHandler(TargetSelectedEvent, value); }
            remove { RemoveHandler(TargetSelectedEvent, value); }
        }

        public static readonly RoutedEvent CoordinateChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(CoordinateChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RadarCanvas));

        public event RoutedEventHandler CoordinateChanged
        {
            add { AddHandler(CoordinateChangedEvent, value); }
            remove { RemoveHandler(CoordinateChangedEvent, value); }
        }

        public static readonly RoutedEvent CanvasZoomEvent = EventManager.RegisterRoutedEvent(
            nameof(CanvasZoom), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RadarCanvas));

        public event RoutedEventHandler CanvasZoom
        {
            add { AddHandler(CanvasZoomEvent, value); }
            remove { RemoveHandler(CanvasZoomEvent, value); }
        }

        public static readonly RoutedEvent CanvasDragEvent = EventManager.RegisterRoutedEvent(
            nameof(CanvasDrag), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RadarCanvas));

        public event RoutedEventHandler CanvasDrag
        {
            add { AddHandler(CanvasDragEvent, value); }
            remove { RemoveHandler(CanvasDragEvent, value); }
        }

        #endregion


        private static Brush[] Colors = new Brush[] { Brushes.White, Brushes.Yellow, Brushes.Green, Brushes.Red };
        private DrawingVisual _gridVisual;
        private DrawingVisual _targetVisual;
        private DrawingVisual _scanVisual;
        private DrawingVisual _originVisual;
        private readonly List<DrawingVisual> _visuals = new List<DrawingVisual>();

        private bool _isDragging = false;
        private Point _lastMousePos;
        private Point _dragStartCenter;

        private System.Windows.Threading.DispatcherTimer _scanTimer;
        private double _scanDirection = 1;

        public RadarCanvas()
        {
            Background = Brushes.Transparent;
            SnapsToDevicePixels = true;

            _gridVisual = new DrawingVisual();
            _targetVisual = new DrawingVisual();
            _scanVisual = new DrawingVisual();
            _originVisual = new DrawingVisual();

            _visuals.Add(_gridVisual);
            _visuals.Add(_scanVisual);
            _visuals.Add(_targetVisual);
            _visuals.Add(_originVisual);

            foreach (var v in _visuals)
                AddVisualChild(v);

            this.SizeChanged += (s, e) =>
            {
                if (CoordinateSystem != null)
                {
                    CoordinateSystem.ActualWidth = ActualWidth;
                    CoordinateSystem.ActualHeight = ActualHeight;
                }
                InvalidateVisual();
            };

            this.MouseLeftButtonDown += OnMouseLeftButtonDown;
            this.MouseMove += OnMouseMove;
            this.MouseLeftButtonUp += OnMouseLeftButtonUp;
            this.MouseLeave += OnMouseLeave;
            this.MouseWheel += OnMouseWheel;

            _scanTimer = new System.Windows.Threading.DispatcherTimer();
            _scanTimer.Interval = TimeSpan.FromMilliseconds(30);
            _scanTimer.Tick += ScanTimer_Tick;
            _scanTimer.Start();
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            if (!ShowScanLine) return;

            double newAngle = ScanAngle + _scanDirection * 0.5;

            if (newAngle >= 180)
            {
                newAngle = 180;
                _scanDirection = -1;
            }
            else if (newAngle <= 0)
            {
                newAngle = 0;
                _scanDirection = 1;
            }

            ScanAngle = newAngle;
        }

        #region Visual 树

        protected override int VisualChildrenCount => _visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _visuals.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _visuals[index];
        }

        #endregion

        #region 渲染

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (CoordinateSystem == null) return;

            DrawGrid(_gridVisual.RenderOpen());
            DrawScanLine(_scanVisual.RenderOpen());
            DrawTargets(_targetVisual.RenderOpen());
            DrawOrigin(_originVisual.RenderOpen());
        }

        private void DrawGrid(DrawingContext dc)
        {
            if (!ShowGrid || CoordinateSystem == null) { dc.Close(); return; }

            var cs = CoordinateSystem;
            var plotRect = cs.PlotRect;
            var typeface = new Typeface("Microsoft YaHei");
            var labelBrush = new SolidColorBrush(Color.FromArgb(180, 144, 202, 249));

            // 深蓝色背景
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(15, 23, 42)), null,
                new Rect(0, 0, cs.ActualWidth, cs.ActualHeight));

            // 绘图区背景（稍亮的深蓝色）
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(20, 30, 50)), null, plotRect);

            // 网格线画笔
            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 0, 188, 212)), 1);
            var axisPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 0, 188, 212)), 2);

            // 垂直网格线 + X轴标签（顶部和底部）
            foreach (var tick in cs.GetXAxisTicks())
            {
                var ptBottom = cs.WorldToScreen(tick, cs.MinY);
                var ptTop = cs.WorldToScreen(tick, cs.MaxY);
                ptTop.Y = Math.Max(ptTop.Y, plotRect.Top);
                ptBottom.Y = Math.Min(ptBottom.Y, plotRect.Bottom);

                Pen penToUse = Math.Abs(tick) < 0.001 ? axisPen : gridPen;
                if (ptTop.Y < ptBottom.Y)
                    dc.DrawLine(penToUse, new Point(ptTop.X, ptTop.Y), new Point(ptBottom.X, ptBottom.Y));

                // 顶部标签
                var ftTop = new FormattedText($"{tick}m", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 11, labelBrush, 1.0);
                dc.DrawText(ftTop, new Point(ptTop.X - ftTop.Width / 2, plotRect.Top - 22));

                // 底部标签
                var ftBottom = new FormattedText($"{tick}m", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 11, labelBrush, 1.0);
                dc.DrawText(ftBottom, new Point(ptBottom.X - ftBottom.Width / 2, plotRect.Bottom + 8));
            }

            // 水平网格线 + Y轴标签（左侧和右侧）
            foreach (var tick in cs.GetYAxisTicks())
            {
                if (tick < 0.1) continue;
                var ptLeft = cs.WorldToScreen(cs.MinX, tick);
                var ptRight = cs.WorldToScreen(cs.MaxX, tick);
                ptLeft.X = Math.Max(ptLeft.X, plotRect.Left);
                ptRight.X = Math.Min(ptRight.X, plotRect.Right);

                Pen penToUse = Math.Abs(tick) < 0.001 ? axisPen : gridPen;
                if (ptLeft.X < ptRight.X)
                    dc.DrawLine(penToUse, new Point(ptLeft.X, ptLeft.Y), new Point(ptRight.X, ptRight.Y));

                // 左侧标签
                var ftLeft = new FormattedText($"{tick}m", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 11, labelBrush, 1.0);
                dc.DrawText(ftLeft, new Point(Math.Max(5, plotRect.Left - ftLeft.Width - 10), ptLeft.Y - ftLeft.Height / 2));

                // 右侧标签
                var ftRight = new FormattedText($"{tick}m", System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 11, labelBrush, 1.0);
                dc.DrawText(ftRight, new Point(plotRect.Right + 5, ptRight.Y - ftRight.Height / 2));
            }

            // 边界线框
            if (ShowBorder)
            {
                var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 0, 188, 212)), 2);
                dc.DrawRectangle(null, borderPen, plotRect);
            }

            // 绘制坐标原点标记
            var origin = cs.WorldToScreen(0, 0);
            if (origin.X >= plotRect.Left && origin.X <= plotRect.Right &&
                origin.Y >= plotRect.Top && origin.Y <= plotRect.Bottom)
            {
                dc.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromRgb(0, 188, 212)), 2), origin, 10, 10);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(255, 68, 68)), null, origin, 5, 5);
            }

            // SensorID - 绘制在网格顶部中央
            var SensoridTag = new FormattedText($"SensorID: 0x{SensorID:X}", System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeface, 11, labelBrush, 1.0);
            dc.DrawText(SensoridTag, new Point(plotRect.Left + plotRect.Width / 2 - SensoridTag.Width / 2, plotRect.Top - 50));
            dc.Close();
        }

        private void DrawScanLine(DrawingContext dc)
        {
            if (!ShowScanLine || CoordinateSystem == null) { dc.Close(); return; }

            var cs = CoordinateSystem;
            var origin = cs.WorldToScreen(0, 0);

            double clampedAngle = Math.Max(0, Math.Min(180, ScanAngle));
            double angleRad = clampedAngle * Math.PI / 180;
            double length = Math.Max(cs.PlotWidth, cs.PlotHeight) * 1.5;

            var endPoint = new Point(origin.X + Math.Sin(angleRad) * length, origin.Y - Math.Cos(angleRad) * length);

            var scanPath = new StreamGeometry();
            using (var ctx = scanPath.Open())
            {
                ctx.BeginFigure(origin, true, true);
                double startAngle = Math.Max(0, clampedAngle);
                double endAngle = Math.Min(180, clampedAngle);
                for (double a = startAngle; a <= endAngle; a += 1)
                {
                    double rad = a * Math.PI / 180;
                    ctx.LineTo(new Point(origin.X + Math.Sin(rad) * length * 0.3, origin.Y - Math.Cos(rad) * length * 0.3), true, false);
                }
            }
            dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(15, 0, 188, 212)), null, scanPath);

            // dc.DrawLine(new Pen(ScanLineBrush, 1.5) { DashStyle = new DashStyle(new double[] { 5, 3 }, 0) }, origin, endPoint);
            dc.Close();
        }

        private void DrawTargets(DrawingContext dc)
        {
            if (Targets == null || CoordinateSystem == null) { dc.Close(); return; }

            var cs = CoordinateSystem;
            var plotRect = cs.PlotRect;
            var typeface = new Typeface("Microsoft YaHei");

            foreach (var target in this.Targets)
            {
                if (!target.IsVisibled) continue;
                var center = cs.WorldToScreen(target.X, target.Y);
                if (center.X < plotRect.Left - 50 || center.X > plotRect.Right + 50 ||
                    center.Y < plotRect.Top - 50 || center.Y > plotRect.Bottom + 50)
                    continue;

                bool isSelected = target == SelectedTarget;
                double size = isSelected ? TargetSelectedSize : TargetDefaultSize;
                size = Math.Max(8, Math.Min(30, size * cs.ZoomLevel));
                var rect = new Rect(center.X - size / 2, center.Y - size / 2, size, size);
                var fillBrush = isSelected ? new SolidColorBrush(Color.FromRgb(0, 188, 212)) : target.FillBrush;
                var strokeBrush = isSelected ? Brushes.Orange : target.StrokeBrush;
                var ID = $"{target.Id:00}";
                var rectBrush = Brushes.White;
                if (isSelected)
                {
                    rectBrush = Brushes.Yellow;
                    if (target.ClassType != -1)
                    {
                        ID = $"{target.Id:00}-({RadarConstaint.ClsassCate[target.ClassType]})";
                    }
                }
                else
                {
                    if (target.MotionType > 0 && target.MotionType < 5)
                    {
                        rectBrush = Brushes.Green;
                    }
                    if (target.ClassType != -1)
                    {
                        ID = $"{target.Id:00}-({RadarConstaint.ClsassCate[target.ClassType]})";
                    }
                }
                var labelFormatted = new FormattedText(ID, System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight, typeface, 11, rectBrush, 1.0);
                labelFormatted.SetFontWeight(FontWeights.Bold);
                dc.DrawText(labelFormatted, new Point(center.X - labelFormatted.Width / 2, center.Y - size / 2 - 20));
                if (isSelected)
                {
                    dc.DrawRectangle(rectBrush, null, new Rect(rect.X - 6, rect.Y - 6, rect.Width + 12, rect.Height + 12));
                    dc.DrawLine(new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 165, 0)), 1),
                        new Point(center.X, center.Y + size / 2 + 2), new Point(center.X, center.Y + size / 2 + 12));
                }
                dc.DrawRectangle(rectBrush, new Pen(strokeBrush, isSelected ? 3 : target.StrokeThickness), rect);
            }

            dc.Close();
        }

        private void DrawOrigin(DrawingContext dc)
        {
            if (CoordinateSystem == null) { dc.Close(); return; }

            var origin = CoordinateSystem.WorldToScreen(0, 0);
            var plotRect = CoordinateSystem.PlotRect;

            if (origin.X >= plotRect.Left && origin.X <= plotRect.Right &&
                origin.Y >= plotRect.Top && origin.Y <= plotRect.Bottom)
            {
                // 青色外圈
                dc.DrawEllipse(null, new Pen(new SolidColorBrush(Color.FromRgb(0, 188, 212)), 2), origin, 12, 12);
                // 红色中心点
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(255, 68, 68)), null, origin, 6, 6);
            }

            dc.Close();
        }

        #endregion

        #region 鼠标交互 - 事件转发给ViewModel

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);

            // 命中检测
            Models.RadarTarget hitTarget = null;
            if (Targets != null && CoordinateSystem != null)
            {
                foreach (var target in Targets)
                {
                    var center = CoordinateSystem.WorldToScreen(target.X, target.Y);
                    double size = (target == SelectedTarget ? TargetSelectedSize : TargetDefaultSize) * CoordinateSystem.ZoomLevel;
                    size = Math.Max(8, Math.Min(30, size));
                    var rect = new Rect(center.X - size / 2, center.Y - size / 2, size, size);
                    if (rect.Contains(pos)) { hitTarget = target; break; }
                }
            }

            if (hitTarget != null)
            {
                // 通过路由事件通知ViewModel
                RaiseEvent(new RoutedEventArgs(TargetSelectedEvent, hitTarget));
                e.Handled = true;
            }
            else
            {
                _isDragging = true;
                _lastMousePos = pos;
                _dragStartCenter = CoordinateSystem?.ZoomCenter ?? new Point();
                CaptureMouse();
                RaiseEvent(new RoutedEventArgs(TargetSelectedEvent, null));
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(this);

            if (CoordinateSystem != null)
            {
                var worldPos = CoordinateSystem.ScreenToWorld(currentPos.X, currentPos.Y);
                RaiseEvent(new RoutedEventArgs(CoordinateChangedEvent, worldPos));
            }

            if (_isDragging && CoordinateSystem != null)
            {
                Point delta = new Point(currentPos.X - _lastMousePos.X, currentPos.Y - _lastMousePos.Y);
                // 通过路由事件通知ViewModel处理拖拽
                RaiseEvent(new RoutedEventArgs(CanvasDragEvent, new Tuple<Point, Point>(delta, _dragStartCenter)));
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //if (CoordinateSystem == null) return;
            //Point mousePos = e.GetPosition(this);
            //// 通过路由事件通知ViewModel处理缩放
            //RaiseEvent(new RoutedEventArgs(CanvasZoomEvent, new Tuple<Point, int>(mousePos, e.Delta)));
            //e.Handled = true;
        }

        #endregion

        public void Refresh() => InvalidateVisual();
    }
}
