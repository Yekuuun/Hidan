namespace Deamon.Ebpf.Abstraction;

internal interface IEbpfMapActions
{
    MapDto? TryGetMap(string name);
    List<MapDto> ListAllMaps();

    (bool, string? err) TryUpdateMapValue(string name, byte[] key, byte[] value);
    (bool, string? err) TryRemoveElement(string name, byte[] key);
}