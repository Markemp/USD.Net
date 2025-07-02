using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Cube", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomCube : UsdGeomGprim
{
    #region Construction
    
    public UsdGeomCube(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdGeomCube() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Cube");
    protected override TfToken GetTypeName() => new TfToken("Cube");
    
    #endregion
    
    #region Size Attribute
    
    /// <summary>
    /// Get the size attribute. Indicates the length of each edge of the cube.
    /// </summary>
    public UsdAttribute GetSizeAttr()
    {
        return GetAttribute(new TfToken("size"));
    }
    
    /// <summary>
    /// Create the size attribute with default value.
    /// </summary>
    public UsdAttribute CreateSizeAttr()
    {
        return CreateAttribute(
            new TfToken("size"), 
            "double", 
            false, 
            SdfVariability.Varying, 
            new VtValue(2.0));
    }
    
    /// <summary>
    /// Get or set the size (edge length) of the cube.
    /// </summary>
    public double Size
    {
        get
        {
            var attr = GetSizeAttr();
            if (attr.IsValid() && attr.Get(out double value))
                return value;
            return 2.0; // Default value
        }
        set
        {
            var attr = GetSizeAttr();
            if (!attr.IsValid())
            {
                attr = CreateSizeAttr();
            }
            attr.Set(new VtValue(value));
            
            // Auto-update extent when size changes
            UpdateExtentFromSize(value);
        }
    }
    
    #endregion
    
    #region Extent Management
    
    /// <summary>
    /// Update the extent based on the current size.
    /// </summary>
    private void UpdateExtentFromSize(double size)
    {
        var extent = ComputeExtentFromSize(size);
        SetExtent(extent);
    }
    
    /// <summary>
    /// Compute extent for a cube with the given size.
    /// </summary>
    public static List<GfVec3f> ComputeExtentFromSize(double size)
    {
        var half = (float)(size / 2.0);
        var min = new GfVec3f(-half, -half, -half);
        var max = new GfVec3f(half, half, half);
        return new List<GfVec3f> { min, max };
    }
    
    /// <summary>
    /// Compute extent for a cube with the given size and transform.
    /// </summary>
    public static List<GfVec3f> ComputeExtentFromSize(double size, GfMatrix4d transform)
    {
        var baseExtent = ComputeExtentFromSize(size);
        return TransformExtent(baseExtent, transform);
    }
    
    protected override bool ComputeExtentFromGeometry(UsdTimeCode time, out List<GfVec3f> extent)
    {
        var size = Size;
        extent = ComputeExtentFromSize(size);
        return true;
    }
    
    #endregion
    
    #region Cube Creation and Modification
    
    /// <summary>
    /// Create a cube with the specified size.
    /// </summary>
    public void CreateCube(double size = 2.0)
    {
        Size = size;
        
        // Set default display properties for a cube
        SetDisplayColor(UsdGeomGprim.White);
        SetDisplayOpacity(1.0f);
    }
    
    /// <summary>
    /// Set the size and automatically update extent.
    /// </summary>
    public void SetSize(double size, UsdTimeCode time = default)
    {
        var attr = GetSizeAttr();
        if (!attr.IsValid())
        {
            attr = CreateSizeAttr();
        }
        attr.Set(new VtValue(size), time);
        
        // Update extent for the new size
        var extent = ComputeExtentFromSize(size);
        SetExtent(extent, time);
    }
    
    /// <summary>
    /// Get the size at a specific time.
    /// </summary>
    public double GetSize(UsdTimeCode time = default)
    {
        var attr = GetSizeAttr();
        if (attr.IsValid() && attr.Get(out double value, time))
            return value;
        return 2.0; // Default value
    }
    
    #endregion
    
    #region Geometric Properties
    
    /// <summary>
    /// Get the volume of the cube.
    /// </summary>
    public double GetVolume(UsdTimeCode time = default)
    {
        var size = GetSize(time);
        return Math.Pow(size, 3);
    }
    
    /// <summary>
    /// Get the surface area of the cube.
    /// </summary>
    public double GetSurfaceArea(UsdTimeCode time = default)
    {
        var size = GetSize(time);
        return 6.0 * Math.Pow(size, 2);
    }
    
    /// <summary>
    /// Get the edge length of the cube.
    /// </summary>
    public double GetEdgeLength(UsdTimeCode time = default)
    {
        return GetSize(time);
    }
    
    /// <summary>
    /// Get the diagonal length of the cube (space diagonal).
    /// </summary>
    public double GetDiagonalLength(UsdTimeCode time = default)
    {
        var size = GetSize(time);
        return size * Math.Sqrt(3.0);
    }
    
    /// <summary>
    /// Get the face diagonal length of the cube.
    /// </summary>
    public double GetFaceDiagonalLength(UsdTimeCode time = default)
    {
        var size = GetSize(time);
        return size * Math.Sqrt(2.0);
    }
    
    #endregion
    
    #region Point and Distance Operations
    
    /// <summary>
    /// Check if a point is inside the cube.
    /// </summary>
    public bool ContainsPoint(GfVec3f point, UsdTimeCode time = default)
    {
        var half = (float)(GetSize(time) / 2.0);
        return Math.Abs(point.X) <= half && 
               Math.Abs(point.Y) <= half && 
               Math.Abs(point.Z) <= half;
    }
    
    /// <summary>
    /// Get the distance from the cube surface to a point.
    /// Negative values mean the point is inside the cube.
    /// </summary>
    public double GetDistanceToPoint(GfVec3f point, UsdTimeCode time = default)
    {
        var half = (float)(GetSize(time) / 2.0);
        
        // Distance to each face
        var dx = Math.Max(0, Math.Abs(point.X) - half);
        var dy = Math.Max(0, Math.Abs(point.Y) - half);
        var dz = Math.Max(0, Math.Abs(point.Z) - half);
        
        // If point is inside, return negative distance to closest face
        if (dx == 0 && dy == 0 && dz == 0)
        {
            var distToFaceX = half - Math.Abs(point.X);
            var distToFaceY = half - Math.Abs(point.Y);
            var distToFaceZ = half - Math.Abs(point.Z);
            return -Math.Min(Math.Min(distToFaceX, distToFaceY), distToFaceZ);
        }
        
        // Point is outside, return Euclidean distance
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    /// <summary>
    /// Get the closest point on the cube surface to a given point.
    /// </summary>
    public GfVec3f GetClosestPointOnSurface(GfVec3f point, UsdTimeCode time = default)
    {
        var half = (float)(GetSize(time) / 2.0);
        
        var closestX = Math.Max(-half, Math.Min(half, point.X));
        var closestY = Math.Max(-half, Math.Min(half, point.Y));
        var closestZ = Math.Max(-half, Math.Min(half, point.Z));
        
        // If point is inside, project to closest face
        if (Math.Abs(point.X) <= half && Math.Abs(point.Y) <= half && Math.Abs(point.Z) <= half)
        {
            var distToFaceX = half - Math.Abs(point.X);
            var distToFaceY = half - Math.Abs(point.Y);
            var distToFaceZ = half - Math.Abs(point.Z);
            
            if (distToFaceX <= distToFaceY && distToFaceX <= distToFaceZ)
            {
                closestX = point.X >= 0 ? half : -half;
            }
            else if (distToFaceY <= distToFaceZ)
            {
                closestY = point.Y >= 0 ? half : -half;
            }
            else
            {
                closestZ = point.Z >= 0 ? half : -half;
            }
        }
        
        return new GfVec3f(closestX, closestY, closestZ);
    }
    
    #endregion
    
    #region Mesh Generation
    
    /// <summary>
    /// Generate a polygonal mesh representation of the cube.
    /// </summary>
    public UsdGeomMesh GenerateMesh()
    {
        // Create a mesh on the same stage as this cube
        var stage = _prim.GetStage();
        if (stage == null)
            return new UsdGeomMesh();
            
        var meshPath = _prim.GetPath().AppendChild(new TfToken("mesh"));
        var mesh = UsdGeomMesh.Define(stage, meshPath);
        
        GenerateCubeMesh(mesh, GetSize());
        
        return mesh;
    }
    
    /// <summary>
    /// Generate cube mesh data.
    /// </summary>
    private static void GenerateCubeMesh(UsdGeomMesh mesh, double size)
    {
        var half = (float)(size / 2.0);
        
        // 8 cube vertices
        var points = new List<GfVec3f>
        {
            new GfVec3f(-half, -half, -half), // 0
            new GfVec3f( half, -half, -half), // 1  
            new GfVec3f( half,  half, -half), // 2
            new GfVec3f(-half,  half, -half), // 3
            new GfVec3f(-half, -half,  half), // 4
            new GfVec3f( half, -half,  half), // 5
            new GfVec3f( half,  half,  half), // 6
            new GfVec3f(-half,  half,  half)  // 7
        };
        
        // 6 quad faces (front, back, left, right, bottom, top)
        var faceVertexIndices = new List<int>
        {
            // Front face (z = +half)
            4, 5, 6, 7,
            // Back face (z = -half)  
            1, 0, 3, 2,
            // Left face (x = -half)
            0, 4, 7, 3,
            // Right face (x = +half)
            5, 1, 2, 6,
            // Bottom face (y = -half)
            0, 1, 5, 4,
            // Top face (y = +half)
            3, 7, 6, 2
        };
        
        var faceVertexCounts = new List<int> { 4, 4, 4, 4, 4, 4 };
        
        mesh.Points = points;
        mesh.FaceVertexIndices = faceVertexIndices;
        mesh.FaceVertexCounts = faceVertexCounts;
        mesh.SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    #endregion
    
    #region Face and Edge Access
    
    /// <summary>
    /// Get the 8 corner vertices of the cube.
    /// </summary>
    public List<GfVec3f> GetCornerVertices(UsdTimeCode time = default)
    {
        var half = (float)(GetSize(time) / 2.0);
        return new List<GfVec3f>
        {
            new GfVec3f(-half, -half, -half),
            new GfVec3f( half, -half, -half),
            new GfVec3f( half,  half, -half),
            new GfVec3f(-half,  half, -half),
            new GfVec3f(-half, -half,  half),
            new GfVec3f( half, -half,  half),
            new GfVec3f( half,  half,  half),
            new GfVec3f(-half,  half,  half)
        };
    }
    
    /// <summary>
    /// Get the 6 face center points of the cube.
    /// </summary>
    public List<GfVec3f> GetFaceCenters(UsdTimeCode time = default)
    {
        var half = (float)(GetSize(time) / 2.0);
        return new List<GfVec3f>
        {
            new GfVec3f(0, 0, half),    // Front
            new GfVec3f(0, 0, -half),   // Back
            new GfVec3f(-half, 0, 0),   // Left
            new GfVec3f(half, 0, 0),    // Right
            new GfVec3f(0, -half, 0),   // Bottom
            new GfVec3f(0, half, 0)     // Top
        };
    }
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdGeomCube Get(UsdStage stage, SdfPath path)
    {
        return Get<UsdGeomCube>(stage, path);
    }
    
    public new static UsdGeomCube Define(UsdStage stage, SdfPath path)
    {
        return Define<UsdGeomCube>(stage, path);
    }
    
    /// <summary>
    /// Create a cube with specified size.
    /// </summary>
    public static UsdGeomCube Define(UsdStage stage, SdfPath path, double size)
    {
        var cube = Define(stage, path);
        cube.CreateCube(size);
        return cube;
    }
    
    #endregion
}