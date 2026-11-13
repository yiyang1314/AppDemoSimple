using Rxbit.Brigade.CanDevices.Config;
using Rxbit.Brigade.CanDevices.Services;
using Rxbit.Brigade.CanDevices.Structs;
using Rxbit.Brigade.EcuServices.Models;
using Rxbit.Brigade.EcuServices.Utils;
using Rxbit.Brigade.EcuServices.ZflashItems;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rxbit.Brigade.EcuServices.Services
{
    public delegate void NotifyMessageEvent(EcuMessage ecuMessage, string timestemp, byte[] data, bool IsTx, string desc);
    public delegate void DownFileProgressEvent(int NumId, string title, int blockIndex, int progress);
    public delegate void ZflashProgressEvent(int NumId, string title, byte[] rsp, string errormsg);
    public class RxBitUdsServices : IDisposable
    {
        private readonly CanCommService _canService;
        public ChannelConfig channelConfig { get; set; }
        public NotifyMessageEvent NotifyMsg { get; set; }
        public DownFileProgressEvent DownFileProgress { get; set; }
        public ZflashProgressEvent FlowProgress { get; set; }
        public Action<RXCanMessage> MessageDataComed { get; set; }
        public Action<List<RXCanMessage>> DataListComed { get; set; }

        private AutoResetEvent rspRspEvent = new AutoResetEvent(false);
        public  Func<string, string> DirsKey_DataComed { get; set; }
        public CanCommService GetService() { return _canService; }

        public int ErrorNumId { get; set; }
        public long UseTimes { get; set; }
        public string UseTimesStr
        {
            get { return $"{UseTimes / 60000}m {UseTimes / 1000 % 60}s {UseTimes % 1000}ms"; }
        }
        #region  更到参数设置
        public uint TargetId
        {
            get { return _targetId; }
            set { _targetId = value; }
        }

        private uint _targetId = 0x79f; // ECU接收ID
        public uint SourceId
        {
            get { return _sourceId; }
            set { _sourceId = value; }
        }
        private uint _sourceId = 0x797; // 工具发送ID

        public volatile byte Sid = 0x10;
        private string _dllPath = "d:";

        public string ZflashDir
        {
            get { return _dllPath; }
            set { _dllPath = value; }
        }

        private volatile int stmin = 0;
        private volatile bool IsNRC78 = false;
        public bool IsEnableRecodingLog { get; set; }
        #endregion

        #region 6 --- 日志录制
        public Byte[] Respones
        {
            get { return rsp; }
            set { rsp = value; }
        }
        private volatile byte[] rsp = new byte[8];
        private void SendMsg(EcuMessage ecuMessage, byte[] data)
        {
            var msg = new RXCanMessage()
            {
                DATA = data,
                DLC = 8,
                ID = SourceId,
                TIMESTAMP = this._canService.LastRecCanFrameTIMESTAMP
            };

            if (ecuMessage == EcuMessage.Request)
            {
                msg.DLC = (byte)data.Length;
            }
            else if (ecuMessage == EcuMessage.Negative)
            {
                msg.DLC = 3;
            }
            else if (ecuMessage == EcuMessage.Positive && data[1] != 0x7f && data[0] < 8)
            {
                msg.DLC = data[0];
            }
            MessageDataComed?.Invoke(msg);
        }

        public string ErrorMsg { get; set; }
        public void SendMsg(string desc, bool error = false)
        {
            NotifyMsg?.Invoke(EcuMessage.Nonamal, DateTime.Now.ToString("HH:mm:ss.fff"), null, false, desc);
            ErrorMsg = desc;
        }
        #endregion

        #region 构造器初始化
        public RxBitUdsServices(CanCommService canService)
        {
            _canService = canService;
        }


        private void _canService_CanDataSent(ICanChannelService arg1, List<RXCanMessage> arg2)
        {
            this.DataListComed?.Invoke(arg2);
            if (this.channelConfig == null || arg1.ChannelConfig.Id != this.channelConfig.Id)
            {
                return;
            }
            arg2.Where(u => u.ID == this._sourceId).ToList().ForEach(u => {
                MessageDataComed?.Invoke(u);
            });
        }

        private bool WaitOne(int sid = 0x10, int time = 2000)
        {
            rsp[0] = 0;
            int resid = sid + 0x40;
            rspRspEvent.WaitOne(time);
            return rsp[1] == resid;
        }
        private bool WaitFlowOne(int time = 2000)
        {
            int resid = 0x30;
            rspRspEvent.WaitOne(time);
            return rsp[0] == resid;
        }
        private List<byte> flowdata = new List<byte>();
        private int flowLength = 0;
        private void _canService_CanDataComed(ICanChannelService arg1, List<RXCanMessage> msgs)
        {
            if (this.channelConfig != null && arg1.Id != this.channelConfig.Id)
            {
                return;
            }
            var frames = msgs.FindAll(u => u.ID == _targetId).ToList();
            if (frames.Count > 0)
            {
                foreach (var item in frames)
                {
                    MessageDataComed?.Invoke(item);
                    if (item.DATA[0] == 0x30) // 流程帧  && Sid == item.DATA[1]
                    {
                        Buffer.BlockCopy(item.DATA, 0, rsp, 0, 8);
                        stmin = item.DATA[2];
                        rspRspEvent.Set();
                    }
                    else if (item.DATA[0] == 0x02 && item.DATA[1] == 0x76) // 块接收完成
                    {
                        Array.Copy(item.DATA, 0, rsp, 0, 3);
                        rspRspEvent.Set();
                    }
                    else if (item.DATA[1] == 0x7F && item.DATA[3] == 0x78) // 太繁忙，请等待
                    {
                        IsNRC78 = true; // 03 7F 31 78 82 09 00 00
                    }
                    else if (item.DATA[0] >= 0x10 || item.DATA[0] >= 0x20) // 则为首帧，连续帧；
                    {
                        int frametype = (item.DATA[0] >> 4) & 0x0f;
                        if (frametype == 1) // 则为首帧
                        {
                            flowdata.Clear();
                            flowLength = item.DATA[0] + ((item.DATA[0] & 0x0f) << 8);
                            flowdata.AddRange(item.DATA.Skip(2));
                            Array.Copy(item.DATA, 0, rsp, 0, 8);
                            byte[] src = new byte[8];
                            src[0] = 0x30;
                            this._canService.Send(this.channelConfig, this._sourceId, src);
                            //rspRspEvent.Set();
                        }
                        else if ((item.DATA[0] >= 0x20) && (item.DATA[0] <= 0x2f)) // 连续帧
                        {
                            flowdata.AddRange(item.DATA.Skip(1));
                            if (flowdata.Count >= flowLength)
                            {
                                //FlowRspEvent.Set();
                                rspRspEvent.Set();
                            }
                        }
                    }
                    else
                    {
                        Array.Copy(item.DATA, 0, rsp, 0, 8);
                        rspRspEvent.Set();
                    }
                }
            }
            this.DataListComed?.Invoke(msgs);
        }
        #endregion

        #region 简单烧录
        // UDS服务ID
        private const byte UdsDiagnosticSession = 0x10; // 会话控制
        private const byte UdsRequestTransferExit = 0x37; // 结束传输
        private const byte UdsRoutineControl = 0x31;    // 例程控制（擦除Flash）
        #endregion

        #region 5 --- UDS消息发送
        public bool SendFlowUds(byte[] data)
        {
            SendMsg(EcuMessage.Request, data);
            byte[] src = new byte[8];
            src[0] = (byte)(0x10 + (data.Length >> 8));
            Sid = data[0];
            src[1] = (byte)(data.Length & 0xff);
            Buffer.BlockCopy(data, 0, src, 2, 6);
            byte[] mblock = new byte[8];
            mblock[0] = 0x21;
            Buffer.BlockCopy(data, 6, mblock, 1, data.Length - 6);
            _canService.Send(this.channelConfig, this._sourceId, src);
            bool result = WaitFlowOne();
            if (!result)
            {
                return false;
            }
            _canService.Send(this.channelConfig, this._sourceId, mblock);
            result = WaitOne(Sid, 1000);
            SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp);
            return result;
        }

        /// <summary>
        /// 发送多帧
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        /// <param name="bs"></param>
        /// <returns></returns>
        private bool SendMuiltFrame(byte[] data, int time = 2000)
        {
            SendMsg(EcuMessage.Request, data);
            byte[] src = new byte[8];
            src[0] = (byte)(0x10 + (data.Length >> 8));
            Sid = data[0];
            src[1] = (byte)(data.Length & 0xff);
            Buffer.BlockCopy(data, 0, src, 2, 6);
            _canService.Send(this.channelConfig, this._sourceId, src);
            if (!WaitFlowOne())
            {
                FlowProgress?.Invoke(-1, "First Frame", src, "Flow control timeout");
                return false;
            }

            int pos = 6;
            int bs = rsp[1];
            List<byte[]> freames = GetFrames(data, 6, data.Length - pos, 8, 0);
            if (bs == 0)
            {
                bs = freames.Count;
            }
            for (int i = 0; i < freames.Count; i += bs)
            {
                for (int j = 0; j < bs; j++)
                {
                    if ((j + i) < freames.Count)
                    {
                        _canService.Send(this.channelConfig, this._sourceId, freames[j + i]);
                    }
                    else
                    {
                        break;
                    }
                }
                if (i + bs > freames.Count)
                {
                    break;
                }
            }
            bool reult = WaitOne(Sid, time);
            SendMsg(reult ? EcuMessage.Positive : EcuMessage.Negative, rsp);
            return reult && rsp[1] != 0x7F;
        }

        public bool SendFlowFrame(byte[] data)
        {
            _canService.Send(this.channelConfig, this._sourceId, data);
            return WaitFlowOne();
        }
        public bool SendUds(byte[] data)
        {
            if (!this.IsRunnning) return false;
            byte[] src = new byte[8];
            Buffer.BlockCopy(data, 0, src, 1, data.Length);
            src[0] = (byte)data.Length;
            Sid = data[0];
            SendMsg(EcuMessage.Request, data);
            if (!_canService.Send(this.channelConfig, this._sourceId, src))
            {
                SendMsg("Send message failed", true);
                return false;
            }
            bool result = WaitOne(Sid, 2000);
            SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp);
            return result && rsp[1] == (0x40 + Sid);
        }

        public void Send(uint protoolId, byte[] data)
        {
            _canService.Send(this.channelConfig, protoolId, data);
        }
        #endregion

        #region 1 --- 通用类型消息
        public bool SendGeneralUds(byte[] data)
        {
            IsRunnning = true;
            bool ret = false;
            if (data.Length > 7)
            {
                ret = SendMuiltFrame(data);
            }
            else
            {
                ret = SendUds(data);

            }
            IsRunnning = false;
            return ret;
        }
        public bool SendGenranr(byte sid, byte[] data, string desc = "Session Diagnosis")
        {
            var mdata = new byte[] { sid };
            bool ret = false;
            if (data.Length > 6)
            {
                ret = SendMuiltFrame(mdata.Concat(data).ToArray());
            }
            else
            {
                ret = SendUds(mdata.Concat(data).ToArray());
            }
            return ret;
        }
        public bool SendGenranr(ZFlashGener flashGener)
        {
            byte[] subdata = flashGener.Data.Split(' ').Where(u => u.Length > 0).Select(u => Convert.ToByte(u, 16)).ToArray();
            var sid = Convert.ToByte(flashGener.Sid, 16);
            return SendGenranr(sid, subdata, flashGener.NumID + "." + flashGener.Desc);
        }
        #endregion

        #region 2 --- 安全访问解密
        public bool UnlockSecrityAccess(ZFlashSecurity flashSecurity)
        {
            string dllpath = Path.Combine(_dllPath, flashSecurity.DllPath);
            if (!File.Exists(dllpath))
            {
                FlowProgress?.Invoke(-1, flashSecurity.NumID + "." + flashSecurity.Desc, null, "Unlock DLL path not found");
                return false;
            }
            return UnlockSecrityAccess(dllpath, (byte)flashSecurity.Level, flashSecurity.IgnoreUnlockedCheck, flashSecurity.NumID + "." + flashSecurity.Desc);
        }

        /// <summary>
        /// 基于DLL路径动态解密算法
        /// </summary>
        /// <param name="dllpath"> 路径</param>
        /// <param name="level">安全级别</param>
        /// <param name="ignoreunlocked">是否忽略</param>
        /// <param name="desc">功能描述</param>
        /// <returns></returns>
        public bool UnlockSecrityAccess(string dllpath, byte level, bool ignoreunlocked = false, string desc = "Security Access")
        {
            byte type = (byte)(ignoreunlocked ? 0 : 1);
            // 2. 安全解锁（27服务）
            if (!SendUds(new byte[] { 0x27, 0x11 }))
            {
                FlowProgress?.Invoke(-1, desc, rsp, "Unlock service request failed");
                return false; // Unlock failed
            }

            byte[] seedArray = new byte[rsp[0] - 2];
            Buffer.BlockCopy(rsp, 3, seedArray, 0, rsp[0] - 2);
            bool ok = DynamicKeyGen.UnlockSeek(dllpath, seedArray, level, out byte[] key); // 调用DLL算法
            if (!ok || !SendUds(new byte[] { 0x27, 0x12 }.Concat(key).ToArray()))
            {
                FlowProgress?.Invoke(-1, desc, new byte[] { 0x27, 0x12 }.Concat(key).ToArray(), "Request frame");
                FlowProgress?.Invoke(-1, desc, rsp, "Unlock failed!");
                return false;
            }
            return true;
        }
        #endregion

        #region 3 --- 文件下载
        /// <summary>
        /// 按文件配置项实现文件下载
        /// </summary>
        /// <param name="flashDown"></param>
        /// <returns></returns>
        public bool SendFlashDownFile(ZFlashDown flashDown)
        {
            return SendDownFileBlock(flashDown.ZFlashBlock.Datas, flashDown.Addresssize, flashDown.Lengthsize,
                (byte)(flashDown.DataFormatIdentifier & 0xff), flashDown.NumID, flashDown.EraseEcuMemoryMode);
        }

        /// <summary>
        /// 文件擦除
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="size">长度大小</param>
        /// <param name="EraseMode">擦除方式</param>
        /// <returns></returns>
        private bool DoEraseFlash(byte addresslen, byte[] addr, byte[] size, int EraseMode = 3)
        {
            if (EraseMode == 3) // 不擦除
            {
                return true;
            }

            if (EraseMode == 0) // 带参数擦除
            {
                byte[] data = new byte[] { 0x31, 0x01, 0xFF, 0, addresslen };
                return SendMuiltFrame(data.Concat(addr).Concat(size).ToArray(), 10000);
            }

            if (EraseMode == 2) // 不带参数擦除
            {
                return SendUds(new byte[] { 0x31, 0x01, 0xff, 00 });
            }

            if (EraseMode == 1)
            {
                SendMsg("Block erase function not supported");
                return false;
            }
            // EraseMode 0 ---- 带参数擦除 31 01 ff 00 + 地址 + 长度 
            // EraseMode 1 ---- 分块擦除 31 01 ff 00 + 地址 + 长度 
            // EraseMode 2 ---- 不带参数擦除 31 01 ff 00
            // EraseMode 3 ---- 不擦除
            return true;
        }

        /// <summary>
        /// 文件发送
        /// </summary>
        /// <param name="dataBlock">文件块</param>
        /// <param name="addresslen">块地址长度</param>
        /// <param name="sizelen">块字节长度</param>
        /// <param name="reqParam">请求参数</param>
        /// <param name="zflashnumId">id</param>
        /// <param name="EraseMode">擦除模式</param>
        /// <returns></returns>
        private bool SendDownFileBlock(DataBlock dataBlock, int addresslen = 4, int sizelen = 3, byte reqParam = 0, int zflashnumId = 0, int EraseMode = 0)
        {
            FlowProgress?.Invoke(zflashnumId, "File erasing........", null, "");
            byte address = (byte)(((sizelen & 0xf) << 4) + (addresslen & 0xf));//04
            // { 0x00, 0x08, 0x00, 0x00 }; // 起始地址0x08000000
            byte[] addr = BitConverter.GetBytes(dataBlock.Address).Reverse().Skip(4 - addresslen).ToArray();
            byte[] size = BitConverter.GetBytes(dataBlock.Data.Count).Reverse().Skip(4 - sizelen).ToArray();
            // 下载前擦除
            if (!DoEraseFlash(address, addr, size, EraseMode))
            {
                FlowProgress?.Invoke(-1, zflashnumId + ".File Download", null, "Parameter erase failed");
                return false;
            }
            // 发送文件大小
            bool result = SendFlowUds(new byte[] { 0x34, reqParam, address }.Concat(addr).Concat(size).ToArray());
            if (!result)
            {
                FlowProgress?.Invoke(-1, zflashnumId + ".File Download", rsp, "Request send file address length failed");
                return false;
            }
            int maxbyte = rsp[2] >> 4;
            int blocklen = (rsp[3] << 8) + rsp[4];
            int canlen = 8;
            var stopwatch = new Stopwatch();
            List<List<Frame>> datapaacket = SplitToBlocks(dataBlock.Data.ToArray(), blocklen, canlen);
            DownFileProgress?.Invoke(0, "", datapaacket.Count, 0);
            int bs = 20;
            int stmin = 0x7f;
            List<byte[]> muiltframe = new List<byte[]>();
            foreach (var mFrame in datapaacket)
            {
                Frame first = mFrame.First();
                if (!SendFlowFrame(first.Data))
                {
                    FlowProgress?.Invoke(-1, $"{zflashnumId}.File Download Block[{first.BlockIndex}] First Frame", first.Data, "Flow control frame response timeout");
                    return false;
                }
                bs = rsp[1];
                int blockBs = rsp[1];
                if (blockBs == 0)
                {
                    bs = mFrame.Count - 1;
                }
                stmin = rsp[2];
                stopwatch.Restart();
                for (int i = 1; i < mFrame.Count; i += bs)
                {
                    if (!this.IsRunnning) return false;
                    int validbs = Math.Min(bs, mFrame.Count - i);
                    result = _canService.SendMuiltFrame(this.channelConfig, this._sourceId, mFrame, i, validbs);
                    if ((blockBs != 0) && validbs == bs)
                    {
                        if (!WaitFlowOne())
                        {
                            SendMsg($"{mFrame[i + validbs].Data[0].ToString("X2")}");
                            FlowProgress?.Invoke(-1, $"{zflashnumId}.File Download Block[{first.BlockIndex}] Frame[{i}]", first.Data, "Consecutive frame response timeout");
                            return false;
                        }
                        bs = rsp[1];
                        stmin = rsp[2];
                        muiltframe.Clear();
                    }
                    else
                    {
                        if (!WaitOne(0x36, 2000) || rsp[2] != (first.BlockIndex % 256))
                        {
                            SendMsg($"{mFrame[i + validbs - 1].Data[1].ToString("X2")}");
                            return false;
                        }
                    }
                    if (blockBs == 0) break;
                }
                stopwatch.Stop();
                SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp);
                DownFileProgress?.Invoke(zflashnumId, $"Elapsed: {stopwatch.ElapsedMilliseconds}ms, Estimated {stopwatch.ElapsedMilliseconds * (datapaacket.Count - first.BlockIndex) / 60000 + 1}m", zflashnumId, (int)(100 * 1.0 * first.BlockIndex / datapaacket.Count));
            }
            return SendUds(new byte[] { UdsRequestTransferExit });
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="src"></param>
        /// <param name="maxlen"></param>
        /// <param name="canlen"></param>
        /// <returns></returns>
        public List<List<Frame>> SplitToBlocks(byte[] src, int maxlen, int canlen)
        {
            int BLOCK_PAY = maxlen - 2; // 载荷
            int srcLen = src.Length;
            int totalBlocks = (srcLen + BLOCK_PAY - 1) / BLOCK_PAY;
            var result = new List<List<Frame>>(totalBlocks);

            // 首帧固定长度字节
            for (int block = 0; block < totalBlocks; block++)
            {
                int srcOffset = block * BLOCK_PAY;
                int payLen = Math.Min(BLOCK_PAY, srcLen - srcOffset);
                var blockFrames = new List<Frame>(147); // 预估值

                // 首帧：2 B 固定长度 + 2 B 块索引 + 4 B 数据
                byte[] first = new byte[canlen];
                first[0] = (byte)(0x10 + (((payLen + 2) >> 8) & 0x0f));
                first[1] = (byte)((payLen + 2) & 0xff);
                first[2] = 0x36;
                first[3] = (byte)((block + 1) & 0xFF);
                Buffer.BlockCopy(src, srcOffset, first, 4, Math.Min(4, payLen));
                // 不足4字节已补0
                blockFrames.Add(new Frame
                {
                    BlockIndex = (ushort)((block + 1)),
                    FrameIndex = 0,
                    Data = first
                });

                // 剩余数据 → 7 B 一帧（动态算）
                int remaining = payLen - 4;
                int frames7 = (remaining + 7 - 1) / 7;
                for (int f = 0; f < frames7; f++)
                {
                    int off = srcOffset + 4 + f * 7;
                    int len = Math.Min(7, remaining - f * 7);
                    byte[] pkt = new byte[8];
                    pkt[0] = (byte)(((f + 1) & 0x0f) + 0x20);           // 帧索引 1..
                    Buffer.BlockCopy(src, off, pkt, 1, len);
                    // 不足7字节右侧已补0
                    blockFrames.Add(new Frame
                    {
                        BlockIndex = (ushort)((block + 1)),
                        FrameIndex = (byte)((f + 1) % 16 + 0x20),
                        Data = pkt
                    });
                }
                result.Add(blockFrames);
            }
            return result;
        }
        /// <summary>
        /// 获取连续帧
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="startPos">起始位置</param>
        /// <param name="vaildBlockLen">最大长度</param>
        /// <param name="minByteLen">发送最大长度</param>
        /// <param name="bs">连续发送帧数</param>
        /// <returns></returns>
        private List<byte[]> GetFrames(byte[] data, int pos, int vaildBlockLen, int canlen, int usecount = 4)
        {
            int bipos = 0;
            int cf = 0x21;
            List<byte[]> freames = new List<byte[]>();
            do
            {
                byte[] minBytes = new byte[canlen];
                int bVaildLen = Math.Min(canlen - 1, vaildBlockLen - usecount - bipos);
                if (cf > 0x2f)
                {
                    cf = 0x20;
                }
                minBytes[0] = (byte)cf;
                Array.Copy(data, pos + bipos, minBytes, 1, bVaildLen);
                freames.Add(minBytes);
                bipos += (canlen - 1);
                cf++;
            } while ((vaildBlockLen - bipos - 1) > 0);
            return freames;
        }
        #endregion

        #region 0 --- 消息分发
        /// <summary>
        /// Zflash测试项
        /// </summary>
        /// <param name="zFlash">flash模块</param>
        /// <returns></returns>
        public bool SendZFlashItem(ZFlashItem zFlash)
        {
            if (zFlash == null) return false;
            bool result = false;
            SendMsg(zFlash.NumID + "." + zFlash.Desc);
            FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, null, "");
            try
            {
                zFlash.Status = ZFlashItemStatus.Running;
                if (zFlash is ZFlashGener)
                {
                    result = SendGenranr(zFlash as ZFlashGener);
                }
                else if (zFlash is ZFlashSecurity)
                {
                    result = UnlockSecrityAccess(zFlash as ZFlashSecurity);
                }
                else if (zFlash is ZFlashDown)
                {
                    result = SendFlashDownFile(zFlash as ZFlashDown);
                }
                if (zFlash is ZFlashDelay)
                {
                    ZFlashDelay zFlashDelay = zFlash as ZFlashDelay;
                    Thread.Sleep(zFlashDelay.DelayTime);
                    result = true;
                }
                zFlash.Status = result ? ZFlashItemStatus.Finshed : ZFlashItemStatus.Faild;
                if (!result)
                {
                    SendMsg(zFlash.NumID + "." + zFlash.Desc, true);
                    this.ErrorNumId = zFlash.NumID;
                    FlowProgress?.Invoke(-1, zFlash.NumID + "." + zFlash.Desc, rsp, "Message timeout");
                }
                return result;
            }
            catch (Exception ex)
            {
                SendMsg(zFlash.NumID + "." + zFlash.Desc + " Exception: " + ex.Message, true);
                FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, rsp, ex.Message);
                zFlash.Status = ZFlashItemStatus.Faild;
            }
            finally
            {
            }
            return false;
        }

        public async Task<bool> StartBootloader(List<ZFlashItem> flashItems)
        {
            if (this.channelConfig == null) { return false; }
            this.IsRunnning = true;
            return await Task.Run(() => {
                bool result = false;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (ZFlashItem flashItem in flashItems)
                {
                    result = SendZFlashItem(flashItem);
                    if (!result)
                    {
                        break;
                    }
                }
                stopwatch.Stop();
                this.UseTimes = stopwatch.ElapsedMilliseconds;
                SendMsg(result ? "Zflash programming completed.." : "Programming failed!");
                this.IsRunnning = false;
                return result;
            });
        }
        private volatile bool mIsReg;
        public void RegisterBoot()
        {
            if (!this.mIsReg)
            {
                this._canService.CanDataComed -= _canService_CanDataComed;
                this._canService.CanDataSent -= _canService_CanDataSent;
                this._canService.CanDataComed += _canService_CanDataComed;
                this._canService.CanDataSent += _canService_CanDataSent;
                this.mIsReg = true;
            }
        }

        public void DisposibleBoot()
        {
            this._canService.CanDataComed -= _canService_CanDataComed;
            this._canService.CanDataSent -= _canService_CanDataSent;
            this.mIsReg = false;
        }

        /// <summary>
        /// 执行烧录流程
        /// </summary>
        /// <param name="flashItems">UDS测试项</param>
        /// <param name="mNotifyMsg">UDS直接接收事件</param>
        /// <param name="Progress">文件块上传事件</param>
        public async Task<bool> StartBootloader(List<ZFlashItem> flashItems, NotifyMessageEvent mNotifyMsg, DownFileProgressEvent Progress)
        {
            if (this.channelConfig == null) { return false; }
            this.NotifyMsg -= mNotifyMsg;
            this.DownFileProgress -= Progress;
            this.NotifyMsg += mNotifyMsg;
            this.DownFileProgress += Progress;
            this.IsRunnning = true;
            this._canService.CanDataComed -= _canService_CanDataComed;
            this._canService.CanDataSent -= _canService_CanDataSent;
            this._canService.CanDataComed += _canService_CanDataComed;
            this._canService.CanDataSent += _canService_CanDataSent;
            return await Task.Run(() => {
                bool result = false;
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                foreach (ZFlashItem flashItem in flashItems)
                {
                    result = SendZFlashItem(flashItem);
                    if (!result)
                    {
                        break;
                    }
                }
                stopwatch.Stop();
                this.UseTimes = stopwatch.ElapsedMilliseconds;
                SendMsg(result ? "Zflash programming completed.." : "Programming failed!");
                this.NotifyMsg -= mNotifyMsg;
                this.DownFileProgress -= Progress;
                this.IsRunnning = false;
                this._canService.CanDataComed -= _canService_CanDataComed;
                this._canService.CanDataSent -= _canService_CanDataSent;
                return result;
            });
        }
        /// <summary>
        /// 执行烧录流程
        /// </summary>
        /// <param name="flashItems">UDS测试项</param>
        /// <param name="mNotifyMsg">UDS直接接收事件</param>
        /// <param name="Progress">文件块上传事件</param>
        public async Task<bool> StartEcu(List<ZFlashItem> flashItems, DownFileProgressEvent Progress)
        {
            this.DownFileProgress -= Progress;
            this.DownFileProgress += Progress;
            this.IsRunnning = true;
            return await Task.Run(() => {
                bool result = false;
                foreach (ZFlashItem flashItem in flashItems)
                {
                    result = SendZFlashItem(flashItem);
                    if (!result)
                    {
                        break;
                    }
                }
                this.DownFileProgress -= Progress;
                this.IsRunnning = false;
                return result;
            });
        }

        public async Task StopBootloader(int timeout = 5000)
        {
            this.IsRunnning = false;
            await Task.Delay(timeout);
        }
        private volatile bool IsRunnning = false;
        private bool disposedValue;

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~RxBitUdsServices()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// UDS协议服务（ISO 14229）
    /// </summary>
    public class UdsService
    {
        private readonly CanCommService _canService;
        public ChannelConfig channelConfig { get; set; }
        public NotifyMessageEvent NotifyMsg { get; set; }
        public DownFileProgressEvent DownFileProgress { get; set; }
        public ZflashProgressEvent FlowProgress { get; set; }
        public Action<ZflashLog> UdsAutoEvent { get; set; }
        private readonly uint _channel;
        public Func<string, string> DirsKey_DataComed { get; set; }

        public bool IsCheckVersion { get; set; } = true;
        #region  更到参数设置
        public uint TargetId
        {
            get { return _targetId; }
            set { _targetId = value; }
        }

        private uint _targetId = 0x79f; // ECU接收ID
        public uint SourceId
        {
            get { return _sourceId; }
            set { _sourceId = value; }
        }
        private uint _sourceId = 0x797; // 工具发送ID

        public volatile byte Sid = 0x10;
        private string _dllPath = "d:";

        public string ZflashDir
        {
            get { return _dllPath; }
            set { _dllPath = value; }
        }

        private volatile int stmin = 0;
        private volatile bool IsNRC78 = false;
        public bool IsEnableRecodingLog { get; set; }
        #endregion

        #region 6 --- 日志录制
        public Byte[] Respones
        {
            get { return rsp; }
            set { rsp = value; }
        }
        private volatile byte[] rsp = new byte[8];

        public Byte[] FlowRsp { get { return flowdata.GetRange(0, Math.Min(flowdata.Count, flowLength)).ToArray(); } }
        public bool IsRecodingLog { get; set; }
        private void SendMsg(EcuMessage ecuMessage, byte[] data, bool IsTx = true, string desc = "")
        {
            if (this.IsRecodingLog)
            {
                NotifyMsg?.Invoke(ecuMessage, DateTime.Now.ToString("HH:mm:ss.fff"), data, IsTx, "");
            }
        }
        private void SendMsg(byte[] data, string tempstamp)
        {
            if (this.IsRecodingLog)
                NotifyMsg?.Invoke(EcuMessage.Source, tempstamp, data, true, "");
        }
        public void SendMsg(string desc, bool error = false)
        {
            if (error || this.IsRecodingLog)
                NotifyMsg?.Invoke(EcuMessage.Nonamal, DateTime.Now.ToString("HH:mm:ss.fff"), null, false, desc);
        }
        #endregion

        #region 构造器初始化
        private ConcurrentDictionary<byte, (string ChineseMsg, string EnglishMsg)> NPCs = new ConcurrentDictionary<byte, (string ChineseMsg, string EnglishMsg)>();
        public UdsService(CanCommService canService, uint channel)
        {
            _canService = canService;
            _channel = channel;
            InitNpcs();
        }

        public int ConfdType { get; set; } = 0;
        public bool IsCanfd { get; set; }
        private int rspTime = 2000;
        public void LoadedCanEvent(int cantype, bool mIsCanfd = false)
        {
            _canService.CanDataComed -= _canService_CanDataComed;
            _canService.CanDataSent -= _canService_CanDataSent;
            _canService.CanDataComed += _canService_CanDataComed;
            _canService.CanDataSent += _canService_CanDataSent;
            rspTime = 10000;
            if (this.channelConfig != null)
            {
                _canService.SetCantype(this.channelConfig, (byte)cantype, mIsCanfd);
                this.ConfdType = cantype;
                this.IsCanfd = mIsCanfd;
                rspTime = cantype == 2 ? 12000 : 10000;
            }
        }

        public void UnLoadedCanEvent()
        {
            _canService.CanDataComed -= _canService_CanDataComed;
            _canService.CanDataSent -= _canService_CanDataSent;
            _canService?.SetCantype(this.channelConfig, 0, false);
        }

        private void _canService_CanDataSent(ICanChannelService arg1, List<RXCanMessage> arg2)
        {
            if (!this.IsRecodingLog || this.channelConfig == null || arg1.ChannelConfig.Id != this.channelConfig.Id)
            {
                return;
            }
            string tempstamp = DateTime.Now.ToString("HH:mm:ss.fff");
            arg2.Where(u => u.ID == this._sourceId).ToList().ForEach(u => {
                SendMsg(u.DATA, tempstamp); //MsgQueues.Enqueue(u);
            });
        }
        private void _canService_CanDataComed(ICanChannelService arg1, List<RXCanMessage> msgs)
        {
            var frames = msgs.FindAll(u => u.ID == _targetId);
            if (frames.Count > 0)
            {
                foreach (var item in frames)
                {
                    ///SendMsg(EcuMessage.Source, item.DATA, false);
                    if (item.DATA[0] == 0x30) // 流程帧  && Sid == item.DATA[1]
                    {
                        Buffer.BlockCopy(item.DATA, 0, rsp, 0, 8);
                        stmin = item.DATA[2];
                        this.IsReviceMsg = true;
                    }
                    else if (item.DATA[0] == 2 && item.DATA[1] == 0x76) // 流程帧  && Sid == item.DATA[1]
                    {
                        Buffer.BlockCopy(item.DATA, 0, rsp, 0, 8);
                        IsReviceMsg = true;
                    }
                    else if (item.DATA[1] == 0x7F && item.DATA[3] == 0X78) // 太繁忙，请等待
                    {
                        IsNRC78 = true; // 03 7F 31 78 82 09 00 00
                    }
                    else if (item.DATA[0] >= 0x10 || item.DATA[0] >= 0x20) // 则为首帧，连续帧；
                    {
                        int frametype = (item.DATA[0] >> 4) & 0x0f;
                        if (frametype == 1) // 则为首帧
                        {
                            flowdata.Clear();
                            flowLength = item.DATA[1] + ((item.DATA[0] & 0x0f) << 8);
                            flowdata.AddRange(item.DATA.Skip(2));
                            Array.Copy(item.DATA, 0, rsp, 0, 8);
                            byte[] src = new byte[8];
                            src[0] = 0x30;
                            _canService.Send(this.channelConfig, this._sourceId, src);
                        }
                        else if ((item.DATA[0] >= 0x20) && (item.DATA[0] <= 0x2f)) // 连续帧
                        {
                            flowdata.AddRange(item.DATA.Skip(1));
                            IsReviceMsg = flowdata.Count >= flowLength;
                            rsp[0] = 7;
                            Buffer.BlockCopy(flowdata.ToArray(), 0, rsp, 1, 7);
                        }
                    }
                    else
                    {
                        Array.Copy(item.DATA, 0, rsp, 0, 8);
                        IsReviceMsg = true;
                    }
                }
            }
        }
        private volatile bool IsReviceMsg = false;
        private bool WaitOne(int sid = 0x10, int time = 2000, bool mReviceMsg = false)
        {
            rsp[0] = 0;
            int resid = sid + 0x40;
            this.IsReviceMsg = mReviceMsg;
            int timesatrt = 0;
            while (timesatrt <= time)
            {
                Thread.Sleep(4);
                timesatrt += 4;
                if (IsReviceMsg && rsp[1] == resid)
                {
                    return true;
                }
            }
            FlowProgress?.Invoke(-1, null, rsp, DirsKey_DataComed.Invoke("Ecuboot_NoDllip"));
            return IsReviceMsg;
        }
        private bool WaitFlowOne(int time = 2000)
        {
            int resid = 0x30;
            int timesatrt = 0;
            while (timesatrt <= time)
            {
                Thread.Sleep(4);
                timesatrt += 4;
                if (this.IsReviceMsg && rsp[0] == resid)
                {
                    return true;
                }
            }
            FlowProgress?.Invoke(-1, null, rsp, "Flow control message timeout");
            return false;
        }
        private List<byte> flowdata = new List<byte>();
        private int flowLength = 0;

        private void InitNpcs()
        {
            NPCs.TryAdd(0x10, ("Not defined in protocol", "GeneralReject"));
            NPCs.TryAdd(0x11, ("ECU does not implement this service", "ServiceNotSupported"));
            NPCs.TryAdd(0x12, ("Sub-function does not exist", "SubFunctionNotSupported"));
            NPCs.TryAdd(0x13, ("Length/format error, 1 byte more or less", "IncorrectMessageLength/InvalidFormat"));
            NPCs.TryAdd(0x22, ("Preconditions not met, low voltage, gear D, engine running", "ConditionsNotCorrect"));
            NPCs.TryAdd(0x24, ("Send sequence error, sent 0x27 without entering extended session", "RequestSequenceError"));
            NPCs.TryAdd(0x31, ("Address/data ID illegal, download start address exceeds Flash size", "RequestOutOfRange"));
            NPCs.TryAdd(0x33, ("Sent without unlocking, 0x34, 0x36", "SecurityAccessDenied"));
            NPCs.TryAdd(0x35, ("Key calculation error", "InvalidKey"));
            NPCs.TryAdd(0x36, ("Consecutive wrong keys, locked after reaching max attempts", "ExceedNumberOfAttempts"));
            NPCs.TryAdd(0x37, ("Penalty countdown, wait 10s after 0x36 triggered", "RequiredTimeDelayNotExpired"));
            NPCs.TryAdd(0x70, ("Download not allowed, Boot not unlocked or write protected", "UploadDownloadNotAccepted"));
            NPCs.TryAdd(0x71, ("Data flow transfer suspended", "TransferDataSuspended"));
            NPCs.TryAdd(0x72, ("Program/erase failed, Flash check error, hardware fault", "GeneralProgrammingFailure"));
            NPCs.TryAdd(0x73, ("Block sequence counter jump, sequence not cycled 0x01-0xFF", "WrongBlockSequenceCounter"));
            NPCs.TryAdd(0x78, ("Please wait - delayed response for long erase, long CRC calculation", "ResponsePending"));
            NPCs.TryAdd(0x7E, ("Sub-function not supported in current session", "SubFunctionNotSupportedInActiveSession"));
            NPCs.TryAdd(0x7F, ("Service not supported in current session, default session sent 0x34", "ServiceNotSupportedInActiveSession"));
            NPCs.TryAdd(0x92, ("System voltage too high", "VoltageTooHigh"));
            NPCs.TryAdd(0x93, ("System voltage too low", "VoltageTooLow"));
        }
        #endregion

        #region 简单烧录
        // UDS服务ID
        private const byte UdsDiagnosticSession = 0x10; // 会话控制
        private const byte UdsRequestTransferExit = 0x37; // 结束传输
        private const byte UdsRoutineControl = 0x31;    // 例程控制（擦除Flash）
        #endregion

        #region 5 --- UDS消息发送
        public bool SendFlowUds(byte[] data)
        {
            SendMsg(EcuMessage.Request, data);
            byte[] src = new byte[8];
            src[0] = (byte)(0x10 + (data.Length >> 8));
            Sid = data[0];
            src[1] = (byte)(data.Length & 0xff);
            Buffer.BlockCopy(data, 0, src, 2, 6);

            byte[] mblock = new byte[8];
            mblock[0] = 0x21; this.IsReviceMsg = false;
            Buffer.BlockCopy(data, 6, mblock, 1, data.Length - 6);
            _canService.Send(this.channelConfig, this._sourceId, src);
            bool result = WaitFlowOne();
            if (!result)
            {
                return false;
            }
            //SendMsg(EcuMessage.Source, mblock);
            _canService.Send(this.channelConfig, this._sourceId, mblock);
            result = WaitOne(Sid, 2000);
            SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp, false);
            return result;
        }

        /// <summary>
        /// 发送多帧
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="data"></param>
        /// <param name="bs"></param>
        /// <returns></returns>
        private bool SendMuiltFrame(byte[] data, int time = 2000)
        {
            SendMsg(EcuMessage.Request, data);
            byte[] src = new byte[8];
            src[0] = (byte)(0x10 + (data.Length >> 8));
            Sid = data[0];
            src[1] = (byte)(data.Length & 0xff);
            IsReviceMsg = false;
            Buffer.BlockCopy(data, 0, src, 2, 6);
            _canService.Send(this.channelConfig, this._sourceId, src);
            if (!WaitFlowOne())
            {
                FlowProgress?.Invoke(-1, "First Frame", src, "Flow control timeout");
                return false;
            }

            int pos = 6;
            int bs = 1;
            if (rsp[1] > 1)
            {
                bs = rsp[1];
            }
            List<byte[]> freames = GetFrames(data, 6, data.Length - pos, 8, 0);
            for (int i = 0; i < freames.Count; i += bs)
            {
                for (int j = 0; j < bs; j++)
                {
                    if ((j + i) < freames.Count)
                    {
                        _canService.Send(this.channelConfig, this._sourceId, freames[j + i]);
                    }
                    else
                    {
                        break;
                    }
                }
                if (i + bs > freames.Count)
                {
                    break;
                }
            }
            bool reult = WaitOne(Sid, time);
            SendMsg(reult ? EcuMessage.Positive : EcuMessage.Negative, rsp);
            return reult && rsp[1] != 0x7F;
        }

        public bool SendUds(byte[] data)
        {
            if (!this.IsRunnning) return false;
            byte[] src = new byte[8];
            Buffer.BlockCopy(data, 0, src, 1, data.Length);
            src[0] = (byte)data.Length;
            Sid = data[0];
            SendMsg(EcuMessage.Request, data);
            if (!_canService.Send(this.channelConfig, this._sourceId, src))
            {
                string error = $"Device [{this.channelConfig.CCategroy}] Channel [{this.channelConfig.ChannelIndex}] unavailable or not initialized";
                SendMsg(error, true);
                FlowProgress?.Invoke(-1, "Send message failed", null, error);
                return false;
            }
            bool result = WaitOne(Sid, rspTime);
            SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp, false);
            return result && rsp[1] == (0x40 + Sid);
        }
        #endregion

        #region 1 --- 通用类型消息
        public bool SendGeneralUds(byte[] data)
        {
            IsRunnning = true;
            bool ret = false;
            if (data.Length > 7)
            {
                ret = SendMuiltFrame(data);
            }
            else
            {
                if (!this.IsRunnning) return false;
                byte[] src = new byte[8];
                Buffer.BlockCopy(data, 0, src, 1, data.Length);
                src[0] = (byte)data.Length;
                Sid = data[0];
                SendMsg(EcuMessage.Request, data);
                _canService.Send(this.channelConfig, this._sourceId, src);
                ret = WaitOne(Sid, 5000);
                if ((rsp[0] >> 4) == 1)
                {
                    ret = flowdata.Count > flowLength;
                }
            }
            IsRunnning = false;
            return ret;
        }

        public bool SendGenranr(byte sid, byte[] data, string desc = "Session Diagnosis")
        {
            var mdata = new byte[] { sid };
            if (data.Length > 6)
            {
                if (this.IsCanfd && this.ConfdType > 0)
                {
                    mdata = new byte[data.Length > 13 ? 20 : 16];
                    mdata[0] = (byte)((data.Length + 1) >> 8);
                    mdata[1] = (byte)((data.Length + 1) & 0x0f);
                    mdata[2] = sid;
                    Buffer.BlockCopy(data, 0, mdata, 3, data.Length);
                    SendMsg(EcuMessage.Request, mdata, true);
                    _canService.Send(this.channelConfig, this._sourceId, mdata);
                    bool ret = WaitOne(sid, rspTime);
                    SendMsg(ret ? EcuMessage.Positive : EcuMessage.Negative, rsp, false);
                    return ret && rsp[1] == (0x40 + sid);
                }
                return SendMuiltFrame(mdata.Concat(data).ToArray(), rspTime);
            }
            return SendUds(mdata.Concat(data).ToArray());
        }
        public bool SendGenranr(ZFlashGener flashGener)
        {
            byte[] subdata = flashGener.Data.Split(' ').Where(u => u.Length > 0).Select(u => Convert.ToByte(u, 16)).ToArray();
            var sid = Convert.ToByte(flashGener.Sid, 16);
            return SendGenranr(sid, subdata, flashGener.Desc);
        }
        #endregion

        #region 2 --- 安全访问解密
        public bool UnlockSecrityAccess(ZFlashSecurity flashSecurity)
        {
            string dllpath = Path.Combine(_dllPath, flashSecurity.DllPath);
            return UnlockSecrityAccess(dllpath, (byte)flashSecurity.Level, flashSecurity.IgnoreUnlockedCheck, flashSecurity.Desc);
        }

        /// <summary>
        /// 基于DLL路径动态解密算法
        /// </summary>
        /// <param name="dllpath"> 路径</param>
        /// <param name="level">安全级别</param>
        /// <param name="ignoreunlocked">是否忽略</param>
        /// <param name="desc">功能描述</param>
        /// <returns></returns>
        public bool UnlockSecrityAccess(string dllpath, byte level, bool ignoreunlocked = false, string desc = "Security Access")
        {
            // 2. 安全解锁（27服务）
            if (!SendUds(new byte[] { 0x27, level }))
            {
                FlowProgress?.Invoke(-1, desc, rsp, "Unlock service request failed");
                return false; // Unlock failed
            }

            byte[] seedArray = new byte[rsp[0] - 2];
            Buffer.BlockCopy(rsp, 3, seedArray, 0, rsp[0] - 2);
            bool ok = DynamicKeyGen.UnlockSeek(dllpath, seedArray, level, out byte[] key); // 调用DLL算法
            if (!ok || !SendUds(new byte[] { 0x27, ++level }.Concat(key).ToArray()))
            {
                FlowProgress?.Invoke(-1, desc, new byte[] { 0x27, level }.Concat(key).ToArray(), "Request frame");
                FlowProgress?.Invoke(-1, desc, rsp, "Unlock failed!");
                return false;
            }
            return true;
        }
        #endregion

        #region 3 --- 文件下载
        /// <summary>
        /// 按文件配置项实现文件下载
        /// </summary>
        /// <param name="flashDown"></param>
        /// <returns></returns>
        public bool SendFlashDownFile(ZFlashDown flashDown)
        {
            return SendBlock3(flashDown.ZFlashBlock.Datas, flashDown.Addresssize, flashDown.Lengthsize,
                (byte)(flashDown.DataFormatIdentifier & 0xff), flashDown.NumID, flashDown.EraseEcuMemoryMode);
        }

        /// <summary>
        /// 文件擦除
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="size">长度大小</param>
        /// <param name="EraseMode">擦除方式</param>
        /// <returns></returns>
        private bool DoEraseFlash(byte addresslen, byte[] addr, byte[] size, int EraseMode = 3)
        {
            if (EraseMode == 3) // 不擦除
            {
                return true;
            }

            if (EraseMode == 0) // 带参数擦除
            {
                byte[] data = new byte[] { 0x31, 0x01, 0xFF, 0, addresslen };
                return SendMuiltFrame(data.Concat(addr).Concat(size).ToArray(), 10000);
            }

            if (EraseMode == 2) // 不带参数擦除
            {
                return SendUds(new byte[] { 0x31, 0x01, 0xff, 00 });
            }

            if (EraseMode == 1)
            {
                SendMsg("Block erase function not supported");
                return false;
            }
            // EraseMode 0 ---- 带参数擦除 31 01 ff 00 + 地址 + 长度 
            // EraseMode 1 ---- 分块擦除 31 01 ff 00 + 地址 + 长度 
            // EraseMode 2 ---- 不带参数擦除 31 01 ff 00
            // EraseMode 3 ---- 不擦除
            return true;
        }

        /// <summary>
        /// 文件发送
        /// </summary>
        /// <param name="dataBlock">文件块</param>
        /// <param name="addresslen">块地址长度</param>
        /// <param name="sizelen">块字节长度</param>
        /// <param name="reqParam">请求参数</param>
        /// <param name="zflashnumId">id</param>
        /// <param name="EraseMode">擦除模式</param>
        /// <returns></returns>
        private bool SendBlock3(DataBlock dataBlock, int addresslen = 4, int sizelen = 3, byte reqParam = 0, int zflashnumId = 0, int EraseMode = 0)
        {
            FlowProgress?.Invoke(zflashnumId, "File erasing...", null, "File erasing");
            byte address = (byte)(((sizelen & 0xf) << 4) + (addresslen & 0xf));//04
            // { 0x00, 0x08, 0x00, 0x00 }; // 起始地址0x08000000
            byte[] addr = BitConverter.GetBytes(dataBlock.Address).Reverse().Skip(4 - addresslen).ToArray();
            byte[] size = BitConverter.GetBytes(dataBlock.Data.Count).Reverse().Skip(4 - sizelen).ToArray();
            // 下载前擦除
            if (!DoEraseFlash(address, addr, size, EraseMode))
            {
                FlowProgress?.Invoke(-1, "File Download", null, "Parameter erase failed");
                return false;
            }
            // 发送文件大小
            bool result = SendFlowUds(new byte[] { 0x34, reqParam, address }.Concat(addr).Concat(size).ToArray());
            if (!result)
            {
                FlowProgress?.Invoke(-1, "File Download", rsp, "Request send file address length failed");
                return false;
            }
            int maxbyte = rsp[2] >> 4;
            int blocklen = (rsp[3] << 8) + rsp[4];
            int canlen = (this.IsCanfd && this.ConfdType >= 1) ? 64 : 8;
            var stopwatch = new Stopwatch();
            List<List<Frame>> datapaacket = SplitToBlocks(dataBlock.Data.ToArray(), blocklen, canlen);
            DownFileProgress?.Invoke(0, "", datapaacket.Count, 0);
            int bs = 20;
            int stmin = 0x7f;
            List<byte[]> muiltframe = new List<byte[]>();
            foreach (var mFrame in datapaacket)
            {
                Frame first = mFrame.First(); 
                this.IsReviceMsg = false; 
                this.rsp[0] = 0;
                if (!_canService.Send(this.channelConfig, this._sourceId, first.Data)) 
                {
                    FlowProgress?.Invoke(-1, "File Download", first.Data, "Flow control send First Frame is Faild!");
                    return false;
                }
                if (!WaitFlowOne(this.rspTime))
                {
                    FlowProgress?.Invoke(-1, "File Download First Frame", first.Data, "Flow control response timeout");
                    return false;
                }
                bs = rsp[1];
                stmin = rsp[2];
                int ybs = rsp[1];
                stopwatch.Restart();
                for (int i = 1; i < mFrame.Count; i += bs)
                {
                    if (!this.IsRunnning) return false;
                    int validbs = Math.Min(bs, mFrame.Count - i);
                    if (ybs == 0)
                    {
                        validbs = mFrame.Count - i;
                    }
                    this.IsReviceMsg = false;
                    this.rsp[0] = 0;
                    result = _canService.SendMuiltFrame(this.channelConfig, this._sourceId, mFrame, i, validbs);
                    if (!result)
                    {
                        FlowProgress?.Invoke(-1, "File Download First Frame", first.Data, "Consecutive frame transmission incomplete");
                        return false;
                    }
                    //if (ybs != 0 && ()
                    if (ybs != 0 && (validbs == bs && (validbs + i + 1) <= mFrame.Count))
                    {
                        if (!WaitFlowOne())
                        {
                            SendMsg($"{mFrame[i].Data[0].ToString("X2")} Flow control response timeout");
                            FlowProgress?.Invoke(-1, "File Download First Frame", first.Data, "Flow control response timeout");
                            return false;
                        }
                        bs = rsp[1];
                        stmin = rsp[2];
                        //Thread.Sleep(10);
                        muiltframe.Clear();
                    }
                    else
                    {
                        WaitOne(0x36, 3000, IsReviceMsg);
                        if (rsp[1] != 0x76 || rsp[2] != (first.BlockIndex % 256))
                        {
                            SendMsg($"{mFrame[i + validbs - 1].Data[1].ToString("X2")}");
                            return false;
                        }
                        if (ybs == 0) break;
                    }
                }
                stopwatch.Stop();
                SendMsg(result ? EcuMessage.Positive : EcuMessage.Negative, rsp, false);
                DownFileProgress?.Invoke(zflashnumId, $"Elapsed per block: {stopwatch.ElapsedMilliseconds}ms, Estimated {stopwatch.ElapsedMilliseconds * (datapaacket.Count - first.BlockIndex) / 60000 + 1}min", first.BlockIndex, (int)(100 * 1.0 * first.BlockIndex / datapaacket.Count));
            }
            return SendUds(new byte[] { UdsRequestTransferExit });
        }

        /// <summary>
        /// 拆包
        /// </summary>
        /// <param name="src"></param>
        /// <param name="maxlen"></param>
        /// <param name="canlen"></param>
        /// <returns></returns>
        public List<List<Frame>> SplitToBlocks(byte[] src, int maxlen, int canlen)
        {
            int BLOCK_PAY = maxlen - 2; // 载荷
            int srcLen = src.Length;
            int totalBlocks = (srcLen + BLOCK_PAY - 1) / BLOCK_PAY;
            var result = new List<List<Frame>>(totalBlocks);
            int vLen = canlen - 1;
            // 首帧固定长度字节
            for (int block = 0; block < totalBlocks; block++)
            {
                int srcOffset = block * BLOCK_PAY;
                int payLen = Math.Min(BLOCK_PAY, srcLen - srcOffset);
                var blockFrames = new List<Frame>(147); // 预估值

                // 首帧：2 B 固定长度 + 2 B 块索引 + 4 B 数据
                byte[] first = new byte[canlen];
                first[0] = (byte)(0x10 + (((payLen + 2) >> 8) & 0x0f));
                first[1] = (byte)((payLen + 2) & 0xff);
                first[2] = 0x36;
                first[3] = (byte)((block + 1) & 0xFF);
                Buffer.BlockCopy(src, srcOffset, first, 4, Math.Min(canlen - 4, payLen));
                // 不足4字节已补0
                blockFrames.Add(new Frame
                {
                    BlockIndex = (ushort)((block + 1)),
                    FrameIndex = 0,
                    Data = first
                });

                // 剩余数据 → 7 B 一帧（动态算）
                int remaining = payLen - (canlen - 4);
                int frames7 = (remaining + vLen - 1) / vLen;
                for (int f = 0; f < frames7; f++)
                {
                    int off = srcOffset + (canlen - 4) + f * vLen;
                    int len = Math.Min(vLen, remaining - f * vLen);
                    byte[] pkt = new byte[len > 7 ? len + 1 : 8];
                    pkt[0] = (byte)(((f + 1) & 0x0f) + 0x20);           // 帧索引 1..
                    Buffer.BlockCopy(src, off, pkt, 1, len);
                    // 不足7字节右侧已补0
                    blockFrames.Add(new Frame
                    {
                        BlockIndex = (ushort)((block + 1)),
                        FrameIndex = (byte)((f + 1) % 16 + 0x20),
                        Data = pkt
                    });
                }
                result.Add(blockFrames);
            }
            return result;
        }
        /// <summary>
        /// 获取连续帧
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="startPos">起始位置</param>
        /// <param name="vaildBlockLen">最大长度</param>
        /// <param name="minByteLen">发送最大长度</param>
        /// <param name="bs">连续发送帧数</param>
        /// <returns></returns>
        private List<byte[]> GetFrames(byte[] data, int pos, int vaildBlockLen, int canlen, int usecount = 4)
        {
            int bipos = 0;
            int cf = 0x21;
            List<byte[]> freames = new List<byte[]>();
            do
            {
                byte[] minBytes = new byte[canlen];
                int bVaildLen = Math.Min(canlen - 1, vaildBlockLen - usecount - bipos);
                if (cf > 0x2f)
                {
                    cf = 0x20;
                }
                minBytes[0] = (byte)cf;
                Array.Copy(data, pos + bipos, minBytes, 1, bVaildLen);
                freames.Add(minBytes);
                bipos += (canlen - 1);
                cf++;
            } while ((vaildBlockLen - bipos - 1) > 0);
            return freames;
        }
        #endregion

        #region 0 --- 消息分发

        /// <summary>
        /// Zflash测试项
        /// </summary>
        /// <param name="zFlash">flash模块</param>
        /// <returns></returns>
        public bool SendZFlashItem(ZFlashItem zFlash)
        {
            if (zFlash == null) return false;
            bool result = false;
            SendMsg(zFlash.NumID + "." + zFlash.Desc);
            FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, null, "start running...");
            try
            {
                zFlash.Status = ZFlashItemStatus.Running;
                if (zFlash is ZFlashGener)
                {
                    result = SendGenranr(zFlash as ZFlashGener);
                }
                else if (zFlash is ZFlashSecurity)
                {
                    result = UnlockSecrityAccess(zFlash as ZFlashSecurity);
                }
                else if (zFlash is ZFlashExdFunc)
                {
                    IsCheckVersion = rsp[0] == 5 && rsp[1] == 0x71 && rsp[5] == 1;
                    FlowProgress?.Invoke(-1, zFlash.Desc, null, IsCheckVersion? "check version is success!" : "this version is Upgrade  faild!");
                    result = IsCheckVersion;
                }
                else if (zFlash is ZFlashDown)
                {
                    result = SendFlashDownFile(zFlash as ZFlashDown);
                }
                if (zFlash is ZFlashDelay)
                {
                    ZFlashDelay zFlashDelay = zFlash as ZFlashDelay;
                    Thread.Sleep(zFlashDelay.DelayTime);
                    result = true;
                }
                
                zFlash.Status = result ? ZFlashItemStatus.Finshed : ZFlashItemStatus.Faild;
                if (!result)
                {
                    SendMsg($"{zFlash.NumID}.{zFlash.Desc}-NPC:{rsp[0]:X2} {rsp[1]:X2} {rsp[2]:X2} {rsp[3]:X2}", true);
                    FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, rsp, "");
                    return false;
                }
                FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, null, zFlash.Desc + " is running finisned!" );
                return result;
            }
            catch (Exception ex)
            {
                SendMsg(zFlash.NumID + "." + zFlash.Desc + " Exception: " + ex.Message, true);
                FlowProgress?.Invoke(zFlash.NumID, zFlash.Desc, rsp, ex.Message);
                zFlash.Status = ZFlashItemStatus.Faild;
            }
            finally
            {

            }
            return false;
        }

        /// <summary>
        /// 执行烧录流程
        /// </summary>
        /// <param name="flashItems">UDS测试项</param>
        /// <param name="mNotifyMsg">UDS直接接收事件</param>
        /// <param name="Progress">文件块上传事件</param>
        public async Task<bool> StartBootloader(List<ZFlashItem> flashItems, NotifyMessageEvent mNotifyMsg, DownFileProgressEvent Progress)
        {
            this.NotifyMsg -= mNotifyMsg;
            this.DownFileProgress -= Progress;
            this.NotifyMsg += mNotifyMsg;
            this.DownFileProgress += Progress;
            this.IsRunnning = true;
            return await Task.Run(() => {
                bool result = false;
                foreach (ZFlashItem flashItem in flashItems)
                {
                    result = SendZFlashItem(flashItem);
                    if (!result)
                    {
                        break;
                    }
                }
                SendMsg(result ? "Zflash programming completed.." : "Programming failed!");
                this.NotifyMsg -= mNotifyMsg;
                this.DownFileProgress -= Progress;
                this.IsRunnning = false;
                return result;
            });
        }
        /// <summary>
        /// 执行烧录流程
        /// </summary>
        /// <param name="flashItems">UDS测试项</param>
        /// <param name="mNotifyMsg">UDS直接接收事件</param>
        /// <param name="Progress">文件块上传事件</param>
        public async Task<bool> StartEcu(List<ZFlashItem> flashItems, DownFileProgressEvent Progress)
        {
            this.DownFileProgress -= Progress;
            this.DownFileProgress += Progress;
            this.IsRunnning = true;
            return await Task.Run(() => {
                bool result = false;
                foreach (ZFlashItem flashItem in flashItems)
                {
                    result = SendZFlashItem(flashItem);
                    if (!result)
                    {
                        break;
                    }
                }
                if (result) 
                {
                    FlowProgress?.Invoke(flashItems.Count+1, "Upgrade App", rsp, "this Version is Upgrade success!");
                }
                this.DownFileProgress -= Progress;
                this.IsRunnning = false;
                return result;
            });
        }
        public async Task StopBootloader()
        {
            this.IsRunnning = false;
            await Task.Delay(5000);
        }
        private volatile bool IsRunnning = false;
        #endregion
    }
}
