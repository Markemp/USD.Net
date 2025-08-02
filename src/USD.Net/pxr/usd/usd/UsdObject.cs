using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all USD objects that can exist on a stage.
/// </summary>
/// <remarks>
/// Provides common functionality and shared implementation for all USD scene graph objects
/// including prims, attributes, and relationships. This abstract class implements the
/// IUsdObject interface with default behavior that can be overridden by derived classes.
/// </remarks>
public abstract class UsdObject : IUsdObject
{
    private readonly UsdStage? _stage;
    private readonly ISdfPath _path;
    private readonly Dictionary<TfToken, VtValue> _metadata = new();

    #region Construction

    /// <summary>
    /// Protected constructor for invalid USD objects.
    /// </summary>
    protected UsdObject() : this(null, SdfPath.EmptyPath())
    {
    }

    /// <summary>
    /// Protected constructor for USD objects with stage and path.
    /// </summary>
    protected UsdObject(UsdStage? stage, ISdfPath path)
    {
        _stage = stage;
        _path = path ?? SdfPath.EmptyPath();
    }

    #endregion

    #region Structural and Integrity Info

    public virtual bool IsValid() => _stage is not null && !_path.IsEmpty();

    public virtual UsdStage? GetStage() => _stage;

    public virtual ISdfPath GetPath() => _path;

    public virtual ISdfPath GetPrimPath()
    {
        // For prims, return the path directly; for properties, return the prim path
        return _path.IsPropertyPath() ? _path.GetPrimPath() : _path;
    }

    public virtual IUsdPrim GetPrim()
    {
        if (!IsValid())
            return (IUsdPrim)(new UsdPrim()); // Invalid prim

        var primPath = GetPrimPath();
        return (IUsdPrim)_stage!.GetPrimAtPath(primPath);
    }

    public virtual TfToken GetName() => new(_path.GetName());

    public abstract string GetDescription();

    #endregion

    #region Type Conversion

    public virtual T As<T>() where T : class, IUsdObject => this is T result ? result : default!;

    public virtual bool Is<T>() where T : class, IUsdObject => this is T;

    #endregion

    #region Static Methods
    
    /// <summary>
    /// Return the namespace delimiter used to separate namespaces in property names.
    /// </summary>
    public static char GetNamespaceDelimiter() => ':';
    
    #endregion

    #region Generic Metadata Access

    public virtual bool GetMetadata<T>(TfToken key, out T value)
    {
        if (_metadata.TryGetValue(key, out var vtValue))
        {
            value = vtValue.Get<T>();
            return true;
        }
        value = default!;
        return false;
    }

    public virtual bool GetMetadata(TfToken key, out VtValue value)
    {
        return _metadata.TryGetValue(key, out value!);
    }

    public virtual bool SetMetadata<T>(TfToken key, T value)
    {
        if (!IsValid())
            return false;

        _metadata[key] = new VtValue(value);
        return true;
    }

    public virtual bool SetMetadata(TfToken key, VtValue value)
    {
        if (!IsValid())
            return false;

        _metadata[key] = value;
        return true;
    }

    public virtual bool ClearMetadata(TfToken key)
    {
        if (!IsValid())
            return false;

        return _metadata.Remove(key);
    }

    public virtual bool HasMetadata(TfToken key)
    {
        return _metadata.ContainsKey(key);
    }

    public virtual bool HasAuthoredMetadata(TfToken key)
    {
        // For now, same as HasMetadata - in full implementation would check authoring
        return HasMetadata(key);
    }

    public virtual bool GetMetadataByDictKey<T>(TfToken key, TfToken keyPath, out T value)
    {
        value = default!;
        if (!GetMetadata(key, out VtValue dictValue) || !dictValue.IsHolding<VtDictionary>())
            return false;

        var dict = dictValue.Get<VtDictionary>();
        return GetValueFromDictPath(dict, keyPath.ToString(), out value);
    }

    public virtual bool GetMetadataByDictKey(TfToken key, TfToken keyPath, out VtValue value)
    {
        value = VtValue.Empty;
        if (!GetMetadata(key, out VtValue dictValue) || !dictValue.IsHolding<VtDictionary>())
            return false;

        var dict = dictValue.Get<VtDictionary>();
        return GetValueFromDictPath(dict, keyPath.ToString(), out value);
    }

    public virtual bool SetMetadataByDictKey<T>(TfToken key, TfToken keyPath, T value)
    {
        return SetMetadataByDictKey(key, keyPath, new VtValue(value));
    }

    public virtual bool SetMetadataByDictKey(TfToken key, TfToken keyPath, VtValue value)
    {
        if (!IsValid())
            return false;

        // Get or create the dictionary
        if (!_metadata.TryGetValue(key, out var dictValue) || !dictValue.IsHolding<VtDictionary>())
        {
            dictValue = new VtValue(new VtDictionary());
            _metadata[key] = dictValue;
        }

        var dict = dictValue.Get<VtDictionary>();
        SetValueInDictPath(dict, keyPath.ToString(), value);
        return true;
    }

    public virtual bool ClearMetadataByDictKey(TfToken key, TfToken keyPath)
    {
        if (!GetMetadata(key, out VtValue dictValue) || !dictValue.IsHolding<VtDictionary>())
            return false;

        var dict = dictValue.Get<VtDictionary>();
        return RemoveValueFromDictPath(dict, keyPath.ToString());
    }

    public virtual bool HasMetadataDictKey(TfToken key, TfToken keyPath)
    {
        if (!GetMetadata(key, out VtValue dictValue) || !dictValue.IsHolding<VtDictionary>())
            return false;

        var dict = dictValue.Get<VtDictionary>();
        return HasValueInDictPath(dict, keyPath.ToString());
    }

    public virtual bool HasAuthoredMetadataDictKey(TfToken key, TfToken keyPath)
    {
        // For now, same as HasMetadataDictKey
        return HasMetadataDictKey(key, keyPath);
    }

    public virtual Dictionary<TfToken, VtValue> GetAllMetadata()
    {
        return new Dictionary<TfToken, VtValue>(_metadata);
    }

    public virtual Dictionary<TfToken, VtValue> GetAllAuthoredMetadata()
    {
        // For now, same as GetAllMetadata
        return GetAllMetadata();
    }

    #endregion

    #region Core Metadata Fields

    public virtual bool IsHidden()
    {
        return GetMetadata(new TfToken("hidden"), out bool hidden) && hidden;
    }

    public virtual bool SetHidden(bool hidden)
    {
        return SetMetadata(new TfToken("hidden"), hidden);
    }

    public virtual bool ClearHidden()
    {
        return ClearMetadata(new TfToken("hidden"));
    }

    public virtual bool HasAuthoredHidden()
    {
        return HasAuthoredMetadata(new TfToken("hidden"));
    }

    public virtual VtDictionary GetCustomData()
    {
        if (GetMetadata(new TfToken("customData"), out VtDictionary customData))
            return customData;
        return new VtDictionary();
    }

    public virtual VtValue GetCustomDataByKey(TfToken keyPath)
    {
        GetMetadataByDictKey(new TfToken("customData"), keyPath, out VtValue value);
        return value;
    }

    public virtual void SetCustomData(VtDictionary customData)
    {
        SetMetadata(new TfToken("customData"), customData);
    }

    public virtual void SetCustomDataByKey(TfToken keyPath, VtValue value)
    {
        SetMetadataByDictKey(new TfToken("customData"), keyPath, value);
    }

    public virtual void ClearCustomData()
    {
        ClearMetadata(new TfToken("customData"));
    }

    public virtual void ClearCustomDataByKey(TfToken keyPath)
    {
        ClearMetadataByDictKey(new TfToken("customData"), keyPath);
    }

    public virtual bool HasCustomData()
    {
        return HasMetadata(new TfToken("customData"));
    }

    public virtual bool HasCustomDataKey(TfToken keyPath)
    {
        return HasMetadataDictKey(new TfToken("customData"), keyPath);
    }

    public virtual bool HasAuthoredCustomData()
    {
        return HasAuthoredMetadata(new TfToken("customData"));
    }

    public virtual bool HasAuthoredCustomDataKey(TfToken keyPath)
    {
        return HasAuthoredMetadataDictKey(new TfToken("customData"), keyPath);
    }

    public virtual VtDictionary GetAssetInfo()
    {
        if (GetMetadata(new TfToken("assetInfo"), out VtDictionary assetInfo))
            return assetInfo;
        return new VtDictionary();
    }

    public virtual VtValue GetAssetInfoByKey(TfToken keyPath)
    {
        GetMetadataByDictKey(new TfToken("assetInfo"), keyPath, out VtValue value);
        return value;
    }

    public virtual void SetAssetInfo(VtDictionary assetInfo)
    {
        SetMetadata(new TfToken("assetInfo"), assetInfo);
    }

    public virtual void SetAssetInfoByKey(TfToken keyPath, VtValue value)
    {
        SetMetadataByDictKey(new TfToken("assetInfo"), keyPath, value);
    }

    public virtual void ClearAssetInfo()
    {
        ClearMetadata(new TfToken("assetInfo"));
    }

    public virtual void ClearAssetInfoByKey(TfToken keyPath)
    {
        ClearMetadataByDictKey(new TfToken("assetInfo"), keyPath);
    }

    public virtual bool HasAssetInfo()
    {
        return HasMetadata(new TfToken("assetInfo"));
    }

    public virtual bool HasAssetInfoKey(TfToken keyPath)
    {
        return HasMetadataDictKey(new TfToken("assetInfo"), keyPath);
    }

    public virtual bool HasAuthoredAssetInfo()
    {
        return HasAuthoredMetadata(new TfToken("assetInfo"));
    }

    public virtual bool HasAuthoredAssetInfoKey(TfToken keyPath)
    {
        return HasAuthoredMetadataDictKey(new TfToken("assetInfo"), keyPath);
    }

    public virtual string GetDocumentation()
    {
        if (GetMetadata(new TfToken("documentation"), out string doc))
            return doc;
        return string.Empty;
    }

    public virtual bool SetDocumentation(string doc)
    {
        return SetMetadata(new TfToken("documentation"), doc);
    }

    public virtual bool ClearDocumentation()
    {
        return ClearMetadata(new TfToken("documentation"));
    }

    public virtual bool HasAuthoredDocumentation()
    {
        return HasAuthoredMetadata(new TfToken("documentation"));
    }

    public virtual string GetDisplayName()
    {
        if (GetMetadata(new TfToken("displayName"), out string name))
            return name;
        return string.Empty;
    }

    public virtual bool SetDisplayName(string name)
    {
        return SetMetadata(new TfToken("displayName"), name);
    }

    public virtual bool ClearDisplayName()
    {
        return ClearMetadata(new TfToken("displayName"));
    }

    public virtual bool HasAuthoredDisplayName()
    {
        return HasAuthoredMetadata(new TfToken("displayName"));
    }

    #endregion

    #region Object Identity and Comparison

    /// <summary>
    /// Return true if this object has the same stage and path as another object.
    /// </summary>
    public virtual bool IsSameAs(UsdObject other)
    {
        if (other is null)
            return false;
            
        return ReferenceEquals(_stage, other._stage) && _path.Equals(other._path);
    }

    /// <summary>
    /// Return the string representation of this object's path.
    /// </summary>
    public override string ToString() => _path.GetString();

    public override bool Equals(object? obj) => obj is UsdObject other && IsSameAs(other);

    public override int GetHashCode() 
        => HashCode.Combine(_stage?.GetHashCode() ?? 0, _path.GetHashCode());

    #endregion

    #region Helper Methods for Dictionary Navigation

    private static bool GetValueFromDictPath<T>(VtDictionary dict, string keyPath, out T value)
    {
        value = default!;
        var keys = keyPath.Split(':');
        VtValue currentValue = new VtValue(dict);

        foreach (var key in keys)
        {
            if (!currentValue.IsHolding<VtDictionary>())
                return false;

            var currentDict = currentValue.Get<VtDictionary>();
            if (!currentDict.TryGetValue(key, out var nextValue) || nextValue is null)
                return false;
            
            currentValue = nextValue;
        }

        if (currentValue.IsHolding<T>())
        {
            value = currentValue.Get<T>();
            return true;
        }
        return false;
    }

    private static bool GetValueFromDictPath(VtDictionary dict, string keyPath, out VtValue value)
    {
        value = VtValue.Empty;
        var keys = keyPath.Split(':');
        VtValue currentValue = new VtValue(dict);

        foreach (var key in keys)
        {
            if (!currentValue.IsHolding<VtDictionary>())
                return false;

            var currentDict = currentValue.Get<VtDictionary>();
            if (!currentDict.TryGetValue(key, out var nextValue) || nextValue == null)
                return false;
            
            currentValue = nextValue;
        }

        value = currentValue;
        return true;
    }

    private static void SetValueInDictPath(VtDictionary dict, string keyPath, VtValue value)
    {
        var keys = keyPath.Split(':');
        var currentDict = dict;

        // Navigate to the parent dictionary
        for (int i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];
            if (!currentDict.TryGetValue(key, out var subValue) || !subValue.IsHolding<VtDictionary>())
            {
                subValue = new VtValue(new VtDictionary());
                currentDict[key] = subValue;
            }
            currentDict = subValue.Get<VtDictionary>();
        }

        // Set the value
        currentDict[keys[^1]] = value;
    }

    private static bool RemoveValueFromDictPath(VtDictionary dict, string keyPath)
    {
        var keys = keyPath.Split(':');
        var currentDict = dict;

        // Navigate to the parent dictionary
        for (int i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];
            if (!currentDict.TryGetValue(key, out var subValue) || !subValue.IsHolding<VtDictionary>())
                return false;
            
            currentDict = subValue.Get<VtDictionary>();
        }

        // Remove the value
        return currentDict.Remove(keys[^1]);
    }

    private static bool HasValueInDictPath(VtDictionary dict, string keyPath)
    {
        return GetValueFromDictPath(dict, keyPath, out VtValue _);
    }

    #endregion
}