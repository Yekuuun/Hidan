namespace Deamon.Ebpf;

/// <summary>
/// Main enum to describe current ebpf service status.
/// </summary>
internal enum EbpfState : int
{
    NotLoaded = 0x0,
    Running   = 0x1
}