using LMS_APIs.Models;

namespace LMS_APIs.Repository
{
    public interface IUser
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<ServiceResult<Borrowers>> RegisterAsync(RegisterRequest request);
    }
}
