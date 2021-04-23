using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PhonebookAPI_dotnet.Domain
{
    public class PhonebookEntry
    {
        [Key]
        public int Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }
    }
}