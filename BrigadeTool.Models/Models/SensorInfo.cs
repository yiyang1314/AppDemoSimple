using Rxbit.Brigade.Models.EnumEntitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.Brigade.Models.Models
{
    public class SensorInfo : ViewModelBase
    {
        public uint CanID { get; set; }
        public string Desc => GetDesc();
        public string TypeDesc => GetTypeDesc();
        private GenType _Type  = GenType.Gen1Type;

        public GenType Type
        {
            get { return _Type; }
            set 
            { 
                SetProperty(ref _Type,value);
                OnPropertyChanged(nameof(TypeDesc));
            }
        }
        public bool IsVaild => Type == GenType.Gen1Type || Type == GenType.Gen2Type;

        public string GetDesc()
        {
            if (Type == GenType.Gen1Type || Type == GenType.Gen2Type|| Type == GenType.GenNoInit)
            {
                return string.Format("0x{0:x} ", this.CanID);
            }

            if (Type == GenType.Gen1XType || Type == GenType.Gen2XType)
            {
                return string.Format(" {0} - {1}XX ", this.Type, this.CanID >> 8);
            }
            return " -- None --";
        }

        public string GetTypeDesc()
        {
            if (Type == GenType.Gen1Type || Type == GenType.Gen2Type )
            {
                return string.Format("Gen2Type {0} ", ((int)Type));
            }
            if (Type == GenType.GenNoInit)
            {
                return " -- GenType NoInit --";
            }
            if (Type == GenType.Gen1XType || Type == GenType.Gen2XType)
            {
                return string.Format(" {0} - {1}XX ", this.Type, this.CanID >> 8);
            }
            return " -- None --";
        }
        public static List<SensorInfo> CreateData()
        {
            var list = new List<SensorInfo>();
            uint canID = 0x300;
            list.Add(new SensorInfo { CanID = 0, Type = GenType.Gen1None, });
            list.Add(new SensorInfo { CanID = canID, Type = GenType.Gen1XType, });
            for (int i = 1; i < 9; i++)
            {
                list.Add(new SensorInfo { CanID = (uint)(canID + 16 * i), Type = GenType.Gen1Type, });
            }
            list.Add(new SensorInfo { CanID = 0x390, Type = GenType.GenNoInit, });
            canID = 0x200;
            list.Add(new SensorInfo { CanID = canID, Type = GenType.Gen2XType, });
            for (int i = 1; i < 9; i++)
            {
                list.Add(new SensorInfo { CanID = (uint)(canID + 16 * i), Type = GenType.Gen2Type, });
            }
            return list;
        }
    }

    public class SensorGroup
    {
        public string GroupName { get; set; }
        public List<SensorInfo> Sensors { get; set; } = new List<SensorInfo>();
    }
}
