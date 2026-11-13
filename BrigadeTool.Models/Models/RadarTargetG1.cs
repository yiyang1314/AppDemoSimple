using Rxbit.Brigade.Models.EnumEntitys;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Markup;
using static Rxbit.Brigade.Models.Models.BrigadeCanProtocol;

namespace Rxbit.Brigade.Models.Models
{
    public class RadarTargetDto
    {
        public string Desc { get; set; }

        public uint CanID { get; set; }

        public GenType GenType { get; set; } = GenType.Gen1Type;

        public uint FrameID { get; set; }

        public Byte NumObj { get; set; }

        public UInt16 Rnum { get; set; } = 0;
        public UInt16 Anum { get; set; } = 0;

        public RadarGen2Header Gen2Header { get; set; }

        public ConcurrentDictionary<uint, BaseRadarTarget> RadarTargets { get; set; } = new ConcurrentDictionary<uint, BaseRadarTarget>();
    }

    public class BaseRadarTarget : ViewModelBase
    {
        public string Desc { get; set; } = string.Empty;
        public uint CanID { get; set; }
        public GenType GenType { get; set; } = GenType.Gen1Type;

        public Byte Id { get; set; }
        public float PolarRadius { get; set; }
        public float PolarAngle { get; set; }

        public float CoordinatesX { get; set; }
        public float CoordinatesY { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public float Vr { get; set; }
        public float Vx { get; set; }
        public float Vy { get; set; }
    }

    /// <summary>
    /// 310协议
    /// </summary>
    public class RadarGen1Obj : BaseRadarTarget
    {
        public Byte SignalPower { get; set; }
        public Byte Status { get; set; }
        public Byte TriggerEvent { get; set; }
        public Byte DetectionFlag { get; set; }
        public Byte IdentificationFlag { get; set; }
        public Byte VoltageError1 { get; set; }
        public Byte MMICError2 { get; set; }
        public Byte TemperatureError3 { get; set; }
        public Byte CanError4 { get; set; }

        public void Clear()
        {
            this.GenType = GenType.Gen1Type;
            this.PolarRadius = 0;
            this.PolarAngle = 0;
            this.CoordinatesX = 0;
            this.CoordinatesY = 0;
            this.X = 0;
            this.Y = 0;
            this.Vr = 0;
            this.Vx = 0;
            this.Vy = 0;
            this.SignalPower = 0;
            this.Status = 0;
            this.TriggerEvent = 0;
            this.DetectionFlag = 0;
            this.IdentificationFlag = 0;
            this.VoltageError1 = 0;
            this.MMICError2 = 0;
            this.TemperatureError3 = 0;
            this.CanError4 = 0;
        }

        public void ParseDect(DefectInfo defectObj, uint cid)
        {
            this.CanID = defectObj.ID;
            this.GenType = GenType.Gen1Type;
            this.PolarRadius = defectObj.PolarRadius;
            this.PolarAngle = defectObj.PolarAngle;
            this.CoordinatesX = defectObj.CoordinatesX;
            this.CoordinatesY = defectObj.CoordinatesY;
            this.X = defectObj.X;
            this.Y = defectObj.Y;
            this.Vr = defectObj.PolarRadius;
            this.Vx = defectObj.Vx;
            this.Vy = defectObj.Vy;
            this.SignalPower = this.SignalPower;
            this.Id = defectObj.ID;
            this.Status = defectObj.Status;
            this.TriggerEvent = defectObj.TriggerEvent;
            this.DetectionFlag = defectObj.DetectionFlag;
            this.IdentificationFlag = defectObj.IdentificationFlag;
            this.VoltageError1 = defectObj.SensorError1;
            this.MMICError2 = defectObj.SensorError2;
            this.TemperatureError3 = defectObj.SensorError3;
            this.CanError4 = defectObj.SensorError4;
            this.Desc = defectObj.GetDesc(defectObj.ID);
        }
    }

    /// <summary>
    /// 210协议
    /// </summary>
    public class RadarGen2Obj : BaseRadarTarget
    {
        #region N1
        public uint N1ID { get; set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public float ProExist { get; set; }
        #endregion

        #region N2
        public uint N2ID { get; set; }
        public Byte Alive { get; set; }
        public float RCS { get; set; }
        public float MAG { get; set; }
        public float SNR { get; set; }
        public float AccX { get; set; }
        public float AccY { get; set; }
        public Byte SportAttribut { get; set; }
        public Byte Classification { get; set; }

        public Byte HighReflectionFlag { get; set; }
        #endregion

        public void Parse2N1(BrigadeCanProtocol.ObjN1 objN1, uint cid)
        {
            this.N1ID = cid;
            this.Id = objN1.Id;
            this.PolarRadius = objN1.PolarRadius;
            this.PolarAngle = objN1.PolarAngle;
            this.CoordinatesX = objN1.CoordinatesX;
            this.CoordinatesY = objN1.CoordinatesY;
            this.X = objN1.CoordinatesY;
            this.Y = objN1.CoordinatesX;
            this.Vr = objN1.Vr;
            this.Vx = objN1.Vx;
            this.Vy = objN1.Vy;
            this.ProExist = objN1.ProExist;
        }

        public void Parse2N2(BrigadeCanProtocol.ObjN2 objN2, uint cid)
        {
            this.GenType = GenType.Gen2Type;
            this.N2ID = cid;
            this.Alive = objN2.Alive;
            this.RCS = objN2.RCS;
            this.MAG = objN2.MAG;
            this.SNR = objN2.SNR;
            this.AccX = objN2.AccX;
            this.AccY = objN2.AccY;
            this.SportAttribut = objN2.SportAttribut;
            this.Classification = objN2.Classification;
            this.HighReflectionFlag = objN2.HighReflectionFlag;
            this.Desc = GetDesc();
        }

        public void Clear()
        {
            this.N1ID = 0;
            this.Id = 0;
            this.PolarRadius = 0;
            this.PolarAngle = 0;
            this.CoordinatesX = 0;
            this.CoordinatesY = 0;
            this.X = 0;
            this.Y = 0;
            this.Vr = 0;
            this.Vx = 0;
            this.Vy = 0;
            this.ProExist = 0;

            this.N2ID = 0;
            this.Alive = 0;
            this.RCS = 0;
            this.MAG = 0;
            this.SNR = 0;
            this.AccX = 0;
            this.AccY = 0;
            this.SportAttribut = 0;
            this.Classification = 0;
            this.HighReflectionFlag = 0;
            this.Desc = GetDesc();
        }
        public string GetDesc()
        {
            uint cid = this.N2ID & 0x2F0;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Type: Gen2Type");
            sb.AppendLine($"CanID: {cid:x}");
            sb.AppendLine($"ID: {this.Id}");
            sb.AppendLine($"{RadarConstaint.Reflection}: {RadarConstaint.YesArrs[this.HighReflectionFlag]}");
            sb.AppendLine($"{RadarConstaint.LongitudeTip}: {this.CoordinatesX:F3} m");
            sb.AppendLine($"{RadarConstaint.LatgitudeTip}: {this.CoordinatesY:F3} m");
            sb.AppendLine($"{RadarConstaint.LatVxTip}: {this.Vy:F2} km/h");
            sb.AppendLine($"{RadarConstaint.LongVyTip}: {this.Vx:F2} km/h");
            sb.AppendLine($"{RadarConstaint.ProbExistTip}: {this.ProExist:F2}%");
            sb.AppendLine($"{RadarConstaint.LatAccVxTip}: {this.AccY:F2} m/s²");
            sb.AppendLine($"{RadarConstaint.LongAccVyTip}: {this.AccX:F2} m/s²");
            sb.AppendLine($"{RadarConstaint.LiveTip}: {this.Alive}");
            sb.AppendLine($"{RadarConstaint.RCSTip}: {this.RCS:F2}（dB）");
            sb.AppendLine($"{RadarConstaint.SNRTip}: {this.SNR}");
            sb.AppendLine($"{RadarConstaint.MAGTip}: {this.MAG}");
            sb.AppendLine($"{RadarConstaint.SportTip}: {this.SportAttribut} - " + RadarConstaint.MoveString[this.SportAttribut]);
            sb.AppendLine($"{RadarConstaint.ClassTip}: {this.Classification} - " + RadarConstaint.ClsassCate[this.Classification]);
            return sb.ToString();
        }
    }
    public class RadarGen2Header
    {
        public float GroundSpeed { get; set; }
        public Byte IdentificationFlag { get; set; }

        public Byte TBDError5 { get; set; }
        public Byte TBDError6 { get; set; }
        public Byte VoltageError1 { get; set; }
        public Byte MMICError2 { get; set; }
        public Byte TemperatureError3 { get; set; }
        public Byte CanError4 { get; set; }

        public void Clear()
        {
            this.GroundSpeed = 36.25f;
            this.IdentificationFlag = 0;
            this.TBDError5 = 0;
            this.TBDError6 = 0;
            this.VoltageError1 = 0;
            this.MMICError2 = 0;
            this.TemperatureError3 = 0;
            this.CanError4 = 0;
        }

        public void ParseHeader(BrigadeCanProtocol.ObjStatus objStatus)
        {
            this.GroundSpeed = objStatus.GroundSpeed;
            this.IdentificationFlag = objStatus.IdentificationFlag;
            this.TBDError5 = objStatus.TBDError5;
            this.TBDError6 = objStatus.TBDError6;
            this.MMICError2 = objStatus.SensorError2;
            this.CanError4 = objStatus.SensorError4;
            this.VoltageError1 = objStatus.SensorError1;
            this.TemperatureError3 = objStatus.SensorError3;
        }

    }
    public struct BrigadeCanProtocol
    {
        public enum Message : UInt32
        {
            ObjectInfoMode1_MIN = 0x310,
            ObjectInfoMode1_MAX = 0x38F,
            ObjectMinN0 = 0x210,
            ObjectMaxN2 = 0x282,
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DefectInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private Byte[] Data;

            public void Init(Byte[] src)
            {
                if (this.Data == null)
                {
                    this.Data = new Byte[8];
                }

                if (src == null || src.Length > 8)
                {
                    throw new ArgumentException("data length: 8");
                }
                Buffer.BlockCopy(src, 0, this.Data, 0, 8);
            }

            public float PolarRadius => Data[0] * 0.25f;
            public float PolarAngle => Data[1] - 128f;

            public float CoordinatesX => Data[2] * 0.25f; // 纵向
            public float CoordinatesY => Data[3] * 0.25f - 32f; // 横向
            public float X => CoordinatesY;
            public float Y => CoordinatesX;

            public float Vr => Data[4] * 0.5f - 64f;
            public float Vx => (float)(Vr * Math.Cos(PolarAngle * Math.PI / 180));
            public float Vy => (float)(Vr * Math.Sin(PolarAngle * Math.PI / 180));
            public Byte SignalPower => Data[5];
            public Byte ID => (byte)((Data[6] >> 4) & 0x0f);
            public Byte Status => (byte)((Data[6] >> 3) & 0x01);
            public Byte TriggerEvent => (byte)((Data[6] >> 1) & 0x03);
            public Byte DetectionFlag => (byte)(Data[7] & 0x01);
            public Byte IdentificationFlag => (byte)((Data[7] >> 1) & 0x03);
            public Byte SensorError1 => (byte)((Data[7] >> 4) & 0x01);
            public Byte SensorError2 => (byte)((Data[7] >> 5) & 0x01);
            public Byte SensorError3 => (byte)((Data[7] >> 6) & 0x01);
            public Byte SensorError4 => (byte)((Data[7] >> 7) & 0x01);

            public string GetDesc(uint canId)
            {
                if (Data == null) return string.Empty;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Type: Gen2Type");
                sb.AppendLine($"CanID: {canId:X}");
                sb.AppendLine($"ID: {this.ID}");
                sb.AppendLine($"{RadarConstaint.LongitudeTip}: {this.CoordinatesX:F3} m");
                sb.AppendLine($"{RadarConstaint.LatgitudeTip}: {this.CoordinatesY:F3} m");
                sb.AppendLine($"{RadarConstaint.PolarRadiusTip}: {this.PolarRadius:F3} m");
                sb.AppendLine($"{RadarConstaint.PolarAngleTip}: {this.PolarAngle:F2}°");
                sb.AppendLine($"{RadarConstaint.RelativeVTip}: {this.Vr:F3} km/h");
                sb.AppendLine($"{RadarConstaint.SignalTip}: {this.SignalPower}（dB）");
                sb.AppendLine($"{RadarConstaint.AppearTip}: {this.Status}-" + RadarConstaint.RepeatArrs[this.Status]);
                sb.AppendLine($"{RadarConstaint.TriggerTip}: {this.TriggerEvent}-" + RadarConstaint.Alivedecs[this.TriggerEvent]);
                sb.AppendLine($"{RadarConstaint.DetectionTip}: {this.DetectionFlag}-" + RadarConstaint.VaildArrs[this.DetectionFlag]);
                sb.AppendLine($"{RadarConstaint.RadarTypeTip}: {this.IdentificationFlag}-" + RadarConstaint.IdentArrs[this.IdentificationFlag]);
                sb.AppendLine($"{RadarConstaint.VoltageTip}: {this.SensorError1}-" + RadarConstaint.NGArrs[this.SensorError1]);
                sb.AppendLine($"{RadarConstaint.MMICTip}: {this.SensorError2}-" + RadarConstaint.NGArrs[this.SensorError2]);
                sb.AppendLine($"{RadarConstaint.TemperaTip}: {this.SensorError3}-" + RadarConstaint.NGArrs[this.SensorError3]);
                sb.AppendLine($"{RadarConstaint.CanTip}: {this.SensorError4}-" + RadarConstaint.NGArrs[this.SensorError4]);
                return sb.ToString();
            }

            public string ToCsv(StringBuilder sb)
            {
                if (Data == null) return string.Empty;
                sb.Append($",{this.ID},{this.PolarRadius:F3},{this.PolarAngle:F2},{this.CoordinatesX:F3},{this.CoordinatesY:F3},");
                sb.Append($"{this.Vr:F3},{this.SignalPower},{this.Status},{this.TriggerEvent},");
                sb.Append($"{this.DetectionFlag},{this.IdentificationFlag},{this.SensorError1},{this.SensorError2},{this.SensorError3},{this.SensorError4}");
                return sb.ToString();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ObjStatus
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private Byte[] Data;
            public void Init(Byte[] src)
            {
                if (Data == null)
                {
                    this.Data = new Byte[8];
                }

                if (src == null || src.Length < 8)
                {
                    throw new ArgumentException("data length: 8");
                }
                Buffer.BlockCopy(src, 0, Data, 0, 8);
            }
            public UInt16 FrameID => (ushort)((Data[1] << 8) + Data[0]);
            public Byte NumObj => (byte)(Data[2] & 0x1f);
            public float GroundSpeed => Data[3] * 0.5f - 64f;
            public Byte IdentificationFlag => (byte)(Data[7] & 0x03);
            public Byte TBDError5 => (byte)((Data[7] >> 2) & 0x01);
            public Byte TBDError6 => (byte)((Data[7] >> 3) & 0x01);
            public Byte SensorError1 => (byte)((Data[7] >> 4) & 0x01);
            public Byte SensorError2 => (byte)((Data[7] >> 5) & 0x01);
            public Byte SensorError3 => (byte)((Data[7] >> 6) & 0x01);
            public Byte SensorError4 => (byte)((Data[7] >> 7) & 0x01);
            public bool IsEmpty => Data == null;
            public void ToCsv(StringBuilder sb)
            {
                if (Data == null) return ;
                sb.Append($"{this.FrameID},{this.NumObj},{this.GroundSpeed:F2},{this.IdentificationFlag},");
                sb.Append($"{this.TBDError5},{this.TBDError6},{this.SensorError1},{this.SensorError2},{this.SensorError3},{this.SensorError4},");

            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ObjN1
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private Byte[] Data;

            public void Init(Byte[] src)
            {
                if (Data == null)
                {
                    this.Data = new Byte[8];
                }

                if (src == null || src.Length < 8)
                {
                    throw new ArgumentException("data length: 8");
                }
                Buffer.BlockCopy(src, 0, Data, 0, 8);
            }
            public Byte Id => (byte)(Data[0] & 0x1f);
            public float PolarRadius => (float)Math.Sqrt(CoordinatesX * CoordinatesX + CoordinatesY * CoordinatesY);
            public float PolarAngle => (float)(Math.Atan2(CoordinatesY, CoordinatesX) * 180f / Math.PI);
            public float CoordinatesX => ((Data[2] << 8) + Data[1]) * 0.01f; //纵向
            public float CoordinatesY => Data[3] * 0.1f - 12.8f; // 横向
            public float Vr => (float)Math.Sqrt(Vx * Vx + Vy * Vy);
            public float Vx => Data[4] * 0.5f - 64f;
            public float Vy => Data[5] * 0.5f - 64f;
            /// <summary>
            /// 置信度
            /// </summary>
            public float ProExist => Data[6] * 0.392f;

            public void ToCsv(StringBuilder sb)
            {
                if (Data == null) 
                {
                    sb.Append($",,,,,,");
                    return;
                }
                sb.Append($"{this.Id},{this.PolarRadius:F2},{this.PolarAngle:F2},{this.CoordinatesX:F3},{CoordinatesY:F2},");
                sb.Append($"{this.Vx:F2},{this.Vy:F2},{this.ProExist:F2},");
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ObjN2
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private Byte[] Data;

            public void Init(Byte[] src)
            {
                if (Data == null)
                {
                    this.Data = new Byte[8];
                }

                if (src == null || src.Length < 8)
                {
                    throw new ArgumentException("data length: 8");
                }
                Buffer.BlockCopy(src, 0, Data, 0, 8);
            }

            public Byte Id => (byte)(Data[0] & 0x1f);
            public Byte Alive => Data[1];
            public float RCS => Data[2] * 0.05f;
            public float MAG => Data[3] * 0.5f;
            public float SNR => Data[4] * 0.25f;
            public float AccX => Data[5] * 0.1f - 12.8f;
            public float AccY => Data[6] * 0.1f - 12.8f;
            public Byte SportAttribut => (byte)(Data[7] & 0x07);
            public Byte Classification => (byte)((Data[7] >> 4) & 0x03);
            public Byte HighReflectionFlag => (byte)((Data[0] >> 5) & 0x01);
            public void ToCsv(StringBuilder sb)
            {
                if (Data == null)
                {
                    sb.Append($",,,,,,,");
                    return;
                }
                sb.Append($"{this.Alive},{this.RCS:F2},{this.MAG:F3},{SNR:F2},");
                sb.Append($"{this.AccX:F2},{this.AccY:F2},{this.SportAttribut},{this.Classification},{this.HighReflectionFlag}");
            }
        }
    }
}
