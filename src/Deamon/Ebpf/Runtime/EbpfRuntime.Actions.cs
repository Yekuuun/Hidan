using Mango;

namespace Deamon.Ebpf.Runtime;

/// <summary>
/// Contains all public actions available.
/// </summary>
internal partial class EbpfRuntime
{
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
}