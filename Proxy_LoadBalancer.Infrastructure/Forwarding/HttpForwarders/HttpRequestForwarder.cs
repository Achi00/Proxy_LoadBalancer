using Microsoft.AspNetCore.Http;
using Proxy_LoadBalancer.Infrastructure.Health;
using Proxy_LoadBalancer.Infrastructure.Routing;
using System.Net.Http.Headers;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders
{
    public class HttpRequestForwarder
    {
        private readonly IHttpClientFactory _factory;
        private readonly PassiveHealthTracker _healthTracker;

        public HttpRequestForwarder(IHttpClientFactory factory, PassiveHealthTracker healthTracker)
        {
            _factory = factory;
            _healthTracker = healthTracker;
        }
        public async Task<HttpResponseMessage> ForwardAsync(HttpContext context, ResolvedRoute resolvedRoute, CancellationToken ct)
        {
            // already filtered routes
            var destination = resolvedRoute.Cluster.Destinations.First();
            // build destination url
            var FullUrl = BuildDestinationUri(context, resolvedRoute);

            // outgoing request
            var req = CreateForwardRequest(context, FullUrl);

            // create named cleint which is already registered
            // forward http request to destination
            var client = _factory.CreateClient("proxy");

            Console.WriteLine($"{destination.Address} health state is: {_healthTracker.IsHealthy(destination.Address)}");
            // with health check
            // 3 failures, servuce is tracked as unhealthy
            try
            {
                var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                if ((int)res.StatusCode >= 500)
                {
                    _healthTracker.RecordFailure(destination.Address);
                }
                else
                {
                    _healthTracker.RecordSuccess(destination.Address);
                }

                return res;
            }
            catch (HttpRequestException)
            {
                // track failure
                _healthTracker.RecordFailure(destination.Address);
                // middleware handles rest
                throw;
            }
        }

        private Uri BuildDestinationUri(HttpContext context, ResolvedRoute resolvedRoute)
        {
            var baseUri = new Uri(resolvedRoute.Cluster.Destinations.First().Address);

            var path = context.Request.Path.Value ?? "";
            var query = context.Request.QueryString.Value ?? "";

            var transform = resolvedRoute.Route.Transform;

            // check if prefix remove is set explicitly in config
            if (transform?.RemovePrefix == true)
            {
                var prefix = resolvedRoute.Route.Match.Path;

                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path.Substring(prefix.Length);

                    if (!path.StartsWith("/"))
                        path = "/" + path;
                }
            }

            if (!string.IsNullOrEmpty(transform?.AddPrefix))
            {
                path = transform.AddPrefix.TrimEnd('/') + "/" + path.TrimStart('/');
            }

            var finalUri = new Uri(baseUri, path + query);
            
            return finalUri;
        }

        private HttpRequestMessage CreateForwardRequest(HttpContext context, Uri destinationUri)
        {
            var request = context.Request;

            var forwardRequest = new HttpRequestMessage(
                new HttpMethod(request.Method),
                destinationUri);

            // create content first (important for content headers), also covere if send as chunks, no length that case
            if (request.ContentLength > 0 || request.ContentLength == null && request.Body.CanRead || request.Headers.ContainsKey("Transfer-Encoding"))
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
