using System.Runtime.CompilerServices;
using Deamon.Ebpf.Abstraction;
using Deamon.Ebpf.Events;
using Deamon.Logger;

namespace Deamon.Ebpf.Logging;

/// <summary>
/// Validates the raw records produced by the runtime & turns them into
/// <see cref="EbpfEvent"/>. Knows the wire format, knows nothing about libbpf.
/// </summary>
internal sealed class EbpfEventReader(IEbpfRawEventSource source) : IEbpfEventReader
{
    private readonly IEbpfRawEventSource _source = source;

    private long _eventsRead = 0;
    private long _eventsMalformed = 0;

    public long EventsRead => Interlocked.Read(ref _eventsRead);
    public long EventsMalformed => Interlocked.Read(ref _eventsMalformed);

    public async IAsyncEnumerable<EbpfEvent> ReadAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach(var raw in _source.ReadRawAsync(cancellationToken))
        {
            if(!TryBuild(raw, out var evt))
            {
                Interlocked.Increment(ref _eventsMalformed);
                continue;
            }

            Interlocked.Increment(ref _eventsRead);

            yield return evt;
        }
    }

    /// <summary>
    /// Parses & validates one record. A bad record means the .o and this code
    /// drifted apart, so it is dropped with a warning rather than throwing.
    /// </summary>
    private static bool TryBuild(byte[] raw, out EbpfEvent evt)
    {
        evt = default;

        if(!EbpfEventHeader.TryParse(raw, out var header))
        {
            DeamonLogger.WriteLog(ELogError.WARNING,
                $"Record too short for a header : {raw.Length}B, need {EbpfEventHeader.HeaderSize}B.");
            return false;
        }

        //ASSUMPTION : hdr.size is the whole record, header included. If the bpf
        //side sets it to the payload length only, compare against
        //raw.Length - HeaderSize instead.
        if(header.Size != raw.Length)
        {
            DeamonLogger.WriteLog(ELogError.WARNING,
                $"[{header.Type}] header size {header.Size}B does not match record {raw.Length}B.");
            return false;
        }

        if(!Enum.IsDefined(header.Type))
        {
            DeamonLogger.WriteLog(ELogError.WARNING, $"Unknown event type : {(byte)header.Type}.");
            return false;
        }

        evt = new EbpfEvent(header, new ReadOnlyMemory<byte>(raw, EbpfEventHeader.HeaderSize, raw.Length - EbpfEventHeader.HeaderSize));

        return true;
    }
}