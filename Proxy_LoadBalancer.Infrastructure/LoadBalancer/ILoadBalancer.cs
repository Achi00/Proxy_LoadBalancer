using Proxy_LoadBalancer.Infrastructure.Health;
using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.LoadBalancer
{
    public interface ILoadBalancer
    {
        DestinationOption SelectDestination(ClusterOption cluster, PassiveHealthTracker healthTracker);
    }
}