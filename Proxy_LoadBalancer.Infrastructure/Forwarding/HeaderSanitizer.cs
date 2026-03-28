using Microsoft.AspNetCore.Http;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding
{
    public class HeaderSanitizer
    {
        // hop by hop headers baed on RFC 7230 we should never forward these
        private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailer", "Transfer-Encoding", "Upgrade", "Proxy-Connection"
        };

        // Strip these from client — proxy will re-set them with correct values
        private static readonly HashSet<string> ForwardingHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "X-Forwarded-For", "X-Forwarded-Host", "X-Forwarded-Proto",
            "X-Forwarded-Port", "X-Real-IP", "X-Client-IP"
        };

        // Never let client set these — internal trust headers
        private static readonly HashSet<string> InternalHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "X-Internal-Request", "X-Authenticated-User", "X-Auth-Token",
            "X-Request-ID"
        };

        public static void SanitizeRequestHeaders(HttpRequestMessage outgoing, HttpRequest incoming)
        {
            // copy headers from incoming request
            foreach (var header in incoming.Headers)
            {
                outgoing.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            // remove hop by hop headers listed inside connection header
            if (outgoing.Headers.TryGetValues("Connection", out var connectionValues))
            {
                foreach (var value in connectionValues)
                {
                    foreach (var token in value.Split(','))
                    {
                        outgoing.Headers.Remove(token.Trim());
                    }
                }
            }

            // remove all hop by hop headers
            foreach (var header in HopByHopHeaders)
            {
                outgoing.Headers.Remove(header);
            }

            // strip forwarding headers (will reset below)
            foreach (var header in ForwardingHeaders)
            {
                outgoing.Headers.Remove(header);
            }

            // strip internal/trust headers unconditionally
            foreach (var header in InternalHeaders)
            {
                outgoing.Headers.Remove(header);
            }

            // reset forwarding headers with correct values
            var clientIp = GetClientIp(incoming);
            outgoing.Headers.TryAddWithoutValidation("X-Forwarded-For", clientIp);
            outgoing.Headers.TryAddWithoutValidation("X-Real-IP", clientIp);
            outgoing.Headers.TryAddWithoutValidation("X-Forwarded-Host", incoming.Host.Value);
            outgoing.Headers.TryAddWithoutValidation("X-Forwarded-Proto", incoming.Scheme);

            outgoing.Headers.TryAddWithoutValidation("X-Request-ID", Guid.NewGuid().ToString());
        }

        private static string GetClientIp(HttpRequest request)
        {
            // read the real IP from context
            return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
