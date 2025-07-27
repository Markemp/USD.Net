namespace Pxr.Usd.Sdf;

/// <summary>
/// Identifies the logical object behind an SdfSpec.
/// 
/// This is simply the layer the spec belongs to and the path to the spec.
/// </summary>
public class SdfIdentity : ISdfIdentity
{
    private readonly WeakReference<ISdfLayer> _layerRef;
    private readonly ISdfPath _path;

    /// <summary>
    /// Constructs an identity with the given layer and path.
    /// </summary>
    public SdfIdentity(ISdfLayer layer, ISdfPath path)
    {
        _layerRef = new WeakReference<ISdfLayer>(layer ?? throw new ArgumentNullException(nameof(layer)));
        _path = path ?? throw new ArgumentNullException(nameof(path));
    }

    /// <summary>
    /// Returns the layer that this identity refers to.
    /// </summary>
    public ISdfLayer? GetLayer()
    {
        _layerRef.TryGetTarget(out var layer);
        return layer;
    }

    /// <summary>
    /// Returns the path that this identity refers to.
    /// </summary>
    public ISdfPath GetPath() => _path;

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified identity is equal to the current identity.
    /// </summary>
    public bool Equals(ISdfIdentity? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return _path.Equals(other.GetPath()) &&
               ReferenceEquals(GetLayer(), other.GetLayer());
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current identity.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ISdfIdentity);
    }

    /// <summary>
    /// Compares the current instance with another identity and returns an integer that 
    /// indicates whether the current instance precedes, follows, or occurs in the same 
    /// position in the sort order as the other identity.
    /// </summary>
    public int CompareTo(ISdfIdentity? other)
    {
        if (ReferenceEquals(other, null))
            return 1;
        if (ReferenceEquals(this, other))
            return 0;

        // First compare layers by identity
        var thisLayer = GetLayer();
        var otherLayer = other.GetLayer();

        if (thisLayer == null && otherLayer == null)
            return _path.CompareTo(other.GetPath());
        if (thisLayer == null)
            return -1;
        if (otherLayer == null)
            return 1;

        // Compare layer references (this is a simplification)
        var layerComparison = thisLayer.GetHashCode().CompareTo(otherLayer.GetHashCode());
        if (layerComparison != 0)
            return layerComparison;

        return _path.CompareTo(other.GetPath());
    }

    /// <summary>
    /// Returns a hash code for this identity.
    /// </summary>
    public override int GetHashCode()
    {
        var layer = GetLayer();
        var layerHash = layer?.GetHashCode() ?? 0;
        return HashCode.Combine(layerHash, _path.GetHashCode());
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(SdfIdentity? left, SdfIdentity? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(SdfIdentity? left, SdfIdentity? right)
    {
        return !(left == right);
    }

    #endregion

    /// <summary>
    /// Returns a string representation of this identity.
    /// </summary>
    public override string ToString()
    {
        var layer = GetLayer();
        var layerName = layer?.GetDisplayName() ?? "(expired)";
        return $"SdfIdentity({layerName}:{_path})";
    }
}