namespace Proxy_LoadBalancer.Infrastructure.Options.Health
{
    public class HealthCheckOption
    {
        public string Path { get; set; }
        public int IntervalSeconds { get; set; }
        public int TimeoutSeconds { get; set; }
        public int UnhealthyThreshold { get; set; }
        public int HealthyThreshold { get; set; }
    }
}
