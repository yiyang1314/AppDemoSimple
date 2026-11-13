using System;
using System.Collections.Generic;

namespace Rxbit.BrigadeApp.Models
{
    public class DetectionObject
    {
        public int ObjectId { get; set; }
        public double Distance { get; set; }
        public double Angle { get; set; }
        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
        public double RelativeSpeed { get; set; }
        public double SignalLevel { get; set; }
        public int ObjectLevel { get; set; }
        public string AppearanceStatus { get; set; }
        public string TriggerEvent { get; set; }
        public string DetectionFlag { get; set; }
        public string IdentificationFlag { get; set; }
        public string VoltageFault { get; set; }
        public string MmicFault { get; set; }
        public string TemperatureFault { get; set; }
        public string CanFault { get; set; }
        public string SensorType { get; set; }
    }

    public class SensorDetectionData
    {
        public string SensorId { get; set; }
        public List<DetectionObject> Objects { get; set; } = new List<DetectionObject>();
    }
}
