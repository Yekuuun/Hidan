using Deamon.Ebpf;
using Deamon.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Deamon;

internal class Program
{
    public static async Task Main(string[] args)
    {
        long count  = 0;
        var builder = ConfigureAppBuilder(args);

        //register EBPF host service.
        builder.Services.AddEbpf(
            ebpfConfiguration: new EbpfConfiguration(){ ProgramName = "Hidan", ProgramPath = Path.Combine(AppContext.BaseDirectory, "main.bpf.o") },
            configuration:builder.Configuration.GetSection("Ebpf")
        );

        using var host = builder.Build();
        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

        try
        {
            await host.StartAsync();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
            ConfigSigHandler(cts);

            while (!cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                DeamonLogger.WriteLog(ELogError.OK, $"[*] event running {count}");
                count++;
            }

            DeamonLogger.WriteLog(ELogError.OK, "Exiting Deamon...");
            await host.StopAsync();
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Global app error : {ex.Message}");
            return;
        }
    }

    /// <summary>
    /// Configure CTRL-C handler.
    /// </summary>
    private static void ConfigSigHandler(CancellationTokenSource cts)
    {
        Console.CancelKeyPress += (_, e) =>
        {
            Console.WriteLine("\n");
            e.Cancel = true;
            cts.Cancel();
        };
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

        builder.Services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(15));

        builder.ConfigureContainer(new DefaultServiceProviderFactory(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        }));

        return builder;
    }
}