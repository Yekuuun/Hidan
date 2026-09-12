using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Deamon.Config;

internal static class ConfigureServices
{
    /// <summary>
    /// Configure app launch.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static HostApplicationBuilder ConfigureAppBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory, // appsettings.json
        });

        builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(15));

        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        }));

        return builder;
    }
}