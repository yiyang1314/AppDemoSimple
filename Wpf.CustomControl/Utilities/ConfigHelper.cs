using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// INI 风格的键值对配置文件工具类。
    /// 格式：
    ///   [Section]
    ///   Key=Value
    ///   ;注释
    /// 支持分区、读写、默认值、批量操作。
    /// </summary>
    public class ConfigHelper
    {
        private readonly string _filePath;
        private readonly Dictionary<string, Dictionary<string, string>> _sections;
        private static readonly char[] TrimChars = new char[] { ' ', '\t' };

        /// <summary>
        /// 创建配置实例
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        public ConfigHelper(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }
            _filePath = filePath;
            _sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 当前配置文件路径
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        public void Load()
        {
            _sections.Clear();

            if (!File.Exists(_filePath))
            {
                return;
            }

            string currentSection = string.Empty;
            string[] lines = File.ReadAllLines(_filePath, Encoding.UTF8);

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                // 注释行
                if (line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                // 分区行 [Section]
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    continue;
                }

                // 键值行 Key=Value
                int idx = line.IndexOf('=');
                if (idx > 0)
                {
                    string key = line.Substring(0, idx).Trim(TrimChars);
                    string value = line.Substring(idx + 1).Trim(TrimChars);

                    if (!_sections.ContainsKey(currentSection))
                    {
                        _sections[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    }
                    _sections[currentSection][key] = value;
                }
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        public void Save()
        {
            FileHelper.EnsureDirectory(_filePath);

            StringBuilder sb = new StringBuilder();
            foreach (var sectionPair in _sections)
            {
                sb.Append("[");
                sb.Append(sectionPair.Key);
                sb.AppendLine("]");

                foreach (var kv in sectionPair.Value)
                {
                    sb.Append(kv.Key);
                    sb.Append("=");
                    sb.AppendLine(kv.Value);
                }
                sb.AppendLine();
            }

            File.WriteAllText(_filePath, sb.ToString(), Encoding.UTF8);
        }

        #region 读取

        /// <summary>
        /// 读取字符串值
        /// </summary>
        public string Get(string section, string key, string defaultValue = "")
        {
            if (TryGetSection(section, out var sectionData) &&
                sectionData.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// 读取整数值
        /// </summary>
        public int GetInt(string section, string key, int defaultValue = 0)
        {
            string value = Get(section, key);
            int result;
            return int.TryParse(value, out result) ? result : defaultValue;
        }

        /// <summary>
        /// 读取布尔值
        /// </summary>
        public bool GetBool(string section, string key, bool defaultValue = false)
        {
            string value = Get(section, key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }
            if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1") || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("0") || value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return defaultValue;
        }

        /// <summary>
        /// 读取双精度值
        /// </summary>
        public double GetDouble(string section, string key, double defaultValue = 0)
        {
            string value = Get(section, key);
            double result;
            return double.TryParse(value, out result) ? result : defaultValue;
        }

        #endregion

        #region 写入

        /// <summary>
        /// 设置字符串值
        /// </summary>
        public void Set(string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
            {
                section = string.Empty;
            }
            if (!_sections.ContainsKey(section))
            {
                _sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            _sections[section][key] = value ?? string.Empty;
        }

        public void Set(string section, string key, int value)
        {
            Set(section, key, value.ToString());
        }

        public void Set(string section, string key, bool value)
        {
            Set(section, key, value ? "true" : "false");
        }

        public void Set(string section, string key, double value)
        {
            Set(section, key, value.ToString("R"));
        }

        /// <summary>
        /// 移除指定键
        /// </summary>
        public bool RemoveKey(string section, string key)
        {
            if (TryGetSection(section, out var sectionData))
            {
                return sectionData.Remove(key);
            }
            return false;
        }

        /// <summary>
        /// 移除整个分区
        /// </summary>
        public bool RemoveSection(string section)
        {
            return _sections.Remove(section);
        }

        #endregion

        #region 查询

        /// <summary>
        /// 是否包含指定分区
        /// </summary>
        public bool ContainsSection(string section)
        {
            return _sections.ContainsKey(section);
        }

        /// <summary>
        /// 是否包含指定键
        /// </summary>
        public bool ContainsKey(string section, string key)
        {
            return TryGetSection(section, out var sectionData) && sectionData.ContainsKey(key);
        }

        /// <summary>
        /// 获取所有分区名
        /// </summary>
        public IEnumerable<string> GetSectionNames()
        {
            return _sections.Keys;
        }

        /// <summary>
        /// 获取指定分区所有键
        /// </summary>
        public IEnumerable<string> GetKeys(string section)
        {
            if (TryGetSection(section, out var sectionData))
            {
                return sectionData.Keys;
            }
            return new string[0];
        }

        #endregion

        #region 静态便捷方法

        /// <summary>
        /// 读取配置项（一次性读取文件，性能较低，但适合单次操作）
        /// </summary>
        public static string ReadValue(string filePath, string section, string key, string defaultValue = "")
        {
            var config = new ConfigHelper(filePath);
            config.Load();
            return config.Get(section, key, defaultValue);
        }

        /// <summary>
        /// 写入配置项（保留原有内容，仅更新指定项）
        /// </summary>
        public static void WriteValue(string filePath, string section, string key, string value)
        {
            var config = new ConfigHelper(filePath);
            config.Load();
            config.Set(section, key, value);
            config.Save();
        }

        #endregion

        private bool TryGetSection(string section, out Dictionary<string, string> sectionData)
        {
            if (string.IsNullOrEmpty(section))
            {
                section = string.Empty;
            }
            return _sections.TryGetValue(section, out sectionData);
        }
    }
}
