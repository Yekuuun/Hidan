using System.Globalization;
using System.Net;
using System.Numerics;

namespace Deamon.Utils;

/// <summary>
/// Parses a raw string into a value of a runtime-known <see cref="Type"/>.
/// Integers: decimal, or hex with "0x". Bool: true/false/1/0.
/// byte[]: hex string. Enum: name or defined numeric value.
/// 
/// written using @Claude.
/// </summary>
public static class TypedValueParser
{
    private delegate bool RawParser(string raw, out object? result);

    private static readonly Dictionary<Type, RawParser> Parsers = new()
    {
        [typeof(byte)]      = Integer<byte>,
        [typeof(sbyte)]     = Integer<sbyte>,
        [typeof(short)]     = Integer<short>,
        [typeof(ushort)]    = Integer<ushort>,
        [typeof(int)]       = Integer<int>,
        [typeof(uint)]      = Integer<uint>,
        [typeof(long)]      = Integer<long>,
        [typeof(ulong)]     = Integer<ulong>,
        [typeof(float)]     = Floating<float>,
        [typeof(double)]    = Floating<double>,
        [typeof(bool)]      = Boolean,
        [typeof(string)]    = Text,
        [typeof(Guid)]      = GuidValue,
        [typeof(byte[])]    = HexBytes,
    };

    public static bool CanParse(Type type) => Parsers.ContainsKey(type) || type.IsEnum;

    /// <summary>Never throws on bad input. 'error' says what went wrong.</summary>
    public static bool TryParse(string raw, Type type, out object? result, out string? error)
    {
        ArgumentNullException.ThrowIfNull(raw);
        ArgumentNullException.ThrowIfNull(type);

        result = null;
        error = null;

        if (type.IsEnum)
        {
            if (EnumValue(raw, type, out result)) return true;
            error = $"'{raw}' is not a valid {type.Name}. Expected: {string.Join(", ", Enum.GetNames(type))}.";
            return false;
        }

        if (!Parsers.TryGetValue(type, out RawParser? parser))
        {
            error = $"Unsupported type: {type.Name}.";
            return false;
        }

        if (parser(raw, out result)) return true;

        error = $"'{raw}' is not a valid {type.Name}.";
        return false;
    }

    /// <summary>Throws FormatException or NotSupportedException on failure.</summary>
    public static object Parse(string raw, Type type)
    {
        if (!CanParse(type))
            throw new NotSupportedException($"Unsupported type: {type.Name}.");

        if (TryParse(raw, type, out object? result, out string? error))
            return result!;

        throw new FormatException(error);
    }

    #region PARSERS

    private static bool Integer<T>(string raw, out object? result) where T : IBinaryInteger<T>
    {
        string s = raw.Trim();
        bool ok;
        T value;

        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            // Signed types read hex as raw bits: "0xFF" is -1 for sbyte.
            ok = T.TryParse(s[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value!);
        }
        else
        {
            ok = T.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value!);
        }

        result = ok ? value : null;
        return ok;
    }

    private static bool Floating<T>(string raw, out object? result) where T : INumberBase<T>
    {
        bool ok = T.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out T? value);
        result = ok ? value : null;
        return ok;
    }

    private static bool Boolean(string raw, out object? result)
    {
        string s = raw.Trim();
        switch (s)
        {
            case "1": result = true;  return true;
            case "0": result = false; return true;
        }

        bool ok = bool.TryParse(s, out bool value);
        result = ok ? value : null;
        return ok;
    }

    private static bool Text(string raw, out object? result)
    {
        result = raw;
        return true;
    }

    private static bool GuidValue(string raw, out object? result)
    {
        bool ok = Guid.TryParse(raw.Trim(), out Guid value);
        result = ok ? value : null;
        return ok;
    }

    private static bool HexBytes(string raw, out object? result)
    {
        string s = raw.Trim();
        if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) s = s[2..];

        try
        {
            // Needs an even number of hex digits.
            result = Convert.FromHexString(s);
            return true;
        }
        catch (FormatException)
        {
            result = null;
            return false;
        }
    }

    private static bool EnumValue(string raw, Type enumType, out object? result)
    {
        result = null;
        string s = raw.Trim();

        if (!Enum.TryParse(enumType, s, ignoreCase: true, out object? value)) return false;

        // Enum.TryParse accepts any number, even undefined ones. Reject those, except for [Flags].
        bool isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
        if (!isFlags && !Enum.IsDefined(enumType, value!)) return false;

        result = value;
        return true;
    }

    #endregion
}