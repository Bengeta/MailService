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
            var smtpPort = 587;
            string username = "your_username";
            string password = "your_password";

            // Формирование сообщения
            MailMessage message = new MailMessage();
            message.Subject = "Подтверждение почты";
            message.Body = "Код подтверждения: " + request.Code ;
            message.From = new MailAddress("sender@example.com");
            message.To.Add(new MailAddress(request.Mail));

            // Подключение к SMTP-серверу
            SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(username, password);

            // Отправка сообщения
            smtpClient.Send(message);
            Console.WriteLine("Сообщение успешно отправлено");

            return new ResponseModel<bool> { ResultCode = ResultCode.Success, Message = "Sms отправлена" };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed, Message = e.Message };
        }
    }
}
