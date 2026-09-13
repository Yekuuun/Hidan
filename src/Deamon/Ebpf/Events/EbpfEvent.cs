namespace Deamon.Ebpf.Events;
 
/// <summary>
/// One validated record : its header, plus the still-unparsed payload bytes.
/// </summary>
/// <remarks>
/// The payload is handed out raw on purpose — payload structs do not exist on
/// the bpf side yet. Once they do, a typed layer parses <see cref="Payload"/>
/// according to <see cref="EbpfEventHeader.Type"/>.
/// </remarks>
internal readonly record struct EbpfEvent(EbpfEventHeader Header, ReadOnlyMemory<byte> Payload)
{
    public override string ToString() => $"[{Header.Type}] ts={Header.Timestamp} payload={Payload.Length}B";
}