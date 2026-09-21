using System.Text;
using System.Windows.Input;
using Deamon.Cmd.Abstraction;
using Deamon.Gui.Abstraction;
using Terminal.Gui.Input;

namespace Deamon.Cmd;

internal sealed class CommandRegistry
{
    private readonly Dictionary<string, ICmdCommand> _commands = [];

    public CommandRegistry(IEnumerable<ICmdCommand> commands)
    {
        foreach(var c in commands)
        {
            if(string.IsNullOrWhiteSpace(c.Name ?? string.Empty))
                continue;

            string key = c.Name!.ToLower();
            _commands[key] = c;

            if(c.Aliases.Count != 0)
                foreach(string alias in c.Aliases)
                {
                    if(string.IsNullOrWhiteSpace(alias))
                        break;

                    _commands[alias] = c;
                }
        }
    }

    public void TryExecute(string rawInput, IOutputCommand output)
    {
        var tokens = Tokenize(rawInput);
        if(tokens.Length == 0)
            return;

        string cmd    = tokens[0];
        string[] args = tokens[1..];

        if(!_commands.TryGetValue(cmd, out var command))
        {
            output.WriteOutput($"{cmd} command not found");
            return;
        }

        try
        {
            command.Execute(args, output);
        }
        catch(Exception ex)
        {
            output.WriteOutput($"Error executing command {cmd} with exception : {ex.Message}");
        }
    }

    /// <summary>
    /// tokenize raw user input from HandleCommandLine in TerminalGui
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string[] Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
 
        foreach (var c in input)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0) { tokens.Add(current.ToString()); current.Clear(); }
                continue;
            }

            current.Append(c);
        }
        if (current.Length > 0) 
            tokens.Add(current.ToString());
            
        return [.. tokens];
    }
}
