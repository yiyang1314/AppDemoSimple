using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.ZflashItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rxbit.Brigade.EcuServices.Utils
{
    public static class SimpleEcuTool
    {
        public static Func<string,string> DirsKey_DataComed { get; set; }
        #region 常量路径定义
        /// <summary>
        /// 三方软件
        /// </summary>
        private static string FLASH_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu");
        public static string Hex2Bin_EXE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "hex2bin.exe");
        public static string Bin2Hex_EXE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "bin2hex.exe");
        private static string KY_EXE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "ZCANPRO_CRC.exe");
        private static string BH_EXE = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sub-exe", "AES-CMAC.exe");

        /// <summary>
        /// Flash驱动
        /// </summary>
        private static string KY_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "KY_FlashDriver.hex");
        private static string TC_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Tc_flash_driver_ram_IS_GD.hex");
        private static string SDXA_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "sdxa_flash_driver.hex");
        private static string BSIS_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "bsiss_flashdrive.hex");
        private static string MG_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "TC_FlashDriver.hex");
        private static string BH_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Bh_FlashDriver.hex");
        private static string Brigade_FLASH_DRIVER = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Brigade_flash.hex");

        /// <summary>
        /// 解密算法路径
        /// </summary>
        private static string MG_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Mg_SeednKey.dll");
        private static string BH_L_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "SeednKey_BH_CRRL.dll");
        private static string BH_R_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "SeednKey_BH_CRRR.dll");
        private static string TC_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "TC_SeednKey.dll");
        private static string KY_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "SeednKey_KY.dll");
        private static string SDXA_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "SDXA_SeednKey.dll");
        private static string BSIS_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "bsiss_SeednKey.dll");
        public static string Brigade_SECRITY_DLL = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RadarEcu", "Brigade_SeednKey.dll");
        #endregion

        public static List<ZFlashItem> ParseHexPackage(EcuRadar radar, string hexfile, out string errormsg, bool IsLrRadar = false)
        {
            List<ZFlashItem> list = new List<ZFlashItem>();
            switch (radar)
            {
                case EcuRadar.MG_Radar:
                    errormsg = MgTC404(list, hexfile);
                    break;
                case EcuRadar.AMFRRadar:
                    errormsg = ShzjTC404(list, hexfile);
                    break;
                case EcuRadar.BissRadar:
                    errormsg = Update1stBsis(list, hexfile);
                    break;
                case EcuRadar.AngleRadar:
                    errormsg = UpdateKyAngle(list, hexfile);
                    break;
                case EcuRadar.BiHu_Radar:
                    errormsg = UpdateBhAngle(list, hexfile, IsLrRadar);
                    break;
                case EcuRadar.Brigade_Radar:
                    errormsg = Brigade(list, hexfile);
                    break;
                case EcuRadar.SDXARadar:
                    errormsg = UpdateBsis(list, hexfile);
                    break;
                default:
                    errormsg = Update1stBsis(list, hexfile);
                    break;
            }
            return list;
        }

        #region 凯翼-前雷达-侧盲 App烧录

        /// <summary>
        /// 旧版前雷达升级流程
        /// </summary>
        /// <param name="errormsg"></param>
        /// <param name="hexFile"></param>
        /// <returns></returns>
        public static string Brigade(List<ZFlashItem> list, string hexFile)
        {
            if (!File.Exists(Brigade_SECRITY_DLL))
            {
                return DirsKey_DataComed.Invoke("Ecuboot_NoDllip");
            }
            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow14Tip") });
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow1Tip") });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow5Tip") });
            
            list.Add(new ZFlashSecurity() { DllPath = Brigade_SECRITY_DLL, Level = 17, NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow6Tip") });
            // 1: 加载falsh文件
            if (!File.Exists(Brigade_FLASH_DRIVER))
            {
                return DirsKey_DataComed.Invoke("Ecuboot_FlashNoExistTip");
            }
            //DataBlock block = new DataBlock();
            FlashParseFile.ParseHexFileEx(Brigade_FLASH_DRIVER, out DataBlock block);
            string crcreult = "";
            if (block == null)
            {
                return DirsKey_DataComed.Invoke("Ecuboot_FlashParserErrorTip");
            }
            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = Brigade_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow7Tip")
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 DF 01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow11Tip") });
            // 1: 加载APP文件
            ParseHexFile parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(hexFile, ref block);
            if (block == null)
            {
                return DirsKey_DataComed.Invoke("Ecuboot_AppFlashTip");
            }
            var block1 = block;
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow10Tip")
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 DF 01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow8Tip") });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow12Tip") });
            list.Add(new ZFlashExdFunc() { NumID = numId++, Desc = "Check Version" });
            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow13Tip") });
            list.Add(new ZFlashDelay() { DelayTime = 1000, NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow22Tip") });
            list.Add(new ZFlashGener() { Sid = "10", Data = "01", SubId = "01", NumID = numId++, Desc = DirsKey_DataComed.Invoke("Ecuboot_Flow14Tip") });
            return "";
        }

        /// <summary>
        /// 旧版前雷达升级流程
        /// </summary>
        /// <param name="errormsg"></param>
        /// <param name="hexFile"></param>
        /// <returns></returns>
        public static string MgTC404(List<ZFlashItem> list, string hexFile)
        {
            if (!File.Exists(MG_SECRITY_DLL))
            {
                return "当前文件夹没有对应DLL文件";
            }
            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展模式" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 02", SubId = "01", NumID = numId++, Desc = "检查预编程条件" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "关闭DTC诊断" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "03 01", SubId = "03", NumID = numId++, Desc = "关闭非诊断通信" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });
            list.Add(new ZFlashSecurity() { DllPath = MG_SECRITY_DLL, Level = 17, NumID = numId++, Desc = "ECU解锁" });
            // 1: 加载falsh文件
            if (!File.Exists(MG_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            DataBlock block = new DataBlock();
            ParseHexFile parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(MG_FLASH_DRIVER, ref block);
            string crcreult = "";
            if (block == null)
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret))
            {
                crcreult = ret.ToUpper();
            }

            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = MG_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "2E", Data = "F1 5A 20 22 01 01 20 20 20 20 20 20 20 20", SubId = "", NumID = numId++, Desc = "写入指纹信息" });
            // 1: 加载APP文件
            parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(hexFile, ref block);
            if (block == null)
            {
                return "当前文件夹App文件不存在";
            }
            var block1 = block;
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret1))
            {
                crcreult = ret1.ToUpper();
            }
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = "依赖性检查" });
            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 1000, NumID = numId++, Desc = "延时" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "诊断会话控制" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "01", SubId = "01", NumID = numId++, Desc = "控制DTC设置" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "00 03 ", SubId = "00", NumID = numId++, Desc = "通讯控制" });
            return "";
        }

        /// <summary>
        /// 上海支架前雷达
        /// </summary>
        /// <param name="list">烧录流程集合</param>
        /// <param name="hexFile">App文件</param>
        /// <returns></returns>
        public static string ShzjTC404(List<ZFlashItem> list, string hexFile)
        {
            string errormsg = string.Empty;
            if (!File.Exists(TC_SECRITY_DLL))
            {
                return "当前文件夹没有对应DLL文件";
            }

            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展模式" });
            // list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 02", SubId = "01", NumID = numId++, Desc = "检查预编程条件" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "关闭DTC诊断" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "03 01", SubId = "03", NumID = numId++, Desc = "关闭非诊断通信" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });

            //string dllpath = Path.Combine(Path.GetDirectoryName(hexFile), "UDS_FlashDriver.hex");
            list.Add(new ZFlashSecurity() { DllPath = TC_SECRITY_DLL, Level = 17, NumID = numId++, Desc = "ECU解锁" });

            list.Add(new ZFlashGener() { Sid = "2E", Data = "F1 84 25 06 12 20 20 20 20 20 20 22", SubId = "", NumID = numId++, Desc = "写指纹信息" });
            list.Add(new ZFlashGener() { Sid = "22", Data = "F1 84", SubId = "", NumID = numId++, Desc = "按标识符读取数据" });

            // 1: 加载falsh文件
            if (!File.Exists(TC_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            DataBlock block = new DataBlock();
            ParseHexFile parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(TC_FLASH_DRIVER, ref block);
            string crcreult = "";
            if (block == null)
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret))
            {
                crcreult = ret.ToUpper();
            }

            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = TC_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F1 A0 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });

            // 1: 加载APP文件
            parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(hexFile, ref block);
            if (block == null)
            {
                return "当前文件夹App文件不存在";
            }
            var block1 = block;
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret1))
            {
                crcreult = ret1.ToUpper();
            }
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F1 A0 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = "依赖性检查" });
            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 3000, NumID = numId++, Desc = "延时" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "诊断会话控制" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "00 03", SubId = "00", NumID = numId++, Desc = "通讯控制" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "01", SubId = "01", NumID = numId++, Desc = "关闭DTC诊断" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "01", SubId = "01", NumID = numId++, Desc = "进入默认会话" });
            list.Add(new ZFlashGener() { Sid = "14", Data = "FF FF FF", SubId = "", NumID = numId++, Desc = "清除诊断信息" });
            list.Add(new ZFlashExdFunc() { NumID = numId++, Desc = "雷达App升级完成" });
            return errormsg;
        }

        /// <summary>
        /// 时代新安侧盲
        /// </summary>
        /// <param name="list">烧录流程集合</param>
        /// <param name="hexFile">App文件</param>
        /// <returns></returns>
        public static string UpdateBsis(List<ZFlashItem> list, string hexFile)
        {
            string errormsg = string.Empty;
            if (!File.Exists(BSIS_SECRITY_DLL))
            {
                return "当前文件夹没有对应DLL文件";
            }

            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展会话" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "控制DTC设置" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "03 01", SubId = "03", NumID = numId++, Desc = "通讯控制" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });

            //string dllpath = Path.Combine(Path.GetDirectoryName(hexFile), "UDS_FlashDriver.hex");
            list.Add(new ZFlashSecurity() { DllPath = SDXA_SECRITY_DLL, Level = 3, NumID = numId++, Desc = "ECU解锁" });

            list.Add(new ZFlashGener() { Sid = "2E", Data = "F1 84 01 00 25 06 12 20 20 20 20 20 20 22", SubId = "", NumID = numId++, Desc = "写入指纹信息" });
            list.Add(new ZFlashGener() { Sid = "22", Data = "F1 84", SubId = "", NumID = numId++, Desc = "按标识符读取数据" });

            // 1: 加载falsh文件
            if (!File.Exists(BSIS_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            DataBlock block = new DataBlock();
            FlashParseFile.ParseHexFileEx(SDXA_FLASH_DRIVER, out block);
            string crcreult = "";
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret))
            {
                crcreult = ret.ToUpper();
            }

            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = BSIS_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F1 A0 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            // 1: 加载APP文件
            FlashParseFile.ParseHexFileEx(hexFile, out block);
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹App文件不存在";
            }
            var block1 = block;
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret1))
            {
                crcreult = ret1.ToUpper();
            }
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });

            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F1 A0 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = "检查程序依赖性" });

            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 3000, NumID = numId++, Desc = "延时" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "诊断会话控制" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "00 03 ", SubId = "00", NumID = numId++, Desc = "通讯控制" });

            list.Add(new ZFlashGener() { Sid = "85", Data = "01", SubId = "01", NumID = numId++, Desc = "控制DTC设置" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "01", SubId = "01", NumID = numId++, Desc = "诊断会话控制" });
            list.Add(new ZFlashGener() { Sid = "14", Data = "FF FF FF", SubId = "", NumID = numId++, Desc = "清除诊断信息" });
            return errormsg;
        }

        /// <summary>
        /// 一代侧盲公版
        /// </summary>
        /// <param name="list">烧录流程集合</param>
        /// <param name="hexFile">App文件</param>
        /// <returns></returns>
        public static string Update1stBsis(List<ZFlashItem> list, string hexFile)
        {
            string errormsg = string.Empty;
            if (!File.Exists(BSIS_SECRITY_DLL))
            {
                return "当前文件夹没有对应DLL文件";
            }

            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展会话" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 02", SubId = "01", NumID = numId++, Desc = "检查预编程条件" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "控制DTC设置" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "03 01", SubId = "03", NumID = numId++, Desc = "通讯控制" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });

            //string dllpath = Path.Combine(Path.GetDirectoryName(hexFile), "UDS_FlashDriver.hex");
            list.Add(new ZFlashSecurity() { DllPath = BSIS_SECRITY_DLL, Level = 17, NumID = numId++, Desc = "ECU解锁" });
            // 1: 加载falsh文件
            if (!File.Exists(BSIS_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            DataBlock block = new DataBlock();
            ParseHexFile parseHexFile = new ParseHexFile();
            //parseHexFile.ParseSigleHexfile(BSIS_FLASH_DRIVER, ref block);
            FlashParseFile.ParseHexFileEx(BSIS_FLASH_DRIVER, out block);
            string crcreult = "";
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret))
            {
                crcreult = ret.ToUpper();
            }

            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = BSIS_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "2E", Data = "F1 5A 20 22 01 01 20 20 20 20 20 20 20 20", SubId = "", NumID = numId++, Desc = "写入指纹信息" });

            // 1: 加载APP文件
            FlashParseFile.ParseHexFileEx(hexFile, out block);
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹App文件不存在";
            }
            var block1 = block;
            if (CrcCalculator.CalculateCrc(block.Data.ToArray(), CrcCalculator.CRC32, out string ret1))
            {
                crcreult = ret1.ToUpper();
            }
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });

            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + crcreult, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = "检查程序依赖性" });

            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 3000, NumID = numId++, Desc = "延时" });
            return errormsg;
        }

        /// <summary>
        /// 角雷达
        /// </summary>
        /// <param name="list">烧录流程集合</param>
        /// <param name="hexFile">App文件</param>
        /// <returns></returns>
        public static string UpdateKyAngle(List<ZFlashItem> list, string hexFile)
        {
            string errormsg = string.Empty;
            if (!File.Exists(KY_SECRITY_DLL))
            {
                return "当前文件夹没有对应DLL文件";
            }

            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展模式" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "关闭DTC诊断" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "01 03", SubId = "01", NumID = numId++, Desc = "通讯控制" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });

            //string dllpath = Path.Combine(Path.GetDirectoryName(hexFile), "UDS_FlashDriver.hex");
            list.Add(new ZFlashSecurity() { DllPath = KY_SECRITY_DLL, Level = 3, NumID = numId++, Desc = "ECU解锁" });
            // 1: 加载falsh文件
            if (!File.Exists(KY_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            DataBlock block = new DataBlock();
            ParseHexFile parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(KY_FLASH_DRIVER, ref block);
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }

            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = KY_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });
            // 1: 加载APP文件
            parseHexFile = new ParseHexFile();
            parseHexFile.ParseSigleHexfile(hexFile, ref block);
            if (block == null || block.Data.Count == 0)
            {
                return "当前文件夹App文件不存在";
            }
            var block1 = block;
            if (!CrcCalculator.CalculateCrc(block1.Data.ToArray(), CrcCalculator.CRC16, out string result))
            {
                return result;
            }
            string str = FormatHexWithSpaces(block1.Address.ToString("X8") + block1.Data.Count.ToString("X8") + result);
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 00 44 " + str, SubId = "01", NumID = numId++, Desc = "启动例程" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "03 FF 00", SubId = "03", NumID = numId++, Desc = "获取例程结果" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "02 FF 00", SubId = "02", NumID = numId++, Desc = "停止例程" });
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 4,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });
            string str1 = FormatHexWithSpaces(block1.Data.Count.ToString("X8") + block1.Address.ToString("X8") + result);
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01 " + str1, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "03 FF 01", SubId = "03", NumID = numId++, Desc = "获取例程结果" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "02 FF 01", SubId = "02", NumID = numId++, Desc = "停止例程" });

            list.Add(new ZFlashGener() { Sid = "31", Data = "01 DA 04 " + FormatHexWithSpaces(result), SubId = "01", NumID = numId++, Desc = "启动例程" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "03 DA 04", SubId = "03", NumID = numId++, Desc = "获取例程结果" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "02 DA 04", SubId = "02", NumID = numId++, Desc = "停止例程" });

            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 1000, NumID = numId++, Desc = "延时" });
            list.Add(new ZFlashGener() { Sid = "14", Data = "FF FF FF", SubId = "", NumID = numId++, Desc = "清除诊断信息" });
            return errormsg;
        }

        /// <summary>
        /// 角雷达
        /// </summary>
        /// <param name="list">烧录流程集合</param>
        /// <param name="hexFile">App文件</param>
        /// <returns></returns>
        public static string UpdateBhAngle(List<ZFlashItem> list, string hexFile, bool IsLrRadar)
        {
            string datestr = DateTime.Now.ToString("yy dd MM ");
            string errormsg = string.Empty;
            string dllpath = IsLrRadar ? BH_R_SECRITY_DLL : BH_L_SECRITY_DLL;
            if (!File.Exists(dllpath))
            {
                return "当前文件夹没有对应DLL文件";
            }
            int numId = 1;
            list.Add(new ZFlashGener() { Sid = "10", Data = "03", SubId = "03", NumID = numId++, Desc = "进入扩展模式" });
            list.Add(new ZFlashGener() { Sid = "22", Data = "F1 5B", SubId = "", NumID = numId++, Desc = "按标识符读取数据" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 02", SubId = "01", NumID = numId++, Desc = "检查预编程条件" });
            list.Add(new ZFlashGener() { Sid = "85", Data = "02", SubId = "02", NumID = numId++, Desc = "关闭DTC诊断" });
            list.Add(new ZFlashGener() { Sid = "28", Data = "03 03", SubId = "03", NumID = numId++, Desc = "关闭非诊断通信" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "02", SubId = "02", NumID = numId++, Desc = "进入编程模式" });

            list.Add(new ZFlashSecurity() { DllPath = dllpath, Level = 0x09, NumID = numId++, Desc = "ECU解锁" });
            // 1: 加载falsh文件
            if (!File.Exists(BH_FLASH_DRIVER))
            {
                return "当前文件夹Flash驱动文件不存在";
            }
            if (!FlashParseFile.ParseHexFileEx(BH_FLASH_DRIVER, out DataBlock block))
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            ZFlashFileBlock flashBlock = new ZFlashFileBlock()
            {
                Address = block.Address.ToString("X8"),
                Length = block.Data.Count,
                Datas = block
            };
            list.Add(new ZFlashDown()
            {
                File = BH_FLASH_DRIVER,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock,
                EraseEcuMemoryMode = 3,
                DataFormatIdentifier = 0,
                Desc = "Flash驱动下载"
            });

            if (!CalcBhCrc(IsLrRadar, out string Crcresult, BH_FLASH_DRIVER))
            {
                return Crcresult;
            }
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + Crcresult, SubId = "01", NumID = numId++, Desc = "CRC校验" });

            // 2: 加载APP文件
            if (!FlashParseFile.ParseHexFileEx(hexFile, out DataBlock block1))
            {
                return "当前文件夹Flash驱动文件无效或文件格式异常";
            }
            if (!CalcBhCrc(IsLrRadar, out string Crcresult1, hexFile))
            {
                return Crcresult1;
            }
            list.Add(new ZFlashGener() { Sid = "2E", Data = "F1 5A 25 12 01 " + Crcresult1 + " 30 02", SubId = "", NumID = numId++, Desc = "按标识符写数据" });
            ZFlashFileBlock flashBlock1 = new ZFlashFileBlock()
            {
                Address = block1.Address.ToString("X8"),
                Length = block1.Data.Count,
                Datas = block1
            };
            list.Add(new ZFlashDown()
            {
                File = hexFile,
                Addresssize = 4,
                Lengthsize = 3,
                NumID = numId++,
                ZFlashBlock = flashBlock1,
                EraseEcuMemoryMode = 0,
                DataFormatIdentifier = 0,
                Desc = "文件APP下载"
            });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 F0 01 " + Crcresult1, SubId = "01", NumID = numId++, Desc = "CRC校验" });
            list.Add(new ZFlashGener() { Sid = "31", Data = "01 FF 01", SubId = "01", NumID = numId++, Desc = "例程控制" });

            list.Add(new ZFlashGener() { Sid = "11", Data = "01", SubId = "01", NumID = numId++, Desc = "ECU重置" });
            list.Add(new ZFlashDelay() { DelayTime = 2000, NumID = numId++, Desc = "延时" });
            list.Add(new ZFlashGener() { Sid = "10", Data = "01", SubId = "01", NumID = numId++, Desc = "诊断会话控制" });
            return errormsg;
        }
        #endregion

        #region CRC 校验
        /// <summary>
        /// BH Crc校验
        /// </summary>
        /// <param name="exepath"></param>
        /// <param name="IsLrRadar"></param>
        /// <param name="Crcresult"></param>
        /// <param name="binFile"></param>
        /// <returns></returns>
        public static bool CalcBhCrc(bool IsLrRadar, out string Crcresult, string hexFile)
        {
            string binFile = Path.ChangeExtension(hexFile, ".bin");
            if (!File.Exists(binFile))
            {
                CrcCalculator.InvokeExe(Hex2Bin_EXE, out string result2, hexFile);
            }
            if (!CrcCalculator.InvokeExe(BH_EXE, out string result1, binFile))
            {
                Crcresult = "计算App CRC校验码异常";
                return false;
            }
            var crclist = result1.Split(new string[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (crclist.Length <= 2)
            {
                Crcresult = "CRC校验失败";
                return false;
            }
            var crc = crclist[crclist.Length - 2];
            if (IsLrRadar)
            {
                crc = crclist[crclist.Length - 1];
            }
            var strlist = crc.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            for (int i = 3; i < strlist.Length; i++)
            {
                FormatHexWithSpaces(sb, strlist[i]);
                sb.Append(" ");
            }
            Crcresult = sb.ToString().ToUpper().Trim();
            return true;
        }
        public static bool CalcKYCrc(out string errormsg, out string[] Crcresult, string hexFile)
        {
            Crcresult = new string[4];
            errormsg = string.Empty;
            if (!CrcCalculator.InvokeExe(KY_EXE, out string result1, hexFile))
            {
                errormsg = result1;
                return false;
            }
            var crclist1 = result1.Split(new string[] { ",", ":", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (crclist1.Length != 4)
            {
                errormsg = "APP CRC校验失败";
                return false;
            }
            Crcresult = crclist1.Select(u => FormatHexWithSpaces(u)).ToArray();
            return true;
        }

        /// <summary>
        /// 将十六进制字符串按每2个字符添加空格分隔
        /// </summary>
        /// <param name="hexString">原始十六进制字符串（不含空格）</param>
        /// <returns>带空格分隔的十六进制字符串</returns>
        public static string FormatHexWithSpaces(StringBuilder sb, string hexString)
        {
            // 空值/空字符串校验
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            // 移除原始字符串中可能存在的空格（可选，根据需求决定）
            hexString = hexString.Replace(" ", "");
            // 每2个字符遍历一次
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // 取当前2个字符（最后不足2个时取剩余部分）
                string chunk = hexString.Substring(i, Math.Min(2, hexString.Length - i));
                sb.Append(chunk);
                // 最后一段不添加空格
                if (i + 2 < hexString.Length)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }
        public static string FormatHexWithSpaces(string hexString)
        {
            StringBuilder sb = new StringBuilder();
            // 空值/空字符串校验
            if (string.IsNullOrEmpty(hexString))
            {
                return string.Empty;
            }

            // 移除原始字符串中可能存在的空格（可选，根据需求决定）
            hexString = hexString.Replace(" ", "");
            // 每2个字符遍历一次
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // 取当前2个字符（最后不足2个时取剩余部分）
                string chunk = hexString.Substring(i, Math.Min(2, hexString.Length - i));
                sb.Append(chunk);
                // 最后一段不添加空格
                if (i + 2 < hexString.Length)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString().ToUpper().Trim();
        }
        #endregion
    }
}
