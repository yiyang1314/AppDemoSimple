using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Concurrent;

namespace Rxbit.Brigade.Models.Helper
{
    public class BlfParser
    {
        /******************************************************************************
        *                                                                             *
        * read BL file                                                                *
        *                                                                             *
        ******************************************************************************/
        #region 定义常量
        const ulong GENERIC_READ = 0x80000000L;
        const ulong GENERIC_WRITE = 0x40000000L;
        const ulong GENERIC_EXECUT = 0x20000000L;
        const ulong GENERIC_ALL = 0x10000000L;
        const uint BL_OBJ_FLAG_TIME_TEN_MICS = 0x00000001;
        const uint BL_OBJ_FLAG_TIME_ONE_NANS = 0x00000002;
        const uint BL_OBJ_SIGNATURE = 0x4A424F4C;
        const uint BL_OBJ_TYPE_UNKNOWN = 0;
        const uint BL_OBJ_TYPE_CAN_MESSAGE = 1;
        const uint BL_OBJ_TYPE_CAN_ERROR = 2;
        const uint BL_OBJ_TYPE_CAN_OVERLOAD = 3;
        const uint BL_OBJ_TYPE_CAN_STATISTIC = 4;
        const uint BL_OBJ_TYPE_APP_TRIGGER = 5;
        const uint BL_OBJ_TYPE_ENV_INTEGER = 6;
        const uint BL_OBJ_TYPE_ENV_DOUBLE = 7;
        const uint BL_OBJ_TYPE_ENV_STRING = 8;
        const uint BL_OBJ_TYPE_ENV_DATA = 9;
        const uint BL_OBJ_TYPE_LOG_CONTAINER = 10;
        const uint BL_OBJ_TYPE_LIN_MESSAGE = 11;
        const uint BL_OBJ_TYPE_LIN_CRC_ERROR = 12;
        const uint BL_OBJ_TYPE_LIN_DLC_INFO = 13;
        const uint BL_OBJ_TYPE_LIN_RCV_ERROR = 14;
        const uint BL_OBJ_TYPE_LIN_SND_ERROR = 15;
        const uint BL_OBJ_TYPE_LIN_SLV_TIMEOUT = 16;
        const uint BL_OBJ_TYPE_LIN_SCHED_MODCH = 17;
        const uint BL_OBJ_TYPE_LIN_SYN_ERROR = 18;
        const uint BL_OBJ_TYPE_LIN_BAUDRATE = 19;
        const uint BL_OBJ_TYPE_LIN_SLEEP = 20;
        const uint BL_OBJ_TYPE_LIN_WAKEUP = 21;
        const uint BL_OBJ_TYPE_MOST_SPY = 22;
        const uint BL_OBJ_TYPE_MOST_CTRL = 23;
        const uint BL_OBJ_TYPE_MOST_LIGHTLOCK = 24;
        const uint BL_OBJ_TYPE_MOST_STATISTIC = 25;
        const uint BL_OBJ_TYPE_reserved_1 = 26;
        const uint BL_OBJ_TYPE_reserved_2 = 27;
        const uint BL_OBJ_TYPE_reserved_3 = 28;
        const uint BL_OBJ_TYPE_FLEXRAY_DATA = 29;
        const uint BL_OBJ_TYPE_FLEXRAY_SYNC = 30;
        const uint BL_OBJ_TYPE_CAN_DRIVER_ERROR = 31;
        const uint BL_OBJ_TYPE_MOST_PKT = 32;
        const uint BL_OBJ_TYPE_MOST_PKT2 = 33;
        const uint BL_OBJ_TYPE_MOST_HWMODE = 34;
        const uint BL_OBJ_TYPE_MOST_REG = 35;
        const uint BL_OBJ_TYPE_MOST_GENREG = 36;
        const uint BL_OBJ_TYPE_MOST_NETSTATE = 37;
        const uint BL_OBJ_TYPE_MOST_DATALOST = 38;
        const uint BL_OBJ_TYPE_MOST_TRIGGER = 39;
        const uint BL_OBJ_TYPE_FLEXRAY_CYCLE = 40;
        const uint BL_OBJ_TYPE_FLEXRAY_MESSAGE = 41;
        const uint BL_OBJ_TYPE_LIN_CHECKSUM_INFO = 42;
        const uint BL_OBJ_TYPE_LIN_SPIKE_EVENT = 43;
        const uint BL_OBJ_TYPE_CAN_DRIVER_SYNC = 44;
        const uint BL_OBJ_TYPE_FLEXRAY_STATUS = 45;
        const uint BL_OBJ_TYPE_GPS_EVENT = 46;
        const uint BL_OBJ_TYPE_FR_ERROR = 47;
        const uint BL_OBJ_TYPE_FR_STATUS = 48;
        const uint BL_OBJ_TYPE_FR_STARTCYCLE = 49;
        const uint BL_OBJ_TYPE_FR_RCVMESSAGE = 50;
        const uint BL_OBJ_TYPE_REALTIMECLOCK = 51;
        const uint BL_OBJ_TYPE_AVAILABLE2 = 52;
        const uint BL_OBJ_TYPE_AVAILABLE3 = 53;
        const uint BL_OBJ_TYPE_LIN_STATISTIC = 54;
        const uint BL_OBJ_TYPE_J1708_MESSAGE = 55;
        const uint BL_OBJ_TYPE_J1708_VIRTUAL_MSG = 56;
        const uint BL_OBJ_TYPE_LIN_MESSAGE2 = 57;
        const uint BL_OBJ_TYPE_LIN_SND_ERROR2 = 58;
        const uint BL_OBJ_TYPE_LIN_SYN_ERROR2 = 59;
        const uint BL_OBJ_TYPE_LIN_CRC_ERROR2 = 60;
        const uint BL_OBJ_TYPE_LIN_RCV_ERROR2 = 61;
        const uint BL_OBJ_TYPE_LIN_WAKEUP2 = 62;
        const uint BL_OBJ_TYPE_LIN_SPIKE_EVENT2 = 63;
        const uint BL_OBJ_TYPE_LIN_LONG_DOM_SIG = 64;
        const uint BL_OBJ_TYPE_APP_TEXT = 65;
        const uint BL_OBJ_TYPE_FR_RCVMESSAGE_EX = 66;
        const uint BL_OBJ_TYPE_MOST_STATISTICEX = 67;
        const uint BL_OBJ_TYPE_MOST_TXLIGHT = 68;
        const uint BL_OBJ_TYPE_MOST_ALLOCTAB = 69;
        const uint BL_OBJ_TYPE_MOST_STRESS = 70;
        const uint BL_OBJ_TYPE_ETHERNET_FRAME = 71;
        const uint BL_OBJ_TYPE_SYS_VARIABLE = 72;
        const uint BL_OBJ_TYPE_CAN_ERROR_EXT = 73;
        const uint BL_OBJ_TYPE_CAN_DRIVER_ERROR_EXT = 74;
        const uint BL_OBJ_TYPE_LIN_LONG_DOM_SIG2 = 75;
        const uint BL_OBJ_TYPE_MOST_150_MESSAGE = 76;
        const uint BL_OBJ_TYPE_MOST_150_PKT = 77;
        const uint BL_OBJ_TYPE_MOST_ETHERNET_PKT = 78;
        const uint BL_OBJ_TYPE_MOST_150_MESSAGE_FRAGMENT = 79;
        const uint BL_OBJ_TYPE_MOST_150_PKT_FRAGMENT = 80;
        const uint BL_OBJ_TYPE_MOST_ETHERNET_PKT_FRAGMENT = 81;
        const uint BL_OBJ_TYPE_MOST_SYSTEM_EVENT = 82;
        const uint BL_OBJ_TYPE_MOST_150_ALLOCTAB = 83;
        const uint BL_OBJ_TYPE_MOST_50_MESSAGE = 84;
        const uint BL_OBJ_TYPE_MOST_50_PKT = 85;
        const uint BL_OBJ_TYPE_CAN_MESSAGE2 = 86;
        const uint BL_OBJ_TYPE_LIN_UNEXPECTED_WAKEUP = 87;
        const uint BL_OBJ_TYPE_LIN_SHORT_OR_SLOW_RESPONSE = 88;
        const uint BL_OBJ_TYPE_LIN_DISTURBANCE_EVENT = 89;
        const uint BL_OBJ_TYPE_SERIAL_EVENT = 90;
        const uint BL_OBJ_TYPE_OVERRUN_ERROR = 91;
        const uint BL_OBJ_TYPE_EVENT_COMMENT = 92;
        const uint BL_OBJ_TYPE_WLAN_FRAME = 93;
        const uint BL_OBJ_TYPE_WLAN_STATISTIC = 94;
        const uint BL_OBJ_TYPE_MOST_ECL = 95;
        const uint BL_OBJ_TYPE_GLOBAL_MARKER = 96;
        const uint BL_OBJ_TYPE_AFDX_FRAME = 97;
        const uint BL_OBJ_TYPE_AFDX_STATISTIC = 98;
        const uint BL_OBJ_TYPE_KLINE_STATUSEVENT = 99;
        const uint BL_OBJ_TYPE_CAN_FD_MESSAGE = 100;
        const uint BL_OBJ_TYPE_CAN_FD_MESSAGE_64 = 101;
        const uint BL_OBJ_TYPE_ETHERNET_RX_ERROR = 102;
        const uint BL_OBJ_TYPE_ETHERNET_STATUS = 103;
        const uint BL_OBJ_TYPE_CAN_FD_ERROR_64 = 104;
        const uint BL_OBJ_TYPE_LIN_SHORT_OR_SLOW_RESPONSE2 = 105;
        const uint BL_OBJ_TYPE_AFDX_STATUS = 106;
        const uint BL_OBJ_TYPE_AFDX_BUS_STATISTIC = 107;
        const uint BL_OBJ_TYPE_reserved_4 = 108;
        const uint BL_OBJ_TYPE_AFDX_ERROR_EVENT = 109;
        const uint BL_OBJ_TYPE_A429_ERROR = 110;
        const uint BL_OBJ_TYPE_A429_STATUS = 111;
        const uint BL_OBJ_TYPE_A429_BUS_STATISTIC = 112;
        const uint BL_OBJ_TYPE_A429_MESSAGE = 113;
        const uint BL_OBJ_TYPE_ETHERNET_STATISTIC = 114;
        const uint BL_OBJ_TYPE_reserved_5 = 115;
        const uint BL_OBJ_TYPE_reserved_6 = 116;
        const uint BL_OBJ_TYPE_reserved_7 = 117;
        const uint BL_OBJ_TYPE_TEST_STRUCTURE = 118;
        const uint BL_OBJ_TYPE_DIAG_REQUEST_INTERPRETATION = 119;
        const uint BL_OBJ_TYPE_ETHERNET_FRAME_EX = 120;
        const uint BL_OBJ_TYPE_ETHERNET_FRAME_FORWARDED = 121;
        const uint BL_OBJ_TYPE_ETHERNET_ERROR_EX = 122;
        const uint BL_OBJ_TYPE_ETHERNET_ERROR_FORWARDED = 123;
        private const string ENV_DATA1 = "01234567";
        private const string ENV_DATA2 = "76543210";
        #endregion
        string[] Mon = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec" };
        string[] Week = { "Mon", "Tue", "Wed", "Thur", "Fri", "Sat", "Sun" };
        string ENV_NAME1 = "EnvString";
        string ENV_NAME2 = "EnvInt";
        public int WriteBlf(IntPtr pFileName, UInt32 pWritten)
        {
            IntPtr hFile;
            SYSTEMTIME systemTime;
            bool bSuccess;
            UInt64 i;
            VBLAppTrigger appTrigger;
            VBLCANMessage message;
            VBLEnvironmentVariable variable_s;
            VBLEnvironmentVariable variable_i;
            VBLEthernetFrame ethframe;

            byte[] src = { 0, 1, 2, 3, 4, 5 };
            byte[] dst = { 6, 7, 8, 9, 10, 11 };
            byte[] ethbuffer = new byte[1500];
            VBLAppText appText;
            UInt64 time;

            pWritten = 0;

            /* open file */

            hFile = BINLOG.BLCreateFile(pFileName, GENERIC_WRITE);

            //if (INVALID_HANDLE_VALUE == hFile)
            //{
            //    return -1;
            //}

            /* set applicaton information */
            /* bSuccess = BLSetApplication( hFile, BL_APPID_UNKNOWN, 1, 0, 0); */
            bSuccess = BINLOG.BLSetApplication(hFile, 6, 1, 0, 1);

            //GetSystemTime(&systemTime);
            systemTime = new SYSTEMTIME();

            bSuccess = bSuccess && BINLOG.BLSetMeasurementStartTime(hFile, systemTime);

            /* set write options */
            bSuccess = bSuccess && BINLOG.BLSetWriteOptions(hFile, 6, 0);

            if (bSuccess)
            {
                // initialize all members to zero (including reserved space)
                //memset(&appTrigger, 0, sizeof(VBLAppTrigger));
                //memset(&message, 0, sizeof(VBLCANMessage));
                //memset(&variable_s, 0, sizeof(VBLEnvironmentVariable));
                //memset(&variable_i, 0, sizeof(VBLEnvironmentVariable));
                //memset(&ethframe, 0, sizeof(VBLEthernetFrame));
                appTrigger = new VBLAppTrigger();
                appTrigger.mHeader = new VBLObjectHeader();
                //appTrigger.mHeader.mBase = new VBLObjectHeaderBase();

                message = new VBLCANMessage();
                message.mHeader = new VBLObjectHeader();
                message.mHeader.mBase = new VBLObjectHeaderBase();

                variable_s = new VBLEnvironmentVariable();
                variable_s.mHeader = new VBLObjectHeader();
                variable_s.mHeader.mBase = new VBLObjectHeaderBase();

                variable_i = new VBLEnvironmentVariable();
                variable_i.mHeader = new VBLObjectHeader();
                variable_i.mHeader.mBase = new VBLObjectHeaderBase();

                ethframe = new VBLEthernetFrame();
                ethframe.mHeader = new VBLObjectHeader();
                ethframe.mHeader.mBase = new VBLObjectHeaderBase();

                appText = new VBLAppText();
                appText.mHeader = new VBLObjectHeader();
                appText.mHeader.mBase = new VBLObjectHeaderBase();

                /* setup object headers */
                appTrigger.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                appTrigger.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(appTrigger.mHeader);
                appTrigger.mHeader.mBase.mHeaderVersion = 1;
                appTrigger.mHeader.mBase.mObjectSize = (uint)Marshal.SizeOf(appTrigger);
                appTrigger.mHeader.mBase.mObjectType = BL_OBJ_TYPE_APP_TRIGGER;
                appTrigger.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                message.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                message.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(message.mHeader);
                message.mHeader.mBase.mHeaderVersion = 1;
                message.mHeader.mBase.mObjectSize = (uint)Marshal.SizeOf(message);
                message.mHeader.mBase.mObjectType = BL_OBJ_TYPE_CAN_MESSAGE;
                message.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                variable_s.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                variable_s.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(variable_s.mHeader);
                variable_s.mHeader.mBase.mHeaderVersion = 1;
                variable_s.mHeader.mBase.mObjectType = BL_OBJ_TYPE_ENV_STRING;
                variable_s.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                variable_i.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                variable_i.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(variable_i.mHeader);
                variable_i.mHeader.mBase.mHeaderVersion = 1;
                variable_i.mHeader.mBase.mObjectType = BL_OBJ_TYPE_ENV_INTEGER;
                variable_i.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                ethframe.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                ethframe.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(ethframe.mHeader);
                ethframe.mHeader.mBase.mHeaderVersion = 1;
                ethframe.mHeader.mBase.mObjectType = BL_OBJ_TYPE_ETHERNET_FRAME;
                ethframe.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                appText.mHeader.mBase.mSignature = BL_OBJ_SIGNATURE;
                appText.mHeader.mBase.mHeaderSize = (UInt16)Marshal.SizeOf(appText.mHeader);
                appText.mHeader.mBase.mHeaderVersion = 1;
                appText.mHeader.mBase.mObjectType = BL_OBJ_TYPE_APP_TEXT;
                appText.mHeader.mObjectFlags = BL_OBJ_FLAG_TIME_ONE_NANS;

                for (i = 0; i < 1000; ++i)
                {
                    ethbuffer[i] = (byte)i;
                }

                for (i = 0; i < 1000 && bSuccess; ++i)
                {
                    /* increment in milliseconds */
                    time = (ulong)i * 10000000;

                    /* setup app trigger object header */
                    appTrigger.mHeader.mObjectTimeStamp = time;

                    /* write app trigger object */
                    bSuccess = BINLOG.BLWriteObject(hFile, ref appTrigger.mHeader.mBase);
                    pWritten += (uint)(bSuccess ? 1 : 0);

                    /* setup CAN object header */
                    message.mHeader.mObjectTimeStamp = time;

                    /* setup CAN message */
                    message.mChannel = 1;
                    message.mFlags = 0;
                    message.mDLC = 8;
                    message.mID = 0x100;
                    if (i % 2 == 1)
                    {
                        message.mData = Encoding.ASCII.GetBytes("01234567");
                    }
                    else
                    {
                        message.mData = Encoding.ASCII.GetBytes("76543210");
                    }


                    /* write CAN message */
                    bSuccess = BINLOG.BLWriteObject(hFile, ref message.mHeader.mBase);
                    pWritten += (uint)(bSuccess ? 1 : 0);

                    if (0 == (i % 3) && bSuccess)
                    {
                        /* setup environment variable object headers */
                        variable_s.mHeader.mObjectTimeStamp = time;
                        variable_i.mHeader.mObjectTimeStamp = time;

                        /* setup environment variables */
                        variable_s.mNameLength = (uint)ENV_NAME1.Length;
                        variable_s.mDataLength = (uint)(i % 2 == 1 ? ENV_DATA1.Length : ENV_DATA2.Length);
                        variable_s.mName = Marshal.StringToHGlobalAnsi(ENV_NAME1);

                        if (i % 2 == 1)
                        {
                            variable_s.mData = Marshal.StringToHGlobalAnsi(ENV_DATA1);

                        }
                        else
                        {
                            variable_s.mData = Marshal.StringToHGlobalAnsi(ENV_DATA1);
                        }

                        variable_s.mHeader.mBase.mObjectSize = (uint)(Marshal.SizeOf(variable_s) + variable_s.mNameLength + variable_s.mDataLength);

                        variable_i.mNameLength = (uint)ENV_NAME2.Length;
                        variable_i.mDataLength = 4;
                        variable_i.mName = Marshal.StringToHGlobalAnsi(ENV_NAME2);
                        variable_i.mData = Marshal.StringToHGlobalAnsi(i.ToString());
                        variable_i.mHeader.mBase.mObjectSize = (uint)(Marshal.SizeOf(variable_i) + variable_i.mNameLength + variable_i.mDataLength);

                        /* write environment variables */
                        bSuccess = BINLOG.BLWriteObject(hFile, ref variable_s.mHeader.mBase);
                        pWritten += (uint)(bSuccess ? 1 : 0);

                        bSuccess = bSuccess && BINLOG.BLWriteObject(hFile, ref variable_i.mHeader.mBase);
                        pWritten += (uint)(bSuccess ? 1 : 0);

                        /* write ethernet frame */
                        ethframe.mHeader.mObjectTimeStamp = time;
                        ethframe.mSourceAddress = new byte[6];
                        Array.Copy(src, ethframe.mSourceAddress, ethframe.mSourceAddress.Length);
                        ethframe.mChannel = 0;
                        ethframe.mDestinationAddress = new byte[6];
                        Array.Copy(dst, ethframe.mDestinationAddress, ethframe.mDestinationAddress.Length);
                        ethframe.mDir = 0;
                        ethframe.mType = 0x0800;
                        ethframe.mTPID = 0;
                        ethframe.mTCI = 0;
                        ethframe.mPayLoadLength = (UInt16)i;


                        IntPtr pRawData = Marshal.AllocHGlobal(1500); //分配非托管内存空间
                        Marshal.Copy(ethbuffer, 0, pRawData, 1500);//将非托管内存指针数据复制到数组中

                        ethframe.mPayLoad = pRawData;
                        Marshal.FreeHGlobal(pRawData);//释放内存
                        ethframe.mHeader.mBase.mObjectSize = (uint)(Marshal.SizeOf(ethframe) + ethframe.mPayLoadLength);

                        bSuccess = bSuccess && BINLOG.BLWriteObject(hFile, ref ethframe.mHeader.mBase);
                        pWritten += (uint)(bSuccess ? 1 : 0);

                        /* write text */
                        if ((i % 100) == 0)
                        {
                            //char[] text = new char[128];

                            //sprintf(text, "%d objects written...", *pWritten);
                            string text = string.Format("{0} objects written...", pWritten);
                            appText.mText = Marshal.StringToCoTaskMemAuto(text);
                            appText.mTextLength = (uint)(text.Length);
                            appText.mHeader.mBase.mObjectSize = (uint)(Marshal.SizeOf(appText) + appText.mTextLength);
                            appText.mHeader.mObjectTimeStamp = time;

                            bSuccess = bSuccess && BINLOG.BLWriteObject(hFile, ref appText.mHeader.mBase);
                            pWritten += (uint)(bSuccess ? 1 : 0);
                        }
                    }
                }
                string text2 = "All objects written...";
                appText.mText = Marshal.StringToHGlobalAnsi(text2);
                appText.mTextLength = (uint)(text2.Length);
                appText.mHeader.mBase.mObjectSize = (uint)(Marshal.SizeOf(appText) + appText.mTextLength);

                bSuccess = bSuccess && BINLOG.BLWriteObject(hFile, ref appText.mHeader.mBase);
                pWritten += (uint)(bSuccess ? 1 : 0);
            }

            /* close file */
            if (!BINLOG.BLCloseHandle(hFile))
            {
                return -1;
            }
            return bSuccess ? 0 : -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="outPuth"></param>
        /// <returns></returns>
        public int ReadBlfFile(IntPtr pFileName, string outPuth)
        {
            IntPtr hFile;
            UInt32 pRead;
            VBLObjectHeaderBase headerbase = new VBLObjectHeaderBase();
            VBLCANMessage message = new VBLCANMessage();
            message.mData = new byte[8];
            VBLCANFDMessage message64 = new VBLCANFDMessage();
            message64.mData = new byte[84];
            VBLEnvironmentVariable variable = new VBLEnvironmentVariable();
            VBLEthernetFrame ethframe = new VBLEthernetFrame();
            ethframe.mSourceAddress = new byte[6];
            ethframe.mDestinationAddress = new byte[6];
            VBLAppText appText = new VBLAppText();
            VBLFileStatisticsEx statistics = new VBLFileStatisticsEx();
            statistics.mReserved = new UInt32[18];
            bool bSuccess;
            pRead = 0;
            /* open file */
            hFile = BINLOG.BLCreateFile(pFileName, GENERIC_READ);
            IntPtr ptstatistics = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLFileStatisticsEx)));
            IntPtr ptmessage64 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLCANFDMessage)));
            IntPtr ptmessage = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLCANMessage)));
            IntPtr ptvariable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLEnvironmentVariable)));
            IntPtr ptethframe = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLEthernetFrame)));
            IntPtr ptappText = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLAppText)));
            BINLOG.BLGetFileStatisticsEx(hFile, ptstatistics);
            statistics = (VBLFileStatisticsEx)Marshal.PtrToStructure(ptstatistics, typeof(VBLFileStatisticsEx));
            //创建文件
            FileStream fs = new FileStream(outPuth, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            string writeData;
            try
            {
                writeData = "date " + Mon[statistics.mMeasurementStartTime.wMonth - 1] + " "
                   + Week[statistics.mMeasurementStartTime.wDayOfWeek - 1]
                   + " " + statistics.mMeasurementStartTime.wDay
                   + " " + statistics.mMeasurementStartTime.wHour
                   + ":" + statistics.mMeasurementStartTime.wMinute
                   + ":" + statistics.mMeasurementStartTime.wSecond
                   + "." + statistics.mMeasurementStartTime.wMilliseconds
                   + "\nbase hex  timestamps absolute\ninternal events logged\n// version N/A";
            }
            catch (Exception)
            {
            }
            finally
            {
                writeData = "date " + "Jan" + " "
               + "Mon"
               + " " + statistics.mMeasurementStartTime.wDay
               + " " + statistics.mMeasurementStartTime.wHour
               + ":" + statistics.mMeasurementStartTime.wMinute
               + ":" + statistics.mMeasurementStartTime.wSecond
               + "." + statistics.mMeasurementStartTime.wMilliseconds
               + "\nbase hex  timestamps absolute\ninternal events logged\n// version N/A";
            }
            sw.WriteLine(writeData);
            bSuccess = true;
            /* read base object header from file */
            while (bSuccess && BINLOG.BLPeekObject(hFile, ref headerbase))
            {
                switch (headerbase.mObjectType)
                {
                    case BL_OBJ_TYPE_CAN_FD_MESSAGE_64:
                        message64.mHeader.mBase = headerbase;

                        Marshal.StructureToPtr(message64, ptmessage64, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptmessage64, (ulong)Marshal.SizeOf(message64));
                        message64 = (VBLCANFDMessage)Marshal.PtrToStructure(ptmessage64, typeof(VBLCANFDMessage));
                        if (bSuccess)
                        {
                            string time = (message64.mHeader.mObjectTimeStamp / 1000000000.0).ToString("F8");//时间戳
                            string type = message64.mFlags == 8 ? "" : "CANFD";
                            string flag = message64.mHeader.mObjectFlags.ToString();
                            string id = Convert.ToString(message64.mID, 16);
                            //switch (message64.mFlags)
                            //{
                            //    case 32:
                            //        type3 = "1 0 d" + "  ";
                            //        break;
                            //    case 64:
                            //        type3 = "1 0 f" + "  ";
                            //        break;
                            //    default:
                            //        type3 = "1 0 8" + "  ";
                            //        break;
                            //} //Rx 0 0 d 15 64
                            string mflag = message64.mFlags.ToString();
                            Byte[] data2 = new Byte[message64.mFlags];
                            Buffer.BlockCopy(message64.mData, 20, data2, 0, message64.mFlags);
                            sw.WriteLine($"{time} CANFD {message64.mHeader.mObjectFlags} {message64.mID.ToString("X")} Rx 0 0 d {DataConverter.DataLen2DLC(message64.mFlags)} {message64.mFlags} {BitConverter.ToString(data2, 0).Replace("-", " ")}");
                            //string sss = string.Format("{0:0.00000000} {1} {2} {3} Rx 1 0 d {4} {5} {6}", time, type, flag,
                            //    id, mflag, mflag, BitConverter.ToString(data2, 0).Replace("-", " "));
                            //sw.WriteLine(sss);
                            BINLOG.BLFreeObject(hFile, ref message64.mHeader.mBase);
                        }

                        break;
                    case BL_OBJ_TYPE_CAN_MESSAGE:
                        /* read CAN message */
                        message.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(message, ptmessage, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptmessage, (ulong)Marshal.SizeOf(message));
                        message = (VBLCANMessage)Marshal.PtrToStructure(ptmessage, typeof(VBLCANMessage));
                        /* free memory for the CAN message */
                        if (bSuccess)
                        {
                            string time = (message.mHeader.mObjectTimeStamp / 1000000000.0).ToString("F8");//时间戳
                            Byte[] data2 = new Byte[message.mDLC];
                            Buffer.BlockCopy(message.mData, 0, data2, 0, message.mDLC);
                            sw.WriteLine($"{time} {message.mChannel} {message.mID.ToString("X")} Rx d {message.mDLC} {BitConverter.ToString(data2, 0).Replace("-", " ")}");
                            BINLOG.BLFreeObject(hFile, ref message.mHeader.mBase);
                        }
                        break;
                    case BL_OBJ_TYPE_ENV_INTEGER:
                    case BL_OBJ_TYPE_ENV_DOUBLE:
                    case BL_OBJ_TYPE_ENV_STRING:
                    case BL_OBJ_TYPE_ENV_DATA:
                        /* read environment variable */
                        variable.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(variable, ptvariable, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptvariable, (ulong)Marshal.SizeOf(variable));
                        variable = (VBLEnvironmentVariable)Marshal.PtrToStructure(ptvariable, typeof(VBLEnvironmentVariable));
                        /* free memory for the environment variable */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref variable.mHeader.mBase);
                        }
                        break;
                    case BL_OBJ_TYPE_ETHERNET_FRAME:
                        /* read ethernet frame */
                        ethframe.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(ethframe, ptethframe, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptethframe, (ulong)Marshal.SizeOf(ethframe));
                        ethframe = (VBLEthernetFrame)Marshal.PtrToStructure(ptethframe, typeof(VBLEthernetFrame));
                        /* free memory for the frame */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref ethframe.mHeader.mBase);
                        }
                        break;
                    case BL_OBJ_TYPE_APP_TEXT:
                        /* read text */
                        appText.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(appText, ptappText, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptappText, (ulong)Marshal.SizeOf(appText));
                        appText = (VBLAppText)Marshal.PtrToStructure(ptappText, typeof(VBLAppText));
                        if (null != appText.mText)
                        {
                            Console.WriteLine(appText.mText);
                        }
                        /* free memory for the text */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref appText.mHeader.mBase);
                        }
                        break;
                    default:
                        /* skip all other objects */
                        bSuccess = BINLOG.BLSkipObject(hFile, ref headerbase);
                        break;
                }
                if (bSuccess)
                {
                    pRead += 1;
                }
            }

            Marshal.FreeHGlobal(ptstatistics);
            Marshal.FreeHGlobal(ptmessage64);
            Marshal.FreeHGlobal(ptmessage);
            Marshal.FreeHGlobal(ptvariable);
            Marshal.FreeHGlobal(ptethframe);
            Marshal.FreeHGlobal(ptappText);
            //End Triggerblock
            sw.WriteLine("End Triggerblock");
            //清空缓冲区  
            sw.Flush();
            //关闭流  
            sw.Close();
            fs.Close();
            /* close file */
            if (!BINLOG.BLCloseHandle(hFile))
            {
                return -1;
            }
            return bSuccess ? 0 : -1;
        }

        /******************************************************************************
        *                                                                             *
        * skip all elements of an BL file                                             *
        *                                                                             *
        ******************************************************************************/
        public int SkipTest(IntPtr pFileName, UInt32 pRead)
        {
            IntPtr hFile;
            VBLObjectHeaderBase headerbase = new VBLObjectHeaderBase();
            VBLFileStatisticsEx statistics = new VBLFileStatisticsEx();
            statistics.mReserved = new UInt32[18];
            bool bSuccess;

            //if (NULL == pRead)
            //{
            //    return -1;
            //}

            pRead = 0;

            /* open file */
            hFile = BINLOG.BLCreateFile(pFileName, GENERIC_READ);

            //if (INVALID_HANDLE_VALUE == hFile)
            //{
            //    return -1;
            //}

            //BINLOG.BLGetFileStatisticsEx(hFile, ref statistics);

            bSuccess = true;

            /* read base object header from file */
            while (bSuccess && BINLOG.BLPeekObject(hFile, ref headerbase))
            {
                bSuccess = BINLOG.BLSkipObject(hFile, ref headerbase);

                if (bSuccess)
                {
                    pRead += 1;
                }
            }

            /* close file */
            if (!BINLOG.BLCloseHandle(hFile))
            {
                return -1;
            }

            //printf("The skip test was: %s\n", bSuccess ? "successful" : "not successful");

            return bSuccess ? 0 : -1;
        }

        public int Deal()
        {
            string FileName = "D:\\tang\\demo-data\\zcanpro-config\\Trace1.blf";
            UInt32 dwWritten = 0;
            UInt32 dwRead = 0;
            UInt32 dwSkip = 0;
            IntPtr pFileName = Marshal.StringToHGlobalAnsi(FileName);
            int result = 0;

            result = WriteBlf(pFileName, dwWritten);

            if (0 != result)
            {
                return result;
            }

            result = ReadBlfFile(pFileName, "");

            if (0 != result)
            {
                return result;
            }

            result = SkipTest(pFileName, dwSkip);

            if (0 != result)
            {
                return result;
            }

            return ((dwWritten + 1) == dwRead && dwRead == dwSkip) ? 0 : -1;
        }

        public int Parse(string filename)
        {
            string outPuth = filename.Replace(".blf", ".asc");
            IntPtr pFileName = Marshal.StringToHGlobalAnsi(filename);
            int result = ReadBlfFile(pFileName, outPuth);
            return result;
        }
        public ConcurrentDictionary<ulong, List<Frame>> ParseBlfFile(string blfName)
        {
            IntPtr hFile;
            UInt32 pRead;
            IntPtr pFileName = Marshal.StringToHGlobalAnsi(blfName);
            if (IntPtr.Size == 4)
            {
                blfName = "";
            }
            else
            {
                blfName = "";
            }
            VBLObjectHeaderBase headerbase = new VBLObjectHeaderBase();
            VBLCANMessage message = new VBLCANMessage();
            message.mData = new byte[8];
            VBLCANFDMessage message64 = new VBLCANFDMessage();
            message64.mData = new byte[84];
            VBLEnvironmentVariable variable = new VBLEnvironmentVariable();
            VBLEthernetFrame ethframe = new VBLEthernetFrame();
            ethframe.mSourceAddress = new byte[6];
            ethframe.mDestinationAddress = new byte[6];
            VBLAppText appText = new VBLAppText();
            VBLFileStatisticsEx statistics = new VBLFileStatisticsEx();
            statistics.mReserved = new UInt32[18];
            bool bSuccess;
            pRead = 0;
            /* open file */
            hFile = BINLOG.BLCreateFile(pFileName, GENERIC_READ);
            IntPtr ptstatistics = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLFileStatisticsEx)));
            IntPtr ptmessage64 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLCANFDMessage)));
            IntPtr ptmessage = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLCANMessage)));
            IntPtr ptvariable = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLEnvironmentVariable)));
            IntPtr ptethframe = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLEthernetFrame)));
            IntPtr ptappText = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VBLAppText)));
            BINLOG.BLGetFileStatisticsEx(hFile, ptstatistics);
            statistics = (VBLFileStatisticsEx)Marshal.PtrToStructure(ptstatistics, typeof(VBLFileStatisticsEx));
            ConcurrentDictionary<ulong, List<Frame>> Blfs = new ConcurrentDictionary<ulong, List<Frame>>();
            bSuccess = true;
            UInt64 firstTimeStamp = 0;
            /* read base object header from file */
            while (bSuccess && BINLOG.BLPeekObject(hFile, ref headerbase))
            {
                switch (headerbase.mObjectType)
                {
                    case BL_OBJ_TYPE_CAN_FD_MESSAGE_64:
                        message64.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(message64, ptmessage64, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptmessage64, (ulong)Marshal.SizeOf(message64));
                        message64 = (VBLCANFDMessage)Marshal.PtrToStructure(ptmessage64, typeof(VBLCANFDMessage));
                        if (bSuccess)
                        {
                            if (firstTimeStamp == 0) 
                            {
                                firstTimeStamp = message64.mHeader.mObjectTimeStamp;
                            }
                            var frame = new Frame();
                            frame.CanType = "CANFD";
                            frame.DLC = message64.mFlags;
                            frame.ID = message64.mID;
                            frame.Channel = message64.mHeader.mObjectFlags;
                            frame.Data = new byte[frame.DLC];
                            ulong KEY = (message64.mHeader.mObjectTimeStamp - firstTimeStamp) / 1000000;
                            frame.TimeStmp = 1.0* KEY / 1000.0;
                            Buffer.BlockCopy(message64.mData, 20, frame.Data, 0, frame.DLC);
                            frame.DataHex = BitConverter.ToString(frame.Data).Replace("-", " ");
                            BINLOG.BLFreeObject(hFile, ref message64.mHeader.mBase);
                            if (!Blfs.ContainsKey(KEY))
                            {
                                Blfs.TryAdd(KEY,new List<Frame>());
                            }
                            if (Blfs.TryGetValue(KEY,out List<Frame> frrames))
                            {
                                frrames.Add(frame);
                            }
                        }

                        break;
                    case BL_OBJ_TYPE_CAN_MESSAGE:
                        /* read CAN message */
                        message.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(message, ptmessage, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptmessage, (ulong)Marshal.SizeOf(message));
                        message = (VBLCANMessage)Marshal.PtrToStructure(ptmessage, typeof(VBLCANMessage));
                        if (bSuccess)
                        {
                            if (firstTimeStamp == 0)
                            {
                                firstTimeStamp = message.mHeader.mObjectTimeStamp;
                            }
                            var frame = new Frame();
                            
                            ulong KEY = (message.mHeader.mObjectTimeStamp - firstTimeStamp) / 1000000;
                            frame.TimeStmp = 1.0 * KEY / 1000.0;
                            frame.DLC = message.mDLC;
                            frame.ID = message.mID;
                            frame.Channel = message.mChannel;
                            frame.Data = new byte[8];
                            
                            Buffer.BlockCopy(message.mData, 0, frame.Data, 0, message.mDLC);
                            frame.DataHex = BitConverter.ToString(frame.Data).Replace("-"," ");

                            BINLOG.BLFreeObject(hFile, ref message.mHeader.mBase);
                            if (!Blfs.ContainsKey(KEY))
                            {
                                Blfs.TryAdd(KEY, new List<Frame>());
                            }
                            if (Blfs.TryGetValue(KEY, out List<Frame> frrames))
                            {
                                frrames.Add(frame);
                            }
                        }
                        break;
                    case BL_OBJ_TYPE_ENV_INTEGER:
                    case BL_OBJ_TYPE_ENV_DOUBLE:
                    case BL_OBJ_TYPE_ENV_STRING:
                    case BL_OBJ_TYPE_ENV_DATA:
                        /* read environment variable */
                        variable.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(variable, ptvariable, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptvariable, (ulong)Marshal.SizeOf(variable));
                        variable = (VBLEnvironmentVariable)Marshal.PtrToStructure(ptvariable, typeof(VBLEnvironmentVariable));
                        /* free memory for the environment variable */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref variable.mHeader.mBase);
                        }
                        break;
                    case BL_OBJ_TYPE_ETHERNET_FRAME:
                        /* read ethernet frame */
                        ethframe.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(ethframe, ptethframe, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptethframe, (ulong)Marshal.SizeOf(ethframe));
                        ethframe = (VBLEthernetFrame)Marshal.PtrToStructure(ptethframe, typeof(VBLEthernetFrame));
                        /* free memory for the frame */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref ethframe.mHeader.mBase);
                        }
                        break;
                    case BL_OBJ_TYPE_APP_TEXT:
                        /* read text */
                        appText.mHeader.mBase = headerbase;
                        Marshal.StructureToPtr(appText, ptappText, true);
                        bSuccess = BINLOG.BLReadObjectSecure(hFile, ptappText, (ulong)Marshal.SizeOf(appText));
                        appText = (VBLAppText)Marshal.PtrToStructure(ptappText, typeof(VBLAppText));
                        if (null != appText.mText)
                        {
                            Console.WriteLine(appText.mText);
                        }
                        /* free memory for the text */
                        if (bSuccess)
                        {
                            BINLOG.BLFreeObject(hFile, ref appText.mHeader.mBase);
                        }
                        break;
                    default:
                        /* skip all other objects */
                        bSuccess = BINLOG.BLSkipObject(hFile, ref headerbase);
                        break;
                }
                if (bSuccess)
                {
                    pRead += 1;
                }
            }

            Marshal.FreeHGlobal(ptstatistics);
            Marshal.FreeHGlobal(ptmessage64);
            Marshal.FreeHGlobal(ptmessage);
            Marshal.FreeHGlobal(ptvariable);
            Marshal.FreeHGlobal(ptethframe);
            Marshal.FreeHGlobal(ptappText);
            Marshal.FreeHGlobal(pFileName);
            if (!BINLOG.BLCloseHandle(hFile))
            {
                return default;
            }
            return Blfs;
        }
    }


    public class BINLOG
    {
        public string DllPath = "binlog.dll";
        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLCreateFile")]
        public static extern IntPtr BLCreateFile(IntPtr inPath, ulong outPath);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLSetApplication")]
        public static extern bool BLSetApplication(IntPtr hFile, byte appID, byte appMajor, byte appMinor, byte appBuild);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLSetMeasurementStartTime")]
        public static extern bool BLSetMeasurementStartTime(IntPtr hFile, SYSTEMTIME lpStartTime);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLSetWriteOptions")]
        public static extern bool BLSetWriteOptions(IntPtr hFile, UInt32 dwCompression, UInt32 dwFlags);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLWriteObject")]
        public static extern bool BLWriteObject(IntPtr hFile, ref VBLObjectHeaderBase pBase);//VBLObjectHeaderBase

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLCloseHandle")]
        public static extern bool BLCloseHandle(IntPtr hFile);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLGetFileStatisticsEx")]
        public static extern bool BLGetFileStatisticsEx(IntPtr hFile, IntPtr pStatistics);//VBLFileStatisticsEx


        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLPeekObject")]
        public static extern bool BLPeekObject(IntPtr hFile, ref VBLObjectHeaderBase pBase);


        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLReadObjectSecure")]
        public static extern bool BLReadObjectSecure(IntPtr hFile, IntPtr pBase, ulong expectedSize);// VBLObjectHeaderBase

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLFreeObject")]
        public static extern bool BLFreeObject(IntPtr hFile, ref VBLObjectHeaderBase pBase);

        [DllImport(@"binlog.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl, EntryPoint = "BLSkipObject")]
        public static extern bool BLSkipObject(IntPtr hFile, ref VBLObjectHeaderBase pBase);
    }

    #region BINLOG - Struct
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLAppTrigger
    {
        public VBLObjectHeader mHeader;                     /* object header */
        public UInt64 mPreTriggerTime;             /* pre-trigger time */
        public UInt64 mPostTriggerTime;            /* post-trigger time */
        public UInt16 mChannel;                    /* channel of event which triggered (if any) */
        public UInt16 mFlags;                      /* trigger type (see above) */
        public UInt32 mAppSecific2;                /* app specific member 2 */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VBLCANMessage
    {
        public VBLObjectHeader mHeader;                     /* object header */
        public UInt16 mChannel;                    /* application channel */
        public byte mFlags;                      /* CAN dir & rtr */
        public byte mDLC;                        /* CAN dlc */
        public UInt32 mID;                         /* CAN ID */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] mData;                    /* CAN data */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLCANFDMessage
    {
        public VBLObjectHeader mHeader;                     /* object header */
        public UInt16 mChannel;                    /* application channel */
        public byte mFlags;                      /* CAN dir & rtr */
        public byte mDLC;                        /* CAN dlc */
        public UInt32 mID;                         /* CAN ID */
        public UInt32 mFrameLength;                /* message length in ns - without 3 inter frame space bits and by Rx-message also without 1 End-Of-Frame bit */
        public byte mArbBitCount;                /* bit count of arbitration phase */
        public byte mCANFDFlags;                 /* CAN FD flags */
        public byte mValidDataBytes;             /* Valid payload length of mData */
        public byte mReserved1;                  /* reserved */
        public UInt32 mReserved2;                  /* reserved */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 84)]
        public byte[] mData;                   /* CAN FD data */
    }
    ;

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLEnvironmentVariable
    {
        public VBLObjectHeader mHeader;                     /* object header - NOTE! set the object size to*/
        /* */
        /* mHeader.mObjectSize = sizeof( VBLEnvironmentVariable) + mNameLength + mDataLength; */
        /* */
        public uint mNameLength;                 /* length of variable name in bytes */
        public uint mDataLength;                 /* length of variable data in bytes */
        public IntPtr mName;                       /* variable name in MBCS */
        public IntPtr mData;                       /* variable data */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLAppText
    {
        public VBLObjectHeader mHeader;                     /* object header - NOTE! set the object size to*/
        /* */
        /* mHeader.mObjectSize = sizeof( VBLAppText) + mTextLength; */
        /* */
        public UInt32 mSource;                     /* source of text */
        public UInt32 mReserved;                   /* reserved */
        public UInt32 mTextLength;                 /* text length in bytes */
        public IntPtr mText;                       /* text in MBCS */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLEthernetFrame
    {
        public VBLObjectHeader mHeader;                     /* object header - NOTE! set the object size to*/
        /* mHeader.mObjectSize = sizeof( VBLEthernetFrame) + mPayLoadLength; */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] mSourceAddress;
        public UInt16 mChannel;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] mDestinationAddress;
        public UInt16 mDir;                        /* Direction flag: 0=Rx, 1=Tx, 2=TxRq */
        public UInt16 mType;
        public UInt16 mTPID;
        public UInt16 mTCI;
        public UInt16 mPayLoadLength;              /* Number of valid mPayLoad bytes */
        public IntPtr mPayLoad;                    /* Max 1582 (1600 packet length - 18 header) data bytes per frame  */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLObjectHeader
    {
        public VBLObjectHeaderBase mBase;                   /* base header object */
        public UInt32 mObjectFlags;            /* object flags */
        public UInt16 mClientIndex;            /* client index of send node */
        public UInt16 mObjectVersion;          /* object specific version */
        public UInt64 mObjectTimeStamp;        /* object timestamp */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct VBLObjectHeaderBase
    {
        public UInt32 mSignature;                        /* signature (BL_OBJ_SIGNATURE) */
        public UInt16 mHeaderSize;                       /* sizeof object header */
        public UInt16 mHeaderVersion;                    /* header version (1) */
        public UInt32 mObjectSize;                       /* object size */
        public UInt32 mObjectType;                       /* object type (BL_OBJ_TYPE_XXX) */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct VBLFileStatisticsEx
    {
        public UInt32 mStatisticsSize;                   /* sizeof (VBLFileStatisticsEx) */
        public byte mApplicationID;                    /* application ID */
        public byte mApplicationMajor;                 /* application major number */
        public byte mApplicationMinor;                 /* application minor number */
        public byte mApplicationBuild;                 /* application build number */
        public UInt64 mFileSize;                         /* file size in bytes */
        public UInt64 mUncompressedFileSize;             /* uncompressed file size in bytes */
        public UInt32 mObjectCount;                      /* number of objects */
        public UInt32 mObjectsRead;                      /* number of objects read */
        public SYSTEMTIME mMeasurementStartTime;             /* measurement start time */
        public SYSTEMTIME mLastObjectTime;                   /* last object time */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        public UInt32[] mReserved;                     /* reserved */
    }
    #endregion
}
