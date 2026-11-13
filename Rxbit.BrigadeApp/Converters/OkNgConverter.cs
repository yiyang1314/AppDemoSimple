using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Rxbit.BrigadeApp.Converters
{

    [ValueConversion(typeof(int), typeof(string))]
    public class OkNgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "NG";
            int val = System.Convert.ToInt32(value);
            return val == 0 ? "OK" : "NG";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private static IntToVisibilityConverter _instance;
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new IntToVisibilityConverter());
        }

        /// <summary>
        /// 参数格式说明（逗号分隔，顺序可选）
        /// 例1："1"        → val >= 1 显示，否则Collapsed
        /// 例2："0,Inverse" → val >= 0 隐藏，否则显示
        /// 例3："5,Hidden"  → val >=5 显示，否则占位隐藏(Hidden)
        /// 例4："2,Inverse,Hidden"
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 默认配置
            int threshold = 1;
            bool isInverse = false;
            bool useHidden = false;

            // 解析传入参数
            if (parameter != null)
            {
                string[] args = parameter.ToString().Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries);
                // 第一参数：阈值数字
                if (int.TryParse(args[0].Trim(), out int num))
                    threshold = num;

                // 解析附加标记
                foreach (var arg in args)
                {
                    string tag = arg.Trim().ToLower();
                    if (tag == "inverse") isInverse = true;
                    if (tag == "hidden") useHidden = true;
                }
            }

            // 获取输入int值
            int inputVal = value is int i ? i : 0;
            bool condition = inputVal >= threshold;

            // 反转逻辑
            if (isInverse) condition = !condition;

            // 返回可见性
            if (condition)
                return Visibility.Visible;
            return useHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        // 不需要双向转换场景直接抛异常
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Int转Visibility为单向转换，不支持回写");
        }
    }
}
