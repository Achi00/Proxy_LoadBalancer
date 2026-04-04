using Serilog;

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
                        retainedFileCountLimit: 7);
            });

            return builder;
        }
    }
}
