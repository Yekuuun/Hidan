using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapCacheHideDir() : BaseMapConfig(BpfMapType.Hash)
{
    public override string Name => "hide_cache_dir";

    public override string Description => "Dir's to hide";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad() => new Dictionary<byte[], byte[]>();
}