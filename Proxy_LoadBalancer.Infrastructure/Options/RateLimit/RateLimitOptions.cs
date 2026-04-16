namespace Proxy_LoadBalancer.Infrastructure.Options.RateLimit
{
    public sealed class RateLimitOptions
    {
        public int RequestsPerWindow { get; init; } = 100;
        public int WindowSeconds { get; init; } = 60;
        public bool Enabled { get; init; } = true;
    }
}
