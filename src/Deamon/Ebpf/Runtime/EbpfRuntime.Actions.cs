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
        if(_state != Abstraction.EbpfState.Running)
            return [];

        if(_bpfObject == null)
            return [];

        List<ProgramDto> programs = [];

        //listing.
        foreach(var prog in _bpfObject.Programs)
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
        if(_state != Abstraction.EbpfState.Running)
            return null;

        if(_bpfObject == null)
            return null;

        if(string.IsNullOrWhiteSpace(name))
            return null;

        List<ProgramDto> programs = [];

        //listing.
        foreach(var prog in _bpfObject.Programs)
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
        if(_state != Abstraction.EbpfState.Running)
            return [];

        if(_bpfMaps.Keys.Count == 0)
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

        BpfMap? map = null;

        if(_state != Abstraction.EbpfState.Running)
            return null;

        if(_bpfMaps.Keys.Count == 0)
            return null;

        bool found = _bpfMaps.TryGetValue(name, out map);

        if(!found || map == null)
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

    #endregion
}