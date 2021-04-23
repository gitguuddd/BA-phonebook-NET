using System.Collections;
using System.Collections.Generic;

namespace PhonebookAPI_dotnet.Responses
{
    public class AuthFailedResponse
    {
        public IEnumerable<string> Errors { get; set; }
    }
}