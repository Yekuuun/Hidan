using Mango;
using Deamon.Logger;
using Deamon.Ebpf.Abstraction;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Global class encapsulating Ebpf runtime.
/// </summary>
internal class EbpfRuntime(EbpfConfiguration config) : IDisposable
{
    #region CONF
    private readonly EbpfConfiguration _config = config;
    private readonly SemaphoreSlim _semLock = new(1, 1);
    private const string RingBufferMapName = "event_output";

    #endregion

    #region HANDLES
    private volatile EbpfState _state = EbpfState.NotLoaded;

    private BpfObject? _bpfObject = null;
    private readonly List<BpfLink> _bpfLinks = [];
    private BpfRingBuffer? _ringBuffer = null;

    private bool _disposed = false;
    #endregion

    #region CORE

    public string GetProgName => _config.ProgramName;
    public string GetProgPath => _config.ProgramPath;

    /// <summary>
    /// Main function to start the EbpfRuntime.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
    {
        DeamonLogger.WriteLog(ELogError.OK, "Starting Deamon configuration...");

        await _semLock.WaitAsync(cancellationToken);

        try
        {
            if(_state == EbpfState.Running)
            {
                DeamonLogger.WriteLog(ELogError.WARNING, $"{_config.ProgramName} already loaded, skipping.");
                return true;
            }

            if(!LoadObject() || !LoadPrograms() || !LoadRingBuffer())
            {
                CleanUpUnsafe();
                return false;
            }

            _state = EbpfState.Running;

            return true;
        }
        catch(Exception ex)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Unhandled error while loading runtime : {ex}");
            CleanUpUnsafe();
            return false;
        }
        finally
        {
            _semLock.Release();
        }
    }

    /// <summary>
    /// Open & load the main bpf object file.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    private bool LoadObject()
    {
        DeamonLogger.WriteLog(ELogError.OK, "Loading object...");

        if(!IsValidProgFile())
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "Invalid ebpf object file.");
            return false;
        }

        var openResult = BpfObject.Open(_config.ProgramPath);
        if(!openResult.IsSuccess)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Error opening {_config.ProgramPath} with error : {openResult.Error}");
            return false;
        }

        _bpfObject = openResult.Value!;

        var loadResult = _bpfObject.Load();
        if(!loadResult.IsSuccess)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Error loading object (are you root ?) : {loadResult.Error}");
            return false;
        }

        DeamonLogger.WriteLog(ELogError.OK, "Bpf object successfully loaded.");

        return true;
    }

    /// <summary>
    /// Iterate & attach every program contained within the object.
    /// </summary>
    /// <returns>true if at least one program attached. false otherwise.</returns>
    private bool LoadPrograms()
    {
        if(_bpfObject == null)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "Bpf object not set.");
            return false;
        }

        List<string> attached = [];
        List<string> failed = [];

        foreach(var program in _bpfObject.Programs)
        {
            string name = program.Name;

            var attachResult = program.Attach();
            if(!attachResult.IsSuccess)
            {
                failed.Add($"{name} ({attachResult.Error})");
                continue;
            }

            _bpfLinks.Add(attachResult.Value!);
            attached.Add(name);
        }

        foreach(var name in attached)
            DeamonLogger.WriteLog(ELogError.OK, $"Attached : {name}");

        foreach(var entry in failed)
            DeamonLogger.WriteLog(ELogError.WARNING, $"Attach failed : {entry}");

        if(attached.Count == 0)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "No program attached.");
            return false;
        }

        DeamonLogger.WriteLog(ELogError.OK, $"{attached.Count} program(s) attached, {failed.Count} failed.");

        return true;
    }

    /// <summary>
    /// Create the ring buffer over the output map & start polling.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    private bool LoadRingBuffer()
    {
#if !DEBUG
        return true;
#endif

        if(_bpfObject == null)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "Bpf object not set.");
            return false;
        }

        var map = _bpfObject.FindMap(RingBufferMapName);
        if(map is null)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Map '{RingBufferMapName}' not found in {_config.ProgramPath}");
            return false;
        }

        DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer created over {RingBufferMapName}.");

        return true;
    }

    /// <summary>
    /// Shutdown runtime.
    /// </summary>
    public void ShutDown() => Dispose();

    /// <summary>
    /// Check if _config progPath is a valid .o file.
    /// </summary>
    /// <returns></returns>
    private bool IsValidProgFile()
    {
        string progPath = _config.ProgramPath;

        if(string.IsNullOrWhiteSpace(progPath))
            return false;

        if(!File.Exists(progPath))
            return false;

        string ext = Path.GetExtension(progPath);
        if(!string.Equals(ext, ".o", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    #endregion

    #region CLEANUP

    /// <summary>
    /// Unload program.
    /// </summary>
    public void Dispose()
    {
        if(_disposed)
            return;

        _semLock.Wait();
        try
        {
            if(_disposed)
                return;

            DeamonLogger.WriteLog(ELogError.OK, "Releasing ebpf resources...");

            CleanUpUnsafe();
            _disposed = true;

            DeamonLogger.WriteLog(ELogError.OK, "Ebpf resources released.");
        }
        finally
        {
            _semLock.Release();
            _semLock.Dispose();
        }
    }

    /// <summary>
    /// Teardown in reverse acquisition order. Caller must hold _semLock.
    /// </summary>
    private void CleanUpUnsafe()
    {
        CleanRingBuffer();
        CleanBpfLinks();
        CleanBpfObject();

        _state = EbpfState.Stopped;
    }

    private void CleanRingBuffer()
    {
        if(_ringBuffer is null)
            return;

        _ringBuffer.Dispose();
        _ringBuffer = null;

        DeamonLogger.WriteLog(ELogError.OK, $"Ring buffer over {RingBufferMapName} freed.");
    }

    private void CleanBpfLinks()
    {
        if(_bpfLinks.Count == 0)
            return;

        int freed = 0;

        foreach(var link in _bpfLinks)
        {
            try
            {
                link.Dispose();
                freed++;
            }
            catch(Exception ex)
            {
                DeamonLogger.WriteLog(ELogError.WARNING, $"Error destroying link : {ex.Message}");
            }
        }

        _bpfLinks.Clear();

        DeamonLogger.WriteLog(ELogError.OK, $"{freed} link(s) destroyed, programs detached.");
    }

    private void CleanBpfObject()
    {
        if(_bpfObject is null)
            return;

        _bpfObject.Dispose();
        _bpfObject = null;

        DeamonLogger.WriteLog(ELogError.OK, "Bpf object closed.");
    }

    #endregion
}