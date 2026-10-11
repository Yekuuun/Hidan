namespace Deamon.Ebpf.Abstraction;

internal interface IEbpfMapActions
{
    MapDto? TryGetMap(string name);
    List<MapDto> ListAllMaps();
    IEbpfMapConfig? GetMapConf(string name);
    (bool, string? err) TryUpdateMapValue(string name, byte[] key, byte[] value);
    (bool, string? err) TryRemoveElement(string name, byte[] key);
}