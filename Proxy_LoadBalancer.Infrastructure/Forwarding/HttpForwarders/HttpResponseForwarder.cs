using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Proxy_LoadBalancer.Infrastructure.Options;
using System.ComponentModel.DataAnnotations;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders
{
    public class HttpResponseForwarder
    {
        private readonly DestinationOption _destinationOptions;
        private readonly IHttpClientFactory _factory;

        public HttpResponseForwarder(IHttpClientFactory factory)
        {
            _factory = factory;
        }

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
            if (response.Content.Headers != null)
            {
                foreach (var kvp in response.Content.Headers)
                {
                    if (!HeaderSanitizer.HopByHop.Contains(kvp.Key))
                    {
                        context.Response.Headers.Add(kvp.Key, new StringValues(kvp.Value.ToArray()));
                    }
                }
            }

            // body stream
            if (response.Content != null)
            {
                await response.Content.CopyToAsync(context.Response.Body, ct);
            }
        }
    }
}
