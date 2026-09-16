using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

/// <summary>
/// The daemon's main kernel ring buffer : a stream of records, not a keyed
/// store, so it carries no default entries.
/// </summary>
internal sealed class MapRingbuffer() : BaseMapConfig(BpfMapType.Ringbuf)
{
    public override string Name => "event_output";
    public override string Description => "Main ring buffer for the Hidan project";

    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad() => new Dictionary<byte[], byte[]>();
}
