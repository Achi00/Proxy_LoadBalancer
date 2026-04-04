using Proxy_LoadBalancer.Infrastructure.LoadBalancer;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Host.Infrastructure.Extensions
{
    public static class WebExtensions
    {
        public static IServiceCollection AddProxyConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // checking mismatch on startup so it blows before http request
            services.AddOptions<ProxyOption>()
                .Bind(configuration.GetSection("Proxy"))
                .PostConfigure(options =>
                {
                    foreach (var (key, cluster) in options.Clusters)
                    {
                        cluster.Id = key;
                    }
                })
                .Validate(options =>
                {
                    foreach (var cluster in options.Clusters.Values)
                    {
                        foreach (var dest in cluster.Destinations)
                        {
                            if 
                            (
                                !Uri.TryCreate(dest.Address, UriKind.Absolute, out var uri) ||
                                (uri.Scheme != "http" && uri.Scheme != "https")
                            )
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }, "One or more cluster destinations have an invalid or non-http/https URI.")
                .ValidateOnStart();

            services.AddSingleton<ILoadBalancer, LoadBalancerService>();

            return services;
        }
    }
}
