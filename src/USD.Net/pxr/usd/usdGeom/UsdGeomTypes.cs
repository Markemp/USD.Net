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