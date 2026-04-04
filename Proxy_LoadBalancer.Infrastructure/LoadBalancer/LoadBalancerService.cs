using Proxy_LoadBalancer.Infrastructure.Health;
using Proxy_LoadBalancer.Infrastructure.LoadBalancer.Strategy;
using Proxy_LoadBalancer.Infrastructure.Options;
using System.Collections.Concurrent;

namespace Proxy_LoadBalancer.Infrastructure.LoadBalancer
{
    public class LoadBalancerService : ILoadBalancer
    {
        // singleton, will use concurent dict, for thread safety
        private readonly ConcurrentDictionary<string, RoundRobinStrategy<DestinationOption>> _strategies = new();

        public DestinationOption SelectDestination(ClusterOption cluster, PassiveHealthTracker healthTracker)
        {
            // filter healthy services
            var healthy = cluster.Destinations
                .Where(d => healthTracker.IsHealthy(d.Address))
                .ToList();

            if (healthy.Count == 0)
            {
                throw new InvalidOperationException($"No healthy destinations available for cluster {cluster.Id}");
            }

            // creates strategy per cluster
            var strategy = _strategies.GetOrAdd(cluster.Id, _ => new RoundRobinStrategy<DestinationOption>());
            
            // passing healthy services only
            return strategy.GetNext(healthy);
        }
    }
}
