using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Structs;
using System;
using System.Collections.Generic;

namespace Rxbit.Brigade.CanDevices.Services
{
    public interface ICanChannelService
    {
        Int64 Id { get; }
        DateTime BeginRecordDt { get; set; }
        ChannelConfig ChannelConfig { get; }
        byte CanType { get; set; }
        bool UseCanfd { get; set; }
        Boolean Open(ChannelConfig ccc);
        List<RXCanMessage> Read();
        Boolean Close();
        Boolean Send(UInt32 protocolId, Byte[] data);
        Boolean SendMuiltFrame(UInt32 protocolId, List<Frame> frames, int offset = 0, int bs = 0, bool IsContius = true);
        Boolean GetOnlineState(ChannelConfig cc);
        Boolean GetDeviceInfo(ChannelConfig cc,ref DeviceInfo device);
    }

    public static class CanTool
    {
        /// <summary>
        /// 获取 CAN FD 合法长度（自动向上取整到标准值）
        /// </summary>
        public static int GetCanFdValidLength(int len)
        {
            if (len <= 8) return len;
            if (len <= 12) return 12;
            if (len <= 16) return 16;
            if (len <= 20) return 20;
            if (len <= 24) return 24;
            if (len <= 32) return 32;
            if (len <= 48) return 48;
            if (len <= 64) return 64;
            throw new ArgumentException("CAN FD 最大 64 字节");
        }

        /// <summary>
        /// 根据长度获取正确 DLC
        /// </summary>
        public static byte GetCanFdDlc(int len)
        {
            if (len <= 8) return 8;

            switch (len)
            {
                case 12: return 9;
                case 16: return 10;
                case 20: return 11;
                case 24: return 12;
                case 32: return 13;
                case 48: return 14;
                case 64: return 15;
                default: return 0;
            }
        }
    }
}
