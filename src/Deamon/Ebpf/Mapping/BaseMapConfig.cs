using System.Runtime.InteropServices;
using Deamon.Ebpf.Abstraction;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

/// <summary>
/// Common metadata for a loaded map. Concrete maps that need seeding derive
/// from <see cref="SeededMapConfig{TValue}"/> instead.
/// </summary>
internal abstract class BaseMapConfig(BpfMapType type) : IEbpfMapConfig
{
    public abstract string Name {get;}
    public abstract string Description {get;}

    public BpfMapType MapType => type;

    public abstract IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad();
}
