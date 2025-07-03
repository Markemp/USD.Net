using System;
using System.Collections.Generic;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Defines a primitive cylinder, centered at the origin, whose spine aligns
/// with the specified axis.
/// </summary>
[UsdSchema("Cylinder", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomCylinder : UsdGeomGprim
{
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Cylinder");
    
    #endregion
    
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomCylinder on UsdPrim prim.
    /// </summary>
    public UsdGeomCylinder(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomCylinder on an invalid prim.
    /// </summary>
    public UsdGeomCylinder() : base()
    {
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomCylinder holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomCylinder Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomCylinder(prim);
    }
    
    /// <summary>
    /// Attempt to ensure a UsdPrim adhering to this schema at path is defined
    /// on this stage.
    /// </summary>
    public static new UsdGeomCylinder Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Cylinder");
        return new UsdGeomCylinder(prim);
    }
    
    #endregion
    
    #region Schema Attributes
    
    /// <summary>
    /// The length of the cylinder's spine.
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
    /// The radius of the cylinder.
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
    /// The axis along which the cylinder's spine is aligned.
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
    /// Compute the extent for the cylinder defined by the given height, radius, and axis.
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
    /// Compute the extent for the cylinder defined by the given height, radius, and axis,
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
    /// Get the volume of this cylinder.
    /// </summary>
    public double GetVolume()
    {
        var h = Height;
        var r = Radius;
        
        // Volume = π * r² * h
        return Math.PI * r * r * h;
    }
    
    /// <summary>
    /// Get the surface area of this cylinder (including caps).
    /// </summary>
    public double GetSurfaceArea()
    {
        var h = Height;
        var r = Radius;
        
        // Surface area = 2 * cap area + lateral area
        // Cap area: π * r²
        // Lateral area: 2 * π * r * h
        var capArea = 2.0 * Math.PI * r * r;
        var lateralArea = 2.0 * Math.PI * r * h;
        
        return capArea + lateralArea;
    }
    
    /// <summary>
    /// Get the lateral surface area of this cylinder (excluding caps).
    /// </summary>
    public double GetLateralSurfaceArea()
    {
        var h = Height;
        var r = Radius;
        
        // Lateral area: 2 * π * r * h
        return 2.0 * Math.PI * r * h;
    }
    
    /// <summary>
    /// Check if a point is inside this cylinder.
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        var h = Height;
        var r = Radius;
        var axis = Axis.GetText();
        
        float spineCoord, perp1, perp2;
        
        // Project point onto cylinder coordinate system
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
        
        // Check if point is within cylinder's height bounds
        if (Math.Abs(spineCoord) > halfHeight)
            return false;
        
        // Check if point is within cylinder's radius
        var perpDistanceSquared = perp1 * perp1 + perp2 * perp2;
        return perpDistanceSquared <= r * r;
    }
    
    #endregion
    
    #region ComputeExtentFromGeometry Override
    
    /// <summary>
    /// Compute extent for this cylinder from its current geometry.
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
    /// Create a cylinder primitive with specified parameters.
    /// </summary>
    public static UsdGeomCylinder CreatePrimitive(UsdStage stage, SdfPath path, double height = 2.0, double radius = 1.0, TfToken? axis = null)
    {
        var cylinder = Define(stage, path);
        cylinder.Height = height;
        cylinder.Radius = radius;
        cylinder.Axis = axis ?? new TfToken("Z");
        
        return cylinder;
    }
    
    #endregion
}