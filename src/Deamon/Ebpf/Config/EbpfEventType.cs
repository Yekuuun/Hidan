namespace Deamon.Ebpf.Config;

internal enum EbpfEventType : int
{
    EVENT_DEBUG     = 0x1,
    EVENT_TRIGGERED = 0x2
}