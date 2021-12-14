using DataCommunicator.Context;
using DataCommunicator.DTOs;
using DataCommunicator.Models;
using DataCommunicator.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace DataCommunicator
{
    public class RabbitManager
    {
        private Timer _timer;
        EventingBasicConsumer consumer;
        IConnection _connection;
        IModel _channel;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitManager> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitManager(IConfiguration configuration, ILogger<RabbitManager> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _timer = new Timer(5000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var dbService = scope.ServiceProvider.GetRequiredService<DbService>();
            var name = Dns.GetHostName();
            var ip = Utils.Utils.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Ethernet);

            if (string.IsNullOrEmpty(ip))
            {
                ip = Utils.Utils.GetLocalIPv4(System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211);
            }

            var config = new SystemConfig()
            {
                Ip = ip,
                HostName = name
            };

            dbService.UpdateSystemConfiguration(config);

            if (_connection == null)
            {
                RetryConnection();
            }
            else if (!_connection.IsOpen)
            {
                consumer.Received -= Consumer_Received;
                _connection.Dispose();
                RetryConnection();
            }
        }

        private void RetryConnection()
        {
            try
            {
                var ip = _configuration.GetSection("RabbitServerIP").Value;
                var factory = new ConnectionFactory() { HostName = ip, UserName = "admin", Password = "admin" };
                _connection = factory.CreateConnection();

                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: "config_module",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                _channel.ExchangeDeclare("amq.topic", "topic", true, false);

                _channel.QueueBind("config_module", "amq.topic", "#", null);

                consumer = new EventingBasicConsumer(_channel);

                consumer.Received += Consumer_Received;

                _channel.BasicConsume(queue: "config_module",
                                         autoAck: true,
                                         consumer: consumer);
                _logger.LogInformation($"RabbitMQ Connected. IP: {ip}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when connecting with RabbitMQ IP: {_configuration.GetSection("RabbitServerIP").Value}", ex);
            }
        }

        private async void Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);

                var module = JsonSerializer.Deserialize<ModulesDTO>(message);

                using var scope = _serviceScopeFactory.CreateScope();
                var myScopedService = scope.ServiceProvider.GetService<DbService>();
                await myScopedService.AddNewModuleAsync(module);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error when receive RabbitMQ nessage", ex);
            }
        }
    }
}
