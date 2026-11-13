using System.Windows;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 确认对话框，显示标题、内容和确认/取消按钮，返回用户选择结果。
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public bool? DialogResultValue { get; private set; }

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public ConfirmDialog(string title, string content, string confirmText = "确定", string cancelText = "取消")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtContent.Text = content;
            BtnConfirm.Content = confirmText;
            BtnCancel.Content = cancelText;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = false;
            Close();
        }

        /// <summary>
        /// 显示确认对话框，返回 true（确认）/ false（取消）/ null（关闭）
        /// </summary>
        public static bool? Show(string title, string content, string confirmText = "确定", string cancelText = "取消")
        {
            var dialog = new ConfirmDialog(title, content, confirmText, cancelText);
            dialog.ShowDialog();
            return dialog.DialogResultValue;
        }

        /// <summary>
        /// 显示确认对话框（简化版），标题默认为"确认"
        /// </summary>
        public static bool? Show(string content)
        {
            return Show("确认", content);
        }
    }
}
