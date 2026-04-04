namespace Proxy_LoadBalancer.Infrastructure.LoadBalancer.Strategy
{
    public class RoundRobinStrategy<T>
    {
        private int _currentIndex = -1;

        public T GetNext(List<T> servers)
        {
            var index = Interlocked.Increment(ref _currentIndex);
            // with modulo op wrap around to the start of the list
            // Math.Abs handles negative overflow case
            return servers[Math.Abs(index % servers.Count)];
        }
    }
}
