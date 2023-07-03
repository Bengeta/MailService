using Models;
using Requests;

namespace Interfaces;
public interface IMailRepository
{
    public Task<ResponseModel<bool>> SendVerificationCode(SendVerificationCodeRequest request);
}
