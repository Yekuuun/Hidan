using System.Collections.ObjectModel;
using System.CommandLine;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class MapCommand(IEbpfMapActions actions) : CliCommandBase
{
    public override string Name => "map";

    public override string Description => "Inspect & edit loaded eBPF maps.";

    protected override void Configure(Command command, IOutputCommand output)
    {
        command.Subcommands.Add(BuildList(output));
        command.Subcommands.Add(BuildShow(output));
        command.Subcommands.Add(BuildDump(output));
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

        list.SetAction(_ => 
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
        });

        return list;
    }

    /// <summary>
    /// Build show command utility
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildShow(IOutputCommand output)
    {
        var nameArg = new Argument<string>("name") { Description = "map name, as printed by 'map list'.", };
        var show = new Command("show", "Show a single map configuration.") { nameArg };

        show.SetAction(pr =>
        {
            string name = pr.GetValue<string>(nameArg) ?? string.Empty;
            MapDto? map = actions.TryGetMap(name);

            if(!map.HasValue)
            {
                output.WriteOutput($"Map with name : {name} not found");
            }
            else
            {
                output.WriteOutput($"Name : {map.Value.Name}, FD : {map.Value.Fd}, Max entries : {map.Value.MaxEntries}, Type : {map.Value.MapType.ToString()}");
            }
        });

        return show;
    }

    /// <summary>
    /// TO DO : 
    /// 
    /// Implement core dump utility.
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private static Command BuildDump(IOutputCommand output)
    {
        var dump = new Command("dump", "Dump map datas.");
        dump.SetAction(_ =>
        {
            output.WriteOutput("Use bpftool base utility to dump map informations. Ex : sudo bpftool map dump id <map_id>");
        });

        return dump;
    }

    #endregion
}