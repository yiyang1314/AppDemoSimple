using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Rxbit.Brigade.Controll.Mvvm
{
    // Bool → Brush (true: ActiveBg, false: Normal)  - NullConverter
    public class BoolToBrushConverter : IValueConverter
    {
        public Brush TrueValue { get; set; }
        public Brush FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return TrueValue;
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Bool → Thickness (BorderThickness)
    public class BoolToThicknessConverter : IValueConverter
    {
        public double TrueV { get; set; } = 1;
        public double FalseV { get; set; } = 0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new Thickness(TrueV);
            return new Thickness(FalseV);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Bool → Visibility
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ============================================================
    // 固定预设的静态转换器（HMI XAML 中使用的 NullConverter1..12）
    // ============================================================

    // HMI1 LeftMenu: IsActive=true → 高亮背景色
    public class NullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(22, 55, 100));
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI1 LeftMenu: IsActive=true → 前景色 白/灰
    public class NullConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(140, 160, 190));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI2 Beauty Stage: IsActive=true → 背景蓝
    public class NullConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(18, 40, 72));
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI2 Beauty Stage: IsActive=true → 边框蓝
    public class NullConverter4 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(28, 140, 255));
            return new SolidColorBrush(Color.FromRgb(42, 64, 101));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI3 Core: IsActive=true → 菜单背景
    public class NullConverter5 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(22, 54, 92));
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI3 Core: IsActive=true → 左边框
    public class NullConverter6 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new Thickness(3, 0, 0, 0);
            return new Thickness(0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI3 Core: IsActive=true → 前景白
    public class NullConverter7 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(150, 170, 200));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI3 Core: IsAlert=true → 红底
    public class NullConverter8 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(184, 40, 45));
            return new SolidColorBrush(Color.FromRgb(30, 66, 99));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI5 Param: IsActive → Visibility (左侧高亮条)
    public class NullConverter9 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI5 Param: IsActive → 背景 (卡片选中态)
    public class NullConverter10 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(47, 128, 237));
            return new SolidColorBrush(Color.FromRgb(42, 42, 46));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI5 Param: IsActive → 前景 (图标白/灰)
    public class NullConverter11 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(180, 180, 190));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI5 Param: IsActive → 前景 (文字白/灰)
    public class NullConverter12 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(138, 138, 142));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            { throw new NotImplementedException(); }
    }

    // HMI3 CoreGreen
    public class StaticResourceHelper
    {
        // 预留
    }

    // Bool → FontWeight
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return FontWeights.Bold;
            return FontWeights.Normal;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    // Shop: Bool → Green active background
    public class ShopNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(0, 181, 120));
            return new SolidColorBrush(Color.FromRgb(240, 243, 247));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    // Shop: Bool → Foreground (white/secondary gray)
    public class ShopNullConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return Brushes.White;
            return new SolidColorBrush(Color.FromRgb(96, 98, 102));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    // Shop: Bool → Green border brush
    public class ShopNullConverter3 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(0, 181, 120));
            return new SolidColorBrush(Color.FromRgb(228, 231, 237));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    // Shop: Bool → Foreground (green active/gray normal)
    public class ShopNullConverter4 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return new SolidColorBrush(Color.FromRgb(0, 181, 120));
            return new SolidColorBrush(Color.FromRgb(96, 98, 102));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
