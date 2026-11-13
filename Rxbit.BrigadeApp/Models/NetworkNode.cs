

namespace Rxbit.BrigadeApp.Models
{
    public class NetworkNode
    {
        public string CanId { get; set; }
        public string ObjectId { get; set; }
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
        public bool IsConnected { get; set; }
    }

    public class SensorErrorData
    {
        public string SensorId { get; set; }
        public int Error5 { get; set; }
        public int Error6 { get; set; }
        public string Status { get; set; }
    }
}
