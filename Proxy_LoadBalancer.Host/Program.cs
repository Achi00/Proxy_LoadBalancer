using Proxy_LoadBalancer.Host.Infrastructure.Extensions;
using Proxy_LoadBalancer.Host.Middleware;
using Proxy_LoadBalancer.Infrastructure.Forwarding.HttpForwarders;
using Proxy_LoadBalancer.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProxyConfig(builder.Configuration);

builder.Services
    .AddHttpClient("proxy")
    .AddProxyResiliencePolicy(perAttemptTimeoutSeconds: 10, absoluteTimeoutSeconds: 25);

builder.Services.AddSingleton<ProxyMiddleware>();
builder.Services.AddSingleton<ConfigRouteResolver>();
builder.Services.AddSingleton<HttpRequestForwarder>();
builder.Services.AddSingleton<HttpResponseForwarder>();

var app = builder.Build();

app.UseMiddleware<ProxyExceptionHandlerMiddleware>();
app.UseMiddleware<ProxyMiddleware>();

app.MapGet("/", () => "Hello World!");

app.Run();
