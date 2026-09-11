using Deamon.Ebpf;
using Deamon.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deamon;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var builder = ConfigureAppBuilder(args);

        builder.Services.AddEbpf(
            ebpfConfiguration: new EbpfConfiguration(){ ProgramName = "Hidan", ProgramPath = "main.bpf.o" },
            configuration:builder.Configuration.GetSection("Ebpf")
        );

        using var host = builder.Build();
        try
        {
            await host.RunAsync();
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Global app error : {ex.Message}");
            return;
        }
    }

    /// <summary>
    /// Configure app launch.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static HostApplicationBuilder ConfigureAppBuilder(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory, // appsettings.json
        });

        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
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