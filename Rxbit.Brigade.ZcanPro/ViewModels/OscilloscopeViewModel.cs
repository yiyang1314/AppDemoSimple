using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using Rxbit.Brigade.ZcanPro.Mvvm;

namespace Rxbit.Brigade.ZcanPro.ViewModels
{
    public class OscilloscopeViewModel : ObservableObject
    {
        private int _viewId;
        private string _title;
        private ObservableCollection<ISeries> _series;
        private bool _isRunning = true;
        private string _statusText = "运行中";

        public int ViewId { get => _viewId; set => SetProperty(ref _viewId, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ObservableCollection<ISeries> Series { get => _series; set => SetProperty(ref _series, value); }
        public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }

        public ObservableCollection<Axis> XAxes { get; }
        public ObservableCollection<Axis> YAxes { get; }

        public ObservableCollection<string> ChannelOptions { get; }
        public string SelectedChannel { get; set; }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand CaptureCommand { get; }

        public OscilloscopeViewModel(int id)
        {
            _viewId = id;
            _title = $"示波器视图 {id}";

            ChannelOptions = new ObservableCollection<string> { "CH1", "CH2", "CH3", "CH4" };
            SelectedChannel = "CH1";

            var values1 = new ObservableCollection<double>();
            var values2 = new ObservableCollection<double>();
            var random = new Random();
            for (int i = 0; i < 200; i++)
            {
                double t = i * 0.1;
                values1.Add(Math.Sin(t * 5) * 3 + Math.Sin(t * 12) * 1.5 + (random.NextDouble() - 0.5) * 0.5);
                values2.Add(Math.Cos(t * 3) * 2.5 + Math.Sin(t * 8) * 1 + (random.NextDouble() - 0.5) * 0.5);
            }

            _series = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Name = "CH1 - CAN_H",
                    Values = values1,
                    Fill = null,
                    GeometrySize = 2
                },
                new LineSeries<double>
                {
                    Name = "CH2 - CAN_L",
                    Values = values2,
                    Fill = null,
                    GeometrySize = 2
                }
            };

            XAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "时间 (s)",
                    MinLimit = 0,
                    MaxLimit = 20,
                    MinStep = 5
                }
            };

            YAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "电压 (V)",
                    MinLimit = -8,
                    MaxLimit = 8,
                    MinStep = 2
                }
            };

            StartCommand = new RelayCommand(() => { IsRunning = true; StatusText = "运行中"; });
            StopCommand = new RelayCommand(() => { IsRunning = false; StatusText = "已停止"; });
            ResetCommand = new RelayCommand(() => { StatusText = "已重置"; });
            CaptureCommand = new RelayCommand(() => { StatusText = "已捕获"; });
        }
    }
}