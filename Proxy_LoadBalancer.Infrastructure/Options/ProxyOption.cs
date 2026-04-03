using Proxy_LoadBalancer.Infrastructure.Options.Defaults;

namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class ProxyOption
    {
        public ProxyDefaults Defaults { get; set; }
        public Dictionary<string, ClusterOption> Clusters { get; set; }
        public List<RouteOption> Routes { get; set; }
    }
}
