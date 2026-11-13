using System;

namespace Rxbit.Brigade.CanDevices.Config
{
    public class UniformFileNaming
    {
        public static String GetFileName(ChannelConfig ccc)
        {
            return $"{ccc.CCategroy}_{ccc.CanIndex}_{ccc.ChannelIndex}.asc";
        }

        //public static String GetFileName(NetConnConfig ncc, FileType fi)
        //{
        //    return $"{ncc.Location}_{ncc.DCategory}_{ncc.DeviceIndex}_{ncc.ComputerWhere}.{fi}";
        //}

        public enum FileType
        {
            bin,
            json,
            blf,
            asc,
            csv
        }
    }
}
