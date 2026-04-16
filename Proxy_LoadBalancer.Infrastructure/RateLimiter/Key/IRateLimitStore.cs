namespace Proxy_LoadBalancer.Infrastructure.RateLimiter.Key
{
    public interface IRateLimitStore
    {
        // returns current count after increment
        int Increment(string key, TimeSpan window);
    }
}
