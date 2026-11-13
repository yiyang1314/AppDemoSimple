using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Wpf.CustomControl.Utilities
{
    /// <summary>
    /// XML 文件序列化/反序列化工具类。
    /// 基于 System.Xml.Serialization.XmlSerializer，支持泛型类型。
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// 将对象序列化为 XML 字符串
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象实例</param>
        /// <param name="omitXmlDeclaration">是否省略 XML 声明</param>
        /// <param name="indent">是否缩进</param>
        /// <returns>XML 字符串</returns>
        public static string Serialize<T>(T obj, bool omitXmlDeclaration = false, bool indent = true)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = indent,
                OmitXmlDeclaration = omitXmlDeclaration,
                IndentChars = "  "
            };

            using (StringWriter stringWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(xmlWriter, obj, ns);
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// 将对象保存到 XML 文件
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象实例</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="indent">是否缩进</param>
        public static void Save<T>(T obj, string filePath, bool indent = true)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = indent,
                IndentChars = "  "
            };

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (XmlWriter writer = XmlWriter.Create(fs, settings))
            {
                serializer.Serialize(writer, obj);
            }
        }

        /// <summary>
        /// 从 XML 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="xml">XML 字符串</param>
        /// <returns>对象实例</returns>
        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// 从 XML 文件加载并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <returns>对象实例；文件不存在或反序列化失败返回 default(T)</returns>
        public static T Load<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return (T)serializer.Deserialize(fs);
            }
        }

        /// <summary>
        /// 尝试从 XML 文件加载并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="filePath">文件路径</param>
        /// <param name="result">反序列化结果</param>
        /// <returns>是否成功</returns>
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
