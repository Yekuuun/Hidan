using Deamon.Ebpf.Abstraction;
using Deamon.Logger;
using Microsoft.Extensions.Hosting;

namespace Deamon.Gui;

/// <summary>
/// Hosts <see cref="TerminalGui"/> for the lifetime of the app. Terminal.Gui
/// needs a thread of its own : a pooled thread blocked for hours is not one,
/// so the UI gets a dedicated thread & ExecuteAsync just waits on it.
/// </summary>
internal sealed class TerminalGuiService(IEbpfEventReader reader, IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var uiExited = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var uiThread = new Thread(() => RunUi(stoppingToken, uiExited))
        {
            Name = "hidan-ui",
            IsBackground = true
        };

        uiThread.Start();

        return uiExited.Task;
    }

    private void RunUi(CancellationToken stoppingToken, TaskCompletionSource uiExited)
    {
        var gui = new TerminalGui(reader);
        gui.QuitRequested += lifetime.StopApplication;
        DeamonLogger.SetSink((level, msg) => gui.Write($"[{level}] {msg}"));

        try
        {
            gui.Run(stoppingToken);
        }
        catch(Exception ex)
        {
            DeamonLogger.SetSink(null);
            DeamonLogger.WriteLog(ELogError.ERROR, $"Terminal UI stopped on error : {ex.Message}");
        }
        finally
        {
            DeamonLogger.SetSink(null);

            lifetime.StopApplication();
            uiExited.TrySetResult();
        }
    }
}
