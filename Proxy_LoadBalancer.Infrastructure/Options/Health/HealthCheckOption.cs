namespace Proxy_LoadBalancer.Infrastructure.Options.Health
{
    public class HealthCheckOption
    {
        public bool Enabled { get; set; } = true;
        public int IntervalSeconds { get; set; } = 10;
        public string Path { get; set; } = "/health";
    }
}
