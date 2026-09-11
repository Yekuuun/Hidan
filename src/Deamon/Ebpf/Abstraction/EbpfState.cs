namespace Deamon.Ebpf.Abstraction;

internal enum EbpfState : int
{
    NotLoaded = 0x0,
    Running   = 0x1,
    Stopped   = 0x2
}