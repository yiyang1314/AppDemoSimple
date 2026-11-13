using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Wpf.CustomControl.Windowing
{
    [TemplatePart(Name = "PART_TitleBar", Type = typeof(Border))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_Logo", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_Title", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_MaximizeIcon", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_RestoreIcon", Type = typeof(ContentPresenter))]
    public class CustomWindow : Window
    {
        private Border _titleBar;
        private Button _minimizeButton;
        private Button _maximizeButton;
        private Button _closeButton;
        private ContentPresenter _logoPresenter;
        private Point _dragStartPoint;
        private bool _isDragging;
        private const double DragThreshold = 2.0;

        static CustomWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow),
                new FrameworkPropertyMetadata(typeof(CustomWindow)));
        }

        public static readonly DependencyProperty LogoProperty =
            DependencyProperty.Register("Logo", typeof(object), typeof(CustomWindow),
                new PropertyMetadata(null));

        public object Logo
        {
            get { return GetValue(LogoProperty); }
            set { SetValue(LogoProperty, value); }
        }

        public static readonly DependencyProperty LogoTemplateProperty =
            DependencyProperty.Register("LogoTemplate", typeof(DataTemplate), typeof(CustomWindow),
                new PropertyMetadata(null));

        public DataTemplate LogoTemplate
        {
            get { return (DataTemplate)GetValue(LogoTemplateProperty); }
            set { SetValue(LogoTemplateProperty, value); }
        }

        public static readonly DependencyProperty TitleBarBackgroundProperty =
            DependencyProperty.Register("TitleBarBackground", typeof(Brush), typeof(CustomWindow),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(14, 36, 72))));

        public Brush TitleBarBackground
        {
            get { return (Brush)GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarForegroundProperty =
            DependencyProperty.Register("TitleBarForeground", typeof(Brush), typeof(CustomWindow),
                new PropertyMetadata(Brushes.White));

        public Brush TitleBarForeground
        {
            get { return (Brush)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarHeightProperty =
            DependencyProperty.Register("TitleBarHeight", typeof(double), typeof(CustomWindow),
                new PropertyMetadata(40.0));

        public double TitleBarHeight
        {
            get { return (double)GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }

        public static readonly DependencyProperty ShowTitleBarProperty =
            DependencyProperty.Register("ShowTitleBar", typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowTitleBar
        {
            get { return (bool)GetValue(ShowTitleBarProperty); }
            set { SetValue(ShowTitleBarProperty, value); }
        }

        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register("ShowMinimizeButton", typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowMinimizeButton
        {
            get { return (bool)GetValue(ShowMinimizeButtonProperty); }
            set { SetValue(ShowMinimizeButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register("ShowMaximizeButton", typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowMaximizeButton
        {
            get { return (bool)GetValue(ShowMaximizeButtonProperty); }
            set { SetValue(ShowMaximizeButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, value); }
        }

        public static readonly DependencyProperty BorderRadiusProperty =
            DependencyProperty.Register("BorderRadius", typeof(CornerRadius), typeof(CustomWindow),
                new PropertyMetadata(new CornerRadius(0)));

        public CornerRadius BorderRadius
        {
            get { return (CornerRadius)GetValue(BorderRadiusProperty); }
            set { SetValue(BorderRadiusProperty, value); }
        }

        public static readonly DependencyProperty CanResizeProperty =
            DependencyProperty.Register("CanResize", typeof(bool), typeof(CustomWindow),
                new PropertyMetadata(true));

        public bool CanResize
        {
            get { return (bool)GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _titleBar = GetTemplateChild("PART_TitleBar") as Border;
            _minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
            _maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _logoPresenter = GetTemplateChild("PART_Logo") as ContentPresenter;

            if (_titleBar != null)
            {
                _titleBar.MouseLeftButtonDown -= TitleBar_MouseLeftButtonDown;
                _titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
                _titleBar.MouseMove -= TitleBar_MouseMove;
                _titleBar.MouseMove += TitleBar_MouseMove;
                _titleBar.MouseLeftButtonUp -= TitleBar_MouseLeftButtonUp;
                _titleBar.MouseLeftButtonUp += TitleBar_MouseLeftButtonUp;
                _titleBar.MouseLeave -= TitleBar_MouseLeave;
                _titleBar.MouseLeave += TitleBar_MouseLeave;
            }

            if (_minimizeButton != null)
            {
                _minimizeButton.Click -= MinimizeButton_Click;
                _minimizeButton.Click += MinimizeButton_Click;
            }

            if (_maximizeButton != null)
            {
                _maximizeButton.Click -= MaximizeButton_Click;
                _maximizeButton.Click += MaximizeButton_Click;
            }

            if (_closeButton != null)
            {
                _closeButton.Click -= CloseButton_Click;
                _closeButton.Click += CloseButton_Click;
            }

            if (_logoPresenter != null)
            {
                _logoPresenter.MouseLeftButtonDown -= Logo_MouseLeftButtonDown;
                _logoPresenter.MouseLeftButtonDown += Logo_MouseLeftButtonDown;
            }

            UpdateMaximizeRestoreIcons();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            UpdateMaximizeRestoreIcons();
        }

        private void UpdateMaximizeRestoreIcons()
        {
            if (WindowState == WindowState.Maximized)
            {
                MaximizeIconVisibility = Visibility.Collapsed;
                RestoreIconVisibility = Visibility.Visible;
            }
            else
            {
                MaximizeIconVisibility = Visibility.Visible;
                RestoreIconVisibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty MaximizeIconVisibilityProperty =
            DependencyProperty.Register("MaximizeIconVisibility", typeof(Visibility), typeof(CustomWindow),
                new PropertyMetadata(Visibility.Visible));

        public Visibility MaximizeIconVisibility
        {
            get { return (Visibility)GetValue(MaximizeIconVisibilityProperty); }
            set { SetValue(MaximizeIconVisibilityProperty, value); }
        }

        public static readonly DependencyProperty RestoreIconVisibilityProperty =
            DependencyProperty.Register("RestoreIconVisibility", typeof(Visibility), typeof(CustomWindow),
                new PropertyMetadata(Visibility.Collapsed));

        public Visibility RestoreIconVisibility
        {
            get { return (Visibility)GetValue(RestoreIconVisibilityProperty); }
            set { SetValue(RestoreIconVisibilityProperty, value); }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            _dragStartPoint = e.GetPosition(this);
            _isDragging = false;

            if (WindowState == WindowState.Maximized)
            {
                var mouseX = e.GetPosition(this).X;
                var ratio = mouseX / ActualWidth;
                WindowState = WindowState.Normal;
                var newWidth = ActualWidth;
                Left = PointToScreen(e.GetPosition(this)).X - newWidth * ratio;
                Top = 0;
            }

            _titleBar.CaptureMouse();
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_titleBar != null && _titleBar.IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPoint = e.GetPosition(this);
                var diff = _dragStartPoint - currentPoint;

                if (!_isDragging)
                {
                    if (Math.Abs(diff.X) > DragThreshold || Math.Abs(diff.Y) > DragThreshold)
                    {
                        _isDragging = true;
                    }
                    else
                    {
                        return;
                    }
                }

                if (_isDragging)
                {
                    var screenPoint = PointToScreen(currentPoint);
                    Left = screenPoint.X - _dragStartPoint.X;
                    Top = screenPoint.Y - _dragStartPoint.Y;
                }
            }
        }

        private void TitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_titleBar != null && _titleBar.IsMouseCaptured)
            {
                _titleBar.ReleaseMouseCapture();
            }
            _isDragging = false;
        }

        private void TitleBar_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_titleBar != null && _titleBar.IsMouseCaptured)
            {
                _titleBar.ReleaseMouseCapture();
            }
        }

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Close();
                return;
            }
            ShowSystemMenu();
        }

        private void ShowSystemMenu()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var point = PointToScreen(Mouse.GetPosition(this));
                var menu = GetSystemMenu(hwnd, false);
                if (menu != IntPtr.Zero)
                {
                    TrackPopupMenu(menu, 0, (int)point.X, (int)point.Y, 0, hwnd, IntPtr.Zero);
                    DestroyMenu(menu);
                }
            }
            catch
            {
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }
    }
}
