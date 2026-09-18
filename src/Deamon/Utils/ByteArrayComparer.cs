namespace Deamon.Utils;

public sealed class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public static readonly ByteArrayComparer Instance = new();

    public bool Equals(byte[]? x, byte[]? y) => x is null || y is null ? ReferenceEquals(x, y) : x.AsSpan().SequenceEqual(y);

    public int GetHashCode(byte[] obj)
    {
        var hash = new HashCode();
        hash.AddBytes(obj);
        return hash.ToHashCode();
    }
}