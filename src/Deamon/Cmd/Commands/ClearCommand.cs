using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;
using Deamon.Gui.Config;

namespace Deamon.Cmd.Commands;

internal sealed class ClearCommand : CliCommandBase
{
    public override string Name => "clear";

    public override string Description => "Clear terminal entries";

    protected override void Configure(Command command, IOutputCommand output)
    {
        //bare 'clear' keeps wiping both panes : that is what it used to do.
        var paneArg = new Argument<string>("pane")
        {
            Description = "console (command output), events (ring buffer) or both.",
            Arity       = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = _ => "both"
        };

        paneArg.AcceptOnlyFromAmong("console", "events", "both");
        command.Arguments.Add(paneArg);

        command.SetAction(pr => output.Clear(pr.GetValue<string>(paneArg) switch
        {
            "console" => EOutputPane.Command,
            "events"  => EOutputPane.Events,
            _         => EOutputPane.Both
        }));
    }
}
