using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace Deamon.Gui;

/// <summary>
/// Handle all events for Terminal Gui interface.
/// </summary>
internal partial class TerminalGui
{
        //buffered between UI ticks so we do one Invoke per tick, not per event.
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly ObservableCollection<string> _lines = [];

    #region EVENTS

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach(var evt in _reader.ReadAsync(stoppingToken))
            {
                Write($"{evt}");
            }
        }
        catch(OperationCanceledException)
        {
            //normal shutdown.
        }
        catch(Exception ex)
        {
            Write($"reader stopped : {ex.Message}");
        }
    }

    /// <summary>
    /// Pushes a line into the events pane. Safe to call from any thread.
    /// </summary>
    /// <param name="line"></param>
    public void Write(string line) => Enqueue($"{DateTime.Now:HH:mm:ss}  {line}");

    //called from the background task : only touches the concurrent queue.
    private void Enqueue(string line) => _pending.Enqueue(line);

    //runs on the UI thread via the timer : safe to touch the views here.
    private bool FlushPending()
    {
        bool changed = false;

        while(_pending.TryDequeue(out var line))
        {
            _lines.Add(line);
            changed = true;
        }

        if(!changed)
            return true;

        //ObservableCollection has no RemoveRange : trim the head one by one.
        while(_lines.Count > MaxLines)
            _lines.RemoveAt(0);

        _logView.SelectedItem = _lines.Count - 1;

        return true;
    }

    #endregion

    #region CMD

    public void RequestQuit()
    {
        QuitRequested?.Invoke();
        _app?.RequestStop();
    }

    public void WriteOutput(string line) => Write(line);

    public void Clear() => _lines.Clear();

    private void HandleCommand(string command) => _cmdRegister.TryExecute(command, this);

    #endregion
}