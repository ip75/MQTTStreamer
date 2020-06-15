using System;

namespace trafficStreamer.Common
{
    public class TrafficEvent
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public byte [] Payload { get; set; }
        public DateTime Datetime { get; set; }
    }
}
