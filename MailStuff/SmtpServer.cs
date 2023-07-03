using Interfaces;
using Repository;
using SmtpServer;
using SmtpServer.Net;
using SmtpServer.Storage;
using SmtpServer.Tracing;

namespace MailStuff;
public class MySmtpServer
{
    private readonly IConfiguration _configuration;
    public MySmtpServer(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task RunAsync()
    {
        var options = new SmtpServerOptionsBuilder()
            .ServerName(_configuration.GetValue<string>("ServerName"))
            .Port(25, 587)
            .Build();
        
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IMailboxFilter, MyMailboxFilter>()
            .AddSingleton<IMessageStore, MyMessageStore>()
            .AddSingleton<IRabbitMqService, RabbitMqService>()
            .AddSingleton<IConfiguration>(_configuration)
            .BuildServiceProvider();

        var smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);
        
        smtpServer.SessionCreated += OnSessionCreated;
            smtpServer.SessionCompleted += OnSessionCompleted;
            smtpServer.SessionFaulted += OnSessionFaulted;
            smtpServer.SessionCancelled += OnSessionCancelled;
            
        await smtpServer.StartAsync(CancellationToken.None);
    }
    
     static void OnSessionFaulted(object sender, SessionFaultedEventArgs e)
        {
            Console.WriteLine("SessionFaulted: {0}", e.Exception);
        }

        static void OnSessionCancelled(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCancelled");
        }

        static void OnSessionCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCreated: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting += OnCommandExecuting;
            e.Context.CommandExecuted += OnCommandExecuted;
        }

        static void OnCommandExecuting(object sender, SmtpCommandEventArgs e)
        {
            Console.WriteLine("Command Executing");
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnCommandExecuted(object sender, SmtpCommandEventArgs e)
        {
            Console.WriteLine("Command Executed");
            new TracingSmtpCommandVisitor(Console.Out).Visit(e.Command);
        }

        static void OnSessionCompleted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("SessionCompleted: {0}", e.Context.Properties[EndpointListener.RemoteEndPointKey]);

            e.Context.CommandExecuting -= OnCommandExecuting;
            e.Context.CommandExecuted -= OnCommandExecuted;
        }
}
