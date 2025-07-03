using System;
using System.Collections.Generic;
using System.Numerics;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Defines a primitive plane, centered at the origin, and oriented perpendicular
/// to the specified axis. The plane extends infinitely in all directions along
/// its surface, but for rendering and bounds computation, it has finite width
/// and length.
/// </summary>
[UsdSchema("Plane", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomPlane : UsdGeomGprim
{
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Plane");
    
    #endregion
    
    #region Construction
    
    /// <summary>
    /// Construct a UsdGeomPlane on UsdPrim prim.
    /// </summary>
    public UsdGeomPlane(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct a UsdGeomPlane on an invalid prim.
    /// </summary>
    public UsdGeomPlane() : base()
    {
    }
    
    #endregion
    
    #region Factory Methods
    
    /// <summary>
    /// Return a UsdGeomPlane holding the prim adhering to this schema at path on stage.
    /// </summary>
    public static UsdGeomPlane Get(UsdStage stage, SdfPath path)
    {
        var prim = stage.GetPrimAtPath(path);
        return new UsdGeomPlane(prim);
    }
    
    /// <summary>
    /// Attempt to ensure a UsdPrim adhering to this schema at path is defined
    /// on this stage.
    /// </summary>
    public static new UsdGeomPlane Define(UsdStage stage, SdfPath path)
    {
        var prim = stage.DefinePrim(path, "Plane");
        return new UsdGeomPlane(prim);
    }
    
    #endregion
    
    #region Schema Attributes
    
    /// <summary>
    /// The width of the plane. When axis is "Z", width represents the size
    /// along the X-axis.
    /// </summary>
    public double Width
    {
        get
        {
            var attr = GetWidthAttr();
            if (attr.Get(out double value))
                return value;
            return 2.0; // Default value
        }
        set
        {
            var attr = CreateWidthAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when width changes
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// The length of the plane. When axis is "Z", length represents the size
    /// along the Y-axis.
    /// </summary>
    public double Length
    {
        get
        {
            var attr = GetLengthAttr();
            if (attr.Get(out double value))
                return value;
            return 2.0; // Default value
        }
        set
        {
            var attr = CreateLengthAttr();
            attr.Set(new VtValue(value));
            
            // Update extent when length changes
            UpdateExtentFromGeometry();
        }
    }
    
    /// <summary>
    /// The axis to which the plane's normal is aligned.
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
    /// Create the width attribute.
    /// </summary>
    public UsdAttribute CreateWidthAttr()
    {
        return CreateAttribute(new TfToken("width"), "double");
    }
    
    /// <summary>
    /// Get the width attribute.
    /// </summary>
    public UsdAttribute GetWidthAttr()
    {
        return GetAttribute(new TfToken("width"));
    }
    
    /// <summary>
    /// Create the length attribute.
    /// </summary>
    public UsdAttribute CreateLengthAttr()
    {
        return CreateAttribute(new TfToken("length"), "double");
    }
    
    /// <summary>
    /// Get the length attribute.
    /// </summary>
    public UsdAttribute GetLengthAttr()
    {
        return GetAttribute(new TfToken("length"));
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
    /// Compute the extent for the plane defined by the given width, length, and axis.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(double width, double length, TfToken axis)
    {
        var halfWidth = (float)(width * 0.5);
        var halfLength = (float)(length * 0.5);
        
        var min = Vector3.Zero;
        var max = Vector3.Zero;
        
        switch (axis.GetText())
        {
            case "X":
                // Plane in yz-plane, width=z-axis, length=y-axis
                min = new Vector3(0, -halfLength, -halfWidth);
                max = new Vector3(0, halfLength, halfWidth);
                break;
            case "Y":
                // Plane in xz-plane, width=x-axis, length=z-axis
                min = new Vector3(-halfWidth, 0, -halfLength);
                max = new Vector3(halfWidth, 0, halfLength);
                break;
            case "Z":
            default:
                // Plane in xy-plane, width=x-axis, length=y-axis
                min = new Vector3(-halfWidth, -halfLength, 0);
                max = new Vector3(halfWidth, halfLength, 0);
                break;
        }
        
        return (min, max);
    }
    
    /// <summary>
    /// Compute the extent for the plane defined by the given width, length, and axis,
    /// transformed by the given matrix.
    /// </summary>
    public static (Vector3 min, Vector3 max) ComputeExtent(double width, double length, TfToken axis, Matrix4x4 transform)
    {
        var (localMin, localMax) = ComputeExtent(width, length, axis);
        
        // For a plane, we need to consider the 4 corners
        var corners = new List<Vector3>();
        
        switch (axis.GetText())
        {
            case "X":
                corners.AddRange(new[]
                {
                    new Vector3(0, localMin.Y, localMin.Z),
                    new Vector3(0, localMax.Y, localMin.Z),
                    new Vector3(0, localMin.Y, localMax.Z),
                    new Vector3(0, localMax.Y, localMax.Z)
                });
                break;
            case "Y":
                corners.AddRange(new[]
                {
                    new Vector3(localMin.X, 0, localMin.Z),
                    new Vector3(localMax.X, 0, localMin.Z),
                    new Vector3(localMin.X, 0, localMax.Z),
                    new Vector3(localMax.X, 0, localMax.Z)
                });
                break;
            case "Z":
            default:
                corners.AddRange(new[]
                {
                    new Vector3(localMin.X, localMin.Y, 0),
                    new Vector3(localMax.X, localMin.Y, 0),
                    new Vector3(localMin.X, localMax.Y, 0),
                    new Vector3(localMax.X, localMax.Y, 0)
                });
                break;
        }
        
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
    /// Get the area of this plane.
    /// </summary>
    public double GetArea()
    {
        return Width * Length;
    }
    
    /// <summary>
    /// Get the perimeter of this plane.
    /// </summary>
    public double GetPerimeter()
    {
        return 2.0 * (Width + Length);
    }
    
    /// <summary>
    /// Get the normal vector of this plane in world space.
    /// </summary>
    public Vector3 GetNormal()
    {
        switch (Axis.GetText())
        {
            case "X":
                return Vector3.UnitX;
            case "Y":
                return Vector3.UnitY;
            case "Z":
            default:
                return Vector3.UnitZ;
        }
    }
    
    /// <summary>
    /// Get the distance from a point to this plane.
    /// </summary>
    public float GetDistanceToPoint(Vector3 point)
    {
        var normal = GetNormal();
        
        // Distance = |normal · point - d| where d = 0 (plane passes through origin)
        return Math.Abs(Vector3.Dot(normal, point));
    }
    
    /// <summary>
    /// Check if a point is on this plane (within the finite bounds).
    /// </summary>
    public bool ContainsPoint(Vector3 point, float tolerance = 1e-6f)
    {
        var axis = Axis.GetText();
        var w = Width;
        var l = Length;
        
        // First check if point is on the plane (within tolerance)
        var distToPlane = GetDistanceToPoint(point);
        if (distToPlane > tolerance)
            return false;
        
        // Then check if point is within bounds
        switch (axis)
        {
            case "X":
                return Math.Abs(point.Y) <= l * 0.5 && Math.Abs(point.Z) <= w * 0.5;
            case "Y":
                return Math.Abs(point.X) <= w * 0.5 && Math.Abs(point.Z) <= l * 0.5;
            case "Z":
            default:
                return Math.Abs(point.X) <= w * 0.5 && Math.Abs(point.Y) <= l * 0.5;
        }
    }
    
    /// <summary>
    /// Project a point onto this plane.
    /// </summary>
    public Vector3 ProjectPoint(Vector3 point)
    {
        var normal = GetNormal();
        var distanceToPlane = Vector3.Dot(normal, point);
        
        // Project point onto plane by moving along normal
        return point - distanceToPlane * normal;
    }
    
    #endregion
    
    #region ComputeExtentFromGeometry Override
    
    /// <summary>
    /// Compute extent for this plane from its current geometry.
    /// </summary>
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        extent = new List<GfVec3f>();
        
        var (min, max) = ComputeExtent(Width, Length, Axis);
        extent.Add(new GfVec3f(min.X, min.Y, min.Z));
        extent.Add(new GfVec3f(max.X, max.Y, max.Z));
        
        return true;
    }
    
    #endregion
    
    #region Creation Helper
    
    /// <summary>
    /// Create a plane primitive with specified parameters.
    /// </summary>
    public static UsdGeomPlane CreatePrimitive(UsdStage stage, SdfPath path, double width = 2.0, double length = 2.0, TfToken? axis = null, bool doubleSided = true)
    {
        var plane = Define(stage, path);
        plane.Width = width;
        plane.Length = length;
        plane.Axis = axis ?? new TfToken("Z");
        plane.DoubleSided = doubleSided;
        
        return plane;
    }
    
    #endregion
}