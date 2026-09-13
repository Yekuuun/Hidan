namespace Deamon.Ebpf.Abstraction;

/// <summary>
/// Exposes the raw records consumed from the kernel ring buffer, with no
/// interpretation of their content.
/// </summary>
/// <remarks>
/// Each element is a private copy of one record : the span handed over by
/// libbpf only lives for the duration of the native callback, so nothing can
/// be handed out by reference.
/// </remarks>
internal interface IEbpfRawEventSource
{
    IAsyncEnumerable<byte[]> ReadRawAsync(CancellationToken cancellationToken = default);
}