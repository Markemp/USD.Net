using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

public abstract class UsdProperty : UsdObject, IUsdProperty
{
    private readonly Dictionary<string, object> _metadata = [];
    private bool _isCustom;
    private string _displayGroup = string.Empty;

    #region Construction

    protected UsdProperty() : base()
    {
    }

    protected UsdProperty(UsdStage stage, ISdfPath path) : base(stage, path)
    {
    }

    #endregion

    #region Property Validation

    public virtual bool IsDefined() => IsValid() && IsAuthored();

    public virtual bool IsAuthored()
    {
        // TODO: Check if property has authored opinions in layer stack
        return IsValid();
    }

    public virtual bool IsAuthoredAt(UsdEditTarget editTarget)
    {
        // TODO: Check specific edit target for authoring
        return IsAuthored();
    }

    #endregion

    #region Custom and Namespace Properties

    public virtual bool IsCustom() => _isCustom;

    public virtual bool SetCustom(bool isCustom)
    {
        _isCustom = isCustom;
        return true;
    }

    public virtual TfToken GetNamespace()
    {
        // Match OpenUSD behavior: use GetName() which returns TfToken, then get its string
        string fullName = GetName().GetString();
        int delim = fullName.LastIndexOf(GetNamespaceDelimiter());
        
        // If delimiter is at the end, that's invalid
        if (delim == fullName.Length - 1)
            return TfToken.Empty;
            
        return delim == -1 ? TfToken.Empty : new TfToken(fullName.Substring(0, delim));
    }

    public virtual TfToken GetBaseName()
    {
        // Match OpenUSD behavior: use GetName() which returns TfToken, then get its string
        string fullName = GetName().GetString();
        int delim = fullName.LastIndexOf(GetNamespaceDelimiter());
        
        // If delimiter is at the end, that's invalid
        if (delim == fullName.Length - 1)
            return TfToken.Empty;
            
        return delim == -1 ? GetName() : new TfToken(fullName.Substring(delim + 1));
    }

    public virtual IReadOnlyList<string> SplitName()
    {
        // Match OpenUSD behavior: use GetName() which returns TfToken, then tokenize
        // In OpenUSD this uses SdfPath::TokenizeIdentifier
        string fullName = GetName().GetString();
        
        // Empty or ends with delimiter is invalid
        if (string.IsNullOrEmpty(fullName) || fullName.EndsWith(GetNamespaceDelimiter()))
            return Array.Empty<string>();
            
        var parts = fullName.Split(GetNamespaceDelimiter());
        return parts.ToList().AsReadOnly();
    }

    #endregion

    #region Display Groups

    public virtual string GetDisplayGroup()
    {
        return _displayGroup;
    }

    public virtual bool SetDisplayGroup(string displayGroup)
    {
        _displayGroup = displayGroup ?? string.Empty;
        return true;
    }

    public virtual bool ClearDisplayGroup()
    {
        _displayGroup = string.Empty;
        return true;
    }

    /// <summary>
    /// Return true if this property has a display group.
    /// </summary>
    public virtual bool HasAuthoredDisplayGroup()
    {
        return !string.IsNullOrEmpty(_displayGroup);
    }

    public virtual bool SetNestedDisplayGroups(IEnumerable<string> nestedGroups)
    {
        if (nestedGroups is null)
        {
            _displayGroup = string.Empty;
            return true;
        }

        _displayGroup = string.Join(":", nestedGroups);
        return true;
    }

    public virtual IReadOnlyList<string> GetNestedDisplayGroups()
    {
        if (string.IsNullOrEmpty(_displayGroup))
            return Array.Empty<string>();
            
        return _displayGroup.Split(':', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly();
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Set metadata for this property.
    /// </summary>
    public override bool SetMetadata<T>(TfToken key, T value)
    {
        _metadata[key.GetText()] = value!;
        return true;
    }

    /// <summary>
    /// Get metadata for this property.
    /// </summary>
    public virtual T? GetMetadata<T>(TfToken key)
    {
        if (_metadata.TryGetValue(key.GetText(), out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Return true if this property has metadata with the given key.
    /// </summary>
    public override bool HasMetadata(TfToken key)
    {
        return _metadata.ContainsKey(key.GetText());
    }

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public override bool ClearMetadata(TfToken key)
    {
        return _metadata.Remove(key.GetText());
    }

    /// <summary>
    /// Get all metadata keys for this property.
    /// </summary>
    public virtual IEnumerable<string> GetMetadataKeys()
    {
        return _metadata.Keys;
    }

    #endregion

    #region Property Stack

    public virtual IReadOnlyList<SdfPropertySpec> GetPropertyStack(UsdTimeCode time = default)
    {
        // TODO: Implement property stack resolution from layer composition
        return Array.Empty<SdfPropertySpec>();
    }

    public virtual IReadOnlyList<(SdfPropertySpec spec, SdfLayerOffset offset)> GetPropertyStackWithLayerOffsets(UsdTimeCode time = default)
    {
        // TODO: Implement property stack with layer offsets
        return Array.Empty<(SdfPropertySpec, SdfLayerOffset)>();
    }

    #endregion

    #region Flattening

    public virtual IUsdProperty FlattenTo(IUsdPrim parent)
    {
        // TODO: Implement property flattening
        throw new NotImplementedException("Property flattening not yet implemented");
    }

    public virtual IUsdProperty FlattenTo(IUsdPrim parent, TfToken propName)
    {
        // TODO: Implement property flattening with custom name
        throw new NotImplementedException("Property flattening not yet implemented");
    }

    public virtual IUsdProperty FlattenTo(IUsdProperty property)
    {
        // TODO: Implement property-to-property flattening
        throw new NotImplementedException("Property flattening not yet implemented");
    }

    #endregion

    #region Conversions

    /// <summary>
    /// Implicit conversion to bool for validity checking.
    /// </summary>
    public static implicit operator bool(UsdProperty property)
    {
        return property?.IsValid() == true;
    }

    #endregion
}