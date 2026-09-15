using System.Collections.ObjectModel;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class ClearCommand : ICmdCommand
{
    public string Name => "clear";

    public string Description => "Clear terminal entries";

    public ReadOnlyCollection<string> Aliases => [];

    public void Execute(string[] args, IOutputCommand output) => output.Clear();
}