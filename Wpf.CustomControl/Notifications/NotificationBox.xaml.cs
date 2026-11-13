using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Wpf.CustomControl.Notifications
{
    /// <summary>
    /// 消息通知类型
    /// </summary>
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// 通知显示位置
    /// </summary>
    public enum NotificationPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// 消息弹框通知窗口，支持位置配置、延时自动关闭和手动关闭。
    /// 界面采用圆角阴影、左侧彩条、图标、倒计时进度条，淡入淡出动画。
    /// </summary>
    public partial class NotificationBox : Window
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _progressTimer;
        private double _remainingMs;
        private double _totalMs;
        private double _progressWidth;
        private bool _isClosing;

        public NotificationType NotifyType { get; private set; }
        public NotificationPosition Position { get; private set; }
        public int DurationMs { get; private set; }

        /// <summary>
        /// 通知被关闭时触发（无论是自动还是手动）
        /// </summary>
        public event EventHandler ClosedByUser;

        public NotificationBox()
        {
            InitializeComponent();
        }

        public NotificationBox(string title, string message,
            NotificationType type = NotificationType.Info,
            int durationMs = 3000,
            NotificationPosition position = NotificationPosition.BottomRight)
        {
            InitializeComponent();

            NotifyType = type;
            Position = position;
            DurationMs = durationMs;
            _totalMs = durationMs;
            _remainingMs = durationMs;
            _progressWidth = 338;

            TxtTitle.Text = title;
            TxtMessage.Text = message;

            ApplyTypeStyle(type);

            Loaded += NotificationBox_Loaded;
            MouseEnter += NotificationBox_MouseEnter;
            MouseLeave += NotificationBox_MouseLeave;
        }

        private void ApplyTypeStyle(NotificationType type)
        {
            Brush accent;
            string icon;

            switch (type)
            {
                case NotificationType.Success:
                    accent = TryFindResource("NotifySuccessBrush") as Brush ?? Brushes.Green;
                    icon = "✓";
                    break;
                case NotificationType.Warning:
                    accent = TryFindResource("NotifyWarningBrush") as Brush ?? Brushes.Orange;
                    icon = "!";
                    break;
                case NotificationType.Error:
                    accent = TryFindResource("NotifyErrorBrush") as Brush ?? Brushes.Red;
                    icon = "✕";
                    break;
                default:
                    accent = TryFindResource("NotifyInfoBrush") as Brush ?? Brushes.Blue;
                    icon = "i";
                    break;
            }

            AccentBar.Background = accent;
            IconBackground.Background = accent;
            ProgressBar.Background = accent;
            TxtIcon.Text = icon;
        }

        private void NotificationBox_Loaded(object sender, RoutedEventArgs e)
        {
            // 淡入动画
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            fadeIn.FillBehavior = FillBehavior.HoldEnd;
            BeginAnimation(OpacityProperty, fadeIn);

            // 启动自动关闭计时器
            if (DurationMs > 0)
            {
                StartTimers();
            }
            else
            {
                // 不自动关闭时隐藏进度条
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void StartTimers()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DurationMs) };
            _timer.Tick += delegate { AutoClose(); };

            // 进度条计时器，每 16ms 更新一次（约 60fps）
            _progressTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _progressTimer.Tick += delegate { UpdateProgress(); };

            _timer.Start();
            _progressTimer.Start();
        }

        private void UpdateProgress()
        {
            _remainingMs -= 16;
            if (_remainingMs < 0)
            {
                _remainingMs = 0;
            }

            double ratio = _totalMs > 0 ? _remainingMs / _totalMs : 0;
            ProgressBar.Width = _progressWidth * ratio;
        }

        private void NotificationBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 鼠标悬停时暂停倒计时
            if (_timer != null) _timer.Stop();
            if (_progressTimer != null) _progressTimer.Stop();
        }

        private void NotificationBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 鼠标离开时恢复倒计时
            if (_timer != null && !_isClosing) _timer.Start();
            if (_progressTimer != null && !_isClosing) _progressTimer.Start();
        }

        private void AutoClose()
        {
            CloseWithAnimation();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseWithAnimation();
        }

        /// <summary>
        /// 带淡出动画关闭窗口
        /// </summary>
        public void CloseWithAnimation()
        {
            if (_isClosing) return;
            _isClosing = true;

            if (_timer != null) _timer.Stop();
            if (_progressTimer != null) _progressTimer.Stop();

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += delegate
            {
                var handler = ClosedByUser;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
                Close();
            };
            BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// 设置窗口位置（由 NotificationManager 调用）
        /// </summary>
        public void SetLocation(double left, double top)
        {
            Left = left;
            Top = top;
        }
    }
}
