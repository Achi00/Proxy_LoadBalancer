using Serilog;
using Serilog.Events;

namespace Proxy_LoadBalancer.Host.Infrastructure.Extensions
{
    public static class LogExtensions
    {
        public static IHostBuilder AddSerilogConfig(this IHostBuilder builder)
        {
            builder.UseSerilog((ctx, config) =>
            {
                config
                    .ReadFrom.Configuration(ctx.Configuration)
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "logs/proxy-.log",
                        // new file each day
                        rollingInterval: RollingInterval.Day,
                        // keep last 7 days
                        retainedFileCountLimit: 7)
                    .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.ContainsKey("SourceContext") &&
                        e.Properties["SourceContext"].ToString().Contains("ActiveHealthCheckWorker"))
                    .WriteTo.File("logs/health-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7))
                    .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning ||
                                             e.Level == LogEventLevel.Error)
                    .WriteTo.File("logs/errors-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7));
            });

            return builder;
        }
    }
}
