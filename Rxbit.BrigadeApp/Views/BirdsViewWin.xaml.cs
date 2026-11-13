using Rxbit.Brigade.Models.Models;
using Rxbit.BrigadeApp.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// BirdsViewWin.xaml 的交互逻辑
    /// </summary>
    public partial class BirdsViewWin : WindowBase
    {
        private RadarCoordinateSystem _coordSystem;
        private DispatcherTimer _scanTimer;
        private DispatcherTimer _fpsTimer;
        private ObservableCollection<RadarTarget1> _targets;
        private Random _random;
        private RadarTarget1 _selectedTarget;
        private bool _isScanning = false;
        private double _scanAngle = 0;
        private bool _isDragging = false;
        private Point _lastMousePos;
        private Point _dragStartWorldPos;
        private Dictionary<RadarTarget1, Rectangle> _targetRects;
        private Dictionary<RadarTarget1, TextBlock> _targetLabels;
        private Dictionary<RadarTarget1, Border> _targetBorders;
        private int _frameCount = 0;
        private DateTime _lastFpsTime;

        public BirdsViewWin()
        {
            InitializeComponent();
            InitializeComponent();
            InitializeRadar();
        }

        private void InitializeRadar()
        {
            _random = new Random();
            _targets = new ObservableCollection<RadarTarget1>();
            _targetRects = new Dictionary<RadarTarget1, Rectangle>();
            _targetLabels = new Dictionary<RadarTarget1, TextBlock>();
            _targetBorders = new Dictionary<RadarTarget1, Border>();
            _lastFpsTime = DateTime.Now;

            _coordSystem = new RadarCoordinateSystem
            {
                MinX = -8,
                MaxX = 8,
                MinY = 0,
                MaxY = 60,
                TickIntervalX = 4,
                TickIntervalY = 12,
                ScaleX = 1.0,
                ScaleY = 1.0,
                ZoomLevel = 1.0,
                ZoomCenter = new Point(0, 0)
            };

            TargetListBox.ItemsSource = _targets;

            // 扫描定时器 (50ms周期)
            _scanTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _scanTimer.Tick += ScanTimer_Tick;

            // FPS统计定时器
            _fpsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _fpsTimer.Tick += (s, e) =>
            {
                FpsText.Text = $"FPS: {_frameCount}";
                _frameCount = 0;
            };
            _fpsTimer.Start();
            
            //SizeChanged += (s, e) =>
            //{
            //    UpdateCoordinateSystem();
            //    DrawGrid();
            //    UpdateAllTargets();
            //};

            //Loaded += (s, e) =>
            //{
            //    UpdateCoordinateSystem();
            //    DrawGrid();
            //    UpdateOriginMarker();
            //    // 预置4个目标模拟图片效果
            //    AddPresetTargets();
            //};
        }

        private void AddPresetTargets()
        {
            var presets = new[]
            {
                new { Id = 1, X = 2.5, Y = 5.0 },
                new { Id = 2, X = 5.5, Y = 12.0 },
                new { Id = 3, X = -1.5, Y = 30.0 },
                new { Id = 4, X = 6.5, Y = 40.0 }
            };

            foreach (var p in presets)
            {
                var target = new RadarTarget1
                {
                    Id = p.Id,
                    X = p.X,
                    Y = p.Y,
                    Type = p.Id == 2 ? "车辆" : "行人",
                    Speed = _random.NextDouble() * 10,
                    Heading = _random.NextDouble() * 360,
                    FillBrush = p.Id == 2 ? new SolidColorBrush(Color.FromRgb(0, 188, 212)) : new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                    StrokeBrush = p.Id == 2 ? Brushes.Cyan : Brushes.Gray
                };
                _targets.Add(target);
                CreateTargetVisual(target);
            }
            TargetCountText.Text = _targets.Count.ToString();
        }

        private void UpdateCoordinateSystem()
        {
            _coordSystem.ActualWidth = Math.Max(400, RadarCanvas.ActualWidth);
            _coordSystem.ActualHeight = Math.Max(300, RadarCanvas.ActualHeight);
        }

        #region 网格绘制
        private void DrawGrid()
        {
            GridCanvas.Children.Clear();
            double w = _coordSystem.ActualWidth;
            double h = _coordSystem.ActualHeight;

            // 垂直网格线 + X轴标签
            foreach (var tick in _coordSystem.GetXAxisTicks())
            {
                var ptBottom = _coordSystem.WorldToScreen(tick, _coordSystem.MinY);
                var ptTop = _coordSystem.WorldToScreen(tick, _coordSystem.MaxY);

                var line = new Line
                {
                    X1 = ptBottom.X,
                    Y1 = ptTop.Y,
                    X2 = ptBottom.X,
                    Y2 = ptBottom.Y,
                    Stroke = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GridCanvas.Children.Add(line);

                // X轴标签 (顶部和底部都显示)
                var labelTop = new TextBlock
                {
                    Text = $"{tick}m",
                    Foreground = new SolidColorBrush(Color.FromArgb(140, 200, 200, 200)),
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(labelTop, ptTop.X - 15);
                Canvas.SetTop(labelTop, ptTop.Y - 18);
                GridCanvas.Children.Add(labelTop);

                var labelBottom = new TextBlock
                {
                    Text = $"{tick}m",
                    Foreground = new SolidColorBrush(Color.FromArgb(140, 200, 200, 200)),
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(labelBottom, ptBottom.X - 15);
                Canvas.SetTop(labelBottom, ptBottom.Y + 4);
                GridCanvas.Children.Add(labelBottom);
            }

            // 水平网格线 + Y轴标签
            foreach (var tick in _coordSystem.GetYAxisTicks())
            {
                var ptLeft = _coordSystem.WorldToScreen(_coordSystem.MinX, tick);
                var ptRight = _coordSystem.WorldToScreen(_coordSystem.MaxX, tick);

                var line = new Line
                {
                    X1 = ptLeft.X,
                    Y1 = ptLeft.Y,
                    X2 = ptRight.X,
                    Y2 = ptRight.Y,
                    Stroke = new SolidColorBrush(Color.FromArgb(35, 255, 255, 255)),
                    StrokeThickness = 0.5
                };
                GridCanvas.Children.Add(line);

                // Y轴标签 (左侧)
                var label = new TextBlock
                {
                    Text = $"{tick}m",
                    Foreground = new SolidColorBrush(Color.FromArgb(140, 200, 200, 200)),
                    FontSize = 10,
                    TextAlignment = TextAlignment.Right
                };
                Canvas.SetLeft(label, Math.Max(2, ptLeft.X - 38));
                Canvas.SetTop(label, ptLeft.Y - 7);
                GridCanvas.Children.Add(label);
            }

            // 外边框
            var border = new Rectangle
            {
                Width = w,
                Height = h,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            GridCanvas.Children.Add(border);
        }

        private void UpdateOriginMarker()
        {
            var origin = _coordSystem.WorldToScreen(0, 0);
            Canvas.SetLeft(OriginMarker, origin.X - 5);
            Canvas.SetTop(OriginMarker, origin.Y - 5);
        }
        #endregion

        #region 目标管理
        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            _frameCount++;

            // 随机生成新目标 (10%概率)
            if (_random.NextDouble() < 0.10 && _targets.Count < 20)
            {
                GenerateRandomTarget();
            }

            // 更新扫描线
            _scanAngle += 3;
            if (_scanAngle >= 360) _scanAngle = 0;
            UpdateScanLine();

            // 更新目标位置 (模拟移动)
            foreach (var target in _targets)
            {
                if (_random.NextDouble() < 0.3)
                {
                    double moveX = (_random.NextDouble() - 0.5) * 0.3;
                    double moveY = (_random.NextDouble() - 0.5) * 0.3;
                    target.X = Math.Max(_coordSystem.MinX, Math.Min(_coordSystem.MaxX, target.X + moveX));
                    target.Y = Math.Max(_coordSystem.MinY, Math.Min(_coordSystem.MaxY, target.Y + moveY));
                }
            }

            UpdateAllTargets();
        }

        private void GenerateRandomTarget()
        {
            int id = _targets.Count > 0 ? _targets.Max(t => t.Id) + 1 : 1;
            double x = _random.NextDouble() * (_coordSystem.MaxX - _coordSystem.MinX) + _coordSystem.MinX;
            double y = _random.NextDouble() * (_coordSystem.MaxY - _coordSystem.MinY) + _coordSystem.MinY;

            var target = new RadarTarget1
            {
                Id = id,
                X = x,
                Y = y,
                Type = GetRandomType(),
                Speed = _random.NextDouble() * 15,
                Heading = _random.NextDouble() * 360,
                DetectTime = DateTime.Now,
                Status = "正常",
                FillBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160)),
                StrokeBrush = Brushes.Cyan,
                StrokeThickness = 2
            };

            _targets.Add(target);
            CreateTargetVisual(target);
            TargetCountText.Text = _targets.Count.ToString();
            StatusText.Text = $"检测到新目标: ID={id:00} ({x:F1}m, {y:F1}m)";
        }

        private string GetRandomType()
        {
            string[] types = { "车辆", "行人", "骑行者", "障碍物" };
            return types[_random.Next(types.Length)];
        }

        private void CreateTargetVisual(RadarTarget1 target)
        {
            // 目标容器
            var border = new Border
            {
                Width = 14,
                Height = 14,
                Background = target.FillBrush,
                BorderBrush = target.StrokeBrush,
                BorderThickness = new Thickness(target.StrokeThickness),
                CornerRadius = new CornerRadius(2),
                Cursor = Cursors.Hand,
                Tag = target,
                Child = new Grid()
            };

            border.MouseLeftButtonDown += Target_MouseLeftButtonDown;

            // ID标签
            var label = new TextBlock
            {
                Text = $"{target.Id:00}",
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };

            _targetBorders[target] = border;
            _targetLabels[target] = label;

            TargetCanvas.Children.Add(border);
            TargetCanvas.Children.Add(label);

            UpdateTargetPosition(target);
        }

        private void UpdateTargetPosition(RadarTarget1 target)
        {
            if (!_targetBorders.ContainsKey(target)) return;

            var border = _targetBorders[target];
            var label = _targetLabels[target];
            var center = _coordSystem.WorldToScreen(target.X, target.Y);

            double size = Math.Max(10, 14 * _coordSystem.ZoomLevel);
            size = Math.Min(size, 30);

            border.Width = size;
            border.Height = size;
            border.CornerRadius = new CornerRadius(2);

            if (target.IsSelected)
            {
                border.BorderBrush = Brushes.Orange;
                border.BorderThickness = new Thickness(3);
                border.Background = new SolidColorBrush(Color.FromRgb(0, 188, 212));
                border.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Orange,
                    BlurRadius = 8,
                    ShadowDepth = 0
                };
            }
            else
            {
                border.BorderBrush = target.StrokeBrush;
                border.BorderThickness = new Thickness(target.StrokeThickness);
                border.Background = target.FillBrush;
                border.Effect = null;
            }

            Canvas.SetLeft(border, center.X - size / 2);
            Canvas.SetTop(border, center.Y - size / 2);

            Canvas.SetLeft(label, center.X - 12);
            Canvas.SetTop(label, center.Y - size / 2 - 18);
        }

        private void UpdateAllTargets()
        {
            foreach (var target in _targets)
                UpdateTargetPosition(target);
            UpdateScanLine();
            UpdateOriginMarker();
        }

        private void UpdateScanLine()
        {
            var origin = _coordSystem.WorldToScreen(0, 0);
            double angleRad = _scanAngle * Math.PI / 180;
            double length = Math.Max(_coordSystem.ActualWidth, _coordSystem.ActualHeight) * 1.8;

            ScanLine.X1 = origin.X;
            ScanLine.Y1 = origin.Y;
            ScanLine.X2 = origin.X + Math.Sin(angleRad) * length;
            ScanLine.Y2 = origin.Y - Math.Cos(angleRad) * length;
        }
        #endregion

        #region 目标选择与详情
        private void Target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var target = border?.Tag as RadarTarget1;
            if (target != null)
            {
                SelectTarget(target);
                e.Handled = true;
            }
        }

        private void SelectTarget(RadarTarget1 target)
        {
            if (_selectedTarget != null)
                _selectedTarget.IsSelected = false;

            _selectedTarget = target;

            if (target != null)
            {
                target.IsSelected = true;
                TargetListBox.SelectedItem = target;
                UpdateDetailPanel(target);
                StatusText.Text = $"已选中目标: ID={target.Id:00} | 类型: {target.Type}";
            }
            else
            {
                TargetListBox.SelectedItem = null;
                ClearDetailPanel();
                StatusText.Text = "就绪";
            }

            UpdateAllTargets();
        }

        private void UpdateDetailPanel(RadarTarget1 target)
        {
            DetailPanel.Children.Clear();

            var title = new TextBlock
            {
                Text = $"目标 {target.Id:00}",
                Foreground = new SolidColorBrush(Color.FromRgb(0, 188, 212)),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            DetailPanel.Children.Add(title);

            AddDetailRow("位置坐标", $"X: {target.X:F2}m  Y: {target.Y:F2}m");
            AddDetailRow("目标类型", target.Type);
            AddDetailRow("移动速度", $"{target.Speed:F1} m/s");
            AddDetailRow("运动航向", $"{target.Heading:F1}°");
            AddDetailRow("目标状态", target.Status);
            AddDetailRow("检测时间", target.DetectTime.ToString("HH:mm:ss.fff"));
            AddDetailRow("尺寸大小", $"{target.Width:F1}m × {target.Height:F1}m");
        }

        private void AddDetailRow(string label, string value)
        {
            var labelBlock = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                FontSize = 10,
                Margin = new Thickness(0, 6, 0, 2)
            };
            var valueBlock = new TextBlock
            {
                Text = value,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 4)
            };
            DetailPanel.Children.Add(labelBlock);
            DetailPanel.Children.Add(valueBlock);
        }

        private void ClearDetailPanel()
        {
            DetailPanel.Children.Clear();
            var hint = new TextBlock
            {
                Text = "点击目标或列表项查看详情",
                Foreground = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            DetailPanel.Children.Add(hint);
        }

        private void TargetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TargetListBox.SelectedItem is RadarTarget1 target)
                SelectTarget(target);
        }
        #endregion

        #region 缩放与平移交互
        private void RadarCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(RadarCanvas);
            var worldPos = _coordSystem.ScreenToWorld(mousePos.X, mousePos.Y);

            double zoomDelta = e.Delta > 0 ? 1.15 : 0.87;
            double newZoom = _coordSystem.ZoomLevel * zoomDelta;
            newZoom = Math.Max(0.3, Math.Min(8.0, newZoom));

            _coordSystem.ZoomLevel = newZoom;
            _coordSystem.ZoomCenter = new Point(
                _coordSystem.ZoomCenter.X + (worldPos.X - _coordSystem.ZoomCenter.X) * (1 - 1 / zoomDelta),
                _coordSystem.ZoomCenter.Y + (worldPos.Y - _coordSystem.ZoomCenter.Y) * (1 - 1 / zoomDelta)
            );

            ZoomInfoText.Text = $"Zoom: {newZoom:F1}x";
            DrawGrid();
            UpdateAllTargets();
            e.Handled = true;
        }

        private void RadarCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePos = e.GetPosition(RadarCanvas);
            _dragStartWorldPos = _coordSystem.ZoomCenter;
            RadarCanvas.CaptureMouse();

            if (e.OriginalSource == RadarCanvas || e.OriginalSource == GridCanvas || e.OriginalSource == ScanCanvas)
            {
                SelectTarget(null);
            }
        }

        private void RadarCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPos = e.GetPosition(RadarCanvas);
            var worldPos = _coordSystem.ScreenToWorld(currentPos.X, currentPos.Y);
            CoordinateText.Text = $"({worldPos.X:F1}m, {worldPos.Y:F1}m)";

            if (_isDragging)
            {
                double dx = currentPos.X - _lastMousePos.X;
                double dy = currentPos.Y - _lastMousePos.Y;

                double ppmX = (_coordSystem.ActualWidth / (_coordSystem.MaxX - _coordSystem.MinX))
                              * _coordSystem.ScaleX * _coordSystem.ZoomLevel;
                double ppmY = ((_coordSystem.ActualHeight - 60) / (_coordSystem.MaxY - _coordSystem.MinY))
                              * _coordSystem.ScaleY * _coordSystem.ZoomLevel;

                _coordSystem.ZoomCenter = new Point(
                    _dragStartWorldPos.X - dx / ppmX,
                    _dragStartWorldPos.Y + dy / ppmY
                );

                DrawGrid();
                UpdateAllTargets();
            }
        }

        private void RadarCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            RadarCanvas.ReleaseMouseCapture();
        }

        private void RadarCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            RadarCanvas.ReleaseMouseCapture();
        }
        #endregion

        #region 按钮事件
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            _isScanning = true;
            _scanTimer.Start();
            StartBtn.IsEnabled = false;
            StopBtn.IsEnabled = true;
            StatusText.Text = "雷达扫描中...";
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _isScanning = false;
            _scanTimer.Stop();
            StartBtn.IsEnabled = true;
            StopBtn.IsEnabled = false;
            StatusText.Text = "扫描已停止";
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            // 停止扫描
            if (_isScanning)
                StopBtn_Click(null, null);

            // 清除所有目标
            TargetCanvas.Children.Clear();
            _targets.Clear();
            _targetBorders.Clear();
            _targetLabels.Clear();
            _targetRects?.Clear();
            _selectedTarget = null;

            // 清除详情
            ClearDetailPanel();
            TargetCountText.Text = "0";

            // 保持坐标系和网格不变
            StatusText.Text = "目标已清除 | 坐标系保持不变";
        }

        private void ResetZoomBtn_Click(object sender, RoutedEventArgs e)
        {
            _coordSystem.ZoomLevel = 1.0;
            _coordSystem.ZoomCenter = new Point(0, 0);
            ZoomInfoText.Text = "Zoom: 1.0x";
            DrawGrid();
            UpdateAllTargets();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }
        #endregion
    }


    #region 数据模型
    public class RadarTarget1 : DependencyObject
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; } = 0.8;
        public double Height { get; set; } = 0.8;
        public string Type { get; set; } = "未知目标";
        public double Speed { get; set; } = 0;
        public double Heading { get; set; } = 0;
        public DateTime DetectTime { get; set; } = DateTime.Now;
        public string Status { get; set; } = "正常";
        public Brush FillBrush { get; set; } = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        public Brush StrokeBrush { get; set; } = Brushes.Cyan;
        public double StrokeThickness { get; set; } = 2;

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(RadarTarget), new PropertyMetadata(false));

        public override string ToString()
        {
            return $"目标 {Id:00}\n" +
                   $"━━━━━━━━━━━━━━\n" +
                   $"位置: ({X:F2}m, {Y:F2}m)\n" +
                   $"类型: {Type}\n" +
                   $"速度: {Speed:F1} m/s\n" +
                   $"航向: {Heading:F1}°\n" +
                   $"状态: {Status}\n" +
                   $"检测时间: {DetectTime:HH:mm:ss.fff}";
        }
    }
    #endregion

    #region 坐标系统工具类
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

        /// <summary>
        /// 世界坐标(米) -> 屏幕坐标(像素)
        /// </summary>
        public Point WorldToScreen(double x, double y)
        {
            double originX = ActualWidth / 2 + ZoomCenter.X * GetPixelPerMeterX();
            double originY = ActualHeight - 40 - ZoomCenter.Y * GetPixelPerMeterY();

            double pixelPerMeterX = GetPixelPerMeterX();
            double pixelPerMeterY = GetPixelPerMeterY();

            double screenX = originX + x * pixelPerMeterX;
            double screenY = originY - y * pixelPerMeterY;

            return new Point(screenX, screenY);
        }

        /// <summary>
        /// 屏幕坐标(像素) -> 世界坐标(米)
        /// </summary>
        public Point ScreenToWorld(double screenX, double screenY)
        {
            double originX = ActualWidth / 2 + ZoomCenter.X * GetPixelPerMeterX();
            double originY = ActualHeight - 40 - ZoomCenter.Y * GetPixelPerMeterY();

            double pixelPerMeterX = GetPixelPerMeterX();
            double pixelPerMeterY = GetPixelPerMeterY();

            double x = (screenX - originX) / pixelPerMeterX;
            double y = (originY - screenY) / pixelPerMeterY;

            return new Point(x, y);
        }

        private double GetPixelPerMeterX()
        {
            return (ActualWidth / (MaxX - MinX)) * ScaleX * ZoomLevel;
        }

        private double GetPixelPerMeterY()
        {
            return ((ActualHeight - 60) / (MaxY - MinY)) * ScaleY * ZoomLevel;
        }

        public List<double> GetXAxisTicks()
        {
            var ticks = new List<double>();
            for (double x = MinX; x <= MaxX; x += TickIntervalX)
                ticks.Add(x);
            return ticks;
        }

        public List<double> GetYAxisTicks()
        {
            var ticks = new List<double>();
            for (double y = MinY; y <= MaxY; y += TickIntervalY)
                ticks.Add(y);
            return ticks;
        }
    }
    #endregion
}
