using Proxy_LoadBalancer.Infrastructure.Cache;
using Proxy_LoadBalancer.Infrastructure.Cache.Key;
using Proxy_LoadBalancer.Infrastructure.Cache.Policy;
using Proxy_LoadBalancer.Infrastructure.Models;

namespace Proxy_LoadBalancer.Host.Middleware
{
    public class ResponseCacheMiddleware : IMiddleware
    {
        private readonly ICachePolicy _cachePolicy;
        private readonly IResponseCacheStore _store;
        private readonly ICacheKeyProvider _keyProvider;
        private readonly ILogger<ResponseCacheMiddleware> _logger;


        public ResponseCacheMiddleware(
            ICachePolicy cachePolicy, 
            IResponseCacheStore store, 
            ICacheKeyProvider keyProvider,
            ILogger<ResponseCacheMiddleware> logger
        )
        {
            _cachePolicy = cachePolicy;
            _store = store;
            _keyProvider = keyProvider;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // checks if requets is cachable
            if (!_cachePolicy.IsRequestCacheable(context))
            {
                await next(context);
                return;
            }

            // generate cache key, very headers are optional
            var key = _keyProvider.GenerateKey(context);

            var mustRevalidate = _cachePolicy.MustRevalidate(context);

            if (!mustRevalidate && _store.TryGet(key, out var cached) && cached is not null)
            {
                // cache was hit, write stored response directly to client
                await WriteCachedResponseAsync(context, cached);
                return;
            }

            // cache miss
            // capture response
            var originalBody = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                // forward to upstream
                await next(context);

                await context.Response.Body.FlushAsync();

                // check if we should store this response
                if (_cachePolicy.IsResponseCacheable(context))
                {
                    var ttl = _cachePolicy.GetTtl(context.Response) ?? TimeSpan.FromSeconds(300);
                    var body = memoryStream.ToArray();

                    var entry = new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        Headers = GetCacheableHeaders(context.Response.Headers),
                        Body = body,
                        CachedAt = DateTimeOffset.UtcNow
                    };

                    _store.Set(key, entry, ttl);
                    // DEBUG
                    _logger.LogInformation($"Cache STORED for {context.Request.Path}");
                }
            }
            finally
            {
                // copy buffered body to response stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBody);
                context.Response.Body = originalBody;
            }
        }

        private async Task WriteCachedResponseAsync(HttpContext context, CachedResponse cached)
        {
            // DEBUG
            _logger.LogInformation($"Cache HIT for {context.Request.Path}");

            context.Response.StatusCode = cached.StatusCode;

            foreach (var (key, values) in cached.Headers)
            {
                context.Response.Headers[key] = values;
            }

            await context.Response.Body.WriteAsync(cached.Body);
        }
        private static Dictionary<string, string[]> GetCacheableHeaders(IHeaderDictionary headers)
        {
            var headersToCache = new[]
            {
                "Content-Type", "Content-Encoding", "Content-Length",
                "ETag", "Last-Modified", "Cache-Control",
                "Expires", "Vary", "Location"
            };

            return headers
                .Where(h => headersToCache.Contains(h.Key, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(
                    h => h.Key,
                    h => h.Value.Select(v => v ?? string.Empty).ToArray()
                );
        }
    }
}
