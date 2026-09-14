using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Abstraction;

internal interface ICmdCommand
{
    string Name {get; }
    string Description {get; }

    /// <summary>
    /// The caller owns the output : a command writes to whoever asked for it,
    /// it does not hold a reference to the front-end.
    /// </summary>
    void Execute(string[] args, IOutputCommand output);
}
