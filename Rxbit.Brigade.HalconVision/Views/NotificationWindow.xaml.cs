using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Rxbit.Brigade.HalconVision.Views
{
    public partial class NotificationWindow : Window
    {
        private DispatcherTimer _autoCloseTimer;
        private Storyboard _blinkStoryboard;
        private bool _isClosing;

        public string NotificationTitle
        {
            get => TitleText.Text;
            set => TitleText.Text = value;
        }

        public string NotificationMessage
        {
            get => MessageText.Text;
            set => MessageText.Text = value;
        }

        public string NotificationIcon
        {
            get => IconText.Text;
            set => IconText.Text = value;
        }

        public string ActionButtonText
        {
            get => ActionBtn.Content?.ToString() ?? "";
            set
            {
                ActionBtn.Content = value;
                ActionBtn.Visibility = string.IsNullOrEmpty(value)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public event Action ActionRequested;
        public event Action Closed;

        public NotificationWindow()
        {
            InitializeComponent();
            Loaded += NotificationWindow_Loaded;
            MouseLeftButtonDown += NotificationWindow_MouseLeftButtonDown;
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _blinkStoryboard = TryFindResource("BlinkStoryboard") as Storyboard;
            UpdateTime();
        }

        public void StartAutoClose(int milliseconds)
        {
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(milliseconds)
            };
            _autoCloseTimer.Tick += (s, args) =>
            {
                _autoCloseTimer.Stop();
                CloseNotification();
            };
            _autoCloseTimer.Start();
        }

        public void StartBlink()
        {
            if (_blinkStoryboard != null)
            {
                _blinkStoryboard.Begin(this, true);
            }
        }

        public void StopBlink()
        {
            if (_blinkStoryboard != null)
            {
                _blinkStoryboard.Stop(this);
            }
        }

        private void UpdateTime()
        {
            TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void NotificationWindow_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Click anywhere on notification to dismiss
            if (_autoCloseTimer != null) _autoCloseTimer.Stop();
            CloseNotification();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_autoCloseTimer != null) _autoCloseTimer.Stop();
            CloseNotification();
        }

        private void ActionBtn_Click(object sender, RoutedEventArgs e)
        {
            ActionRequested?.Invoke();
            CloseNotification();
        }

        public void CloseNotification()
        {
            if (_isClosing) return;
            _isClosing = true;
            StopBlink();

            var storyboard = TryFindResource("SlideOutStoryboard") as Storyboard;
            if (storyboard != null)
            {
                storyboard.Completed += (s, args) =>
                {
                    Closed?.Invoke();
                    Close();
                };
                storyboard.Begin(this);
            }
            else
            {
                Closed?.Invoke();
                Close();
            }
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            // Pause auto-close on hover
            if (_autoCloseTimer != null && _autoCloseTimer.IsEnabled)
            {
                _autoCloseTimer.Stop();
            }
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            // Resume auto-close on leave (3 more seconds)
            if (!_isClosing)
            {
                StartAutoClose(3000);
            }
        }
    }
}
