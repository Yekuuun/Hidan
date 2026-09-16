using Mango.Interops;

namespace Deamon.Ebpf.Abstraction;

/// <summary>
/// Metadata every loaded map exposes, whatever its value type. Kept
/// non-generic so the loader can hold one heterogeneous list of configs.
/// </summary>
internal interface IEbpfMapConfig
{
    string Name {get;}
    string Description {get;}

    BpfMapType MapType {get;}

    IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad();
}
