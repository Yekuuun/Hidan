using System.Buffers.Binary;
using Deamon.Ebpf.Config;

namespace Deamon.Ebpf.Events;

/// <summary>
/// Fixed header prefixing every record written to the ring buffer.
/// </summary>
/// <remarks>
/// Mirrors the packed struct from the bpf side :
/// <code>
/// typedef struct ebpf_event_hdr {
///     __u8  type;       // offset 0
///     __u16 size;       // offset 1
///     __u64 timestamp;  // offset 3
/// } __attribute__((packed));
/// </code>
/// Being packed, there is no padding and the fields are not aligned — which is
/// fine here, BinaryPrimitives reads at arbitrary offsets.
/// <para>
/// <see cref="Timestamp"/> is the raw value of bpf_ktime_get_ns() — nanoseconds
/// since boot, not since the Unix epoch. Use <see cref="EbpfClock"/> to convert
/// it to a wall-clock time.
/// </para>
/// </remarks>
internal readonly record struct EbpfEventHeader(EbpfEventType Type, ushort Size, ulong Timestamp)
{
    //Size, in bytes, of the header itself.
    internal const int HeaderSize = 11;

    /// <summary>
    /// Reads the header off the front of a record.
    /// </summary>
    internal static bool TryParse(ReadOnlySpan<byte> data, out EbpfEventHeader header)
    {
        header = default;

        if(data.Length < HeaderSize)
            return false;

        header = new EbpfEventHeader(
            (EbpfEventType)data[0],
            BinaryPrimitives.ReadUInt16LittleEndian(data[1..3]),
            BinaryPrimitives.ReadUInt64LittleEndian(data[3..11]));

        return true;
    }
}