using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Rxbit.BrigadeApp.Controls
{
    /// <summary>
    /// RecordButton.xaml 的交互逻辑
    /// </summary>
    public partial class RecordButton : UserControl
    {
        #region 依赖属性（外部可绑定设置）
        // 主体紫色
        public SolidColorBrush MainColor
        {
            get => (SolidColorBrush)GetValue(MainColorProperty);
            set => SetValue(MainColorProperty, value);
        }
        public static readonly DependencyProperty MainColorProperty =
            DependencyProperty.Register(nameof(MainColor), typeof(SolidColorBrush),
                typeof(RecordButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(184, 120, 242))));

        // 进度绿色
        public SolidColorBrush ProgressColor
        {
            get => (SolidColorBrush)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }
        public static readonly DependencyProperty ProgressColorProperty =
            DependencyProperty.Register(nameof(ProgressColor), typeof(SolidColorBrush),
                typeof(RecordButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(153, 238, 34))));

        // 是否正在录制
        public bool IsRecording
        {
            get => (bool)GetValue(IsRecordingProperty);
            set => SetValue(IsRecordingProperty, value);
        }
        public static readonly DependencyProperty IsRecordingProperty =
            DependencyProperty.Register(nameof(IsRecording), typeof(bool),
                typeof(RecordButton), new PropertyMetadata(false, OnIsRecordingChanged));

        // 录制总时长（秒）
        public int RecordSeconds
        {
            get => (int)GetValue(RecordSecondsProperty);
            set => SetValue(RecordSecondsProperty, value);
        }
        public static readonly DependencyProperty RecordSecondsProperty =
            DependencyProperty.Register(nameof(RecordSeconds), typeof(int),
                typeof(RecordButton), new PropertyMetadata(0));
        #endregion

        #region 私有字段
        private readonly DispatcherTimer _aniTimer;
        private double _angle = 0;
        // 录制状态变更事件
        public event Action<bool> RecordStateChanged;
        #endregion

        public RecordButton()
        {
            InitializeComponent();
            // 动画刷新间隔 100ms
            _aniTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _aniTimer.Tick += AnimationTick;
        }

        #region 录制状态切换回调
        private static void OnIsRecordingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as RecordButton;
            bool isRec = (bool)e.NewValue;
            ctrl?.SwitchRecordState(isRec);
        }

        /// <summary>
        /// 切换待机 / 录制UI样式
        /// </summary>
        private void SwitchRecordState(bool isRecording)
        {
            if (isRecording)
            {
                // 开始录制
                IdleDot.Visibility = Visibility.Collapsed;
                RecordingRing.Visibility = Visibility.Visible;
                ProgressRing.Visibility = Visibility.Visible;
                _angle = 0;
                RecordSeconds = 0;
                _aniTimer.Start();
            }
            else
            {
                // 停止录制，重置
                _aniTimer.Stop();
                IdleDot.Visibility = Visibility.Visible;
                RecordingRing.Visibility = Visibility.Collapsed;
                ProgressRing.Visibility = Visibility.Collapsed;
                _angle = 0;
                RecordSeconds = 0;
            }
            RecordStateChanged?.Invoke(isRecording);
        }
        #endregion

        #region 环形进度动画更新
        private void AnimationTick(object sender, EventArgs e)
        {
            // 0.1s累加36°，1秒一圈360°
            _angle += 36;
            if (_angle >= 360)
            {
                _angle = 0;
                RecordSeconds++;
            }
            UpdateArcProgress(_angle);
        }

        /// <summary>
        /// 刷新圆弧路径
        /// </summary>
        private void UpdateArcProgress(double angle)
        {
            double rad = angle * Math.PI / 180;
            double r = 50;
            double cx = 60, cy = 60;

            double endX = cx + r * Math.Sin(rad);
            double endY = cy - r * Math.Cos(rad);

            ProgressArc.Point = new Point(endX, endY);
            ProgressArc.IsLargeArc = angle > 180;
        }
        #endregion

        #region 对外快捷方法
        public void StartRecord() => IsRecording = true;
        public void StopRecord() => IsRecording = false;
        public void ToggleRecord() => IsRecording = !IsRecording;
        #endregion
    }
}
