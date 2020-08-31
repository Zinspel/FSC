using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuditQualification.Services.GraphOperations
{
    public static class Bootstrapper
    {
        public static void AddGraphService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebOptions>(configuration);
            services.AddHttpClient<IGraphApiOperations, GraphApiOperationService>();
        }
    }
}