namespace Proxy_LoadBalancer.Domain.Entity
{
    public class RouteConfig
    {
        public string Path { get; set; }
        public List<string> Destinations { get; set; }
    }
}
