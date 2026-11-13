using System;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashSetting : ZFlashItem
    {
        public ZFlashSetting()
        {
            this.ZFlashUdsType = ZFlashUdsEnum.Setting;
            //< src_addr > 700 </ src_addr >
            //< dst_addr > 79f </ dst_addr >
            //< fun_addr > 7df </ fun_addr >
            //< phy_addr > 797 </ phy_addr >
            //< comm_frame_type > 0 </ comm_frame_type >
            //< session_keep_suppress_response > 1 </ session_keep_suppress_response >
            //< session_keep_enable > 0 </ session_keep_enable >
            //< session_keep_addr > 797 </ session_keep_addr >
            //< timeout > 2000 </ timeout >
            //< neg78_timeout > 5000 </ neg78_timeout >
            //< local_st_min > 0 </ local_st_min >
            //< remote_st_min > 0 </ remote_st_min >
            //< is_modify_ecu_stmin > 1 </ is_modify_ecu_stmin >
            //< file_block_interval > 0 </ file_block_interval >
            //< file_block_retry > 2 </ file_block_retry >
            //< block_size > 0 </ block_size >
            //< version > 1 </ version >
            //< is_fill_byte > 1 </ is_fill_byte >
            //< fill_byte > 0 </ fill_byte >
            //< fc_timeout > 1000 </ fc_timeout >
            //< session_keep_interval > 2000 </ session_keep_interval >
            //< show_session_keep_info > 0 </ show_session_keep_info >
            //< show_raw_frame > 1 </ show_raw_frame >
            //< output_max_row_count > 100000 </ output_max_row_count >
            //< single_file_max_row_count > 100000 </ single_file_max_row_count >
            //< lib_path ></ lib_path >
            //< can_type > 0 </ can_type >
            //< check_any_negative_response > 0 </ check_any_negative_response >
            //< wait_if_suppress_response > 1 </ wait_if_suppress_response >
        }
        #region 属性
        public string SrcAddr
        {
            get { return mSrcAddr; }
            set
            {
                Set(ref mSrcAddr, value);
            }
        }
        private string mSrcAddr = "791";

        public string DstAddr
        {
            get { return mDstAddr; }
            set { Set(ref mDstAddr, value); }
        }
        private string mDstAddr = "799";

        public string FunAddr
        {
            get { return mFunAddr; }
            set { Set(ref mFunAddr, value); }
        }
        private string mFunAddr = "7df";

        public string PhyAddr
        {
            get { return mPhyAddr; }
            set { Set(ref mPhyAddr, value); }
        }
        private string mPhyAddr = "791";

        public byte CommFrameType
        {
            get { return mCommFrameType; }
            set
            {
                Set(ref mCommFrameType, value);
            }
        }
        private byte mCommFrameType = 0;

        public int SessionKeepSuppreResp
        {
            get { return mSessionKeepSuppreResp; }
            set { Set(ref mSessionKeepSuppreResp, value); }
        }
        private int mSessionKeepSuppreResp;

        public int SessionKeepEnable
        {
            get { return mSessionKeepEnable; }
            set { Set(ref mSessionKeepEnable, value); }
        }
        private int mSessionKeepEnable;

        public string SessionKeepAddr
        {
            get { return mSessionKeepAddr; }
            set { Set(ref mSessionKeepAddr, value); }
        }
        private string mSessionKeepAddr = "7df";

        public int Timeout
        {
            get { return mTimeout; }
            set { Set(ref mTimeout, value); }
        }
        private int mTimeout;

        public int Neg78Timeout
        {
            get { return mNeg78Timeout; }
            set { Set(ref mNeg78Timeout, value); }
        }
        private int mNeg78Timeout;

        public int LocalStMin
        {
            get { return mLocalStMin; }
            set { Set(ref mLocalStMin, value); }
        }
        private int mLocalStMin;
        public int RemoteStMin
        {
            get { return mRemoteStMin; }
            set { Set(ref mRemoteStMin, value); }
        }
        private int mRemoteStMin;
        public int IsModifyEcuStmin
        {
            get { return mIsModifyEcuStmin; }
            set { Set(ref mIsModifyEcuStmin, value); }
        }
        private int mIsModifyEcuStmin;
        public int FileBlockInterval
        {
            get { return mFileBlockInterval; }
            set { Set(ref mFileBlockInterval, value); }
        }
        private int mFileBlockInterval;
        public int FileBlockRetry
        {
            get { return mFileBlockRetry; }
            set { Set(ref mFileBlockRetry, value); }
        }
        private int mFileBlockRetry;
        public int BlockSize
        {
            get { return mBlockSize; }
            set { Set(ref mBlockSize, value); }
        }
        private int mBlockSize;
        public int Version
        {
            get { return mVersion; }
            set { Set(ref mVersion, value); }
        }
        private int mVersion;
        public int IsFillByte
        {
            get { return mIsFillByte; }
            set { Set(ref mIsFillByte, value); }
        }
        private int mIsFillByte;
        public int FillByte
        {
            get { return mFillByte; }
            set { Set(ref mFillByte, value); }
        }
        private int mFillByte;
        public int FcTimeout
        {
            get { return mFcTimeout; }
            set { Set(ref mFcTimeout, value); }
        }
        private int mFcTimeout;
        public int SessionKeepInterval
        {
            get { return mSessionKeepInterval; }
            set { Set(ref mSessionKeepInterval, value); }
        }
        private int mSessionKeepInterval;
        public int ShowSessionKeepInfo
        {
            get { return mShowSessionKeepInfo; }
            set { Set(ref mShowSessionKeepInfo, value); }
        }
        private int mShowSessionKeepInfo;
        public int ShowRawFrame
        {
            get { return mShowRawFrame; }
            set { Set(ref mShowRawFrame, value); }
        }
        private int mShowRawFrame;
        public int OutputMaxRowCount
        {
            get { return mOutputMaxRowCount; }
            set { Set(ref mOutputMaxRowCount, value); }
        }
        private int mOutputMaxRowCount;
        public int SingleFileMaxRowCount
        {
            get { return mSingleFileMaxRowCount; }
            set { Set(ref mSingleFileMaxRowCount, value); }
        }
        private int mSingleFileMaxRowCount;

        public string LibPath
        {
            get { return mLibPath; }
            set { Set(ref mLibPath, value); }
        }
        private string mLibPath = string.Empty;

        public byte CanType
        {
            get { return mCanType; }
            set { Set(ref mCanType, value); }
        }
        private byte mCanType;

        public byte CheckAnyNegativeResp
        {
            get { return mCheckAnyNegativeResp; }
            set { Set(ref mCheckAnyNegativeResp, value); }
        }
        private byte mCheckAnyNegativeResp;

        public byte WaitIfSuppressResp
        {
            get { return mWaitIfSuppressResp; }
            set { Set(ref mWaitIfSuppressResp, value); }
        }
        private byte mWaitIfSuppressResp;
        #endregion 属性
        public ZFlashSetting ParseZflashBuilder(XElement ele)
        {
            this.mSrcAddr = ele.Element("src_addr").Value;
            this.mDstAddr = ele.Element("dst_addr").Value;
            this.mFunAddr = ele.Element("fun_addr").Value;
            this.mPhyAddr = ele.Element("phy_addr").Value;
            this.mCommFrameType = Convert.ToByte(ele.Element("comm_frame_type").Value);
            this.mSessionKeepSuppreResp = Convert.ToByte(ele.Element("session_keep_suppress_response").Value);
            this.mSessionKeepEnable = Convert.ToByte(ele.Element("session_keep_enable").Value);
            this.mSessionKeepAddr = ele.Element("session_keep_addr").Value;
            this.mTimeout = Convert.ToInt32(ele.Element("timeout").Value);
            this.mNeg78Timeout = Convert.ToInt32(ele.Element("neg78_timeout").Value);
            this.mLocalStMin = Convert.ToInt32(ele.Element("local_st_min").Value);
            this.mRemoteStMin = Convert.ToInt32(ele.Element("remote_st_min").Value);
            this.mIsModifyEcuStmin = Convert.ToInt32(ele.Element("is_modify_ecu_stmin").Value);
            this.mFileBlockInterval = Convert.ToInt32(ele.Element("file_block_interval").Value);
            this.mFileBlockRetry = Convert.ToInt32(ele.Element("file_block_retry").Value);
            this.mBlockSize = Convert.ToInt32(ele.Element("block_size").Value);
            this.mVersion = Convert.ToInt32(ele.Element("version").Value);
            this.mIsFillByte = Convert.ToInt32(ele.Element("is_fill_byte").Value);
            this.mFillByte = Convert.ToInt32(ele.Element("fill_byte").Value);
            this.mFcTimeout = Convert.ToInt32(ele.Element("fc_timeout").Value);
            this.mSessionKeepInterval = Convert.ToInt32(ele.Element("session_keep_interval").Value);
            this.mShowSessionKeepInfo = Convert.ToInt32(ele.Element("show_session_keep_info").Value);
            this.mShowRawFrame = Convert.ToInt32(ele.Element("show_raw_frame").Value);
            this.mOutputMaxRowCount = Convert.ToInt32(ele.Element("output_max_row_count").Value);
            this.mSingleFileMaxRowCount = Convert.ToInt32(ele.Element("single_file_max_row_count").Value);
            this.mLibPath = ele.Element("lib_path").Value;
            this.mCanType = Convert.ToByte(ele.Element("can_type").Value);
            this.mCheckAnyNegativeResp = Convert.ToByte(ele.Element("check_any_negative_response").Value);
            this.mWaitIfSuppressResp = Convert.ToByte(ele.Element("wait_if_suppress_response").Value);
            return this;
        }
    }
}
