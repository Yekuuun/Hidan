using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace Deamon.Gui;

internal partial class TerminalGui
{
    private const int MaxLines = 2000;
    private ListView _logView = null!;
    private TextField _input = null!;

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

    private void OnInputKey(object? sender, Key e)
    {
        if(e != Key.Enter)
            return;

        string line = _input.Text?.ToString() ?? string.Empty;
        _input.Text = string.Empty;
        e.Handled = true;

        HandleCommand(line.Trim());
    }
}