using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Base class for all prims that can be rendered or have a meaningful geometric interpretation.
/// UsdGeomImageable provides attributes and methods for controlling visibility and purpose.
/// </summary>
[UsdSchema("Imageable", UsdSchemaKind.AbstractTyped, IsAbstract = true)]
public class UsdGeomImageable : UsdTyped
{
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomImageable on the prim held by schemaObj.
    /// </summary>
    public UsdGeomImageable(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct an invalid UsdGeomImageable.
    /// </summary>
    protected UsdGeomImageable() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.AbstractTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Imageable");
    protected override TfToken GetTypeName() => TfToken.Empty; // Abstract schema has no concrete type
    
    #endregion
    
    #region Visibility Management
    
    /// <summary>
    /// Visibility of this prim.
    /// "inherited" means inherit from parent, "invisible" means prune this subtree.
    /// </summary>
    public UsdGeomVisibility Visibility
    {
        get
        {
            var attr = GetVisibilityAttr();
            if (!attr.IsValid())
                return UsdGeomVisibility.Inherited;
                
            if (attr.Get(out string value))
            {
                return value switch
                {
                    "invisible" => UsdGeomVisibility.Invisible,
                    _ => UsdGeomVisibility.Inherited
                };
            }
            return UsdGeomVisibility.Inherited;
        }
        set
        {
            var attr = CreateVisibilityAttr();
            var tokenValue = value switch
            {
                UsdGeomVisibility.Invisible => "invisible",
                _ => "inherited"
            };
            attr.Set(tokenValue);
        }
    }
    
    /// <summary>
    /// Get the visibility attribute.
    /// </summary>
    public UsdAttribute GetVisibilityAttr()
    {
        return GetAttribute(new TfToken("visibility"));
    }
    
    /// <summary>
    /// Create the visibility attribute with default value.
    /// </summary>
    public UsdAttribute CreateVisibilityAttr()
    {
        return CreateAttribute(
            new TfToken("visibility"), 
            "token", 
            false, 
            SdfVariability.Varying, 
            new VtValue("inherited"));
    }
    
    /// <summary>
    /// Compute the effective visibility at the given time.
    /// This walks up the hierarchy to find the effective visibility.
    /// </summary>
    public UsdGeomVisibility ComputeVisibility(UsdTimeCode time = default)
    {
        // Start with this prim
        var currentPrim = _prim;
        
        while (currentPrim.IsValid())
        {
            var imageable = new UsdGeomImageable(currentPrim);
            var attr = imageable.GetVisibilityAttr();
            
            if (attr.IsValid() && attr.Get(out string value, time))
            {
                if (value == "invisible")
                    return UsdGeomVisibility.Invisible;
            }
            
            // Move up to parent
            currentPrim = currentPrim.Parent ?? new UsdPrim();
        }
        
        return UsdGeomVisibility.Inherited;
    }
    
    #endregion
    
    #region Purpose Management
    
    /// <summary>
    /// Purpose classification for this prim.
    /// Used for filtering during traversal and rendering.
    /// </summary>
    public UsdGeomPurpose Purpose
    {
        get
        {
            var attr = GetPurposeAttr();
            if (!attr.IsValid())
                return UsdGeomPurpose.Default;
                
            if (attr.Get(out string value))
            {
                return value switch
                {
                    "render" => UsdGeomPurpose.Render,
                    "proxy" => UsdGeomPurpose.Proxy,
                    "guide" => UsdGeomPurpose.Guide,
                    _ => UsdGeomPurpose.Default
                };
            }
            return UsdGeomPurpose.Default;
        }
        set
        {
            var attr = CreatePurposeAttr();
            var tokenValue = value switch
            {
                UsdGeomPurpose.Render => "render",
                UsdGeomPurpose.Proxy => "proxy",
                UsdGeomPurpose.Guide => "guide",
                _ => "default"
            };
            attr.Set(tokenValue);
        }
    }
    
    /// <summary>
    /// Get the purpose attribute.
    /// </summary>
    public UsdAttribute GetPurposeAttr()
    {
        return GetAttribute(new TfToken("purpose"));
    }
    
    /// <summary>
    /// Create the purpose attribute with default value.
    /// </summary>
    public UsdAttribute CreatePurposeAttr()
    {
        return CreateAttribute(
            new TfToken("purpose"), 
            "uniform token", 
            false, 
            SdfVariability.Uniform, 
            new VtValue("default"));
    }
    
    /// <summary>
    /// Compute the effective purpose.
    /// Purpose is NOT inherited - each prim has its own purpose.
    /// </summary>
    public UsdGeomPurpose ComputePurpose()
    {
        return Purpose;
    }
    
    #endregion
    
    #region Proxy Relationships
    
    /// <summary>
    /// Get the proxy prim relationship.
    /// This allows render prims to point to their proxy counterparts.
    /// </summary>
    public UsdRelationship GetProxyPrimRel()
    {
        return GetRelationship(new TfToken("proxyPrim"));
    }
    
    /// <summary>
    /// Create the proxy prim relationship.
    /// </summary>
    public UsdRelationship CreateProxyPrimRel()
    {
        return CreateRelationship(new TfToken("proxyPrim"));
    }
    
    /// <summary>
    /// Set the proxy prim for this imageable.
    /// </summary>
    public bool SetProxyPrim(UsdPrim proxyPrim)
    {
        if (!proxyPrim.IsValid())
            return false;
            
        var rel = CreateProxyPrimRel();
        return rel.SetTargets(new[] { proxyPrim.GetPath() });
    }
    
    /// <summary>
    /// Get the proxy prim for this imageable.
    /// </summary>
    public UsdPrim? GetProxyPrim()
    {
        var rel = GetProxyPrimRel();
        if (!rel.IsValid())
            return null;
            
        var targets = rel.GetTargets();
        if (targets.Length > 0)
        {
            var stage = _prim.GetStage();
            if (stage != null)
            {
                var proxyPrim = stage.GetPrimAtPath(targets[0]);
                return proxyPrim.IsValid() ? proxyPrim : null;
            }
        }
        
        return null;
    }
    
    #endregion
    
    #region Static Factory Methods
    
    
    /// <summary>
    /// Define a UsdGeomImageable on the stage.
    /// Since UsdGeomImageable is abstract, this typically shouldn't be called directly.
    /// </summary>
    public static UsdGeomImageable Define(UsdStage stage, SdfPath path)
    {
        // Note: This is abstract, so typically you'd define concrete subclasses
        // But we provide this for completeness
        if (stage == null)
            return new UsdGeomImageable();
            
        var prim = stage.DefinePrim(path);
        if (!prim.IsValid())
            return new UsdGeomImageable();
            
        return new UsdGeomImageable(prim);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if this prim has any imageable attributes authored.
    /// </summary>
    public bool HasImageableAttributes()
    {
        return GetVisibilityAttr().IsValid() || 
               GetPurposeAttr().IsValid() || 
               GetProxyPrimRel().IsValid();
    }
    
    /// <summary>
    /// Get all imageable-related attributes for this prim.
    /// </summary>
    public IEnumerable<UsdAttribute> GetImageableAttributes()
    {
        var attributes = new List<UsdAttribute>();
        
        var visAttr = GetVisibilityAttr();
        if (visAttr.IsValid())
            attributes.Add(visAttr);
            
        var purposeAttr = GetPurposeAttr();
        if (purposeAttr.IsValid())
            attributes.Add(purposeAttr);
            
        return attributes;
    }
    
    /// <summary>
    /// Make this prim invisible.
    /// </summary>
    public void MakeInvisible()
    {
        Visibility = UsdGeomVisibility.Invisible;
    }
    
    /// <summary>
    /// Make this prim visible (inherited).
    /// </summary>
    public void MakeVisible()
    {
        Visibility = UsdGeomVisibility.Inherited;
    }
    
    /// <summary>
    /// Check if this prim is effectively visible at the given time.
    /// </summary>
    public bool IsVisible(UsdTimeCode time = default)
    {
        return ComputeVisibility(time) != UsdGeomVisibility.Invisible;
    }
    
    #endregion
}