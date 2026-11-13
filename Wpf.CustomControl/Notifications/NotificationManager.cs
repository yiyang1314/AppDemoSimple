using System;
using System.Collections.Generic;
using System.Windows;

namespace Wpf.CustomControl.Notifications
{
    /// <summary>
    /// 消息通知管理器，负责创建、定位和堆叠消息弹框通知。
    /// 支持全局默认配置（位置、持续时间、间距）。
    /// </summary>
    public static class NotificationManager
    {
        /// <summary>
        /// 默认位置
        /// </summary>
        public static NotificationPosition DefaultPosition = NotificationPosition.BottomRight;

        /// <summary>
        /// 默认持续时间（毫秒），0 表示不自动关闭
        /// </summary>
        public static int DefaultDurationMs = 3000;

        /// <summary>
        /// 通知之间的间距
        /// </summary>
        public static double Spacing = 10;

        /// <summary>
        /// 距离屏幕边缘的边距
        /// </summary>
        public static double Margin = 16;

        // 每个位置当前活动的通知列表
        private static readonly Dictionary<NotificationPosition, List<NotificationBox>> _activeNotifications
            = new Dictionary<NotificationPosition, List<NotificationBox>>();

        /// <summary>
        /// 显示信息通知
        /// </summary>
        public static void Info(string message, string title = "信息")
        {
            Show(message, NotificationType.Info, title);
        }

        /// <summary>
        /// 显示成功通知
        /// </summary>
        public static void Success(string message, string title = "成功")
        {
            Show(message, NotificationType.Success, title);
        }

        /// <summary>
        /// 显示警告通知
        /// </summary>
        public static void Warning(string message, string title = "警告")
        {
            Show(message, NotificationType.Warning, title);
        }

        /// <summary>
        /// 显示错误通知
        /// </summary>
        public static void Error(string message, string title = "错误")
        {
            Show(message, NotificationType.Error, title);
        }

        /// <summary>
        /// 显示通知（使用默认配置）
        /// </summary>
        public static void Show(string message,
            NotificationType type = NotificationType.Info,
            string title = null)
        {
            Show(message, type, DefaultDurationMs, DefaultPosition, title);
        }

        /// <summary>
        /// 显示通知（自定义配置）
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="type">通知类型</param>
        /// <param name="durationMs">持续时间（毫秒），0 表示不自动关闭</param>
        /// <param name="position">显示位置</param>
        /// <param name="title">标题，null 则根据类型自动选择</param>
        public static void Show(string message,
            NotificationType type,
            int durationMs,
            NotificationPosition position,
            string title = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = GetDefaultTitle(type);
            }

            // 在 UI 线程上创建窗口
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    CreateAndShow(message, type, durationMs, position, title);
                }));
            }
            else
            {
                CreateAndShow(message, type, durationMs, position, title);
            }
        }

        private static void CreateAndShow(string message,
            NotificationType type,
            int durationMs,
            NotificationPosition position,
            string title)
        {
            var box = new NotificationBox(title, message, type, durationMs, position);

            if (!_activeNotifications.ContainsKey(position))
            {
                _activeNotifications[position] = new List<NotificationBox>();
            }

            _activeNotifications[position].Add(box);

            box.ClosedByUser += delegate { RemoveNotification(box); };

            Locate(box, position);
            box.Show();
        }

        private static void RemoveNotification(NotificationBox box)
        {
            var position = box.Position;
            if (_activeNotifications.ContainsKey(position))
            {
                _activeNotifications[position].Remove(box);
                RelocateAll(position);
            }
        }

        /// <summary>
        /// 重新计算指定位置所有通知的坐标（在通知关闭后堆叠重排）
        /// </summary>
        private static void RelocateAll(NotificationPosition position)
        {
            if (!_activeNotifications.ContainsKey(position)) return;

            var list = _activeNotifications[position];
            var workArea = SystemParameters.WorkArea;

            for (int i = 0; i < list.Count; i++)
            {
                var box = list[i];
                var point = CalculatePosition(box, position, i, workArea);
                box.SetLocation(point.X, point.Y);
            }
        }

        private static void Locate(NotificationBox box, NotificationPosition position)
        {
            var workArea = SystemParameters.WorkArea;
            int index = _activeNotifications.ContainsKey(position)
                ? _activeNotifications[position].Count - 1
                : 0;
            var point = CalculatePosition(box, position, index, workArea);
            box.SetLocation(point.X, point.Y);
        }

        private static Point CalculatePosition(NotificationBox box,
            NotificationPosition position,
            int index,
            Rect workArea)
        {
            // 确保窗口已测量
            box.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var size = box.DesiredSize;
            if (size.Width == 0) size = new Size(340, 80);

            double x = 0;
            double y = 0;
            double offset = index * (size.Height + Spacing);

            switch (position)
            {
                case NotificationPosition.TopLeft:
                    x = workArea.Left + Margin;
                    y = workArea.Top + Margin + offset;
                    break;
                case NotificationPosition.TopCenter:
                    x = workArea.Left + (workArea.Width - size.Width) / 2;
                    y = workArea.Top + Margin + offset;
                    break;
                case NotificationPosition.TopRight:
                    x = workArea.Right - Margin - size.Width;
                    y = workArea.Top + Margin + offset;
                    break;
                case NotificationPosition.BottomLeft:
                    x = workArea.Left + Margin;
                    y = workArea.Bottom - Margin - size.Height - offset;
                    break;
                case NotificationPosition.BottomCenter:
                    x = workArea.Left + (workArea.Width - size.Width) / 2;
                    y = workArea.Bottom - Margin - size.Height - offset;
                    break;
                case NotificationPosition.BottomRight:
                default:
                    x = workArea.Right - Margin - size.Width;
                    y = workArea.Bottom - Margin - size.Height - offset;
                    break;
            }

            return new Point(x, y);
        }

        private static string GetDefaultTitle(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success: return "成功";
                case NotificationType.Warning: return "警告";
                case NotificationType.Error: return "错误";
                default: return "信息";
            }
        }

        /// <summary>
        /// 关闭并清除所有活动通知
        /// </summary>
        public static void CloseAll()
        {
            foreach (var pair in _activeNotifications)
            {
                foreach (var box in pair.Value)
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            box.CloseWithAnimation();
                        }));
                    }
                    else
                    {
                        box.CloseWithAnimation();
                    }
                }
            }
            _activeNotifications.Clear();
        }
    }
}
