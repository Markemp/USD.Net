using Pxr.Base.Tf;

namespace Pxr.Base.Vt;

/// <summary>
/// VtValue is a type-erased container that can hold values of any type, providing runtime type information.
/// It allows USD to store heterogeneous data while maintaining type safety through runtime checks.
/// </summary>
public sealed class VtValue : IEquatable<VtValue>
{
    // Static empty instance for efficiency
    public static readonly VtValue Empty = new VtValue();

    // Cast registry for custom type conversions
    private static readonly Dictionary<(Type from, Type to), Delegate> _castRegistry = new();
    private static readonly object _castRegistryLock = new object();

    private readonly object? _value;
    private readonly Type _type;
    private readonly bool _isEmpty;

    static VtValue()
    {
        RegisterBuiltinCasts();
    }

    public VtValue()
    {
        _value = null;
        _type = typeof(void);
        _isEmpty = true;
    }

    public VtValue(object? value)
    {
        if (value == null)
        {
            _value = null;
            _type = typeof(void);
            _isEmpty = true;
        }
        else
        {
            _value = value;
            _type = value.GetType();
            _isEmpty = false;
        }
    }

    /// <summary>
    /// Create a VtValue holding the specified value.
    /// </summary>
    public static VtValue Create<T>(T value) => new VtValue(value);

    /// <summary>
    /// Create an empty VtValue.
    /// </summary>
    public static VtValue CreateEmpty() => Empty;

    /// <summary>
    /// Returns true if this value is empty.
    /// </summary>
    public bool IsEmpty() => _isEmpty;

    /// <summary>
    /// Returns true if the value is not empty.
    /// </summary>
    public bool IsHolding() => !_isEmpty;

    /// <summary>
    /// Get the value cast to type T. Throws if the cast fails.
    /// </summary>
    public T Get<T>()
    {
        if (_isEmpty)
        {
            throw new InvalidOperationException($"Cannot get value of type '{typeof(T).Name}' from empty VtValue");
        }

        // Try direct cast first
        if (_value is T directCast)
            return directCast;

        // Try registered cast
        var castResult = TryCast(typeof(T));
        if (castResult != null)
            return (T)castResult;

        // Fall back to Convert.ChangeType
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType);

        try
        {
            if (underlyingType != null)
            {
                if (_value == null)
                    return default!;
                var convertedValue = Convert.ChangeType(_value, underlyingType);
                return (T)Activator.CreateInstance(targetType, convertedValue)!;
            }

            return (T)Convert.ChangeType(_value!, targetType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert from type '{_type.Name}' to '{targetType.Name}'", ex);
        }
    }

    /// <summary>
    /// Get the value cast to type T, or defaultValue if empty or cast fails.
    /// </summary>
    public T GetWithDefault<T>(T defaultValue = default!)
    {
        if (_isEmpty)
            return defaultValue;

        try
        {
            return Get<T>();
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Try to get the value as type T. Returns true if successful.
    /// </summary>
    public bool TryGet<T>(out T value)
    {
        if (_isEmpty)
        {
            value = default!;
            return false;
        }

        try
        {
            value = Get<T>();
            return true;
        }
        catch
        {
            value = default!;
            return false;
        }
    }

    /// <summary>
    /// Returns the raw value without casting.
    /// </summary>
    public object? UncheckedGet() => _value;

    /// <summary>
    /// Returns true if this value is holding an object of type T.
    /// </summary>
    public bool IsHolding<T>()
    {
        if (_isEmpty)
            return false;

        var targetType = typeof(T);

        // Direct type match
        if (_type == targetType)
            return true;

        // Direct value type check
        if (_value is T)
            return true;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            return _value == null || _type == underlyingType || underlyingType.IsAssignableFrom(_type);
        }

        return false;
    }

    /// <summary>
    /// Returns true if this value is holding an object of the specified type.
    /// </summary>
    public bool IsHolding(Type type)
    {
        if (_isEmpty)
            return false;

        // Direct type match
        if (_type == type)
            return true;

        // Assignability check
        if (_value != null && type.IsAssignableFrom(_value.GetType()))
            return true;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            return _value == null || _type == underlyingType || underlyingType.IsAssignableFrom(_type);
        }

        return false;
    }

    /// <summary>
    /// Return true if this value is holding an array type (e.g., VtArray or .NET array).
    /// </summary>
    public bool IsArrayValued()
    {
        return !_isEmpty && _value is Array;
    }

    /// <summary>
    /// Return the number of elements in the array, or 0 if not an array.
    /// </summary>
    public int GetArraySize()
    {
        return _value is Array array ? array.Length : 0;
    }

    /// <summary>
    /// Returns the C# Type object for the held type (alias for GetType).
    /// </summary>
    public Type GetTypeid() => _type;

    /// <summary>
    /// Returns the C# Type object for the held type (more C# idiomatic name).
    /// </summary>
    public Type GetHeldType() => _type;

    /// <summary>
    /// Return the type name of the held type.
    /// </summary>
    public string GetTypeName() => _type.Name;

    /// <summary>
    /// Return the element type if this value is holding an array, otherwise null.
    /// </summary>
    public Type? GetElementTypeid()
        => _value is Array array ? array.GetType().GetElementType() : null;

    /// <summary>
    /// Return a hash code for the held value.
    /// </summary>
    public override int GetHashCode()
    {
        if (_isEmpty)
            return 0;
        return HashCode.Combine(_value, _type);
    }

    /// <summary>
    /// Returns true if this value can be hashed.
    /// </summary>
    public bool CanHash() => true; // In C#, all objects can be hashed

    /// <summary>
    /// Test for equality.
    /// </summary>
    public override bool Equals(object? obj) => obj is VtValue other && Equals(other);

    /// <summary>
    /// Test for equality.
    /// </summary>
    public bool Equals(VtValue? other)
    {
        if (other is null)
            return false;

        if (_isEmpty && other._isEmpty)
            return true;

        if (_isEmpty != other._isEmpty)
            return false;

        if (_type != other._type)
            return false;

        return Equals(_value, other._value);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(VtValue? left, VtValue? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(VtValue? left, VtValue? right) => !(left == right);

    /// <summary>
    /// Return a string representation of this value.
    /// </summary>
    public override string ToString()
    {
        if (_isEmpty)
            return "VtValue()";

        if (_value is Array array)
        {
            var elements = array.Cast<object>().Take(10).Select(e => e?.ToString() ?? "null");
            var preview = string.Join(", ", elements);
            if (array.Length > 10)
                preview += ", ...";
            return $"VtValue([{preview}])";
        }

        return $"VtValue({_value})";
    }

    /// <summary>
    /// Stream this value out to a TextWriter.
    /// </summary>
    public void WriteTo(TextWriter writer)
    {
        writer.Write(ToString());
    }

    /// <summary>
    /// Swap the held value with another VtValue.
    /// </summary>
    public VtValue Swap(VtValue other)
    {
        var temp = new VtValue(other._value);
        other = new VtValue(this._value);
        return temp;
    }

    /// <summary>
    /// Cast the held value to the type of other.
    /// </summary>
    public VtValue CastToTypeOf(VtValue other)
    {
        if (_isEmpty || other._isEmpty)
            return Empty;

        return CastToTypeid(other._type);
    }

    /// <summary>
    /// Cast the held value to the specified type.
    /// </summary>
    public VtValue CastToTypeid(Type targetType)
    {
        if (_isEmpty)
            return Empty;

        if (_type == targetType)
            return this;

        var result = TryCast(targetType);
        if (result != null)
            return new VtValue(result);

        // Try Convert.ChangeType as fallback
        try
        {
            var converted = Convert.ChangeType(_value, targetType);
            return new VtValue(converted);
        }
        catch
        {
            return Empty;
        }
    }

    /// <summary>
    /// Try to cast using registered converters.
    /// </summary>
    private object? TryCast(Type targetType)
    {
        lock (_castRegistryLock)
        {
            if (_castRegistry.TryGetValue((_type, targetType), out var converter))
                return converter.DynamicInvoke(_value);
        }
        return null;
    }

    /// <summary>
    /// Register a cast from one type to another.
    /// </summary>
    public static void RegisterCast<TFrom, TTo>(Func<TFrom, TTo> converter)
    {
        lock (_castRegistryLock)
        {
            _castRegistry[(typeof(TFrom), typeof(TTo))] = converter;
        }
    }

    /// <summary>
    /// Returns true if a cast from one type to another has been registered.
    /// </summary>
    public static bool CanCast(Type from, Type to)
    {
        if (from == to)
            return true;

        lock (_castRegistryLock)
        {
            return _castRegistry.ContainsKey((from, to));
        }
    }

    /// <summary>
    /// Register built-in numeric casts (similar to C++ version).
    /// </summary>
    private static void RegisterBuiltinCasts()
    {
        // Register bidirectional casts for common numeric types
        RegisterNumericCasts<bool, int>();
        RegisterNumericCasts<bool, float>();
        RegisterNumericCasts<bool, double>();

        RegisterNumericCasts<int, float>();
        RegisterNumericCasts<int, double>();
        RegisterNumericCasts<int, long>();
        RegisterNumericCasts<int, uint>();

        RegisterNumericCasts<float, double>();
        RegisterNumericCasts<long, double>();

        RegisterCast<TfToken, string>(token => token.GetText());
        RegisterCast<string, TfToken>(str => new TfToken(str));
    }

    private static void RegisterNumericCasts<T1, T2>()
        where T1 : IConvertible
        where T2 : IConvertible
    {
        RegisterCast<T1, T2>(a => (T2)Convert.ChangeType(a, typeof(T2)));
        RegisterCast<T2, T1>(b => (T1)Convert.ChangeType(b, typeof(T1)));
    }

    /// <summary>
    /// Create a VtValue holding a default value for the specified type.
    /// </summary>
    public static VtValue CreateDefault(Type type)
    {
        if (type.IsValueType)
        {
            return new VtValue(Activator.CreateInstance(type));
        }
        return Empty;
    }

    /// <summary>
    /// Create a VtValue holding a default value of type T.
    /// </summary>
    public static VtValue CreateDefault<T>()
    {
        return new VtValue(default(T));
    }
}

/// <summary>
/// Extension methods for VtValue to make porting easier.
/// </summary>
public static class VtValueExtensions
{
    /// <summary>
    /// Stream out a vector of VtValues (matching C++ VtStreamOut).
    /// </summary>
    public static void StreamOut(this IEnumerable<VtValue> values, TextWriter writer)
    {
        writer.Write('[');
        bool first = true;
        foreach (var value in values)
        {
            if (!first)
                writer.Write(", ");
            writer.Write(value.ToString());
            first = false;
        }
        writer.Write(']');
    }
}