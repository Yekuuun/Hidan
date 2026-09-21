using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Deamon.Cmd.Commands;

internal sealed class HelpCommand(IServiceProvider services) : CliCommandBase
{
    public override string Name => "help";

    public override string Description => "List every available command.";

    public override ReadOnlyCollection<string> Aliases => ["?"];

    protected override void Configure(Command command, IOutputCommand output)
    {
        command.SetAction(_ =>
        {
            var commands = services.GetServices<ICmdCommand>()
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .DistinctBy(c => c.Name)
                .OrderBy(c => c.Name);

            output.WriteOutput("Available commands :");

            foreach(var cmd in commands)
            {
                string aliases = cmd.Aliases.Count != 0 ? $" (aliases : {string.Join(", ", cmd.Aliases)})" : string.Empty;
                output.WriteOutput($"  {cmd.Name,-10} {cmd.Description}{aliases}");
            }
        });
    }
}
