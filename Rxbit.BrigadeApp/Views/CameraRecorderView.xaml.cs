using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System;
using System.Windows;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// CameraRecorderView.xaml 的交互逻辑
    /// </summary>
    public partial class CameraRecorderView : WindowBase
    {
        private readonly CameraRecorderViewModel _viewModel;

        public CameraRecorderView()
        {
            InitializeComponent();
            _viewModel = new CameraRecorderViewModel();
            DataContext = _viewModel;
            Closed += (s, e) => _viewModel.Dispose();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
