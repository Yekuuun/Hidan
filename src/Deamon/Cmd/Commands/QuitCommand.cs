using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class QuidCommand(IAppLifeCycle lifeCycle) : ICmdCommand
{
    private readonly IAppLifeCycle _lifeCycle = lifeCycle;

    public string Name => "quit";
    public string Description => "Quit Hidan Deamon application.";

    public void Execute(string[] args) => _lifeCycle.RequestQuit();
}