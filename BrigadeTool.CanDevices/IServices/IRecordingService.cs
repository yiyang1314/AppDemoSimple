using System;

namespace Rxbit.Brigade.CanDevices.IServices
{
    public interface IRecordingService
    {
        /// <summary>
        /// 开始录制数据
        /// </summary>
        Boolean StartRecording(String directory);

        /// <summary>
        /// 停止录制数据
        /// </summary>
        Boolean StopRecording();
    }
}
