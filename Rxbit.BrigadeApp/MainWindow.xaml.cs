using Rxbit.BrigadeApp.Controls;
using Rxbit.BrigadeApp.Views;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp
{
    public partial class MainWindow : WindowBase
    {
        private DispatcherTimer _timer;
        private string _currentTime;

        public string CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = value;
                OnPropertyChanged("CurrentTime");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            this.Closed += MainWindow_Closed;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            AboutView aboutView = new AboutView();
            aboutView.Show();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutView aboutView = new AboutView();
            aboutView.Show();
        }
    }
}
