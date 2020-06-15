using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace trafficStreamer
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option('t', "topic", Default = "/traffic/+/ta_events/#", Required = false, HelpText = "Topic to subscribe in MQTT Mosquitto.")]
        public string Topic { get; set; }
        [Option('f', "file", Default = "events.db", Required = false, HelpText = "File to operate.")]
        public string File { get; set; }
        [Option('c', "command", Default = "WRITE", Required = false, HelpText = "Command to run. WRITE - save stream with specified topic to file, PLAY - push to broker Mosquitto data from file.")]
        public string Command { get; set; }
        [Option('r', longName: "rate", Default = 0, Required = false, HelpText = "Speed of playing data from saved file. 0 - play data without any delay. Applicable only for PLAY command.")]
        public double? PlayRate { get; set; }
        [Option('h', longName: "host", Required = true, HelpText = "Host of Mosquitto broker where connect to.")]
        public string Host { get; set; }
        [Option('p', longName: "port", Default = 1883, Required = false, HelpText = "Port of Mosquitto broker where connect to. \n1883 : MQTT, unencrypted\n8883 : MQTT, encrypted\n8884 : MQTT, encrypted, client certificate required\n8080 : MQTT over WebSockets, unencrypted\n8081 : MQTT over WebSockets, encrypted")]
        public int Port { get; set; }
        [Option('u', longName: "user", Default = null, Required = false, HelpText = "Username to connect to Mosquitto broker.")]
        public string User { get; set; }
        [Option('w', longName: "password", Default = null, Required = false, HelpText = "Password to connect to Mosquitto broker.")]
        public string Password { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var errors = new List<Error>();
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed((options) =>
                {
                    Runner.Launch(options);
                })
                .WithNotParsed(err => { errors = err.ToList(); });

            if (errors.Any())
            {
                Console.WriteLine(HelpText.AutoBuild(result));
            }
        }
    }
}
