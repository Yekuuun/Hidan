using Deamon.Gui.Config;

namespace Deamon.Gui.Abstraction;

internal interface IOutputCommand
{
    void WriteOutput(string line);
    void Clear(EOutputPane pane);
}
