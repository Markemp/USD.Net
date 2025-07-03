using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// UsdGeomPrimvarsAPI encodes geometric "primitive variables", as UsdGeomPrimvar,
/// which interpolate across a primitive's topology, can override shader inputs,
/// and inherit down namespace.
/// </summary>
[UsdSchema("PrimvarsAPI", UsdSchemaKind.NonAppliedAPI)]
public class UsdGeomPrimvarsAPI : UsdAPISchemaBase
{
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomPrimvarsAPI on a UsdPrim.
    /// </summary>
    public UsdGeomPrimvarsAPI(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomPrimvarsAPI on an invalid prim.
    /// </summary>
    public UsdGeomPrimvarsAPI() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.NonAppliedAPI;
    protected override TfToken GetSchemaTypeName() => new TfToken("PrimvarsAPI");
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomPrimvarsAPI holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomPrimvarsAPI Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomPrimvarsAPI(prim);
    }
    
    /// <summary>
    /// Return a UsdGeomPrimvarsAPI for the given prim.
    /// </summary>
    public static UsdGeomPrimvarsAPI Get(UsdPrim prim)
    {
        return new UsdGeomPrimvarsAPI(prim);
    }
    
    #endregion
    
    #region Primvar Creation
    
    /// <summary>
    /// Create a new primvar with the given name, type, and interpolation.
    /// </summary>
    public UsdGeomPrimvar CreatePrimvar(
        TfToken name, 
        string typeName, 
        UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex,
        int elementSize = -1)
    {
        if (!Prim.IsValid())
            return new UsdGeomPrimvar();
            
        if (!UsdGeomPrimvarConstants.IsValidPrimvarName(name.GetText()))
            return new UsdGeomPrimvar();
            
        var attrName = UsdGeomPrimvarConstants.MakePrimvarAttrName(name.GetText());
        var attr = Prim.CreateAttribute(new TfToken(attrName), typeName);
        
        if (!attr.IsValid())
            return new UsdGeomPrimvar();
            
        var primvar = new UsdGeomPrimvar(attr);
        
        // Set interpolation if specified
        if (UsdGeomPrimvarConstants.IsValidInterpolation(interpolation))
            primvar.SetInterpolation(interpolation);
            
        // Set element size if specified  
        if (elementSize > 0)
            primvar.SetElementSize(elementSize);
            
        return primvar;
    }
    
    /// <summary>
    /// Create a non-indexed primvar with the given name, type, value, and interpolation.
    /// </summary>
    public UsdGeomPrimvar CreateNonIndexedPrimvar<T>(
        TfToken name,
        string typeName,
        T value,
        UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex,
        int elementSize = -1,
        UsdTimeCode time = default)
    {
        var primvar = CreatePrimvar(name, typeName, interpolation, elementSize);
        if (primvar.IsValid())
        {
            primvar.Set(value, time);
        }
        return primvar;
    }
    
    /// <summary>
    /// Create an indexed primvar with the given name, type, values, indices, and interpolation.
    /// </summary>
    public UsdGeomPrimvar CreateIndexedPrimvar<T>(
        TfToken name,
        string typeName,
        List<T> values,
        List<int> indices,
        UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex,
        int elementSize = -1,
        UsdTimeCode time = default)
    {
        var primvar = CreatePrimvar(name, typeName, interpolation, elementSize);
        if (primvar.IsValid())
        {
            primvar.Set(values, time);
            primvar.SetIndices(indices, time);
        }
        return primvar;
    }
    
    #endregion
    
    #region Primvar Deletion and Blocking
    
    /// <summary>
    /// Remove a primvar by name (blocks the attribute).
    /// </summary>
    public bool RemovePrimvar(TfToken name)
    {
        var primvar = GetPrimvar(name);
        if (!primvar.IsValid())
            return false;
            
        // Block the primvar attribute
        primvar.GetAttr().Block();
        
        // Also block indices if they exist
        if (primvar.IsIndexed())
            primvar.BlockIndices();
        
        return true;
    }
    
    /// <summary>
    /// Block a primvar by name.
    /// </summary>
    public void BlockPrimvar(TfToken name)
    {
        var primvar = GetPrimvar(name);
        if (primvar.IsValid())
        {
            primvar.GetAttr().Block();
            primvar.BlockIndices();
        }
    }
    
    #endregion
    
    #region Basic Primvar Queries
    
    /// <summary>
    /// Get a primvar by name from this prim.
    /// </summary>
    public UsdGeomPrimvar GetPrimvar(TfToken name)
    {
        if (!Prim.IsValid())
            return new UsdGeomPrimvar();
            
        var attrName = UsdGeomPrimvarConstants.MakePrimvarAttrName(name.GetText());
        var attr = Prim.GetAttribute(new TfToken(attrName));
        
        return new UsdGeomPrimvar(attr);
    }
    
    /// <summary>
    /// Check if this prim has a primvar with the given name.
    /// </summary>
    public bool HasPrimvar(TfToken name)
    {
        var primvar = GetPrimvar(name);
        return primvar.IsValid();
    }
    
    /// <summary>
    /// Get all primvars defined on this prim (including those without values).
    /// </summary>
    public List<UsdGeomPrimvar> GetPrimvars()
    {
        var primvars = new List<UsdGeomPrimvar>();
        
        if (!Prim.IsValid())
            return primvars;
            
        var attrs = Prim.GetAttributes();
        foreach (var attr in attrs)
        {
            var attrName = attr.GetName();
            if (UsdGeomPrimvarConstants.IsPrimvarName(attrName) && 
                !attrName.EndsWith(UsdGeomPrimvarConstants.IndicesSuffix))
            {
                primvars.Add(new UsdGeomPrimvar(attr));
            }
        }
        
        return primvars;
    }
    
    /// <summary>
    /// Get all primvars that have authored opinions on this prim.
    /// </summary>
    public List<UsdGeomPrimvar> GetAuthoredPrimvars()
    {
        return GetPrimvars().Where(p => p.GetAttr().IsAuthored()).ToList();
    }
    
    /// <summary>
    /// Get all primvars that have values on this prim.
    /// </summary>
    public List<UsdGeomPrimvar> GetPrimvarsWithValues()
    {
        return GetPrimvars().Where(p => p.HasValue()).ToList();
    }
    
    /// <summary>
    /// Get all primvars that have authored values on this prim.
    /// </summary>
    public List<UsdGeomPrimvar> GetPrimvarsWithAuthoredValues()
    {
        return GetPrimvars().Where(p => p.GetAttr().IsAuthored()).ToList();
    }
    
    #endregion
    
    #region Inheritance Queries
    
    /// <summary>
    /// Find a primvar with the given name, including inherited ones.
    /// This is a simplified version that searches up the namespace hierarchy.
    /// </summary>
    public UsdGeomPrimvar FindPrimvarWithInheritance(TfToken name)
    {
        // First check this prim
        var primvar = GetPrimvar(name);
        if (primvar.IsValid() && primvar.HasValue())
            return primvar;
            
        // Walk up the hierarchy looking for constant primvars
        var prim = Prim;
        while (prim.IsValid())
        {
            var parentPrim = prim.GetParent();
            if (!parentPrim.IsValid())
                break;
                
            var parentAPI = new UsdGeomPrimvarsAPI(parentPrim);
            var parentPrimvar = parentAPI.GetPrimvar(name);
            
            if (parentPrimvar.IsValid() && parentPrimvar.HasValue())
            {
                // Only inherit constant primvars
                if (parentPrimvar.GetInterpolation() == UsdGeomInterpolation.Constant)
                    return parentPrimvar;
            }
            
            prim = parentPrim;
        }
        
        return new UsdGeomPrimvar();
    }
    
    /// <summary>
    /// Find all primvars available at this prim, including inherited ones.
    /// This is a simplified version that searches up the namespace hierarchy.
    /// </summary>
    public List<UsdGeomPrimvar> FindPrimvarsWithInheritance()
    {
        var allPrimvars = new Dictionary<string, UsdGeomPrimvar>();
        
        // Collect primvars from ancestors first (lower precedence)
        var prim = Prim;
        var ancestors = new List<UsdPrim>();
        
        while (prim.IsValid())
        {
            ancestors.Add(prim);
            prim = prim.GetParent();
        }
        
        // Walk from root to leaf, accumulating primvars
        ancestors.Reverse();
        
        foreach (var ancestorPrim in ancestors)
        {
            var ancestorAPI = new UsdGeomPrimvarsAPI(ancestorPrim);
            var ancestorPrimvars = ancestorAPI.GetPrimvarsWithValues();
            
            foreach (var ancestorPrimvar in ancestorPrimvars)
            {
                var name = ancestorPrimvar.GetPrimvarName().GetText();
                
                // Only inherit constant primvars from ancestors
                if (ancestorPrim != Prim && 
                    ancestorPrimvar.GetInterpolation() != UsdGeomInterpolation.Constant)
                    continue;
                    
                // Later prims (closer to leaf) override earlier ones
                allPrimvars[name] = ancestorPrimvar;
            }
        }
        
        return allPrimvars.Values.ToList();
    }
    
    /// <summary>
    /// Find inheritable primvars on this prim (constant interpolation).
    /// </summary>
    public List<UsdGeomPrimvar> FindInheritablePrimvars()
    {
        return GetPrimvarsWithValues()
            .Where(p => p.GetInterpolation() == UsdGeomInterpolation.Constant)
            .ToList();
    }
    
    /// <summary>
    /// Check if this prim possibly has an inherited primvar with the given name.
    /// </summary>
    public bool HasPossiblyInheritedPrimvar(TfToken name)
    {
        var primvar = FindPrimvarWithInheritance(name);
        return primvar.IsValid();
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if a property name can be contained by the primvars API.
    /// </summary>
    public static bool CanContainPropertyName(TfToken name)
    {
        return UsdGeomPrimvarConstants.IsPrimvarName(name.GetText());
    }
    
    /// <summary>
    /// Get all primvar names (base names without "primvars:" prefix) on this prim.
    /// </summary>
    public List<TfToken> GetPrimvarNames()
    {
        var names = new List<TfToken>();
        var primvars = GetPrimvars();
        
        foreach (var primvar in primvars)
        {
            names.Add(primvar.GetPrimvarName());
        }
        
        return names;
    }
    
    /// <summary>
    /// Create common display primvars (displayColor and displayOpacity).
    /// </summary>
    public void CreateDisplayPrimvars(List<GfVec3f>? colors = null, List<float>? opacities = null)
    {
        // Create displayColor primvar
        if (colors != null && colors.Count > 0)
        {
            var colorPrimvar = CreateNonIndexedPrimvar(
                new TfToken("displayColor"),
                "color3f[]",
                colors,
                UsdGeomInterpolation.Constant);
        }
        
        // Create displayOpacity primvar
        if (opacities != null && opacities.Count > 0)
        {
            var opacityPrimvar = CreateNonIndexedPrimvar(
                new TfToken("displayOpacity"),
                "float[]",
                opacities,
                UsdGeomInterpolation.Constant);
        }
    }
    
    #endregion
}