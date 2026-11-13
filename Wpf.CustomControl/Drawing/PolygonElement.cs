using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 多边形绘制元素，支持任意数量的顶点。
    /// </summary>
    public class PolygonElement : DrawingElement
    {
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(PointCollection), typeof(PolygonElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public PointCollection Points
        {
            get { return (PointCollection)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        protected override void OnDraw(DrawingContext drawingContext)
        {
            PointCollection points = Points;
            if (points == null || points.Count < 2)
            {
                return;
            }

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                ctx.BeginFigure(points[0], true, true);
                for (int i = 1; i < points.Count; i++)
                {
                    ctx.LineTo(points[i], true, false);
                }
            }
            geometry.Freeze();
            drawingContext.DrawGeometry(Fill, GetPen(), geometry);
        }
    }
}
