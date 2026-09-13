using Deamon.Ebpf.Abstraction;
using Deamon.Logger;
using Microsoft.Extensions.Hosting;

namespace Deamon.Ebpf.Debug;

/// <summary>
/// Used for debugging without TERMINAL.GUI.
/// 
/// Add service into ServiceCollectionExtension to activate raw console logging.
/// </summary>
/// <param name="reader"></param>
[Obsolete("Using Terminal.GUI for logging.")]
internal sealed class EbpfEventLoggerService(IEbpfEventReader reader) : BackgroundService
{
    private readonly IEbpfEventReader _reader = reader;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        DeamonLogger.WriteLog(ELogError.OK, "Event logger started, draining events...");

        try
        {
            await foreach(var evt in _reader.ReadAsync(stoppingToken))
                DeamonLogger.WriteLog(ELogError.EVENT, $"EVENT {evt}");
        }
        catch(OperationCanceledException)
        {
            //silence.
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Event logger stopped on error : {ex}");
        }

        DeamonLogger.WriteLog(ELogError.OK,
            $"Event logger stopped ({_reader.EventsRead} read, {_reader.EventsMalformed} malformed).");
    }
}