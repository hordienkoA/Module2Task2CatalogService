using CartService.BLL.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CartService.BLL.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;
        public RabbitMqPublisher(string host, string username, string password)
        {
            _factory = new ConnectionFactory
            {
                HostName = host,
                UserName = username,
                Password = password
            };
        }

        public async Task Publish<T>(T message, string queueName)
        {
            await using var connection = await _factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: body);

        }
    }
}
