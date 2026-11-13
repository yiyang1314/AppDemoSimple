using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using System;
using System.Configuration;
using System.IO;

namespace Rxbit.Brigade.Models.Helper
{
    /// <summary>
    /// 系统AppSettings配置读写 + 自定义配置文件序列化
    /// </summary>
    public static class ConfigHelper
    {
        #region App.config AppSettings 键值读写
        /// <summary>读取配置字符串</summary>
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>写入AppSettings</summary>
        public static void SetAppSetting(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings[key] == null)
                config.AppSettings.Settings.Add(key, value);
            else
                config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>读取并转泛型</summary>
        public static T GetAppSetting<T>(string key)
        {
            string val = GetAppSetting(key);
            return (T)Convert.ChangeType(val, typeof(T));
        }
        #endregion

        #region 自定义实体配置文件（复用JsonHelper）
        /// <summary>保存实体到自定义配置json文件</summary>
        public static void SaveConfig<T>(T model, string configPath)
        {
            JsonHelper.SaveToFile(model, configPath);
        }

        /// <summary>读取自定义配置实体</summary>
        public static T ReadConfig<T>(string configPath, T defaultModel = default)
        {
            if (!File.Exists(configPath))
            {
                if (defaultModel != null)
                    SaveConfig(defaultModel, configPath);
                return defaultModel;
            }
            return JsonHelper.LoadFromFile<T>(configPath);
        }
        #endregion

        public static void Test()
        {
            // App.config 键值
            ConfigHelper.SetAppSetting("tag", "COM3");
            ConfigHelper.SetAppSetting("name", "COM3");
            ConfigHelper.SetAppSetting("age", "COM3");
            ConfigHelper.SetAppSetting("nid", "COM3");
            ConfigHelper.SetAppSetting("DevicePort", "COM3");
            string port = ConfigHelper.GetAppSetting("DevicePort");

            // 自定义配置文件
            var defCfg = new RadarGen1Obj()
            {
            
            };
            var readCfg = ConfigHelper.ReadConfig(Path.Combine(RadarConstaint.LogPath,"Test.config"), defCfg);
        }
    }
}
