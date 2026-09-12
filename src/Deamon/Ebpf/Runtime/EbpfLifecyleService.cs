using Deamon.Logger;
using Microsoft.Extensions.Hosting;
namespace Deamon.Ebpf.Runtime;

internal sealed class EbpfLifecycleService(EbpfRuntime runtime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        bool load = await runtime.LoadAsync(cancellationToken);
        if(!load)
            throw new InvalidOperationException($"Failed to load ebpf program '{runtime.GetProgName}'.");

        DeamonLogger.WriteLog(ELogError.OK, $"{runtime.GetProgName} running.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        runtime.ShutDown();
        return Task.CompletedTask;
    }
}