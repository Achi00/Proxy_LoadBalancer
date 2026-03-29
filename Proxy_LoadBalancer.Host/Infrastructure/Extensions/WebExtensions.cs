using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Host.Infrastructure.Extensions
{
    public static class WebExtensions
    {
        public static IServiceCollection AddProxyConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ProxyOption>(
                configuration.GetSection("Proxy"));

            return services;
        }
    }
}
