using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Wpf.CustomControl.Converters
{
    /// <summary>
    /// 枚举状态转颜色转换器。
    /// 根据枚举值（或其字符串形式）返回对应的颜色画刷。
    ///
    /// 用法 1（ConverterParameter 直接指定映射）:
    ///   ConverterParameter="Running=Green|Stopped=Gray|Error=Red|Default=Black"
    ///   - 使用 | 分隔不同枚举值
    ///   - 使用 = 分隔枚举名与颜色名
    ///   - Default 为默认颜色
    ///   - 颜色名支持 System.Windows.Media.Colors 中的命名颜色（如 Green、#FF0000）
    ///
    /// 用法 2（不传 parameter，使用内置默认映射）:
    ///   提供常见运行状态的默认颜色映射：
    ///     Running/Active/Online/Connected -> Green
    ///     Stopped/Idle/Offline/Disconnected -> Gray
    ///     Error/Failed/Fault -> Red
    ///     Warning/Pending/Waiting -> Orange
    ///     Default -> Black
    /// </summary>
    public class EnumToColorConverter : IValueConverter
    {
        private static readonly Dictionary<string, Brush> DefaultMappings =
            new Dictionary<string, Brush>(StringComparer.OrdinalIgnoreCase)
            {
                { "Running", Brushes.Green },
                { "Active", Brushes.Green },
                { "Online", Brushes.Green },
                { "Connected", Brushes.Green },
                { "Start", Brushes.Green },
                { "Success", Brushes.Green },
                { "Ok", Brushes.Green },

                { "Stopped", Brushes.Gray },
                { "Stop", Brushes.Gray },
                { "Idle", Brushes.Gray },
                { "Offline", Brushes.Gray },
                { "Disconnected", Brushes.Gray },
                { "Disabled", Brushes.Gray },
                { "None", Brushes.Gray },

                { "Error", Brushes.Red },
                { "Failed", Brushes.Red },
                { "Fault", Brushes.Red },
                { "Fail", Brushes.Red },

                { "Warning", Brushes.Orange },
                { "Pending", Brushes.Orange },
                { "Waiting", Brushes.Orange },
                { "Pause", Brushes.Orange },
                { "Paused", Brushes.Orange },

                { "Default", Brushes.Black },
                { "Unknown", Brushes.DarkGray },
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DefaultMappings["Default"];
            }

            string key = value.ToString();

            // 优先使用 ConverterParameter 中定义的映射
            if (parameter != null)
            {
                Brush brush = ResolveFromParameter(parameter.ToString(), key);
                if (brush != null)
                {
                    return brush;
                }
            }

            // 使用默认映射
            if (DefaultMappings.ContainsKey(key))
            {
                return DefaultMappings[key];
            }

            return DefaultMappings["Default"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 颜色到枚举的反向转换不常用，抛出未实现异常
            throw new NotImplementedException();
        }

        private static Brush ResolveFromParameter(string parameter, string key)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                return null;
            }

            // 格式: Key1=Color1|Key2=Color2|Default=Color3
            string[] entries = parameter.Split('|');
            foreach (string entry in entries)
            {
                string[] parts = entry.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                string entryKey = parts[0].Trim();
                string colorSpec = parts[1].Trim();

                if (entryKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return ParseBrush(colorSpec);
                }

                if (entryKey.Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    // 先记录默认值，继续匹配后续项
                    // 但如果当前 key 匹配 Default 则直接返回
                }
            }

            // 没有精确匹配，返回 parameter 中定义的 Default
            foreach (string entry in entries)
            {
                string[] parts = entry.Split('=');
                if (parts.Length == 2 && parts[0].Trim().Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    return ParseBrush(parts[1].Trim());
                }
            }

            return null;
        }

        private static Brush ParseBrush(string colorSpec)
        {
            if (string.IsNullOrEmpty(colorSpec))
            {
                return Brushes.Black;
            }

            // 支持 #RRGGBB 或 #AARRGGBB 格式
            if (colorSpec.StartsWith("#", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(colorSpec);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    return Brushes.Black;
                }
            }

            // 命名颜色（如 Green、Red）
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(colorSpec);
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Black;
            }
        }
    }
}
