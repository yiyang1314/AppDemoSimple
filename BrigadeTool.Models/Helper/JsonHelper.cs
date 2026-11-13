using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Rxbit.Brigade.Models.Models;

namespace Rxbit.Brigade.Models.Helper
{
    /// <summary>
    /// Newtonsoft.Json 序列化工具类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>Newtonsoft 配置对象（正确类型 JsonSerializerSettings）</summary>
        private static readonly JsonSerializerSettings _jsonSettings;

        static JsonHelper()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore // 解决ViewModel循环引用
            };
            // 枚举输出文字而非数字
            _jsonSettings.Converters.Add(new StringEnumConverter());
        }

        #region 对象 ↔ Json字符串
        public static string Serialize<T>(T model)
        {
            if (model == null) return string.Empty;
            return JsonConvert.SerializeObject(model, _jsonSettings);
        }

        public static T Deserialize<T>(string jsonStr)
        {
            if (string.IsNullOrWhiteSpace(jsonStr))
                throw new ArgumentException("Json字符串不能为空");
            return JsonConvert.DeserializeObject<T>(jsonStr, _jsonSettings);
        }
        #endregion

        #region 对象 ↔ Json文件
        public static void SaveToFile<T>(T model, string filePath)
        {
            string json = Serialize(model);
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public static T LoadFromFile<T>(string filePath, T defaultModel = default)
        {
            if (!File.Exists(filePath))
            {
                if (defaultModel != null)
                    SaveToFile(defaultModel, filePath);
                return defaultModel;
            }
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            return Deserialize<T>(json);
        }
        #endregion

        #region JObject动态操作
        public static JObject ReadJObject(string jsonStr)
        {
            return JObject.Parse(jsonStr);
        }

        public static JObject LoadJObjectFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            return JObject.Parse(json);
        }
        #endregion

        /// <summary>
        /// demo 测试
        /// </summary>
        public static void Test()
        {
            // Json
            var model = new SensorInfo();
            JsonHelper.SaveToFile(model, @"D:\cfg.json");
            var load = JsonHelper.LoadFromFile(@"D:\cfg.json", new SensorInfo());

            // 5. 读取动态JObject（无实体类时灵活取值）
            JObject jObj = JsonHelper.LoadJObjectFromFile(@"D:\FlashTaskConfig.json");
            string fileName = jObj["FileName"]?.ToString();
        }
    }
}
