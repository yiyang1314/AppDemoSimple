using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 椭圆绘制元素，通过中心点和半径定义。
    /// </summary>
    public class EllipseElement : DrawingElement
    {
        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(EllipseElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(EllipseElement),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusXProperty =
            DependencyProperty.Register("RadiusX", typeof(double), typeof(EllipseElement),
                new FrameworkPropertyMetadata(25.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty RadiusYProperty =
            DependencyProperty.Register("RadiusY", typeof(double), typeof(EllipseElement),
                new FrameworkPropertyMetadata(25.0, FrameworkPropertyMetadataOptions.AffectsRender));

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
            drawingContext.DrawEllipse(Fill, GetPen(), new Point(CenterX, CenterY), RadiusX, RadiusY);
        }
    }
}
