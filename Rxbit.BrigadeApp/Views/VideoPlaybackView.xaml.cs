using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.ViewModels;
using System.Windows;

namespace Rxbit.BrigadeApp.Views
{
    /// <summary>
    /// VideoPlaybackView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlaybackView : WindowBase
    {
        private readonly VideoPlaybackViewModel _viewModel;

        public VideoPlaybackView()
        {
            InitializeComponent();
            _viewModel = new VideoPlaybackViewModel();
            DataContext = _viewModel;
            Closed += (s, e) => _viewModel.Dispose();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
