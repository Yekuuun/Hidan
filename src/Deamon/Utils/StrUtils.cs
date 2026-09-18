using System.Text;

namespace Deamon.Utils;

internal static class StrUtils
{
    public static byte[] ToFixedKeySize(string s, int size)
    {
        byte[] buffer = new byte[size];
        var written   = Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, 0);
        if(written >= size)
            throw new ArgumentException($"'{s}' is too long for a {size}-byte key (max {size - 1} chars + null terminator).");

        return buffer;
    }
}