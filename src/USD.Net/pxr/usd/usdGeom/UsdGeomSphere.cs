using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Sphere", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomSphere : UsdGeomGprim
{
    #region Construction
    
    public UsdGeomSphere(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdGeomSphere() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Sphere");
    protected override TfToken GetTypeName() => new TfToken("Sphere");
    
    #endregion
    
    #region Radius Attribute
    
    /// <summary>
    /// Get the radius attribute. Indicates the sphere's radius.
    /// </summary>
    public UsdAttribute GetRadiusAttr()
    {
        return GetAttribute(new TfToken("radius"));
    }
    
    /// <summary>
    /// Create the radius attribute with default value.
    /// </summary>
    public UsdAttribute CreateRadiusAttr()
    {
        return CreateAttribute(
            new TfToken("radius"), 
            "double", 
            false, 
            SdfVariability.Varying, 
            new VtValue(1.0));
    }
    
    /// <summary>
    /// Get or set the radius of the sphere.
    /// </summary>
    public double Radius
    {
        get
        {
            var attr = GetRadiusAttr();
            if (attr.IsValid() && attr.Get(out double value))
                return value;
            return 1.0; // Default value
        }
        set
        {
            var attr = GetRadiusAttr();
            if (!attr.IsValid())
            {
                attr = CreateRadiusAttr();
            }
            attr.Set(new VtValue(value));
            
            // Auto-update extent when radius changes
            UpdateExtentFromRadius(value);
        }
    }
    
    #endregion
    
    #region Extent Management
    
    /// <summary>
    /// Update the extent based on the current radius.
    /// </summary>
    private void UpdateExtentFromRadius(double radius)
    {
        var extent = ComputeExtentFromRadius(radius);
        SetExtent(extent);
    }
    
    /// <summary>
    /// Compute extent for a sphere with the given radius.
    /// </summary>
    public static List<GfVec3f> ComputeExtentFromRadius(double radius)
    {
        var r = (float)radius;
        var min = new GfVec3f(-r, -r, -r);
        var max = new GfVec3f(r, r, r);
        return new List<GfVec3f> { min, max };
    }
    
    /// <summary>
    /// Compute extent for a sphere with the given radius and transform.
    /// </summary>
    public static List<GfVec3f> ComputeExtentFromRadius(double radius, GfMatrix4d transform)
    {
        var baseExtent = ComputeExtentFromRadius(radius);
        return TransformExtent(baseExtent, transform);
    }
    
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        var radius = GetRadius(time);
        extent = ComputeExtentFromRadius(radius);
        return true;
    }
    
    #endregion
    
    #region Sphere Creation and Modification
    
    /// <summary>
    /// Create a sphere with the specified radius.
    /// </summary>
    public void CreateSphere(double radius = 1.0)
    {
        Radius = radius;
        
        // Set default display properties for a sphere
        SetDisplayColor(UsdGeomGprim.White);
        SetDisplayOpacity(1.0f);
    }
    
    /// <summary>
    /// Set the radius and automatically update extent.
    /// </summary>
    public void SetRadius(double radius, UsdTimeCode time = default)
    {
        var attr = GetRadiusAttr();
        if (!attr.IsValid())
        {
            attr = CreateRadiusAttr();
        }
        attr.Set(new VtValue(radius), time);
        
        // Update extent for the new radius
        var extent = ComputeExtentFromRadius(radius);
        SetExtent(extent, time);
    }
    
    /// <summary>
    /// Get the radius at a specific time.
    /// </summary>
    public double GetRadius(UsdTimeCode time = default)
    {
        var attr = GetRadiusAttr();
        if (attr.IsValid())
        {
            // Try to get value at specific time first
            if (time != UsdTimeCode.Default() && attr.Get(out double timeValue, time))
                return timeValue;
            // Fall back to default value
            if (attr.Get(out double value))
                return value;
        }
        return 1.0; // Default value
    }
    
    #endregion
    
    #region Geometric Properties
    
    /// <summary>
    /// Get the volume of the sphere.
    /// </summary>
    public double GetVolume(UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        return (4.0 / 3.0) * Math.PI * Math.Pow(radius, 3);
    }
    
    /// <summary>
    /// Get the surface area of the sphere.
    /// </summary>
    public double GetSurfaceArea(UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        return 4.0 * Math.PI * Math.Pow(radius, 2);
    }
    
    /// <summary>
    /// Get the diameter of the sphere.
    /// </summary>
    public double GetDiameter(UsdTimeCode time = default)
    {
        return GetRadius(time) * 2.0;
    }
    
    /// <summary>
    /// Get the circumference of the sphere (great circle).
    /// </summary>
    public double GetCircumference(UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        return 2.0 * Math.PI * radius;
    }
    
    #endregion
    
    #region Point and Distance Operations
    
    /// <summary>
    /// Check if a point is inside the sphere.
    /// </summary>
    public bool ContainsPoint(GfVec3f point, UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        var distanceSquared = point.X * point.X + point.Y * point.Y + point.Z * point.Z;
        return distanceSquared <= radius * radius;
    }
    
    /// <summary>
    /// Get the distance from the sphere surface to a point.
    /// Negative values mean the point is inside the sphere.
    /// </summary>
    public double GetDistanceToPoint(GfVec3f point, UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        var distance = Math.Sqrt(point.X * point.X + point.Y * point.Y + point.Z * point.Z);
        return distance - radius;
    }
    
    /// <summary>
    /// Get the closest point on the sphere surface to a given point.
    /// </summary>
    public GfVec3f GetClosestPointOnSurface(GfVec3f point, UsdTimeCode time = default)
    {
        var radius = GetRadius(time);
        var distance = Math.Sqrt(point.X * point.X + point.Y * point.Y + point.Z * point.Z);
        
        if (distance == 0)
        {
            // Point is at origin, return any point on surface
            return new GfVec3f((float)radius, 0, 0);
        }
        
        var scale = (float)(radius / distance);
        return new GfVec3f(point.X * scale, point.Y * scale, point.Z * scale);
    }
    
    #endregion
    
    #region Mesh Generation
    
    /// <summary>
    /// Generate a polygonal mesh representation of the sphere.
    /// </summary>
    public UsdGeomMesh GenerateMesh(int subdivisions = 2)
    {
        // Create a mesh on the same stage as this sphere
        var stage = _prim.GetStage();
        if (stage == null)
            return new UsdGeomMesh();
            
        var meshPath = _prim.GetPath().AppendChild(new TfToken("mesh"));
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        GenerateSphereMesh(mesh, GetRadius(), subdivisions);
        
        return mesh;
    }
    
    /// <summary>
    /// Generate sphere mesh data using icosphere subdivision.
    /// </summary>
    private static void GenerateSphereMesh(UsdGeomMesh mesh, double radius, int subdivisions)
    {
        var points = new List<GfVec3f>();
        var indices = new List<int>();
        var counts = new List<int>();
        
        // Start with icosahedron
        var t = (float)((1.0 + Math.Sqrt(5.0)) / 2.0); // Golden ratio
        var r = (float)radius;
        
        // 12 vertices of icosahedron
        var icosahedronVertices = new[]
        {
            new GfVec3f(-1, t, 0), new GfVec3f(1, t, 0), new GfVec3f(-1, -t, 0), new GfVec3f(1, -t, 0),
            new GfVec3f(0, -1, t), new GfVec3f(0, 1, t), new GfVec3f(0, -1, -t), new GfVec3f(0, 1, -t),
            new GfVec3f(t, 0, -1), new GfVec3f(t, 0, 1), new GfVec3f(-t, 0, -1), new GfVec3f(-t, 0, 1)
        };
        
        // Normalize and scale vertices to sphere surface
        foreach (var vertex in icosahedronVertices)
        {
            var length = (float)Math.Sqrt(vertex.X * vertex.X + vertex.Y * vertex.Y + vertex.Z * vertex.Z);
            var normalized = new GfVec3f(vertex.X / length * r, vertex.Y / length * r, vertex.Z / length * r);
            points.Add(normalized);
        }
        
        // 20 triangular faces of icosahedron
        var triangles = new[]
        {
            new[] {0, 11, 5}, new[] {0, 5, 1}, new[] {0, 1, 7}, new[] {0, 7, 10}, new[] {0, 10, 11},
            new[] {1, 5, 9}, new[] {5, 11, 4}, new[] {11, 10, 2}, new[] {10, 7, 6}, new[] {7, 1, 8},
            new[] {3, 9, 4}, new[] {3, 4, 2}, new[] {3, 2, 6}, new[] {3, 6, 8}, new[] {3, 8, 9},
            new[] {4, 9, 5}, new[] {2, 4, 11}, new[] {6, 2, 10}, new[] {8, 6, 7}, new[] {9, 8, 1}
        };
        
        // For simplicity, just use the icosahedron (subdivisions would require edge subdivision)
        foreach (var triangle in triangles)
        {
            indices.AddRange(triangle);
            counts.Add(3);
        }
        
        mesh.Points = points;
        mesh.FaceVertexIndices = indices;
        mesh.FaceVertexCounts = counts;
        mesh.SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdGeomSphere Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdGeomSphere>(stage, path);
    }
    
    public new static UsdGeomSphere Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdGeomSphere>(stage, path);
    }
    
    /// <summary>
    /// Create a sphere with specified radius.
    /// </summary>
    public static UsdGeomSphere Define(UsdStage stage, SdfPath path, double radius)
    {
        var sphere = Define(stage, path);
        sphere.CreateSphere(radius);
        return sphere;
    }
    
    #endregion
}