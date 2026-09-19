using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class ProgCommand(IEbpfProgramActions actions) : CliCommandBase
{
    public override string Name => "prog";

    public override string Description => "Loaded programs base actions.";

    public override ReadOnlyCollection<string> Aliases => ["program"];

    protected override void Configure(Command command, IOutputCommand output)
    {
        command.Subcommands.Add(BuildList(output));
        command.Subcommands.Add(BuildShow(output));
    }

    #region SUB_COMMANDS

    /// <summary>
    /// Build prog list command utility
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildList(IOutputCommand output)
    {
        var list = new Command("list", "List every loaded programs.");

        list.SetAction(_ =>
        {
            List<ProgramDto> progs = actions.ListPrograms();
            if (progs.Count == 0)
            {
                output.WriteOutput("No programs loaded. Inspect loaded program using bpftool.");
            }
            else
            {
                output.WriteOutput("Loaded programs informations : ");
                foreach (ProgramDto map in progs)
                {
                    output.WriteOutput($"Name : {map.Name}, FD : {map.Fd}, Type : {map.ProgramType.ToString()}");
                }
            }
        });

        return list;
    }

    /// <summary>
    /// Build show command
    /// </summary>
    /// <param name="name"></param>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildShow(IOutputCommand output)
    {
        var nameArg = new Argument<string>("name") { Description = "program name, as printed by 'prog list'.", };
        var show = new Command("show", "Dump one program information.") { nameArg };

        show.SetAction(pr =>
        {
            string name = pr.GetValue<string>(nameArg) ?? string.Empty;
            ProgramDto? prog = actions.FindProgram(name);

            if(!prog.HasValue)
            {
                output.WriteOutput($"Map with name : {name} not found");
            }
            else
            {
                output.WriteOutput($"Name : {prog.Value.Name}, FD : {prog.Value.Fd},  Type : {prog.Value.ProgramType.ToString()}");
            }
        });

        return show;
    }

    #endregion
}