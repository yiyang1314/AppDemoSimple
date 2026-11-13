using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.CustomControl.Icons
{
    /// <summary>
    /// 矢量图标显示控件，通过 IconKind 选择内置图标，通过 Foreground 设置颜色。
    /// 基于 OnRender 直接绘制 Geometry，支持缩放（Stretch）。
    /// </summary>
    public class IconElement : Control
    {
        static IconElement()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconElement),
                new FrameworkPropertyMetadata(typeof(IconElement)));
        }

        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register("IconKind", typeof(IconKind), typeof(IconElement),
                new FrameworkPropertyMetadata(IconKind.Play, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof(Stretch), typeof(IconElement),
                new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>图标类型</summary>
        public IconKind IconKind
        {
            get { return (IconKind)GetValue(IconKindProperty); }
            set { SetValue(IconKindProperty, value); }
        }

        /// <summary>拉伸方式，默认 Uniform</summary>
        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            Geometry geometry = IconPaths.GetGeometry(IconKind);
            if (geometry == null)
            {
                return;
            }

            Brush brush = Foreground;
            if (brush == null)
            {
                brush = Brushes.Black;
            }

            // 计算缩放和平移，使图标在控件范围内居中显示
            Rect bounds = geometry.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            double scaleX = 1, scaleY = 1, offsetX = 0, offsetY = 0;
            double renderWidth = ActualWidth > 0 ? ActualWidth : Width;
            double renderHeight = ActualHeight > 0 ? ActualHeight : Height;
            if (renderWidth <= 0 || renderHeight <= 0)
            {
                renderWidth = 24;
                renderHeight = 24;
            }

            switch (Stretch)
            {
                case Stretch.None:
                    break;
                case Stretch.Fill:
                    scaleX = renderWidth / bounds.Width;
                    scaleY = renderHeight / bounds.Height;
                    break;
                case Stretch.Uniform:
                    scaleX = scaleY = System.Math.Min(renderWidth / bounds.Width, renderHeight / bounds.Height);
                    break;
                case Stretch.UniformToFill:
                    scaleX = scaleY = System.Math.Max(renderWidth / bounds.Width, renderHeight / bounds.Height);
                    break;
            }

            offsetX = (renderWidth - bounds.Width * scaleX) / 2 - bounds.X * scaleX;
            offsetY = (renderHeight - bounds.Height * scaleY) / 2 - bounds.Y * scaleY;

            drawingContext.PushTransform(new TranslateTransform(offsetX, offsetY));
            drawingContext.PushTransform(new ScaleTransform(scaleX, scaleY));
            drawingContext.DrawGeometry(brush, null, geometry);
            drawingContext.Pop();
            drawingContext.Pop();
        }
    }
}
