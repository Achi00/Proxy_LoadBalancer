using System.Collections.Concurrent;

namespace Proxy_LoadBalancer.Infrastructure.Health
{
    public class PassiveHealthTracker
    {
        private readonly ConcurrentDictionary<string, DestinationHealth> _states = new();
        private readonly int _failureThreshold;
        private readonly TimeSpan _cooldown;

        public PassiveHealthTracker(int failureThreshold = 1, TimeSpan? cooldown = null)
        {
            _failureThreshold = failureThreshold;
            _cooldown = cooldown ?? TimeSpan.FromSeconds(30);
        }

        public DestinationHealth GetOrCreate(string address) =>
            _states.GetOrAdd(address, _ => new DestinationHealth());

        public bool IsHealthy(string address) =>
            GetOrCreate(address).IsHealthy;

        public void RecordFailure(string address) =>
            GetOrCreate(address).RecordFailure(_failureThreshold, _cooldown);

        public void RecordSuccess(string address) =>
            GetOrCreate(address).RecordSuccess();
    }
}
