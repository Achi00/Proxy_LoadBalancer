namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class ClusterOption
    {
        public string Id { get; set; }
        public string LoadBalancing { get; set; }
        public List<DestinationOption> Destinations { get; set; } = new();
    }
}
