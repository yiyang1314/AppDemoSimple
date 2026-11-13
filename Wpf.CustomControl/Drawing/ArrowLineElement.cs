using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 带箭头的直线绘制元素，支持两端箭头。
    /// </summary>
    public class ArrowLineElement : DrawingElement
    {
        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1", typeof(double), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register("Y1", typeof(double), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register("X2", typeof(double), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2", typeof(double), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HeadSizeProperty =
            DependencyProperty.Register("HeadSize", typeof(double), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HasStartArrowProperty =
            DependencyProperty.Register("HasStartArrow", typeof(bool), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty HasEndArrowProperty =
            DependencyProperty.Register("HasEndArrow", typeof(bool), typeof(ArrowLineElement),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }

        public double HeadSize
        {
            get { return (double)GetValue(HeadSizeProperty); }
            set { SetValue(HeadSizeProperty, value); }
        }

        public bool HasStartArrow
        {
            get { return (bool)GetValue(HasStartArrowProperty); }
            set { SetValue(HasStartArrowProperty, value); }
        }

        public bool HasEndArrow
        {
            get { return (bool)GetValue(HasEndArrowProperty); }
            set { SetValue(HasEndArrowProperty, value); }
        }

        protected override void OnDraw(DrawingContext drawingContext)
        {
            Point start = new Point(X1, Y1);
            Point end = new Point(X2, Y2);
            Pen pen = GetPen();

            drawingContext.DrawLine(pen, start, end);

            if (HasEndArrow)
            {
                DrawArrow(drawingContext, pen, start, end);
            }
            if (HasStartArrow)
            {
                DrawArrow(drawingContext, pen, end, start);
            }
        }

        private void DrawArrow(DrawingContext drawingContext, Pen pen, Point from, Point to)
        {
            double angle = System.Math.Atan2(to.Y - from.Y, to.X - from.X);
            double size = HeadSize;
            // 箭头两个侧点
            Point p1 = new Point(
                to.X - size * System.Math.Cos(angle - System.Math.PI / 6),
                to.Y - size * System.Math.Sin(angle - System.Math.PI / 6));
            Point p2 = new Point(
                to.X - size * System.Math.Cos(angle + System.Math.PI / 6),
                to.Y - size * System.Math.Sin(angle + System.Math.PI / 6));

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(to, true, true);
                ctx.LineTo(p1, true, false);
                ctx.LineTo(p2, true, false);
            }
            geometry.Freeze();
            Brush fill = Stroke != null ? Stroke.Clone() : Brushes.Black;
            if (fill.CanFreeze)
            {
                fill.Freeze();
            }
            drawingContext.DrawGeometry(fill, pen, geometry);
        }
    }
}
