using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace trafficStreamer.Services
{
    public class WriteStreamService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}