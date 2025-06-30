using System;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPayload represents a payload to scene description in a layer.
/// Payloads enable optional loading of content for memory management.
/// Unlike references, payloads can be explicitly loaded/unloaded.
/// </summary>
public readonly struct SdfPayload : IEquatable<SdfPayload>, IComparable<SdfPayload>
{
    private readonly string _assetPath;
    private readonly SdfPath _primPath;
    private readonly SdfLayerOffset _layerOffset;

    #region Construction

    /// <summary>
    /// Create an empty payload.
    /// </summary>
    public SdfPayload()
    {
        _assetPath = string.Empty;
        _primPath = SdfPath.EmptyPath();
        _layerOffset = SdfLayerOffset.Identity();
    }

    /// <summary>
    /// Create a payload with the given asset path.
    /// </summary>
    public SdfPayload(string assetPath) : this(assetPath, SdfPath.EmptyPath())
    {
    }

    /// <summary>
    /// Create a payload with the given asset path and prim path.
    /// </summary>
    public SdfPayload(string assetPath, SdfPath primPath) : this(assetPath, primPath, SdfLayerOffset.Identity())
    {
    }

    /// <summary>
    /// Create a payload with the given asset path, prim path, and layer offset.
    /// </summary>
    public SdfPayload(string assetPath, SdfPath primPath, SdfLayerOffset layerOffset)
    {
        _assetPath = assetPath ?? string.Empty;
        _primPath = primPath;
        _layerOffset = layerOffset;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the asset path of this payload.
    /// An empty path indicates an internal payload.
    /// </summary>
    public string GetAssetPath() => _assetPath;

    /// <summary>
    /// Get the prim path of this payload.
    /// An empty path indicates a payload to the default prim.
    /// </summary>
    public SdfPath GetPrimPath() => _primPath;

    /// <summary>
    /// Get the layer offset of this payload.
    /// </summary>
    public SdfLayerOffset GetLayerOffset() => _layerOffset;

    #endregion

    #region Payload Types

    /// <summary>
    /// Return true if this is an internal payload (empty asset path).
    /// </summary>
    public bool IsInternal() => string.IsNullOrEmpty(_assetPath);

    /// <summary>
    /// Return true if this payload targets the default prim (empty prim path).
    /// </summary>
    public bool IsDefaultPrim() => _primPath.IsEmpty();

    /// <summary>
    /// Return true if this payload has a layer offset (non-identity transformation).
    /// </summary>
    public bool HasLayerOffset() => !_layerOffset.IsIdentity();

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create an external payload to the specified asset and prim.
    /// </summary>
    public static SdfPayload CreateExternal(string assetPath, SdfPath primPath = default, SdfLayerOffset layerOffset = default)
    {
        return new SdfPayload(assetPath, primPath, layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    /// <summary>
    /// Create an internal payload to the specified prim in the same layer stack.
    /// </summary>
    public static SdfPayload CreateInternal(SdfPath primPath, SdfLayerOffset layerOffset = default)
    {
        return new SdfPayload(string.Empty, primPath, layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    /// <summary>
    /// Create a payload to the default prim of the specified asset.
    /// </summary>
    public static SdfPayload CreateDefaultPrim(string assetPath, SdfLayerOffset layerOffset = default)
    {
        return new SdfPayload(assetPath, SdfPath.EmptyPath(), layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    #endregion

    #region Mutation (returns new instances)

    /// <summary>
    /// Return a new payload with the specified asset path.
    /// </summary>
    public SdfPayload WithAssetPath(string assetPath)
    {
        return new SdfPayload(assetPath, _primPath, _layerOffset);
    }

    /// <summary>
    /// Return a new payload with the specified prim path.
    /// </summary>
    public SdfPayload WithPrimPath(SdfPath primPath)
    {
        return new SdfPayload(_assetPath, primPath, _layerOffset);
    }

    /// <summary>
    /// Return a new payload with the specified layer offset.
    /// </summary>
    public SdfPayload WithLayerOffset(SdfLayerOffset layerOffset)
    {
        return new SdfPayload(_assetPath, _primPath, layerOffset);
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Payloads are equal if they have the same asset path and prim path.
    /// Layer offset is NOT considered for equality.
    /// </summary>
    public bool Equals(SdfPayload other)
    {
        return string.Equals(_assetPath, other._assetPath, StringComparison.Ordinal) &&
               _primPath.Equals(other._primPath);
    }

    public override bool Equals(object? obj)
    {
        return obj is SdfPayload other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_assetPath, _primPath);
    }

    /// <summary>
    /// Compare payloads by asset path first, then by prim path.
    /// </summary>
    public int CompareTo(SdfPayload other)
    {
        var assetPathComparison = string.Compare(_assetPath, other._assetPath, StringComparison.Ordinal);
        if (assetPathComparison != 0)
            return assetPathComparison;

        return _primPath.CompareTo(other._primPath);
    }

    public static bool operator ==(SdfPayload left, SdfPayload right) => left.Equals(right);
    public static bool operator !=(SdfPayload left, SdfPayload right) => !left.Equals(right);
    public static bool operator <(SdfPayload left, SdfPayload right) => left.CompareTo(right) < 0;
    public static bool operator >(SdfPayload left, SdfPayload right) => left.CompareTo(right) > 0;
    public static bool operator <=(SdfPayload left, SdfPayload right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SdfPayload left, SdfPayload right) => left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this payload.
    /// </summary>
    public override string ToString()
    {
        if (IsInternal())
        {
            if (IsDefaultPrim())
                return "SdfPayload(internal, default prim)";
            else
                return $"SdfPayload(internal, {_primPath})";
        }
        else
        {
            var path = IsDefaultPrim() ? "default prim" : _primPath.GetString();
            var offset = HasLayerOffset() ? $", {_layerOffset}" : "";
            return $"SdfPayload({_assetPath}, {path}{offset})";
        }
    }

    #endregion
}