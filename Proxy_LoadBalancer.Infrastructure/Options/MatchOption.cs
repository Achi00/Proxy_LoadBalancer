namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class MatchOption
    {
        public string Path { get; set; }
        public string PathMatch { get; set; }
        public List<string> Methods { get; set; }
    }
}
