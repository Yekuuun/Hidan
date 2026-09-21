namespace Deamon.Ebpf.Events;

/// <summary>
/// Base class in charge of the debugging config.
/// </summary>
internal sealed class EbpfDebugConfig
{
    private readonly Lock _accessLock = new();
    private bool debugActive = true;

    public void SetDebugStatus(bool status)
    {
        lock(_accessLock)
        {
            if(status != debugActive)
                debugActive = status;
        }
    }

    public bool GetStatus()
    {
        lock(_accessLock)
        {
            return debugActive;
        }   
    }
}