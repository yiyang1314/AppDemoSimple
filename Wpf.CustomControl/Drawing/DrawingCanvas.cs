using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wpf.CustomControl.Drawing
{
    /// <summary>
    /// 绘制容器，继承 Canvas，用于管理 DrawingElement 子元素。
    /// 提供 AddElement/RemoveElement/Clear 方法。
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        private readonly List<DrawingElement> _elements;

        public DrawingCanvas()
        {
            _elements = new List<DrawingElement>();
            ClipToBounds = true;
            Background = Brushes.Transparent;
        }

        /// <summary>
        /// 添加绘制元素
        /// </summary>
        public void AddElement(DrawingElement element)
        {
            if (element == null)
            {
                return;
            }
            _elements.Add(element);
            Children.Add(element);
            InvalidateVisual();
        }

        /// <summary>
        /// 移除绘制元素
        /// </summary>
        public void RemoveElement(DrawingElement element)
        {
            if (element == null)
            {
                return;
            }
            _elements.Remove(element);
            Children.Remove(element);
            InvalidateVisual();
        }

        /// <summary>
        /// 清除所有绘制元素
        /// </summary>
        public void ClearElements()
        {
            _elements.Clear();
            Children.Clear();
            InvalidateVisual();
        }

        /// <summary>
        /// 获取所有绘制元素
        /// </summary>
        public IReadOnlyList<DrawingElement> Elements
        {
            get { return _elements.AsReadOnly(); }
        }
    }
}
