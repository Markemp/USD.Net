using System;
using System.Collections.Generic;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Defines a primitive capsule, also known as a "pill" shape, 
/// centered at the origin, whose spine aligns with the specified axis.
/// 
/// A capsule is a cylinder with hemispherical caps. The total length
/// is height + 2*radius, where height is the cylindrical portion only.
/// </summary>
[UsdSchema("Capsule", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomCapsule : UsdGeomGprim
{
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Capsule");
    
    #endregion
    
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomCapsule on UsdPrim prim.
    /// </summary>
    public UsdGeomCapsule(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomCapsule on an invalid prim.
    /// </summary>
    public UsdGeomCapsule() : base()
    {
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomCapsule holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomCapsule Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomCapsule(prim);
    }
    
    /// <summary>
    /// Attempt to ensure a UsdPrim adhering to this schema at path is defined
    /// on this stage.
    /// </summary>
    public static new UsdGeomCapsule Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Capsule");
        return new UsdGeomCapsule(prim);
    }
    
    #endregion
    
    #region Schema Attributes
    
    /// <summary>
    /// The length of the cylindrical portion of the capsule, i.e. the capsule's
    /// total length minus the contributions of the two hemispherical end caps.
    /// If height is 0, the capsule becomes a sphere.
    /// </summary>
    public double Height
    {
        get
        {
            var attr = GetHeightAttr();
            if (attr.Get(out double value))
                return value;
            return 1.0; // Default value
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
    /// The radius of the capsule.
    /// </summary>
    public double Radius
    {
        get
        {
            var attr = GetRadiusAttr();
            if (attr.Get(out double value))
                return value;
            return 0.5; // Default value
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
    /// The axis along which the spine of the capsule is aligned.
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
    /// Compute the extent for the capsule defined by the given height, radius, and axis.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(double height, double radius, TfToken axis)
    {
        // Capsule's total extent includes hemisphere caps
        var halfHeightWithCap = (float)(height * 0.5 + radius);
        var radiusFloat = (float)radius;
        
        var min = Vector3.Zero;
        var max = Vector3.Zero;
        
        switch (axis.GetText())
        {
            case "X":
                min = new Vector3(-halfHeightWithCap, -radiusFloat, -radiusFloat);
                max = new Vector3(halfHeightWithCap, radiusFloat, radiusFloat);
                break;
            case "Y":
                min = new Vector3(-radiusFloat, -halfHeightWithCap, -radiusFloat);
                max = new Vector3(radiusFloat, halfHeightWithCap, radiusFloat);
                break;
            case "Z":
            default:
                min = new Vector3(-radiusFloat, -radiusFloat, -halfHeightWithCap);
                max = new Vector3(radiusFloat, radiusFloat, halfHeightWithCap);
                break;
        }
        
        return (min, max);
    }
    
    /// <summary>
    /// Compute the extent for the capsule defined by the given height, radius, and axis,
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
    /// Get the volume of this capsule.
    /// </summary>
    public double GetVolume()
    {
        var h = Height;
        var r = Radius;
        
        // Volume = cylinder volume + sphere volume
        // Cylinder: π * r² * h
        // Sphere: (4/3) * π * r³
        var cylinderVolume = Math.PI * r * r * h;
        var sphereVolume = (4.0 / 3.0) * Math.PI * r * r * r;
        
        return cylinderVolume + sphereVolume;
    }
    
    /// <summary>
    /// Get the surface area of this capsule.
    /// </summary>
    public double GetSurfaceArea()
    {
        var h = Height;
        var r = Radius;
        
        // Surface area = cylinder side area + sphere surface area
        // Cylinder side: 2 * π * r * h
        // Sphere: 4 * π * r²
        var cylinderSideArea = 2.0 * Math.PI * r * h;
        var sphereArea = 4.0 * Math.PI * r * r;
        
        return cylinderSideArea + sphereArea;
    }
    
    /// <summary>
    /// Get the total length of this capsule (height + 2*radius).
    /// </summary>
    public double GetTotalLength()
    {
        return Height + 2.0 * Radius;
    }
    
    /// <summary>
    /// Check if a point is inside this capsule.
    /// </summary>
    public bool ContainsPoint(Vector3 point)
    {
        var h = Height;
        var r = Radius;
        var axis = Axis.GetText();
        
        float spineCoord, perp1, perp2;
        
        // Project point onto capsule coordinate system
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
        var perpDistanceSquared = perp1 * perp1 + perp2 * perp2;
        
        // Check cylindrical portion
        if (Math.Abs(spineCoord) <= halfHeight)
        {
            return perpDistanceSquared <= r * r;
        }
        
        // Check hemispherical caps
        var capCenter = spineCoord > 0 ? halfHeight : -halfHeight;
        var distToCapCenter = spineCoord - capCenter;
        var totalDistanceSquared = perpDistanceSquared + distToCapCenter * distToCapCenter;
        
        return totalDistanceSquared <= r * r;
    }
    
    #endregion
    
    #region ComputeExtentFromGeometry Override
    
    /// <summary>
    /// Compute extent for this capsule from its current geometry.
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
    /// Create a capsule primitive with specified parameters.
    /// </summary>
    public static UsdGeomCapsule CreatePrimitive(UsdStage stage, SdfPath path, double height = 1.0, double radius = 0.5, TfToken? axis = null)
    {
        var capsule = Define(stage, path);
        capsule.Height = height;
        capsule.Radius = radius;
        capsule.Axis = axis ?? new TfToken("Z");
        
        return capsule;
    }
    
    #endregion
}