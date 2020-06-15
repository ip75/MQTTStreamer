using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using Serilog;
using trafficStreamer.Services;

namespace trafficStreamer
{
    public static class Runner
    {
        public static void Launch(Options options)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) => {
                    config.AddEnvironmentVariables();

                    //if (args != null)
                    //{
                    //    config.AddCommandLine(args);
                    //}
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MosquittoService>();
                    services.AddSingleton<EventStorage>();

                    services.AddSingleton(options);
                    services.AddSingleton(new MqttFactory().CreateMqttClient());
                })
                .ConfigureLogging(configureLogging: (hostingContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.WithMachineName().Enrich.WithProcessId().Enrich.WithProcessName()
                        .WriteTo.ColoredConsole()
                        .CreateLogger();
                    logging.AddSerilog(dispose: true);
                })
                .UseSerilog();

//            await builder.RunConsoleAsync();
//            await builder.Build().RunAsync();
            builder.Build().Run();
        }
    }
}
