using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Parsers;
using Rxbit.Brigade.Models.EnumEntitys;
using Rxbit.Brigade.Models.Models;
using Rxbit.Brigade.Services;
using Rxbit.BrigadeApp.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static Rxbit.Brigade.Models.Models.BrigadeCanProtocol;

namespace Rxbit.BrigadeApp.Services
{
    public class BrigadeCanMessageParser : CanMessageParser
    {
        private readonly DataMediatorService<RadarTargetDto> _trackService;

        public BrigadeCanMessageParser(DataMediatorService<RadarTargetDto> trackService) : base(1, DeviceCategory.Brigade)
        {
            this._trackService = trackService;
            uint Id = 0x200;
            for (uint i = 1; i < 9; i++)
            {
                this.GenRadar2Dirs.Add(Id + i* 16, 1);
                this.GenRadar2Dirs.Add(Id + i* 16 + 1, 2);
                this.GenRadar2Dirs.Add(Id + i* 16 + 2, 3);
            }
        }
        private UInt16 frameId = 0;
        public GenType gType { get; set; } = GenType.Gen1Type;

        public SensorInfo sensorInfo;
        private uint LastSensorId = 0;
        public override void Init(ConfigBus configBus)
        {
            base.Init(configBus);
            this.frameId = 0;
            var bus = configBus as BrigadeConfigBus;
            this.gType = bus.Sensor.Type;
            this.sensorInfo = bus.Sensor;
            this.GenRadar2Targets.Clear();
            LastSensorId = bus.Sensor.CanID;
        }

        public override void Parse()
        {
            while (!this.canMessages.IsEmpty)
            {
                if (this.canMessages.TryDequeue(out var cmw))
                {
                    foreach (var cmsg in cmw.Messages)
                    {
                        var cid = (cmsg.ID << 1) >> 1;
                        if ((cid >= (uint)BrigadeCanProtocol.Message.ObjectInfoMode1_MIN) && cid <= (uint)BrigadeCanProtocol.Message.ObjectInfoMode1_MAX)
                        {
                            ParseN1Mode(cid, cmsg.DATA);
                        }
                        if ((cid >= (uint)BrigadeCanProtocol.Message.ObjectMinN0) && cid <= (uint)BrigadeCanProtocol.Message.ObjectMaxN2)
                        {
                            var type = cid & 0x0f;
                            if (type == 0)
                            {
                                if (rtd.Rnum > 0)
                                {
                                    if (LastSensorId != cid)
                                    {
                                        LastSensorId = cid;
                                    }
                                    else
                                    {
                                        this._trackService.Enqueue(rtd);
                                    }
                                    rtd = new RadarTargetDto()
                                    {
                                        GenType = GenType.Gen2Type,
                                        CanID = cid
                                    };
                                }
                                GenRadar2Targets.Clear();
                                objStatus.Init(cmsg.DATA);
                                rtd.NumObj = objStatus.NumObj;
                                rtd.FrameID = objStatus.FrameID;
                                rtd.Gen2Header = new RadarGen2Header();
                                rtd.Gen2Header.ParseHeader(objStatus);
                            }
                            else if (type == 1)
                            {

                                objN1.Init(cmsg.DATA);
                                if (objN1.Id != 0 && !GenRadar2Targets.ContainsKey(objN1.Id))
                                {
                                    var rt = new RadarGen2Obj();
                                    rt.Parse2N1(objN1, cid);
                                    GenRadar2Targets.Add(objN1.Id, rt);
                                }
                            }
                            else if (type == 2)
                            {
                                rtd.Rnum++;
                                objN2.Init(cmsg.DATA);
                                if (objN2.Id != 0 && GenRadar2Targets.TryGetValue(objN2.Id, out RadarGen2Obj rt))
                                {
                                    rt.Parse2N2(objN2, cid);
                                    rtd.RadarTargets.TryAdd(objN2.Id, rt);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析N1模型
        /// </summary>
        /// <param name="msgId"></param>
        /// <param name="data"></param>
        private void ParseN1Mode(uint msgId, byte[] data)
        {
            if (msgId == this.sensorInfo.CanID)
            {
                this.frameId++;
                if (this.frameId > 0)
                {
                    rtd.Anum = (ushort)rtd.RadarTargets.Count;
                    if (LastSensorId != msgId)
                    {
                        LastSensorId = msgId;
                    }
                    else
                    {
                        this._trackService.Enqueue(rtd);
                    }
                    rtd = new RadarTargetDto()
                    {
                        GenType = gType,
                        CanID = msgId
                    };
                }
            }
            
            defectInfo.Init(data);
            if (!rtd.RadarTargets.ContainsKey(defectInfo.ID))
            {
                var rt = new RadarGen1Obj();
                rt.ParseDect(defectInfo, msgId);
                rtd.RadarTargets.TryAdd(defectInfo.ID,rt);
                rtd.Rnum++;
            }
        }

        private Dictionary<uint, byte> GenRadar2Dirs = new Dictionary<uint, byte>();

        private Dictionary<byte, RadarGen2Obj> GenRadar2Targets = new Dictionary<byte, RadarGen2Obj>();
        private  ObjStatus objStatus = new ObjStatus();
        private  ObjN1 objN1 = new ObjN1();
        private  ObjN2 objN2 = new ObjN2();
        private DefectInfo defectInfo = new BrigadeCanProtocol.DefectInfo();
        private RadarTargetDto rtd = new RadarTargetDto();
    }
}
