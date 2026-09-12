using System.Threading.Channels;
using Deamon.Ebpf.Config;
using Deamon.Logger;
using Mango;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Dedicated EbpfRuntime class extension for better visibility.
/// </summary>
internal partial class EbpfRuntime
{
    #region RINGBUFFER_CONFIG

    private BpfRingBuffer? _ringBuffer = null;
    private readonly Channel<byte[]> _rawEvents = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(8192)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleWriter = true,
        SingleReader = false
    });

    private Action<ReadOnlySpan<byte>>? _onEventCallback = null;
    private Thread? _pollThread = null;
    private CancellationTokenSource? _pollCts = null;

    #endregion 

    #region RINGBUFFER_BUSINESS

    /// <summary>
    /// Reads raw ring buffer records until the runtime stops.
    /// </summary>
    public IAsyncEnumerable<byte[]> ReadRawAsync(CancellationToken cancellationToken = default) => _rawEvents.Reader.ReadAllAsync(cancellationToken);

    /// <summary>
    /// Resolve the ring buffer output map. Polling is not wired up yet.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    private bool LoadRingBuffer()
    {
        if(_bpfObject is null)
            return false;

        string ringBufferMapName = EbpfMaps.RingbufferMapName;

        var map = _bpfObject.FindMap(ringBufferMapName);
        if(map is null)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Unable to find {ringBufferMapName} map.");
            return false;
        }

        DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer map {ringBufferMapName} ready.");

        _onEventCallback = OnEvent;
        var result = BpfRingBuffer.Create(map, _onEventCallback);
        if(!result.IsSuccess)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Unable to create ring buffer : {result.Error}");
            return false;
        }

        _ringBuffer = result.Value!;

        StartPolling();

        DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer polling {EbpfMaps.RingbufferMapName}.");

        return true;
    }

    /// <summary>
    /// Main thread function for handling polling from the ringbuffer.
    /// </summary>
    private void StartPolling()
    {
        if(_ringBuffer is null)
            return;

        _pollCts  = new();
        var token = _pollCts.Token;

        _pollThread = new Thread(() =>
        {
            try
            {
                while(!token.IsCancellationRequested)
                    _ringBuffer.Poll(timeoutMs:200);
            }
            catch(Exception ex)
            {
                DeamonLogger.WriteLog(ELogError.ERROR, $"Polling stopped with error : {ex.Message}");
            }
        })
        {
            IsBackground = true,
            Name = "ebpf-ringbuffer-poll"
        };

        _pollThread.Start();
    }

    private bool StopPolling()
    {
        if(_pollThread is null)
            return true;

        try
        {
            _pollCts?.Cancel();

            //timeout above the poll timeout, to let the in-flight call return.
            return !_pollThread.IsAlive || _pollThread.Join(TimeSpan.FromSeconds(2));
        }
        finally
        {
            _pollCts?.Dispose();
            _pollCts = null;
            _pollThread = null;
        }
    }

    private void OnEvent(ReadOnlySpan<byte> data)
    {
        try
        {
            var copy = data.ToArray();
            _rawEvents.Writer.TryWrite(copy);
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Error queuing record : {ex.Message}");
        }
    }

    #endregion

    #region CLEANUP
    private void CleanRingBuffer()
    {
        _rawEvents.Writer.TryComplete();

        if(!StopPolling())
        {
            DeamonLogger.WriteLog(ELogError.WARNING, "Ring buffer left allocated, poll thread still alive.");
            return;
        }

        if(_ringBuffer is null)
            return;

        _ringBuffer.Dispose();
        _ringBuffer = null;
        _onEventCallback = null;

        DeamonLogger.WriteLog(ELogError.OK, "Ring buffer freed.");
    }

    #endregion 
}