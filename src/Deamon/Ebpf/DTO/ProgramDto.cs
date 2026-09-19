using Mango.Interops;

namespace Deamon.Ebpf.DTO;

internal record struct ProgramDto(
    string Name, 
    int Fd,
    BpfProgramType ProgramType
);