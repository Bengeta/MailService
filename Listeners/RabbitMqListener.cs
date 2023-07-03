using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Interfaces;
using RabbitMQ.Client.Exceptions;
using Requests;

namespace Listeners;
public class RabbitMqListener : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;
    private readonly IPhoneRepository _phoneRepository;
    private readonly IMailRepository _mailRepository;
    private readonly IConfiguration _configuration;

    public RabbitMqListener(IPhoneRepository phoneRepository, IMailRepository mailRepository, IConfiguration configuration)
    {
        _phoneRepository = phoneRepository;
        _mailRepository = mailRepository;
        _configuration = configuration;
        var factory = new ConnectionFactory()
        {
            HostName = configuration.GetValue<string>("RabbitMQHost"),
            Port = configuration.GetValue<int>("RabbitMQPort"),
            UserName = configuration.GetValue<string>("RabbitUser"),
            Password = configuration.GetValue<string>("RabbitPass"),
            DispatchConsumersAsync = true,
        };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            _channel.QueueDeclare(queue: "VerifyCodeQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании подключения и канала RabbitMQ: {ex.Message}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (ch, ea) =>
        {
            try
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());

                Console.WriteLine($"Получена очередная заявка: {content}");

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve,
                    MaxDepth = 64
                };

                var request = JsonSerializer.Deserialize<SendVerificationCodeRequest>(content, options);

                Console.WriteLine($"Code: {request.Code} Phone: {request.Phone}");

                if (request.IsMail)
                    _mailRepository.SendVerificationCode(request);
                else
                    _phoneRepository.SendVerificationCode(request);

                Console.WriteLine($"Получено сообщение: {content}");

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке сообщения: {ex.Message}");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        try
        {
            _channel.BasicConsume("VerifyCodeQueue", false, consumer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при старте потребителя RabbitMQ: {ex.Message}");
            throw;
        }

        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        try
        {
            _channel.Close();
        }
        catch (AlreadyClosedException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии канала RabbitMQ: {ex.Message}");
        }

        try
        {
            _connection.Close();
        }
        catch (AlreadyClosedException) { }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при закрытии соединения RabbitMQ: {ex.Message}");
        }

        base.Dispose();
    }
}