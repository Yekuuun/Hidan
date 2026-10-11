namespace Deamon.Ebpf.Abstraction;

public record struct EbpfMapTypeObj(
    Type Key,
    Type Val
);