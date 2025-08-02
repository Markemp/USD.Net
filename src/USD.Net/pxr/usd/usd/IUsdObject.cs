using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Map of metadata keys to values, equivalent to C++ std::map&lt;TfToken, VtValue&gt;.
/// </summary>
using UsdMetadataValueMap = Dictionary<TfToken, VtValue>;

/// <summary>
/// Base interface for Usd scenegraph objects, providing common API.
/// </summary>
/// <remarks>
/// The commonality between the three types of scenegraph objects in Usd
/// (UsdPrim, UsdAttribute, UsdRelationship) is that they can all have metadata.
/// Other objects in the API (UsdReferences, UsdVariantSets, etc.) simply are
/// kinds of metadata.
/// 
/// IUsdObject's API primarily provides schema for interacting with the metadata
/// common to all the scenegraph objects, as well as generic access to metadata.
/// </remarks>
public interface IUsdObject
{
    #region Structural and Integrity Info

    /// <summary>
    /// Return true if this is a valid object, false otherwise.
    /// </summary>
    bool IsValid();

    /// <summary>
    /// Return the stage that owns the object, and to whose state and lifetime
    /// this object's validity is tied.
    /// </summary>
    UsdStage? GetStage();

    /// <summary>
    /// Return the complete scene path to this object on its UsdStage,
    /// which may (UsdPrim) or may not (all other subclasses) return a
    /// cached result.
    /// </summary>
    ISdfPath GetPath();

    /// <summary>
    /// Return this object's path if this object is a prim, otherwise this
    /// object's nearest owning prim's path. Equivalent to GetPrim().GetPath().
    /// </summary>
    ISdfPath GetPrimPath();

    /// <summary>
    /// Return this object if it is a prim, otherwise return this object's
    /// nearest owning prim.
    /// </summary>
    IUsdPrim GetPrim();

    /// <summary>
    /// Return the full name of this object, i.e. the last component of its
    /// SdfPath in namespace.
    /// </summary>
    /// <remarks>
    /// This is equivalent to, but generally cheaper than,
    /// GetPath().GetNameToken()
    /// </remarks>
    TfToken GetName();

    /// <summary>
    /// Return a string that provides a brief summary description of the
    /// object. This method, along with IsValid()/bool_operator,
    /// is always safe to call on a possibly-expired object, and the
    /// description will specify whether the object is valid or expired,
    /// along with a few other bits of data.
    /// </summary>
    string GetDescription();

    #endregion

    #region Type Conversion

    /// <summary>
    /// Convert this UsdObject to another object type T if possible. Return
    /// an invalid T instance if this object's dynamic type is not
    /// convertible to T or if this object is invalid.
    /// </summary>
    T As<T>() where T : class, IUsdObject;

    /// <summary>
    /// Return true if this object is convertible to T. This is equivalent
    /// to but cheaper than: bool(obj.As&lt;T&gt;())
    /// </summary>
    bool Is<T>() where T : class, IUsdObject;

    #endregion

    #region Generic Metadata Access

    /// <summary>
    /// Resolve the requested metadatum named key into value,
    /// returning true on success.
    /// </summary>
    /// <returns>
    /// false if key was not resolvable, or if value's
    /// type T differed from that of the resolved metadatum.
    /// </returns>
    /// <remarks>
    /// For any composition-related metadata, as enumerated in
    /// GetAllMetadata(), this method will return only the strongest
    /// opinion found, not applying the composition rules used by Pcp
    /// to process the data. For more processed/composed views of
    /// composition data, please refer to the specific interface classes,
    /// such as UsdReferences, UsdInherits, UsdVariantSets, etc.
    /// </remarks>
    bool GetMetadata<T>(TfToken key, out T value);
    bool GetMetadata(TfToken key, out VtValue value);

    /// <summary>
    /// Set metadatum key's value to value.
    /// </summary>
    /// <returns>
    /// false if value's type does not match the schema type for key.
    /// </returns>
    bool SetMetadata<T>(TfToken key, T value);
    bool SetMetadata(TfToken key, VtValue value);

    /// <summary>
    /// Clears the authored key's value at the current EditTarget,
    /// returning false on error.
    /// </summary>
    /// <remarks>
    /// If no value is present, this method is a no-op and returns true. It is
    /// considered an error to call ClearMetadata when no spec is present for
    /// this UsdObject, i.e. if the object has no presence in the
    /// current UsdEditTarget.
    /// </remarks>
    bool ClearMetadata(TfToken key);

    /// <summary>
    /// Returns true if the key has a meaningful value, that is, if
    /// GetMetadata() will provide a value, either because it was authored
    /// or because a prim's metadata fallback will be provided.
    /// </summary>
    bool HasMetadata(TfToken key);

    /// <summary>
    /// Returns true if the key has an authored value, false if no
    /// value was authored or the only value available is a prim's metadata
    /// fallback.
    /// </summary>
    bool HasAuthoredMetadata(TfToken key);

    /// <summary>
    /// Resolve the requested dictionary sub-element keyPath of
    /// dictionary-valued metadatum named key into value,
    /// returning true on success.
    /// </summary>
    /// <remarks>
    /// If you know you need just a small number of elements from a dictionary,
    /// accessing them element-wise using this method can be much less
    /// expensive than fetching the entire dictionary with GetMetadata(key).
    /// 
    /// The keyPath is a ':'-separated path addressing an element
    /// in subdictionaries.
    /// </remarks>
    bool GetMetadataByDictKey<T>(TfToken key, TfToken keyPath, out T value);
    bool GetMetadataByDictKey(TfToken key, TfToken keyPath, out VtValue value);

    /// <summary>
    /// Author value to the field identified by key and keyPath
    /// at the current EditTarget. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries stored in the metadata field at
    /// key. Return true if the value is authored successfully, false
    /// otherwise.
    /// </summary>
    bool SetMetadataByDictKey<T>(TfToken key, TfToken keyPath, T value);
    bool SetMetadataByDictKey(TfToken key, TfToken keyPath, VtValue value);

    /// <summary>
    /// Clear any authored value identified by key and keyPath
    /// at the current EditTarget. The keyPath is a ':'-separated path
    /// identifying a path in subdictionaries stored in the metadata field at
    /// key. Return true if the value is cleared successfully, false
    /// otherwise.
    /// </summary>
    bool ClearMetadataByDictKey(TfToken key, TfToken keyPath);

    /// <summary>
    /// Return true if there exists any authored or fallback opinion for
    /// key and keyPath. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries stored in the metadata field at
    /// key.
    /// </summary>
    bool HasMetadataDictKey(TfToken key, TfToken keyPath);

    /// <summary>
    /// Return true if there exists any authored opinion (excluding
    /// fallbacks) for key and keyPath. The keyPath is a ':'-separated
    /// path identifying a value in subdictionaries stored in the metadata field
    /// at key.
    /// </summary>
    bool HasAuthoredMetadataDictKey(TfToken key, TfToken keyPath);

    /// <summary>
    /// Resolve and return all metadata (including both authored and
    /// fallback values) on this object, sorted lexicographically.
    /// </summary>
    /// <remarks>
    /// This method does not return field keys for composition arcs, such
    /// as references, inherits, payloads, sublayers, variants, or primChildren,
    /// nor does it return the default value, timeSamples, or spline.
    /// </remarks>
    Dictionary<TfToken, VtValue> GetAllMetadata();

    /// <summary>
    /// Resolve and return all user-authored metadata on this object,
    /// sorted lexicographically.
    /// </summary>
    /// <remarks>
    /// This method does not return field keys for composition arcs, such
    /// as references, inherits, payloads, sublayers, variants, or primChildren,
    /// nor does it return the default value, timeSamples, or spline.
    /// </remarks>
    Dictionary<TfToken, VtValue> GetAllAuthoredMetadata();

    #endregion

    #region Core Metadata Fields

    /// <summary>
    /// Gets the value of the 'hidden' metadata field, false if not
    /// authored.
    /// </summary>
    /// <remarks>
    /// When an object is marked as hidden, it is an indicator to clients who
    /// generically display objects (such as GUI widgets) that this object
    /// should not be included, unless explicitly asked for. Although this
    /// is just a hint and thus up to each application to interpret, we
    /// use it primarily as a way of simplifying hierarchy displays, by
    /// hiding only the representation of the object itself, not its
    /// subtree, instead "pulling up" everything below it one level in the
    /// hierarchical nesting.
    /// 
    /// Note again that this is a hint for UI only - it should not be
    /// interpreted by any renderer as making a prim invisible to drawing.
    /// </remarks>
    bool IsHidden();

    /// <summary>
    /// Sets the value of the 'hidden' metadata field. See IsHidden()
    /// for details.
    /// </summary>
    bool SetHidden(bool hidden);

    /// <summary>
    /// Clears the opinion for "Hidden" at the current EditTarget.
    /// </summary>
    bool ClearHidden();

    /// <summary>
    /// Returns true if hidden was explicitly authored and GetMetadata()
    /// will return a meaningful value for Hidden.
    /// </summary>
    /// <remarks>
    /// Note that IsHidden returns a fallback value (false) when hidden is not
    /// authored.
    /// </remarks>
    bool HasAuthoredHidden();

    /// <summary>
    /// Return this object's composed customData dictionary.
    /// </summary>
    /// <remarks>
    /// CustomData is "custom metadata", a place for applications and users
    /// to put uniform data that is entirely dynamic and subject to no schema
    /// known to Usd. Unlike metadata like 'hidden', 'displayName' etc,
    /// which must be declared in code or a data file that is considered part
    /// of one's Usd distribution (e.g. a plugInfo.json file) to be used,
    /// customData keys and the datatypes of their corresponding values are
    /// ad hoc. No validation will ever be performed that values for the
    /// same key in different layers are of the same type - strongest simply
    /// wins.
    /// 
    /// Dictionaries like customData are composed element-wise, and are
    /// nestable.
    /// 
    /// There is no means to query a customData field's valuetype other
    /// than fetching the value and interrogating it.
    /// </remarks>
    VtDictionary GetCustomData();

    /// <summary>
    /// Return the element identified by keyPath in this object's
    /// composed customData dictionary. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries. This is in general more
    /// efficient than composing the entire customData dictionary and then
    /// pulling out one sub-element.
    /// </summary>
    VtValue GetCustomDataByKey(TfToken keyPath);

    /// <summary>
    /// Author this object's customData dictionary to customData at
    /// the current EditTarget.
    /// </summary>
    void SetCustomData(VtDictionary customData);

    /// <summary>
    /// Author the element identified by keyPath in this object's
    /// customData dictionary at the current EditTarget. The keyPath is a
    /// ':'-separated path identifying a value in subdictionaries.
    /// </summary>
    void SetCustomDataByKey(TfToken keyPath, VtValue value);

    /// <summary>
    /// Clear the authored opinion for this object's customData
    /// dictionary at the current EditTarget. Do nothing if there is no such
    /// authored opinion.
    /// </summary>
    void ClearCustomData();

    /// <summary>
    /// Clear the authored opinion identified by keyPath in this
    /// object's customData dictionary at the current EditTarget. The
    /// keyPath is a ':'-separated path identifying a value in subdictionaries.
    /// Do nothing if there is no such authored opinion.
    /// </summary>
    void ClearCustomDataByKey(TfToken keyPath);

    /// <summary>
    /// Return true if there are any authored or fallback opinions for
    /// this object's customData dictionary, false otherwise.
    /// </summary>
    bool HasCustomData();

    /// <summary>
    /// Return true if there are any authored or fallback opinions for
    /// the element identified by keyPath in this object's customData
    /// dictionary, false otherwise. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries.
    /// </summary>
    bool HasCustomDataKey(TfToken keyPath);

    /// <summary>
    /// Return true if there are any authored opinions (excluding
    /// fallback) for this object's customData dictionary, false otherwise.
    /// </summary>
    bool HasAuthoredCustomData();

    /// <summary>
    /// Return true if there are any authored opinions (excluding
    /// fallback) for the element identified by keyPath in this object's
    /// customData dictionary, false otherwise. The keyPath is a
    /// ':'-separated path identifying a value in subdictionaries.
    /// </summary>
    bool HasAuthoredCustomDataKey(TfToken keyPath);

    /// <summary>
    /// Return this object's composed assetInfo dictionary.
    /// </summary>
    /// <remarks>
    /// The asset info dictionary is used to annotate objects representing the
    /// root-prims of assets (generally organized as models) with various
    /// data related to asset management. For example, asset name, root layer
    /// identifier, asset version etc.
    /// 
    /// The elements of this dictionary are composed element-wise, and are
    /// nestable.
    /// 
    /// There is no means to query an assetInfo field's valuetype other
    /// than fetching the value and interrogating it.
    /// </remarks>
    VtDictionary GetAssetInfo();

    /// <summary>
    /// Return the element identified by keyPath in this object's
    /// composed assetInfo dictionary. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries. This is in general more
    /// efficient than composing the entire assetInfo dictionary than
    /// pulling out one sub-element.
    /// </summary>
    VtValue GetAssetInfoByKey(TfToken keyPath);

    /// <summary>
    /// Author this object's assetInfo dictionary to assetInfo at
    /// the current EditTarget.
    /// </summary>
    void SetAssetInfo(VtDictionary assetInfo);

    /// <summary>
    /// Author the element identified by keyPath in this object's
    /// assetInfo dictionary at the current EditTarget. The keyPath is a
    /// ':'-separated path identifying a value in subdictionaries.
    /// </summary>
    void SetAssetInfoByKey(TfToken keyPath, VtValue value);

    /// <summary>
    /// Clear the authored opinion for this object's assetInfo
    /// dictionary at the current EditTarget. Do nothing if there is no such
    /// authored opinion.
    /// </summary>
    void ClearAssetInfo();

    /// <summary>
    /// Clear the authored opinion identified by keyPath in this
    /// object's assetInfo dictionary at the current EditTarget. The
    /// keyPath is a ':'-separated path identifying a value in subdictionaries.
    /// Do nothing if there is no such authored opinion.
    /// </summary>
    void ClearAssetInfoByKey(TfToken keyPath);

    /// <summary>
    /// Return true if there are any authored or fallback opinions for
    /// this object's assetInfo dictionary, false otherwise.
    /// </summary>
    bool HasAssetInfo();

    /// <summary>
    /// Return true if there are any authored or fallback opinions for
    /// the element identified by keyPath in this object's assetInfo
    /// dictionary, false otherwise. The keyPath is a ':'-separated path
    /// identifying a value in subdictionaries.
    /// </summary>
    bool HasAssetInfoKey(TfToken keyPath);

    /// <summary>
    /// Return true if there are any authored opinions (excluding
    /// fallback) for this object's assetInfo dictionary, false otherwise.
    /// </summary>
    bool HasAuthoredAssetInfo();

    /// <summary>
    /// Return true if there are any authored opinions (excluding
    /// fallback) for the element identified by keyPath in this object's
    /// assetInfo dictionary, false otherwise. The keyPath is a
    /// ':'-separated path identifying a value in subdictionaries.
    /// </summary>
    bool HasAuthoredAssetInfoKey(TfToken keyPath);

    /// <summary>
    /// Return this object's documentation (metadata). This returns the
    /// empty string if no documentation has been set.
    /// </summary>
    string GetDocumentation();

    /// <summary>
    /// Sets this object's documentation (metadata). Returns true on success.
    /// </summary>
    bool SetDocumentation(string doc);

    /// <summary>
    /// Clears this object's documentation (metadata) in the current EditTarget
    /// (only). Returns true on success.
    /// </summary>
    bool ClearDocumentation();

    /// <summary>
    /// Returns true if documentation was explicitly authored and GetMetadata()
    /// will return a meaningful value for documentation.
    /// </summary>
    bool HasAuthoredDocumentation();

    /// <summary>
    /// Return this object's display name (metadata). This returns the
    /// empty string if no display name has been set.
    /// </summary>
    string GetDisplayName();

    /// <summary>
    /// Sets this object's display name (metadata). Returns true on success.
    /// </summary>
    /// <remarks>
    /// DisplayName is meant to be a descriptive label, not necessarily an
    /// alternate identifier; therefore there is no restriction on which
    /// characters can appear in it.
    /// </remarks>
    bool SetDisplayName(string name);

    /// <summary>
    /// Clears this object's display name (metadata) in the current EditTarget
    /// (only). Returns true on success.
    /// </summary>
    bool ClearDisplayName();

    /// <summary>
    /// Returns true if displayName was explicitly authored and GetMetadata()
    /// will return a meaningful value for displayName.
    /// </summary>
    bool HasAuthoredDisplayName();

    #endregion
}