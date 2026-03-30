using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders
{
    public class HttpResponseForwarder
    {
        public async Task ForwardAsync(HttpContext context, HttpResponseMessage response, CancellationToken ct = default)
        {
            context.Response.StatusCode = (int)response.StatusCode;

            // filter out hop-by-hop response headers
            foreach (var kvp in response.Headers)
            {
                if (!HeaderSanitizer.HopByHop.Contains(kvp.Key))
                {
                    context.Response.Headers.Add(kvp.Key, new StringValues(kvp.Value.ToArray()));
                }
            }
            // filter out hop-by-hop response content headers (Content-Type, Content-Length, ect...)
            foreach (var kvp in response.Content.Headers)
            {
                if (!HeaderSanitizer.HopByHop.Contains(kvp.Key))
                {
                    context.Response.Headers.Add(kvp.Key, new StringValues(kvp.Value.ToArray()));
                }
            }

            // body stream
            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);

            //await responseStream.CopyToAsync(context.Response.BodyWriter, ct);

            await context.Response.Body.FlushAsync(ct);
        }
    }
}
