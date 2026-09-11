using Mango;
using Deamon.Logger;
using Deamon.Ebpf.Abstraction;
using Microsoft.Extensions.Logging;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Global class encapsulating Ebpf runtime.
/// </summary>
internal class EbpfRuntime(EbpfConfiguration config, ILogger<EbpfRuntime> logger) : IDisposable
{
    #region CONF
    private readonly EbpfConfiguration _config = config;
    private readonly ILogger<EbpfRuntime> _logger = logger;
    private readonly SemaphoreSlim _semLock = new(1, 1);
    #endregion

    #region HANDLES
    private volatile EbpfState _state = EbpfState.NotLoaded;
    private BpfObject? _bpfObject = null;

    #endregion

    #region CORE

    public string GetProgName => _config.ProgramName;
    public string GetProgPath => _config.ProgramPath;

    /// <summary>
    /// Main function to start the EbpfRuntime.
    /// </summary>
    /// <returns>true if success. false if error occured.</returns>
    public async Task<bool> LoadAsync()
    {
        try
        {
            await _semLock.WaitAsync();

            if (!IsValidProgFile())
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

            //do something.
            //....

            _state = EbpfState.Running;
            return true;
        }
        catch(Exception)
        {
            //log.

            return false;
        }
        finally
        {
            _semLock.Release();
        }
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
        if(!string.Equals(ext, ".o"))
            return false;

        return true;
    }

    #endregion

    #region CLEANUP

    /// <summary>
    /// Unload program.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Dispose()
    {
        CleanBpfObject();
    }

    private void CleanBpfObject()
    {
        _bpfObject?.Dispose();

        _bpfObject = null;
        _state     = EbpfState.Stopped;
    }

    #endregion
}