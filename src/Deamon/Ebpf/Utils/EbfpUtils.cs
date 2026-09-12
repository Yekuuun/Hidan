namespace Deamon.Ebpf.Utils;

internal static class EbpfUtils
{
    /// <summary>
    /// Check if _bpfConfig progPath is a valid .o file.
    /// </summary>
    /// <returns></returns>
    public static bool IsValidProgFile(string progPath)
    {
        if(string.IsNullOrWhiteSpace(progPath))
            return false;

        if(!File.Exists(progPath))
            return false;

        string ext = Path.GetExtension(progPath);
        if(!string.Equals(ext, ".o", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}