using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Rxbit.Brigade.HalconVision.Mvvm;

namespace Rxbit.Brigade.HalconVision.ViewModels
{
    public class GraphicsWindowViewModel : ObservableObject
    {
        private int _windowId;
        private string _title;
        private double _zoomLevel = 1.0;
        private double _offsetX;
        private double _offsetY;
        private string _statusText = "就绪";
        private ObservableCollection<GraphicsShape> _shapes;
        private int _selectedToolIndex;

        public int WindowId { get => _windowId; set => SetProperty(ref _windowId, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public double ZoomLevel { get => _zoomLevel; set => SetProperty(ref _zoomLevel, value); }
        public double OffsetX { get => _offsetX; set => SetProperty(ref _offsetX, value); }
        public double OffsetY { get => _offsetY; set => SetProperty(ref _offsetY, value); }
        public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
        public ObservableCollection<GraphicsShape> Shapes { get => _shapes; set => SetProperty(ref _shapes, value); }
        public int SelectedToolIndex { get => _selectedToolIndex; set => SetProperty(ref _selectedToolIndex, value); }

        public ObservableCollection<string> Tools { get; } = new ObservableCollection<string>
        {
            "选择", "移动", "矩形", "圆形", "椭圆", "直线", "折线", "文字", "画笔", "橡皮擦"
        };

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand FitCommand { get; }

        public GraphicsWindowViewModel(int id)
        {
            _windowId = id;
            _title = $"图形窗口 [HALCON{id}]";
            _shapes = new ObservableCollection<GraphicsShape>();

            ZoomInCommand = new RelayCommand(() => { ZoomLevel = Math.Min(ZoomLevel * 1.2, 10); StatusText = $"缩放: {ZoomLevel:F2}x"; });
            ZoomOutCommand = new RelayCommand(() => { ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.1); StatusText = $"缩放: {ZoomLevel:F2}x"; });
            FitCommand = new RelayCommand(() => { ZoomLevel = 1.0; OffsetX = 0; OffsetY = 0; StatusText = "适合窗口"; });
        }
    }

    public class GraphicsShape : ObservableObject
    {
        private string _type;
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private string _color;
        private double _thickness = 2;

        public string Type { get => _type; set => SetProperty(ref _type, value); }
        public double X { get => _x; set => SetProperty(ref _x, value); }
        public double Y { get => _y; set => SetProperty(ref _y, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public string Color { get => _color; set => SetProperty(ref _color, value); }
        public double Thickness { get => _thickness; set => SetProperty(ref _thickness, value); }
    }
}