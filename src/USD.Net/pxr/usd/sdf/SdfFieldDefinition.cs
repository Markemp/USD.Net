namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

/// <summary>
/// Concrete implementation of field definition.
/// </summary>
internal class SdfFieldDefinition : IFieldDefinition
{
    private readonly SdfSchemaBase _schema;
    private readonly List<KeyValuePair<TfToken, object>> _info = new();
    
    public TfToken Name { get; }
    public VtValue FallbackValue { get; private set; }
    public bool IsPlugin { get; private set; }
    public bool IsReadOnly { get; private set; }
    public bool HoldsChildren { get; private set; }
    
    public IReadOnlyList<KeyValuePair<TfToken, object>> Info => _info.AsReadOnly();
    
    // Validator delegates
    private Func<VtValue, SdfAllowed>? _valueValidator;
    private Func<VtValue, SdfAllowed>? _listValueValidator;
    private Func<VtValue, SdfAllowed>? _mapKeyValidator;
    private Func<VtValue, SdfAllowed>? _mapValueValidator;
    
    public SdfFieldDefinition(SdfSchemaBase schema, TfToken name, VtValue fallbackValue)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        Name = name;
        FallbackValue = fallbackValue;
    }
    
    // Fluent API for field configuration
    public SdfFieldDefinition WithFallbackValue(VtValue value)
    {
        FallbackValue = value;
        return this;
    }
    
    public SdfFieldDefinition AsPlugin()
    {
        IsPlugin = true;
        return this;
    }
    
    public SdfFieldDefinition AsReadOnly()
    {
        IsReadOnly = true;
        return this;
    }
    
    public SdfFieldDefinition AsChildren()
    {
        HoldsChildren = true;
        return this;
    }
    
    public SdfFieldDefinition AddInfo(TfToken key, object value)
    {
        _info.Add(new KeyValuePair<TfToken, object>(key, value));
        return this;
    }
    
    public SdfFieldDefinition WithValueValidator(Func<VtValue, SdfAllowed> validator)
    {
        _valueValidator = validator;
        return this;
    }
    
    public SdfFieldDefinition WithListValueValidator(Func<VtValue, SdfAllowed> validator)
    {
        _listValueValidator = validator;
        return this;
    }
    
    public SdfFieldDefinition WithMapKeyValidator(Func<VtValue, SdfAllowed> validator)
    {
        _mapKeyValidator = validator;
        return this;
    }
    
    public SdfFieldDefinition WithMapValueValidator(Func<VtValue, SdfAllowed> validator)
    {
        _mapValueValidator = validator;
        return this;
    }
    
    // Validation methods
    public SdfAllowed IsValidValue<T>(T value)
    {
        return _valueValidator?.Invoke(new VtValue(value)) ?? SdfAllowed.IsAllowed();
    }
    
    public SdfAllowed IsValidListValue<T>(T value)
    {
        return _listValueValidator?.Invoke(new VtValue(value)) ?? SdfAllowed.IsAllowed();
    }
    
    public SdfAllowed IsValidMapKey<T>(T value)
    {
        return _mapKeyValidator?.Invoke(new VtValue(value)) ?? SdfAllowed.IsAllowed();
    }
    
    public SdfAllowed IsValidMapValue<T>(T value)
    {
        return _mapValueValidator?.Invoke(new VtValue(value)) ?? SdfAllowed.IsAllowed();
    }
}