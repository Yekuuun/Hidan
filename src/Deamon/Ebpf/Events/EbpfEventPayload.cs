using System.Text;

namespace Deamon.Ebpf.Events;

/// <summary>
/// Fixed-size payload following every record's header.
/// </summary>
/// <remarks>
/// Mirrors the packed struct from the bpf side :
/// <code>
/// typedef struct ebpf_event_payload {
///     char event_name[MAX_EVENT_NAME]; // offset 0,   128B
///     char event_desc[MAX_EVENT_DESC]; // offset 128, 512B
/// } __attribute__((packed));
/// </code>
/// Each field is a NUL-terminated (or NUL-padded) C string ; bytes past the
/// first NUL are ignored.
/// </remarks>
internal readonly record struct EbpfEventPayload(string EventName, string EventDesc)
{
    //Size, in bytes, of each fixed field on the bpf side.
    internal const int EventNameSize = 128;
    internal const int EventDescSize = 512;

    //Size, in bytes, of the payload itself.
    internal const int PayloadSize = EventNameSize + EventDescSize;

    /// <summary>
    /// Reads the payload off the back of a record (right after the header).
    /// </summary>
    internal static bool TryParse(ReadOnlySpan<byte> data, out EbpfEventPayload payload)
    {
        payload = default;

        if(data.Length < PayloadSize)
            return false;

        var name = ReadCString(data[..EventNameSize]);
        var desc = ReadCString(data.Slice(EventNameSize, EventDescSize));

        payload = new EbpfEventPayload(name, desc);

        return true;
    }

    private static string ReadCString(ReadOnlySpan<byte> field)
    {
        var nul = field.IndexOf((byte)0);
        var bytes = nul < 0 ? field : field[..nul];

        return Encoding.UTF8.GetString(bytes);
    }
}
