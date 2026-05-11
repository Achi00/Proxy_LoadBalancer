namespace Proxy_LoadBalancer.Infrastructure.RateLimiter.Models
{
    public sealed class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTimeOffset WindowStart { get; init; }
    }
}
