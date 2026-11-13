using System.Windows;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 警告对话框，显示警告图标和提示内容，单按钮确认。
    /// </summary>
    public partial class WarningDialog : Window
    {
        public bool? DialogResultValue { get; private set; }

        public WarningDialog()
        {
            InitializeComponent();
        }

        public WarningDialog(string title, string content)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtContent.Text = content;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = true;
            Close();
        }

        /// <summary>
        /// 显示警告对话框
        /// </summary>
        public static bool? Show(string content)
        {
            return Show("警告", content);
        }

        /// <summary>
        /// 显示警告对话框（带自定义标题）
        /// </summary>
        public static bool? Show(string title, string content)
        {
            var dialog = new WarningDialog(title, content);
            dialog.ShowDialog();
            return dialog.DialogResultValue;
        }
    }
}
