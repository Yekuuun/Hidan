namespace Deamon.Logger;

internal static class DeamonLogger
{
    //set while a front-end owns the console : writing to Console then would
    //corrupt its rendering, so lines go to the sink instead.
    private static volatile Action<ELogError, string>? _sink;

    /// <summary>
    /// Redirects the log stream to <paramref name="sink"/>, or back to the
    /// console when passed null.
    /// </summary>
    /// <param name="sink"></param>
    public static void SetSink(Action<ELogError, string>? sink) => _sink = sink;

    /// <summary>
    /// Global Deamon console logging handler.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public static void WriteLog(ELogError level, string msg)
    {
        var sink = _sink;
        if(sink is not null)
        {
            sink(level, msg);
            return;
        }

#if DEBUG 
        var (prefix, color) = level switch
        {
            ELogError.OK      => ("[*]", ConsoleColor.Green),
            ELogError.WARNING => ("[!]", ConsoleColor.Yellow),
            ELogError.ERROR   => ("[X]", ConsoleColor.Red),
            ELogError.EVENT   => ("[$]", ConsoleColor.Blue),
            _                 => ("[?]", ConsoleColor.White)
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"{prefix} Deamon : {msg}");
        Console.ResetColor();
#else
        return;
#endif
    }
}