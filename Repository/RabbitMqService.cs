using Interfaces;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Repository;
public class RabbitMqService : IRabbitMqService
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentQueue<IConnection> _connectionPool;
    private readonly object _lock = new object();

    public RabbitMqService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionPool = new ConcurrentQueue<IConnection>();
    }

    private IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration.GetValue<string>("RabbitMQHost"),
            Port = _configuration.GetValue<int>("RabbitMQPort"),
            UserName = _configuration.GetValue<string>("RabbitUser"),
            Password = _configuration.GetValue<string>("RabbitPass")
        };

        return factory.CreateConnection();
    }

    private IConnection GetConnection()
    {
        if (_connectionPool.TryDequeue(out var connection))
        {
            if (IsConnectionValid(connection))
                return connection;
            else
                connection.Dispose();
        }

        return CreateConnection();
    }

    private void ReturnConnection(IConnection connection)
    {
        _connectionPool.Enqueue(connection);
    }

    private bool IsConnectionValid(IConnection connection)
    {
        // Perform any validation to check if the connection is still valid
        return connection?.IsOpen == true;
    }

    public async Task SendMessage(object obj, List<string> queueList)
    {
        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            MaxDepth = 64
        };
        var message = JsonSerializer.Serialize(obj, options);
        foreach (var queue in queueList)
            await SendMessage(message, queue);
    }

    public async Task SendMessage(string message, string queueName)
    {
        try
        {
            var retryPolicy = Policy
                .Handle<BrokerUnreachableException>()
                .RetryAsync(3, async (ex, retryCount) =>
                {
                    Console.WriteLine($"RabbitMQ connection error: {ex.Message}");
                    Console.WriteLine($"Retrying connection ({retryCount})...");
                    await Task.Delay(1000); // Pause before retrying
                });

            await retryPolicy.ExecuteAsync(async () =>
            {
                using (var connection = GetConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var body = Encoding.UTF8.GetBytes(message);

                    await Task.Run(() => channel.BasicPublish("", queueName, null, body));

                    Console.WriteLine("Message sent successfully.");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

}