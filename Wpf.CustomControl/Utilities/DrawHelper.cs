using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// 坐标系绘制结果信息
    /// </summary>
    public class CoordinateSystemResult
    {
        /// <summary>
        /// X轴缩放比例
        /// </summary>
        public double XScale { get; set; }

        /// <summary>
        /// Y轴缩放比例
        /// </summary>
        public double YScale { get; set; }

        /// <summary>
        /// 原点坐标（画布像素坐标）
        /// </summary>
        public Point Origin { get; set; }

        /// <summary>
        /// X轴最小值（数学坐标）
        /// </summary>
        public double XMin { get; set; }

        /// <summary>
        /// X轴最大值（数学坐标）
        /// </summary>
        public double XMax { get; set; }

        /// <summary>
        /// Y轴最小值（数学坐标）
        /// </summary>
        public double YMin { get; set; }

        /// <summary>
        /// Y轴最大值（数学坐标）
        /// </summary>
        public double YMax { get; set; }

        /// <summary>
        /// 画布宽度
        /// </summary>
        public double CanvasWidth { get; set; }

        /// <summary>
        /// 画布高度
        /// </summary>
        public double CanvasHeight { get; set; }

        /// <summary>
        /// Y轴最小值（数学坐标）
        /// </summary>
        public double Xoffset { get; set; }

        /// <summary>
        /// Y轴最大值（数学坐标）
        /// </summary>
        public double Yoffset { get; set; }

        public override string ToString()
        {
            return string.Format("坐标系：\r\n画布宽度: {0:F1} \r\n画布高度: {1:F1}\r\n X轴缩放比例: {2:F1}\r\n Y轴缩放比例: {3:F1}\r\n原点: [{4:F1},{5:F1}]",
               CanvasWidth, CanvasHeight, XScale, YScale, Origin.X, Origin.Y);
        }
    }
    public static class DrawHelper
    {
        #region 全局默认画笔配置
        private const double DefaultStrokeThickness = 2;
        private static readonly Brush DefaultStroke = Brushes.Black;
        private static readonly Brush DefaultFill = Brushes.Transparent;

        private static CoordinateSystemResult coordinateConfig;
        #endregion

        #region 1.绘制矩形
        public static Rectangle DrawRect(Canvas canvas, string tag, Point center, double width, double height,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
        {
            var rect = new Rectangle
            {
                Width = width,
                Height = height,
                Stroke = stroke ?? DefaultStroke,
                Fill = fill ?? DefaultFill,
                StrokeThickness = thickness
            };
            Canvas.SetLeft(rect, center.X);
            Canvas.SetTop(rect, center.Y);
            canvas.Children.Add(rect);
            DrawText(canvas, new Point(center.X + 10 + width, center.Y - 10), tag, stroke, 12);
            return rect;
        }
        #endregion

        #region 2.绘制圆形（圆心+半径）
        public static Ellipse DrawCircle(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
        {
            var ell = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = stroke ?? DefaultStroke,
                Fill = fill ?? DefaultFill,
                StrokeThickness = thickness
            };
            Canvas.SetLeft(ell, center.X - radius);
            Canvas.SetTop(ell, center.Y - radius);
            canvas.Children.Add(ell);
            DrawText(canvas, new Point(center.X + 10 + ell.Width, center.Y - 10), tag, stroke, 12);
            return ell;
        }
        #endregion

        #region 3.绘制椭圆（圆心+XY半径）
        public static Ellipse DrawEllipse(Canvas canvas, string tag, Point center, double rx, double ry,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
        {
            var ell = new Ellipse
            {
                Width = rx * 2,
                Height = ry * 2,
                Stroke = stroke ?? DefaultStroke,
                Fill = fill ?? DefaultFill,
                StrokeThickness = thickness
            };
            Canvas.SetLeft(ell, center.X - rx);
            Canvas.SetTop(ell, center.Y - ry);
            canvas.Children.Add(ell);
            return ell;
        }
        #endregion

        #region 4.绘制圆环（外半径、内半径、圆心）
        public static Path DrawRing(Canvas canvas, string tag, Point center, double outerR, double innerR,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
        {
            var geo = new CombinedGeometry(GeometryCombineMode.Xor,
                new EllipseGeometry(center, outerR, outerR),
                new EllipseGeometry(center, innerR, innerR));

            var path = new Path
            {
                Data = geo,
                Stroke = stroke ?? DefaultStroke,
                Fill = fill ?? DefaultFill,
                StrokeThickness = thickness
            };
            canvas.Children.Add(path);
            return path;
        }
        #endregion

        #region 5.正多边形通用（正3角/正5/正6边形）
        public static Polygon DrawRegularPolygon(Canvas canvas, string tag, Point center, double radius, int sideCount,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
        {
            var poly = new Polygon
            {
                Stroke = stroke ?? DefaultStroke,
                Fill = fill ?? DefaultFill,
                StrokeThickness = thickness
            };
            PointCollection pts = new PointCollection();
            double startRad = -90 * Math.PI / 180;
            double perRad = 2 * Math.PI / sideCount;

            for (int i = 0; i < sideCount; i++)
            {
                double rad = startRad + i * perRad;
                double x = center.X + radius * Math.Cos(rad);
                double y = center.Y + radius * Math.Sin(rad);
                pts.Add(new Point(x, y));
            }
            poly.Points = pts;
            canvas.Children.Add(poly);
            return poly;
        }

        public static Polygon DrawTriangle(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
            => DrawRegularPolygon(canvas, tag, center, radius, 3, stroke, fill, thickness);

        public static Polygon DrawPentagon(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
            => DrawRegularPolygon(canvas, tag, center, radius, 5, stroke, fill, thickness);

        public static Polygon DrawHexagon(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
            => DrawRegularPolygon(canvas, tag, center, radius, 6, stroke, fill, thickness);

        public static Polygon DrawHightagon(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
            => DrawRegularPolygon(canvas, tag, center, radius, 8, stroke, fill, thickness);

        public static Polygon DrawEvtweenagon(Canvas canvas, string tag, Point center, double radius,
            Brush stroke = null, Brush fill = null, double thickness = DefaultStrokeThickness)
            => DrawRegularPolygon(canvas, tag, center, radius, 10, stroke, fill, thickness);
        #endregion

        #region 6.带箭头直线（核心：自动计算箭头三角坐标）
        public static Path DrawArrowLine(Canvas canvas, string tag, Point start, Point end, double arrowSize = 12,
            Brush stroke = null, double thickness = DefaultStrokeThickness)
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

            Path path = new Path
            {
                Data = pg,
                Stroke = stroke ?? DefaultStroke,
                Fill = stroke ?? DefaultStroke,
                StrokeThickness = thickness
            };
            canvas.Children.Add(path);
            var pp = new Point();
            if (Math.Abs(end.X - start.X) < 1)
            {
                pp.Y = end.Y + 16;
                pp.X = end.X - 20;
            }
            else
            {
                pp.Y = end.Y;
                pp.X = end.X - 30;
            }
            DrawText(canvas, pp, tag, stroke, 12);
            return path;
        }

        public static Path DrawLine(Canvas canvas, string tag, Point start, Point end,
            Brush stroke = null, double thickness = DefaultStrokeThickness)
        {
            PathGeometry pg = new PathGeometry();
            PathFigure lineFig = new PathFigure(start, new[] { new LineSegment(end, true) }, false);
            pg.Figures.Add(lineFig);

            Path path = new Path
            {
                Data = pg,
                Stroke = stroke ?? DefaultStroke,
                Fill = stroke ?? DefaultStroke,
                StrokeThickness = thickness
            };
            canvas.Children.Add(path);
            return path;
        }

        public static Path DrawCors(Canvas canvas, string tag, Point start, double arrowSize = 12, Brush stroke = null, double thickness = DefaultStrokeThickness)
        {
            PathGeometry pg = new PathGeometry();
            PathFigure hFig = new PathFigure(new Point(start.X - arrowSize / 2, start.Y), new[] { new LineSegment(new Point(start.X + arrowSize / 2, start.Y), true) }, false);
            PathFigure v1Fig = new PathFigure(new Point(start.X, start.Y - arrowSize / 2), new[] { new LineSegment(new Point(start.X, start.Y + arrowSize / 2), true) }, false);
            pg.Figures.Add(hFig);
            pg.Figures.Add(v1Fig);
            Path path = new Path
            {
                Data = pg,
                Stroke = stroke ?? DefaultStroke,
                Fill = stroke ?? DefaultStroke,
                StrokeThickness = thickness
            };
            canvas.Children.Add(path);
            return path;
        }
        #endregion

        #region 7.绘制文本
        public static TextBlock DrawText(Canvas canvas, Point pos, string text,
            Brush fore = null, double fontSize = 12)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = fore ?? Brushes.Black,
                FontSize = fontSize
            };
            Canvas.SetLeft(tb, pos.X);
            Canvas.SetTop(tb, pos.Y);
            canvas.Children.Add(tb);
            return tb;
        }
        #endregion

        #region 清空画布
        public static void ClearCanvas(Canvas canvas) => canvas.Children.Clear();
        #endregion

        #region 8.平面直角坐标系绘制

        /// <summary>
        /// 绘制平面直角十字坐标系（完整四象限）
        /// </summary>
        /// <returns>坐标系绘制结果信息，包含缩放比例、原点坐标和显示范围</returns>
        public static CoordinateSystemResult DrawCoordinateSystem(Canvas canvas, Point origin, double width, double height,
            double xMin, double xMax, double yMin, double yMax, double xStep, double yStep, string unit = "m")
        {
            Brush axisBrush = Brushes.Red;
            Brush gridBrush = Brushes.DarkGray;
            Brush labelBrush = Brushes.White;
            double axisThickness = 1.5;
            double gridThickness = 0.5;
            double margin = 60;  // 边距（增加边距确保标签完整显示）
            double labelHeight = 10;  // 标签高度

            // 确保画布实际尺寸可用
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // 计算可用区域（扣除边距）
            double availableWidth = canvasWidth - margin * 2;
            double availableHeight = canvasHeight - margin * 2;

            // 计算坐标范围
            double xRange = xMax - xMin;
            double yRange = yMax - yMin;

            // 计算比例（确保坐标轴在画布内）
            double xScale = availableWidth / xRange;
            double yScale = availableHeight / yRange;

            // 确保比例不为零
            xScale = xScale > 0 ? xScale : 10;
            yScale = yScale > 0 ? yScale : 10;

            // 原点位置设置在画布中心
            double actualOriginX = canvasWidth / 2;
            double actualOriginY = canvasHeight / 2;
            Point actualOrigin = new Point(actualOriginX, actualOriginY);

            // 计算轴端点（确保不溢出，完整显示正负方向）
            double xStartCanvas = actualOrigin.X + xMin * xScale;
            double xEndCanvas = actualOrigin.X + xMax * xScale;
            double yStartCanvas = actualOrigin.Y - yMax * yScale;
            double yEndCanvas = actualOrigin.Y - yMin * yScale;

            // 限制轴端点在画布边界内
            xStartCanvas = Math.Max(margin, xStartCanvas);
            xEndCanvas = Math.Min(canvasWidth - margin, xEndCanvas);
            yStartCanvas = Math.Max(margin, yStartCanvas);
            yEndCanvas = Math.Min(canvasHeight - margin, yEndCanvas);

            // 绘制X轴（从左到右）
            DrawArrowLine(canvas, "+X", new Point(xStartCanvas, actualOrigin.Y),
                new Point(xEndCanvas, actualOrigin.Y), 12, axisBrush, axisThickness);

            // 绘制Y轴（从下到上）
            DrawArrowLine(canvas, "+Y", new Point(actualOrigin.X, yEndCanvas),
                new Point(actualOrigin.X, yStartCanvas), 12, axisBrush, axisThickness);

            // 绘制X轴刻度和标签
            for (double x = xMin; x <= xMax; x += xStep)
            {
                double canvasX = actualOrigin.X + x * xScale;

                if (canvasX < margin || canvasX > canvasWidth - margin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = canvasX,
                    Y1 = actualOrigin.Y - 5,
                    X2 = canvasX,
                    Y2 = actualOrigin.Y + 5,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签（放在X轴上方，避免被底部遮挡）
                string label = x == 0 ? "0" : $"{x}{unit}";
                TextBlock tb = new TextBlock
                {
                    Text = label,
                    Foreground = labelBrush,
                    FontSize = 10
                };
                Canvas.SetLeft(tb, canvasX - 15);
                Canvas.SetTop(tb, actualOrigin.Y + labelHeight);  // 移到X轴上方
                canvas.Children.Add(tb);

                // 垂直网格线
                Line gridLine = new Line
                {
                    X1 = canvasX,
                    Y1 = yStartCanvas,
                    X2 = canvasX,
                    Y2 = yEndCanvas,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 绘制Y轴刻度和标签
            for (double y = yMin; y <= yMax; y += yStep)
            {
                double canvasY = actualOrigin.Y - y * yScale;

                if (canvasY < margin || canvasY > canvasHeight - margin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = actualOrigin.X - 5,
                    Y1 = canvasY,
                    X2 = actualOrigin.X + 5,
                    Y2 = canvasY,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签
                string label = y == 0 ? "" : $"{y}{unit}";
                if (!string.IsNullOrEmpty(label))
                {
                    TextBlock tb = new TextBlock
                    {
                        Text = label,
                        Foreground = labelBrush,
                        FontSize = 10,
                        Width = 35,
                        TextAlignment = TextAlignment.Right
                    };
                    Canvas.SetLeft(tb, Math.Max(5, actualOrigin.X - 40));
                    Canvas.SetTop(tb, canvasY - 6);
                    canvas.Children.Add(tb);
                }

                // 水平网格线
                Line gridLine = new Line
                {
                    X1 = xStartCanvas,
                    Y1 = canvasY,
                    X2 = xEndCanvas,
                    Y2 = canvasY,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 原点标记
            Ellipse originDot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.White,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            Canvas.SetLeft(originDot, actualOrigin.X - 4);
            Canvas.SetTop(originDot, actualOrigin.Y - 4);
            canvas.Children.Add(originDot);

            // 返回坐标系信息
            coordinateConfig = new CoordinateSystemResult
            {
                XScale = xScale,
                YScale = yScale,
                Origin = actualOrigin,
                XMin = xMin,
                XMax = xMax,
                YMin = yMin,
                YMax = yMax,
                CanvasWidth = canvasWidth,
                CanvasHeight = canvasHeight,
                Xoffset = margin,
                Yoffset = margin
            };
            return coordinateConfig;
        }

        /// <summary>
        /// 绘制第一、第二象限坐标系（雷达常用，支持负X轴）
        /// </summary>
        /// <returns>坐标系绘制结果信息，包含缩放比例、原点坐标和显示范围</returns>
        public static CoordinateSystemResult DrawFirstSecondQuadrant(Canvas canvas, Point origin, double width, double height,
            double xMin, double xMax, double yMax, double xStep, double yStep, string unit = "m")
        {
            Brush axisBrush = Brushes.Red;
            Brush gridBrush = Brushes.DarkGray;
            Brush labelBrush = Brushes.White;
            double axisThickness = 1.5;
            double gridThickness = 0.5;
            double margin = 50;  // 边距（增加边距确保标签不被遮挡）
            double labelMargin = 25;  // 标签额外边距

            // 确保画布实际尺寸可用
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // 计算可用区域（扣除边距和标签空间）
            double availableWidth = canvasWidth - margin * 2;
            double availableHeight = canvasHeight - margin * 2 - labelMargin;

            // 计算坐标范围
            double xRange = xMax - xMin;

            // 计算比例（确保坐标轴在画布内）
            double xScale = availableWidth / xRange;
            double yScale = availableHeight / yMax;

            // 确保比例不为零
            xScale = xScale > 0 ? xScale : 10;
            yScale = yScale > 0 ? yScale : 10;

            // 重新计算原点位置，确保在画布内
            double actualOriginX = Math.Max(margin + 30, Math.Min(canvasWidth - margin - 30, origin.X));
            double actualOriginY = Math.Max(margin + 30, Math.Min(canvasHeight - margin - labelMargin - 30, origin.Y));
            Point actualOrigin = new Point(actualOriginX, actualOriginY);

            // 计算轴端点（确保不溢出，支持负X轴）
            double xStartCanvas = Math.Max(margin, actualOrigin.X + xMin * xScale);  // 负X轴端点
            double xEndCanvas = Math.Min(canvasWidth - margin, actualOrigin.X + xMax * xScale);  // 正X轴端点
            double yStartCanvas = Math.Max(margin, actualOrigin.Y - yMax * yScale) - 15;  // Y轴顶端
            double yEndCanvas = Math.Min(canvasHeight - margin - labelMargin, actualOrigin.Y);  // Y轴底端（原点）

            // 绘制X轴（从负方向到正方向，完整覆盖第一、第二象限）
            DrawArrowLine(canvas, "+X", new Point(xStartCanvas, actualOrigin.Y),
                new Point(xEndCanvas + 15, actualOrigin.Y), 12, axisBrush, axisThickness);

            // 绘制Y轴（从原点向上）
            DrawArrowLine(canvas, "+Y", new Point(actualOrigin.X, yEndCanvas),
                new Point(actualOrigin.X, yStartCanvas), 12, axisBrush, axisThickness);

            // 绘制X轴刻度和标签（覆盖正负X轴，支持第二象限）
            for (double x = xMin; x <= xMax; x += xStep)
            {
                double canvasX = actualOrigin.X + x * xScale;

                if (canvasX < margin || canvasX > canvasWidth - margin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = canvasX,
                    Y1 = actualOrigin.Y - 5,
                    X2 = canvasX,
                    Y2 = actualOrigin.Y + 5,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签（放在X轴上方，避免被底部遮挡）
                string label = x == 0 ? "0" : $"{x}{unit}";
                TextBlock tb = new TextBlock
                {
                    Text = label,
                    Foreground = labelBrush,
                    FontSize = 10
                };
                Canvas.SetLeft(tb, canvasX - 12);
                Canvas.SetTop(tb, actualOrigin.Y + 10);  // 移到X轴上方
                canvas.Children.Add(tb);

                // 垂直网格线（从Y轴顶端到底端）
                Line gridLine = new Line
                {
                    X1 = canvasX,
                    Y1 = yStartCanvas,
                    X2 = canvasX,
                    Y2 = yEndCanvas,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 绘制Y轴刻度和标签
            for (double y = 0; y <= yMax; y += yStep)
            {
                double canvasY = actualOrigin.Y - y * yScale;

                if (canvasY < margin || canvasY > canvasHeight - margin - labelMargin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = actualOrigin.X - 5,
                    Y1 = canvasY,
                    X2 = actualOrigin.X + 5,
                    Y2 = canvasY,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签（Y>0时显示）
                if (y > 0)
                {
                    TextBlock tb = new TextBlock
                    {
                        Text = $"{y}{unit}",
                        Foreground = labelBrush,
                        FontSize = 10,
                        Width = 30,
                        TextAlignment = TextAlignment.Right
                    };
                    Canvas.SetLeft(tb, Math.Max(5, actualOrigin.X - 35));
                    Canvas.SetTop(tb, canvasY - 6);
                    canvas.Children.Add(tb);
                }

                // 水平网格线（从负X轴到正X轴，覆盖第一、第二象限）
                Line gridLine = new Line
                {
                    X1 = xStartCanvas,
                    Y1 = canvasY,
                    X2 = xEndCanvas,
                    Y2 = canvasY,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 原点标记
            Ellipse originDot = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Red
            };
            Canvas.SetLeft(originDot, actualOrigin.X - 5);
            Canvas.SetTop(originDot, actualOrigin.Y - 5);
            canvas.Children.Add(originDot);

            // 返回坐标系信息（第一、第二象限Y最小值为0）
            coordinateConfig = new CoordinateSystemResult
            {
                XScale = xScale,
                YScale = yScale,
                Origin = actualOrigin,
                XMin = xMin,
                XMax = xMax,
                YMin = 0,  // 第一、第二象限Y从0开始
                YMax = yMax,
                CanvasWidth = canvasWidth,
                CanvasHeight = canvasHeight,
                Xoffset = margin,
                Yoffset = margin
            };
            return coordinateConfig;
        }

        /// <summary>
        /// 绘制第一、第二象限坐标系（简化版本，兼容旧调用）
        /// </summary>
        public static CoordinateSystemResult DrawFirstSecondQuadrant(Canvas canvas, Point origin, double width, double height,
            double xMax, double yMax, double xStep, double yStep, string unit = "m")
        {
            return DrawFirstSecondQuadrant(canvas, origin, width, height, -xMax, xMax, yMax, xStep, yStep, unit);
        }

        /// <summary>
        /// 绘制平面直角十字坐标系（完整四象限）
        /// </summary>
        /// <returns>坐标系绘制结果信息，包含缩放比例、原点坐标和显示范围</returns>
        public static CoordinateSystemResult DrawFirstCoordinate(Canvas canvas, Point origin, double width, double height, double xMax, double yMax, double xStep, double yStep, string unit = "m")
        {
            Brush axisBrush = Brushes.Red;
            Brush gridBrush = Brushes.DarkGray;
            Brush labelBrush = Brushes.White;
            double axisThickness = 1.5;
            double gridThickness = 0.5;
            double margin = 60;  // 边距（增加边距确保标签完整显示）
            double labelHeight = 10;  // 标签高度

            // 确保画布实际尺寸可用
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // 计算可用区域（扣除边距）
            double availableWidth = canvasWidth - margin * 2;
            double availableHeight = canvasHeight - margin * 2;

            // 计算坐标范围
            double xRange = xMax;
            double yRange = yMax;

            // 计算比例（确保坐标轴在画布内）
            double xScale = availableWidth / xRange;
            double yScale = availableHeight / yRange;

            // 确保比例不为零
            xScale = xScale > 0 ? xScale : 10;
            yScale = yScale > 0 ? yScale : 10;

            // 原点位置设置在画布中心
            double actualOriginX = margin;
            double actualOriginY = canvasHeight - margin;
            Point actualOrigin = new Point(actualOriginX, actualOriginY);

            // 计算轴端点（确保不溢出，完整显示正负方向）
            double xStartCanvas = actualOrigin.X;
            double xEndCanvas = actualOrigin.X + xMax * xScale;
            double yStartCanvas = actualOrigin.Y - yMax * yScale;
            double yEndCanvas = actualOrigin.Y;

            // 限制轴端点在画布边界内
            xStartCanvas = Math.Max(margin, xStartCanvas);
            xEndCanvas = Math.Min(canvasWidth - margin, xEndCanvas);
            yStartCanvas = Math.Max(margin, yStartCanvas);
            yEndCanvas = Math.Min(canvasHeight - margin, yEndCanvas);

            // 绘制X轴（从左到右）
            DrawArrowLine(canvas, "+X", new Point(actualOrigin.X, actualOrigin.Y),
                new Point(actualOriginX + availableWidth + 10, actualOrigin.Y), 12, axisBrush, axisThickness);

            // 绘制Y轴（从下到上）
            DrawArrowLine(canvas, "+Y", new Point(actualOrigin.X, actualOrigin.Y),
                new Point(actualOrigin.X, actualOrigin.Y - availableHeight + 10), 12, axisBrush, axisThickness);

            // 绘制X轴刻度和标签
            for (double x = 0; x <= xMax; x += xStep)
            {
                double canvasX = actualOrigin.X + x * xScale;

                if (canvasX < margin || canvasX > canvasWidth - margin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = canvasX,
                    Y1 = actualOrigin.Y - 5,
                    X2 = canvasX,
                    Y2 = actualOrigin.Y + 5,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签（放在X轴上方，避免被底部遮挡）
                string label = x == 0 ? "0" : $"{x}{unit}";
                TextBlock tb = new TextBlock
                {
                    Text = label,
                    Foreground = labelBrush,
                    FontSize = 10
                };
                Canvas.SetLeft(tb, canvasX - 15);
                Canvas.SetTop(tb, actualOrigin.Y + labelHeight);  // 移到X轴上方
                canvas.Children.Add(tb);

                // 垂直网格线
                Line gridLine = new Line
                {
                    X1 = canvasX,
                    Y1 = yStartCanvas,
                    X2 = canvasX,
                    Y2 = yEndCanvas,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 绘制Y轴刻度和标签
            for (double y = 0; y <= yMax; y += yStep)
            {
                double canvasY = actualOrigin.Y - y * yScale;

                if (canvasY < margin || canvasY > canvasHeight - margin)
                    continue;

                // 刻度线
                Line tick = new Line
                {
                    X1 = actualOrigin.X - 5,
                    Y1 = canvasY,
                    X2 = actualOrigin.X + 5,
                    Y2 = canvasY,
                    Stroke = axisBrush,
                    StrokeThickness = 1
                };
                canvas.Children.Add(tick);

                // 刻度标签
                string label = y == 0 ? "" : $"{y}{unit}";
                if (!string.IsNullOrEmpty(label))
                {
                    TextBlock tb = new TextBlock
                    {
                        Text = label,
                        Foreground = labelBrush,
                        FontSize = 10,
                        Width = 35,
                        TextAlignment = TextAlignment.Right
                    };
                    Canvas.SetLeft(tb, Math.Max(5, actualOrigin.X - 40));
                    Canvas.SetTop(tb, canvasY - 6);
                    canvas.Children.Add(tb);
                }

                // 水平网格线
                Line gridLine = new Line
                {
                    X1 = xStartCanvas,
                    Y1 = canvasY,
                    X2 = xEndCanvas,
                    Y2 = canvasY,
                    Stroke = gridBrush,
                    StrokeThickness = gridThickness,
                    StrokeDashArray = new DoubleCollection { 3, 3 }
                };
                canvas.Children.Add(gridLine);
            }

            // 原点标记
            Ellipse originDot = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = Brushes.White,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            Canvas.SetLeft(originDot, actualOrigin.X - 4);
            Canvas.SetTop(originDot, actualOrigin.Y - 4);
            canvas.Children.Add(originDot);

            // 返回坐标系信息
            var coordinateConfig = new CoordinateSystemResult
            {
                XScale = xScale,
                YScale = yScale,
                Origin = actualOrigin,
                XMin = 0,
                XMax = xMax,
                YMin = 0,
                YMax = yMax,
                CanvasWidth = canvasWidth,
                CanvasHeight = canvasHeight,
                Xoffset = margin,
                Yoffset = margin
            };
            return coordinateConfig;
        }
        public static Point Math2CoordinatePoint(double canvasX, double canvasY)
        {
            return Math2CoordinatePoint(coordinateConfig, canvasX, canvasY);
        }
        public static Point Math2CoordinatePoint(CoordinateSystemResult coordinate, double canvasX, double canvasY)
        {
            if (coordinate == null)
            {
                return new Point(0, 0);
            }
            var p = new Point();
            p.X = coordinate.Origin.X + canvasX * coordinate.XScale;
            if (canvasY > 0)
            {
                p.Y = coordinate.Origin.Y - canvasY * coordinate.YScale;
            }
            else
            {
                p.Y = coordinate.Origin.Y - canvasX * coordinate.YScale;
            }
            return p;
        }
        public static Point MathToCanvas(Point origin, double width, double height,
            double xMin, double xMax, double yMin, double yMax, double mathX, double mathY)
        {
            double xScale = width * 2 / (xMax - xMin);
            double yScale = height * 2 / (yMax - yMin);

            return new Point(
                origin.X + mathX * xScale,
                origin.Y - mathY * yScale
            );
        }

        public static Point MathToCanvasFirstQuadrant(Point origin, double width, double height,
            double xMin, double xMax, double yMax, double mathX, double mathY)
        {
            double xScale = width / (xMax - xMin);
            double yScale = height / yMax;

            return new Point(
                origin.X + (mathX - xMin) * xScale,
                origin.Y - mathY * yScale
            );
        }

        public static Point CanvasToMath(double canvasX, double canvasY)
        {
            double mathX = coordinateConfig.XMin + (canvasX - coordinateConfig.Xoffset) / coordinateConfig.XScale;
            double mathY = coordinateConfig.YMin + (canvasY - coordinateConfig.Yoffset) / coordinateConfig.YScale;

            return new Point(Math.Round(mathX, 1), Math.Round(mathY, 1));
        }
        #endregion

        public static void TestDrawAll(Canvas pointCanvas)
        {
            double width = 10;
            double height = 10;
            DrawHelper.DrawRect(pointCanvas, null, new Point(20, 20), 100, 80, Brushes.DarkBlue, Brushes.LightBlue, 2);
            DrawHelper.DrawCircle(pointCanvas, null, new Point(180, 60), 40, Brushes.Red, Brushes.Pink);
            DrawHelper.DrawEllipse(pointCanvas, null, new Point(280, 60), 50, 30, Brushes.Green, Brushes.LightGreen);
            DrawHelper.DrawRing(pointCanvas, null, new Point(390, 60), 45, 25, Brushes.Black, Brushes.Gold);
            DrawHelper.DrawTriangle(pointCanvas, null, new Point(60, 180), 40, Brushes.Brown, Brushes.Wheat);
            DrawHelper.DrawPentagon(pointCanvas, null, new Point(180, 180), 40, Brushes.DarkCyan, Brushes.Aqua);
            DrawHelper.DrawHexagon(pointCanvas, null, new Point(300, 180), 40, Brushes.Purple, Brushes.Lavender);
            DrawHelper.DrawLine(pointCanvas, null, new Point(20, 300), new Point(400, 300), Brushes.DarkOrange, 3);
            DrawHelper.DrawArrowLine(pointCanvas, "Y", new Point(width / 2, height), new Point(width / 2, 0), 15, Brushes.DarkOrange, 1);
            DrawHelper.DrawArrowLine(pointCanvas, "X", new Point(0, height), new Point(width, height), 15, Brushes.DarkOrange, 1);
            DrawHelper.DrawText(pointCanvas, new Point(250, 300), "测试绘图文本", Brushes.White, 14);
        }

    }
}
