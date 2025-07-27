namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfReference represents a reference to scene description in a layer.
/// References enable the composition of scene description from multiple layers.
/// </summary>
public readonly struct SdfReference : IEquatable<SdfReference>, IComparable<SdfReference>
{
    private readonly string _assetPath;
    private readonly SdfPath _primPath;
    private readonly SdfLayerOffset _layerOffset;
    private readonly Dictionary<string, object>? _customData;

    #region Construction

    /// <summary>
    /// Create an empty reference.
    /// </summary>
    public SdfReference()
    {
        _assetPath = string.Empty;
        _primPath = SdfPath.EmptyPath();
        _layerOffset = SdfLayerOffset.Identity();
        _customData = null;
    }

    /// <summary>
    /// Create a reference with the given asset path.
    /// </summary>
    public SdfReference(string assetPath) : this(assetPath, SdfPath.EmptyPath())
    {
    }

    /// <summary>
    /// Create a reference with the given asset path and prim path.
    /// </summary>
    public SdfReference(string assetPath, SdfPath primPath) : this(assetPath, primPath, SdfLayerOffset.Identity())
    {
    }

    /// <summary>
    /// Create a reference with the given asset path, prim path, and layer offset.
    /// </summary>
    public SdfReference(string assetPath, SdfPath primPath, SdfLayerOffset layerOffset) : this(assetPath, primPath, layerOffset, null)
    {
    }

    /// <summary>
    /// Create a reference with all parameters.
    /// </summary>
    public SdfReference(string assetPath, SdfPath primPath, SdfLayerOffset layerOffset, Dictionary<string, object>? customData)
    {
        _assetPath = assetPath ?? string.Empty;
        _primPath = primPath;
        _layerOffset = layerOffset;
        _customData = customData;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the asset path of this reference.
    /// An empty path indicates an internal reference.
    /// </summary>
    public string GetAssetPath() => _assetPath;

    /// <summary>
    /// Get the prim path of this reference.
    /// An empty path indicates a reference to the default prim.
    /// </summary>
    public SdfPath GetPrimPath() => _primPath;

    /// <summary>
    /// Get the layer offset of this reference.
    /// </summary>
    public SdfLayerOffset GetLayerOffset() => _layerOffset;

    /// <summary>
    /// Get the custom data of this reference.
    /// </summary>
    public Dictionary<string, object>? GetCustomData() => _customData;

    #endregion

    #region Reference Types

    /// <summary>
    /// Return true if this is an internal reference (empty asset path).
    /// </summary>
    public bool IsInternal() => string.IsNullOrEmpty(_assetPath);

    /// <summary>
    /// Return true if this reference targets the default prim (empty prim path).
    /// </summary>
    public bool IsDefaultPrim() => _primPath.IsEmpty();

    /// <summary>
    /// Return true if this reference has a layer offset (non-identity transformation).
    /// </summary>
    public bool HasLayerOffset() => !_layerOffset.IsIdentity();

    /// <summary>
    /// Return true if this reference has custom data.
    /// </summary>
    public bool HasCustomData() => _customData != null && _customData.Count > 0;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create an external reference to the specified asset and prim.
    /// </summary>
    public static SdfReference CreateExternal(string assetPath, SdfPath primPath = default, SdfLayerOffset layerOffset = default)
    {
        return new SdfReference(assetPath, primPath, layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    /// <summary>
    /// Create an internal reference to the specified prim in the same layer stack.
    /// </summary>
    public static SdfReference CreateInternal(SdfPath primPath, SdfLayerOffset layerOffset = default)
    {
        return new SdfReference(string.Empty, primPath, layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    /// <summary>
    /// Create a reference to the default prim of the specified asset.
    /// </summary>
    public static SdfReference CreateDefaultPrim(string assetPath, SdfLayerOffset layerOffset = default)
    {
        return new SdfReference(assetPath, SdfPath.EmptyPath(), layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset);
    }

    #endregion

    #region Mutation (returns new instances)

    /// <summary>
    /// Return a new reference with the specified asset path.
    /// </summary>
    public SdfReference WithAssetPath(string assetPath)
    {
        return new SdfReference(assetPath, _primPath, _layerOffset, _customData);
    }

    /// <summary>
    /// Return a new reference with the specified prim path.
    /// </summary>
    public SdfReference WithPrimPath(SdfPath primPath)
    {
        return new SdfReference(_assetPath, primPath, _layerOffset, _customData);
    }

    /// <summary>
    /// Return a new reference with the specified layer offset.
    /// </summary>
    public SdfReference WithLayerOffset(SdfLayerOffset layerOffset)
    {
        return new SdfReference(_assetPath, _primPath, layerOffset, _customData);
    }

    /// <summary>
    /// Return a new reference with the specified custom data.
    /// </summary>
    public SdfReference WithCustomData(Dictionary<string, object>? customData)
    {
        return new SdfReference(_assetPath, _primPath, _layerOffset, customData);
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// References are equal if they have the same asset path and prim path.
    /// Layer offset and custom data are NOT considered for equality.
    /// </summary>
    public bool Equals(SdfReference other)
    {
        return string.Equals(_assetPath, other._assetPath, StringComparison.Ordinal) &&
               _primPath.Equals(other._primPath);
    }

    public override bool Equals(object? obj)
    {
        return obj is SdfReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_assetPath, _primPath);
    }

    /// <summary>
    /// Compare references by asset path first, then by prim path.
    /// </summary>
    public int CompareTo(SdfReference other)
    {
        var assetPathComparison = string.Compare(_assetPath, other._assetPath, StringComparison.Ordinal);
        if (assetPathComparison != 0)
            return assetPathComparison;

        return _primPath.CompareTo(other._primPath);
    }

    public static bool operator ==(SdfReference left, SdfReference right) => left.Equals(right);
    public static bool operator !=(SdfReference left, SdfReference right) => !left.Equals(right);
    public static bool operator <(SdfReference left, SdfReference right) => left.CompareTo(right) < 0;
    public static bool operator >(SdfReference left, SdfReference right) => left.CompareTo(right) > 0;
    public static bool operator <=(SdfReference left, SdfReference right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SdfReference left, SdfReference right) => left.CompareTo(right) >= 0;

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this reference.
    /// </summary>
    public override string ToString()
    {
        if (IsInternal())
        {
            if (IsDefaultPrim())
                return "SdfReference(internal, default prim)";
            else
                return $"SdfReference(internal, {_primPath})";
        }
        else
        {
            var path = IsDefaultPrim() ? "default prim" : _primPath.GetString();
            var offset = HasLayerOffset() ? $", {_layerOffset}" : "";
            return $"SdfReference({_assetPath}, {path}{offset})";
        }
    }

    #endregion
}

/// <summary>
/// SdfLayerOffset represents a time offset and scale to be applied to animation splines.
/// This enables time-domain transformations when composing layers.
/// </summary>
public readonly struct SdfLayerOffset : IEquatable<SdfLayerOffset>
{
    private readonly double _offset;
    private readonly double _scale;

    #region Construction

    /// <summary>
    /// Create an identity layer offset (no transformation).
    /// </summary>
    public SdfLayerOffset() : this(0.0, 1.0)
    {
    }

    /// <summary>
    /// Create a layer offset with the specified offset and scale.
    /// </summary>
    public SdfLayerOffset(double offset, double scale = 1.0)
    {
        _offset = offset;
        _scale = scale;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the time offset.
    /// </summary>
    public double GetOffset() => _offset;

    /// <summary>
    /// Get the time scale.
    /// </summary>
    public double GetScale() => _scale;

    #endregion

    #region Transformation

    /// <summary>
    /// Apply this layer offset to a time value.
    /// Result = (time * scale) + offset
    /// </summary>
    public double TransformTime(double time)
    {
        return (time * _scale) + _offset;
    }

    /// <summary>
    /// Get the inverse of this layer offset.
    /// </summary>
    public SdfLayerOffset GetInverse()
    {
        if (_scale == 0.0)
            throw new InvalidOperationException("Cannot invert layer offset with zero scale");

        var inverseScale = 1.0 / _scale;
        var inverseOffset = -_offset * inverseScale;
        return new SdfLayerOffset(inverseOffset, inverseScale);
    }

    /// <summary>
    /// Compose this layer offset with another offset.
    /// The result applies other first, then this offset.
    /// </summary>
    public SdfLayerOffset ComposeWith(SdfLayerOffset other)
    {
        // Apply other first: (time * other.scale) + other.offset
        // Then apply this: ((time * other.scale) + other.offset) * this.scale + this.offset
        // Result: (time * other.scale * this.scale) + (other.offset * this.scale) + this.offset
        var newScale = other._scale * _scale;
        var newOffset = (other._offset * _scale) + _offset;
        return new SdfLayerOffset(newOffset, newScale);
    }

    #endregion

    #region Identity and Validation

    /// <summary>
    /// Return true if this is an identity transformation.
    /// </summary>
    public bool IsIdentity() => _offset == 0.0 && _scale == 1.0;

    /// <summary>
    /// Return true if this offset has valid (finite) values.
    /// </summary>
    public bool IsValid() => double.IsFinite(_offset) && double.IsFinite(_scale);

    /// <summary>
    /// Return true if this is the default (uninitialized) value.
    /// </summary>
    public bool IsDefault() => _offset == 0.0 && _scale == 0.0;

    /// <summary>
    /// Get the identity layer offset.
    /// </summary>
    public static SdfLayerOffset Identity() => new(0.0, 1.0);

    #endregion

    #region Equality and Operators

    public bool Equals(SdfLayerOffset other)
    {
        return _offset.Equals(other._offset) && _scale.Equals(other._scale);
    }

    public override bool Equals(object? obj)
    {
        return obj is SdfLayerOffset other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_offset, _scale);
    }

    public static bool operator ==(SdfLayerOffset left, SdfLayerOffset right) => left.Equals(right);
    public static bool operator !=(SdfLayerOffset left, SdfLayerOffset right) => !left.Equals(right);

    /// <summary>
    /// Compose two layer offsets using the multiplication operator.
    /// </summary>
    public static SdfLayerOffset operator *(SdfLayerOffset left, SdfLayerOffset right)
    {
        return left.ComposeWith(right);
    }

    #endregion

    #region String Representation

    public override string ToString()
    {
        if (IsIdentity())
            return "SdfLayerOffset(identity)";
        else
            return $"SdfLayerOffset(offset={_offset}, scale={_scale})";
    }

    #endregion
}