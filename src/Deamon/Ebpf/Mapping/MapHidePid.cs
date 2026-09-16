using Deamon.Ebpf.Utils;
using Deamon.Utils;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapHidePid() : BaseMapConfig(BpfMapType.Hash)
{
    public override string Name => "hide_pid_cache";

    public override string Description => "simple maps for pid's to hide";

    /// <summary>
    /// Return Deamon process id.
    /// </summary>
    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad()
    {
        return new Dictionary<byte[], byte[]>(ByteArrayComparer.Instance)
        {
            [EbpfUtils.ToBytes(Environment.ProcessId)] = EbpfUtils.ToBytes((byte)1) 
        };
    }
}