using System;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdEditTarget defines a mapping from scene graph paths to SdfSpec paths in a particular SdfLayer 
/// where edits should be directed, and establishes the composition context for authored scene description.
/// </summary>
public readonly struct UsdEditTarget : IEquatable<UsdEditTarget>
{
    private readonly SdfLayer? _layer;
    private readonly UsdPathMapping _mapping;

    #region Construction

    /// <summary>
    /// Construct a null UsdEditTarget. A null edit target will return false for IsValid().
    /// </summary>
    public UsdEditTarget()
    {
        _layer = null;
        _mapping = UsdPathMapping.Identity();
    }

    /// <summary>
    /// Construct a UsdEditTarget with the specified layer.
    /// </summary>
    public UsdEditTarget(SdfLayer? layer) : this(layer, UsdPathMapping.Identity())
    {
    }

    /// <summary>
    /// Construct a UsdEditTarget with the specified layer and path mapping.
    /// </summary>
    public UsdEditTarget(SdfLayer? layer, UsdPathMapping mapping)
    {
        _layer = layer;
        _mapping = mapping;
    }

    #endregion

    #region Validity and State

    /// <summary>
    /// Return true if this edit target is null. Null edit targets are invalid.
    /// </summary>
    public bool IsNull() => _layer == null;

    /// <summary>
    /// Return true if this edit target is valid. Valid edit targets have a non-null layer.
    /// </summary>
    public bool IsValid() => _layer != null;

    /// <summary>
    /// Get the layer that edits should be directed to.
    /// </summary>
    public SdfLayer? GetLayer() => _layer;

    /// <summary>
    /// Get the path mapping function for this edit target.
    /// </summary>
    public UsdPathMapping GetMapFunction() => _mapping;

    #endregion

    #region Path Mapping

    /// <summary>
    /// Map the provided scene graph path to a SdfSpec path for the edit target's layer.
    /// </summary>
    public SdfPath MapToSpecPath(SdfPath scenePath)
    {
        if (!IsValid() || scenePath.IsEmpty())
            return SdfPath.EmptyPath();

        return _mapping.MapSourceToTarget(scenePath);
    }

    /// <summary>
    /// Convenience method for getting a SdfPrimSpec in the edit target's layer for scenePath.
    /// </summary>
    public SdfPrimSpec? GetPrimSpecForScenePath(SdfPath scenePath)
    {
        var specPath = MapToSpecPath(scenePath);
        if (specPath.IsEmpty() || !specPath.IsPrimPath() || _layer == null)
            return null;

        // For now, create a new prim spec - in a full implementation this would
        // look up existing specs in the layer
        return new SdfPrimSpec(_layer, specPath);
    }

    /// <summary>
    /// Convenience method for getting a SdfPropertySpec in the edit target's layer for scenePath.
    /// </summary>
    public SdfPropertySpec? GetPropertySpecForScenePath(SdfPath scenePath)
    {
        var specPath = MapToSpecPath(scenePath);
        if (specPath.IsEmpty() || !specPath.IsPropertyPath())
            return null;

        // SdfPropertySpec is already available
        return new SdfPropertySpec();
    }

    /// <summary>
    /// Convenience method for getting any SdfSpec in the edit target's layer for scenePath.
    /// </summary>
    public object? GetSpecForScenePath(SdfPath scenePath)
    {
        var specPath = MapToSpecPath(scenePath);
        if (specPath.IsEmpty())
            return null;

        if (specPath.IsPrimPath())
            return GetPrimSpecForScenePath(scenePath);
        else if (specPath.IsPropertyPath())
            return GetPropertySpecForScenePath(scenePath);

        return null;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create an edit target for the session layer with an identity mapping.
    /// </summary>
    public static UsdEditTarget ForSessionLayer(SdfLayer? sessionLayer)
    {
        return new UsdEditTarget(sessionLayer, UsdPathMapping.Identity());
    }

    /// <summary>
    /// Create an edit target for a local layer with an identity mapping.
    /// </summary>
    public static UsdEditTarget ForLocalLayer(SdfLayer? layer)
    {
        return new UsdEditTarget(layer, UsdPathMapping.Identity());
    }

    /// <summary>
    /// Create an edit target for local direct variant editing.
    /// This is for editing variant selections directly in the local layer stack.
    /// </summary>
    public static UsdEditTarget ForLocalDirectVariant(SdfLayer? layer, SdfPath variantSelectionPath)
    {
        // Create a mapping that handles variant selections
        var mapping = UsdPathMapping.CreateVariantMapping(variantSelectionPath);
        return new UsdEditTarget(layer, mapping);
    }

    #endregion

    #region Composition

    /// <summary>
    /// Return a new edit target that composes this edit target over the specified weaker edit target.
    /// </summary>
    public UsdEditTarget ComposeOver(UsdEditTarget weaker)
    {
        if (!IsValid())
            return weaker;
        if (!weaker.IsValid())
            return this;

        // For simple composition, prefer the stronger (this) target
        // TODO: Implement proper composition when full composition system is available
        return this;
    }

    #endregion

    #region Equality and Hashing

    public bool Equals(UsdEditTarget other)
    {
        return ReferenceEquals(_layer, other._layer) && _mapping.Equals(other._mapping);
    }

    public override bool Equals(object? obj)
    {
        return obj is UsdEditTarget other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_layer?.GetHashCode() ?? 0, _mapping.GetHashCode());
    }

    public static bool operator ==(UsdEditTarget left, UsdEditTarget right) => left.Equals(right);
    public static bool operator !=(UsdEditTarget left, UsdEditTarget right) => !left.Equals(right);

    #endregion

    #region String Representation

    public override string ToString()
    {
        if (!IsValid())
            return "UsdEditTarget(null)";

        var layerName = _layer?.GetDisplayName() ?? "unknown";
        return $"UsdEditTarget({layerName})";
    }

    #endregion
}

/// <summary>
/// Helper class for path mapping operations in UsdEditTarget.
/// Represents a mapping function that can transform scene graph paths to spec paths.
/// </summary>
public readonly struct UsdPathMapping : IEquatable<UsdPathMapping>
{
    private readonly SdfPath _variantSelectionPath;
    private readonly bool _isIdentity;

    private UsdPathMapping(bool isIdentity, SdfPath variantSelectionPath = default)
    {
        _isIdentity = isIdentity;
        _variantSelectionPath = variantSelectionPath;
    }

    /// <summary>
    /// Create an identity mapping that maps paths unchanged.
    /// </summary>
    public static UsdPathMapping Identity() => new(true);

    /// <summary>
    /// Create a variant mapping for the specified variant selection path.
    /// </summary>
    public static UsdPathMapping CreateVariantMapping(SdfPath variantSelectionPath)
    {
        return new(false, variantSelectionPath);
    }

    /// <summary>
    /// Map a source path to a target path using this mapping.
    /// </summary>
    public SdfPath MapSourceToTarget(SdfPath sourcePath)
    {
        if (sourcePath.IsEmpty())
            return SdfPath.EmptyPath();

        if (_isIdentity)
            return sourcePath;

        // TODO: Implement variant path mapping when variant system is available
        // For now, return identity mapping
        return sourcePath;
    }

    public bool Equals(UsdPathMapping other)
    {
        return _isIdentity == other._isIdentity && _variantSelectionPath.Equals(other._variantSelectionPath);
    }

    public override bool Equals(object? obj)
    {
        return obj is UsdPathMapping other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_isIdentity, _variantSelectionPath.GetHashCode());
    }

    public static bool operator ==(UsdPathMapping left, UsdPathMapping right) => left.Equals(right);
    public static bool operator !=(UsdPathMapping left, UsdPathMapping right) => !left.Equals(right);
}