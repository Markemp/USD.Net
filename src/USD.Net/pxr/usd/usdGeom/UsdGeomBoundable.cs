using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Boundable", UsdSchemaKind.AbstractTyped, IsAbstract = true)]
public abstract class UsdGeomBoundable : UsdGeomXformable
{
    #region Construction
    
    protected UsdGeomBoundable(UsdPrim prim) : base(prim)
    {
    }
    
    protected UsdGeomBoundable() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.AbstractTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Boundable");
    protected override TfToken GetTypeName() => TfToken.Empty;
    
    #endregion
    
    #region Extent Attribute
    
    /// <summary>
    /// Get the extent attribute. Extent is a 3D range measuring the geometric
    /// extent of the authored gprim in its own local space (transform not applied).
    /// </summary>
    public UsdAttribute GetExtentAttr()
    {
        return GetAttribute(new TfToken("extent"));
    }
    
    /// <summary>
    /// Create the extent attribute with default value.
    /// </summary>
    public UsdAttribute CreateExtentAttr()
    {
        return CreateAttribute(
            new TfToken("extent"), 
            "float3[]", 
            false, 
            SdfVariability.Varying);
    }
    
    #endregion
    
    #region Extent Computation
    
    /// <summary>
    /// Get or compute the extent for this boundable at the given time.
    /// If extent is authored, returns the authored value.
    /// Otherwise attempts to compute extent from geometry.
    /// </summary>
    public bool ComputeExtent(UsdTimeCode time, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        // First try to get authored extent
        var extentAttr = GetExtentAttr();
        if (extentAttr.IsValid() && extentAttr.Get(out List<GfVec3f> authoredExtent, time))
        {
            extent = authoredExtent;
            return true;
        }
        
        // If no authored extent, try to compute from plugins/geometry
        return ComputeExtentFromGeometry(time, out extent);
    }
    
    /// <summary>
    /// Compute extent from the underlying geometry.
    /// This is a fallback when no extent is authored.
    /// Subclasses should override to provide geometry-specific extent computation.
    /// </summary>
    protected virtual bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        // Default implementation returns empty extent
        // Concrete geometry classes should override this
        return false;
    }
    
    /// <summary>
    /// Set the extent for this boundable.
    /// </summary>
    public bool SetExtent(List<GfVec3f> extent, UsdTimeCode time = default)
    {
        var extentAttr = GetExtentAttr();
        if (!extentAttr.IsValid())
        {
            extentAttr = CreateExtentAttr();
        }
        return extentAttr.Set(new VtValue(extent), time);
    }
    
    /// <summary>
    /// Set the extent using min/max bounds.
    /// </summary>
    public bool SetExtent(GfVec3f min, GfVec3f max, UsdTimeCode time = default)
    {
        var extent = new List<GfVec3f> { min, max };
        return SetExtent(extent, time);
    }
    
    /// <summary>
    /// Clear the authored extent.
    /// </summary>
    public bool ClearExtent()
    {
        var extentAttr = GetExtentAttr();
        if (extentAttr.IsValid())
        {
            return extentAttr.Clear();
        }
        return true;
    }
    
    /// <summary>
    /// Check if extent is authored on this boundable.
    /// </summary>
    public bool HasAuthoredExtent()
    {
        var extentAttr = GetExtentAttr();
        return extentAttr.IsValid() && extentAttr.HasValue();
    }
    
    /// <summary>
    /// Get the extent as a bounding box (min, max).
    /// Returns false if extent cannot be determined.
    /// </summary>
    public bool GetExtentBounds(UsdTimeCode time, out GfVec3f min, out GfVec3f max)
    {
        min = new GfVec3f(0, 0, 0);
        max = new GfVec3f(0, 0, 0);
        
        if (ComputeExtent(time, out var extent) && extent.Count >= 2)
        {
            min = extent[0];
            max = extent[1];
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Transform the given extent by a matrix.
    /// Useful for computing world-space bounds.
    /// </summary>
    public static List<GfVec3f> TransformExtent(List<GfVec3f> extent, GfMatrix4d transform)
    {
        if (extent.Count < 2)
            return extent;
            
        var min = extent[0];
        var max = extent[1];
        
        // Transform all 8 corners of the bounding box
        var corners = new[]
        {
            new GfVec3f(min.X, min.Y, min.Z),
            new GfVec3f(max.X, min.Y, min.Z),
            new GfVec3f(min.X, max.Y, min.Z),
            new GfVec3f(max.X, max.Y, min.Z),
            new GfVec3f(min.X, min.Y, max.Z),
            new GfVec3f(max.X, min.Y, max.Z),
            new GfVec3f(min.X, max.Y, max.Z),
            new GfVec3f(max.X, max.Y, max.Z)
        };
        
        var transformedMin = new GfVec3f(float.MaxValue, float.MaxValue, float.MaxValue);
        var transformedMax = new GfVec3f(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var corner in corners)
        {
            var transformedCorner = transform.Transform(corner);
            
            if (transformedCorner.X < transformedMin.X) transformedMin.X = transformedCorner.X;
            if (transformedCorner.Y < transformedMin.Y) transformedMin.Y = transformedCorner.Y;
            if (transformedCorner.Z < transformedMin.Z) transformedMin.Z = transformedCorner.Z;
            
            if (transformedCorner.X > transformedMax.X) transformedMax.X = transformedCorner.X;
            if (transformedCorner.Y > transformedMax.Y) transformedMax.Y = transformedCorner.Y;
            if (transformedCorner.Z > transformedMax.Z) transformedMax.Z = transformedCorner.Z;
        }
        
        return new List<GfVec3f> { transformedMin, transformedMax };
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Expand an extent to include a point.
    /// </summary>
    public static List<GfVec3f> ExpandExtent(List<GfVec3f> extent, GfVec3f point)
    {
        if (extent.Count < 2)
        {
            return new List<GfVec3f> { point, point };
        }
        
        var min = extent[0];
        var max = extent[1];
        
        var newMin = new GfVec3f(
            Math.Min(min.X, point.X),
            Math.Min(min.Y, point.Y),
            Math.Min(min.Z, point.Z)
        );
        
        var newMax = new GfVec3f(
            Math.Max(max.X, point.X),
            Math.Max(max.Y, point.Y),
            Math.Max(max.Z, point.Z)
        );
        
        return new List<GfVec3f> { newMin, newMax };
    }
    
    /// <summary>
    /// Combine two extents into a single extent that encompasses both.
    /// </summary>
    public static List<GfVec3f> CombineExtents(List<GfVec3f> extent1, List<GfVec3f> extent2)
    {
        if (extent1.Count < 2) return extent2;
        if (extent2.Count < 2) return extent1;
        
        var min1 = extent1[0];
        var max1 = extent1[1];
        var min2 = extent2[0];
        var max2 = extent2[1];
        
        var combinedMin = new GfVec3f(
            Math.Min(min1.X, min2.X),
            Math.Min(min1.Y, min2.Y),
            Math.Min(min1.Z, min2.Z)
        );
        
        var combinedMax = new GfVec3f(
            Math.Max(max1.X, max2.X),
            Math.Max(max1.Y, max2.Y),
            Math.Max(max1.Z, max2.Z)
        );
        
        return new List<GfVec3f> { combinedMin, combinedMax };
    }
    
    /// <summary>
    /// Check if an extent is valid (has finite bounds).
    /// </summary>
    public static bool IsValidExtent(List<GfVec3f> extent)
    {
        if (extent.Count < 2)
            return false;
            
        var min = extent[0];
        var max = extent[1];
        
        return float.IsFinite(min.X) && float.IsFinite(min.Y) && float.IsFinite(min.Z) &&
               float.IsFinite(max.X) && float.IsFinite(max.Y) && float.IsFinite(max.Z) &&
               min.X <= max.X && min.Y <= max.Y && min.Z <= max.Z;
    }
    
    /// <summary>
    /// Update the extent based on the current geometry.
    /// This is called automatically when geometry-affecting properties change.
    /// Derived classes should override ComputeExtentFromGeometry to provide specific logic.
    /// </summary>
    protected virtual void UpdateExtentFromGeometry()
    {
        if (ComputeExtentFromGeometry(UsdTimeCode.Default(), out var extent))
        {
            SetExtent(extent);
        }
    }
    
    #endregion
}