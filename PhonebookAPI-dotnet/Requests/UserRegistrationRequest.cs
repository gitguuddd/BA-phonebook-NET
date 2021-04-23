using System.ComponentModel.DataAnnotations;

namespace PhonebookAPI_dotnet.Requests
{
    public class UserRegistrationRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string PhoneNumber { get; set; }
    }
}