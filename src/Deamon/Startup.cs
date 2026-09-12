using Deamon.Config;
using Deamon.Ebpf;
using Deamon.Logger;
using Microsoft.Extensions.Hosting;

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