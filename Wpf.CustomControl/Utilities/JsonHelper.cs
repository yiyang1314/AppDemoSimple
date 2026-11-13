using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// JSON 序列化/反序列化工具类。
    /// 基于 System.Runtime.Serialization.Json.DataContractJsonSerializer，
    /// 需要类型标记 [DataContract] 和 [DataMember] 特性。
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象实例</param>
        /// <param name="encoding">编码，默认 UTF-8</param>
        /// <returns>JSON 字符串</returns>
        public static string Serialize<T>(T obj, Encoding encoding = null)
        {
            if (obj == null)
            {
                return "null";
            }

            encoding = encoding ?? Encoding.UTF8;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return encoding.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// 将对象保存到 JSON 文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象实例</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码，默认 UTF-8</param>
        public static void Save<T>(T obj, string filePath, Encoding encoding = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            encoding = encoding ?? Encoding.UTF8;
            FileHelper.EnsureDirectory(filePath);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.WriteObject(fs, obj);
            }
        }

        /// <summary>
        /// 从 JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <param name="encoding">编码，默认 UTF-8</param>
        /// <returns>对象实例</returns>
        public static T Deserialize<T>(string json, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            encoding = encoding ?? Encoding.UTF8;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(json)))
            {
                return (T)serializer.ReadObject(ms);
            }
        }

        /// <summary>
        /// 从 JSON 文件加载并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>对象实例；文件不存在返回 default(T)</returns>
        public static T Load<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return (T)serializer.ReadObject(fs);
            }
        }

        /// <summary>
        /// 尝试从 JSON 文件加载并反序列化为对象
        /// </summary>
        public static bool TryLoad<T>(string filePath, out T result)
        {
            result = default(T);
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }
                result = Load<T>(filePath);
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
