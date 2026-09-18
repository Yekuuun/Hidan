using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapGetDents64() : BaseMapConfig(BpfMapType.Hash)
{
    public override string Name => "getdents_cache";

    public override string Description => "Base cache for handling return for sys_getdents64 calls.";

     public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad() => new Dictionary<byte[], byte[]>();
}