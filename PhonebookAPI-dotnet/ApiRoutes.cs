namespace PhonebookAPI_dotnet
{
    public static class ApiRoutes
    {
        private const string Base= "api";

        public static class PhonebookEntries
        {
            public const string Index = Base + "/phonebookEntries";
            
            public const string Get = Base + "/phonebookEntries/{id}";
            
            public const string Create = Base + "/phonebookEntries";
            
            public const string Update = Base + "/phonebookEntries/{id}";
            
            public const string Delete = Base + "/phonebookEntries/{id}";
        }

        public static class Identity
        {
            public const string Login = Base + "/identity/login";
            
            public const string Register = Base + "/identity/register";

            public const string Refresh = Base + "/identity/refresh";
        }
    }
}