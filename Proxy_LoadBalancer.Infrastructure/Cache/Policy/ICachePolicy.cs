using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Policy
{
    public interface ICachePolicy
    {
        bool IsRequestCacheable(HttpContext context);
        bool IsResponseCacheable(HttpContext context, HttpResponseMessage response);
        TimeSpan? GetTtl(HttpResponseMessage response);
    }
}
