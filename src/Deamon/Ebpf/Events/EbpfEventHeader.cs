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
///     __u32 timestamp;  // offset 3
/// } __attribute__((packed));
/// </code>
/// Being packed, there is no padding and the fields are not aligned — which is
/// fine here, BinaryPrimitives reads at arbitrary offsets.
/// </remarks>
internal readonly record struct EbpfEventHeader(EbpfEventType Type, ushort Size, uint Timestamp)
{
    //Size, in bytes, of the header itself.
    internal const int HeaderSize = 7;

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
            BinaryPrimitives.ReadUInt32LittleEndian(data[3..7]));

        return true;
    }
}