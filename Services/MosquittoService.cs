using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using trafficStreamer.Common;

namespace trafficStreamer.Services
{
    public class MosquittoService : BackgroundService
    {
        private readonly Options _options;
        private readonly IMqttClient _client;
        private readonly EventStorage _storage;
        private readonly ILogger<MosquittoService> _logger;
        private const string ClientId = "MQTT broker tester";

        public MosquittoService(Options options, IMqttClient client, EventStorage storage, ILogger<MosquittoService> logger)
        {
            _options = options;
            _client = client;
            _storage = storage;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // connect to broker
            var connectOptions = new MqttClientOptionsBuilder()
                    .WithClientId(ClientId)
                    .WithTcpServer(_options.Host, _options.Port)
//                    .WithCredentials(_options.User, _options.Password)
//                    .WithTls()
//                    .WithCleanSession()
                    .Build();

            _client.UseConnectedHandler(async e =>
            {
                // Subscribe to a topic
                await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(_options.Topic).Build());
            });

            if (_options.Command == Commands.Write)
            {
                _client.UseApplicationMessageReceivedHandler(ev =>
                {
                    // dump messages to storage
                    _storage.StoreEvent(
                        new TrafficEvent
                        {
                            Topic = ev.ApplicationMessage.Topic,
                            Datetime = DateTime.Now,
                            Payload = ev.ApplicationMessage.Payload
                        } );

                    _logger.LogInformation("### RECEIVED APPLICATION MESSAGE ###");
                    _logger.LogInformation($"+ Topic = {ev.ApplicationMessage.Topic}");
                    _logger.LogInformation($"+ Payload = {Encoding.UTF8.GetString(ev.ApplicationMessage.Payload)}");
                    _logger.LogInformation($"+ QoS = {ev.ApplicationMessage.QualityOfServiceLevel}");
                    _logger.LogInformation($"+ Retain = {ev.ApplicationMessage.Retain}");
                });
            }

            try
            {
                await _client.ConnectAsync(connectOptions, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error when connecting to broker  {_options.Host}:{_options.Port}{_options.Topic}");
                return;
            }

            await StartProcessing(stoppingToken);
        }

        private async Task StartProcessing(CancellationToken stoppingToken)
        {
            while (true)
            {
                switch (_options.Command)
                {
                    case Commands.Write:

                        if (stoppingToken.IsCancellationRequested)
                        {
                            _storage.FlushData();
                            _logger.LogInformation($"connection to broker was canceled: {_options.Host}:{_options.Port}{_options.Topic}");
                            break;
                        }

                        await Task.Delay( 100, stoppingToken);  // switch context to avoid hang

                        continue;
                    case Commands.Play:

                        _storage.GetEvents( async (eventCurrent, eventNext) =>
                        {
                            var duration = eventCurrent.Datetime - eventNext.Datetime;

                            await Task.Delay( duration.Multiply(_options.PlayRate ?? 0), stoppingToken);

                            await _client.PublishAsync( new MqttApplicationMessage
                                {
                                    Topic = eventCurrent.Topic,
                                    Payload = eventCurrent.Payload
                                }
                            );
                            _logger.LogInformation($"topic: {eventCurrent.Topic} push event: {Encoding.UTF8.GetString(eventCurrent.Payload)}");
                        }, stoppingToken);
                        _logger.LogInformation($"All events are played..");
                        break;
                    default:
                        _logger.LogError($"Received unknown command: {_options.Command}");
                        break;
                }
                break;
            }
        }
    }
}
