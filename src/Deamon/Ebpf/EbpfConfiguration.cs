namespace Deamon.Ebpf;

public sealed class EbpfConfiguration
{
    /// <summary>
    /// nameof loaded ebpf program.
    /// </summary>
    public required string ProgramName {get; init;}
    
    /// <summary>
    /// path to ebpf program to load.
    /// </summary>
    public required string ProgramPath {get; init;}
}