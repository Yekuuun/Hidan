using System.Collections.Concurrent;
using Deamon.Ebpf.Abstraction;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
using System.Collections.ObjectModel;

namespace Deamon.Gui;

/// <summary>
/// Terminal.Gui v2 front-end : a scrollable events pane on top, a command
/// input at the bottom. Owns the UI thread ; the event stream is drained on a
/// background task & marshalled in via Application.Invoke.
/// </summary>
internal sealed class TerminalGui(IEbpfEventReader reader)
{
    private const int MaxLines = 2000;

    private readonly IEbpfEventReader _reader = reader;

    //buffered between UI ticks so we do one Invoke per tick, not per event.
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly ObservableCollection<string> _lines = [];

    private ListView _logView = null!;
    private TextField _input = null!;

    //the running instance : only valid for the lifetime of Run.
    private IApplication? _app;

    //raised when the user asks to quit ; lets Main stop the host cleanly.
    public event Action? QuitRequested;

    /// <summary>
    /// Blocks on the Terminal.Gui main loop until the user quits. Must run on
    /// the thread that owns the console.
    /// </summary>
    public void Run(CancellationToken stoppingToken)
    {
        //instance-based model : the static Application gateway is legacy.
        using IApplication app = Application.Create().Init();
        _app = app;

        try
        {
            //a runnable handed to Run is ours to dispose.
            using var win = BuildUi();

            //drain the reader off the UI thread.
            _ = Task.Run(() => ConsumeAsync(stoppingToken), stoppingToken);

            //flush buffered lines into the view on a UI-thread timer : one
            //refresh every 100ms regardless of event rate.
            app.AddTimeout(TimeSpan.FromMilliseconds(100), FlushPending);

            //external cancellation (host shutting down) closes the UI. The
            //registration is disposed before the app so the callback cannot
            //land on a disposed instance.
            using var cancellation = stoppingToken.Register(() => app.Invoke(a => a.RequestStop()));

            app.Run(win);
        }
        finally
        {
            _app = null;
        }
    }

    private Window BuildUi()
    {
        var win = new Window
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Title = "Hidan — events (type 'quit' to exit)"
        };

        var logFrame = new FrameView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()! - 3,
            Title = "events"
        };

        _logView = new ListView
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };
        _logView.SetSource(_lines);
        logFrame.Add(_logView);

        var promptFrame = new FrameView
        {
            X = 0,
            Y = Pos.Bottom(logFrame),
            Width = Dim.Fill(),
            Height = 3,
            Title = "command"
        };

        _input = new TextField
        {
            X = 0, Y = 0,
            Width = Dim.Fill()
        };
        _input.KeyDown += OnInputKey;
        promptFrame.Add(_input);

        win.Add(logFrame, promptFrame);

        _input.SetFocus();

        return win;
    }

    #region INPUT

    private void OnInputKey(object? sender, Key e)
    {
        if(e != Key.Enter)
            return;

        string line = _input.Text?.ToString() ?? string.Empty;
        _input.Text = string.Empty;
        e.Handled = true;

        HandleCommand(line.Trim());
    }

    /// <summary>
    /// TO DO : BUILD FULL COMMAND LINE HANDLER.
    /// </summary>
    /// <param name="command"></param>
    private void HandleCommand(string command)
    {
        switch(command.ToLowerInvariant())
        {
            case "quit":
            case "exit":
                QuitRequested?.Invoke();
                _app?.RequestStop();
                break;

            case "clear":
                _lines.Clear();
                break;

            case "":
                break;

            default:
                Write($"unknown command : {command}");
                break;
        }
    }

    #endregion

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
}