namespace Deamon.Ebpf.Abstraction;

internal interface IEbpfProgramActions
{
    List<ProgramDto> ListPrograms();
    ProgramDto? FindProgram(string name);
}