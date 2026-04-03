using Proxy_LoadBalancer.Infrastructure.Options;

namespace Proxy_LoadBalancer.Infrastructure.Validator
{
    public class ClusterValidator
    {
        // validation for http/https mismatch, avoid 502 error
        public void Validate(ClusterOption cluster)
        {
            foreach (var dest in cluster.Destinations)
            {
                if (!Uri.TryCreate(dest.Address, UriKind.Absolute, out var uri))
                    throw new InvalidOperationException(
                        $"Destination '{dest.Address}' is not a valid absolute URI.");

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    throw new InvalidOperationException(
                        $"Destination '{dest.Address}' must use http or https.");
            }
        }
    }
}
