using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MQTTnet;

namespace trafficStreamer.Services
{
    public class PlayStreamService : BackgroundService
    {
        private readonly MessageQueue _queue;

        public PlayStreamService(MessageQueue queue)
        {
            _queue = queue;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                // TODO: get event data from storage and put it to queue

                _queue.Enqueue(new MqttApplicationMessage());
            }
        }
    }
}