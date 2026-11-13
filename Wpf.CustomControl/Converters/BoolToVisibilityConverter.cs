using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.CustomControl.Converters
{
    /// <summary>
    /// bool 转 Visibility 转换器。
    /// true 显示，false 隐藏（默认 Collapse）。
    /// 可通过 ConverterParameter 控制隐藏方式：
    ///   - Hidden：使用 Visibility.Hidden
    ///   - Collapse（默认）：使用 Visibility.Collapsed
    /// 可通过 ConverterParameter=Invert 反转结果（true 隐藏，false 显示）。
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = false;
            if (value is bool b)
            {
                boolValue = b;
            }
            else if (value != null)
            {
                bool parsed;
                if (bool.TryParse(value.ToString(), out parsed))
                {
                    boolValue = parsed;
                }
            }

            bool invert = IsInvert(parameter);
            boolValue = invert ? !boolValue : boolValue;

            if (boolValue)
            {
                return Visibility.Visible;
            }

            return GetHiddenVisibility(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = value is Visibility v ? v : Visibility.Collapsed;
            bool result = visibility == Visibility.Visible;

            bool invert = IsInvert(parameter);
            return invert ? !result : result;
        }

        private static bool IsInvert(object parameter)
        {
            if (parameter == null)
            {
                return false;
            }
            string s = parameter.ToString();
            return s.IndexOf("Invert", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Visibility GetHiddenVisibility(object parameter)
        {
            if (parameter == null)
            {
                return Visibility.Collapsed;
            }
            string s = parameter.ToString();
            if (s.IndexOf("Hidden", StringComparison.OrdinalIgnoreCase) >= 0 &&
                s.IndexOf("Invert", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return Visibility.Hidden;
            }
            return Visibility.Collapsed;
        }
    }
}
