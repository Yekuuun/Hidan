using Deamon.Config;
using Deamon.Ebpf;
using Deamon.Logger;
using Deamon.Gui.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deamon;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var builder = ConfigureServices.ConfigureAppBuilder(args);

        //register EBPF host service.
        builder.Services.AddEbpf(
            ebpfConfiguration: new EbpfConfiguration(){ ProgramName = "Hidan", ProgramPath = Path.Combine(AppContext.BaseDirectory, "main.bpf.o") },
            configuration:builder.Configuration.GetSection("Ebpf")
        );

        //the terminal UI owns the console : anything else writing to stdout
        //scribbles over it, so the default console providers go.
        builder.Logging.ClearProviders();

        //registered after AddEbpf : hosted services start in order, so the
        //ring buffer exists before the UI starts draining it.
        builder.Services.AddHostedService<TerminalGuiService>();

        using var host = builder.Build();

        try
        {
            await host.RunAsync();
            DeamonLogger.WriteLog(ELogError.OK, "Exiting Deamon...");
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Global app error : {ex.Message}");
            return;
        }
    }
}