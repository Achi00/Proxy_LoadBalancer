namespace Proxy_LoadBalancer.Infrastructure.Models
{
    public sealed class CachedResponse
    {
        public int StatusCode { get; init; }

        // includes selective headers only
        /*
         * Content-Type, Content-Encoding, Content-Length 
         * ETag, Last-Modified
         * Cache-Control, Expires 
         * Vary 
         * Location for 301s
         */
        public Dictionary<string, string[]> Headers { get; init; } = new();

        public byte[] Body { get; init; } = [];

        public DateTimeOffset CachedAt { get; init; }

        // vary headers support
        public string[] VaryHeaderNames { get; init; } = [];
        public Dictionary<string, string> VaryValues = new();

    }
}
