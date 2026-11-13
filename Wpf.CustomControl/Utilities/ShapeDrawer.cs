using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Utilities
{
    public enum TargetShapeType
    {
        Rectangle,      // 矩形
        Circle,         // 圆形
        Ellipse,        // 椭圆
        Triangle,       // 三角形
        Pentagon,       // 五边形
        Hexagon,        // 六边形
        Ring,           // 圆环
        Cross,          // 十字
        Diamond,        // 菱形
        Arrow           // 箭头
    }
    /// <summary>
    /// 形状绘制工具类 - 支持多种几何形状绘制
    /// </summary>
    public static class ShapeDrawer
    {
        /// <summary>
        /// 根据形状类型绘制目标
        /// </summary>
        public static void DrawShape(DrawingContext dc, TargetShapeType shapeType,
            Point center, double width, double height,
            Brush fill, Pen stroke, bool isSelected)
        {
            switch (shapeType)
            {
                case TargetShapeType.Rectangle:
                    DrawRectangle(dc, center, width, height, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Circle:
                    DrawCircle(dc, center, Math.Min(width, height) / 2, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Ellipse:
                    DrawEllipse(dc, center, width / 2, height / 2, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Triangle:
                    DrawPolygon(dc, center, width, height, 3, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Pentagon:
                    DrawPolygon(dc, center, width, height, 5, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Hexagon:
                    DrawPolygon(dc, center, width, height, 6, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Ring:
                    DrawRing(dc, center, Math.Min(width, height) / 2, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Cross:
                    DrawCross(dc, center, width, height, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Diamond:
                    DrawDiamond(dc, center, width, height, fill, stroke, isSelected);
                    break;
                case TargetShapeType.Arrow:
                    DrawArrowShape(dc, center, width, height, fill, stroke, isSelected);
                    break;
                default:
                    DrawRectangle(dc, center, width, height, fill, stroke, isSelected);
                    break;
            }
        }

        /// <summary>
        /// 绘制矩形
        /// </summary>
        private static void DrawRectangle(DrawingContext dc, Point center, double width, double height,
            Brush fill, Pen stroke, bool isSelected)
        {
            var rect = new Rect(center.X - width / 2, center.Y - height / 2, width, height);
            dc.DrawRectangle(fill, stroke, rect);

            if (isSelected)
                DrawSelectionFrame(dc, rect);
        }

        /// <summary>
        /// 绘制圆形
        /// </summary>
        private static void DrawCircle(DrawingContext dc, Point center, double radius,
            Brush fill, Pen stroke, bool isSelected)
        {
            dc.DrawEllipse(fill, stroke, center, radius, radius);

            if (isSelected)
            {
                var rect = new Rect(center.X - radius - 3, center.Y - radius - 3, radius * 2 + 6, radius * 2 + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 绘制椭圆
        /// </summary>
        private static void DrawEllipse(DrawingContext dc, Point center, double radiusX, double radiusY,
            Brush fill, Pen stroke, bool isSelected)
        {
            dc.DrawEllipse(fill, stroke, center, radiusX, radiusY);

            if (isSelected)
            {
                var rect = new Rect(center.X - radiusX - 3, center.Y - radiusY - 3, radiusX * 2 + 6, radiusY * 2 + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 绘制正多边形（三角形、五边形、六边形等）
        /// </summary>
        private static void DrawPolygon(DrawingContext dc, Point center, double width, double height,
            int sides, Brush fill, Pen stroke, bool isSelected)
        {
            var geometry = CreatePolygonGeometry(center, width, height, sides);
            dc.DrawGeometry(fill, stroke, geometry);

            if (isSelected)
            {
                double maxR = Math.Max(width, height) / 2 + 3;
                var rect = new Rect(center.X - maxR, center.Y - maxR, maxR * 2, maxR * 2);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 创建正多边形几何
        /// </summary>
        private static Geometry CreatePolygonGeometry(Point center, double width, double height, int sides)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            double radiusX = width / 2;
            double radiusY = height / 2;
            double angleStep = 2 * Math.PI / sides;
            // 旋转偏移使三角形顶点朝上
            double startAngle = -Math.PI / 2;

            for (int i = 0; i < sides; i++)
            {
                double angle = startAngle + i * angleStep;
                var point = new Point(
                    center.X + radiusX * Math.Cos(angle),
                    center.Y + radiusY * Math.Sin(angle));

                if (i == 0)
                    figure.StartPoint = point;
                else
                    figure.Segments.Add(new LineSegment(point, true));
            }

            figure.IsClosed = true;
            geometry.Figures.Add(figure);
            return geometry;
        }

        /// <summary>
        /// 绘制圆环
        /// </summary>
        private static void DrawRing(DrawingContext dc, Point center, double outerRadius,
            Brush fill, Pen stroke, bool isSelected)
        {
            double innerRadius = outerRadius * 0.6;

            // 外圆
            dc.DrawEllipse(fill, stroke, center, outerRadius, outerRadius);
            // 内圆（挖空）
            dc.DrawEllipse(new SolidColorBrush(Color.FromArgb(255, 0, 30, 0)), null, center, innerRadius, innerRadius);

            if (isSelected)
            {
                var rect = new Rect(center.X - outerRadius - 3, center.Y - outerRadius - 3,
                    outerRadius * 2 + 6, outerRadius * 2 + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 绘制十字
        /// </summary>
        private static void DrawCross(DrawingContext dc, Point center, double width, double height,
            Brush fill, Pen stroke, bool isSelected)
        {
            double armWidth = Math.Min(width, height) * 0.25;
            double halfW = width / 2;
            double halfH = height / 2;

            // 垂直臂
            var vRect = new Rect(center.X - armWidth / 2, center.Y - halfH, armWidth, height);
            dc.DrawRectangle(fill, stroke, vRect);

            // 水平臂
            var hRect = new Rect(center.X - halfW, center.Y - armWidth / 2, width, armWidth);
            dc.DrawRectangle(fill, stroke, hRect);

            if (isSelected)
            {
                var rect = new Rect(center.X - halfW - 3, center.Y - halfH - 3, width + 6, height + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 绘制菱形
        /// </summary>
        private static void DrawDiamond(DrawingContext dc, Point center, double width, double height,
            Brush fill, Pen stroke, bool isSelected)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(center.X, center.Y - height / 2)
            };
            figure.Segments.Add(new LineSegment(new Point(center.X + width / 2, center.Y), true));
            figure.Segments.Add(new LineSegment(new Point(center.X, center.Y + height / 2), true));
            figure.Segments.Add(new LineSegment(new Point(center.X - width / 2, center.Y), true));
            figure.IsClosed = true;

            geometry.Figures.Add(figure);
            dc.DrawGeometry(fill, stroke, geometry);

            if (isSelected)
            {
                var rect = new Rect(center.X - width / 2 - 3, center.Y - height / 2 - 3, width + 6, height + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        public static void DrawArrowLine(DrawingContext dc, string tag, Point start, Point end,
            double arrowSize = 12, Brush stroke = null, double thickness = 1)
        {
            PathGeometry pg = new PathGeometry();
            PathFigure lineFig = new PathFigure(start, new[] { new LineSegment(end, true) }, false);
            pg.Figures.Add(lineFig);

            Vector dir = end - start;
            dir.Normalize();
            Vector vert = new Vector(-dir.Y, dir.X);
            Point arrowBase = end - dir * arrowSize;
            Point p1 = arrowBase + vert * arrowSize / 2;
            Point p2 = arrowBase - vert * arrowSize / 2;

            PathFigure arrowFig = new PathFigure(end, new[]
            {
                new LineSegment(p1,true),
                new LineSegment(p2,true)
            }, true);
            pg.Figures.Add(arrowFig);

            dc.DrawGeometry(Brushes.White, new Pen(Brushes.White, thickness), pg);
            var idText = new FormattedText(
                    tag,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Consolas"),
                    8,
                   Brushes.White,
                    12);

            dc.DrawText(idText, end);
        }

        /// <summary>
        /// 绘制箭头形状
        /// </summary>
        private static void DrawArrowShape(DrawingContext dc, Point center, double width, double height,
            Brush fill, Pen stroke, bool isSelected)
        {
            double headWidth = width * 0.4;
            double headHeight = height * 0.4;
            double shaftWidth = width * 0.2;
            double shaftHeight = height * 0.6;

            var geometry = new PathGeometry();
            var figure = new PathFigure
            {
                StartPoint = new Point(center.X, center.Y - height / 2) // 箭头尖端
            };

            // 箭头右侧
            figure.Segments.Add(new LineSegment(new Point(center.X + headWidth / 2, center.Y - height / 2 + headHeight), true));
            // 箭杆右侧
            figure.Segments.Add(new LineSegment(new Point(center.X + shaftWidth / 2, center.Y - height / 2 + headHeight), true));
            figure.Segments.Add(new LineSegment(new Point(center.X + shaftWidth / 2, center.Y + height / 2), true));
            // 箭杆底部
            figure.Segments.Add(new LineSegment(new Point(center.X - shaftWidth / 2, center.Y + height / 2), true));
            // 箭杆左侧
            figure.Segments.Add(new LineSegment(new Point(center.X - shaftWidth / 2, center.Y - height / 2 + headHeight), true));
            // 箭头左侧
            figure.Segments.Add(new LineSegment(new Point(center.X - headWidth / 2, center.Y - height / 2 + headHeight), true));

            figure.IsClosed = true;
            geometry.Figures.Add(figure);

            dc.DrawGeometry(fill, stroke, geometry);

            if (isSelected)
            {
                var rect = new Rect(center.X - width / 2 - 3, center.Y - height / 2 - 3, width + 6, height + 6);
                DrawSelectionFrame(dc, rect);
            }
        }

        /// <summary>
        /// 绘制选中框
        /// </summary>
        private static void DrawSelectionFrame(DrawingContext dc, Rect rect)
        {
            dc.DrawRectangle(null, new Pen(Brushes.Red, 1.5) { DashStyle = DashStyles.Dash },
                new Rect(rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6));
        }

        /// <summary>
        /// 获取形状外接矩形（用于命中测试）
        /// </summary>
        public static Rect GetBoundingRect(TargetShapeType shapeType, Point center, double width, double height)
        {
            switch (shapeType)
            {
                case TargetShapeType.Circle:
                case TargetShapeType.Ring:
                    double r = Math.Min(width, height) / 2;
                    return new Rect(center.X - r, center.Y - r, r * 2, r * 2);
                default:
                    return new Rect(center.X - width / 2, center.Y - height / 2, width, height);
            }
        }
    }
}
