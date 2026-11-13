using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rxbit.BrigadeApp.Converters
{
    /// <summary>进度值/最大值 → 0~1比例</summary>
    public class ProgressRatioConverter : MarkupExtension, IMultiValueConverter
    {
        private static ProgressRatioConverter _inst;
        public override object ProvideValue(IServiceProvider serviceProvider) => _inst ?? (_inst = new ProgressRatioConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0d;
            double current = (double)values[0];
            double max = (double)values[1];
            if (max <= 0) return 0d;
            double raw = current / max;
            //double ratio = max <= 0 ? 0 : raw > 1 ? 1 : raw;
            return raw > 1 ? 1 : raw;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>比例转环形圆弧角度（0~360）</summary>
    public class ProgressToArcConverter : MarkupExtension, IMultiValueConverter
    {
        private static ProgressToArcConverter _inst;
        public override object ProvideValue(IServiceProvider serviceProvider) => _inst ?? (_inst = new ProgressToArcConverter());

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0d;
            double current = (double)values[0];
            double max = (double)values[1];
            double raw = current / max;
            double ratio = max <= 0 ? 0 : raw > 1 ? 1 : raw;
            return ratio * 360;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
