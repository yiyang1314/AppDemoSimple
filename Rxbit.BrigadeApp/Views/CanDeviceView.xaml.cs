
using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System.Windows;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// CanDeviceView.xaml 的交互逻辑
    /// </summary>
    public partial class CanDeviceView : WindowBase
    {
        private CanDeviceViewModel Vm;
        public CanDeviceView()
        {
            InitializeComponent();
            Vm = this.DataContext as CanDeviceViewModel;
            Vm.OpenSuccess = ShowWin;
        }

        private void ShowWin(bool show)
        {
            // 设备打开成功后关闭配置窗口（隐藏会让 ShowDialog 一直阻塞）
            this.Close();
        }
    }
}