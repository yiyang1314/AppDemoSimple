using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 圆弧绘制元素，通过中心点、半径、起始角度和扫掠角度定义。
    /// 角度单位为度（0°指向右方，顺时针方向）。
    /// </summary>
    public class ArcElement : DrawingElement
    {
        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(ArcElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(ArcElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(ArcElement),
                new FrameworkPropertyMetadata(25.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SweepAngleProperty =
            DependencyProperty.Register("SweepAngle", typeof(double), typeof(ArcElement),
                new FrameworkPropertyMetadata(90.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double CenterX
        {
            get { return (double)GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        public double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public double SweepAngle
        {
            get { return (double)GetValue(SweepAngleProperty); }
            set { SetValue(SweepAngleProperty, value); }
        }

        protected override void OnDraw(DrawingContext drawingContext)
        {
            double startRad = StartAngle * System.Math.PI / 180.0;
            double endRad = (StartAngle + SweepAngle) * System.Math.PI / 180.0;

            Point startPoint = new Point(
                CenterX + Radius * System.Math.Cos(startRad),
                CenterY + Radius * System.Math.Sin(startRad));
            Point endPoint = new Point(
                CenterX + Radius * System.Math.Cos(endRad),
                CenterY + Radius * System.Math.Sin(endRad));

            PathFigure figure = new PathFigure();
            figure.StartPoint = startPoint;
            ArcSegment arc = new ArcSegment();
            arc.Point = endPoint;
            arc.Size = new Size(Radius, Radius);
            arc.IsLargeArc = System.Math.Abs(SweepAngle) > 180;
            arc.SweepDirection = SweepAngle >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
            figure.Segments.Add(arc);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            geometry.Freeze();

            drawingContext.DrawGeometry(Fill, GetPen(), geometry);
        }
    }
}
