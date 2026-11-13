using System.Collections.Generic;

namespace Rxbit.Brigade.EcuServices.Models
{
    public class DataLine
    {
        public int address;
        public string data;
        public int count;   //每行数据的个数，偏移量用于计算下一个地址   
    }

    public class DataBlock
    {
        public int Address;
        public List<byte> Data;
        public DataBlock()
        {
            Data = new List<byte>();
        }
    }
}
