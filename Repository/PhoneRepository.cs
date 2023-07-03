using System.Net.Http.Headers;
using System.Text;
using Enums;
using Interfaces;
using Models;
using Requests;

namespace Repository;
public class PhoneRepository : IPhoneRepository
{
    private readonly IConfiguration _configuration;
    public PhoneRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ResponseModel<bool>> SendVerificationCode(SendVerificationCodeRequest request)
    {
        try
        {
            var text = "Код подтверждения телефона: " + request.Code;
            var login = _configuration.GetValue<string>("SmsLogin");
            var key = _configuration.GetValue<string>("SmsKey");
            var baseUrl = _configuration.GetValue<string>("SmsBaseUrl");
            var sign = _configuration.GetValue<string>("SmsSign");

            var url = $"{baseUrl}?number={request.Phone}&text={text}&sign={sign}";

            using var client = new HttpClient();
            var byteArray = Encoding.ASCII.GetBytes($"{login}:{key}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Console.WriteLine(request.Phone + " - " + response);
            return new ResponseModel<bool> { ResultCode = ResultCode.Success, Message = "Sms отправлена" };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new ResponseModel<bool> { ResultCode = ResultCode.Failed, Message = e.Message };
        }
    }
}
