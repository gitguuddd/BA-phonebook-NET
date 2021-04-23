using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhonebookAPI_dotnet.Domain;
using PhonebookAPI_dotnet.Requests;
using PhonebookAPI_dotnet.Responses;
using PhonebookAPI_dotnet.Services;

namespace PhonebookAPI_dotnet.Controllers
{

    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService, IPhonebookEntryService phonebookEntryService)
        {
            _identityService = identityService;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest userRegistrationRequest)
        {
            var authResponse = await _identityService.RegisterAsync(userRegistrationRequest);

            return GetAuthResponse(authResponse);
        }
        
        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest userLoginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }
            var authResponse = await _identityService.LoginAsync(userLoginRequest);

            return GetAuthResponse(authResponse);
        }
        
        [HttpPost(ApiRoutes.Identity.Refresh)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var authResponse = await _identityService.RefreshTokenAsync(refreshTokenRequest);

            return GetAuthResponse(authResponse);
        }

        private IActionResult GetAuthResponse(AuthenticationResult authResponse) 
        {
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            
            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }
        
    }
}