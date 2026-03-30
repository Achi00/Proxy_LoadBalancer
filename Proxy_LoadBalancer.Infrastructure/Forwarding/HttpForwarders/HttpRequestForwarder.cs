using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Proxy_LoadBalancer.Infrastructure.Options;
using Proxy_LoadBalancer.Infrastructure.Routing;
using System.Net.Http.Headers;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders
{
    public class HttpRequestForwarder
    {
        private readonly DestinationOption _destinationOptions;
        private readonly RouteOption _routeOptions;
        private readonly IHttpClientFactory _factory;

        public HttpRequestForwarder(
            IOptions<DestinationOption> destinationOptions,
            IOptions<RouteOption> routeOptions,
            IHttpClientFactory factory)
        {
            _destinationOptions = destinationOptions.Value;
            _routeOptions = routeOptions.Value;
            _factory = factory;
        }
        public async Task<HttpResponseMessage> ForwardAsync(HttpContext context, ResolvedRoute resolvedRoute, CancellationToken ct)
        {
            // build destination url
            var FullUrl = BuildDestinationUri(context, resolvedRoute);

            // outgoing request
            var req = CreateForwardRequest(context, FullUrl);

            // forward http request to destination
            var client = _factory.CreateClient();

            return await client.SendAsync(req, ct);
        }

        private Uri BuildDestinationUri(HttpContext context, ResolvedRoute resolvedRoute)
        {
            var baseUri = new Uri(resolvedRoute.Cluster.Destinations.First().Address);

            var path = context.Request.Path.Value!;
            // get prefix from resolved route's match config
            var prefix = resolvedRoute.Route.Match.Path;

            // remove match route prefix
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(prefix.Length);
            }

            var builder = new UriBuilder(baseUri)
            {
                Path = path,
                Query = context.Request.QueryString.HasValue
                    ? context.Request.QueryString.Value
                    : string.Empty
            };

            return builder.Uri;
        }

        private HttpRequestMessage CreateForwardRequest(HttpContext context, Uri destinationUri)
        {
            var request = context.Request;

            var forwardRequest = new HttpRequestMessage(
                new HttpMethod(request.Method),
                destinationUri);

            // create content first (important for content headers), also covere if send as chunks, no length that case
            if (request.ContentLength > 0 || request.Headers.ContainsKey("Transfer-Encoding"))
            {
                // stream content
                forwardRequest.Content = new StreamContent(request.Body);

                if 
                (
                    // in case if content type includes anything other than  bare media type, otherwise will throw
                    // fixed by introducing MediaTypeHeaderValue.TryParse, return result.MediaType & result.CharSet
                    !string.IsNullOrEmpty(request.ContentType) && 
                    MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaType)
                )
                {
                    forwardRequest.Content.Headers.ContentType = mediaType;
                }
            }

            var connectionTokens = GetConnectionHeaderTokens(request);

            // copy headers
            foreach (var header in request.Headers)
            {
                if (connectionTokens.Contains(header.Key))
                {
                    continue;
                }
                // skip invalid headers
                if (!HeaderSanitizer.ShouldForward(header.Key))
                {
                    continue;
                }

                // content headers -> go to Content.Headers
                if (HeaderSanitizer.ContentHeaders.Contains(header.Key))
                {
                    if (forwardRequest.Content != null)
                    {
                        forwardRequest.Content.Headers.TryAddWithoutValidation(
                            header.Key, header.Value.ToArray());
                    }
                }
                else
                {
                    forwardRequest.Headers.TryAddWithoutValidation(
                        header.Key, header.Value.ToArray());
                }
            }

            // apply forwarding headers (X-Forwarded-*)
            ForwardingHeadersBuilder.Apply(forwardRequest, request, destinationUri);

            // set correct HOST (critical), dont use proxy host by mistake!!!
            forwardRequest.Headers.Host = destinationUri.Authority;

            return forwardRequest;
        }

        private HashSet<string> GetConnectionHeaderTokens(HttpRequest request)
        {
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (request.Headers.TryGetValue("Connection", out var values))
            {
                foreach (var value in values)
                {
                    foreach (var token in value.Split(','))
                    {
                        tokens.Add(token.Trim());
                    }
                }
            }

            return tokens;
        }
    }
}
