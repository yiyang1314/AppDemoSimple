using Rxbit.Brigade.CanDevices.Config;
using System;
using System.Collections.Generic;

namespace Rxbit.Brigade.CanDevices.IServices
{
    public interface ILiveService
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Init(ConfigBus configBus);
        /// <summary>
        /// 开启接收
        /// </summary>
        /// <param name="deviceIds">前端设备（雷达）ID 集合</param>
        void StartReceive(int deviceIds);
        /// <summary>
        /// 停止接收
        /// </summary>
        /// <param name="deviceIds">前端设备（雷达）ID 集合</param>
        void StopReceive(int deviceIds);
    }
}
