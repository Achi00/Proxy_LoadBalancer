namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class DestinationOption
    {
        public string Address { get; set; }
        public int? Weight { get; set; }
    }
}
