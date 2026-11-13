using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.CustomControl.Converters
{
    /// <summary>
    /// null 转 '-' 转换器。
    /// 当值为 null、空字符串或 DBNull 时返回 "-"，否则返回原值字符串。
    /// 支持反向转换（始终返回 null）。
    /// </summary>
    public class NullToDashConverter : IValueConverter
    {
        /// <summary>
        /// 当值为空时显示的文本，默认为 "-"
        /// </summary>
        public string EmptyText { get; set; } = "-";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                EmptyText = parameter.ToString();
            }

            if (value == null || value == DBNull.Value)
            {
                return EmptyText;
            }

            string str = value.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return EmptyText;
            }

            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
