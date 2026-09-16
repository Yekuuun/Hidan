namespace Deamon.Gui.Config;

/// <summary>
/// Which half of the front-end a command is talking about.
/// </summary>
internal enum EOutputPane
{
    /// <summary>Left pane : command output.</summary>
    Command,

    /// <summary>Right pane : the ring buffer stream.</summary>
    Events,

    Both
}
