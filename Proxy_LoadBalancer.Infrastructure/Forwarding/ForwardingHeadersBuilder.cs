using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding
{
    public static class ForwardingHeadersBuilder
    {
        public static void Apply(HttpRequestMessage outgoing, HttpRequest incoming, Uri destinationUrl)
        {
            var clientIp = GetClientIp(incoming);

            // if a trusted upstream proxy already set this, preserve the chain.
            if (incoming.Headers.TryGetValue("X-Forwarded-For", out var existingForwardedFor))
            {
                outgoing.Headers.TryAddWithoutValidation(
                    "X-Forwarded-For", $"{existingForwardedFor}, {clientIp}");
            }
            else
            {
                outgoing.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);
            }

            // get dns host name, constructed in BuildDestinationUri, after config file scan
            outgoing.Headers.Host = destinationUrl.Authority;

            outgoing.Headers.TryAddWithoutValidation("X-Forwarded-Proto", incoming.Scheme);
            outgoing.Headers.TryAddWithoutValidation("X-Forwarded-Port",
                incoming.Host.Port?.ToString() ?? (incoming.IsHttps ? "443" : "80"));
            outgoing.Headers.TryAddWithoutValidation("X-Real-IP", clientIp);
            outgoing.Headers.TryAddWithoutValidation("X-Request-ID", Guid.NewGuid().ToString());
        }

        private static string GetClientIp(HttpRequest request)
            => request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
