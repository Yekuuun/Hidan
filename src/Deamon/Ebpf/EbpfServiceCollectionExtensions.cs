using Deamon.Ebpf.Abstraction;
using Deamon.Ebpf.Debug;
using Deamon.Ebpf.Events;
using Deamon.Ebpf.Runtime;
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
 
        services.AddSingleton<EbpfRuntime>();
 
        //same instance behind the interface, or the reader would listen to a
        services.AddSingleton<IEbpfRawEventSource>(sp => (IEbpfRawEventSource)sp.GetRequiredService<EbpfRuntime>());
        services.AddSingleton<IEbpfMapActions>(sp => (IEbpfMapActions)sp.GetRequiredService<EbpfRuntime>());

        services.AddSingleton<IEbpfEventReader, EbpfEventReader>();
 
        //order matters : the lifecycle service must create the ring buffer
        //before the logger starts draining it.
        services.AddHostedService<EbpfLifecycleService>();
        return services;
    }
}