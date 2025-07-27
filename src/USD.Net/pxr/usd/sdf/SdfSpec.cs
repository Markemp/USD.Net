using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// Base class for all Sdf spec classes.
/// </summary>
public abstract class SdfSpec : ISdfSpec
{
    private ISdfIdentity? _id;

    #region Construction

    /// <summary>
    /// Default constructor creates an invalid spec.
    /// </summary>
    protected SdfSpec()
    {
        _id = null;
    }

    /// <summary>
    /// Construct a spec with the given identity.
    /// </summary>
    protected SdfSpec(ISdfIdentity identity)
    {
        _id = identity ?? throw new ArgumentNullException(nameof(identity));
    }

    /// <summary>
    /// Construct a spec with the given layer and path.
    /// </summary>
    protected SdfSpec(ISdfLayer layer, ISdfPath path)
    {
        if (layer is null)
            _id = null;
        else
            _id = new SdfIdentity(layer, path);
    }

    #endregion

    #region Core Properties

    /// <summary>
    /// Returns the SdfSchemaBase for the layer that owns this spec.
    /// </summary>
    public ISdfSchemaBase GetSchema()
    {
        var layer = GetLayer();
        return layer?.GetSchema() ?? throw new InvalidOperationException("Cannot get schema from dormant spec");
    }

    /// <summary>
    /// Returns the SdfSpecType specifying the spec type this object represents.
    /// </summary>
    public SdfSpecType GetSpecType()
    {
        // We can't retrieve an object type for a dormant spec.
        if (_id is null)
            return SdfSpecType.Unknown;

        var layer = _id.GetLayer();
        var path = _id.GetPath();

        return layer?.GetSpecType(path) ?? SdfSpecType.Unknown;
    }

    /// <summary>
    /// Returns true if this object is invalid or expired.
    /// </summary>
    public bool IsDormant()
    {
        // If we have no id, we're dormant.
        if (_id is null)
            return true;

        // If our path is invalid, we must be dormant.
        var path = _id.GetPath();
        if (path.IsEmpty())
            return true;

        // If our layer is invalid, we're dormant. Otherwise we're dormant if the
        // layer has no spec at this path.
        var layer = _id.GetLayer();
        return layer is null || !layer.HasSpec(path);
    }

    /// <summary>
    /// Returns the layer that this object belongs to.
    /// </summary>
    public ISdfLayer? GetLayer() => _id?.GetLayer();

    /// <summary>
    /// Returns the scene path of this object.
    /// </summary>
    public ISdfPath GetPath() => _id?.GetPath() ?? SdfPath.EmptyPath();

    /// <summary>
    /// Returns whether this object's layer can be edited.
    /// </summary>
    public bool PermissionToEdit()
    {
        var layer = GetLayer();
        return layer?.PermissionToEdit() ?? false;
    }

    /// <summary>
    /// Returns whether this spec is valid (not dormant).
    /// </summary>
    public bool IsValid() => !IsDormant();

    #endregion

    #region Field-based Generic API

    /// <summary>
    /// Returns all fields with values.
    /// </summary>
    public List<TfToken> ListFields()
    {
        if (_id is null)
            return [];

        var layer = GetLayer();
        return layer?.ListFields(_id.GetPath()) ?? new List<TfToken>();
    }

    /// <summary>
    /// Returns true if the spec has a non-empty value with field name.
    /// </summary>
    public bool HasField(TfToken name)
    {
        if (_id is null)
            return false;

        var layer = GetLayer();
        return layer?.HasField(_id.GetPath(), name) ?? false;
    }

    /// <summary>
    /// Returns true if the object has a non-empty value with name and type T.
    /// If value is provided, returns the value found.
    /// </summary>
    public bool HasField<T>(TfToken name, out T? value)
    {
        value = default;

        if (!HasField(name))
            return false;

        var vtValue = GetField(name);
        if (vtValue.IsHolding<T>())
        {
            value = vtValue.Get<T>();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a field value by name.
    /// </summary>
    public VtValue GetField(TfToken name)
    {
        if (_id is null)
            return VtValue.CreateEmpty();

        var layer = GetLayer();
        return layer?.GetField(_id.GetPath(), name) ?? VtValue.CreateEmpty();
    }

    /// <summary>
    /// Returns a field value by name. If the object is invalid, or the
    /// value doesn't exist, isn't set, or isn't of the given type then
    /// returns defaultValue.
    /// </summary>
    public T GetFieldAs<T>(TfToken name, T defaultValue = default!)
    {
        var value = GetField(name);
        if (value.IsEmpty() || !value.IsHolding<T>())
            return defaultValue;

        return value.Get<T>();
    }

    /// <summary>
    /// Sets a field value as a boxed VtValue.
    /// </summary>
    public bool SetField(TfToken name, VtValue value)
    {
        if (_id == null)
            return false;

        var layer = GetLayer();
        if (layer == null)
            return false;

        layer.SetField(_id.GetPath(), name, value);
        return true;
    }

    /// <summary>
    /// Sets a field value of type T.
    /// </summary>
    public bool SetField<T>(TfToken name, T value)
    {
        return SetField(name, new VtValue(value));
    }

    /// <summary>
    /// Clears a field.
    /// </summary>
    public bool ClearField(TfToken name)
    {
        if (_id == null)
            return false;

        var layer = GetLayer();
        if (layer == null)
            return false;

        layer.EraseField(_id.GetPath(), name);
        return true;
    }

    /// <summary>
    /// Alias for ClearField to match some C++ API usage.
    /// </summary>
    public bool EraseField(TfToken name) => ClearField(name);

    #endregion

    #region Info API (Metadata)

    /// <summary>
    /// Returns the full list of info keys currently set on this object.
    /// Note: This does not include fields that represent names of children.
    /// </summary>
    public List<TfToken> ListInfoKeys()
    {
        var schema = GetSchema();
        var specDef = schema.GetSpecDefinition(GetSpecType());
        if (specDef == null)
            return new List<TfToken>();

        var result = new List<TfToken>();
        var valueFields = specDef.GetFields();

        foreach (var field in valueFields)
        {
            // Skip fields holding children.
            var fieldDef = schema.GetFieldDefinition(field);
            if (fieldDef?.HoldsChildren() == true)
                continue;

            if (HasInfo(field))
            {
                result.Add(field);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the list of metadata info keys for this object.
    /// This is not the complete list of keys, it is only those that
    /// should be considered to be metadata by inspectors or other presentation UI.
    /// </summary>
    public List<TfToken> GetMetaDataInfoKeys()
    {
        return GetSchema().GetMetadataFields(GetSpecType());
    }

    /// <summary>
    /// Returns this metadata key's displayGroup.
    /// </summary>
    public TfToken GetMetaDataDisplayGroup(TfToken key)
    {
        var specDef = GetSchema().GetSpecDefinition(GetSpecType());
        return specDef?.GetMetadataFieldDisplayGroup(key) ?? TfToken.Empty;
    }

    /// <summary>
    /// Gets the value for the given metadata key.
    /// </summary>
    public VtValue GetInfo(TfToken key)
    {
        var fieldDef = GetSchema().GetFieldDefinition(key);
        if (fieldDef == null)
        {
            // TODO: Add proper error reporting
            return VtValue.CreateEmpty();
        }

        var value = GetField(key);
        return !value.IsEmpty() ? value : fieldDef.GetFallbackValue();
    }

    /// <summary>
    /// Sets the value for the given metadata key.
    /// It is an error to pass a value that is not the correct type for that given key.
    /// </summary>
    public void SetInfo(TfToken key, VtValue value)
    {
        // Perform some validation on the field being modified
        var schema = GetSchema();
        var fieldDef = schema.GetFieldDefinition(key);
        if (!CanEditInfoOnSpec(key, GetSpecType(), schema, fieldDef, "set"))
        {
            return;
        }

        // Attempt to cast the given value to the type specified for the field in the schema
        var fallback = fieldDef!.GetFallbackValue();
        var castValue = fallback.IsEmpty() ? value : VtValue.CastToTypeOf(value, fallback);

        if (castValue.IsEmpty())
        {
            // TODO: Add proper error reporting
            return;
        }

        SetField(key, castValue);
    }

    /// <summary>
    /// Sets the value for entryKey to value within the dictionary 
    /// with the given metadata key dictionaryKey.
    /// </summary>
    public void SetInfoDictionaryValue(TfToken dictionaryKey, TfToken entryKey, VtValue value)
    {
        // Get the current dictionary
        var dictValue = GetInfo(dictionaryKey);
        VtDictionary dict;

        if (dictValue.IsHolding<VtDictionary>())
        {
            dict = dictValue.Get<VtDictionary>();
        }
        else
        {
            dict = new VtDictionary();
        }

        // Modify the dictionary
        if (value.IsEmpty())
        {
            dict.Remove(entryKey);
        }
        else
        {
            dict[entryKey] = value;
        }

        // Set the modified dictionary back
        SetInfo(dictionaryKey, new VtValue(dict));
    }

    /// <summary>
    /// Returns whether there is a setting for the scene spec info with the given key.
    /// </summary>
    public bool HasInfo(TfToken key)
    {
        // It's not an error to call this method with a key that isn't registered
        // with the schema. The file writer needs to be able to query for the
        // presence of metadata fields registered via plugins.
        return HasField(key);
    }

    /// <summary>
    /// Clears the value for scene spec info with the given key.
    /// After calling this, HasInfo() will return false.
    /// </summary>
    public void ClearInfo(TfToken key)
    {
        // Perform some validation to ensure we allow the clearing of this field
        var schema = GetSchema();
        var fieldDef = schema.GetFieldDefinition(key);

        // A field without a definition may still exist as an unknown field.
        if (fieldDef is not null)
        {
            if (!CanEditInfoOnSpec(key, GetSpecType(), schema, fieldDef, "clear"))
                return;
        }

        // TODO: Implement SdfChangeBlock equivalent
        ClearField(key);

        // TODO: Implement cleanup tracking
        // In case this spec is made inert when the info is removed, schedule it to
        // be cleaned up (if the caller has enabled cleanup tracking)
    }

    /// <summary>
    /// Returns the data type for the info with the given key.
    /// </summary>
    public Type GetTypeForInfo(TfToken key)
    {
        var fallback = GetSchema().GetFallback(key);
        return fallback.GetType();
    }

    /// <summary>
    /// Returns the fallback for the info with the given key.
    /// </summary>
    public VtValue GetFallbackForInfo(TfToken key)
    {
        var schema = GetSchema();
        var fieldDef = schema.GetFieldDefinition(key);
        if (fieldDef == null)
        {
            // TODO: Add proper error reporting
            return VtValue.CreateEmpty();
        }

        var specType = GetSpecType();
        var specDef = schema.GetSpecDefinition(specType);
        if (specDef == null || !specDef.IsMetadataField(key))
        {
            // TODO: Add proper error reporting
            return VtValue.CreateEmpty();
        }

        return fieldDef.GetFallbackValue();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Writes this spec to the given stream.
    /// </summary>
    public bool WriteToStream(TextWriter writer, int indent = 0)
    {
        var layer = GetLayer();
        var fileFormat = layer?.GetFileFormat();
        return fileFormat?.WriteToStream(this, writer, indent) ?? false;
    }

    /// <summary>
    /// Returns whether this object has no significant data.
    /// "Significant" here means that the object contributes opinions to a scene.
    /// </summary>
    public bool IsInert(bool ignoreChildren = false)
    {
        if (_id == null)
            return false;

        var layer = GetLayer();
        return layer?._IsInert(_id.GetPath(), ignoreChildren) ?? false;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Helper function that performs common checks to determine if a given
    /// field is editable via the Info API.
    /// </summary>
    private static bool CanEditInfoOnSpec(
        TfToken key,
        SdfSpecType specType,
        ISdfSchemaBase schema,
        ISdfFieldDefinition? fieldDef,
        string editType)
    {
        if (fieldDef == null)
        {
            // TODO: Add proper error reporting
            return false;
        }

        if (fieldDef.IsReadOnly())
        {
            // TODO: Add proper error reporting
            return false;
        }

        if (!schema.IsValidFieldForSpec(fieldDef.GetName(), specType))
        {
            // TODO: Add proper error reporting
            return false;
        }

        return true;
    }

    /// <summary>
    /// Move this spec from oldPath to newPath.
    /// </summary>
    protected bool MoveSpec(ISdfPath oldPath, ISdfPath newPath)
    {
        var layer = GetLayer();
        return layer?._MoveSpec(oldPath, newPath) ?? false;
    }

    /// <summary>
    /// Delete the spec at the given path.
    /// </summary>
    protected static bool DeleteSpec(ISdfLayer layer, ISdfPath path)
    {
        return layer._DeleteSpec(path);
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Equality operator.
    /// </summary>
    public bool Equals(SdfSpec? other)
    {
        if (ReferenceEquals(other, null))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return Equals(_id, other._id);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return Equals(obj as SdfSpec);
    }

    /// <summary>
    /// Comparison operator for ordering.
    /// </summary>
    public int CompareTo(SdfSpec? other)
    {
        if (ReferenceEquals(other, null))
            return 1;
        if (ReferenceEquals(this, other))
            return 0;

        // Compare by identity
        return Comparer<ISdfIdentity?>.Default.Compare(_id, other._id);
    }

    /// <summary>
    /// Hash code.
    /// </summary>
    public override int GetHashCode()
    {
        return _id?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(SdfSpec? left, SdfSpec? right)
    {
        if (ReferenceEquals(left, null))
            return ReferenceEquals(right, null);

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(SdfSpec? left, SdfSpec? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    public static bool operator <(SdfSpec? left, SdfSpec? right)
    {
        if (ReferenceEquals(left, null))
            return !ReferenceEquals(right, null);

        return left.CompareTo(right) < 0;
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    public static bool operator >(SdfSpec? left, SdfSpec? right)
    {
        return right < left;
    }

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    public static bool operator <=(SdfSpec? left, SdfSpec? right)
    {
        return left < right || left == right;
    }

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    public static bool operator >=(SdfSpec? left, SdfSpec? right)
    {
        return left > right || left == right;
    }

    #endregion

    #region String Representation

    /// <summary>
    /// String representation of this spec.
    /// </summary>
    public override string ToString()
    {
        if (IsDormant())
            return $"{GetType().Name}(dormant)";

        return $"{GetType().Name}({GetPath()})";
    }

    IReadOnlyList<TfToken> ISdfSpec.ListInfoKeys()
    {
        return ListInfoKeys();
    }

    IReadOnlyList<TfToken> ISdfSpec.GetMetaDataInfoKeys()
    {
        return GetMetaDataInfoKeys();
    }

    IReadOnlyList<TfToken> ISdfSpec.ListFields()
    {
        return ListFields();
    }

    public bool Equals(ISdfSpec? other)
    {
        throw new NotImplementedException();
    }

    public int CompareTo(ISdfSpec? other)
    {
        throw new NotImplementedException();
    }

    #endregion
}