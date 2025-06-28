using System;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfAssetPath represents a path to an asset, potentially with additional resolution metadata.
/// It can represent both absolute and relative paths, and supports asset resolution context.
/// </summary>
public readonly struct SdfAssetPath : IEquatable<SdfAssetPath>
{
    private readonly string _assetPath;
    private readonly string? _resolvedPath;

    public SdfAssetPath(string assetPath, string? resolvedPath = null)
    {
        _assetPath = assetPath ?? throw new ArgumentNullException(nameof(assetPath));
        _resolvedPath = resolvedPath;
    }

    /// <summary>
    /// Get the asset path as authored.
    /// </summary>
    public string GetAssetPath() => _assetPath;

    /// <summary>
    /// Get the resolved path, if available.
    /// </summary>
    public string GetResolvedPath() => _resolvedPath ?? _assetPath;

    /// <summary>
    /// Return true if this asset path is empty.
    /// </summary>
    public bool IsEmpty() => string.IsNullOrEmpty(_assetPath);

    /// <summary>
    /// Return true if this asset path has a resolved path.
    /// </summary>
    public bool HasResolvedPath() => !string.IsNullOrEmpty(_resolvedPath);

    public static implicit operator SdfAssetPath(string assetPath) => new(assetPath);
    public static implicit operator string(SdfAssetPath assetPath) => assetPath.GetAssetPath();

    public bool Equals(SdfAssetPath other)
    {
        return string.Equals(_assetPath, other._assetPath, StringComparison.Ordinal) &&
               string.Equals(_resolvedPath, other._resolvedPath, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => obj is SdfAssetPath other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_assetPath, _resolvedPath);

    public override string ToString() => _assetPath;

    public static bool operator ==(SdfAssetPath left, SdfAssetPath right) => left.Equals(right);
    public static bool operator !=(SdfAssetPath left, SdfAssetPath right) => !left.Equals(right);

    public static readonly SdfAssetPath Empty = new(string.Empty);
}