using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapCacheHideFile() : BaseMapConfig(BpfMapType.Hash)
{
    public override string Name => "hide_cache_file";

    public override string Description => "Files to hide";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad() => new Dictionary<byte[], byte[]>();
}