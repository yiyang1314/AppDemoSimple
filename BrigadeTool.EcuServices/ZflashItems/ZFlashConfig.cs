using Rxbit.Brigade.EcuServices.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashConfig : ViewModelBase
    {
        public List<ZFlashItem> ZFlashItems { get; set; }
        public ZFlashSetting ZSetting
        {
            get { return mZSetting; }
            set { Set(ref mZSetting, value); }
        }
        private ZFlashSetting mZSetting;
        public string ZFlashFile
        {
            get { return mZFlashFile; }
            set { Set(ref mZFlashFile, value); }
        }
        private string mZFlashFile = "";

        public string HexFile
        {
            get { return mHexFile; }
            set { Set(ref mHexFile, value); }
        }
        private string mHexFile = "";

        #region uds 可见属性
        public bool IsSecrityVisibility
        {
            get { return _IsSecrityVisibility; }
            set { Set(ref _IsSecrityVisibility, value); }
        }
        private bool _IsSecrityVisibility = false;

        public bool IsFileDownVisibility
        {
            get { return _IsFileDownVisibility; }
            set { Set(ref _IsFileDownVisibility, value); }
        }
        private bool _IsFileDownVisibility = false;

        public bool IsGenalVisibility
        {
            get { return _IsGenalVisibility; }
            set { Set(ref _IsGenalVisibility, value); }
        }
        private bool _IsGenalVisibility = false;

        public bool IsDealyVisibility
        {
            get { return _IsDealyVisibility; }
            set { Set(ref _IsDealyVisibility, value); }
        }
        private bool _IsDealyVisibility = false;
        #endregion
        /// <summary>
        /// 解析.zflash或.zflashexe文件（当作XML处理）
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void LoadZFlashConfig(string filePath)
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"文件不存在：{filePath}");
                    return;
                }
                //var ddd = ReadSystemDefaultAnsiXml(filePath);

                ZFlashItems = new List<ZFlashItem>();
                // 加载文件为XDocument（自动处理XML声明和编码）
                XDocument doc = XDocument.Load(filePath);

                // 示例1：获取根节点
                XElement root = doc.Root;
                if (root != null)
                {
                    Console.WriteLine($"根节点名称：{root.Name}");
                }

                // 示例2：查询所有子节点（假设根节点下有<Item>子节点）
                var items = root?.Elements("item").ToList();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        // 读取节点属性（如"Id"）
                        string type = item.Attribute("type")?.Value;
                        // 读取节点文本
                        var nodes = item.Nodes().ToList();
                        if (type == "setting")
                        {
                            ZSetting = new ZFlashSetting().ParseZflashBuilder(item);
                        }
                        else
                        {
                            ZFlashItem zFlash = ZFlashItem.GetInstance(type, item, filePath);
                            zFlash.NumID = ZFlashItems.Count + 1;
                            ZFlashItems.Add(zFlash);
                        }
                    }
                    this.ZFlashFile = filePath;
                }
            }
            catch (Exception ex)
            {
                // 处理异常（如文件格式错误、权限问题等）
                Console.WriteLine($"解析失败：{ex.Message}");
            }
        }
    }
}
