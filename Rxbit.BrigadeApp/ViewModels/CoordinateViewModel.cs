using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Rxbit.BrigadeApp.Controls;
using Rxbit.Brigade.Models.Commands;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class CoordinateViewModel : INotifyPropertyChanged
    {
        #region 属性

        private List<string> _coordinateTypes = new List<string> { "Cartesian", "Radar", "Grid" };
        public List<string> CoordinateTypes
        {
            get => _coordinateTypes;
            set { _coordinateTypes = value; OnPropertyChanged(); }
        }

        private string _selectedCoordinateType = "Cartesian";
        public string SelectedCoordinateType
        {
            get => _selectedCoordinateType;
            set
            {
                _selectedCoordinateType = value;
                OnPropertyChanged();
                UpdateCoordinateType();
            }
        }

        private CoordinateSystemType _coordinateType = CoordinateSystemType.Cartesian;
        public CoordinateSystemType CoordinateType
        {
            get => _coordinateType;
            set { _coordinateType = value; OnPropertyChanged(); }
        }

        private double _xRange = 50;
        public double XRange
        {
            get => _xRange;
            set { _xRange = value; OnPropertyChanged(); }
        }

        private double _yRange = 50;
        public double YRange
        {
            get => _yRange;
            set { _yRange = value; OnPropertyChanged(); }
        }

        private double _xScale = 5;
        public double XScale
        {
            get => _xScale;
            set { _xScale = value; OnPropertyChanged(); }
        }

        private double _yScale = 5;
        public double YScale
        {
            get => _yScale;
            set { _yScale = value; OnPropertyChanged(); }
        }

        private double _radarRadius = 50;
        public double RadarRadius
        {
            get => _radarRadius;
            set { _radarRadius = value; OnPropertyChanged(); }
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set { _isScanning = value; OnPropertyChanged(); }
        }

        private ObservableCollection<TargetInfo> _targets = new ObservableCollection<TargetInfo>();
        public ObservableCollection<TargetInfo> Targets
        {
            get => _targets;
            set { _targets = value; OnPropertyChanged(); }
        }

        private TargetInfo _selectedTarget;
        public TargetInfo SelectedTarget
        {
            get => _selectedTarget;
            set { _selectedTarget = value; OnPropertyChanged(); }
        }

        #endregion

        #region 命令

        public ICommand StartScanCommand { get; }
        public ICommand AddTargetCommand { get; }
        public ICommand ClearTargetsCommand { get; }

        #endregion

        #region 构造函数

        public CoordinateViewModel()
        {
            StartScanCommand = new RelayCommand(StartScan);
            AddTargetCommand = new RelayCommand(AddTarget);
            ClearTargetsCommand = new RelayCommand(ClearTargets);

            // 添加初始目标
            AddInitialTargets();
        }

        #endregion

        #region 方法

        private void UpdateCoordinateType()
        {
            switch (SelectedCoordinateType)
            {
                case "Cartesian":
                    CoordinateType = CoordinateSystemType.Cartesian;
                    break;
                case "Radar":
                    CoordinateType = CoordinateSystemType.Radar;
                    break;
                case "Grid":
                    CoordinateType = CoordinateSystemType.Grid;
                    break;
            }
        }

        private void AddInitialTargets()
        {
            Targets.Add(new TargetInfo
            {
                Id = "T001",
                X = 20,
                Y = 30,
                Distance = 36.06,
                Angle = 56.31,
                Type = "Car",
                Status = "Detected",
                FillColor = Brushes.Red
            });

            Targets.Add(new TargetInfo
            {
                Id = "T002",
                X = -15,
                Y = 25,
                Distance = 29.15,
                Angle = 120.96,
                Type = "Pedestrian",
                Status = "Detected",
                FillColor = Brushes.Yellow
            });

            Targets.Add(new TargetInfo
            {
                Id = "T003",
                X = 35,
                Y = -10,
                Distance = 36.40,
                Angle = -15.95,
                Type = "Bicycle",
                Status = "Detected",
                FillColor = Brushes.Green
            });
        }

        private void StartScan()
        {
            IsScanning = !IsScanning;
            
            if (IsScanning)
            {
                // 模拟扫描过程中添加动态目标
                SimulateScanning();
            }
        }

        private void SimulateScanning()
        {
            var random = new Random();
            int targetId = Targets.Count + 1;

            for (int i = 0; i < 5 && IsScanning; i++)
            {
                double x = (random.NextDouble() - 0.5) * XRange * 2;
                double y = (random.NextDouble() - 0.5) * YRange * 2;

                Targets.Add(new TargetInfo
                {
                    Id = $"T{targetId:D3}",
                    X = x,
                    Y = y,
                    Distance = Math.Sqrt(x * x + y * y),
                    Angle = Math.Atan2(y, x) * 180 / Math.PI,
                    Type = GetRandomTargetType(),
                    Status = "Detected",
                    FillColor = GetRandomColor()
                });

                targetId++;
                System.Threading.Thread.Sleep(500);
            }
        }

        private void AddTarget()
        {
            var random = new Random();
            int targetId = Targets.Count + 1;

            double x = (random.NextDouble() - 0.5) * XRange * 2;
            double y = (random.NextDouble() - 0.5) * YRange * 2;

            Targets.Add(new TargetInfo
            {
                Id = $"T{targetId:D3}",
                X = x,
                Y = y,
                Distance = Math.Sqrt(x * x + y * y),
                Angle = Math.Atan2(y, x) * 180 / Math.PI,
                Type = GetRandomTargetType(),
                Status = "Detected",
                FillColor = GetRandomColor()
            });
        }

        private void ClearTargets()
        {
            Targets.Clear();
            SelectedTarget = null;
        }

        private string GetRandomTargetType()
        {
            var types = new List<string> { "Car", "Pedestrian", "Bicycle", "Truck", "Motorcycle" };
            return types[new Random().Next(types.Count)];
        }

        private Brush GetRandomColor()
        {
            var colors = new List<Brush> { Brushes.Red, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Orange };
            return colors[new Random().Next(colors.Count)];
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}