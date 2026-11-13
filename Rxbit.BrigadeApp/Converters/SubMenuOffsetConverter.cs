using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rxbit.BrigadeApp.Converters
{
    public class SubMenuOffsetConverter : IMultiValueConverter
    {
        /// <summary>
        /// 修正下拉面板的水平位置。
        /// Windows 开启"右键对齐菜单"(MenuDropAlignment) 时，WPF 会把 Popup 的右边缘
        /// 对齐到菜单项右边缘，导致下拉面板相对父菜单项明显左偏。这里在开启该选项时
        /// 补偿 (面板宽度 - 菜单项宽度)，使面板左边缘与菜单项左边缘对齐。
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return 0.0;
            }

            if (!SystemParameters.MenuDropAlignment)
            {
                return 0.0;
            }

            var popupWidth = ToDouble(values[0]);
            var targetWidth = ToDouble(values[1]);
            if (double.IsNaN(popupWidth) || double.IsNaN(targetWidth) || popupWidth <= 0)
            {
                return 0.0;
            }

            return popupWidth - targetWidth;
        }

        private static double ToDouble(object value)
        {
            return value is double d ? d : 0d;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class NonEmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBooleanToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}