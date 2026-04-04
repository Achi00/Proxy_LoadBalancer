using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.LoadBalancer.Strategy
{
    public class RoundRobinStrategy<T> where T : DestinationOption
    {
        private int _currentIndex = -1;

        public T GetNext(List<T> servers)
        {
            // use destination weights from config
            var weighted = servers
                .SelectMany(s => Enumerable.Repeat(s, GetWeight(s)))
                .ToList();
            var index = Interlocked.Increment(ref _currentIndex);
            // with modulo op wrap around to the start of the list
            // Math.Abs handles negative overflow case
            return servers[Math.Abs(index % servers.Count)];
        }

        private int GetWeight(DestinationOption options)
        {
            return options.Weight ?? 0;
        }
    }
}
