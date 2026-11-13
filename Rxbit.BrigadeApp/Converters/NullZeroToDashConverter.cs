using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Rxbit.BrigadeApp.Converters
{
    [ValueConversion(typeof(object), typeof(string))]
    public class NullZeroToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // null 或者数值=0，返回横线
            if (value == null) return "-";
            if (double.TryParse(value.ToString(), out double num))
            {
                return num == 0 ? "-" : num.ToString();
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
