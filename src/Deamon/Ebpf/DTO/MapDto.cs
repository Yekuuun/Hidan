using Mango.Interops;

namespace Deamon.Ebpf.DTO;

internal record struct MapDto(
    string Name,
    int Fd,
    BpfMapType MapType,
    uint KeySize,
    uint ValueSize,
    uint MaxEntries
);