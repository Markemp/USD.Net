namespace Pxr.Usd.Tf;

/// <summary>
/// A type-safe enum wrapper that can hold any enum type while preserving type information.
/// Similar to Pixar's TfEnum but leveraging C#'s better enum support.
/// </summary>
public struct TfEnum : IEquatable<TfEnum>, IComparable<TfEnum>
{
    private readonly Type _enumType;
    private readonly object _value;

    public TfEnum(Enum value)
    {
        _enumType = value.GetType();
        _value = value;
    }

    // For creating from registered values
    internal TfEnum(Type enumType, object value)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));

        _enumType = enumType;
        _value = value;
    }

    public Type EnumType => _enumType;
    public object Value => _value;

    /// <summary>
    /// Check if this enum is of type T
    /// </summary>
    public bool IsA<T>() where T : Enum => _enumType == typeof(T);

    /// <summary>
    /// Get the strongly-typed enum value
    /// </summary>
    public T GetValue<T>() where T : Enum
    {
        if (!IsA<T>())
            throw new InvalidOperationException($"Cannot convert {_enumType.Name} to {typeof(T).Name}");

        return (T)_value;
    }

    /// <summary>
    /// Get the underlying integer value
    /// </summary>
    public int GetIntValue() => Convert.ToInt32(_value);

    #region Equality and Comparison

    public bool Equals(TfEnum other)
    {
        return _enumType == other._enumType &&
               Equals(_value, other._value);
    }

    public override bool Equals(object? obj) => obj is TfEnum other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_enumType, _value);

    public int CompareTo(TfEnum other)
    {
        // First compare by type name for consistency
        var typeComparison = string.Compare(_enumType?.FullName, other._enumType?.FullName, StringComparison.Ordinal);
        if (typeComparison != 0)
            return typeComparison;

        // Then by value
        return GetIntValue().CompareTo(other.GetIntValue());
    }

    public static bool operator ==(TfEnum left, TfEnum right) => left.Equals(right);
    public static bool operator !=(TfEnum left, TfEnum right) => !left.Equals(right);
    public static bool operator <(TfEnum left, TfEnum right) => left.CompareTo(right) < 0;
    public static bool operator >(TfEnum left, TfEnum right) => left.CompareTo(right) > 0;
    public static bool operator <=(TfEnum left, TfEnum right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TfEnum left, TfEnum right) => left.CompareTo(right) >= 0;

    #endregion

    #region String Conversion and Registration

    private static readonly Dictionary<TfEnum, EnumInfo> _enumRegistry = new();
    private static readonly Dictionary<Type, Dictionary<string, object>> _nameToValueCache = new();

    private record EnumInfo(string Name, string DisplayName, string FullName);

    /// <summary>
    /// Register an enum value with custom names
    /// </summary>
    public static void AddName(Enum enumValue, string? displayName = null)
    {
        var tfEnum = new TfEnum(enumValue);
        var name = enumValue.ToString();
        var fullName = $"{enumValue.GetType().Name}::{name}";

        _enumRegistry[tfEnum] = new EnumInfo(name, displayName ?? name, fullName);

        // Cache for reverse lookup
        if (!_nameToValueCache.ContainsKey(enumValue.GetType()))
            _nameToValueCache[enumValue.GetType()] = new Dictionary<string, object>();

        _nameToValueCache[enumValue.GetType()][name] = enumValue;
    }

    /// <summary>
    /// Get the registered name for this enum value
    /// </summary>
    public static string GetName(TfEnum enumValue)
    {
        return _enumRegistry.TryGetValue(enumValue, out var info) ? info.Name : string.Empty;
    }

    /// <summary>
    /// Get the full name (TypeName::ValueName) for this enum value
    /// </summary>
    public static string GetFullName(TfEnum enumValue)
    {
        return _enumRegistry.TryGetValue(enumValue, out var info) ? info.FullName : string.Empty;
    }

    /// <summary>
    /// Get the display name for this enum value
    /// </summary>
    public static string GetDisplayName(TfEnum enumValue)
    {
        return _enumRegistry.TryGetValue(enumValue, out var info) ? info.DisplayName : string.Empty;
    }

    /// <summary>
    /// Get all registered names for an enum type
    /// </summary>
    public static IEnumerable<string> GetAllNames<T>() where T : Enum
        => GetAllNames(typeof(T));

    /// <summary>
    /// Get all registered names for an enum type from a TfEnum instance
    /// </summary>
    public static IEnumerable<string> GetAllNames(TfEnum enumValue)
        => GetAllNames(enumValue.EnumType);

    /// <summary>
    /// Get all registered names for an enum type
    /// </summary>
    public static IEnumerable<string> GetAllNames(Type enumType)
    {
        return _enumRegistry
            .Where(kvp => kvp.Key._enumType == enumType)
            .Select(kvp => kvp.Value.Name);
    }

    /// <summary>
    /// Get enum value from name
    /// </summary>
    public static T GetValueFromName<T>(string name, out bool found) where T : Enum
    {
        if (_nameToValueCache.TryGetValue(typeof(T), out var cache) &&
            cache.TryGetValue(name, out var value))
        {
            found = true;
            return (T)value;
        }

        found = false;
        return (T)Enum.ToObject(typeof(T), -1); // Return -1 like the C++ version
    }

    /// <summary>
    /// Get enum value from name (overload without found parameter)
    /// </summary>
    public static T GetValueFromName<T>(string name) where T : Enum
    {
        return GetValueFromName<T>(name, out _);
    }

    /// <summary>
    /// Get enum value from name for a specific type
    /// </summary>
    public static TfEnum GetValueFromName(Type enumType, string name, out bool found)
    {
        if (_nameToValueCache.TryGetValue(enumType, out var cache) &&
            cache.TryGetValue(name, out var value))
        {
            found = true;
            return new TfEnum(enumType, value);
        }

        found = false;
        return new TfEnum(enumType, Enum.ToObject(enumType, -1));
    }

    /// <summary>
    /// Get enum value from full name (TypeName::ValueName)
    /// </summary>
    public static TfEnum GetValueFromFullName(string fullName, out bool found)
    {
        var registryEntry = _enumRegistry.FirstOrDefault(kvp => kvp.Value.FullName == fullName);
        if (registryEntry.Key._enumType != null)
        {
            found = true;
            return registryEntry.Key;
        }

        found = false;
        return new TfEnum(typeof(int), -1); // Return invalid enum like C++ version
    }

    /// <summary>
    /// Get enum value from full name (overload without found parameter)
    /// </summary>
    public static TfEnum GetValueFromFullName(string fullName)
    {
        return GetValueFromFullName(fullName, out _);
    }

    /// <summary>
    /// Check if an enum type name is known/registered
    /// </summary>
    public static bool IsKnownEnumType(string typeName)
    {
        return _enumRegistry.Keys.Any(key => key._enumType?.Name == typeName);
    }

    /// <summary>
    /// Get Type from enum type name
    /// </summary>
    public static Type? GetTypeFromName(string typeName)
    {
        var enumType = _enumRegistry.Keys
            .Select(key => key._enumType)
            .FirstOrDefault(type => type?.Name == typeName);

        return enumType;
    }

    #endregion

    // Implicit conversion from any enum
    public static implicit operator TfEnum(Enum enumValue) => new(enumValue);

    public override string ToString()
    {
        var name = GetName(this);
        return string.IsNullOrEmpty(name) ? _value?.ToString() ?? "null" : name;
    }
}

// Extension methods for easier registration
public static class TfEnumExtensions
{
    /// <summary>
    /// Register this enum value with optional display name
    /// </summary>
    public static void RegisterName(this Enum enumValue, string? displayName = null)
    {
        TfEnum.AddName(enumValue, displayName);
    }
}