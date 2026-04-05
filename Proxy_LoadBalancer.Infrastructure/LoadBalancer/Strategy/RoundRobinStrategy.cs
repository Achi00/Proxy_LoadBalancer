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
            // with modulo op wrap around to the start of the list
            // Math.Abs handles negative overflow case
            var index = (int)(Interlocked.Increment(ref _currentIndex) % (uint)weighted.Count);

            return weighted[index];
        }

        private int GetWeight(DestinationOption options)
        {
            return options.Weight ?? 1;
        }
    }
}
