using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class ClearCommand : CliCommandBase
{
    public override string Name => "clear";

    public override string Description => "Clear terminal entries";

    protected override void Configure(Command command, IOutputCommand output) => command.SetAction(_ => output.Clear());
}
