using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashItem : ViewModelBase, ICloneable
    {
        public ZFlashUdsEnum ZFlashUdsType { get; set; }
        public int NumID { get; set; }
        public virtual string Desc { get; set; }
        public virtual string Data { get; set; }
        public virtual bool Functionaddr { get; set; }
        public virtual int GetAddressIndex()
        {
            return 0;
        }
        public string BackColor
        {
            get { return mBackColor; }
            set
            {
                Set(ref mBackColor, value);
            }
        }
        private string mBackColor = "#6b8eb9";

        private ZFlashItemStatus mStatus = ZFlashItemStatus.Not_Start;

        public ZFlashItemStatus Status
        {
            get { return mStatus; }
            set
            {
                Set(ref mStatus, value);
                string[] colors = new string[] { "#6b8eb9", "#808080", "#006400", "#c34d4e" };
                int pos = ((int)value);
                BackColor = colors[pos];
            }
        }
        public ZFlashItem() { }
        public static ZFlashItem GetInstance(string type, XElement ele, string zflashpath)
        {
            switch (type)
            {
                case "security":
                    return new ZFlashSecurity().ParseZflashBuilder(ele);
                case "setting":
                    return new ZFlashSetting().ParseZflashBuilder(ele);
                case "general":
                    return new ZFlashGener().ParseZflashBuilder(ele);
                case "download":
                    var zFlashDown = new ZFlashDown().ParseZflashBuilder(ele);
                    string hexfile = zFlashDown.File;
                    if (!File.Exists(zFlashDown.File))
                    {
                        hexfile = Path.Combine(Path.GetDirectoryName(zflashpath), hexfile);
                    }
                    if (File.Exists(hexfile))
                    {
                        zFlashDown.HexPath = Path.GetDirectoryName(zflashpath);
                        List<DataBlock> list = new List<DataBlock>();
                        new ParseHexFile().ParseHexfile(hexfile, ref list);
                        if ((list == null || list.Count == 0) && FlashParseFile.ParseHexFileEx(hexfile, out DataBlock block))
                        {
                            list.Add(block);
                        }
                        if (list != null && list.Count > 0)
                        {
                            var items = list.FirstOrDefault();
                            zFlashDown.ZFlashBlock = new ZFlashFileBlock()
                            {
                                Address = items.Address.ToString("X8"),
                                Length = items.Data.Count,
                                Percent = 0,
                                Datas = items
                            };
                        }
                    }
                    return zFlashDown;
                default:
                    return new ZFlashDelay().ParseZflashBuilder(ele);
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class ZFlashDelay : ZFlashItem
    {
        public override string Desc
        {
            get { return mDesc; }
            set { Set(ref mDesc, value); }
        }
        private string mDesc = "延时";

        public override string Data
        {
            get { return $"{DelayTime} 毫秒"; }
        }

        public int DelayTime
        {
            get { return mDelayTime; }
            set { Set(ref mDelayTime, value); }
        }
        private int mDelayTime;
        public ZFlashDelay()
        {
            //< desc > 延时 </ desc >
            //< value > 1000 </ value >
        }
        public ZFlashDelay ParseZflashBuilder(XElement ele)
        {
            this.ZFlashUdsType = ZFlashUdsEnum.DealyTime;
            this.Desc = ele.Element("desc").Value;
            this.DelayTime = Convert.ToInt32(ele.Element("value").Value);
            return this;
        }
        public override string ToString()
        {
            return $"延时：{DelayTime} 毫秒";
        }
    }
}
