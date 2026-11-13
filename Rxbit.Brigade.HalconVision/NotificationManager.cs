using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Rxbit.Brigade.HalconVision.Views;

namespace Rxbit.Brigade.HalconVision
{
    public class NotificationManager
    {
        private static NotificationManager _instance;
        public static NotificationManager Instance => _instance ??= new NotificationManager();

        private readonly List<NotificationWindow> _activeNotifications = new List<NotificationWindow>();
        private readonly Queue<NotificationRequest> _pendingQueue = new Queue<NotificationRequest>();
        private const int MaxVisible = 3;

        private static Rectangle GetWorkArea()
        {
            return Screen.PrimaryScreen.WorkingArea;
        }

        public void ShowNotification(string title, string message, string icon = "ℹ",
            string actionText = null, Action actionCallback = null, int autoCloseMs = 5000)
        {
            var request = new NotificationRequest
            {
                Title = title,
                Message = message,
                Icon = icon,
                ActionText = actionText,
                ActionCallback = actionCallback,
                AutoCloseMs = autoCloseMs
            };

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_activeNotifications.Count < MaxVisible)
                {
                    CreateAndShow(request);
                }
                else
                {
                    _pendingQueue.Enqueue(request);
                }
            }), DispatcherPriority.Background);
        }

        private void CreateAndShow(NotificationRequest request)
        {
            var window = new NotificationWindow
            {
                NotificationTitle = request.Title,
                NotificationMessage = request.Message,
                NotificationIcon = request.Icon,
                ActionButtonText = request.ActionText
            };

            if (request.ActionCallback != null)
            {
                window.ActionRequested += () => request.ActionCallback();
            }

            window.Closed += () =>
            {
                _activeNotifications.Remove(window);
                RepositionRemaining();

                if (_pendingQueue.Count > 0 && _activeNotifications.Count < MaxVisible)
                {
                    var next = _pendingQueue.Dequeue();
                    CreateAndShow(next);
                }
            };

            _activeNotifications.Add(window);

            var workArea = GetWorkArea();
            double windowWidth = 360;
            double windowHeight = 130;
            double right = workArea.Right;
            double bottom = workArea.Bottom;

            // Set initial position off-screen to the right
            window.Left = right + 50;
            window.Top = bottom - windowHeight - 20;

            window.Show();

            // Trigger slide-in animation
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                var storyboard = window.TryFindResource("SlideInStoryboard") as System.Windows.Media.Animation.Storyboard;
                if (storyboard != null)
                {
                    var animations = storyboard.Children;
                    if (animations.Count > 0 && animations[0] is System.Windows.Media.Animation.DoubleAnimation anim)
                    {
                        var targetLeft = right - windowWidth - 20;
                        anim.To = targetLeft;
                    }
                    storyboard.Begin(window);
                }

                window.StartBlink();

                if (request.AutoCloseMs > 0)
                {
                    window.StartAutoClose(request.AutoCloseMs);
                }
            }), DispatcherPriority.Loaded);
        }

        private void RepositionRemaining()
        {
            var workArea = GetWorkArea();
            double windowWidth = 360;
            double windowHeight = 130;
            double right = workArea.Right;
            double bottom = workArea.Bottom;

            for (int i = 0; i < _activeNotifications.Count; i++)
            {
                var win = _activeNotifications[i];
                double newLeft = right - windowWidth - 20;
                double newTop = bottom - windowHeight - 20 - i * (windowHeight + 10);

                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    To = newLeft,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                win.BeginAnimation(Window.LeftProperty, animation);

                var topAnim = new System.Windows.Media.Animation.DoubleAnimation
                {
                    To = newTop,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                win.BeginAnimation(Window.TopProperty, topAnim);
            }
        }

        public void CloseAll()
        {
            foreach (var win in new List<NotificationWindow>(_activeNotifications))
            {
                win.CloseNotification();
            }
            _pendingQueue.Clear();
        }

        private class NotificationRequest
        {
            public string Title { get; set; }
            public string Message { get; set; }
            public string Icon { get; set; }
            public string ActionText { get; set; }
            public Action ActionCallback { get; set; }
            public int AutoCloseMs { get; set; }
        }
    }
}
