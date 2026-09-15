using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class MapCommand(IEbpfMapActions actions) : CliCommandBase
{
    public override string Name => "maps";

    public override string Description => "Inspect & edit loaded eBPF maps.";

    protected override void Configure(Command command, IOutputCommand output)
    {
        command.Subcommands.Add(BuildList(output));
    }

    #region SUB_COMMANDS

    /// <summary>
    /// Build map list command base utility
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildList(IOutputCommand output)
    {
        var list = new Command("list", "List every loaded map.");

        list.SetAction(_ => ListAllMaps(output));

        return list;
    }

    #endregion

    private void ListAllMaps(IOutputCommand output)
    {
        List<MapDto> maps = actions.ListAllMaps();
        if (maps.Count == 0)
        {
            output.WriteOutput("No maps loaded. Inspect loaded program using bpftool.");
        }
        else
        {
            output.WriteOutput("Loaded maps informations : ");
            foreach (MapDto map in maps)
            {
                output.WriteOutput($"Name : {map.Name}, FD : {map.Fd}, Max entries : {map.MaxEntries}, Type : {map.MapType.ToString()}");
            }
        }
    }
}