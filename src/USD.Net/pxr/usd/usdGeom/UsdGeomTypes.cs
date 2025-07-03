using System;
using System.Numerics;

namespace Pxr.Usd.UsdGeom;

/// <summary>
/// Enum representing the interpolation methods for primitive variables (primvars).
/// This controls how values are interpreted across the geometry surface.
/// </summary>
public enum UsdGeomInterpolation
{
    /// <summary>
    /// One value for the entire primitive.
    /// </summary>
    Constant,
    
    /// <summary>
    /// One value per face or element.
    /// </summary>
    Uniform,
    
    /// <summary>
    /// Values vary smoothly across the primitive (interpolated).
    /// </summary>
    Varying,
    
    /// <summary>
    /// One value per vertex.
    /// </summary>
    Vertex,
    
    /// <summary>
    /// One value per face-vertex (allows discontinuous values at vertices).
    /// </summary>
    FaceVarying
}

/// <summary>
/// Enum for visibility values in USD.
/// </summary>
public enum UsdGeomVisibility
{
    /// <summary>
    /// Inherit visibility from parent.
    /// </summary>
    Inherited,
    
    /// <summary>
    /// Make this prim and its subtree invisible.
    /// </summary>
    Invisible
}

/// <summary>
/// Enum for purpose classification of geometry.
/// </summary>
public enum UsdGeomPurpose
{
    /// <summary>
    /// Default geometry for primary use.
    /// </summary>
    Default,
    
    /// <summary>
    /// High-quality geometry for final rendering.
    /// </summary>
    Render,
    
    /// <summary>
    /// Simplified geometry for performance.
    /// </summary>
    Proxy,
    
    /// <summary>
    /// Geometry for user guidance (guides, handles).
    /// </summary>
    Guide
}

/// <summary>
/// Represents a 3D vector (position, normal, etc.).
/// Compatible with USD's GfVec3f.
/// </summary>
public struct GfVec3f : IEquatable<GfVec3f>
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    
    public GfVec3f(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    public GfVec3f(Vector3 vector)
    {
        X = vector.X;
        Y = vector.Y;
        Z = vector.Z;
    }
    
    /// <summary>
    /// Convert to System.Numerics.Vector3.
    /// </summary>
    public Vector3 ToVector3() => new Vector3(X, Y, Z);
    
    /// <summary>
    /// Implicit conversion to Vector3.
    /// </summary>
    public static implicit operator Vector3(GfVec3f vec) => vec.ToVector3();
    
    /// <summary>
    /// Implicit conversion from Vector3.
    /// </summary>
    public static implicit operator GfVec3f(Vector3 vec) => new GfVec3f(vec);
    
    public bool Equals(GfVec3f other) => X == other.X && Y == other.Y && Z == other.Z;
    
    public override bool Equals(object? obj) => obj is GfVec3f other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    
    public override string ToString() => $"({X}, {Y}, {Z})";
    
    public static bool operator ==(GfVec3f left, GfVec3f right) => left.Equals(right);
    public static bool operator !=(GfVec3f left, GfVec3f right) => !left.Equals(right);
    
    public static GfVec3f Zero => new GfVec3f(0, 0, 0);
    public static GfVec3f One => new GfVec3f(1, 1, 1);
    public static GfVec3f UnitX => new GfVec3f(1, 0, 0);
    public static GfVec3f UnitY => new GfVec3f(0, 1, 0);
    public static GfVec3f UnitZ => new GfVec3f(0, 0, 1);
}

/// <summary>
/// Represents a 2D vector (UV coordinates, etc.).
/// Compatible with USD's GfVec2f.
/// </summary>
public struct GfVec2f : IEquatable<GfVec2f>
{
    public float X { get; set; }
    public float Y { get; set; }
    
    public GfVec2f(float x, float y)
    {
        X = x;
        Y = y;
    }
    
    public GfVec2f(Vector2 vector)
    {
        X = vector.X;
        Y = vector.Y;
    }
    
    /// <summary>
    /// Convert to System.Numerics.Vector2.
    /// </summary>
    public Vector2 ToVector2() => new Vector2(X, Y);
    
    /// <summary>
    /// Implicit conversion to Vector2.
    /// </summary>
    public static implicit operator Vector2(GfVec2f vec) => vec.ToVector2();
    
    /// <summary>
    /// Implicit conversion from Vector2.
    /// </summary>
    public static implicit operator GfVec2f(Vector2 vec) => new GfVec2f(vec);
    
    public bool Equals(GfVec2f other) => X == other.X && Y == other.Y;
    
    public override bool Equals(object? obj) => obj is GfVec2f other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(X, Y);
    
    public override string ToString() => $"({X}, {Y})";
    
    public static bool operator ==(GfVec2f left, GfVec2f right) => left.Equals(right);
    public static bool operator !=(GfVec2f left, GfVec2f right) => !left.Equals(right);
    
    public static GfVec2f Zero => new GfVec2f(0, 0);
    public static GfVec2f One => new GfVec2f(1, 1);
    public static GfVec2f UnitX => new GfVec2f(1, 0);
    public static GfVec2f UnitY => new GfVec2f(0, 1);
}

/// <summary>
/// Represents a 3D color value.
/// Compatible with USD's GfVec3f color representation.
/// </summary>
public struct GfVec3Color : IEquatable<GfVec3Color>
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    
    public GfVec3Color(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }
    
    /// <summary>
    /// Convert to GfVec3f.
    /// </summary>
    public GfVec3f ToVec3f() => new GfVec3f(R, G, B);
    
    /// <summary>
    /// Create from RGB values (0-255).
    /// </summary>
    public static GfVec3Color FromRGB(byte r, byte g, byte b) => 
        new GfVec3Color(r / 255.0f, g / 255.0f, b / 255.0f);
    
    public bool Equals(GfVec3Color other) => R == other.R && G == other.G && B == other.B;
    
    public override bool Equals(object? obj) => obj is GfVec3Color other && Equals(other);
    
    public override int GetHashCode() => HashCode.Combine(R, G, B);
    
    public override string ToString() => $"({R:F3}, {G:F3}, {B:F3})";
    
    public static bool operator ==(GfVec3Color left, GfVec3Color right) => left.Equals(right);
    public static bool operator !=(GfVec3Color left, GfVec3Color right) => !left.Equals(right);
    
    // Common colors
    public static GfVec3Color White => new GfVec3Color(1, 1, 1);
    public static GfVec3Color Black => new GfVec3Color(0, 0, 0);
    public static GfVec3Color Red => new GfVec3Color(1, 0, 0);
    public static GfVec3Color Green => new GfVec3Color(0, 1, 0);
    public static GfVec3Color Blue => new GfVec3Color(0, 0, 1);
}

/// <summary>
/// Represents a 4x4 transformation matrix.
/// Compatible with USD's GfMatrix4d.
/// </summary>
public struct GfMatrix4d : IEquatable<GfMatrix4d>
{
    private Matrix4x4 _matrix;
    
    public GfMatrix4d(Matrix4x4 matrix)
    {
        _matrix = matrix;
    }
    
    /// <summary>
    /// Create identity matrix.
    /// </summary>
    public static GfMatrix4d Identity => new GfMatrix4d(Matrix4x4.Identity);
    
    /// <summary>
    /// Create translation matrix.
    /// </summary>
    public static GfMatrix4d CreateTranslation(GfVec3f translation) =>
        new GfMatrix4d(Matrix4x4.CreateTranslation(translation.ToVector3()));
    
    /// <summary>
    /// Create rotation matrix from Euler angles (in radians).
    /// </summary>
    public static GfMatrix4d CreateRotation(float x, float y, float z) =>
        new GfMatrix4d(Matrix4x4.CreateFromYawPitchRoll(y, x, z));
    
    /// <summary>
    /// Create rotation matrix from quaternion.
    /// </summary>
    public static GfMatrix4d CreateFromQuaternion(Quaternion quaternion) =>
        new GfMatrix4d(Matrix4x4.CreateFromQuaternion(quaternion));
    
    /// <summary>
    /// Create scale matrix.
    /// </summary>
    public static GfMatrix4d CreateScale(GfVec3f scale) =>
        new GfMatrix4d(Matrix4x4.CreateScale(scale.ToVector3()));
    
    /// <summary>
    /// Create scale matrix with uniform scaling.
    /// </summary>
    public static GfMatrix4d CreateScale(float scale) =>
        new GfMatrix4d(Matrix4x4.CreateScale(scale));
    
    /// <summary>
    /// Get the underlying Matrix4x4.
    /// </summary>
    public Matrix4x4 ToMatrix4x4() => _matrix;
    
    /// <summary>
    /// Access matrix elements by linear index (row-major order).
    /// </summary>
    public float this[int index]
    {
        get
        {
            return index switch
            {
                0 => _matrix.M11, 1 => _matrix.M12, 2 => _matrix.M13, 3 => _matrix.M14,
                4 => _matrix.M21, 5 => _matrix.M22, 6 => _matrix.M23, 7 => _matrix.M24,
                8 => _matrix.M31, 9 => _matrix.M32, 10 => _matrix.M33, 11 => _matrix.M34,
                12 => _matrix.M41, 13 => _matrix.M42, 14 => _matrix.M43, 15 => _matrix.M44,
                _ => throw new ArgumentOutOfRangeException(nameof(index), "Matrix index must be 0-15")
            };
        }
    }
    
    /// <summary>
    /// Multiply two transformation matrices.
    /// </summary>
    public static GfMatrix4d operator *(GfMatrix4d left, GfMatrix4d right) =>
        new GfMatrix4d(left._matrix * right._matrix);
    
    /// <summary>
    /// Transform a point by this matrix.
    /// </summary>
    public GfVec3f Transform(GfVec3f point)
    {
        var result = Vector3.Transform(point.ToVector3(), _matrix);
        return new GfVec3f(result);
    }
    
    public bool Equals(GfMatrix4d other) => _matrix.Equals(other._matrix);
    
    public override bool Equals(object? obj) => obj is GfMatrix4d other && Equals(other);
    
    public override int GetHashCode() => _matrix.GetHashCode();
    
    public override string ToString() => _matrix.ToString();
    
    public static bool operator ==(GfMatrix4d left, GfMatrix4d right) => left.Equals(right);
    public static bool operator !=(GfMatrix4d left, GfMatrix4d right) => !left.Equals(right);
}

/// <summary>
/// Constants and utilities for USD Primvars (primitive variables).
/// </summary>
public static class UsdGeomPrimvarConstants
{
    /// <summary>
    /// The namespace prefix for all primvar attributes.
    /// </summary>
    public const string PrimvarNamespace = "primvars:";
    
    /// <summary>
    /// The suffix used for indexed primvar indices.
    /// </summary>
    public const string IndicesSuffix = ":indices";
    
    /// <summary>
    /// Reserved primvar keywords that cannot be used as base names.
    /// </summary>
    public static readonly string[] ReservedKeywords = { "indices" };
    
    /// <summary>
    /// Convert interpolation enum to token string.
    /// </summary>
    public static string InterpolationToString(UsdGeomInterpolation interpolation)
    {
        return interpolation switch
        {
            UsdGeomInterpolation.Constant => "constant",
            UsdGeomInterpolation.Uniform => "uniform", 
            UsdGeomInterpolation.Varying => "varying",
            UsdGeomInterpolation.Vertex => "vertex",
            UsdGeomInterpolation.FaceVarying => "faceVarying",
            _ => "vertex" // Default fallback
        };
    }
    
    /// <summary>
    /// Convert token string to interpolation enum.
    /// </summary>
    public static UsdGeomInterpolation StringToInterpolation(string interpolation)
    {
        return interpolation switch
        {
            "constant" => UsdGeomInterpolation.Constant,
            "uniform" => UsdGeomInterpolation.Uniform,
            "varying" => UsdGeomInterpolation.Varying,
            "vertex" => UsdGeomInterpolation.Vertex,
            "faceVarying" => UsdGeomInterpolation.FaceVarying,
            _ => UsdGeomInterpolation.Vertex // Default fallback
        };
    }
    
    /// <summary>
    /// Check if a name is a valid primvar name.
    /// </summary>
    public static bool IsValidPrimvarName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;
            
        // Strip primvars: prefix if present
        var baseName = name;
        if (name.StartsWith(PrimvarNamespace))
            baseName = name.Substring(PrimvarNamespace.Length);
            
        // Check for reserved keywords
        var parts = baseName.Split(':');
        var finalPart = parts[parts.Length - 1];
        
        foreach (var keyword in ReservedKeywords)
        {
            if (finalPart == keyword)
                return false;
        }
        
        return !string.IsNullOrEmpty(baseName);
    }
    
    /// <summary>
    /// Check if an interpolation value is valid.
    /// </summary>
    public static bool IsValidInterpolation(UsdGeomInterpolation interpolation)
    {
        return interpolation >= UsdGeomInterpolation.Constant && 
               interpolation <= UsdGeomInterpolation.FaceVarying;
    }
    
    /// <summary>
    /// Get the full primvar attribute name from a base name.
    /// </summary>
    public static string MakePrimvarAttrName(string baseName)
    {
        if (string.IsNullOrEmpty(baseName))
            return string.Empty;
            
        if (baseName.StartsWith(PrimvarNamespace))
            return baseName;
            
        return PrimvarNamespace + baseName;
    }
    
    /// <summary>
    /// Get the base name from a full primvar attribute name.
    /// </summary>
    public static string GetPrimvarBaseName(string attrName)
    {
        if (string.IsNullOrEmpty(attrName))
            return string.Empty;
            
        if (attrName.StartsWith(PrimvarNamespace))
            return attrName.Substring(PrimvarNamespace.Length);
            
        return attrName;
    }
    
    /// <summary>
    /// Check if an attribute name represents a primvar.
    /// </summary>
    public static bool IsPrimvarName(string attrName)
    {
        return !string.IsNullOrEmpty(attrName) && attrName.StartsWith(PrimvarNamespace);
    }
}

/// <summary>
/// Common tokens used in UsdGeom.
/// </summary>
public static class UsdGeomTokens
{
    /// <summary>
    /// Interpolation types for UsdGeom.
    /// </summary>
    public static class Interpolation
    {
        public const UsdGeomInterpolation Constant = UsdGeomInterpolation.Constant;
        public const UsdGeomInterpolation Uniform = UsdGeomInterpolation.Uniform;
        public const UsdGeomInterpolation Varying = UsdGeomInterpolation.Varying;
        public const UsdGeomInterpolation Vertex = UsdGeomInterpolation.Vertex;
        public const UsdGeomInterpolation FaceVarying = UsdGeomInterpolation.FaceVarying;
    }
    
    /// <summary>
    /// Common attribute names used in UsdGeom.
    /// </summary>
    public static class AttributeNames
    {
        public const string Points = "points";
        public const string Normals = "normals";
        public const string ST = "st";
        public const string DisplayColor = "displayColor";
        public const string DisplayOpacity = "displayOpacity";
        public const string Velocities = "velocities";
        public const string Accelerations = "accelerations";
        public const string Widths = "widths";
        public const string Ids = "ids";
        public const string Orientations = "orientations";
        public const string Scales = "scales";
        public const string InvisibleIds = "invisibleIds";
    }
}