using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPropertySpec represents a property specification in an SDF layer.
/// It stores the scene description for properties including attributes and relationships.
/// </summary>
public class SdfPropertySpec
{
    private readonly ISdfLayer _layer;
    private readonly ISdfPath _path;
    private readonly string _name;
    private string _typeName;
    private VtValue? _defaultValue;
    private UsdVariability _variability = UsdVariability.Varying;
    private readonly Dictionary<string, object> _metadata = new();

    #region Construction

    /// <summary>
    /// Create an invalid property spec.
    /// </summary>
    public SdfPropertySpec()
    {
        _layer = null!;
        _path = SdfPath.EmptyPath();
        _name = string.Empty;
        _typeName = string.Empty;
    }

    /// <summary>
    /// Create a property spec with layer, path, name, and type.
    /// </summary>
    public SdfPropertySpec(ISdfLayer layer, ISdfPath path, string name, string typeName)
    {
        _layer = layer ?? throw new ArgumentNullException(nameof(layer));
        _path = path;
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _typeName = typeName ?? string.Empty;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Get the layer that owns this property spec.
    /// </summary>
    public ISdfLayer GetLayer() => _layer;

    /// <summary>
    /// Get the path of this property spec.
    /// </summary>
    public ISdfPath GetPath() => _path;

    /// <summary>
    /// Get the name of this property.
    /// </summary>
    public string GetName() => _name;

    /// <summary>
    /// Get the type name of this property.
    /// </summary>
    public string GetTypeName() => _typeName;

    /// <summary>
    /// Set the type name of this property.
    /// </summary>
    public void SetTypeName(string typeName)
    {
        _typeName = typeName ?? string.Empty;
    }

    /// <summary>
    /// Get the variability of this property.
    /// </summary>
    public UsdVariability GetVariability() => _variability;

    /// <summary>
    /// Set the variability of this property.
    /// </summary>
    public void SetVariability(UsdVariability variability)
    {
        _variability = variability;
    }

    /// <summary>
    /// Get the default value of this property.
    /// </summary>
    public VtValue? GetDefaultValue() => _defaultValue;

    /// <summary>
    /// Set the default value of this property.
    /// </summary>
    public void SetDefaultValue(VtValue value)
    {
        _defaultValue = value;
    }

    /// <summary>
    /// Return true if this property has a default value.
    /// </summary>
    public bool HasDefaultValue() => _defaultValue != null;

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new property spec in the given layer.
    /// </summary>
    public static SdfPropertySpec New(ISdfLayer layer, ISdfPath path, string name, string typeName)
        => new SdfPropertySpec(layer, path, name, typeName);

    #endregion

    #region Metadata

    /// <summary>
    /// Set metadata for this property.
    /// </summary>
    public void SetMetadata(string key, object value)
    {
        _metadata[key] = value;
    }

    /// <summary>
    /// Get metadata for this property.
    /// </summary>
    public object? GetMetadata(string key)
    {
        return _metadata.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Return true if this property has metadata with the given key.
    /// </summary>
    public bool HasMetadata(string key) => _metadata.ContainsKey(key);

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public void ClearMetadata(string key)
    {
        _metadata.Remove(key);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Return true if this property spec is valid.
    /// </summary>
    public bool IsValid() => _layer != null && !_path.IsEmpty() && !string.IsNullOrEmpty(_name);

    #endregion
}


