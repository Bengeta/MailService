using System.Buffers;
using Interfaces;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using Models;
using System.Text;

namespace MailStuff;
public class MyMessageStore : MessageStore
{
    private readonly IRabbitMqService _rabbitMqService;
    public MyMessageStore(IRabbitMqService rabbitMqService)
    {
        Console.WriteLine("конструктор стора");
        _rabbitMqService = rabbitMqService;
    }

    public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
    {
        try
        {
            return SmtpResponse.Ok;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return SmtpResponse.Ok;
    }
}
