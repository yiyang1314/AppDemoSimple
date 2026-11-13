using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Rxbit.Brigade.HalconVision.Views
{
    public partial class MdiChildWindow : UserControl
    {
        private bool _isDragging;
        private Point _dragStartPoint;
        private Point _windowStartPoint;
        private bool _isMaximized;
        private Rect _normalBounds;
        // Resize state (right-bottom corner)
        private bool _isResizing;
        private Point _resizeStartScreenPoint;
        private Size _resizeStartSize;

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(MdiChildWindow),
                new PropertyMetadata("子窗口", OnTitleChanged));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(MdiChildWindow),
                new PropertyMetadata("📄", OnIconChanged));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(MdiChildWindow),
                new PropertyMetadata(false, OnIsActiveChanged));

        public static readonly DependencyProperty ChildContentProperty =
            DependencyProperty.Register("ChildContent", typeof(object), typeof(MdiChildWindow),
                new PropertyMetadata(null, OnChildContentChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Icon
        {
            get => (string)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public object ChildContent
        {
            get => GetValue(ChildContentProperty);
            set => SetValue(ChildContentProperty, value);
        }

        public event EventHandler CloseRequested;
        public event EventHandler Activated;

        public MdiChildWindow()
        {
            InitializeComponent();
            TitleText.Text = Title;
            IconBlock.Text = Icon;
            ContentPresenter.Content = ChildContent;

            // PreviewMouseLeftButtonDown on TitleBar starts drag (tunnel event avoids child control blocking)
            TitleBar.PreviewMouseLeftButtonDown += TitleBar_PreviewMouseLeftButtonDown;
            TitleBar.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(TitleBar_MouseDoubleClick));

            // Move/Up on whole window ensure drag works even when cursor leaves TitleBar
            MouseMove += MdiChildWindow_MouseMove;
            MouseLeftButtonUp += MdiChildWindow_MouseLeftButtonUp;

            MouseLeftButtonDown += MdiChildWindow_MouseLeftButtonDown;

            // Right-bottom corner resize via Thumb (no visible grip icon, only SizeNWSE cursor)
            ResizeGripRight.DragStarted += ResizeGripRight_DragStarted;
            ResizeGripRight.DragDelta += ResizeGripRight_DragDelta;
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MdiChildWindow window && window.TitleText != null)
                window.TitleText.Text = (string)e.NewValue;
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MdiChildWindow window && window.IconBlock != null)
                window.IconBlock.Text = (string)e.NewValue;
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MdiChildWindow window)
            {
                window.UpdateActiveStyle();
            }
        }

        private static void OnChildContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MdiChildWindow window && window.ContentPresenter != null)
                window.ContentPresenter.Content = e.NewValue;
        }

        private void UpdateActiveStyle()
        {
            if (IsActive)
            {
                TitleBar.Background = new SolidColorBrush(Color.FromRgb(0, 120, 212));
                TitleText.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
            else
            {
                TitleBar.Background = new SolidColorBrush(Color.FromRgb(58, 58, 79));
                TitleText.Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            }
        }

        private void MdiChildWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Activated?.Invoke(this, EventArgs.Empty);
            IsActive = true;
        }

        private void TitleBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_isMaximized) return;

            // Skip drag if user clicked on a button (minimize/maximize/close)
            if (e.OriginalSource is DependencyObject src)
            {
                var btn = FindAncestor<ButtonBase>(src);
                if (btn != null) return;
            }

            _isDragging = true;
            _dragStartPoint = e.GetPosition(null);
            _windowStartPoint = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));

            if (Parent is Canvas canvas)
            {
                Panel.SetZIndex(this, 1000);
            }

            IsActive = true;
            Activated?.Invoke(this, EventArgs.Empty);
            CaptureMouse();
            e.Handled = true;
        }

        private void MdiChildWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentPoint = e.GetPosition(null);
            Vector delta = currentPoint - _dragStartPoint;

            double newLeft = _windowStartPoint.X + delta.X;
            double newTop = _windowStartPoint.Y + delta.Y;

            if (Parent is Canvas canvas)
            {
                double maxLeft = canvas.ActualWidth - ActualWidth - 10;
                double maxTop = canvas.ActualHeight - ActualHeight - 10;

                newLeft = Math.Max(0, Math.Min(newLeft, maxLeft));
                newTop = Math.Max(0, Math.Min(newTop, maxTop));
            }

            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
            e.Handled = true;
        }

        private void MdiChildWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }
                e.Handled = true;
            }
        }

        private static T FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            DependencyObject current = element;
            while (current != null)
            {
                if (current is T result) return result;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void TitleBar_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (_isMaximized)
            {
                Canvas.SetLeft(this, _normalBounds.X);
                Canvas.SetTop(this, _normalBounds.Y);
                Width = _normalBounds.Width;
                Height = _normalBounds.Height;
                _isMaximized = false;
                MaximizeBtn.Content = "▢";
            }
            else
            {
                _normalBounds = new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), Width, Height);

                if (Parent is Canvas canvas)
                {
                    Canvas.SetLeft(this, 0);
                    Canvas.SetTop(this, 0);
                    Width = canvas.ActualWidth;
                    Height = canvas.ActualHeight;
                }
                _isMaximized = true;
                MaximizeBtn.Content = "❐";
            }
        }

        private void ResizeGripRight_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (_isMaximized) return;
            _isResizing = true;
            _resizeStartScreenPoint = PointToScreen(new Point(0, 0));
            _resizeStartSize = new Size(ActualWidth, ActualHeight);
            // Bring to front while resizing
            if (Parent is Canvas) Panel.SetZIndex(this, 1000);
        }

        private void ResizeGripRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (!_isResizing || _isMaximized) return;

            double newWidth = _resizeStartSize.Width + e.HorizontalChange;
            double newHeight = _resizeStartSize.Height + e.VerticalChange;

            // Clamp to MinWidth/MinHeight and canvas bounds
            newWidth = Math.Max(MinWidth, newWidth);
            newHeight = Math.Max(MinHeight, newHeight);

            if (Parent is Canvas canvas)
            {
                double currentLeft = Canvas.GetLeft(this);
                double currentTop = Canvas.GetTop(this);
                double maxAllowedWidth = canvas.ActualWidth - currentLeft - 4;
                double maxAllowedHeight = canvas.ActualHeight - currentTop - 4;
                newWidth = Math.Min(newWidth, Math.Max(MinWidth, maxAllowedWidth));
                newHeight = Math.Min(newHeight, Math.Max(MinHeight, maxAllowedHeight));
            }

            Width = newWidth;
            Height = newHeight;
            e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }
    }
}