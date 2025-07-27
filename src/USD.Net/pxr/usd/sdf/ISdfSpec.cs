using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// Base interface for all Sdf spec classes.
/// </summary>
public interface ISdfSpec : IEquatable<ISdfSpec>, IComparable<ISdfSpec>
{
    #region SdfSpec Generic API

    /// <summary>
    /// Returns the SdfSchemaBase for the layer that owns this spec.
    /// </summary>
    ISdfSchemaBase GetSchema();

    /// <summary>
    /// Returns the SdfSpecType specifying the spec type this object represents.
    /// </summary>
    SdfSpecType GetSpecType();

    /// <summary>
    /// Returns true if this object is invalid or expired.
    /// </summary>
    bool IsDormant();

    /// <summary>
    /// Returns the layer that this object belongs to.
    /// </summary>
    ISdfLayer? GetLayer();

    /// <summary>
    /// Returns the scene path of this object.
    /// </summary>
    ISdfPath GetPath();

    /// <summary>
    /// Returns whether this object's layer can be edited.
    /// </summary>
    bool PermissionToEdit();

    /// <summary>
    /// Returns whether this spec is valid (not dormant).
    /// </summary>
    bool IsValid();

    /// <summary>
    /// Returns the full list of info keys currently set on this object.
    /// <note>This does not include fields that represent names of children.</note>
    /// </summary>
    IReadOnlyList<TfToken> ListInfoKeys();

    /// <summary>
    /// Returns the list of metadata info keys for this object.
    /// 
    /// This is not the complete list of keys, it is only those that
    /// should be considered to be metadata by inspectors or other 
    /// presentation UI.
    /// 
    /// This is interim API which is likely to change. Only editors with
    /// an immediate specific need (like the Inspector) should use this API.
    /// </summary>
    IReadOnlyList<TfToken> GetMetaDataInfoKeys();

    /// <summary>
    /// Returns this metadata key's displayGroup.
    /// </summary>
    TfToken GetMetaDataDisplayGroup(TfToken key);

    /// <summary>
    /// Gets the value for the given metadata key.
    /// 
    /// This is interim API which is likely to change. Only editors with
    /// an immediate specific need (like the Inspector) should use this API.
    /// </summary>
    VtValue GetInfo(TfToken key);

    /// <summary>
    /// Sets the value for the given metadata key.
    /// 
    /// It is an error to pass a value that is not the correct type for
    /// that given key.
    /// 
    /// This is interim API which is likely to change. Only editors with
    /// an immediate specific need (like the Inspector) should use this API.
    /// </summary>
    void SetInfo(TfToken key, VtValue value);

    /// <summary>
    /// Sets the value for entryKey to value within the dictionary 
    /// with the given metadata key dictionaryKey.
    /// </summary>
    void SetInfoDictionaryValue(TfToken dictionaryKey, TfToken entryKey, VtValue value);

    /// <summary>
    /// Returns whether there is a setting for the scene spec info 
    /// with the given key.
    /// 
    /// When asked for a value for one of its scene spec info, a valid value
    /// will always be returned. But if this API returns false for a scene
    /// spec info, the value of that info will be the defined default value.
    /// 
    /// When dealing with a composedLayer, it is not necessary to worry about
    /// whether a scene spec info "has a value" because the composed layer will
    /// always have a valid value, even if it is the default.
    /// 
    /// A spec may or may not have an expressed value for some of its
    /// scene spec info.
    /// 
    /// This is interim API which is likely to change. Only editors with
    /// an immediate specific need (like the Inspector) should use this API.
    /// </summary>
    bool HasInfo(TfToken key);

    /// <summary>
    /// Clears the value for scene spec info with the given key.
    /// 
    /// After calling this, HasInfo() will return false.
    /// To make HasInfo() return true just set a value for that
    /// scene spec info.
    /// 
    /// This is interim API which is likely to change. Only editors with
    /// an immediate specific need (like the Inspector) should use this API.
    /// </summary>
    void ClearInfo(TfToken key);

    /// <summary>
    /// Returns the data type for the info with the given key.
    /// </summary>
    Type GetTypeForInfo(TfToken key);

    /// <summary>
    /// Returns the fallback for the info with the given key.
    /// </summary>
    VtValue GetFallbackForInfo(TfToken key);

    /// <summary>
    /// Writes this spec to the given stream.
    /// </summary>
    bool WriteToStream(TextWriter writer, int indent = 0);

    /// <summary>
    /// Returns whether this object has no significant data.
    /// 
    /// "Significant" here means that the object contributes opinions to
    /// a scene. If this spec has any child scenegraph objects (e.g.,
    /// prim or property spec), it will be considered significant even if
    /// those child objects are not.
    /// However, if ignoreChildren is true, these child objects
    /// will be ignored.
    /// </summary>
    bool IsInert(bool ignoreChildren = false);

    #endregion

    #region Field-based Generic API

    /// <summary>
    /// Returns all fields with values.
    /// </summary>
    IReadOnlyList<TfToken> ListFields();

    /// <summary>
    /// Returns true if the spec has a non-empty value with field name.
    /// </summary>
    bool HasField(TfToken name);

    /// <summary>
    /// Returns true if the object has a non-empty value with name
    /// and type T. If value ptr is provided, returns the value found.
    /// </summary>
    bool HasField<T>(TfToken name, out T? value);

    /// <summary>
    /// Returns a field value by name.
    /// </summary>
    VtValue GetField(TfToken name);

    /// <summary>
    /// Returns a field value by name. If the object is invalid, or the
    /// value doesn't exist, isn't set, or isn't of the given type then
    /// returns defaultValue.
    /// </summary>
    T GetFieldAs<T>(TfToken name, T defaultValue = default!);

    /// <summary>
    /// Sets a field value as a boxed VtValue.
    /// </summary>
    bool SetField(TfToken name, VtValue value);

    /// <summary>
    /// Sets a field value of type T.
    /// </summary>
    bool SetField<T>(TfToken name, T value);

    /// <summary>
    /// Clears a field.
    /// </summary>
    bool ClearField(TfToken name);

    /// <summary>
    /// Alias for ClearField to match some C++ API usage.
    /// </summary>
    bool EraseField(TfToken name);

    #endregion

    /// <summary>
    /// String representation of this spec.
    /// </summary>
    string ToString();
}