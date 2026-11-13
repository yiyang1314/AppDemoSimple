using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.Utils;
using System;
using System.IO;
using System.Xml.Linq;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class ZFlashDown : ZFlashItem
    {
        public override string Desc
        {
            get { return mDesc; }
            set { Set(ref mDesc, value); }
        }
        private string mDesc = "文件下载";

        public override bool Functionaddr
        {
            get { return mFunctionaddr; }
            set { Set(ref mFunctionaddr, value); }
        }
        private bool mFunctionaddr;

        public string File
        {
            get { return mFile; }
            set { Set(ref mFile, value); }
        }
        private string mFile = "";

        public int DataFormatIdentifier
        {
            get { return mDataFormatIdentifier; }
            set { Set(ref mDataFormatIdentifier, value); }
        }
        private int mDataFormatIdentifier;
        public int Addresssize
        {
            get { return mAddresssize; }
            set { Set(ref mAddresssize, value); }
        }
        private int mAddresssize;
        public int Lengthsize
        {
            get { return mLengthsize; }
            set { Set(ref mLengthsize, value); }
        }
        private int mLengthsize = 0;
        public int EraseEcuMemoryMode
        {
            get { return mEraseEcuMemoryMode; }
            set { Set(ref mEraseEcuMemoryMode, value); }
        }
        private int mEraseEcuMemoryMode;
        public int CrcType
        {
            get { return mCrcType; }
            set
            {
                Set(ref mCrcType, value);
                if (value == 0)
                {
                    CrcPoly = "4c11db7";
                    CrcInitValue = "FFFFFFFF";
                    CrcXorValue = "FFFFFFFF";
                }
                else if (value == 1)
                {
                    CrcPoly = "10";
                    CrcInitValue = "0";
                    CrcXorValue = "0";
                }
                else if (value == 2)
                {
                    CrcPoly = "7";
                    CrcInitValue = "0";
                    CrcXorValue = "0";
                }
                else
                {
                    CrcPoly = "0";
                    CrcInitValue = "0";
                    CrcXorValue = "0";
                }
                ValueChang();
            }
        }
        private int mCrcType;
        public string CrcPoly
        {
            get { return mCrcPoly.ToUpper(); }
            set { Set(ref mCrcPoly, value); }
        }
        private string mCrcPoly = "4c11db7";
        public string CrcInitValue
        {
            get { return mCrcInitValue.ToUpper(); }
            set { Set(ref mCrcInitValue, value); }
        }
        private string mCrcInitValue = "ffffffff";
        public string CrcXorValue
        {
            get { return mCrcXorValue.ToUpper(); }
            set { Set(ref mCrcXorValue, value); }
        }
        private string mCrcXorValue = "ffffffff";
        public int CrcInReverse
        {
            get { return mCrcInReverse; }
            set
            {
                Set(ref mCrcInReverse, value); ValueChang();
            }
        }
        private int mCrcInReverse;
        public int CrcOutReverse
        {
            get { return mCrcOutReverse; }
            set
            {
                Set(ref mCrcOutReverse, value); ValueChang();
            }
        }
        private int mCrcOutReverse;
        public string CrcCmd
        {
            get { return mCrcCmd.ToUpper(); }
            set { Set(ref mCrcCmd, value); }
        }
        private string mCrcCmd = "31 01 02 02";

        public string VarifyCrcCmd
        {
            get { return mVarifyCrcCmd.ToUpper(); }
            set { Set(ref mVarifyCrcCmd, value); }
        }
        private string mVarifyCrcCmd = "";

        public int CrcEnable
        {
            get { return mCrcEnable; }
            set { Set(ref mCrcEnable, value); }
        }
        private int mCrcEnable;
        public int CrcBlockEnable
        {
            get { return mCrcBlockEnable; }
            set { Set(ref mCrcBlockEnable, value); }
        }
        private int mCrcBlockEnable = 0;
        public int CustomParamForS37Enable
        {
            get { return mCustomParamForS37Enable; }
            set { Set(ref mCustomParamForS37Enable, value); }
        }
        private int mCustomParamForS37Enable = 0;
        public string CustomParamForS37
        {
            get { return mCustomParamForS37; }
            set { Set(ref mCustomParamForS37, value); }
        }
        private string mCustomParamForS37 = "";
        public string FileBlocksEnable
        {
            get { return mFileBlocksEnable; }
            set { Set(ref mFileBlocksEnable, value); }
        }
        private string mFileBlocksEnable = "0,1";
        public int EverFileBlockDelay
        {
            get { return mEverFileBlockDelay; }
            set { Set(ref mEverFileBlockDelay, value); }
        }
        private int mEverFileBlockDelay;
        public int CrcBlockCustomEnable
        {
            get { return mCrcBlockCustomEnable; }
            set { Set(ref mCrcBlockCustomEnable, value); }
        }
        private int mCrcBlockCustomEnable;
        public string CrcBlockCustomCmd
        {
            get { return mCrcBlockCustomCmd.ToUpper(); }
            set { Set(ref mCrcBlockCustomCmd, value); }
        }
        private string mCrcBlockCustomCmd = "31 01 F1 A0";

        public ZFlashFileBlock ZFlashBlock
        {
            get { return mZFlashBlock; }
            set { Set(ref mZFlashBlock, value); }
        }
        private ZFlashFileBlock mZFlashBlock;
        public ZFlashDown()
        {
            this.ZFlashUdsType = ZFlashUdsEnum.FileDown;
            //< desc > FLASH驱动下载 </ desc >
            //< functionaddr > false </ functionaddr >
            //< file > UDS_FlashDriver.hex </ file >
            //< dataFormatIdentifier > 0 </ dataFormatIdentifier >
            //< addresssize > 4 </ addresssize >
            //< lengthsize > 3 </ lengthsize >
            //< erase_ecu_memory_mode > 3 </ erase_ecu_memory_mode >
            //< crc_type > 0 </ crc_type >
            //< crc_poly > 4c11db7 </ crc_poly >
            //< crc_init_value > ffffffff </ crc_init_value >
            //< crc_xor_value > ffffffff </ crc_xor_value >
            //< crc_in_reverse > 1 </ crc_in_reverse >
            //< crc_out_reverse > 1 </ crc_out_reverse >
            //< crc_cmd > 31 01 F0 01 </ crc_cmd >
            //< crc_enable > 1 </ crc_enable >
            //< crc_block_enable > 0 </ crc_block_enable >
            //< customParamForS37Enable > 0 </ customParamForS37Enable >
            //< customParamForS37 ></ customParamForS37 >
            //< file_blocks_enable > 0,1 </ file_blocks_enable >
            //< ever_file_block_delay > 0 </ ever_file_block_delay >
            //< crc_block_custom_enable > 0 </ crc_block_custom_enable >
            //< crc_block_custom_cmd > 31 01 F1 A0</ crc_block_custom_cmd >
        }
        public ZFlashDown ParseZflashBuilder(XElement ele)
        {
            this.mDesc = ele.Element("desc").Value;
            this.mFunctionaddr = Convert.ToBoolean(ele.Element("functionaddr").Value);
            this.mFile = ele.Element("file").Value;
            this.mDataFormatIdentifier = Convert.ToInt32(ele.Element("dataFormatIdentifier").Value);
            this.mAddresssize = Convert.ToInt32(ele.Element("addresssize").Value);
            this.mLengthsize = Convert.ToInt32(ele.Element("lengthsize").Value);
            this.mEraseEcuMemoryMode = Convert.ToInt32(ele.Element("erase_ecu_memory_mode").Value);
            this.mCrcType = Convert.ToInt32(ele.Element("crc_type").Value);
            this.mCrcPoly = ele.Element("crc_poly").Value;
            this.mCrcInitValue = ele.Element("crc_init_value").Value;
            this.mCrcXorValue = ele.Element("crc_xor_value").Value;
            this.mCrcInReverse = Convert.ToByte(ele.Element("crc_in_reverse").Value);
            this.mCrcOutReverse = Convert.ToByte(ele.Element("crc_out_reverse").Value);
            this.mCrcCmd = ele.Element("crc_cmd").Value;
            this.mCrcEnable = Convert.ToByte(ele.Element("crc_enable").Value);
            this.mCrcBlockEnable = Convert.ToByte(ele.Element("crc_block_enable").Value);
            this.mCustomParamForS37Enable = Convert.ToByte(ele.Element("customParamForS37Enable").Value);
            this.mCustomParamForS37 = ele.Element("customParamForS37").Value;
            this.mFileBlocksEnable = ele.Element("file_blocks_enable").Value;
            this.mEverFileBlockDelay = Convert.ToByte(ele.Element("ever_file_block_delay").Value);
            this.mCrcBlockCustomEnable = Convert.ToByte(ele.Element("crc_block_custom_enable").Value);
            this.mCrcBlockCustomCmd = ele.Element("crc_block_custom_cmd").Value;
            return this;
        }

        public override string ToString()
        {
            return $"文件名：{File}";
        }

        public override int GetAddressIndex()
        {
            return Functionaddr ? 1 : 0;
        }
        public override string Data
        {
            get { return $"文件名：{File}"; }
        }
        public string VerifResult
        {
            get { return mVerifResult.ToUpper(); }
            set { Set(ref mVerifResult, value); }
        }
        private string mVerifResult = "";
        public string HexPath
        {
            get { return mHexPath; }
            set { Set(ref mHexPath, value); }
        }
        private string mHexPath = "";

        //string VerifResult
        public void ValueChang()
        {
            if (ZFlashBlock == null || ZFlashBlock.Datas == null)
            {
                return;
            }

            CrcType[] types = new CrcType[] {
                Models.CrcType.Crc32,
                Models.CrcType.Crc16,
                Models.CrcType.Crc8,
                Models.CrcType.KYCrc32,
                Models.CrcType.BiHuCRC
            };
            CrcConfig crcConfig = new CrcConfig()
            {
                Type = types[this.CrcType],
                InitialValue = Convert.ToUInt32(CrcInitValue, 16),
                Polynomial = Convert.ToUInt32(CrcPoly, 16),
                InputReverse = CrcInReverse == 1,
                OutputReverse = CrcOutReverse == 1,
                XorOutput = Convert.ToUInt32(CrcXorValue, 16),
            };

            if (crcConfig.Type == Models.CrcType.KYCrc32)
            {
                //CrcCalculator.InvokeExe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "ZCANPRO_CRC.exe"), out string result , File);
                if (SimpleEcuTool.CalcKYCrc(out string msg, out string[] result, Path.Combine(HexPath, File)))
                {
                    VerifResult = string.Join("\n", result);
                }
            }
            else if (crcConfig.Type == Models.CrcType.BiHuCRC)
            {
                //CrcCalculator.InvokeExe(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "ZCANPRO_CRC.exe"), out string result, File);
                if (SimpleEcuTool.CalcBhCrc(false, out string Crcresult1, Path.Combine(HexPath, File)))
                {
                    VerifResult = Crcresult1;
                }
            }
            else
            {
                CrcCalculator.CalculateCrc(ZFlashBlock.Datas.Data.ToArray(), crcConfig, out string vaildstr);
                VarifyCrcCmd = vaildstr;
            }
        }
    }
    public class ZFlashFileBlock : ViewModelBase
    {
        public bool IsSelect
        {
            get { return _IsSelect; }
            set
            {
                Set(ref _IsSelect, value);
            }
        }
        private bool _IsSelect = true;

        public string Address
        {
            get { return mAddress; }
            set
            {
                Set(ref mAddress, value);
            }
        }
        private string mAddress;

        public int Percent
        {
            get { return mPercent; }
            set
            {
                Set(ref mPercent, value);
            }
        }
        private int mPercent;

        public string BlockTitle
        {
            get { return mBlockTitle; }
            set
            {
                Set(ref mBlockTitle, value);
            }
        }
        private string mBlockTitle;

        public long Length
        {
            get { return mLength; }
            set
            {
                Set(ref mLength, value);
            }
        }
        private long mLength = 0;
        public DataBlock Datas { get; set; }

        public override string ToString()
        {
            return $"1.地址0x{Address}, 长度{Length}";
        }

    }
}
