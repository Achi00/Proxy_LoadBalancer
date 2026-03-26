using Microsoft.AspNetCore.Http;
using Proxy_LoadBalancer.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_LoadBalancer.Infrastructure.Forwarding
{
    public class HttpRequestForwarder : IRequestForwarder
    {
        public Task ForwardAsync(HttpContext context, string targetUrl, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
