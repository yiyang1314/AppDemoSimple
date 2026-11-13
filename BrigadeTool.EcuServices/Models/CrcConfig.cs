namespace Rxbit.Brigade.EcuServices.Models
{
    /// <summary>
    /// CRC配置类（对应界面参数）
    /// </summary>
    public class CrcConfig
    {
        /// <summary>
        /// CRC类型（8/16/32）
        /// </summary>
        public CrcType Type { get; set; }

        /// <summary>
        /// 多项式（如CRC8填0x7）
        /// </summary>
        public uint Polynomial { get; set; }

        /// <summary>
        /// 初始值
        /// </summary>
        public uint InitialValue { get; set; }

        /// <summary>
        /// 是否启用输入反转（每个字节按位反转后参与计算）
        /// </summary>
        public bool InputReverse { get; set; }

        /// <summary>
        /// 是否启用输出反转（最终结果按位反转）
        /// </summary>
        public bool OutputReverse { get; set; }

        /// <summary>
        /// 输出异或值
        /// </summary>
        public uint XorOutput { get; set; }
    }
}
