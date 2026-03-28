using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Application.Interfaces;
using Proxy_LoadBalancer.Infrastructure.Options;
using System.Web;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding
{
    public class HttpRequestForwarder : IRequestForwarder
    {
        private readonly DestinationOption _destinationOptions;
        private readonly ProxyOption _proxyOptions;

        public HttpRequestForwarder(
            IOptions<DestinationOption> destinationOptions,
            IOptions<ProxyOption> proxyOptions)
        {
            _destinationOptions = destinationOptions.Value;
            _proxyOptions = proxyOptions.Value;
        }
        public Task ForwardAsync(HttpContext context, string targetUrl, CancellationToken ct)
        {
            var FullUrl = GetDestinationAddress(context);

            var req = CreateForwardRequest(context, FullUrl);
        }

        private string GetDestinationAddress(HttpContext context)
        {
            // build target url
            UriBuilder url = new UriBuilder(_destinationOptions.Address);
            url.Path = context.Request.Path;

            // add queries
            var query = HttpUtility.ParseQueryString(url.Query);
            foreach (var q in context.Request.Query)
            {
                query[q.Key] = q.Value;
            }

            url.Query = query.ToString();

            return url.ToString();
        }

        private HttpRequestMessage CreateForwardRequest(HttpContext context, string targetUrl)
        {
            var request = context.Request;
            var forwardRequest = new HttpRequestMessage(new HttpMethod(request.Method), targetUrl);

            HeaderSanitizer.SanitizeRequestHeaders(forwardRequest, context.Request);
            // copy body if exists
            if (request.ContentLength > 0)
            {
                forwardRequest.Content = new StreamContent(request.Body);
                // copy content type
                if (!string.IsNullOrEmpty(request.ContentType))
                {
                    forwardRequest.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
                }
            }
            return forwardRequest;
        }
    }
}
