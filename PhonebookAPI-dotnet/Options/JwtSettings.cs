using System;

namespace PhonebookAPI_dotnet.Options
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public TimeSpan TokenLifeTime { get; set; }
    }
}