using System.Text;
using Deamon.Gui.Abstraction;

namespace Deamon.Cmd;

/// <summary>
/// Bridges the TextWriter System.CommandLine hands its help & parse errors to,
/// and the line oriented IOutputCommand the front-end exposes : chars are
/// buffered until a newline, then handed over as a single line.
///
/// Not thread safe : one instance per command execution, living only for the
/// duration of the call & used on the caller's thread.
/// </summary>
internal sealed class OutputCommandWriter(IOutputCommand output) : TextWriter
{
    private readonly StringBuilder _current = new();

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        if(value == '\n')
            EndLine();
        else
            _current.Append(value);
    }

    //help arrives as one large multi-line write : the base class would walk it
    //a char at a time through the overload above, this cuts it on newlines.
    public override void Write(ReadOnlySpan<char> buffer)
    {
        while(true)
        {
            int newline = buffer.IndexOf('\n');
            if(newline < 0)
            {
                _current.Append(buffer);
                return;
            }

            _current.Append(buffer[..newline]);
            EndLine();

            buffer = buffer[(newline + 1)..];
        }
    }

    public override void Write(string? value)
    {
        if(!string.IsNullOrEmpty(value))
            Write(value.AsSpan());
    }

    public override void Write(char[] buffer, int index, int count) => Write(buffer.AsSpan(index, count));

    /// <summary>
    /// Emits whatever is still buffered : the last write of a command is not
    /// guaranteed to end on a newline.
    /// </summary>
    public override void Flush()
    {
        if(_current.Length > 0)
            EndLine();
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
            Flush();

        base.Dispose(disposing);
    }

    private void EndLine()
    {
        //drop the \r of a CRLF ending : a lone \r is not ours to interpret.
        if(_current.Length > 0 && _current[^1] == '\r')
            _current.Length--;

        output.WriteOutput(_current.ToString());
        _current.Clear();
    }
}
