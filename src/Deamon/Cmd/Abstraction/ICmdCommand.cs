namespace Deamon.Cmd.Abstraction;

internal interface ICmdCommand
{
    string Name {get; }
    string Description {get; }
    void Execute(string[] args);
}