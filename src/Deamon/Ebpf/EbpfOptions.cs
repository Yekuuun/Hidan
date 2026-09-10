namespace Deamon.Ebpf;

internal sealed class EbpfOptions
{
    /// <summary>
    /// Path to the .bpf.o
    /// </summary>
    public required string ObjectPath {get; set;} = string.Empty;

    /// <summary>
    /// Name of program to attach.
    /// </summary>
    public string ProgName {get; set;} = string.Empty;

    ///OTHER.
    ///Restart policy.
}