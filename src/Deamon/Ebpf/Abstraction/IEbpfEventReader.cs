using Deamon.Ebpf.Events;

namespace Deamon.Ebpf.Abstraction;

/// <summary>
/// Reads validated events off an <see cref="IEbpfRawEventSource"/>.
/// </summary>
internal interface IEbpfEventReader
{
    /// <summary>
    /// Reads events until the source completes or the token is cancelled.
    /// Malformed records are counted & dropped, never thrown.
    /// </summary>
    IAsyncEnumerable<EbpfEvent> ReadAsync(CancellationToken cancellationToken = default);

    long EventsRead { get; }
    long EventsMalformed { get; }
}