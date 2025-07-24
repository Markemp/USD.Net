namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;

/// <summary>
/// Concrete implementation of spec definition.
/// </summary>
internal class SdfSpecDefinition : ISpecDefinition
{
    // Field info tracking metadata and requirements
    private class FieldInfo
    {
        public bool Required { get; set; }
        public bool Metadata { get; set; }
        public TfToken DisplayGroup { get; set; } = TfToken.Empty;
    }
    
    private readonly Dictionary<TfToken, FieldInfo> _fields = new();
    private readonly List<TfToken> _requiredFields = new();
    
    public IReadOnlyList<TfToken> RequiredFields => _requiredFields.AsReadOnly();
    
    public IReadOnlyList<TfToken> GetFields()
    {
        return _fields.Keys.ToList().AsReadOnly();
    }
    
    public IReadOnlyList<TfToken> GetMetadataFields()
    {
        return _fields
            .Where(kvp => kvp.Value.Metadata)
            .Select(kvp => kvp.Key)
            .ToList()
            .AsReadOnly();
    }
    
    public bool IsValidField(TfToken name)
    {
        return _fields.ContainsKey(name);
    }
    
    public bool IsMetadataField(TfToken name)
    {
        return _fields.TryGetValue(name, out var info) && info.Metadata;
    }
    
    public TfToken GetMetadataFieldDisplayGroup(TfToken name)
    {
        return _fields.TryGetValue(name, out var info) && info.Metadata 
            ? info.DisplayGroup 
            : TfToken.Empty;
    }
    
    public bool IsRequiredField(TfToken name)
    {
        return _fields.TryGetValue(name, out var info) && info.Required;
    }
    
    // Methods for building the spec definition
    public SdfSpecDefinition AddField(TfToken name, bool required = false)
    {
        if (!_fields.ContainsKey(name))
        {
            _fields[name] = new FieldInfo { Required = required };
            if (required && !_requiredFields.Contains(name))
            {
                _requiredFields.Add(name);
            }
        }
        return this;
    }
    
    public SdfSpecDefinition AddMetadataField(TfToken name, bool required = false, TfToken? displayGroup = null)
    {
        if (!_fields.ContainsKey(name))
        {
            _fields[name] = new FieldInfo 
            { 
                Required = required, 
                Metadata = true,
                DisplayGroup = displayGroup ?? TfToken.Empty
            };
            if (required && !_requiredFields.Contains(name))
            {
                _requiredFields.Add(name);
            }
        }
        else
        {
            // Update existing field to be metadata
            _fields[name].Metadata = true;
            if (displayGroup.HasValue)
            {
                _fields[name].DisplayGroup = displayGroup.Value;
            }
        }
        return this;
    }
    
    public SdfSpecDefinition CopyFrom(ISpecDefinition other)
    {
        foreach (var field in other.GetFields())
        {
            if (other.IsMetadataField(field))
            {
                AddMetadataField(field, other.IsRequiredField(field), other.GetMetadataFieldDisplayGroup(field));
            }
            else
            {
                AddField(field, other.IsRequiredField(field));
            }
        }
        return this;
    }
}