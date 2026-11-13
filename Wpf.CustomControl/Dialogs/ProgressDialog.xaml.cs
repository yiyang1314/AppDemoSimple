using System.Windows;

namespace Wpf.CustomControl.Dialogs
{
    /// <summary>
    /// 进度对话框，显示进度条、文件名、速度和剩余时间。
    /// 支持 Report 方法更新进度。
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public double Progress
        {
            get { return ProgressBar.Value; }
            set
            {
                ProgressBar.Value = value;
                TxtProgress.Text = System.Math.Round(value, 1) + "%";
            }
        }

        public string FileName
        {
            get { return TxtFileName.Text; }
            set { TxtFileName.Text = value; }
        }

        public string SpeedText
        {
            get { return TxtSpeed.Text; }
            set { TxtSpeed.Text = value; }
        }

        public string RemainingText
        {
            get { return TxtRemaining.Text; }
            set { TxtRemaining.Text = value; }
        }

        public bool IsIndeterminate
        {
            get { return ProgressBar.IsIndeterminate; }
            set { ProgressBar.IsIndeterminate = value; }
        }

        public ProgressDialog()
        {
            InitializeComponent();
        }

        public ProgressDialog(string title, string fileName = "")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtFileName.Text = fileName;
        }

        /// <summary>
        /// 报告进度更新
        /// </summary>
        /// <param name="progress">进度值（0-100）</param>
        /// <param name="speed">速度文本（可选）</param>
        /// <param name="remaining">剩余时间文本（可选）</param>
        public void Report(double progress, string speed = null, string remaining = null)
        {
            Progress = progress;
            if (speed != null)
            {
                SpeedText = speed;
            }
            if (remaining != null)
            {
                RemainingText = remaining;
            }
        }

        /// <summary>
        /// 显示进度对话框（非模态），返回实例供调用方更新进度
        /// </summary>
        public static ProgressDialog Show(string title, string fileName = "")
        {
            var dialog = new ProgressDialog(title, fileName);
            dialog.Show();
            return dialog;
        }

        /// <summary>
        /// 显示进度对话框（模态），返回实例供调用方更新进度
        /// </summary>
        public static ProgressDialog ShowDialog(string title, string fileName = "")
        {
            var dialog = new ProgressDialog(title, fileName);
            dialog.ShowDialog();
            return dialog;
        }
    }
}
