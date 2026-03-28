namespace Proxy_LoadBalancer.Infrastructure.Forwarding
{
    public static class HeaderSanitizer
    {
        // never forward these based on RFC 7230 6.1 , connection-scoped only
        public static readonly HashSet<string> HopByHop = new(StringComparer.OrdinalIgnoreCase)
        {
            "Connection", "Keep-Alive", "Proxy-Authenticate", "Proxy-Authorization",
            "TE", "Trailers", "Transfer-Encoding", "Upgrade", "Proxy-Connection"
        };

        // headers which backend trusts, client must never be able to set these!!
        public static readonly HashSet<string> InternalTrust = new(StringComparer.OrdinalIgnoreCase)
        {
            "X-Internal-Request", "X-Authenticated-User", "X-Auth-Token",
            "X-Auth-User", "X-Admin"
            // TODO: add internal headers here, make dynamic from config file?
        };

        // headers which proxy owns, must be strip incoming, reset with correct values
        public static readonly HashSet<string> ProxyOwned = new(StringComparer.OrdinalIgnoreCase)
        {
            "X-Forwarded-For", "X-Forwarded-Host", "X-Forwarded-Proto",
            "X-Forwarded-Port", "X-Real-IP", "X-Request-ID"
        };

        // content headers belong on HttpContent.Headers, not HttpRequestMessage.Headers
        public static readonly HashSet<string> ContentHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Content-Type", "Content-Length", "Content-Encoding",
            "Content-Language", "Content-Location", "Content-MD5",
            "Content-Range", "Content-Disposition", "Expires", "Last-Modified"
        };

        public static bool ShouldForward(string headerName)
        {
            // dynamically skip headers named in the Connection header (RFC 7230 6.1)
            return !HopByHop.Contains(headerName)
                && !InternalTrust.Contains(headerName)
                && !ProxyOwned.Contains(headerName);
            // Not included !ContentHeaders.Contains(headerName) we let loop handle this
        }
    }

}
