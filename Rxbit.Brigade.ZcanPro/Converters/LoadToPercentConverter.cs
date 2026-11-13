using System;
using System.Globalization;
using System.Windows.Data;

namespace Rxbit.Brigade.ZcanPro.Converters
{
    public class LoadToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue && strValue.EndsWith("%"))
            {
                if (double.TryParse(strValue.TrimEnd('%'), out double result))
                    return result;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}