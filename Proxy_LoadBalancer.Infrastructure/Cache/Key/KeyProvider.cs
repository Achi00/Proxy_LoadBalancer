using Microsoft.AspNetCore.Http;
using System.Text;

namespace Proxy_LoadBalancer.Infrastructure.Cache.Key
{
    public class KeyProvider : ICacheKeyProvider
    {
        public string GenerateKey(HttpContext context, string[]? varyHeaders)
        {
            StringBuilder keyBuilder = new StringBuilder();

            // base key = method + path + query string
            keyBuilder.Append(context.Request.Method);
            keyBuilder.Append(":");
            keyBuilder.Append(context.Request.Path);

            if (context.Request.QueryString.HasValue)
            {
                keyBuilder.Append(context.Request.QueryString);
            }

            // include vary headers if specified
            if (varyHeaders != null && varyHeaders.Length > 0)
            {
                keyBuilder.Append(":");

                foreach (var header in varyHeaders)
                {
                    if (context.Request.Headers.TryGetValue(header, out var headerValue))
                    {
                        keyBuilder.Append(header);
                        keyBuilder.Append("=");
                        keyBuilder.Append(headerValue);
                        keyBuilder.Append(";");
                    }
                }
            }

            return keyBuilder.ToString();
        }
    }
}
