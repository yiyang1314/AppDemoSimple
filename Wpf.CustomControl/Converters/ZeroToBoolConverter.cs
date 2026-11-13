using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.CustomControl.Converters
{
    /// <summary>
    /// 0 转 bool 转换器。
    /// 数值为 0 时返回 false，非 0 时返回 true；支持反向转换（false 返回 0，true 返回 1）。
    /// 可通过 ConverterParameter=Invert 反转结果。
    /// </summary>
    public class ZeroToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = !IsZero(value);
            return InvertIfNeeded(result, parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                bool result = b;
                result = InvertIfNeeded(result, parameter);
                return result ? 1 : 0;
            }
            return 0;
        }

        private static bool IsZero(object value)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                double d = System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                return Math.Abs(d) < double.Epsilon;
            }
            catch
            {
                return true;
            }
        }

        private static bool InvertIfNeeded(bool result, object parameter)
        {
            bool invert = parameter != null &&
                parameter.ToString().Equals("Invert", StringComparison.OrdinalIgnoreCase);
            return invert ? !result : result;
        }
    }
}
