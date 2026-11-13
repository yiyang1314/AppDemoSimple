using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.BrigadeChm.Models;
using Rxbit.BrigadeChm.ViewModels;

namespace Rxbit.BrigadeChm.Views
{
    public partial class HelpPreviewView : Window
    {
        private HelpPreviewViewModel _viewModel;
        private string _currentHtmlPath;

        public HelpPreviewView()
        {
            InitializeComponent();
            _viewModel = new HelpPreviewViewModel();
            DataContext = _viewModel;
            _viewModel.NavigateRequest += OnNavigateRequest;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            Loaded += HelpPreviewView_Loaded;
            Closed += HelpPreviewView_Closed;
        }

        private void HelpPreviewView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigateToHtml(_viewModel.CurrentHtmlPath);
            if (HelpTreeView.Items.Count > 0)
            {
                var firstSection = HelpTreeView.Items[0] as HelpSection;
                if (firstSection != null && firstSection.Topics.Count > 0)
                {
                    NavigateToTopic(firstSection.Topics[0]);
                    SelectTopicInTree(firstSection.Topics[0]);
                }
            }
        }

        private void HelpPreviewView_Closed(object sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.NavigateRequest -= OnNavigateRequest;
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
        }

        private void OnNavigateRequest(object sender, string navigateArg)
        {
            if (navigateArg == "back" || navigateArg == "forward" || navigateArg == "refresh")
            {
                if (!string.IsNullOrEmpty(_currentHtmlPath))
                    NavigateToHtml(_currentHtmlPath);
                return;
            }

            NavigateToHtml(navigateArg);
        }

        private void NavigateToHtml(string htmlPath)
        {
            if (string.IsNullOrEmpty(htmlPath)) return;

            _currentHtmlPath = htmlPath;

            try
            {
                if (File.Exists(htmlPath))
                {
                    var uri = new Uri(Path.GetFullPath(htmlPath));
                    WebBrowser.Navigate(uri);
                }
                else
                {
                    var dir = Path.GetDirectoryName(htmlPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                        WebBrowser.Navigate(new Uri(Path.GetFullPath(htmlPath)));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"导航失败: {ex.Message}");
            }
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HelpPreviewViewModel.SearchResults))
            {
                if (_viewModel.SearchResults != null && _viewModel.SearchResults.Count > 0)
                {
                    SearchResultsBorder.Visibility = Visibility.Visible;
                }
                else
                {
                    SearchResultsBorder.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void WebBrowser_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void WebBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is HelpTopic topic)
            {
                NavigateToTopic(topic);
            }
            else if (e.NewValue is HelpSection section && section.Topics.Count > 0)
            {
                NavigateToTopic(section.Topics[0]);
            }
        }

        private void HelpTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (HelpTreeView.SelectedItem is HelpTopic topic)
            {
                NavigateToTopic(topic);
            }
            else if (HelpTreeView.SelectedItem is HelpSection section && section.Topics.Count > 0)
            {
                NavigateToTopic(section.Topics[0]);
            }
        }

        private void NavigateToTopic(HelpTopic topic)
        {
            if (topic == null) return;

            var htmlDir = Path.Combine(_viewModel.OutputDirectory, "html");
            Directory.CreateDirectory(htmlDir);

            var htmlPath = Path.Combine(htmlDir, topic.FileName);
            if (!File.Exists(htmlPath))
            {
                var html = _viewModel.GetHtmlForTopic(topic);
                File.WriteAllText(htmlPath, html);
            }

            _viewModel.CurrentHtmlPath = htmlPath;
            _viewModel.CurrentTopicTitle = topic.Title;
            NavigateToHtml(htmlPath);
        }

        private void SearchResult_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is HelpTopic topic)
            {
                NavigateToTopic(topic);
                SelectTopicInTree(topic);
                e.Handled = true;
            }
        }

        private void SelectTopicInTree(HelpTopic topic)
        {
            if (topic == null) return;

            for (int i = 0; i < HelpTreeView.Items.Count; i++)
            {
                var sectionItem = HelpTreeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (sectionItem == null) continue;

                var section = HelpTreeView.Items[i] as HelpSection;
                if (section == null) continue;

                for (int j = 0; j < section.Topics.Count; j++)
                {
                    var topicItem = sectionItem.ItemContainerGenerator.ContainerFromIndex(j) as TreeViewItem;
                    if (topicItem != null && topicItem.DataContext == topic)
                    {
                        topicItem.IsSelected = true;
                        topicItem.BringIntoView();
                        return;
                    }
                }
            }
        }
    }
}
