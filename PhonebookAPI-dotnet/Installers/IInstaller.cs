using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PhonebookAPI_dotnet.Installers
{
    public interface IInstaller
    {
        void InstallServices(IServiceCollection services, IConfiguration configuration);
    }
}