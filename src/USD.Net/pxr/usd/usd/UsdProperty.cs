using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdProperty is the base class for UsdAttribute and UsdRelationship, 
/// representing properties in a scene graph.
/// </summary>
public abstract class UsdProperty : UsdObject
{
    private readonly Dictionary<string, object> _metadata = new();
    private bool _isCustom;
    private string _displayGroup = string.Empty;

    #region Construction

    /// <summary>
    /// Create an invalid property.
    /// </summary>
    protected UsdProperty() : base()
    {
    }

    /// <summary>
    /// Create a property with stage and path.
    /// </summary>
    protected UsdProperty(UsdStage stage, SdfPath path) : base(stage, path)
    {
    }

    #endregion

    #region Property Validation

    /// <summary>
    /// Return true if this property is defined (valid and authored).
    /// </summary>
    public virtual bool IsDefined()
    {
        return IsValid() && IsAuthored();
    }

    /// <summary>
    /// Return true if this property has authored opinions.
    /// </summary>
    public virtual bool IsAuthored()
    {
        // TODO: Check if property has authored opinions in layer stack
        return IsValid();
    }

    /// <summary>
    /// Return true if this property is authored at the current edit target.
    /// </summary>
    public virtual bool IsAuthoredAt(UsdEditTarget? editTarget = null)
    {
        // TODO: Check specific edit target for authoring
        return IsAuthored();
    }

    #endregion

    #region Custom and Namespace Properties

    /// <summary>
    /// Return true if this is a custom property.
    /// </summary>
    public virtual bool IsCustom()
    {
        return _isCustom;
    }

    /// <summary>
    /// Set whether this property is custom.
    /// </summary>
    public virtual void SetCustom(bool custom)
    {
        _isCustom = custom;
    }

    /// <summary>
    /// Return the namespace portion of this property's name.
    /// </summary>
    public virtual string GetNamespace()
    {
        var name = GetName();
        var colonIndex = name.IndexOf(':');
        return colonIndex >= 0 ? name.Substring(0, colonIndex) : string.Empty;
    }

    /// <summary>
    /// Return the base name of this property (without namespace).
    /// </summary>
    public virtual string GetBaseName()
    {
        var name = GetName();
        var colonIndex = name.LastIndexOf(':');
        return colonIndex >= 0 ? name.Substring(colonIndex + 1) : name;
    }

    /// <summary>
    /// Split the property name into namespace components and base name.
    /// </summary>
    public virtual (string[] namespaces, string baseName) SplitName()
    {
        var name = GetName();
        var parts = name.Split(':');
        
        if (parts.Length <= 1)
            return (Array.Empty<string>(), name);
            
        var namespaces = new string[parts.Length - 1];
        Array.Copy(parts, namespaces, parts.Length - 1);
        var baseName = parts[parts.Length - 1];
        
        return (namespaces, baseName);
    }

    #endregion

    #region Display Groups

    /// <summary>
    /// Get the display group for this property.
    /// </summary>
    public virtual string GetDisplayGroup()
    {
        return _displayGroup;
    }

    /// <summary>
    /// Set the display group for this property.
    /// </summary>
    public virtual bool SetDisplayGroup(string displayGroup)
    {
        _displayGroup = displayGroup ?? string.Empty;
        return true;
    }

    /// <summary>
    /// Clear the display group for this property.
    /// </summary>
    public virtual bool ClearDisplayGroup()
    {
        _displayGroup = string.Empty;
        return true;
    }

    /// <summary>
    /// Return true if this property has a display group.
    /// </summary>
    public virtual bool HasDisplayGroup()
    {
        return !string.IsNullOrEmpty(_displayGroup);
    }

    /// <summary>
    /// Get nested display groups as a hierarchy.
    /// </summary>
    public virtual string[] GetNestedDisplayGroups()
    {
        if (string.IsNullOrEmpty(_displayGroup))
            return Array.Empty<string>();
            
        return _displayGroup.Split(':', StringSplitOptions.RemoveEmptyEntries);
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Set metadata for this property.
    /// </summary>
    public virtual bool SetMetadata<T>(TfToken key, T value)
    {
        _metadata[key.GetText()] = value!;
        return true;
    }

    /// <summary>
    /// Get metadata for this property.
    /// </summary>
    public virtual T? GetMetadata<T>(TfToken key)
    {
        if (_metadata.TryGetValue(key.GetText(), out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Return true if this property has metadata with the given key.
    /// </summary>
    public virtual bool HasMetadata(TfToken key)
    {
        return _metadata.ContainsKey(key.GetText());
    }

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public virtual bool ClearMetadata(TfToken key)
    {
        return _metadata.Remove(key.GetText());
    }

    /// <summary>
    /// Get all metadata keys for this property.
    /// </summary>
    public virtual IEnumerable<string> GetMetadataKeys()
    {
        return _metadata.Keys;
    }

    #endregion

    #region Property Stack

    /// <summary>
    /// Get the strength-ordered list of property specs for this property.
    /// </summary>
    public virtual IEnumerable<SdfPropertySpec> GetPropertyStack()
    {
        // TODO: Implement property stack resolution from layer composition
        yield break;
    }

    /// <summary>
    /// Get property specs with layer offsets.
    /// </summary>
    public virtual IEnumerable<(SdfPropertySpec spec, SdfLayerOffset offset)> GetPropertyStackWithLayerOffsets()
    {
        // TODO: Implement property stack with layer offsets
        yield break;
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Implicit conversion to bool for validity checking.
    /// </summary>
    public static implicit operator bool(UsdProperty property)
    {
        return property?.IsValid() == true;
    }

    #endregion
}