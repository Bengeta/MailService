namespace Requests;
public class SendVerificationCodeRequest
{
    public string? Phone { get; set; }
    public string? Mail { get; set; }
    public string Code { get; set; }
    public bool IsMail { get; set; }
}
