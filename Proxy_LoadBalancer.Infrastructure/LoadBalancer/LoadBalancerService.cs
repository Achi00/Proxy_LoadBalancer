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
                throw new InvalidOperationException(
                    $"No healthy destinations available for cluster {cluster.Id}");
            }

            // creates strategy per cluster
            if (!_strategies.ContainsKey(cluster.Id))
            {
                _strategies[cluster.Id] = new RoundRobinStrategy<DestinationOption>();
            }
            // pass healthy services only
            return _strategies[cluster.Id].GetNext(healthy);
        }
    }
}
