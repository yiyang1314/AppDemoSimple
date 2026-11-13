namespace Rxbit.Brigade.EcuServices.Models
{
    public class EcuPsConfig
    {
        public int PsServerTime { get; set; } = 3000;
        public int CsServerTime { get; set; } = 3000;

        public int Bs { get; set; } = 3000;

        public int Stmin { get; set; } = 100;
    }
}
