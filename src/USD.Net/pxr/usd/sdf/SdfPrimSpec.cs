using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPrimSpec represents a prim spec in an SdfLayer.
/// It stores the scene description for a prim, including its type, metadata, and child specs.
/// </summary>
/// <remarks>
/// Represents a prim description in an SdfLayer object.
///
/// Every SdfPrimSpec object is defined in a layer.  It is identified by its
/// path (SdfPath class) in the namespace hierarchy of its layer.  SdfPrimSpecs
/// can be created using the New() method as children of either the containing
/// SdfLayer itself (for "root level" prims), or as children of other 
/// SdfPrimSpec objects to extend a hierarchy.  The helper function 
/// SdfCreatePrimInLayer() can be used to quickly create a hierarchy of
/// primSpecs.
///
/// SdfPrimSpec objects have properties of two general types: attributes
/// (containing values) and relationships (different types of connections to
/// other prims and attributes).  Attributes are represented by the
/// SdfAttributeSpec class and relationships by the SdfRelationshipSpec class.
/// Each prim has its own namespace of properties.  Properties are stored and
/// accessed by their name.
///
/// SdfPrimSpec objects have a typeName, permission restriction, and they
/// reference and inherit prim paths.  Permission restrictions control which
/// other layers may refer to, or express opinions about a prim. See the
/// SdfPermission class for more information.
///
/// \todo
/// \li Insert doc about references and inherits here.
/// \li Should have validate... methods for name, children, properties
/// </remarks>
public class SdfPrimSpec
{
    private readonly ISdfLayer _layer;
    private readonly ISdfPath _path;
    private readonly Dictionary<string, object> _metadata = new();
    private readonly Dictionary<string, SdfPropertySpec> _properties = new();
    private readonly List<SdfPrimSpec> _children = new();
    private string _typeName = string.Empty;
    private string _specifier = "def"; // def, over, class

    #region Construction

    /// <summary>
    /// Create an invalid prim spec.
    /// </summary>
    public SdfPrimSpec()
    {
        _layer = null!;
        _path = SdfPath.EmptyPath();
    }

    /// <summary>
    /// Create a prim spec with the given layer and path.
    /// </summary>
    public SdfPrimSpec(ISdfLayer layer, ISdfPath path, string specifier = "def")
    {
        _layer = layer ?? throw new ArgumentNullException(nameof(layer));
        _path = path;
        _specifier = specifier;
    }

    #endregion

    #region Basic Properties

    /// <summary>
    /// Get the layer that owns this prim spec.
    /// </summary>
    public ISdfLayer GetLayer() => _layer;

    /// <summary>
    /// Get the path of this prim spec.
    /// </summary>
    public ISdfPath GetPath() => _path;

    /// <summary>
    /// Get the name of this prim spec (the final path component).
    /// </summary>
    public string GetName() => _path.GetName();

    /// <summary>
    /// Return true if this prim spec is valid.
    /// </summary>
    public bool IsValid() => _layer != null && !_path.IsEmpty();

    #endregion

    #region Type and Specifier

    /// <summary>
    /// Get the type name of this prim spec.
    /// </summary>
    public string GetTypeName() => _typeName;

    /// <summary>
    /// Set the type name of this prim spec.
    /// </summary>
    public void SetTypeName(string typeName)
    {
        _typeName = typeName ?? string.Empty;
    }

    /// <summary>
    /// Return true if this prim spec has a type name.
    /// </summary>
    public bool HasTypeName() => !string.IsNullOrEmpty(_typeName);

    /// <summary>
    /// Get the specifier of this prim spec (def, over, class).
    /// </summary>
    public string GetSpecifier() => _specifier;

    /// <summary>
    /// Set the specifier of this prim spec.
    /// </summary>
    public void SetSpecifier(string specifier)
    {
        _specifier = specifier ?? "def";
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Set metadata for this prim spec.
    /// </summary>
    public void SetMetadata(TfToken key, VtValue value)
    {
        if (key.IsEmpty)
            return;

        _metadata[key.GetText()] = value.GetValue() ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Get metadata from this prim spec.
    /// </summary>
    public VtValue GetMetadata(TfToken key)
    {
        if (key.IsEmpty)
            return VtValue.CreateEmpty();

        return _metadata.TryGetValue(key.GetText(), out var value)
            ? new VtValue(value)
            : VtValue.CreateEmpty();
    }

    /// <summary>
    /// Return true if this prim spec has metadata with the given key.
    /// </summary>
    public bool HasMetadata(TfToken key)
    {
        if (key.IsEmpty)
            return false;

        return _metadata.ContainsKey(key.GetText());
    }

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public void ClearMetadata(TfToken key)
    {
        if (!key.IsEmpty)
            _metadata.Remove(key.GetText());
    }

    /// <summary>
    /// Get all metadata keys for this prim spec.
    /// </summary>
    public IEnumerable<TfToken> GetMetadataKeys()
        => _metadata.Keys.Select(key => new TfToken(key));

    #endregion

    #region Property Management

    /// <summary>
    /// Create a property spec with the given name and type.
    /// </summary>
    public SdfPropertySpec CreateProperty(string name, string typeName)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Property name cannot be null or empty", nameof(name));

        var propertyPath = _path.AppendProperty(name);
        var propertySpec = new SdfPropertySpec(_layer, propertyPath, name, typeName);
        _properties[name] = propertySpec;
        return propertySpec;
    }

    /// <summary>
    /// Get a property spec by name.
    /// </summary>
    public SdfPropertySpec? GetPropertyByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        return _properties.TryGetValue(name, out var property) ? property : null;
    }

    /// <summary>
    /// Return true if this prim spec has a property with the given name.
    /// </summary>
    public bool HasProperty(string name)
    {
        return !string.IsNullOrEmpty(name) && _properties.ContainsKey(name);
    }

    /// <summary>
    /// Remove a property spec by name.
    /// </summary>
    public bool RemoveProperty(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        return _properties.Remove(name);
    }

    /// <summary>
    /// Get all property specs for this prim spec.
    /// </summary>
    public IEnumerable<SdfPropertySpec> GetProperties()
    {
        return _properties.Values;
    }

    /// <summary>
    /// Get all property names for this prim spec.
    /// </summary>
    public IEnumerable<string> GetPropertyNames()
    {
        return _properties.Keys;
    }

    #endregion

    #region Child Prim Management

    /// <summary>
    /// Create a child prim spec with the given name.
    /// </summary>
    public SdfPrimSpec CreateChildPrim(string name, string specifier = "def")
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Child name cannot be null or empty", nameof(name));

        var childPath = _path.AppendChild(name);
        var childSpec = new SdfPrimSpec(_layer, childPath, specifier);
        _children.Add(childSpec);
        return childSpec;
    }

    /// <summary>
    /// Add an existing child prim spec to this prim.
    /// </summary>
    public void AddChild(SdfPrimSpec childSpec)
    {
        if (childSpec?.IsValid() == true && !_children.Contains(childSpec))
        {
            _children.Add(childSpec);
        }
    }

    /// <summary>
    /// Get a child prim spec by name.
    /// </summary>
    public SdfPrimSpec? GetChildByName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        return _children.FirstOrDefault(child => child.GetName() == name);
    }

    /// <summary>
    /// Return true if this prim spec has a child with the given name.
    /// </summary>
    public bool HasChild(string name)
    {
        return GetChildByName(name) != null;
    }

    /// <summary>
    /// Remove a child prim spec by name.
    /// </summary>
    public bool RemoveChild(string name)
    {
        var child = GetChildByName(name);
        if (child == null)
            return false;

        return _children.Remove(child);
    }

    /// <summary>
    /// Get all child prim specs for this prim spec.
    /// </summary>
    public IEnumerable<SdfPrimSpec> GetChildren()
    {
        return _children;
    }

    /// <summary>
    /// Get all child names for this prim spec.
    /// </summary>
    public IEnumerable<string> GetChildNames()
    {
        return _children.Select(child => child.GetName());
    }

    #endregion

    #region Hierarchy Navigation

    /// <summary>
    /// Get the parent prim spec of this prim spec.
    /// </summary>
    public SdfPrimSpec? GetParent()
    {
        var parentPath = _path.GetParentPath();
        if (parentPath.IsEmpty() || parentPath.IsAbsoluteRootPath())
            return null;

        // TODO: This would need layer integration to find the parent spec
        // For now, return null as this requires more complex layer management
        return null;
    }

    /// <summary>
    /// Get the root prim spec of the layer.
    /// </summary>
    public SdfPrimSpec? GetRoot()
    {
        // TODO: This would need layer integration to find the root spec
        return null;
    }

    #endregion

    #region Active and Visibility

    /// <summary>
    /// Get the active flag for this prim spec.
    /// </summary>
    public bool? GetActive()
    {
        if (_metadata.TryGetValue("active", out var value) && value is bool boolValue)
            return boolValue;

        return null; // No authored opinion
    }

    /// <summary>
    /// Set the active flag for this prim spec.
    /// </summary>
    public void SetActive(bool active)
    {
        _metadata["active"] = active;
    }

    /// <summary>
    /// Clear the active flag for this prim spec.
    /// </summary>
    public void ClearActive()
    {
        _metadata.Remove("active");
    }

    /// <summary>
    /// Return true if this prim spec has an authored active opinion.
    /// </summary>
    public bool HasActive()
    {
        return _metadata.ContainsKey("active");
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this prim spec.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "SdfPrimSpec(invalid)";

        var typeName = HasTypeName() ? $" : {GetTypeName()}" : "";
        return $"SdfPrimSpec({GetSpecifier()} \"{GetPath().GetString()}\"{typeName})";
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new prim spec in the given layer at the specified path.
    /// </summary>
    public static SdfPrimSpec CreatePrimSpec(SdfLayer layer, SdfPath path, string specifier = "def")
    {
        return new SdfPrimSpec(layer, path, specifier);
    }

    /// <summary>
    /// Create a new prim spec in the given layer with the specified name as a child of the root.
    /// </summary>
    public static SdfPrimSpec CreatePrimSpec(SdfLayer layer, string name, string specifier = "def")
    {
        var path = new SdfPath("/" + name);
        return new SdfPrimSpec(layer, path, specifier);
    }

    /// <summary>
    /// Create a new prim spec with layer, path, and name.
    /// </summary>
    public static SdfPrimSpec New(SdfLayer layer, SdfPath path, string name)
    {
        return new SdfPrimSpec(layer, path, "def");
    }

    #endregion
}