using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Rxbit.BrigadeApp.Converters
{
    public class ColumnIndexToBrushConverter : IMultiValueConverter
    {
        public string HighlightColor { get; set; } = "#0066cc";
        public string NormalColor { get; set; } = "Transparent";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(NormalColor));

            if (values[0] is int columnIndex && values[1] is int selectedIndex)
            {
                if (columnIndex == selectedIndex)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(HighlightColor));
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(NormalColor));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColumnIndexToForegroundConverter : IMultiValueConverter
    {
        public string HighlightColor { get; set; } = "White";
        public string NormalColor { get; set; } = "#e4e4e4";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(NormalColor));

            if (values[0] is int columnIndex && values[1] is int selectedIndex)
            {
                if (columnIndex == selectedIndex)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(HighlightColor));
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(NormalColor));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
