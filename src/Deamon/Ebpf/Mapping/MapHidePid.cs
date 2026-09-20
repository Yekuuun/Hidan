using System.Collections.ObjectModel;
using System.Diagnostics;
using Deamon.Ebpf.Utils;
using Deamon.Utils;
using Mango.Interops;

namespace Deamon.Ebpf.Mapping;

internal sealed class MapHidePid() : BaseMapConfig(BpfMapType.Hash)
{
    //for testing purpose.
    private ReadOnlyCollection<string> defaultApps = ["gnome-text-editor"];
    public override string Name => "hide_pid_cache";

    public override string Description => "simple maps for pid's to hide";

    /// <summary>
    /// Return Deamon process id.
    /// </summary>
    public override IReadOnlyDictionary<byte[], byte[]> AddDefaultKeyValuesOnLoad()
    {
        Dictionary<byte[], byte[]> defaultPids = new(ByteArrayComparer.Instance)
        {
            //current app pid.
            [EbpfUtils.ToBytes(Environment.ProcessId)] = EbpfUtils.ToBytes((byte)1)
        };

        try
        {
            //default values.
            foreach(string progName in defaultApps)
            {
                if(string.IsNullOrEmpty(progName))
                    continue;

                Process[] procs = Process.GetProcessesByName(progName);
                if(procs.Length == 0)
                    continue;

                foreach(Process proc in procs)
                    defaultPids[EbpfUtils.ToBytes(proc.Id)] = EbpfUtils.ToBytes((byte)1);
            }
        }
        catch{}
        
        return defaultPids;
    }
}