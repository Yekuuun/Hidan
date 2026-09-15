using System.Collections.ObjectModel;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class MapCommand(IEbpfMapActions actions) : ICmdCommand
{
    #region CONFIG
    public string Name => "map";

    public string Description => "Maps interaction commands.";

    public ReadOnlyCollection<string> Aliases => [];
    #endregion

    /// <summary>
    /// TO DO : 
    /// 
    /// UPGRADE CLASS to handle multi arguments type commands (--list, --add <map_id> <new_value>, etc.)
    /// </summary>
    /// <param name="args"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Execute(string[] args, IOutputCommand output)
    {
        List<MapDto> maps = actions.ListAllMaps();
        if(maps.Count == 0)
        {
            output.WriteOutput("No maps loaded. Inspect loaded program using bpftool.");
        }
        else
        {
            output.WriteOutput("Loaded maps informations : ");
            foreach(MapDto map in maps)
            {
                output.WriteOutput($"Name : {map.Name}, FD : {map.Fd}, Max entries : {map.MaxEntries}, Type : {map.MapType.ToString()}");
            }
        }
    }
}