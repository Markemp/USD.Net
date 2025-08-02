using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd.UsdGeom;

[UsdSchema("Mesh", UsdSchemaKind.ConcreteTyped)]
public class UsdGeomMesh : UsdGeomPointBased
{
    #region Construction
    
    public UsdGeomMesh(UsdPrim prim) : base(prim)
    {
    }
    
    public UsdGeomMesh() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
    protected override TfToken GetSchemaTypeName() => new TfToken("Mesh");
    protected override TfToken GetTypeName() => new TfToken("Mesh");
    
    #endregion
    
    #region Core Mesh Attributes
    
    
    /// <summary>
    /// Get the face vertex indices attribute. Flat list of vertex indices
    /// that define each face of the mesh.
    /// </summary>
    public UsdAttribute GetFaceVertexIndicesAttr()
    {
        return GetAttribute(new TfToken("faceVertexIndices"));
    }
    
    /// <summary>
    /// Create the face vertex indices attribute.
    /// </summary>
    public UsdAttribute CreateFaceVertexIndicesAttr()
    {
        return CreateAttribute(
            new TfToken("faceVertexIndices"), 
            "int[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get or set the face vertex indices.
    /// </summary>
    public List<int> FaceVertexIndices
    {
        get
        {
            var attr = GetFaceVertexIndicesAttr();
            if (attr.IsValid() && attr.Get(out List<int> value))
                return value;
            return new List<int>();
        }
        set
        {
            var attr = CreateFaceVertexIndicesAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    /// <summary>
    /// Get the face vertex counts attribute. Number of vertices in each face.
    /// </summary>
    public UsdAttribute GetFaceVertexCountsAttr()
    {
        return GetAttribute(new TfToken("faceVertexCounts"));
    }
    
    /// <summary>
    /// Create the face vertex counts attribute.
    /// </summary>
    public UsdAttribute CreateFaceVertexCountsAttr()
    {
        return CreateAttribute(
            new TfToken("faceVertexCounts"), 
            "int[]", 
            false, 
            SdfVariability.Varying);
    }
    
    /// <summary>
    /// Get or set the face vertex counts.
    /// </summary>
    public List<int> FaceVertexCounts
    {
        get
        {
            var attr = GetFaceVertexCountsAttr();
            if (attr.IsValid() && attr.Get(out List<int> value))
                return value;
            return new List<int>();
        }
        set
        {
            var attr = CreateFaceVertexCountsAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Subdivision Attributes
    
    /// <summary>
    /// Get the subdivision scheme attribute.
    /// </summary>
    public UsdAttribute GetSubdivisionSchemeAttr()
    {
        return GetAttribute(new TfToken("subdivisionScheme"));
    }
    
    /// <summary>
    /// Create the subdivision scheme attribute.
    /// </summary>
    public UsdAttribute CreateSubdivisionSchemeAttr()
    {
        return CreateAttribute(
            new TfToken("subdivisionScheme"), 
            "token", 
            false, 
            SdfVariability.Uniform, 
            new VtValue("catmullClark"));
    }
    
    /// <summary>
    /// Get or set the subdivision scheme.
    /// Valid values: "catmullClark", "loop", "bilinear", "none"
    /// </summary>
    public string SubdivisionScheme
    {
        get
        {
            var attr = GetSubdivisionSchemeAttr();
            if (attr.IsValid() && attr.Get(out string value))
                return value;
            return "catmullClark"; // Default value
        }
        set
        {
            var attr = CreateSubdivisionSchemeAttr();
            attr.Set(new VtValue(value));
        }
    }
    
    #endregion
    
    #region Mesh Topology Methods
    
    /// <summary>
    /// Validate the mesh topology for consistency.
    /// </summary>
    public (bool IsValid, string Reason) ValidateTopology()
    {
        var points = Points;
        var faceVertexIndices = FaceVertexIndices;
        var faceVertexCounts = FaceVertexCounts;
        
        // Check that we have points
        if (points.Count == 0)
            return (false, "Mesh has no points");
            
        // Check that we have faces
        if (faceVertexCounts.Count == 0)
            return (false, "Mesh has no faces (empty faceVertexCounts)");
            
        // Check that sum of face vertex counts equals face vertex indices length
        var expectedIndicesCount = faceVertexCounts.Sum();
        if (expectedIndicesCount != faceVertexIndices.Count)
            return (false, $"Sum of faceVertexCounts ({expectedIndicesCount}) != length of faceVertexIndices ({faceVertexIndices.Count})");
            
        // Check that all face vertex indices are valid
        var numPoints = points.Count;
        foreach (var index in faceVertexIndices)
        {
            if (index < 0 || index >= numPoints)
                return (false, $"Face vertex index {index} is out of range [0, {numPoints - 1}]");
        }
        
        // Check that all faces have at least 3 vertices
        foreach (var count in faceVertexCounts)
        {
            if (count < 3)
                return (false, $"Face has only {count} vertices (minimum is 3)");
        }
        
        return (true, string.Empty);
    }
    
    /// <summary>
    /// Get the number of faces in the mesh.
    /// </summary>
    public int GetFaceCount(UsdTimeCode time = default)
    {
        var faceVertexCounts = FaceVertexCounts;
        return faceVertexCounts.Count;
    }
    
    /// <summary>
    /// Get the number of vertices in the mesh.
    /// </summary>
    public int GetVertexCount(UsdTimeCode time = default)
    {
        var points = Points;
        return points.Count;
    }
    
    #endregion
    
    #region Mesh Creation Helpers
    
    /// <summary>
    /// Create a simple triangle mesh.
    /// </summary>
    public void CreateTriangle(GfVec3f p0, GfVec3f p1, GfVec3f p2)
    {
        Points = new List<GfVec3f> { p0, p1, p2 };
        FaceVertexIndices = new List<int> { 0, 1, 2 };
        FaceVertexCounts = new List<int> { 3 };
        SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    /// <summary>
    /// Create a simple quad mesh.
    /// </summary>
    public void CreateQuad(GfVec3f p0, GfVec3f p1, GfVec3f p2, GfVec3f p3)
    {
        Points = new List<GfVec3f> { p0, p1, p2, p3 };
        FaceVertexIndices = new List<int> { 0, 1, 2, 3 };
        FaceVertexCounts = new List<int> { 4 };
        SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    /// <summary>
    /// Create a cube mesh centered at origin with the given size.
    /// </summary>
    public void CreateCube(float size = 1.0f)
    {
        var half = size / 2.0f;
        
        // 8 cube vertices
        Points = new List<GfVec3f>
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
        FaceVertexIndices = new List<int>
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
        
        FaceVertexCounts = new List<int> { 4, 4, 4, 4, 4, 4 };
        SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    /// <summary>
    /// Create a plane mesh in the XY plane.
    /// </summary>
    public void CreatePlane(float width = 1.0f, float height = 1.0f, int widthSegments = 1, int heightSegments = 1)
    {
        var points = new List<GfVec3f>();
        var faceVertexIndices = new List<int>();
        var faceVertexCounts = new List<int>();
        
        var halfWidth = width / 2.0f;
        var halfHeight = height / 2.0f;
        
        // Generate vertices
        for (int j = 0; j <= heightSegments; j++)
        {
            for (int i = 0; i <= widthSegments; i++)
            {
                var x = (i / (float)widthSegments - 0.5f) * width;
                var y = (j / (float)heightSegments - 0.5f) * height;
                points.Add(new GfVec3f(x, y, 0));
            }
        }
        
        // Generate faces
        for (int j = 0; j < heightSegments; j++)
        {
            for (int i = 0; i < widthSegments; i++)
            {
                var v0 = j * (widthSegments + 1) + i;
                var v1 = v0 + 1;
                var v2 = (j + 1) * (widthSegments + 1) + i + 1;
                var v3 = v2 - 1;
                
                faceVertexIndices.AddRange(new[] { v0, v1, v2, v3 });
                faceVertexCounts.Add(4);
            }
        }
        
        Points = points;
        FaceVertexIndices = faceVertexIndices;
        FaceVertexCounts = faceVertexCounts;
        SubdivisionScheme = "none"; // Polygonal mesh
    }
    
    #endregion
    
    #region Material Binding
    
    // TODO: Implement material binding once UsdShadeMaterial and UsdShadeMaterialBindingAPI are available
    // /// <summary>
    // /// Bind a material to this mesh.
    // /// </summary>
    // public bool BindMaterial(UsdShadeMaterial material, TfToken? purpose = null)
    // {
    //     if (!IsValid || !material.IsValid)
    //         return false;
    //         
    //     var materialBindingAPI = UsdShadeMaterialBindingAPI.Apply(Prim);
    //     return materialBindingAPI.Bind(material, UsdShadeTokens.AllPurpose, purpose?.GetText());
    // }
    
    #endregion
    
    #region UV Sets (Texture Coordinates)
    
    /// <summary>
    /// Create or get a UV set (texture coordinates) primvar.
    /// </summary>
    public UsdGeomPrimvar CreateUVSet(string uvSetName = "st", UsdGeomInterpolation interpolation = UsdGeomInterpolation.FaceVarying)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.CreatePrimvar(new TfToken(uvSetName), "texCoord2f", interpolation);
        
        // Set as texture coordinate
        primvar.SetElementSize(1);
        return primvar;
    }
    
    /// <summary>
    /// Get an existing UV set primvar.
    /// </summary>
    public UsdGeomPrimvar? GetUVSetPrimvar(string uvSetName = "st")
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.GetPrimvar(new TfToken(uvSetName));
        return primvar.IsDefined() ? primvar : null;
    }
    
    /// <summary>
    /// Get an existing UV set primvar (alias for compatibility).
    /// </summary>
    public UsdGeomPrimvar? GetUVSet(string uvSetName = "st")
    {
        return GetUVSetPrimvar(uvSetName);
    }
    
    /// <summary>
    /// Set UV coordinates for a UV set.
    /// </summary>
    public bool SetUVCoordinates(List<GfVec2f> uvCoords, string uvSetName = "st", UsdTimeCode time = default)
    {
        var uvSet = GetUVSetPrimvar(uvSetName) ?? CreateUVSet(uvSetName);
        return uvSet.Set(uvCoords, time);
    }
    
    /// <summary>
    /// Get UV coordinates from a UV set.
    /// </summary>
    public List<GfVec2f> GetUVCoordinates(string uvSetName = "st", UsdTimeCode time = default)
    {
        var uvSet = GetUVSetPrimvar(uvSetName);
        if (uvSet?.Get(out List<GfVec2f> uvCoords, time) == true)
            return uvCoords;
        return new List<GfVec2f>();
    }
    
    /// <summary>
    /// Create multiple UV sets for game assets (diffuse, normal, specular, etc.).
    /// </summary>
    public void CreateGameUVSets(Dictionary<string, List<GfVec2f>> uvSets, UsdTimeCode time = default)
    {
        foreach (var kvp in uvSets)
        {
            SetUVCoordinates(kvp.Value, kvp.Key, time);
        }
    }
    
    /// <summary>
    /// Get all UV set names on this mesh.
    /// </summary>
    public List<string> GetUVSetNames()
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvars = primvarsAPI.GetPrimvars();
        
        return primvars
            .Where(pv => pv.GetTypeName().Contains("texCoord"))
            .Select(pv => pv.GetPrimvarName().GetText())
            .ToList();
    }
    
    #endregion
    
    #region Vertex Colors
    
    /// <summary>
    /// Create or get a vertex color primvar.
    /// </summary>
    public UsdGeomPrimvar CreateVertexColors(string colorSetName = "displayColor", UsdGeomInterpolation interpolation = UsdGeomInterpolation.Vertex)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.CreatePrimvar(new TfToken(colorSetName), "color3f", interpolation);
        
        // Set as color data
        primvar.SetElementSize(1);
        return primvar;
    }
    
    /// <summary>
    /// Get an existing vertex color primvar.
    /// </summary>
    public UsdGeomPrimvar? GetVertexColorPrimvar(string colorSetName = "displayColor")
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.GetPrimvar(new TfToken(colorSetName));
        return primvar.IsDefined() ? primvar : null;
    }
    
    /// <summary>
    /// Set vertex colors.
    /// </summary>
    public bool SetVertexColors(List<GfVec3f> colors, string colorSetName = "displayColor", UsdTimeCode time = default)
    {
        var colorSet = GetVertexColorPrimvar(colorSetName) ?? CreateVertexColors(colorSetName);
        return colorSet.Set(colors, time);
    }
    
    /// <summary>
    /// Get vertex colors.
    /// </summary>
    public List<GfVec3f> GetVertexColors(string colorSetName = "displayColor", UsdTimeCode time = default)
    {
        var colorSet = GetVertexColorPrimvar(colorSetName);
        if (colorSet?.Get(out List<GfVec3f> colors, time) == true)
            return colors;
        return new List<GfVec3f>();
    }
    
    /// <summary>
    /// Set vertex alpha values.
    /// </summary>
    public bool SetVertexAlpha(List<float> alphas, string alphaSetName = "displayOpacity", UsdTimeCode time = default)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.CreatePrimvar(new TfToken(alphaSetName), "float", UsdGeomInterpolation.Vertex);
        return primvar.Set(alphas, time);
    }
    
    /// <summary>
    /// Get vertex alpha values.
    /// </summary>
    public List<float> GetVertexAlpha(string alphaSetName = "displayOpacity", UsdTimeCode time = default)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvar = primvarsAPI.GetPrimvar(new TfToken(alphaSetName));
        if (primvar.IsDefined() && primvar.Get(out List<float> alphas, time))
            return alphas;
        return new List<float>();
    }
    
    #endregion
    
    #region Enhanced Primvar Management
    
    /// <summary>
    /// Create a custom primvar for game-specific data.
    /// </summary>
    public UsdGeomPrimvar CreateCustomPrimvar(string name, string typeName, UsdGeomInterpolation interpolation = UsdGeomInterpolation.Constant)
    {
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        return primvarsAPI.CreatePrimvar(new TfToken(name), typeName, interpolation);
    }
    
    /// <summary>
    /// Set custom data on the mesh (useful for game engine metadata).
    /// </summary>
    public bool SetCustomData<T>(string name, T value, UsdTimeCode time = default)
    {
        try
        {
            var typeName = GetUsdTypeName<T>();
            var primvar = CreateCustomPrimvar(name, typeName, UsdGeomInterpolation.Constant);
            return primvar.Set(value, time);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Get custom data from the mesh.
    /// </summary>
    public T? GetCustomData<T>(string name, UsdTimeCode time = default)
    {
        try
        {
            var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
            var primvar = primvarsAPI.GetPrimvar(new TfToken(name));
            if (primvar.IsDefined() && primvar.Get(out T value, time))
                return value;
        }
        catch
        {
            // Type conversion failed
        }
        return default(T);
    }
    
    private static string GetUsdTypeName<T>()
    {
        var type = typeof(T);
        return type.Name.ToLower() switch
        {
            "int32" => "int",
            "single" => "float",
            "double" => "double",
            "string" => "string",
            "boolean" => "bool",
            _ => "string" // Fallback
        };
    }
    
    /// <summary>
    /// Get all custom primvars on this mesh.
    /// </summary>
    public Dictionary<string, UsdGeomPrimvar> GetAllCustomPrimvars()
    {
        var result = new Dictionary<string, UsdGeomPrimvar>();
        var primvarsAPI = UsdGeomPrimvarsAPI.Get(Prim);
        var primvars = primvarsAPI.GetPrimvars();
        
        foreach (var primvar in primvars)
        {
            var name = primvar.GetPrimvarName().GetText();
            result[name] = primvar;
        }
        
        return result;
    }
    
    #endregion
    
    #region Game Asset Helpers
    
    /// <summary>
    /// Configure this mesh for typical game asset workflows.
    /// Sets up common primvars and material binding.
    /// </summary>
    public void ConfigureForGameAsset()
    {
        // Ensure we have a default UV set
        CreateUVSet("st", UsdGeomInterpolation.FaceVarying);
        
        // Set subdivision scheme to none for game assets
        SubdivisionScheme = "none";
    }
    
    /// <summary>
    /// Create a complete game asset mesh with all typical data.
    /// </summary>
    public void CreateGameAssetMesh(
        List<GfVec3f> vertices,
        List<int> indices,
        List<int> faceCounts,
        List<GfVec2f>? uvCoords = null,
        List<GfVec3f>? normals = null,
        List<GfVec3f>? colors = null,
        Dictionary<string, List<GfVec2f>>? additionalUVSets = null,
        UsdTimeCode time = default)
    {
        // Set basic mesh data
        Points = vertices;
        FaceVertexIndices = indices;
        FaceVertexCounts = faceCounts;
        
        // Configure for game assets
        ConfigureForGameAsset();
        
        // Set UV coordinates
        if (uvCoords != null)
        {
            SetUVCoordinates(uvCoords, "st", time);
        }
        
        // Set normals
        if (normals != null)
        {
            var normalsPrimvar = CreateCustomPrimvar("normals", "normal3f", UsdGeomInterpolation.FaceVarying);
            normalsPrimvar.Set(normals, time);
        }
        
        // Set vertex colors
        if (colors != null)
        {
            SetVertexColors(colors, "displayColor", time);
        }
        
        // Set additional UV sets
        if (additionalUVSets != null)
        {
            CreateGameUVSets(additionalUVSets, time);
        }
    }
    
    #endregion
    
    #region Static Factory Methods
    
    public static UsdGeomMesh Get(UsdStage stage, ISdfPath path)
    {
        return Get<UsdGeomMesh>(stage, path);
    }
    
    public new static UsdGeomMesh Define(UsdStage stage, ISdfPath path)
    {
        return Define<UsdGeomMesh>(stage, path);
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if this mesh is triangulated (all faces are triangles).
    /// </summary>
    public bool IsTriangulated()
    {
        var faceVertexCounts = FaceVertexCounts;
        return faceVertexCounts.All(count => count == 3);
    }
    
    /// <summary>
    /// Check if this mesh is quad-dominant (all faces are quads).
    /// </summary>
    public bool IsQuadDominant()
    {
        var faceVertexCounts = FaceVertexCounts;
        return faceVertexCounts.All(count => count == 4);
    }
    
    /// <summary>
    /// Get mesh statistics.
    /// </summary>
    public (int VertexCount, int FaceCount, int TriangleCount, int QuadCount, int NgonCount) GetStatistics()
    {
        var points = Points;
        var faceVertexCounts = FaceVertexCounts;
        
        var vertexCount = points.Count;
        var faceCount = faceVertexCounts.Count;
        var triangleCount = faceVertexCounts.Count(c => c == 3);
        var quadCount = faceVertexCounts.Count(c => c == 4);
        var ngonCount = faceVertexCounts.Count(c => c > 4);
        
        return (vertexCount, faceCount, triangleCount, quadCount, ngonCount);
    }
    
    #endregion
}