namespace Proxy_LoadBalancer.Infrastructure.Models
{
    public sealed class CachedResponse
    {
        public int StatusCode { get; init; }

        // includes selective headers only
        public Dictionary<string, string[]> Headers { get; init; } = new();

        public byte[] Body { get; init; } = [];

        public DateTimeOffset CachedAt { get; init; }

        public string[] VaryHeaderNames { get; init; } = [];
    }
}
