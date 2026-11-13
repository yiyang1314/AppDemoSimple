using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.Controls
{
    public class RecordProgressButton : Control
    {
        #region 依赖属性
        // 紫色主体颜色
        public SolidColorBrush MainColor
        {
            get => (SolidColorBrush)GetValue(MainColorProperty);
            set => SetValue(MainColorProperty, value);
        }
        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register(nameof(MainColor), typeof(SolidColorBrush),
                typeof(RecordProgressButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(182, 116, 240))));

        // 是否录制
        public bool IsRecording
        {
            get => (bool)GetValue(IsRecordingProperty);
            set => SetValue(IsRecordingProperty, value);
        }
        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register(nameof(IsRecording), typeof(bool),
                typeof(RecordProgressButton), new PropertyMetadata(false, OnRecordStateChanged));

        // 录制总秒数
        public int TotalSeconds
        {
            get => (int)GetValue(TotalSecondsProperty);
            set => SetValue(TotalSecondsProperty, value);
        }
        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register(nameof(TotalSeconds), typeof(int),
                typeof(RecordProgressButton), new PropertyMetadata(0));
        #endregion

        #region 私有字段
        private DispatcherTimer _animateTimer;
        private double _currentAngle = 0;
        // 录制状态变更事件
        public event Action<bool> RecordStatusChanged;
        private ArcSegment _arcSegment;
        private PathFigure _pathFigure;
        #endregion

        static RecordProgressButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordProgressButton),
                new FrameworkPropertyMetadata(typeof(RecordProgressButton)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // 获取模板内圆弧元素
            _pathFigure = GetTemplateChild("PART_Figure") as PathFigure;
            _arcSegment = GetTemplateChild("PART_Arc") as ArcSegment;

            _animateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(80) // 动画刷新帧率
            };
            _animateTimer.Tick += AnimationTick;
        }

        /// <summary>
        /// 录制状态切换回调
        /// </summary>
        private static void OnRecordStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var btn = d as RecordProgressButton;
            bool isRec = (bool)e.NewValue;
            btn?.SwitchRecordMode(isRec);
        }

        private void SwitchRecordMode(bool isRecording)
        {
            RecordStatusChanged?.Invoke(isRecording);
            _currentAngle = 0;
            TotalSeconds = 0;

            if (isRecording)
                _animateTimer.Start();
            else
                _animateTimer.Stop();
        }

        /// <summary>
        /// 环形进度动画帧更新
        /// 顺时针一圈 = 1秒
        /// </summary>
        private void AnimationTick(object sender, EventArgs e)
        {
            _currentAngle += 36;
            if (_currentAngle >= 360)
            {
                _currentAngle = 0;
                TotalSeconds++;
            }
            UpdateArcAngle(_currentAngle);
        }

        /// <summary>
        /// 根据角度刷新绿色圆弧路径
        /// </summary>
        private void UpdateArcAngle(double angle)
        {
            if (_arcSegment == null || _pathFigure == null) return;

            double rad = angle * Math.PI / 180;
            double radius = 55;
            double centerX = 65;
            double centerY = 65;

            double endX = centerX + radius * Math.Sin(rad);
            double endY = centerY - radius * Math.Cos(rad);

            _arcSegment.Point = new Point(endX, endY);
            _arcSegment.IsLargeArc = angle > 180;
        }

        #region 对外快捷调用方法
        public void StartRecord() => IsRecording = true;
        public void StopRecord() => IsRecording = false;
        public void ToggleRecord() => IsRecording = !IsRecording;
        #endregion
    }
}
