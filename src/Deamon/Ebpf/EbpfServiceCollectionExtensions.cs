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
        services.AddHostedService<EbpfLifecycleService>();

        return services;
    }
}