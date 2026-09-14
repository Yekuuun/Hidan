using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;
using Microsoft.Extensions.Hosting;

namespace Deamon.Cmd.Commands;

internal sealed class QuitCommand(IHostApplicationLifetime lifetime) : ICmdCommand
{
    public string Name => "quit";
    public string Description => "Quit Hidan Deamon application.";

    public void Execute(string[] args, IOutputCommand output) => lifetime.StopApplication();
}
