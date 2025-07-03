using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Schema wrapper for UsdAttribute for authoring and introspecting attributes
/// that are primvars (primitive variables).
/// 
/// UsdGeomPrimvar provides API for authoring and retrieving the additional data
/// required to encode an attribute as a "Primvar", which is a convenient contraction
/// of RenderMan's "Primitive Variable" concept.
/// </summary>
public class UsdGeomPrimvar
{
    private readonly UsdAttribute _attr;
    
    #region Construction and Validation
    
    /// <summary>
    /// Create a UsdGeomPrimvar from an existing UsdAttribute.
    /// The attribute may or may not actually be a valid primvar.
    /// </summary>
    public UsdGeomPrimvar(UsdAttribute attr)
    {
        _attr = attr;
    }
    
    /// <summary>
    /// Create an invalid primvar.
    /// </summary>
    public UsdGeomPrimvar()
    {
        _attr = new UsdAttribute();
    }
    
    /// <summary>
    /// Check if this primvar is valid (backed by a valid attribute).
    /// </summary>
    public bool IsValid() => _attr.IsValid();
    
    /// <summary>
    /// Check if the underlying attribute has an authored value.
    /// </summary>
    public bool HasValue() => _attr.IsValid() && _attr.HasValue();
    
    /// <summary>
    /// Check if an attribute represents a valid primvar.
    /// </summary>
    public static bool IsPrimvar(UsdAttribute attr)
    {
        if (!attr.IsValid())
            return false;
            
        var attrName = attr.GetName();
        return UsdGeomPrimvarConstants.IsPrimvarName(attrName);
    }
    
    /// <summary>
    /// Check if a name is a valid primvar name.
    /// </summary>
    public static bool IsValidPrimvarName(TfToken name)
    {
        return UsdGeomPrimvarConstants.IsValidPrimvarName(name.GetText());
    }
    
    /// <summary>
    /// Check if a name is a valid primvar name.
    /// </summary>
    public static bool IsValidPrimvarName(string name)
    {
        return UsdGeomPrimvarConstants.IsValidPrimvarName(name);
    }
    
    /// <summary>
    /// Access the underlying UsdAttribute.
    /// </summary>
    public UsdAttribute GetAttr() => _attr;
    
    #endregion
    
    #region Interpolation Management
    
    /// <summary>
    /// Get the interpolation for this primvar.
    /// Returns the default "vertex" interpolation if not authored.
    /// </summary>
    public UsdGeomInterpolation GetInterpolation()
    {
        if (!_attr.IsValid())
            return UsdGeomInterpolation.Vertex;
            
        var interpolationValue = _attr.GetMetadata<string>("interpolation");
        if (!string.IsNullOrEmpty(interpolationValue))
            return UsdGeomPrimvarConstants.StringToInterpolation(interpolationValue);
            
        return UsdGeomInterpolation.Vertex; // Default
    }
    
    /// <summary>
    /// Set the interpolation for this primvar.
    /// </summary>
    public bool SetInterpolation(UsdGeomInterpolation interpolation)
    {
        if (!_attr.IsValid())
            return false;
            
        if (!UsdGeomPrimvarConstants.IsValidInterpolation(interpolation))
            return false;
            
        var interpolationStr = UsdGeomPrimvarConstants.InterpolationToString(interpolation);
        return _attr.SetMetadata("interpolation", new VtValue(interpolationStr));
    }
    
    /// <summary>
    /// Check if interpolation has been explicitly authored.
    /// </summary>
    public bool HasAuthoredInterpolation()
    {
        if (!_attr.IsValid())
            return false;
            
        var interpolationValue = _attr.GetMetadata<string>("interpolation");
        return !string.IsNullOrEmpty(interpolationValue);
    }
    
    /// <summary>
    /// Check if an interpolation value is valid.
    /// </summary>
    public static bool IsValidInterpolation(UsdGeomInterpolation interpolation)
    {
        return UsdGeomPrimvarConstants.IsValidInterpolation(interpolation);
    }
    
    #endregion
    
    #region Element Size Management
    
    /// <summary>
    /// Get the element size for this primvar.
    /// Returns 1 if not authored (indicating single values).
    /// </summary>
    public int GetElementSize()
    {
        if (!_attr.IsValid())
            return 1;
            
        var elementSize = _attr.GetMetadata<int>("elementSize");
        return elementSize > 0 ? elementSize : 1;
    }
    
    /// <summary>
    /// Set the element size for this primvar.
    /// </summary>
    public bool SetElementSize(int elementSize)
    {
        if (!_attr.IsValid() || elementSize <= 0)
            return false;
            
        return _attr.SetMetadata("elementSize", new VtValue(elementSize));
    }
    
    /// <summary>
    /// Check if element size has been explicitly authored.
    /// </summary>
    public bool HasAuthoredElementSize()
    {
        if (!_attr.IsValid())
            return false;
            
        var elementSize = _attr.GetMetadata<int>("elementSize");
        return elementSize > 0;
    }
    
    #endregion
    
    #region Value Access (Wraps UsdAttribute API)
    
    /// <summary>
    /// Get the value of this primvar at the given time.
    /// </summary>
    public bool Get<T>(out T value, UsdTimeCode time = default)
    {
        value = default(T)!;
        
        if (!_attr.IsValid())
            return false;
            
        return _attr.Get(out value, time);
    }
    
    /// <summary>
    /// Set the value of this primvar at the given time.
    /// </summary>
    public bool Set<T>(T value, UsdTimeCode time = default)
    {
        if (!_attr.IsValid())
            return false;
            
        return _attr.Set(new VtValue(value), time);
    }
    
    /// <summary>
    /// Get the type name of this primvar.
    /// </summary>
    public string GetTypeName()
    {
        if (!_attr.IsValid())
            return string.Empty;
            
        return _attr.GetTypeName();
    }
    
    #endregion
    
    #region Indexed Primvar Support
    
    /// <summary>
    /// Set the indices for this primvar, making it an indexed primvar.
    /// </summary>
    public bool SetIndices(List<int> indices, UsdTimeCode time = default)
    {
        var indicesAttr = GetIndicesAttr();
        if (!indicesAttr.IsValid())
        {
            indicesAttr = CreateIndicesAttr();
            if (!indicesAttr.IsValid())
                return false;
        }
        
        return indicesAttr.Set(new VtValue(indices), time);
    }
    
    /// <summary>
    /// Get the indices for this primvar if it's indexed.
    /// </summary>
    public bool GetIndices(out List<int> indices, UsdTimeCode time = default)
    {
        indices = new List<int>();
        
        var indicesAttr = GetIndicesAttr();
        if (!indicesAttr.IsValid())
            return false;
            
        return indicesAttr.Get(out indices, time);
    }
    
    /// <summary>
    /// Check if this primvar is indexed (has indices attribute).
    /// </summary>
    public bool IsIndexed()
    {
        var indicesAttr = GetIndicesAttr();
        return indicesAttr.IsValid() && indicesAttr.HasValue();
    }
    
    /// <summary>
    /// Block the indices for this primvar, making it non-indexed.
    /// </summary>
    public void BlockIndices()
    {
        var indicesAttr = GetIndicesAttr();
        if (indicesAttr.IsValid())
        {
            indicesAttr.Block();
        }
    }
    
    /// <summary>
    /// Get the indices attribute for this primvar.
    /// </summary>
    private UsdAttribute GetIndicesAttr()
    {
        if (!_attr.IsValid())
            return new UsdAttribute();
            
        var stage = _attr.GetStage();
        if (stage == null) return new UsdAttribute();
        var attrPath = _attr.GetPath();
        var primPath = attrPath.IsPropertyPath() ? attrPath.GetParentPath() : attrPath;
        var prim = stage.GetPrimAtPath(primPath);
        var attrName = _attr.GetName() + UsdGeomPrimvarConstants.IndicesSuffix;
        
        return prim.GetAttribute(new TfToken(attrName));
    }
    
    /// <summary>
    /// Create the indices attribute for this primvar.
    /// </summary>
    private UsdAttribute CreateIndicesAttr()
    {
        if (!_attr.IsValid())
            return new UsdAttribute();
            
        var stage = _attr.GetStage();
        if (stage == null) return new UsdAttribute();
        var attrPath = _attr.GetPath();
        var primPath = attrPath.IsPropertyPath() ? attrPath.GetParentPath() : attrPath;
        var prim = stage.GetPrimAtPath(primPath);
        var attrName = _attr.GetName() + UsdGeomPrimvarConstants.IndicesSuffix;
        
        return prim.CreateAttribute(new TfToken(attrName), "int[]");
    }
    
    /// <summary>
    /// Compute the flattened (de-indexed) values for this primvar.
    /// If not indexed, returns the original values.
    /// </summary>
    public bool ComputeFlattened<T>(out List<T> flattened, UsdTimeCode time = default)
    {
        flattened = new List<T>();
        
        // Get the raw values
        if (!Get(out List<T> values, time))
            return false;
            
        // If not indexed, return original values
        if (!IsIndexed())
        {
            flattened = values;
            return true;
        }
        
        // Get indices and flatten
        if (!GetIndices(out var indices, time))
            return false;
            
        foreach (var index in indices)
        {
            if (index >= 0 && index < values.Count)
                flattened.Add(values[index]);
            else
                return false; // Invalid index
        }
        
        return true;
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get the primvar name (base name without "primvars:" prefix).
    /// </summary>
    public TfToken GetPrimvarName()
    {
        if (!_attr.IsValid())
            return new TfToken();
            
        var attrName = _attr.GetName();
        var baseName = UsdGeomPrimvarConstants.GetPrimvarBaseName(attrName);
        return new TfToken(baseName);
    }
    
    /// <summary>
    /// Check if the primvar name contains namespaces (has colons in base name).
    /// </summary>
    public bool NameContainsNamespaces()
    {
        var primvarName = GetPrimvarName().GetText();
        return !string.IsNullOrEmpty(primvarName) && primvarName.Contains(':');
    }
    
    /// <summary>
    /// Get declaration information for this primvar.
    /// </summary>
    public void GetDeclarationInfo(
        out TfToken name, 
        out string typeName, 
        out UsdGeomInterpolation interpolation, 
        out int elementSize)
    {
        name = GetPrimvarName();
        typeName = GetTypeName();
        interpolation = GetInterpolation();
        elementSize = GetElementSize();
    }
    
    /// <summary>
    /// Check if this primvar is defined (valid and has authored value).
    /// </summary>
    public bool IsDefined() => _attr.IsValid() && _attr.IsDefined();
    
    /// <summary>
    /// Get the prim that owns this primvar.
    /// </summary>
    public UsdPrim GetPrim() 
    {
        if (!_attr.IsValid()) return new UsdPrim();
        var stage = _attr.GetStage();
        if (stage == null) return new UsdPrim();
        var attrPath = _attr.GetPath();
        var primPath = attrPath.IsPropertyPath() ? attrPath.GetParentPath() : attrPath;
        return stage.GetPrimAtPath(primPath);
    }
    
    /// <summary>
    /// Check if two primvars are equal.
    /// </summary>
    public bool Equals(UsdGeomPrimvar other) => _attr.Equals(other._attr);
    
    /// <summary>
    /// String representation for debugging.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "Invalid UsdGeomPrimvar";
            
        var name = GetPrimvarName().GetText();
        var interpolation = GetInterpolation();
        var typeName = GetTypeName();
        var indexed = IsIndexed() ? " (indexed)" : "";
        
        return $"UsdGeomPrimvar '{name}' [{typeName}] {interpolation}{indexed}";
    }
    
    #endregion
}