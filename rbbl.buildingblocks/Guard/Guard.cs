using System.Runtime.CompilerServices;

namespace rbbl.buildingblocks.Guard;

public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotNull<T>(T? value, string paramName) where T : class
    {
        if (value is null) throw new ArgumentNullException(paramName);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
        return value!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("GUID cannot be empty.", paramName);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NotDefault<T>(T value, string paramName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
            throw new ArgumentException("Value cannot be the default for its type.", paramName);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T InRange<T>(T value, T min, T max, string paramName) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}.");
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NonNegative(int value, string paramName)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal NonNegative(decimal value, string paramName)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(paramName, "Value cannot be negative.");
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MaxLength(string value, int maxLength, string paramName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException($"Maximum length is {maxLength}.", paramName);
        return value!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void That(bool condition, string message, string paramName)
    {
        if (!condition) throw new ArgumentException(message, paramName);
    }
}
