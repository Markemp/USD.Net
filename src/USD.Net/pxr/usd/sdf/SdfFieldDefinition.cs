namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

/// <summary>
/// Concrete implementation of field definition.
/// </summary>
internal class SdfFieldDefinition : ISdfFieldDefinition
{
    private readonly ISdfSchemaBase _schema;
    private readonly TfToken _name;
    private VtValue _fallbackValue;
    private bool _isPlugin;
    private bool _isReadOnly;
    private bool _holdsChildren;
    private readonly List<KeyValuePair<TfToken, object>> _info = new();
    
    // Validator delegates
    private Func<VtValue, SdfAllowed>? _valueValidator;
    private Func<VtValue, SdfAllowed>? _listValueValidator;
    private Func<VtValue, SdfAllowed>? _mapKeyValidator;
    private Func<VtValue, SdfAllowed>? _mapValueValidator;
    
    public SdfFieldDefinition(ISdfSchemaBase schema, TfToken name, VtValue fallbackValue)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
        _name = name;
        _fallbackValue = fallbackValue;
    }
    
    // ISdfFieldDefinition implementation
    public TfToken GetName() => _name;
    public VtValue GetFallbackValue() => _fallbackValue;
    public IReadOnlyList<KeyValuePair<TfToken, object>> GetInfo() => _info.AsReadOnly();
    public bool IsPlugin() => _isPlugin;
    public bool IsReadOnly() => _isReadOnly;
    public bool HoldsChildren() => _holdsChildren;
    
    // Fluent API for field configuration
    public SdfFieldDefinition WithFallbackValue(VtValue value)
    {
        _fallbackValue = value;
        return this;
    }
    
    public SdfFieldDefinition AsPlugin()
    {
        _isPlugin = true;
        return this;
    }
    
    public SdfFieldDefinition AsReadOnly()
    {
        _isReadOnly = true;
        return this;
    }
    
    public SdfFieldDefinition AsChildren()
    {
        _holdsChildren = true;
        _isReadOnly = true;  // Match C++ behavior: Children() also sets read-only
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
        => _valueValidator?.Invoke(new VtValue(value)) ?? new SdfAllowed(true);
    
    public SdfAllowed IsValidListValue<T>(T value)
        => _listValueValidator?.Invoke(new VtValue(value)) ?? new SdfAllowed(true);
    
    public SdfAllowed IsValidMapKey<T>(T value)
        => _mapKeyValidator?.Invoke(new VtValue(value)) ?? new SdfAllowed(true);
    
    public SdfAllowed IsValidMapValue<T>(T value)
        => _mapValueValidator?.Invoke(new VtValue(value)) ?? new SdfAllowed(true);
}