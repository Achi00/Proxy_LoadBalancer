namespace Proxy_LoadBalancer.Infrastructure.Health
{
    // atomic operation, executes fully or not at all
    public class DestinationHealth
    {
        private int _failureCount;
        // Environment.TickCount64
        private long _unhealthyUntilTicks;

        public bool IsHealthy => Environment.TickCount64 >= _unhealthyUntilTicks;

        // interock because we need thread safe operation, atomic increment/decrement
        public void RecordFailure(int threshold, TimeSpan cooldown)
        {
            var failures = Interlocked.Increment(ref _failureCount);
            if (failures >= threshold)
            {
                // dont need old value at this moment
                Interlocked.Exchange(ref _unhealthyUntilTicks, Environment.TickCount64 + (long)cooldown.TotalMilliseconds);
                // reset counter
                Interlocked.Exchange(ref _failureCount, 0);
            }
        }

        public void RecordSuccess()
        {
            Interlocked.Exchange(ref _failureCount, 0);
            Interlocked.Exchange(ref _unhealthyUntilTicks, 0);
        }
    }
}
