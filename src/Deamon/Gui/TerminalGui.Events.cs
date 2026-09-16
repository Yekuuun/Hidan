using Deamon.Gui.Abstraction;
using Deamon.Gui.Config;
using Terminal.Gui.Input;

namespace Deamon.Gui;

/// <summary>
/// Handle all events for Terminal Gui interface.
/// </summary>
internal partial class TerminalGui
{
    #region EVENTS

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach(var evt in _reader.ReadAsync(stoppingToken))
            {
                WriteEvent($"{evt}");
            }
        }
        catch(OperationCanceledException)
        {
            //normal shutdown.
        }
        catch(Exception ex)
        {
            WriteEvent($"reader stopped : {ex.Message}");
        }
    }

    /// <summary>
    /// Pushes a line into the ring buffer pane. Safe to call from any thread.
    /// </summary>
    private void WriteEvent(string line) => _events.Enqueue(Stamp(line));

    /// <summary>
    /// Pushes a line into the console pane : app level messages, the logger
    /// sink included. Safe to call from any thread.
    /// </summary>
    public void Write(string line) => _console.Enqueue(Stamp(line));

    private static string Stamp(string line) => $"{DateTime.Now:HH:mm:ss}  {line}";

    //runs on the UI thread via the timer : safe to touch the views here.
    private bool FlushPending()
    {
        _console.Flush();
        _events.Flush();

        return true;
    }

    #endregion

    #region CMD

    //command output is already the answer to something the user typed : no
    //timestamp, it would only push the text right.
    public void WriteOutput(string line) => _console.Enqueue(line);

    public void Clear(EOutputPane pane)
    {
        if(pane is EOutputPane.Command or EOutputPane.Both)
            _console.Clear();

        if(pane is EOutputPane.Events or EOutputPane.Both)
            _events.Clear();
    }

    private void HandleCommand(string command) => _registry.TryExecute(command, this);

    private void OnInputKey(object? sender, Key e)
    {
        if(e != Key.Enter)
            return;

        string line = _input.Text?.ToString() ?? string.Empty;
        _input.Text = string.Empty;
        e.Handled = true;

        HandleCommand(line.Trim());
    }

    #endregion
}
