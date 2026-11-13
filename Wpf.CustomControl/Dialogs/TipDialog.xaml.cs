using System.Windows;
using System.Windows.Threading;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 提示对话框，轻量级提示，自动消失，无按钮。
    /// </summary>
    public partial class TipDialog : Window
    {
        /// <summary>
        /// 提示类型
        /// </summary>
        public enum TipType
        {
            Info,
            Success,
            Warning,
            Error
        }

        private DispatcherTimer _timer;

        public TipDialog()
        {
            InitializeComponent();
        }

        public TipDialog(string message, TipType type = TipType.Info, int durationMs = 2000)
        {
            InitializeComponent();
            TxtMessage.Text = message;

            switch (type)
            {
                case TipType.Info:
                    TxtIcon.Text = "i";
                    break;
                case TipType.Success:
                    TxtIcon.Text = "✓";
                    break;
                case TipType.Warning:
                    TxtIcon.Text = "!";
                    break;
                case TipType.Error:
                    TxtIcon.Text = "✕";
                    break;
            }

            _timer = new DispatcherTimer();
            _timer.Interval = System.TimeSpan.FromMilliseconds(durationMs);
            _timer.Tick += delegate { Close(); };
            Loaded += delegate { _timer.Start(); };
        }

        /// <summary>
        /// 显示提示框，自动消失（非模态）
        /// </summary>
        public static void Show(string message, TipType type = TipType.Info, int durationMs = 2000)
        {
            var dialog = new TipDialog(message, type, durationMs);
            dialog.Show();
        }

        /// <summary>
        /// 显示信息提示
        /// </summary>
        public static void ShowInfo(string message, int durationMs = 2000)
        {
            Show(message, TipType.Info, durationMs);
        }

        /// <summary>
        /// 显示成功提示
        /// </summary>
        public static void ShowSuccess(string message, int durationMs = 2000)
        {
            Show(message, TipType.Success, durationMs);
        }

        /// <summary>
        /// 显示警告提示
        /// </summary>
        public static void ShowWarning(string message, int durationMs = 2000)
        {
            Show(message, TipType.Warning, durationMs);
        }

        /// <summary>
        /// 显示错误提示
        /// </summary>
        public static void ShowError(string message, int durationMs = 2000)
        {
            Show(message, TipType.Error, durationMs);
        }
    }
}
