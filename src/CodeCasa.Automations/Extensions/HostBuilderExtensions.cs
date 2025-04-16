using Microsoft.Extensions.Hosting;
using Serilog;

namespace CodeCasa.Automations.Extensions;

internal static class HostBuilderExtensions
{
    public static IHostBuilder UseCodeCasa(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));
    }
}