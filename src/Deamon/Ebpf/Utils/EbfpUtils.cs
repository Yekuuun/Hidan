using System.Runtime.InteropServices;

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

    /// <summary>
    /// Converts any blittable value to its raw map bytes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToBytes<T>(T value) where T : unmanaged => MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1)).ToArray();
}