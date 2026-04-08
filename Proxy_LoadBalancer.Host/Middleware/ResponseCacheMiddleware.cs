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
        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
