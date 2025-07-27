namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

/// <summary>
/// Generic class that provides information about scene description fields
/// but doesn't actually provide any fields.
/// </summary>
public class SdfSchemaBase : ISdfSchemaBase
{
    private readonly Dictionary<TfToken, ISdfFieldDefinition> _fieldDefinitions = new();
    private readonly Dictionary<SdfSpecType, ISdfSpecDefinition> _specDefinitions = new();
    private readonly HashSet<TfToken> _requiredFieldNames = new();
    private readonly List<SdfValueTypeName> _allTypes = new();
    private readonly Dictionary<TfToken, SdfValueTypeName> _typesByName = new();

    public virtual ISdfFieldDefinition? GetFieldDefinition(TfToken fieldKey)
        => _fieldDefinitions.TryGetValue(fieldKey, out var definition) ? definition : null;

    public virtual ISdfSpecDefinition? GetSpecDefinition(SdfSpecType specType)
        => _specDefinitions.TryGetValue(specType, out var definition) ? definition : null;

    public virtual bool IsRegistered(TfToken fieldKey, out VtValue? fallback)
    {
        var definition = GetFieldDefinition(fieldKey);
        if (definition != null)
        {
            fallback = definition.FallbackValue;
            return true;
        }
        fallback = null;
        return false;
    }

    public virtual bool HoldsChildren(TfToken fieldKey)
    {
        var definition = GetFieldDefinition(fieldKey);
        return definition?.HoldsChildren() ?? false;
    }

    public virtual VtValue GetFallback(TfToken fieldKey)
    {
        var definition = GetFieldDefinition(fieldKey);
        return definition?.FallbackValue ?? VtValue.Empty;
    }

    public virtual VtValue CastToTypeOf(TfToken fieldKey, VtValue value)
    {
        // For now, return the value as-is
        // A full implementation would involve type coercion logic
        return value;
    }

    public virtual bool IsValidFieldForSpec(TfToken fieldKey, SdfSpecType specType)
    {
        var specDefinition = GetSpecDefinition(specType);
        return specDefinition?.IsValidField(fieldKey) ?? false;
    }

    public virtual IReadOnlyList<TfToken> GetFields(SdfSpecType specType)
    {
        var specDefinition = GetSpecDefinition(specType);
        return specDefinition?.GetFields() ?? Array.Empty<TfToken>();
    }

    public virtual IReadOnlyList<TfToken> GetMetadataFields(SdfSpecType specType)
    {
        var specDefinition = GetSpecDefinition(specType);
        return specDefinition?.GetMetadataFields() ?? Array.Empty<TfToken>();
    }

    public virtual TfToken GetMetadataFieldDisplayGroup(SdfSpecType specType, TfToken metadataField)
    {
        var specDefinition = GetSpecDefinition(specType);
        return specDefinition?.GetMetadataFieldDisplayGroup(metadataField) ?? TfToken.Empty;
    }

    public virtual IReadOnlyList<TfToken> GetRequiredFields(SdfSpecType specType)
    {
        var specDefinition = GetSpecDefinition(specType);
        return specDefinition?.RequiredFields ?? Array.Empty<TfToken>();
    }

    public virtual bool IsRequiredFieldName(TfToken fieldName)
    {
        return _requiredFieldNames.Contains(fieldName);
    }

    public virtual SdfAllowed IsValidValue(VtValue value)
    {
        // For now, assume all values are valid
        // A full implementation would check against registered types
        return new SdfAllowed();
    }

    public virtual IReadOnlyList<SdfValueTypeName> GetAllTypes()
        => _allTypes.AsReadOnly();

    public virtual SdfValueTypeName FindType(TfToken typeName)
        => _typesByName.TryGetValue(typeName, out var type) ? type : SdfValueTypeName.Invalid;

    public virtual SdfValueTypeName FindType(string typeName)
        => FindType(new TfToken(typeName));

    public virtual SdfValueTypeName FindType(Type type, TfToken? role = null)
    {
        // For now, return invalid type
        // A full implementation would map C# types to USD types
        return SdfValueTypeName.Invalid;
    }

    public virtual SdfValueTypeName FindType(VtValue value, TfToken? role = null)
    {
        if (value.IsEmpty())
            return SdfValueTypeName.Invalid;
        
        return FindType(value.GetHeldType(), role);
    }

    public virtual SdfValueTypeName FindOrCreateType(TfToken typeName)
    {
        var existing = FindType(typeName);
        if (existing.IsValid)
            return existing;
        
        // Create a temporary type name
        return new SdfValueTypeName(typeName);
    }

    protected virtual void RegisterFieldDefinition(TfToken fieldKey, ISdfFieldDefinition definition)
    {
        _fieldDefinitions[fieldKey] = definition;
    }

    protected virtual void RegisterSpecDefinition(SdfSpecType specType, ISdfSpecDefinition definition)
    {
        _specDefinitions[specType] = definition;
        
        // Add required field names to our set
        foreach (var field in definition.RequiredFields)
        {
            _requiredFieldNames.Add(field);
        }
    }

    protected virtual void RegisterType(SdfValueTypeName typeName)
    {
        _allTypes.Add(typeName);
        _typesByName[typeName.Name] = typeName;
    }
}
