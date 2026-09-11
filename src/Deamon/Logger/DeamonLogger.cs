namespace Deamon.Logger;

internal static class DeamonLogger
{
    public static void WriteLog(ELogError level, string msg)
    {
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
    }
}