using System.Windows;
using System.Windows.Input;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 输入对话框，提供文本输入框，返回用户输入的值。
    /// </summary>
    public partial class InputDialog : Window
    {
        public string InputValue { get; private set; }

        public bool? DialogResultValue { get; private set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string title, string message, string defaultValue = "")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            TxtInput.Text = defaultValue;
            TxtInput.Focus();
            TxtInput.SelectAll();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            InputValue = TxtInput.Text;
            DialogResultValue = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResultValue = false;
            Close();
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnConfirm_Click(sender, null);
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancel_Click(sender, null);
            }
        }

        /// <summary>
        /// 显示输入对话框，通过 out 参数返回输入值
        /// </summary>
        public static bool? ShowDialog(string title, string message, out string inputValue)
        {
            inputValue = string.Empty;
            var dialog = new InputDialog(title, message);
            dialog.ShowDialog();
            if (dialog.DialogResultValue == true)
            {
                inputValue = dialog.InputValue;
            }
            return dialog.DialogResultValue;
        }

        /// <summary>
        /// 显示输入对话框，通过 ref 参数传入默认值并返回输入值
        /// </summary>
        public static bool? Show(string title, string message, ref string inputValue)
        {
            var dialog = new InputDialog(title, message, inputValue);
            dialog.ShowDialog();
            if (dialog.DialogResultValue == true)
            {
                inputValue = dialog.InputValue;
            }
            return dialog.DialogResultValue;
        }
    }
}
