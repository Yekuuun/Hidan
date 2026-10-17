using System.CommandLine;
using System.Text;
using Deamon.Cmd.Abstraction;
using Deamon.Ebpf.Abstraction;
using Deamon.Gui.Abstraction;
using Deamon.Utils;

namespace Deamon.Cmd.Commands;

internal sealed class MapCommand(IEbpfMapActions actions) : CliCommandBase
{
    public override string Name => "map";
    public override string Description => "Inspect & edit loaded eBPF maps.";

    /// <summary>
    /// Main method to config all subcommands modules.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="output"></param>
    protected override void Configure(Command command, IOutputCommand output)
    {
        command.Subcommands.Add(BuildList(output));
        command.Subcommands.Add(BuildShow(output));
        command.Subcommands.Add(BuildDump(output));
        command.Subcommands.Add(BuildMapUpdate(output));
        command.Subcommands.Add(BuildMapRemove(output));
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

    /// <summary>
    /// Update command utility to add / update values in maps
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildMapUpdate(IOutputCommand output)
    {
        var nameArg = new Argument<string>("name") { Description = "map name, as printed by 'map list'" };
        var keyArg  = new Argument<string>("key")  { Description = "key value" };
        var valArg  = new Argument<string>("value") { Description = "value to add / update" };

        // Args must be registered on the command, or GetValue fails.
        var update = new Command("update", "update / add element in a single map") { nameArg, keyArg, valArg };

        update.SetAction(pr =>
        {
            string mapName = pr.GetValue(nameArg) ?? string.Empty;
            string rawKey  = pr.GetValue(keyArg)  ?? string.Empty;
            string rawVal  = pr.GetValue(valArg)  ?? string.Empty;

            IEbpfMapConfig? mapConf = actions.GetMapConf(mapName);
            if (mapConf is null)
            {
                output.WriteOutput($"Unable to find map with name : {mapName}. Use 'map list' to see all available maps.");
                return;
            }

            EbpfMapTypeObj? types = mapConf.KeyValuesTypes;
            if (types is null)
            {
                output.WriteOutput("Unable to get map types configuration.");
                return;
            }

            // Drop '.Value' if EbpfMapTypeObj is a class and not a struct.
            if (!TypedValueParser.TryParse(rawKey, types.Value.Key, out object? key, out string? keyErr))
            {
                output.WriteOutput($"Invalid key: {keyErr}");
                return;
            }

            if (!TypedValueParser.TryParse(rawVal, types.Value.Val, out object? val, out string? valErr))
            {
                output.WriteOutput($"Invalid value: {valErr}");
                return;
            }

            if (!TryEncode(key!, out byte[]? keyBytes) || !TryEncode(val!, out byte[]? valBytes))
            {
                output.WriteOutput("Unable to encode key or value to bytes.");
                return;
            }

            (bool ok, string? err) = actions.TryUpdateMapValue(mapName, keyBytes!, valBytes!);
            output.WriteOutput(ok ? $"Map '{mapName}' updated." : $"Update failed: {err}");
        });

        return update;
    }

    /// <summary>
    /// Delete utility to delete a map element using it's key.
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    private Command BuildMapRemove(IOutputCommand output)
    {
        var nameArg = new Argument<string>("name") { Description = "map name, as printed by 'map list'" };
        var keyArg  = new Argument<string>("key")  { Description = "key of the element to delete" };

        var delete = new Command("delete", "delete an element in a map using its key") { nameArg, keyArg };

        delete.SetAction(pr =>
        {
            string mapName = pr.GetValue(nameArg) ?? string.Empty;
            string rawKey  = pr.GetValue(keyArg)  ?? string.Empty;

            IEbpfMapConfig? mapConf = actions.GetMapConf(mapName);
            if (mapConf is null)
            {
                output.WriteOutput($"Unable to find map with name : {mapName}. Use 'map list' to see all available maps.");
                return;
            }

            EbpfMapTypeObj? types = mapConf.KeyValuesTypes;
            if (types is null)
            {
                output.WriteOutput("Unable to get map types configuration.");
                return;
            }

            // Only the key type matters here, the value type is unused.
            if (!TypedValueParser.TryParse(rawKey, types.Value.Key, out object? key, out string? keyErr))
            {
                output.WriteOutput($"Invalid key: {keyErr}");
                return;
            }

            if (!TryEncode(key!, out byte[]? keyBytes))
            {
                output.WriteOutput("Unable to encode key to bytes.");
                return;
            }

            (bool ok, string? err) = actions.TryRemoveElement(mapName, keyBytes!);
            output.WriteOutput(ok ? $"Element removed from '{mapName}'." : $"Delete failed: {err}");
        });

        return delete;
    }

    private static bool TryEncode(object value, out byte[]? bytes)
    {
        bytes = value switch
        {
            byte b      => [b],
            sbyte sb    => [(byte)sb],
            bool flag   => [(byte)(flag ? 1 : 0)],
            short s     => BitConverter.GetBytes(s),
            ushort us   => BitConverter.GetBytes(us),
            int i       => BitConverter.GetBytes(i),
            uint ui     => BitConverter.GetBytes(ui),
            long l      => BitConverter.GetBytes(l),
            ulong ul    => BitConverter.GetBytes(ul),
            float f     => BitConverter.GetBytes(f),
            double d    => BitConverter.GetBytes(d),
            Guid g      => g.ToByteArray(),
            byte[] raw  => raw,
            string str  => Encoding.UTF8.GetBytes(str),
            _           => null,
        };

        return bytes is not null;
    }

    #endregion
}