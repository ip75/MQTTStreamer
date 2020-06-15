using System.Collections.Generic;
using MQTTnet;

namespace trafficStreamer
{
    public class MessageQueue : Queue<MqttApplicationMessage>
    {
        private long _maxQueueSize;
        public MessageQueue(long maxQueueSize = 1000)
        {
            _maxQueueSize = maxQueueSize;
        }
    }
}
