using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Deamon.Ebpf;

/// <summary>
/// Main singleton class to handle EbpfRuntime.
/// </summary>
internal sealed partial class EbpfRuntime : IHostedService, IDisposable
{
    private readonly EbpfOptions _options;
    private readonly ILogger<EbpfRuntime> _logger;
    private volatile EbpfState _state = EbpfState.NotLoaded;

    public EbpfRuntime(ILogger<EbpfRuntime> logger, EbpfOptions options)
    {
        _logger  = logger;
        _options = options;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}