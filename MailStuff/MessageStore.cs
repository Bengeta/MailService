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
            Console.WriteLine("Пошел сейв");
            await using var stream = new MemoryStream();

            var position = buffer.GetPosition(0);
            while (buffer.TryGet(ref position, out var memory))
            {
                await stream.WriteAsync(memory, cancellationToken);
            }

            stream.Position = 0;

            var message = await MimeKit.MimeMessage.LoadAsync(stream, cancellationToken);
            
            if (message.Attachments.FirstOrDefault() == null)
            {
                return SmtpResponse.Ok;
            };
            
            await using var newStream = new MemoryStream();
            message.Attachments.First().WriteTo(newStream);
            var base64 = System.Text.Encoding.Default.GetString(newStream.ToArray()).Split("\n\n")[1];
            var attach = Convert.FromBase64String(base64);
            
            var MailAnalysisResponse = new MailAnalysisResponse()
            {
                orderId = int.Parse(message.Subject),
                attachment = attach,
            };

            var queueList = new List<string>() { "AnalysisResponseQueue" };
            _rabbitMqService.SendMessage(MailAnalysisResponse, queueList);

            return SmtpResponse.Ok;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return SmtpResponse.Ok;
    }
}
