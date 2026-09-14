using System.Collections.Concurrent;
using Deamon.Ebpf.Abstraction;
using Terminal.Gui.App;
using System.Collections.ObjectModel;
using Deamon.Gui.Abstraction;
using Deamon.Cmd;
using Deamon.Cmd.Commands;

namespace Deamon.Gui;

/// <summary>
/// Terminal.Gui v2 front-end : a scrollable events pane on top, a command
/// input at the bottom. Owns the UI thread ; the event stream is drained on a
/// background task & marshalled in via Application.Invoke.
/// </summary>
internal partial class TerminalGui(IEbpfEventReader reader) : IAppLifeCycle, IOutputCommand
{
    private readonly CommandRegistry _cmdRegister = new();
    private readonly IEbpfEventReader _reader = reader;
    private IApplication? _app;

    //raised when the user asks to quit ; lets Main stop the host cleanly.
    public event Action? QuitRequested;

    /// <summary>
    /// Blocks on the Terminal.Gui main loop until the user quits. Must run on
    /// the thread that owns the console.
    /// </summary>
    public void Run(CancellationToken stoppingToken)
    {
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

            ConfigureCommandRegistry();
            app.Run(win);
        }
        finally
        {
            _app = null;
        }
    }

    /// <summary>
    /// Register all commands.
    /// </summary>
    private void ConfigureCommandRegistry()
    {
        _cmdRegister.RegisterCommand(new QuidCommand(this), "quit", "leave");
    }
}