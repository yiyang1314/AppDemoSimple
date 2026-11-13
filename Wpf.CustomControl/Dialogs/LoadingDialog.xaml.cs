using System.Windows;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 加载对话框，显示旋转加载动画和提示信息，带不确定进度条。
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public string Message
        {
            get { return TxtMessage.Text; }
            set { TxtMessage.Text = value; }
        }

        public LoadingDialog()
        {
            InitializeComponent();
        }

        public LoadingDialog(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;
        }

        /// <summary>
        /// 显示加载框（非模态），返回对话框实例，调用方负责 Close
        /// </summary>
        public static LoadingDialog ShowLoading(string message = "加载中...")
        {
            var dialog = new LoadingDialog(message);
            dialog.Show();
            return dialog;
        }

        /// <summary>
        /// 显示加载框（模态）
        /// </summary>
        public static LoadingDialog ShowLoadingDialog(string message = "加载中...")
        {
            var dialog = new LoadingDialog(message);
            dialog.ShowDialog();
            return dialog;
        }
    }
}
