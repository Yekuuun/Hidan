namespace Deamon.Ebpf.Events;

/// <summary>
/// Converts <see cref="EbpfEventHeader.Timestamp"/> — nanoseconds since boot,
/// as returned by bpf_ktime_get_ns() — into wall-clock time.
/// </summary>
internal static class EbpfClock
{
    private static readonly DateTime sReferenceUtc = DateTime.UtcNow;
    private static readonly TimeSpan sReferenceUptime = TimeSpan.FromMilliseconds(Environment.TickCount64);

    /// <summary>
    /// Converts a boot-relative timestamp, in nanoseconds, to UTC wall-clock time.
    /// </summary>
    internal static DateTime ToUtc(ulong bootNanoseconds)
    {
        var uptime = TimeSpan.FromTicks((long)(bootNanoseconds / 100));
        var bootUtc = sReferenceUtc - sReferenceUptime;

        return bootUtc + uptime;
    }
}
