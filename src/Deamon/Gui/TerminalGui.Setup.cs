using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Deamon.Gui;

internal partial class TerminalGui
{
    private const int MaxLines = 2000;

    //left : whatever a command answered. right : the ring buffer stream.
    private TerminalLogPane _console = null!;
    private TerminalLogPane _events = null!;
    private TextField _input = null!;

    private Window BuildTerminalGui()
    {
        var win = new Window
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Title = "Hidan (type 'quit' to exit)"
        };

        //both panes stop 3 rows short of the bottom : that is the prompt.
        _console = new TerminalLogPane(
            title: "console",
            maxLines: MaxLines,
            x: 0, y: 0,
            width: Dim.Percent(65),
            height: Dim.Fill()! - 3
        );

        _events = new TerminalLogPane(
            title: "events",
            maxLines: MaxLines,
            x: Pos.Right(_console.Frame), y: 0,
            width: Dim.Fill(),
            height: Dim.Fill()! - 3
        );
        
        //-----------------------------------------------------------------

        var promptFrame = new FrameView
        {
            X = 0,
            Y = Pos.Bottom(_console.Frame),
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

        win.Add(_console.Frame, _events.Frame, promptFrame);

        _input.SetFocus();

        return win;
    }
}
