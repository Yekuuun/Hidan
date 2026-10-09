using Deamon.Ebpf.Abstraction;
using Mango;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Contains all public actions available.
/// </summary>
internal partial class EbpfRuntime
{
    #region ACTIONS_PROGRAMS

    /// <summary>
    /// List all loaded programs.
    /// </summary>
    /// <returns></returns>
    public List<ProgramDto> ListPrograms()
    {
        if(!CheckState())
            return [];

        List<ProgramDto> programs = [];

        //listing.
        foreach(var prog in _bpfObject!.Programs)
        {
            programs.Add(new ProgramDto()
            {
                Name = prog.Name,
                Fd = prog.Fd,
                ProgramType = prog.Type
            });
        }

        return programs;
    }

    /// <summary>
    /// Find a single program by it's name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ProgramDto? FindProgram(string name)
    {
        if(!CheckState())
            return null;

        if(string.IsNullOrWhiteSpace(name))
            return null;

        List<ProgramDto> programs = [];

        //listing.
        foreach(var prog in _bpfObject!.Programs)
        {
            if(!string.Equals(prog.Name, name))
                continue;

            return new ProgramDto()
            {
                Name = prog.Name,
                Fd = prog.Fd,
                ProgramType = prog.Type
            };
        }

        return null;
    }

    #endregion

    #region ACTIONS_MAPS
    /// <summary>
    /// list all avalaible loaded maps & return MapDto object list.
    /// </summary>
    /// <returns></returns>
    public List<MapDto> ListAllMaps()
    {
        if(!CheckState())
            return [];

        List<MapDto> result = [];
        foreach(KeyValuePair<string, BpfMap> map in _bpfMaps)
        {
            string mapName = map.Key;
            BpfMap? mapObj = map.Value;

            if(mapObj == null)
                continue;

            result.Add(new MapDto()
            {
                Name = mapName,
                Fd   = mapObj.Fd,
                MapType = mapObj.Type,
                KeySize = mapObj.KeySize,
                ValueSize = mapObj.ValueSize,
                MaxEntries = mapObj.MaxEntries
            });
        }

        return result;
    }

    /// <summary>
    /// Try getting a single map using it's unique name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public MapDto? TryGetMap(string name)
    {
        if(string.IsNullOrEmpty(name))
            return null;

        if (!CheckState())
            return null;

        if(!_bpfMaps.TryGetValue(name, out BpfMap? map) || map == null)
            return null;

        return new MapDto()
        {
            Name = map.Name,
            Fd   = map.Fd,
            MapType = map.Type,
            KeySize = map.KeySize,
            ValueSize = map.ValueSize,
            MaxEntries = map.MaxEntries
        };
    }

    /// <summary>
    /// Try to update / add value inside a targetted map.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public (bool, string? err) TryUpdateMapValue(string name, byte[] key, byte[] value)
    {
        if(string.IsNullOrWhiteSpace(name))
            return default;

        //find map
        if (!CheckState())
            return default;

        if(!_bpfMaps.TryGetValue(name, out BpfMap? map) || !_mapConfigs.TryGetValue(name, out IEbpfMapConfig? conf))
            return default;

        if(conf is null)
            return default;

        //checking type.
        if(!ValidateMapDataTypes(key, value, map))
            return(false, "Bad data types");

        /*
        * Adding new key <> value using byte[] type
        * NOTE : not responsible of bad type passing....
        */
        try
        {
            bool response = map.TryUpdate(new ReadOnlySpan<byte>(key), new ReadOnlySpan<byte>(value));
            return(response, response ? string.Empty :"Error trying to update / add map element");
        }
        catch(Exception)
        {
            return default; //silence.
        }
    }

    public (bool, string? err) TryRemoveElement(string name, byte[] key)
    {
        if(string.IsNullOrWhiteSpace(name))
            return default;

        //find map
        if (!CheckState())
            return default;

        if(!_bpfMaps.TryGetValue(name, out BpfMap? map) || !_mapConfigs.TryGetValue(name, out IEbpfMapConfig? conf))
            return default;

        if(conf is null)
            return default;

        //checking type.
        if(key.Length != (int)map.KeySize)
            return(false, "bad data type");

        /*
        * Removing key <> value using byte[] type key
        * NOTE : not responsible of bad type passing....
        */
        try
        {
            bool response = map.TryDelete(new ReadOnlySpan<byte>(key));
            return(response, response ? string.Empty :"Error trying to remove map element");
        }
        catch(Exception)
        {
            return default; //silence.
        }
    }
    
    /// <summary>
    /// Base check for base map type based operations.
    /// </summary>
    /// <returns></returns>
    private bool CheckState() => _state == Abstraction.EbpfState.Running && _bpfMaps.Keys.Count > 0;
    
    /// <summary>
    /// Utility function for checking value & key sizes before trying to add / update map values.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private static bool ValidateMapDataTypes(byte[] key, byte[] val, BpfMap map) => (key.Length == (int)map.KeySize && val.Length == (int)map.ValueSize);

    #endregion
}