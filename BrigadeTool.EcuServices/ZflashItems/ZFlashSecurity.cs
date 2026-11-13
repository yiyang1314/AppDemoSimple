using System;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashSecurity : ZFlashItem
    {
        public override bool Functionaddr
        {
            get { return mFuncAddr; }
            set { Set(ref mFuncAddr, value); }
        }
        private bool mFuncAddr = true;
        public byte Level
        {
            get { return mLevel; }
            set { Set(ref mLevel, value); }
        }
        private byte mLevel;
        public string SubLevel
        {
            get { return msubLevel.ToString("X2"); }
            set
            {
                if (byte.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out byte v))
                {
                    Set(ref msubLevel, v);
                }
            }
        }
        public byte RootLevel
        {
            get { return msubLevel; }
        }
        private byte msubLevel = 0x12;

        public string DllPath
        {
            get { return mDllPath; }
            set { Set(ref mDllPath, value); }
        }
        private string mDllPath = string.Empty;

        public override string Desc
        {
            get { return mDesc; }
            set { Set(ref mDesc, value); }
        }
        private string mDesc = "安全访问";
        public bool IgnoreUnlockedCheck
        {
            get { return mIgnoreUnlockedCheck; }
            set { Set(ref mIgnoreUnlockedCheck, value); }
        }
        private bool mIgnoreUnlockedCheck;
        public ZFlashSecurity()
        {
            this.ZFlashUdsType = ZFlashUdsEnum.SecrityAcess;
            //< functionaddr > false </ functionaddr >
            //< level > 17 </ level >
            //< dll > SeednKey升级.dll </ dll >
            //< desc > ECU解锁 </ desc >
            //< ignore_unlocked_check > false </ ignore_unlocked_check >
        }
        public ZFlashSecurity ParseZflashBuilder(XElement ele)
        {
            this.mFuncAddr = Convert.ToBoolean(ele.Element("functionaddr").Value);
            this.mLevel = Convert.ToByte(ele.Element("level").Value);
            this.mDllPath = ele.Element("dll").Value;
            this.mDesc = ele.Element("desc").Value;
            this.mIgnoreUnlockedCheck = Convert.ToBoolean(ele.Element("ignore_unlocked_check").Value);
            return this;
        }

        public override string ToString()
        {
            return $"级别：{Level} DLL:{DllPath}";
        }

        public override string Data
        {
            get { return $"级别：{Level} DLL:{DllPath}"; }
        }
        public override int GetAddressIndex()
        {
            return Functionaddr ? 1 : 0;
        }
    }
}
