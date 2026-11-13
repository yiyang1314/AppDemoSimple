
namespace Rxbit.Brigade.EcuServices.ZflashItems
{
    public enum ZFlashUdsEnum
    {
        Setting = 0,
        DealyTime = 1,
        General = 2,
        SecrityAcess = 0x27,
        FileDown = 0x34,
    }
    public enum ZFlashItemStatus
    {
        Not_Start = 0,
        Running = 1,
        Finshed = 2,
        Faild = 3
    }
}
