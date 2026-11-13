using Rxbit.Brigade.Models.EnumEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.BrigadeApp.Models
{
    /// <summary>
    /// Sensor工具类
    /// </summary>
    public static class BrigadeHeler
    {
        private static  Dictionary<uint, uint > CurrDirs = new Dictionary<uint, uint>();

        private static Dictionary<uint, byte> subDirs = new Dictionary<uint, byte>();

        private static Dictionary<byte, uint> TxDirs = new Dictionary<byte, uint>();

        static BrigadeHeler() 
        {
            CurrDirs.Clear();
            subDirs.Clear();
            TxDirs.Clear();
            uint gen1 = 0x310;
            uint gen2 = 0x210;

            CurrDirs.Add(0x390, 0x158);
            for (uint i = 0; i < 8; i++) 
            {
                CurrDirs.Add(gen1 + i*16, 0x150 +i);
                CurrDirs.Add(gen2 + i * 16, 0x200 + i);
                subDirs.Add(gen1 + i * 16, (byte)(i + 1));

            }
            subDirs.Add(0x390, 0x09);
            subDirs.Add(0x210, 0x0B);
            subDirs.Add(0x220, 0x0C);
            subDirs.Add(0x230, 0x0D);
            subDirs.Add(0x240, 0x0E);
            subDirs.Add(0x250, 0x0F);
            subDirs.Add(0x260, 0x10);
            subDirs.Add(0x270, 0x11);
            subDirs.Add(0x280, 0x12);

            for (byte i = 1; i <= 9; i++)
            {
                TxDirs.Add((byte)(i + 1), (uint)(0x310 + i * 16));
            }
            uint START = 0x210;
            TxDirs.Add(0x0B, START);
            TxDirs.Add(0x0C, START+=16);
            TxDirs.Add(0x0D, START+=16);
            TxDirs.Add(0x0E, START+=16);
            TxDirs.Add(0x0F, START+=16);
            TxDirs.Add(0x10, START+=16);
            TxDirs.Add(0x11, START+=16);
            TxDirs.Add(0x12, START+=16);
        }
        public static bool GetSensor(uint CurrSensoiID, uint NewSensoiID,ref uint canID,ref byte sid)
        {
            canID = 0u;
            sid = 0;
            if (!CurrDirs.ContainsKey(CurrSensoiID) || !subDirs.ContainsKey(NewSensoiID)) 
            {
                return false;
            }
            canID = CurrDirs[CurrSensoiID];
            sid = subDirs[NewSensoiID];
            return true;
        }

        public static string  GetGen1(byte Id) 
        {
            return string.Format("Object→{0}", (Id >> 4)& 0x0F);
        }


        public static string GetBaseId(byte Id)
        {
            if (!TxDirs.ContainsKey(Id)) return "Unknow";
            return string.Format("Configure Base lD →0x{0:x}", TxDirs[Id]);
        }
        public static string GetGen2N0(uint cid, ushort frameId)
        {
            return string.Format("FrameID→{0:}", frameId);
        }
        public static string GetGen2N1( byte Id)
        {
            return string.Format("Object→{0}", Id);
        }
        internal static string GetGen2N2(uint ID, byte v)
        {
            return string.Format("{0} - {1} ", RadarConstaint.MoveString[v&0x07], RadarConstaint.ClsassCate[v & 0x03]);
        }
    }
}
