using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;

namespace MailStuff;
public class MyMailboxFilter : IMailboxFilter, IMailboxFilterFactory
{
    public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox @from, int size, CancellationToken cancellationToken)
    {
        // if (String.Equals(@from.Host, "test.com"))
        // {
        //     return Task.FromResult(MailboxFilterResult.Yes);
        // }
        
        // return Task.FromResult(MailboxFilterResult.NoPermanently);
        return Task.FromResult(MailboxFilterResult.Yes);
    }

    public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox @from, CancellationToken token)
    {
        return Task.FromResult(MailboxFilterResult.Yes);
    }

    public IMailboxFilter CreateInstance(ISessionContext context)
    {
        return new MyMailboxFilter();
    }
}
