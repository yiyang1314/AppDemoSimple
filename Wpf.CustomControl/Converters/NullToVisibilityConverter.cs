using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.CustomControl.Converters
{
    /// <summary>
    /// null 转 Visibility 转换器。
    /// 非 null 显示，null 隐藏（默认 Collapse）。
    /// 可通过 ConverterParameter=Invert 反转结果（null 显示，非 null 隐藏）。
    /// 可通过 ConverterParameter=Hidden 设置隐藏方式为 Visibility.Hidden。
    /// 支持空字符串判断（需同时传 EmptyString 参数）。
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = IsNullOrEmpty(value, parameter);
            bool invert = IsInvert(parameter);
            bool shouldShow = invert ? isNull : !isNull;

            if (shouldShow)
            {
                return Visibility.Visible;
            }

            return GetHiddenVisibility(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = value is Visibility v ? v : Visibility.Collapsed;
            bool isVisible = visibility == Visibility.Visible;
            bool invert = IsInvert(parameter);

            // 仅在反向输出为 null/非 null 时有意义
            bool isNotNull = invert ? !isVisible : isVisible;
            return isNotNull ? (object)new object() : null;
        }

        private static bool IsNullOrEmpty(object value, object parameter)
        {
            if (value == null || value == DBNull.Value)
            {
                return true;
            }

            bool checkEmptyString = parameter != null &&
                parameter.ToString().IndexOf("EmptyString", StringComparison.OrdinalIgnoreCase) >= 0;
            if (checkEmptyString)
            {
                string s = value.ToString();
                return string.IsNullOrEmpty(s);
            }

            return false;
        }

        private static bool IsInvert(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }
            return parameter.ToString().IndexOf("Invert", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Visibility GetHiddenVisibility(object parameter)
        {
            if (parameter != null &&
                parameter.ToString().IndexOf("Hidden", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Visibility.Hidden;
            }
            return Visibility.Collapsed;
        }
    }
}
