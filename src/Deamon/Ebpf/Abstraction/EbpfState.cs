namespace Deamon.Ebpf.Abstraction;

/// <summary>
/// Handle EBPF runtime state.
/// </summary>
internal enum EbpfState : int
{
    NotLoaded = 0x0,
    Running   = 0x1,
    Stopped   = 0x2
}