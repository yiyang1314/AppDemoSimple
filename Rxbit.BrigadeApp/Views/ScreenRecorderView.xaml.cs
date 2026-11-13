using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// ScreenRecorderView.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenRecorderView : WindowBase
    {
        private readonly ScreenRecorderViewModel _viewModel;

        public ScreenRecorderView()
        {
            InitializeComponent();
            _viewModel = new ScreenRecorderViewModel();
            DataContext = _viewModel;
            Closed += (s, e) => _viewModel.Dispose();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
