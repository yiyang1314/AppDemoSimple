using System;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashGener : ZFlashItem
    {
        public override bool Functionaddr
        {
            get { return mFuncAddr; }
            set
            {
                Set(ref mFuncAddr, value);
            }
        }
        private bool mFuncAddr = true;
        public bool IsCrcCmd
        {
            get { return mIsCrcCmd; }
            set { Set(ref mIsCrcCmd, value); }
        }
        private bool mIsCrcCmd;
        public string Sid
        {
            get { return mSid; }
            set { Set(ref mSid, value); }
        }
        private string mSid = "10";

        public string SubId
        {
            get { return mSubId; }
            set
            {
                Set(ref mSubId, value);
            }
        }
        private string mSubId = "";

        public override string Desc
        {
            get { return mDesc; }
            set { Set(ref mDesc, value); }
        }
        private string mDesc = "诊断会话控制";

        public override string Data
        {
            get { return mData; }
            set { Set(ref mData, value); }
        }
        private string mData = "";

        public bool SuppressRespon
        {
            get { return mSuppressRespon; }
            set { Set(ref mSuppressRespon, value); }
        }
        private bool mSuppressRespon;
        public int SuppressResponIndex
        {
            get { return SuppressRespon ? 0 : 1; }
            set
            {
                SuppressRespon = value == 0;
            }
        }

        public ZFlashGener()
        {
            this.ZFlashUdsType = ZFlashUdsEnum.General;
            //< functionaddr > false </ functionaddr >
            //< is_crc_cmd > false </ is_crc_cmd >
            //< sid > 11 </ sid >
            //< desc > ECU重置 </ desc >
            //< data > 01 </ data >
            //< suppress_response > false </ suppress_response >
        }

        public ZFlashGener ParseZflashBuilder(XElement ele)
        {
            this.mFuncAddr = Convert.ToBoolean(ele.Element("functionaddr").Value);
            this.mIsCrcCmd = Convert.ToBoolean(ele.Element("is_crc_cmd").Value);
            this.mSid = ele.Element("sid").Value;
            this.mDesc = ele.Element("desc").Value;
            this.mData = ele.Element("data").Value;
            this.mSuppressRespon = Convert.ToBoolean(ele.Element("suppress_response").Value);
            return this;
        }

        public override string ToString()
        {
            return $"数据[{this.Data}]";
        }

        public override int GetAddressIndex()
        {
            return Functionaddr ? 1 : 0;
        }
    }

    public class ZFlashExdFunc : ZFlashItem
    {
        public override bool Functionaddr
        {
            get { return mFuncAddr; }
            set
            {
                Set(ref mFuncAddr, value);
            }
        }
        private bool mFuncAddr = true;
        public uint FuncID
        {
            get { return funcID; }
            set { Set(ref funcID, value); }
        }
        private uint funcID = 0x7a0;

        public uint DstID
        {
            get { return dstID; }
            set { Set(ref dstID, value); }
        }
        private uint dstID = 0x7a0;

        public string Sid
        {
            get { return mSid; }
            set { Set(ref mSid, value); }
        }
        private string mSid = "10";

        public string SubId
        {
            get { return mSubId; }
            set
            {
                Set(ref mSubId, value);
            }
        }
        private string mSubId = "";

        public override string Desc
        {
            get { return mDesc; }
            set { Set(ref mDesc, value); }
        }
        private string mDesc = "扩展功能";

        public override string Data
        {
            get { return mData; }
            set { Set(ref mData, value); }
        }
        private string mData = "";

        public bool SuppressRespon
        {
            get { return mSuppressRespon; }
            set { Set(ref mSuppressRespon, value); }
        }
        private bool mSuppressRespon;
        public int SuppressResponIndex
        {
            get { return SuppressRespon ? 0 : 1; }
            set
            {
                SuppressRespon = value == 0;
            }
        }

        public ZFlashExdFunc()
        {
            this.ZFlashUdsType = ZFlashUdsEnum.General;
        }


        public override string ToString()
        {
            return $"扩展功能-[{this.Data}]";
        }

        public override int GetAddressIndex()
        {
            return Functionaddr ? 1 : 0;
        }
    }
}
