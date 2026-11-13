using System.Collections.Generic;

namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public class UdsConfig
    {
        public List<UdsItem> services { get; set; }
    }
    public class UdsItem
    {
        public bool enable { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public List<UdsSubitem> subitems { get; set; }

        public override string ToString()
        {
            return $"({id}){name}";
        }
    }

    public class UdsSubitem
    {
        public string data { get; set; }
        public string desc { get; set; }
        public bool isIdInvalid { get; set; }
        public string number { get; set; }
        public override string ToString()
        {
            return $"{number} {desc}";
        }
    }
}
