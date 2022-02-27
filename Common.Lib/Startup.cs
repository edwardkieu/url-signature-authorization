using Common.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Lib
{
    public static class Startup
    {
        public static void RegisterUrlSignatureService(this IServiceCollection services)
        {
            services.AddTransient<IUrlSignatureService, UrlSignatureService>();
        }
    }
}