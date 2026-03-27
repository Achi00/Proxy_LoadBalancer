using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    public class ResolvedRoute
    {
        public RouteOption Route { get; set; }
        public ClusterOption Cluster { get; set; }
    }
}
