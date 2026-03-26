using Proxy_LoadBalancer.Infrastructure.Options.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_LoadBalancer.Infrastructure.Options
{
    public class ProxyOption
    {
        public ProxyDefaults Defaults { get; set; }
        public Dictionary<string, ClusterOption> Clusters { get; set; }
        public List<RouteOption> Routes { get; set; }
    }
}
