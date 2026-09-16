using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Deamon.Gui;

/// <summary>
/// One scrollable pane : its frame, the list view inside it & the buffer
/// feeding them.
///
/// Producers push from any thread through <see cref="Enqueue"/> — that is the
/// only member that touches the concurrent queue. Everything else runs on the
/// UI thread, driven by the front-end timer.
/// </summary>
internal sealed class TerminalLogPane
{
    //buffered between UI ticks so we do one refresh per tick, not per line.
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly ObservableCollection<string> _lines = [];
    private readonly ListView _view;
    private readonly int _maxLines;

    public TerminalLogPane(string title, int maxLines, Pos x, Pos y, Dim width, Dim height)
    {
        _maxLines = maxLines;

        Frame = new FrameView
        {
            X = x, Y = y,
            Width = width,
            Height = height,
            Title = title,
        };

        _view = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _view.SetSource(_lines);
        Frame.Add(_view);
    }

    /// <summary>
    /// The view to hand to the parent window : the pane owns its layout.
    /// </summary>
    public FrameView Frame {get;}

    /// <summary>
    /// Queues a line for the next flush. Safe to call from any thread.
    /// </summary>
    public void Enqueue(string line) => _pending.Enqueue(line);

    /// <summary>
    /// Moves whatever is queued into the view & scrolls to the tail. UI thread
    /// only.
    /// </summary>
    public void Flush()
    {
        bool changed = false;

        while(_pending.TryDequeue(out var line))
        {
            _lines.Add(line);
            changed = true;
        }

        if(!changed)
            return;

        //ObservableCollection has no RemoveRange : trim the head one by one.
        while(_lines.Count > _maxLines)
            _lines.RemoveAt(0);

        _view.SelectedItem = _lines.Count - 1;
    }

    /// <summary>
    /// Drops the queued lines too : a clear the user asked for should not be
    /// undone by the next tick. UI thread only.
    /// </summary>
    public void Clear()
    {
        while(_pending.TryDequeue(out _)){}
        _lines.Clear();
    }
}
