using System;
using System.Windows.Media;

namespace Rxbit.Brigade.Models.Models
{
    /// <summary>
    /// 雷达目标数据模型
    /// </summary>
    public class RadarTarget : ViewModelBase
    {
        private byte _id;
        private double _x;
        private double _y;
        private double _PolarRadius;
        private double _PolarAngle;

        private double _vx;
        private double _vy;
        private double _vr;

        private double _width = 0.8;
        private double _height = 0.8;
        private string _type = "unknown";

        private int motionType = -1;
        private int _ClassType = -1;

        private DateTime _detectTime = DateTime.Now;
        private string _status = "-";
        private Brush _fillBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));
        private Brush _strokeBrush = Brushes.Cyan;
        private double _strokeThickness = 2;
        private bool _isSelected;

        public Byte Id
        {
            get => _id;
            set { Set(ref _id, value); }
        }

        public double X
        {
            get => _x;
            set { Set(ref _x, value); }
        }

        public double Y
        {
            get => _y;
            set { Set(ref _y, value); }
        }
        public double PolarRadius
        {
            get => _PolarRadius;
            set { Set(ref _PolarRadius, value); }
        }

        public double PolarAngle
        {
            get => _PolarAngle;
            set { Set(ref _PolarAngle, value); }
        }

        public double VX
        {
            get => _vx;
            set { Set(ref _vx, value); }
        }

        public double VY
        {
            get => _vy;
            set { Set(ref _vy, value); }
        }

        public double VR
        {
            get => _vr;
            set { Set(ref _vr, value); }
        }

        public double Width
        {
            get => _width;
            set { Set(ref _width, value); }
        }

        public double Height
        {
            get => _height;
            set { Set(ref _height, value); }
        }

        public string Type
        {
            get => _type;
            set { Set(ref _type, value); }
        }

        public int ClassType
        {
            get { return _ClassType; }
            set { Set(ref _ClassType, value); }
        }


        public int MotionType
        {
            get { return motionType; }
            set { Set(ref motionType, value); }
        }



        public DateTime DetectTime
        {
            get => _detectTime;
            set { Set(ref _detectTime, value); }
        }
        public string Status
        {
            get => _status;
            set { Set(ref _status, value); }
        }

        private bool _IsVisibled;

        public bool IsVisibled
        {
            get { return _IsVisibled; }
            set { Set(ref _IsVisibled, value); }
        }

        private string desc = string.Empty;

        public string Desc
        {
            get { return desc ; }
            set { Set(ref desc, value); }
        }

        public Brush FillBrush
        {
            get => _fillBrush;
            set { Set(ref _fillBrush, value); }
        }

        public Brush StrokeBrush
        {
            get => _strokeBrush;
            set { Set(ref _strokeBrush, value); }
        }

        public double StrokeThickness
        {
            get => _strokeThickness;
            set { Set(ref _strokeThickness, value); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { Set(ref _isSelected , value); }
        }

        public override bool Equals(object obj)
        {
            var target = obj as RadarTarget ;
            return target != null && target.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id %16;
        }

        public override string ToString()
        {
            return $"Target:{Id:00}\n\nDistance: ({X:F2}m, {Y:F2}m)\ntype: {Type}";
        }
    }
}
