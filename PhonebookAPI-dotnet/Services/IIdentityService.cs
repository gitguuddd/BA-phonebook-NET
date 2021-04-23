using System.Threading.Tasks;
using PhonebookAPI_dotnet.Domain;
using PhonebookAPI_dotnet.Requests;

namespace PhonebookAPI_dotnet.Services
{
    public interface IIdentityService
    {
        public Task<AuthenticationResult> RegisterAsync(UserRegistrationRequest userRegistrationRequest);
        public Task<AuthenticationResult> LoginAsync(UserLoginRequest userLoginRequest);
        Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
    }
}