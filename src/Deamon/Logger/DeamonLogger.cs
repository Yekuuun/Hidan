namespace Deamon.Logger;

internal static class DeamonLogger
{
    /// <summary>
    /// Global Deamon console logging handler.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="msg"></param>
    public static void WriteLog(ELogError level, string msg)
    {
#if DEBUG 
        var (prefix, color) = level switch
        {
            ELogError.OK      => ("[*]", ConsoleColor.Green),
            ELogError.WARNING => ("[!]", ConsoleColor.Yellow),
            ELogError.ERROR   => ("[X]", ConsoleColor.Red),
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