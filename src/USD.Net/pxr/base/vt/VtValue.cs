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
        
        var targetType = typeof(T);
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        
        // Handle null values
        if (_value == null)
        {
            if (underlyingType != null || !targetType.IsValueType)
                return default!;
            
            throw new InvalidOperationException($"Cannot convert null to value type {targetType.Name}");
        }

        try
        {
            // Handle nullable types
            if (underlyingType != null)
            {
                var convertedValue = Convert.ChangeType(_value, underlyingType);
                return (T)Activator.CreateInstance(targetType, convertedValue)!;
            }
            
            // Handle regular conversions
            return (T)Convert.ChangeType(_value, targetType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Cannot convert {_type.Name} to {targetType.Name}", ex);
        }
    }

    public bool IsHolding<T>() 
    {
        var targetType = typeof(T);
        
        // Direct type match
        if (_type == targetType)
            return true;
            
        // Direct value type check
        if (_value != null && _value is T)
            return true;
            
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            return _value == null || _type == underlyingType || underlyingType.IsAssignableFrom(_type);
        }
        
        return false;
    }

    public bool IsHolding(Type type) 
    {
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