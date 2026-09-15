using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;
using Microsoft.Extensions.Hosting;

namespace Deamon.Cmd.Commands;

internal sealed class QuitCommand(IHostApplicationLifetime lifetime) : CliCommandBase
{
    public override string Name => "quit";

    public override string Description => "Quit Hidan Deamon application.";

    public override ReadOnlyCollection<string> Aliases => ["exit", "leave"];

    protected override void Configure(Command command, IOutputCommand output) => command.SetAction(_ => lifetime.StopApplication());
}
