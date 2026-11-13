using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace Rxbit.BrigadeApp.ViewModels
{
    public class BirdsEyeViewModel : INotifyPropertyChanged
    {
        private List<DetectionPoint> _detectionPoints;
        private DetectionPoint _selectedPoint;
        private string _sensorId;

        public List<DetectionPoint> DetectionPoints
        {
            get => _detectionPoints;
            set
            {
                _detectionPoints = value;
                OnPropertyChanged();
            }
        }

        public DetectionPoint SelectedPoint
        {
            get => _selectedPoint;
            set
            {
                if (_selectedPoint != null)
                    _selectedPoint.IsSelected = false;
                
                _selectedPoint = value;
                
                if (_selectedPoint != null)
                    _selectedPoint.IsSelected = true;
                
                OnPropertyChanged();
                SyncTargetSelection();
            }
        }

        public string SensorId
        {
            get => _sensorId;
            set
            {
                _sensorId = value;
                OnPropertyChanged();
            }
        }

        public BirdsEyeViewModel()
        {
            SensorId = "0x310";
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            DetectionPoints = new List<DetectionPoint>
            {
                new DetectionPoint { Id = 1, X = 165, Y = 270, Distance = 5.25, Angle = 0, IsSelected = false },
                new DetectionPoint { Id = 2, X = 240, Y = 230, Distance = 4.25, Angle = 23, IsSelected = true },
                new DetectionPoint { Id = 3, X = 110, Y = 170, Distance = 8.25, Angle = -30, IsSelected = false },
                new DetectionPoint { Id = 4, X = 290, Y = 150, Distance = 6.85, Angle = 45, IsSelected = false }
            };
        }

        public void SelectPoint(int id)
        {
            var point = DetectionPoints.FirstOrDefault(p => p.Id == id);
            if (point != null)
            {
                SelectedPoint = point;
            }
        }

        private void SyncTargetSelection()
        {
            var targetId = SelectedPoint?.Id ?? 0;
            var detectionVM = ViewModelLocator.GetService<DetectionSensorInfoViewModel>();
            var networkVM = ViewModelLocator.GetService<NetworkSummaryViewModel>();
            
            if (detectionVM != null)
            {
                detectionVM.SelectedColumnIndex = targetId;
            }
            if (networkVM != null)
            {
                networkVM.SelectedObjectIndex = targetId;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DetectionPoint : INotifyPropertyChanged
    {
        private bool _isSelected;
        private Brush _fillColor;

        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Distance { get; set; }
        public double Angle { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                FillColor = _isSelected ? new SolidColorBrush(Color.FromRgb(0, 217, 255)) : new SolidColorBrush(Color.FromRgb(128, 128, 128));
                OnPropertyChanged();
            }
        }

        public Brush FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                OnPropertyChanged();
            }
        }

        public DetectionPoint()
        {
            FillColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
