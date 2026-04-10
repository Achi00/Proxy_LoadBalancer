using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Policy
{
    public class CachePolicy : ICachePolicy
    {
        // 5 minutes as default
        private const int DefaultCacheTtlSeconds = 300;

        public TimeSpan? GetTtl(HttpResponseMessage response)
        {
            // Check Cache-Control header (preferred, most common)
            if (response.Headers.TryGetValues("Cache-Control", out var cacheControlValues))
            {
                var ttl = ParseCacheControlTtl(cacheControlValues);
                if (ttl.HasValue)
                {
                    return ttl;
                }
            }

            // Fallback to Expires header
            if (response.Content.Headers.TryGetValues("Expires", out var expiresValues))
            {
                var ttl = ParseExpiresTtl(expiresValues);
                if (ttl.HasValue)
                {
                    return ttl;
                }
            }

            // No explicit cache directive found, return default
            return null;
        }

        public bool IsRequestCacheable(HttpContext context)
        {
            // is method chachable
            if (context.Request.Method != HttpMethod.Get.ToString() && context.Request.Method != HttpMethod.Head.ToString())
            {
                return false;
            }

            // if explicitly denies cache
            //if (context.Request.Headers.TryGetValue("Cache-Control", out var value) && value == "no-store")
            if (context.Request.Headers.TryGetValue("Cache-Control", out var value))
            {
                var directives = value.ToString().Split(',').Select(d => d.Trim());

                if (directives.Contains("no-store", StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }

                // no-cache is cacheble but needs revalidation
            }

            return true;
        }

        // revalidation of headers, seperate method to explicitly call in cache middleware in certain conditions
        public bool MustRevalidate(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Cache-Control", out var value))
            {
                var directives = value.ToString().Split(',').Select(d => d.Trim());
                return directives.Contains("no-cache", StringComparer.OrdinalIgnoreCase);
            }
            return false;
        }

        public bool IsResponseCacheable(HttpResponseMessage response)
        {
            // check response status
            if (!IsRelevantStatus((int)response.StatusCode))
            {
                return false;
            }

            // check headers
            // if explicitly denies cache
            if (response.Headers.TryGetValues("Cache-Control", out var values))
            {
                var directives = values
                    .SelectMany(v => v.Split(','))
                    .Select(d => d.Trim());

                if (directives.Contains("no-store", StringComparer.OrdinalIgnoreCase) ||
                    directives.Contains("private", StringComparer.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        // handle very headers fro response
        public string[] GetVaryHeaders(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Vary", out var values))
                return Array.Empty<string>();

            // "Accept-Encoding, Accept-Language"
            return values
                .SelectMany(v => v.Split(','))
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrEmpty(v))
                .ToArray();
        }

        // check status code if compitable on caching
        private bool IsRelevantStatus(int statusCode) => (statusCode >= 200 && statusCode <= 299)
            || statusCode == 301
            || statusCode == 308
            || statusCode == 404
            || statusCode == 410;

        private TimeSpan? ParseCacheControlTtl(IEnumerable<string> cacheControlValues)
        {
            foreach (var cacheControl in cacheControlValues)
            {
                // Split by comma and check each directive
                var directives = cacheControl.Split(',');

                // Prefer s-maxage (for shared caches) over max-age (for browser caches)
                var sMaxAge = directives
                    .FirstOrDefault(d => d.Trim().StartsWith("s-maxage=", StringComparison.OrdinalIgnoreCase));

                if (sMaxAge != null && ExtractMaxAgeSeconds(sMaxAge) is int sMaxAgeSeconds)
                {
                    return TimeSpan.FromSeconds(sMaxAgeSeconds);
                }

                // Fall back to max-age
                var maxAge = directives
                    .FirstOrDefault(d => d.Trim().StartsWith("max-age=", StringComparison.OrdinalIgnoreCase));

                if (maxAge != null && ExtractMaxAgeSeconds(maxAge) is int maxAgeSeconds)
                {
                    return TimeSpan.FromSeconds(maxAgeSeconds);
                }
            }

            return null;
        }
        // extracts numeric value from "max-age=3600" or "s-maxage=7200"
        private int? ExtractMaxAgeSeconds(string directive)
        {
            var parts = directive.Trim().Split('=');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out var seconds) && seconds >= 0)
            {
                return seconds;
            }
            return null;
        }

        // parses HTTP date format
        private TimeSpan? ParseExpiresTtl(IEnumerable<string> expiresValues)
        {
            var expiresValue = expiresValues.FirstOrDefault();
            if (string.IsNullOrEmpty(expiresValue))
                return null;

            if (DateTimeOffset.TryParse(expiresValue, out var expiresDate))
            {
                var ttl = expiresDate - DateTimeOffset.UtcNow;
                return ttl.TotalSeconds > 0 ? ttl : TimeSpan.Zero;
            }

            return null;
        }
    }
}
