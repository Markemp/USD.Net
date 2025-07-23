using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

// Base class
public abstract class SdfAbstractDataConstValue
{
    protected readonly object _value;
    protected readonly Type _valueType;

    protected SdfAbstractDataConstValue(object value, Type valueType)
    {
        _value = value;
        _valueType = valueType;
    }

    // Changed to return VtValue instead of modifying parameter
    public abstract VtValue GetValue();

    public abstract bool IsEqual(VtValue value);

    // Template method for typed access (matching C++ pattern)
    public bool GetValue<T>(out T v)
    {
        if (_valueType == typeof(T))
        {
            v = (T)_value;
            return true;
        }
        v = default!;
        return false;
    }
}

// Typed derived class
public class SdfAbstractDataConstTypedValue<T> : SdfAbstractDataConstValue
{
    public SdfAbstractDataConstTypedValue(T value)
        : base(value!, typeof(T))
    {
    }

    // Returns a new VtValue instead of modifying parameter
    public override VtValue GetValue() => new(GetTypedValue());

    public override bool IsEqual(VtValue v)
    {
        return v.IsHolding<T>() &&
               EqualityComparer<T>.Default.Equals(v.UncheckedGet<T>(), GetTypedValue());
    }

    private T GetTypedValue() => (T)_value;
}