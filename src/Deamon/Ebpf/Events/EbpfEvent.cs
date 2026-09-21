namespace Deamon.Ebpf.Events;
 
/// <summary>
/// One validated record : its header, plus its parsed payload.
/// </summary>
internal readonly record struct EbpfEvent(EbpfEventHeader Header, EbpfEventPayload Payload)
{
    public override string ToString() =>
        $"[{Header.Type}] ts={EbpfClock.ToUtc(Header.Timestamp):yyyy-MM-dd HH:mm:ss} name={Payload.EventName} desc={Payload.EventDesc}";
}