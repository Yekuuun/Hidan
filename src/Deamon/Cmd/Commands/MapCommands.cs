using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd.Commands;

internal sealed class MapCommand(IEbpfMapActions actions, IOutputCommand cmdOutput) : ICmdCommand
{
    #region CONFIG
    public string Name => "map";

    public string Description => "Maps interaction commands.";
    #endregion

    private readonly IOutputCommand  _cmdOutput = cmdOutput;
    private readonly IEbpfMapActions _actions   = actions;

    /// <summary>
    /// TO DO : 
    /// 
    /// UPGRADE CLASS to handle multi arguments type commands (--list, --add <map_id> <new_value>, etc.)
    /// </summary>
    /// <param name="args"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Execute(string[] args)
    {
        List<MapDto> maps = _actions.ListAllMaps();
        if(maps.Count == 0)
        {
            _cmdOutput.WriteOutput("No maps loaded. Inspect loaded program using bpftool.");
        }
        else
        {
            _cmdOutput.WriteOutput("Loaded maps informations : ");
            foreach(MapDto map in maps)
            {
                _cmdOutput.WriteOutput($"Name : {map.Name}, FD : {map.Fd}, Max entries : {map.MaxEntries}, Type : {map.MapType.ToString()}");
            }
        }
    }
}