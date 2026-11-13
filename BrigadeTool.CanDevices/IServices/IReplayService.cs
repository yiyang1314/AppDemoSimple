using Rxbit.Brigade.CanDevices.Config;
using System;

namespace Rxbit.Brigade.CanDevices.IServices
{
    public interface IReplayService
    {
        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="directory">回放数据目录</param>
        /// <param name="configBus">包括 CAN，LVDS，车载以太网，摄像头等所有配置信息</param>
        /// <returns></returns>
        FrameTimeConfig Init(String directory, ConfigBus configBus);

        /// <summary>
        /// 根据时间戳，获取回放数据
        /// </summary>
        /// <param name="timestamp">相对时间戳</param>
        void GetFrame(UInt64 timestamp);

        void Start();
        void Stop();
    }
}
