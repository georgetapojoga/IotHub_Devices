using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessageLibrary;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Pojoga.TwinDevice
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("Start device at: {time}", DateTimeOffset.Now);

            var deviceClient = DeviceClient.CreateFromConnectionString(_configuration.GetConnectionString("iothub"), TransportType.Amqp);

            var twin = await deviceClient.GetTwinAsync();
            var color = twin.Properties.Desired["color"];
            Console.BackgroundColor = color;
            Console.Clear();

            var reported = new TwinCollection();
            reported["color"] = Console.BackgroundColor.ToString();

            //aggiorno il il reported
            await deviceClient.UpdateReportedPropertiesAsync(reported);

            while (!stoppingToken.IsCancellationRequested)
            {


                var message = await deviceClient.ReceiveAsync(TimeSpan.FromSeconds(30));

                if (message != null)
                {
                    var text = System.Text.UTF8Encoding.UTF8.GetString(message.GetBytes());
                    var json = JsonConvert.DeserializeObject<Messaggio>(text);
                    Console.WriteLine("Messaggio: " + json.Text);
                    int colorText = json.ColorText;
                    Console.ForegroundColor = (ConsoleColor)colorText;

                }
                else
                {
                    await Task.Delay(2000, stoppingToken);

                }
            }
        }

    }
}
