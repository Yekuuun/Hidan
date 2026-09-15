using System.Collections.ObjectModel;
using System.CommandLine;
using System.CommandLine.Help;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Abstraction;

/// <summary>
/// Base for commands whose arguments are worth parsing rather than reading by
/// hand. Owns the System.CommandLine plumbing : the derived class only
/// describes its verbs & options in <see cref="Configure"/>.
///
/// The tree is rebuilt on every execution. That is deliberate : the output sink
/// is a per-call parameter, so building per call lets the actions close over it
/// instead of the command holding mutable state shared between invocations.
/// Commands are singletons driven by a human pressing enter, the allocation is
/// not worth a threading hazard.
/// </summary>
internal abstract class CliCommandBase : ICmdCommand
{
    public abstract string Name {get; }
    public abstract string Description {get; }

    public virtual ReadOnlyCollection<string> Aliases => ReadOnlyCollection<string>.Empty;

    /// <summary>
    /// Describe the verbs, arguments & options of the command. The root is
    /// already named & described from <see cref="Name"/> and
    /// <see cref="Description"/> : naming it here would let the help text drift
    /// from the key the registry dispatches on.
    /// </summary>
    protected abstract void Configure(Command command, IOutputCommand output);

    public void Execute(string[] args, IOutputCommand output)
    {
        var command = new Command(Name, Description);
        Configure(command, output);

        //only a RootCommand gets one for free, and we are never a root : without
        //this, --help parses as an unrecognized argument.
        EnsureHelp(command);

        //disposed after Invoke : System.CommandLine does not always end its last
        //write on a newline, and the flush is what emits that line.
        using var sink = new OutputCommandWriter(output);

        command.Parse(args).Invoke(new InvocationConfiguration
        {
            Output = sink,
            Error  = sink,
        });
    }

    private static void EnsureHelp(Command command)
    {
        if(!command.Options.Any(o => o is HelpOption))
            command.Options.Add(new HelpOption());

        foreach(Command sub in command.Subcommands)
            EnsureHelp(sub);
    }
}
