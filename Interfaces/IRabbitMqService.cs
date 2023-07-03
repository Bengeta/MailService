namespace Interfaces;
public interface IRabbitMqService
{
    public Task SendMessage(object obj, List<string> queueList);
    public Task SendMessage(string message, string queueName);
}
