using System;
using System.Net;
using System.Net.Mail;
using Enums;
using Interfaces;
using Models;
using Requests;
namespace Repository;
public class MailRepository : IMailRepository
{
    private readonly IConfiguration _configuration;
    public MailRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ResponseModel<bool>> SendVerificationCode(SendVerificationCodeRequest request)
    {
        try
        {
            var smtpHost = _configuration.GetValue<string>("ServerName");
            var smtpPort = 25;

            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = false;

            MailMessage message = new MailMessage();
            message.Subject = "Подтверждение почты";
            message.Body = "Код подтверждения: " + request.Code ;
            message.From = new MailAddress("sender@example.com");
            message.To.Add(new MailAddress(request.Mail));

            await smtpClient.SendMailAsync(message);
            Console.WriteLine("Сообщение успешно отправлено. Связь с SMTP-сервером установлена.");
            return new ResponseModel<bool> { ResultCode = ResultCode.Success, Message = "Сообщение успелно отправленно" };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed, Message = e.Message };
        }
    }
}
