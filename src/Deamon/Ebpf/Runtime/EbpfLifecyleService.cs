using Deamon.Logger;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Deamon.Ebpf.Runtime;

internal sealed class EbpfLifecycleService(EbpfRuntime runtime) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        bool load = await runtime.LoadAsync();
        if(!load)
            return;

        DeamonLogger.WriteLog(ELogError.OK, $"{runtime.GetProgName} running.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.Run(() => runtime.ShutDown(), cancellationToken);
}