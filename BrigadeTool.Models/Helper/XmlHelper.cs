using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Rxbit.Brigade.Models.Helper
{
    /// <summary>
    /// XmlSerializer 对象序列化/反序列化工具
    /// 实体需提供无参构造函数
    /// </summary>
    public static class XmlHelper
    {
        #region 对象 ↔ XML字符串
        public static string Serialize<T>(T model)
        {
            using var sw = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(sw, model);
            return sw.ToString();
        }

        public static T Deserialize<T>(string xmlStr)
        {
            using var sr = new StringReader(xmlStr);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(sr);
        }
        #endregion

        #region 对象 ↔ XML文件
        public static void SaveToFile<T>(T model, string filePath)
        {
            string xml = Serialize(model);
            File.WriteAllText(filePath, xml, Encoding.UTF8);
        }

        public static T LoadFromFile<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);
            string xml = File.ReadAllText(filePath, Encoding.UTF8);
            return Deserialize<T>(xml);
        }
        #endregion
    }
}
