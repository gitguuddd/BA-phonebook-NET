using System.ComponentModel.DataAnnotations;

namespace PhonebookAPI_dotnet.Requests
{
    public class UserLoginRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}