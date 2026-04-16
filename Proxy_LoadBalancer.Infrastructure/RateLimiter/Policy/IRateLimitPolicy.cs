using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.RateLimiter.Policy
{
    public interface IRateLimitPolicy
    {
        bool IsEnabled(HttpContext context);
        bool IsExceeded(int currentCount);
        TimeSpan GetWindow();
    }
}
