using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Events;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

/// <summary>
/// QUID ? creating a single CONF command later ?
/// </summary>
internal sealed class DebugCommand(EbpfDebugConfig debugConfig) : CliCommandBase
{
    public override string Name => "debug";

    public override string Description => "Handle debug config for the ebpf ringbuffer";

    protected override void Configure(Command command, IOutputCommand output)
    {
        var statusArg = new Argument<string>("status")
        {
            Description         = "on (enable debug events), off (disable) or status (print current state).",
            Arity               = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = _ => "status"
        };

        statusArg.AcceptOnlyFromAmong("on", "off", "status");
        command.Arguments.Add(statusArg);

        command.SetAction(pr =>
        {
            switch(pr.GetValue<string>(statusArg))
            {
                case "on":
                    debugConfig.SetDebugStatus(true);
                    output.WriteOutput("debug events enabled");
                    break;

                case "off":
                    debugConfig.SetDebugStatus(false);
                    output.WriteOutput("debug events disabled");
                    break;

                default:
                    output.WriteOutput($"debug events are {(debugConfig.GetStatus() ? "enabled" : "disabled")}");
                    break;
            }
        });
    }
}
