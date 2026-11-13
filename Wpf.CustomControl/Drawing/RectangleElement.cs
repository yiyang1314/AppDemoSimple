using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 矩形绘制元素，支持圆角。
    /// </summary>
    public class RectangleElement : DrawingElement
    {
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RectWidthProperty =
            DependencyProperty.Register("RectWidth", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RectHeightProperty =
            DependencyProperty.Register("RectHeight", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusXProperty =
            DependencyProperty.Register("RadiusX", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusYProperty =
            DependencyProperty.Register("RadiusY", typeof(double), typeof(RectangleElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public double RectWidth
        {
            get { return (double)GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }

        public double RectHeight
        {
            get { return (double)GetValue(RectHeightProperty); }
            set { SetValue(RectHeightProperty, value); }
        }

        public double RadiusX
        {
            get { return (double)GetValue(RadiusXProperty); }
            set { SetValue(RadiusXProperty, value); }
        }

        public double RadiusY
        {
            get { return (double)GetValue(RadiusYProperty); }
            set { SetValue(RadiusYProperty, value); }
        }

        protected override void OnDraw(DrawingContext drawingContext)
        {
            Rect rect = new Rect(X, Y, RectWidth, RectHeight);
            if (RadiusX > 0 || RadiusY > 0)
            {
                // 圆角矩形
                RectangleGeometry geometry = new RectangleGeometry(rect, RadiusX, RadiusY);
                drawingContext.DrawGeometry(Fill, GetPen(), geometry);
            }
            else
            {
                drawingContext.DrawRectangle(Fill, GetPen(), rect);
            }
        }
    }
}
