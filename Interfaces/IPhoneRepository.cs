using Models;
using Requests;

namespace Interfaces;
public interface IPhoneRepository
{
    public Task<ResponseModel<bool>> SendVerificationCode(SendVerificationCodeRequest request);
}
