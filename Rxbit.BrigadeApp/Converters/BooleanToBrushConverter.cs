using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Rxbit.BrigadeApp.Converters
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToBrushConverter : IValueConverter
    {
        public string TrueColor { get; set; } = "#0066cc";
        public string FalseColor { get; set; } = "Transparent";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = value is bool b && b;
            string color = isTrue ? TrueColor : FalseColor;
            if (color.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
                return Brushes.Transparent;
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BooleanToForegroundConverter : IValueConverter
    {
        public string TrueColor { get; set; } = "White";
        public string FalseColor { get; set; } = "#e4e4e4";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isTrue = value is bool b && b;
            string color = isTrue ? TrueColor : FalseColor;
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
