using System.Threading.Channels;
using Deamon.Ebpf.Abstraction;
using Deamon.Ebpf.Config;
using Deamon.Logger;
using Mango;
using Mango.Interops;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Dedicated EbpfRuntime class extension for better visibility.
/// </summary>
internal partial class EbpfRuntime
{
    #region RINGBUFFER_CONFIG

    //libbpf reports -EINTR when a signal lands during the poll.
    private const int LibbpfEintr = -4;

    private BpfRingBuffer? _ringBuffer = null;
    private readonly Channel<byte[]> _rawEvents = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(8192)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
        SingleWriter = true,
        SingleReader = false
    });

    private Thread? _pollThread = null;
    private CancellationTokenSource? _pollCts = null;

    #endregion 

    #region RINGBUFFER_BUSINESS

    /// <summary>
    /// Reads raw ring buffer records until the runtime stops.
    /// </summary>
    public IAsyncEnumerable<byte[]> ReadRawAsync(CancellationToken cancellationToken = default) => _rawEvents.Reader.ReadAllAsync(cancellationToken);

    /// <summary>
    /// Resolve the ring buffer output map & start consuming it.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    private bool LoadRingBuffer()
    {
        if(_bpfObject is null)
            return false;


        foreach(KeyValuePair<string, IEbpfMapConfig> mapConfig in _mapConfigs)
        {
            string mapName = mapConfig.Key;
            var conf = mapConfig.Value;

            if(conf.MapType != BpfMapType.Ringbuf)
                continue;

            var map = _bpfObject.FindMap(mapName);
            if(map is null)
            {
                DeamonLogger.WriteLog(ELogError.ERROR, $"Unable to find {mapName} map.");
                return false;
            }

            DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer map {mapName} ready.");

            //Mango.Libbpf >= 0.0.4 roots the native callback for the manager's
            //lifetime : keeping _ringBuffer alive is enough to keep it alive.
            var result = BpfRingBuffer.Create(map, OnEvent);
            if(!result.IsSuccess)
            {
                DeamonLogger.WriteLog(ELogError.ERROR, $"Unable to create ring buffer : {result.Error}");
                return false;
            }

            _ringBuffer = result.Value!;

            StartPolling();

            DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer polling {mapName}.");

        }

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

        //captured : the field is cleared on cleanup, and cleanup only runs
        //once this thread has joined.
        var ringBuffer = _ringBuffer;

        _pollThread = new Thread(() =>
        {
            try
            {
                while(!token.IsCancellationRequested)
                {
                    int result = ringBuffer.Poll(timeoutMs: 200);

                    //a signal during the poll is not a failure.
                    if(result < 0 && result != LibbpfEintr)
                    {
                        DeamonLogger.WriteLog(ELogError.ERROR, $"Polling stopped, ring_buffer__poll returned {result}.");
                        break;
                    }
                }
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

    /// <summary>
    /// Invoked by libbpf on the poll thread for each record. Mango catches
    /// anything thrown here & writes it to stderr, which would scribble over
    /// the terminal UI : nothing is allowed to leave this frame.
    /// </summary>
    /// <param name="data">only valid for the duration of this call.</param>
    private void OnEvent(ReadOnlySpan<byte> data)
    {
        try
        {
            _rawEvents.Writer.TryWrite(data.ToArray());
        }
        catch(Exception ex)
        {
            try
            {
                DeamonLogger.WriteLog(ELogError.ERROR, $"Error queuing record : {ex.Message}");
            }
            catch
            {
                //the logger is the last thing left to fail : swallow it.
            }
        }
    }

    #endregion

    #region CLEANUP

    private void CleanRingBuffer()
    {
        _rawEvents.Writer.TryComplete();

        //disposing under a live poll would pull the handle out from under it.
        if(!StopPolling())
        {
            DeamonLogger.WriteLog(ELogError.WARNING, "Ring buffer left allocated, poll thread still alive.");
            return;
        }

        if(_ringBuffer is null)
            return;

        _ringBuffer.Dispose();
        _ringBuffer = null;

        DeamonLogger.WriteLog(ELogError.OK, "Ring buffer freed.");
    }

    #endregion 
}
