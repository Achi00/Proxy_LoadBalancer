using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Proxy_LoadBalancer.Infrastructure.Cache;
using Proxy_LoadBalancer.Infrastructure.Cache.Key;
using Proxy_LoadBalancer.Infrastructure.Cache.Policy;
using Proxy_LoadBalancer.Infrastructure.Models;

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
        private readonly IResponseCacheStore _store;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly RequestDelegate _next;


        public ResponseCacheMiddleware(ICachePolicy cachePolicy, IResponseCacheStore store, ICacheKeyProvider keyProvider, RequestDelegate next)
        {
            _cachePolicy = cachePolicy;
            _store = store;
            _keyProvider = keyProvider;
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // checks if requets is cachable
            if (!_cachePolicy.IsRequestCacheable(context))
            {
                await _next(context);
                return;
            }

            // very headers are optional
            var key = _keyProvider.GenerateKey(context);

            if (_store.TryGet(key, out var cached))
            {
                await WriteCachedResponse(context, cached);
                return;
            }

            // capture response
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await next(context);
            
            // restore body
            context.Response.Body = originalBody;

            // response part
        }

        private async Task WriteCachedResponse(HttpContext context, CachedResponse cached)
        {
            context.Response.StatusCode = 200;

            await context.Response.Body.WriteAsync(cached.Body, 0, cached.Body.Length);
        }
        private async Task CopyToOriginalStream(Stream memory, Stream original)
        {
            memory.Position = 0;
            await memory.CopyToAsync(original);
        }
    }
}
