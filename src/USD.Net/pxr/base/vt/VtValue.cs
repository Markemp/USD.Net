using System;

namespace Pxr.Base.Vt;

/// <summary>
/// VtValue is a type-erased container that can hold values of any type, providing runtime type information.
/// It allows USD to store heterogeneous data while maintaining type safety through runtime checks.
/// </summary>
public sealed class VtValue
{
    private readonly object? _value;
    private readonly Type _type;

    public VtValue()
    {
        _value = null;
        _type = typeof(void);
    }

    public VtValue(object? value)
    {
        _value = value;
        _type = value?.GetType() ?? typeof(object);
    }

    public T Get<T>()
    {
        if (_value is T directCast)
            return directCast;
        
        if (_value == null && !typeof(T).IsValueType)
            return default!;
            
        if (_value == null && typeof(T).IsValueType)
            throw new InvalidOperationException($"Cannot convert null to value type {typeof(T).Name}");

        try
        {
            return (T)Convert.ChangeType(_value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot convert {_type.Name} to {typeof(T).Name}", ex);
        }
    }

    public bool IsHolding<T>() => _type == typeof(T) || (_value != null && _value is T);

    public bool IsHolding(Type type) => _type == type || (_value != null && type.IsAssignableFrom(_value.GetType()));

    public Type GetHeldType() => _type;

    public bool IsEmpty() => _value == null && _type == typeof(void);

    public object? GetValue() => _value;

    public static VtValue CreateEmpty() => new();

    public static VtValue Create<T>(T value) => new(value);

    public override string ToString()
    {
        if (IsEmpty())
            return "VtValue()";
        
        return $"VtValue({_value})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not VtValue other)
            return false;

        if (_type != other._type)
            return false;

        return Equals(_value, other._value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, _type);
    }
}