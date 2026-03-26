using Microsoft.AspNetCore.Http;
using Proxy_LoadBalancer.Application.DTOs;
using Proxy_LoadBalancer.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy_LoadBalancer.Infrastructure.Routing
{
    internal class ConfigRouteResolver : IRouteResolver
    {
        public ResolvedRouteResponse? Resolve(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
