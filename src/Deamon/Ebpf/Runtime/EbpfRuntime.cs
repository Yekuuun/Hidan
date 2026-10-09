using Mango;
using Deamon.Logger;
using Deamon.Ebpf.Abstraction;
using Microsoft.Extensions.Configuration;
using Deamon.Ebpf.Config;
using Deamon.Ebpf.Utils;
using Deamon.Ebpf.Mapping;
using Mango.Interops;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Global class encapsulating Ebpf runtime.
/// </summary>
internal partial class EbpfRuntime : IEbpfRawEventSource, IEbpfMapActions, IEbpfProgramActions, IDisposable
{
    #region CONF
    private readonly EbpfConfiguration _bpfConfig;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _semLock = new(1, 1);
    #endregion

    #region HANDLES
    private volatile EbpfState _state = EbpfState.NotLoaded;

    private BpfObject? _bpfObject = null;
    private readonly List<BpfLink> _bpfLinks = [];
    private readonly Dictionary<string, BpfMap> _bpfMaps = [];
    private bool _disposed = false;

    //temp. => on build from Singleton IEbpfMapConfig injection
    private readonly Dictionary<string, IEbpfMapConfig> _mapConfigs = [];
    #endregion

    #region CORE

    public string GetProgName => _bpfConfig.ProgramName;
    public string GetProgPath => _bpfConfig.ProgramPath;

    public EbpfRuntime(EbpfConfiguration bpfConfig, IConfiguration configuration, IEnumerable<IEbpfMapConfig> mapInit)
    {
        _bpfConfig = bpfConfig;
        _configuration = configuration;

        //tmp mapsInit
        foreach(var map in mapInit)
            _mapConfigs[map.Name] = map;
    }

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
                DeamonLogger.WriteLog(ELogError.WARNING, $"{_bpfConfig.ProgramName} already loaded, skipping.");
                return true;
            }

            //maps are validated before attaching anything, so a mismatched .o
            //fails before we touch kernel state.
            if(!LoadObject() || !LoadMaps() || !LoadPrograms() || !LoadRingBuffer())
            {
                CleanUpUnsafe();
                return false;
            }

            /*
            * NOTE ? Creating a more generic struct containing both IEbpfMapConfig & bpfMaps ? 
            */
            //_tmpMapsInit.Clear();
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

        if(!EbpfUtils.IsValidProgFile(_bpfConfig.ProgramPath))
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "Invalid ebpf object file.");
            return false;
        }

        var openResult = BpfObject.Open(_bpfConfig.ProgramPath);
        if(!openResult.IsSuccess)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, $"Error opening {_bpfConfig.ProgramPath} with error : {openResult.Error}");
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
    /// Resolve & keep every map the deamon expects to find in the object.
    /// </summary>
    /// <returns>true if every expected map was found. false otherwise.</returns>
    private bool LoadMaps()
    {
        if(_bpfObject == null)
        {
            DeamonLogger.WriteLog(ELogError.ERROR, "Bpf object not set.");
            return false;
        }

        if(_mapConfigs.Keys.Count == 0)
        {
            DeamonLogger.WriteLog(ELogError.WARNING, "No map declared...");
            return true;
        }

        //load maps.
        foreach(KeyValuePair<string, IEbpfMapConfig> mapConfig in _mapConfigs)
        {
            string mapName = mapConfig.Key;
            var conf = mapConfig.Value;

            if(conf.MapType == BpfMapType.Ringbuf)
                continue;

            var map = _bpfObject.FindMap(mapName);
            if(map is null)
            {
                DeamonLogger.WriteLog(ELogError.ERROR, $"Map '{mapName}' not found in {_bpfConfig.ProgramPath}");
                return false;
            }

            //add default data.
            IReadOnlyDictionary<byte[], byte[]> keyValues = conf.AddDefaultKeyValuesOnLoad();
            if(keyValues.Any())
            {
                foreach(KeyValuePair<byte[], byte[]> keyValuePair in keyValues)
                    map.TryUpdate(keyValuePair.Key, keyValuePair.Value);
            }
            //-------------------------------------------------------------------------------

            _bpfMaps[mapName] = map;
            DeamonLogger.WriteLog(ELogError.OK, $"Map {mapName} found.");
        }        
        DeamonLogger.WriteLog(ELogError.OK, $"{_bpfMaps.Count} map(s) resolved.");

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
    /// Shutdown runtime.
    /// </summary>
    public void ShutDown() => Dispose();

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
            _mapConfigs.Clear();
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
        CleanBpfMaps();
        CleanBpfLinks();
        CleanBpfObject();

        _state = EbpfState.Stopped;
    }

    /// <summary>
    /// Maps are borrowed from the bpf object & freed by bpf_object__close, so
    /// we only drop our references here.
    /// </summary>
    private void CleanBpfMaps()
    {
        if(_bpfMaps.Count == 0)
            return;

        int dropped = _bpfMaps.Count;
        _bpfMaps.Clear();

        DeamonLogger.WriteLog(ELogError.OK, $"{dropped} map reference(s) dropped.");
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