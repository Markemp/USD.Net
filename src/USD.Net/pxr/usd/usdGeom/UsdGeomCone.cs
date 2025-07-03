using System;
using System.Collections.Generic;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Defines a primitive cone, centered at the origin, whose base is on the XY plane
/// and whose apex is on the positive Z axis. The cone's symmetry axis aligns with
/// the specified axis.
/// </summary>
[UsdSchema("Cone", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomCone : UsdGeomGprim
{
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Cone");
    
    #endregion
    
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomCone on UsdPrim prim.
    /// </summary>
    public UsdGeomCone(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomCone on an invalid prim.
    /// </summary>
    public UsdGeomCone() : base()
    {
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomCone holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomCone Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomCone(prim);
    }
    
    /// <summary>
    /// Attempt to ensure a UsdPrim adhering to this schema at path is defined
    /// on this stage.
    /// </summary>
    public static new UsdGeomCone Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Cone");
        return new UsdGeomCone(prim);
    }
    
    #endregion
    
    #region Schema Attributes
    
    /// <summary>
    /// The length of the cone's axis from base to apex.
    /// </summary>
    public double Height
    {
        get
        {
            var attr = GetHeightAttr();
            if (attr.Get(out double value))
                return value;
            return 2.0; // Default value
        }
        set
        {
            var attr = CreateHeightAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when height changes
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// The radius of the cone's base.
    /// </summary>
    public double Radius
    {
        get
        {
            var attr = GetRadiusAttr();
            if (attr.Get(out double value))
                return value;
            return 1.0; // Default value
        }
        set
        {
            var attr = CreateRadiusAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when radius changes
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// The axis along which the cone's apex is aligned.
    /// Valid values are "X", "Y", or "Z".
    /// </summary>
    public TfToken Axis
    {
        get
        {
            var attr = GetAxisAttr();
            if (attr.Get(out TfToken value))
                return value;
            return new TfToken("Z"); // Default value
        }
        set
        {
            var attr = CreateAxisAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when axis changes
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// Create the height attribute.
    /// </summary>
    public UsdAttribute CreateHeightAttr()
    {
        return CreateAttribute(new TfToken("height"), "double");
    }
    
    /// <summary>
    /// Get the height attribute.
    /// </summary>
    public UsdAttribute GetHeightAttr()
    {
        return GetAttribute(new TfToken("height"));
    }
    
    /// <summary>
    /// Create the radius attribute.
    /// </summary>
    public UsdAttribute CreateRadiusAttr()
    {
        return CreateAttribute(new TfToken("radius"), "double");
    }
    
    /// <summary>
    /// Get the radius attribute.
    /// </summary>
    public UsdAttribute GetRadiusAttr()
    {
        return GetAttribute(new TfToken("radius"));
    }
    
    /// <summary>
    /// Create the axis attribute.
    /// </summary>
    public UsdAttribute CreateAxisAttr()
    {
        return CreateAttribute(new TfToken("axis"), "token");
    }
    
    /// <summary>
    /// Get the axis attribute.
    /// </summary>
    public UsdAttribute GetAxisAttr()
    {
        return GetAttribute(new TfToken("axis"));
    }
    
    #endregion
    
    #region Extent Computation
    
    /// <summary>
    /// Compute the extent for the cone defined by the given height, radius, and axis.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(double height, double radius, TfToken axis)
    {
        var halfHeight = (float)(height * 0.5);
        var radiusFloat = (float)radius;
        
        var min = Vector3.Zero;
        var max = Vector3.Zero;
        
        switch (axis.GetText())
        {
            case "X":
                min = new Vector3(-halfHeight, -radiusFloat, -radiusFloat);
                max = new Vector3(halfHeight, radiusFloat, radiusFloat);
                break;
            case "Y":
                min = new Vector3(-radiusFloat, -halfHeight, -radiusFloat);
                max = new Vector3(radiusFloat, halfHeight, radiusFloat);
                break;
            case "Z":
            default:
                min = new Vector3(-radiusFloat, -radiusFloat, -halfHeight);
                max = new Vector3(radiusFloat, radiusFloat, halfHeight);
                break;
        }
        
        return (min, max);
    }
    
    /// <summary>
    /// Compute the extent for the cone defined by the given height, radius, and axis,
    /// transformed by the given matrix.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(double height, double radius, TfToken axis, Matrix4x4 transform)
    {
        var (localMin, localMax) = ComputeExtent(height, radius, axis);
        
        // Transform the 8 corners of the bounding box
        var corners = new[]
        {
            new Vector3(localMin.X, localMin.Y, localMin.Z),
            new Vector3(localMax.X, localMin.Y, localMin.Z),
            new Vector3(localMin.X, localMax.Y, localMin.Z),
            new Vector3(localMax.X, localMax.Y, localMin.Z),
            new Vector3(localMin.X, localMin.Y, localMax.Z),
            new Vector3(localMax.X, localMin.Y, localMax.Z),
            new Vector3(localMin.X, localMax.Y, localMax.Z),
            new Vector3(localMax.X, localMax.Y, localMax.Z)
        };
        
        var transformedMin = new Vector3(float.MaxValue);
        var transformedMax = new Vector3(float.MinValue);
        
        foreach (var corner in corners)
        {
            var transformed = Vector3.Transform(corner, transform);
            transformedMin = Vector3.Min(transformedMin, transformed);
            transformedMax = Vector3.Max(transformedMax, transformed);
        }
        
        return (transformedMin, transformedMax);
    }
    
    #endregion
    
    #region Geometric Utilities
    
    /// <summary>
    /// Get the volume of this cone.
    /// </summary>
    public double GetVolume()
    {
        var h = Height;
        var r = Radius;
        
        // Volume = (1/3) * π * r² * h
        return (1.0 / 3.0) * Math.PI * r * r * h;
    }
    
    /// <summary>
    /// Get the surface area of this cone (including base).
    /// </summary>
    public double GetSurfaceArea()
    {
        var h = Height;
        var r = Radius;
        
        // Surface area = base area + lateral area
        // Base area: π * r²
        // Lateral area: π * r * slant_height where slant_height = sqrt(r² + h²)
        var baseArea = Math.PI * r * r;
        var slantHeight = Math.Sqrt(r * r + h * h);
        var lateralArea = Math.PI * r * slantHeight;
        
        return baseArea + lateralArea;
    }
    
    /// <summary>
    /// Get the slant height of this cone.
    /// </summary>
    public double GetSlantHeight()
    {
        var h = Height;
        var r = Radius;
        return Math.Sqrt(r * r + h * h);
    }
    
    /// <summary>
    /// Check if a point is inside this cone.
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        var h = Height;
        var r = Radius;
        var axis = Axis.GetText();
        
        float spineCoord, perp1, perp2;
        
        // Project point onto cone coordinate system
        switch (axis)
        {
            case "X":
                spineCoord = point.X;
                perp1 = point.Y;
                perp2 = point.Z;
                break;
            case "Y":
                spineCoord = point.Y;
                perp1 = point.X;
                perp2 = point.Z;
                break;
            case "Z":
            default:
                spineCoord = point.Z;
                perp1 = point.X;
                perp2 = point.Y;
                break;
        }
        
        var halfHeight = h * 0.5;
        
        // Check if point is within cone's height bounds
        if (spineCoord < -halfHeight || spineCoord > halfHeight)
            return false;
        
        // Calculate radius at this height
        // At base (spineCoord = -halfHeight): radius = r
        // At apex (spineCoord = halfHeight): radius = 0
        var heightFromBase = spineCoord + halfHeight;
        var radiusAtHeight = r * (1.0 - heightFromBase / h);
        
        var perpDistanceSquared = perp1 * perp1 + perp2 * perp2;
        return perpDistanceSquared <= radiusAtHeight * radiusAtHeight;
    }
    
    #endregion
    
    #region ComputeExtentFromGeometry Override
    
    /// <summary>
    /// Compute extent for this cone from its current geometry.
    /// </summary>
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        var (min, max) = ComputeExtent(Height, Radius, Axis);
        extent.Add(new GfVec3f(min.X, min.Y, min.Z));
        extent.Add(new GfVec3f(max.X, max.Y, max.Z));
        
        return true;
    }
    
    #endregion
    
    #region Creation Helper
    
    /// <summary>
    /// Create a cone primitive with specified parameters.
    /// </summary>
    public static UsdGeomCone CreatePrimitive(UsdStage stage, SdfPath path, double height = 2.0, double radius = 1.0, TfToken? axis = null)
    {
        var cone = Define(stage, path);
        cone.Height = height;
        cone.Radius = radius;
        cone.Axis = axis ?? new TfToken("Z");
        
        return cone;
    }
    
    #endregion
}