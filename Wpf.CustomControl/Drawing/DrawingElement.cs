using System.Windows;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 绘制元素抽象基类，基于 FrameworkElement + DrawingVisual，
    /// 通过 OnRender 调用 OnDraw 进行自定义绘制。
    /// 提供 Stroke、StrokeThickness、Fill 等通用依赖属性。
    /// </summary>
    public abstract class DrawingElement : FrameworkElement
    {
        private readonly DrawingVisual _visual;

        protected DrawingElement()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);
            AddLogicalChild(_visual);
        }

        #region 依赖属性

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register("Stroke", typeof(Brush), typeof(DrawingElement),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(DrawingElement),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(DrawingElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>描边画刷</summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        /// <summary>描边粗细</summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        /// <summary>填充画刷</summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        #endregion

        /// <summary>
        /// 获取当前描边画刷对应的 Pen，缓存以提升性能
        /// </summary>
        protected Pen GetPen()
        {
            if (Stroke == null)
            {
                return null;
            }
            Pen pen = new Pen(Stroke, StrokeThickness);
            return pen;
        }

        /// <summary>
        /// 子类重写此方法进行具体绘制
        /// </summary>
        protected abstract void OnDraw(DrawingContext drawingContext);

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            DrawingContext dc = _visual.RenderOpen();
            try
            {
                OnDraw(dc);
            }
            finally
            {
                dc.Close();
            }
            drawingContext.DrawDrawing(_visual.Drawing);
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new System.ArgumentOutOfRangeException("index");
            }
            return _visual;
        }
    }
}
