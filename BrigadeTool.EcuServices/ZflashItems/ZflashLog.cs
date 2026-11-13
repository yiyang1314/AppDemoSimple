using Rxbit.Brigade.CanDevices.Converters;
using Rxbit.Brigade.EcuServices.Models;
using System;
using System.ComponentModel;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum EcuMessage
    {
        [Description("请求帧")]
        Request = 0,// 请求帧
        [Description("原始帧")]
        Source = 1, //原始帧
        [Description("积极响应")]
        Positive = 2, // 积极响应
        [Description("消极响应")]
        Negative = 3, // 消极响应
        [Description("信息")]
        Nonamal = 4, // 正常信息
        [Description("进度信息")]
        ProGress = 5, // 进度信息
        [Description("失败请求")]
        RequestFaild = 6,// 发送失败
        [Description("响应超时")]
        TimeOutFaild = 7,// 超时无响应
        [Description("流控帧")]
        FlowControlFrame = 8,// 超时无响应
    }

    public class ZflashLog : ViewModelBase
    {
        public string TimeStemp
        {
            get { return _TimeStemp; }
            set { Set(ref _TimeStemp, value); }
        }
        private string _TimeStemp;

        public string LogType
        {
            get { return type; }
            set { Set(ref type, value); }
        }
        private string type;

        public string Direction
        {
            get { return _Direction; }
            set { Set(ref _Direction, value); }
        }
        private string _Direction = "";

        public string FrameId
        {
            get { return _FrameId; }
            set { Set(ref _FrameId, value); }
        }
        private string _FrameId = "   ";

        public string Desc
        {
            get { return _desc; }
            set { Set(ref _desc, value); }
        }
        private string _desc = "";

        public string Len
        {
            get { return dlc == 0 ? "" : dlc.ToString(); }
            set { dlc = Convert.ToInt16(value); }
        }

        public int DLc
        {
            get { return dlc; }
            set { Set(ref dlc, value); }
        }
        private int dlc;

        private EcuMessage _Message = EcuMessage.Nonamal;
        public ZflashLog()
        {
        }
        public ZflashLog(EcuMessage message, string timestep, string desc, byte[] data, uint frameId, bool istx = true)
        {
            Message = message;
            TimeStemp = timestep;
            Desc = desc;
        }

        public ZflashLog(EcuMessage message, string timestep, byte[] data, uint frameId, bool istx = true)
        {
            Message = message;
            TimeStemp = timestep;
            if (message == EcuMessage.Source || message == EcuMessage.Request)
            {
                DLc = data.Length;
                Desc = BitConverter.ToString(data).Replace("-", " ");
                Direction = istx ? "Tx" : "Rx";
                FrameId = frameId.ToString("X3");
            }
            else if (message == EcuMessage.Positive)
            {
                DLc = data[0];
                Desc = BitConverter.ToString(data, 1, DLc).Replace("-", " ");
                Direction = "Rx";
                FrameId = frameId.ToString("X3");
            }
            else if (message == EcuMessage.Negative)
            {
                DLc = 3;
                Desc = BitConverter.ToString(data, 1, DLc).Replace("-", " ");
                Direction = "Rx";
                FrameId = frameId.ToString("X3");
            }
            string[] types = { "请求帧 ", "原始帧 ", "积极响应", "消极响应", "信息  " };
            LogType = types[((int)Message)];
        }

        public ZflashLog(string timestep, string desc)
        {
            Message = EcuMessage.Nonamal;
            TimeStemp = timestep;
            Desc = desc;
            LogType = "信息  ";
        }
        public EcuMessage Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public override string ToString()
        {
            if (type == "信息")
            {
                return string.Format("{0} {1} {2}", TimeStemp, type, Desc);
            }
            return string.Format("{0} {1} {2} {3} {4} {5}", TimeStemp, type, FrameId, Direction, DLc, Desc);
        }

        public string PrintLog()
        {
            string[] types = { "请求帧 ", "原始帧 ", "积极响应", "消极响应", "信息  " };
            string type = types[((int)Message)];
            if (Desc == "")
            {
                return type;
            }
            if (type == "信息")
            {
                return string.Format("{0} {1}", type, Desc);
            }

            return string.Format("{0} {1} {2} {3} {4}", type, FrameId, Direction, DLc, Desc);
        }
    }
}
