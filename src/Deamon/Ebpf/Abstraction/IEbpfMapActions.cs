namespace Deamon.Ebpf.Abstraction;

internal interface IEbpfMapActions
{
    List<MapDto> ListAllMaps();
    MapDto? TryGetMap(string name);
}