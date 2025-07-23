using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public class SdfAbstractDataValue
{
    protected object? value;
    protected Type valueType;
    public bool isValueBlock;
    public bool typeMismatch;
    
    public SdfAbstractDataValue(object? value, Type valueType)
    {
        this.value = value;
        this.valueType = valueType;
    }
    
    public bool StoreValue<T>(T v)
    {
        if (v is VtValue vtValue)
        {
            return StoreVtValue(vtValue);
        }
        
        isValueBlock = false;
        typeMismatch = false;
        
        if (v is SdfValueBlock)
        {
            isValueBlock = true;
            return true;
        }
        
        if (valueType == typeof(T))
        {
            value = v;
            return true;
        }
        
        typeMismatch = true;
        return false;
    }
    
    protected virtual bool StoreVtValue(VtValue v)
    {
        value = v.GetValue();
        valueType = v.GetType();
        return true;
    }
}

public class SdfAbstractDataTypedValue<T> : SdfAbstractDataValue
{
    public SdfAbstractDataTypedValue(T? value) : base(value, typeof(T))
    {
    }
    
    protected override bool StoreVtValue(VtValue v)
    {
        typeMismatch = false;
        isValueBlock = false;
        
        if (v.IsHolding<T>())
        {
            value = v.Get<T>();
            return true;
        }
        
        typeMismatch = true;
        return false;
    }
}