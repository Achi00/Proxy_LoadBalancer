using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Proxy_LoadBalancer.Infrastructure.Cache.Policy;

namespace Proxy_LoadBalancer.Host.Middleware
{
    public class ResponseCacheMiddleware : IMiddleware
    {
        // 1. Check IsRequestCacheable
        // 2. Build key, try TryGet
        // 3. HIT -> write cached response to HttpContext.Response, return
        // 4. MISS -> call next (forwards to upstream)
        // 5. Check IsResponseCacheable on the forwarded response
        // 6. YES -> serialize body, build CachedResponse, call Set
        private readonly ICachePolicy _cachePolicy;
        private readonly RequestDelegate _next;


        public ResponseCacheMiddleware(ICachePolicy cachePolicy, RequestDelegate next)
        {
            _cachePolicy = cachePolicy;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (!_cachePolicy.IsRequestCacheable(context))
            {
                await _next(context);
                return;
            }

            // no-cache = skip lookup, go fresh, but still store result
            if (!_cachePolicy.MustRevalidate(context) /* && _store.TryGet(key, out var cached)*/)
            {
                return;
            }
        }
    }
}
