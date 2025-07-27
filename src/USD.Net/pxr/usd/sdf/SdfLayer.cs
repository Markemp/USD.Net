using System.Collections.Concurrent;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

using FileFormatArguments = Dictionary<string, string>;

/// <summary>
/// SdfLayer represents a scene description container that can combine with other layers to form compositions.
/// It stores scene description data and participates in USD's layered composition system.
/// </summary>
/// <remarks>
/// A scene description container that can combine with other such containers
/// to form simple component assets, and successively larger aggregates.  The
/// contents of an SdfLayer adhere to the SdfData data model.  A layer can be
/// ephemeral, or be an asset accessed and serialized through the ArAsset and
/// ArResolver interfaces.
///
/// The SdfLayer class provides a consistent API for accesing and serializing
/// scene description, using any data store provided by Ar plugins.  Sdf
/// itself provides a UTF-8 text format for layers identified by the ".sdf"
/// identifier extension, but via the SdfFileFormat abstraction, allows
/// downstream modules and plugins to adapt arbitrary data formats to the
/// SdfData/SdfLayer model.
///
/// The FindOrOpen() method returns a new SdfLayer object with scene
/// description from any supported asset format. Once read, a layer
/// remembers which asset it was read from. The Save() method saves the layer
/// back out to the original asset.  You can use the Export() method to write
/// the layer to a different location. You can use the GetIdentifier() method
/// to get the layer's Id or GetRealPath() to get the resolved, full URI.
///
/// Layer identifiers are UTF-8 encoded strings. A layer's file format is
/// determined via the identifier's extension (as resolved by Ar) with [A-Z]
/// (and no other characters) explicitly case folded.
///
/// Layers can have a timeCode range (startTimeCode and endTimeCode). This range
/// represents the suggested playback range, but has no impact on the extent of 
/// the animation data that may be stored in the layer. The metadatum 
/// "timeCodesPerSecond" is used to annotate how the time ordinate for samples
/// contained in the file scales to seconds. For example, if timeCodesPerSecond
/// is 24, then a sample at time ordinate 24 should be viewed exactly one second
/// after the sample at time ordinate 0.
/// </remarks>
public class SdfLayer : ISdfLayer
{
    private readonly Dictionary<ISdfPath, SdfPrimSpec> _primSpecs = new();
    private readonly string _identifier;
    private bool _isDirty = false;
    private static readonly ConcurrentDictionary<string, ISdfLayer> _layerRegistry = new();
    private readonly ISdfSchemaBase _schema;
    private ISdfData _data;
    private ISdfFileFormat? _fileFormat;
    private FileFormatArguments _fileFormatArgs = new();
    private bool _permissionToEdit = true;
    private bool _permissionToSave = true;
    private bool _isAnonymous = false;
    private bool _isMuted = false;

    public SdfLayer(string identifier, ISdfSchemaBase? schema = null, ISdfFileFormat? fileFormat = null)
    {
        _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        _schema = schema ?? SdfSchema.Instance;
        _data = new SdfData();
        _fileFormat = fileFormat;
        _isAnonymous = identifier.StartsWith("anon:");
        
        // Initialize the root spec
        var rootPath = SdfPath.AbsoluteRootPath();
        _data.CreateSpec(rootPath, SdfSpecType.PseudoRoot);

        if (!_isAnonymous)
            _layerRegistry[identifier] = this;
    }

    /// <summary>
    /// Get the identifier for this layer.
    /// </summary>
    public string GetIdentifier() => _identifier;
    
    /// <summary>
    /// Sets the layer identifier.
    /// </summary>
    public void SetIdentifier(string identifier)
    {
        // Note: In a full implementation, this would update the layer registry
        // For now, this is a simplified version
        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));
    }
    
    /// <summary>
    /// Update layer asset information.
    /// </summary>
    public void UpdateAssetInfo()
    {
        // Placeholder - would re-resolve the layer identifier
    }
    
    /// <summary>
    /// Returns the resolved path for this layer.
    /// </summary>
    public string GetRealPath() => _isAnonymous ? string.Empty : _identifier;
    
    /// <summary>
    /// Returns the file extension to use for this layer.
    /// </summary>
    public string GetFileExtension()
    {
        if (_isAnonymous)
            return "usda"; // Default extension for anonymous layers
            
        var ext = Path.GetExtension(_identifier);
        return string.IsNullOrEmpty(ext) ? "usda" : ext.TrimStart('.');
    }
    
    /// <summary>
    /// Returns the asset system version of this layer.
    /// </summary>
    public string GetVersion() => string.Empty; // Not implemented
    
    /// <summary>
    /// Returns the layer identifier in asset path form.
    /// </summary>
    public string GetRepositoryPath() => string.Empty; // Not implemented
    
    /// <summary>
    /// Returns the asset name associated with this layer.
    /// </summary>
    public string GetAssetName() => Path.GetFileNameWithoutExtension(_identifier) ?? string.Empty;
    
    /// <summary>
    /// Returns resolve information from the last time the layer identifier was resolved.
    /// </summary>
    public VtValue GetAssetInfo() => new VtValue(); // Not implemented
    
    /// <summary>
    /// Returns the path to the asset specified by assetPath using this layer to anchor the path.
    /// </summary>
    public string ComputeAbsolutePath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath) || _isAnonymous)
            return assetPath;
            
        if (Path.IsPathRooted(assetPath))
            return assetPath;
            
        var layerDir = Path.GetDirectoryName(_identifier);
        return string.IsNullOrEmpty(layerDir) ? assetPath : Path.Combine(layerDir, assetPath);
    }

    /// <summary>
    /// Get the display name for this layer.
    /// </summary>
    public string GetDisplayName() => Path.GetFileName(_identifier) ?? _identifier;
    
    /// <summary>
    /// Returns the file format used by this layer.
    /// </summary>
    public ISdfFileFormat? GetFileFormat() => _fileFormat;
    
    /// <summary>
    /// Returns the file format-specific arguments used during construction.
    /// </summary>
    public FileFormatArguments GetFileFormatArguments() => new(_fileFormatArgs);

    /// <summary>
    /// Return true if this layer has been modified.
    /// </summary>
    public bool IsDirty() => _isDirty;
    
    /// <summary>
    /// Returns whether this layer has no significant data.
    /// </summary>
    public bool IsEmpty()
    {
        // Layer is empty if it has no significant composition data
        // Based on OpenUSD SdfLayer::IsEmpty() implementation
        return GetRootPrims().Count == 0 && 
               GetRootPrimOrder().Count == 0 && 
               GetSubLayerPaths().Count == 0 &&
               GetRelocates().Count == 0;
    }
    
    /// <summary>
    /// Returns true if this layer streams data from its serialized data store.
    /// </summary>
    public bool StreamsData() => false; // Not implemented yet
    
    /// <summary>
    /// Returns true if this layer is detached from its serialized data store.
    /// </summary>
    public bool IsDetached() => false; // Not implemented yet
    
    /// <summary>
    /// Returns true if this layer is an anonymous layer.
    /// </summary>
    public bool IsAnonymous() => _isAnonymous;
    
    /// <summary>
    /// Returns true if this layer is muted.
    /// </summary>
    public bool IsMuted() => _isMuted;
    
    /// <summary>
    /// Mutes or unmutes the current layer.
    /// </summary>
    public void SetMuted(bool muted) => _isMuted = muted;
    
    /// <summary>
    /// Returns true if this layer can be saved to disk.
    /// </summary>
    public bool PermissionToSave() => _permissionToSave;
    
    /// <summary>
    /// Returns true if this layer can be edited.
    /// </summary>
    public bool PermissionToEdit() => _permissionToEdit;

    /// <summary>
    /// Save this layer to its persistent representation.
    /// </summary>
    public bool Save(bool force = false)
    {
        if (!PermissionToSave())
            return false;
            
        if (!_isDirty && !force)
            return true;
            
        if (_isAnonymous || _fileFormat == null)
            return false;
            
        try
        {
            var success = _fileFormat.SaveToFile(this, _identifier, "", _fileFormatArgs);
            if (success)
            {
                _isDirty = false;
            }
            return success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Export this layer to a file.
    /// </summary>
    public bool Export(string filename, string comment = "", FileFormatArguments? args = null)
    {
        if (string.IsNullOrEmpty(filename))
            return false;
            
        try
        {
            // Find appropriate file format for the target file
            var format = ISdfFileFormat.FindByExtension(filename);
            if (format == null)
            {
                // Fall back to text format
                format = SdfTextFileFormat.Instance;
            }
            
            return format.WriteToFile(this, filename, comment, args);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Writes this layer to the given string.
    /// </summary>
    public bool ExportToString(out string result)
    {
        result = string.Empty;
        
        try
        {
            // Use text format for string export
            var format = SdfTextFileFormat.Instance;
            return format.WriteToString(this, out result);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Reads this layer from the given string.
    /// </summary>
    public bool ImportFromString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;
            
        if (!PermissionToEdit())
            return false;
            
        try
        {
            // Use text format for string import
            var format = SdfTextFileFormat.Instance;
            return format.ReadFromString(this, str);
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Imports the content of the given layer path.
    /// </summary>
    public bool Import(string layerPath)
    {
        if (string.IsNullOrEmpty(layerPath))
            return false;
            
        if (!PermissionToEdit())
            return false;
            
        if (layerPath == GetRealPath())
        {
            // Can't import from self, use Reload instead
            return false;
        }
            
        try
        {
            // Find appropriate file format
            var format = ISdfFileFormat.FindByExtension(layerPath);
            if (format == null || !format.CanRead(layerPath))
                return false;
                
            // Clear existing content
            Clear();
            
            // Read from the specified file
            return format.Read(this, layerPath, metadataOnly: false);
        }
        catch
        {
            return false;
        }
    }
    

    /// <summary>
    /// Clear all content from this layer.
    /// </summary>
    public void Clear()
    {
        if (!PermissionToEdit())
            return;
            
        // Clear all data except reinitialize root spec
        _data = new SdfData();
        _primSpecs.Clear();
        
        // Reinitialize the root spec
        var rootPath = SdfPath.AbsoluteRootPath();
        _data.CreateSpec(rootPath, SdfSpecType.PseudoRoot);
        
        _isDirty = true;
    }

    /// <summary>
    /// Reload this layer from its persistent representation.
    /// </summary>
    public bool Reload(bool force = false)
    {
        if (_isAnonymous || _fileFormat == null)
            return false;
            
        if (!force && !_isDirty)
            return true;
            
        try
        {
            // Clear existing content
            Clear();
            
            // Reload from file
            var success = _fileFormat.Read(this, _identifier, metadataOnly: false);
            if (success)
            {
                _isDirty = false;
            }
            return success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Set metadata on this layer.
    /// </summary>
    public void SetMetadata(TfToken key, VtValue value)
    {
        SetField(SdfPath.AbsoluteRootPath(), key, value);
    }

    /// <summary>
    /// Get metadata from this layer.
    /// </summary>
    public VtValue GetMetadata(TfToken key)
    {
        return GetField(SdfPath.AbsoluteRootPath(), key);
    }

    public SdfData GetMetadata()
    {
        var result = new SdfData();
        var absRoot = SdfPath.AbsoluteRootPath();

        result.CreateSpec(absRoot, SdfSpecType.PseudoRoot);

        var fieldNames = ListFields(absRoot);
        foreach (var fieldName in fieldNames)
        {
            var value = GetField(absRoot, fieldName);
            result.Set(absRoot, fieldName, value);
        }

        return result;
    }

    /// <summary>
    /// Return true if this layer has metadata with the given key.
    /// </summary>
    public bool HasMetadata(TfToken key) => HasField(SdfPath.AbsoluteRootPath(), key);

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public void ClearMetadata(TfToken key)
    {
        EraseField(SdfPath.AbsoluteRootPath(), key);
    }

    /// <summary>
    /// Get the schema for this layer
    /// </summary>
    public ISdfSchemaBase GetSchema() => _schema;

    /// <summary>
    /// Check if this layer has a spec at the given path
    /// </summary>
    public bool HasSpec(ISdfPath path) => path is ISdfPath sdfPath && _data.HasSpec(sdfPath);

    /// <summary>
    /// Get the spec type at the given path
    /// </summary>
    public SdfSpecType GetSpecType(ISdfPath path) => _data.GetSpecType(path);

    /// <summary>
    /// Get a field value at the given path
    /// </summary>
    public VtValue GetField(SdfPath path, TfToken fieldName) => _data.Get(path, fieldName);
    public VtValue GetField(ISdfPath path, TfToken fieldName) => 
        path is SdfPath sdfPath ? _data.Get(sdfPath, fieldName) : VtValue.Empty;

    /// <summary>
    /// Check if a field exists at the given path
    /// </summary>
    public bool HasField(ISdfPath path, TfToken fieldName) => _data.Has(path, fieldName);
    
    /// <summary>
    /// Check if a field exists at the given path with output value
    /// </summary>
    public bool HasField(ISdfPath path, TfToken fieldName, out VtValue? value)
    {
        if (_data.Has(path, fieldName))
        {
            value = _data.Get(path, fieldName);
            return true;
        }
        value = null;
        return false;
    }
    
    /// <summary>
    /// Check if a field exists with the specified type
    /// </summary>
    public bool HasField<T>(ISdfPath path, TfToken fieldName, out T? value)
    {
        if (_data.Has(path, fieldName))
        {
            var vtValue = _data.Get(path, fieldName);
            if (vtValue.IsHolding<T>())
            {
                value = vtValue.Get<T>();
                return true;
            }
        }
        value = default(T);
        return false;
    }
    
    /// <summary>
    /// Get a field value with default fallback
    /// </summary>
    public T GetFieldAs<T>(ISdfPath path, TfToken fieldName, T defaultValue = default!)
    {
        if (HasField<T>(path, fieldName, out var value))
            return value!;
        return defaultValue;
    }
    
    /// <summary>
    /// Check if a dictionary field has a specific key
    /// </summary>
    public bool HasFieldDictKey(ISdfPath path, TfToken fieldName, TfToken keyPath)
    {
        return _data.HasDictKey(path, fieldName, keyPath);
    }
    
    /// <summary>
    /// Check if a dictionary field has a specific key with output value
    /// </summary>
    public bool HasFieldDictKey(ISdfPath path, TfToken fieldName, TfToken keyPath, out VtValue? value)
    {
        if (_data.HasDictKey(path, fieldName, keyPath))
        {
            value = _data.GetDictValueByKey(path, fieldName, keyPath);
            return true;
        }
        value = null;
        return false;
    }
    
    /// <summary>
    /// Check if a dictionary field has a specific key with typed output
    /// </summary>
    public bool HasFieldDictKey<T>(ISdfPath path, TfToken fieldName, TfToken keyPath, out T? value)
    {
        if (_data.HasDictKey(path, fieldName, keyPath))
        {
            var vtValue = _data.GetDictValueByKey(path, fieldName, keyPath);
            if (vtValue.IsHolding<T>())
            {
                value = vtValue.Get<T>();
                return true;
            }
        }
        value = default(T);
        return false;
    }
    
    /// <summary>
    /// Get a dictionary field value by key
    /// </summary>
    public VtValue GetFieldDictValueByKey(ISdfPath path, TfToken fieldName, TfToken keyPath)
    {
        return _data.GetDictValueByKey(path, fieldName, keyPath);
    }
    
    /// <summary>
    /// Set a dictionary field value by key
    /// </summary>
    public void SetFieldDictValueByKey(ISdfPath path, TfToken fieldName, TfToken keyPath, VtValue value)
    {
        _PrimSetFieldDictValueByKey(path, fieldName, keyPath, value);
    }
    
    /// <summary>
    /// Set a dictionary field value by key (generic version)
    /// </summary>
    public void SetFieldDictValueByKey<T>(ISdfPath path, TfToken fieldName, TfToken keyPath, T value)
    {
        SetFieldDictValueByKey(path, fieldName, keyPath, new VtValue(value));
    }

    /// <summary>
    /// Set a field value at the given path
    /// </summary>
    public void SetField(ISdfPath path, TfToken fieldName, VtValue value)
    {
        _PrimSetField(path, fieldName, value);
    }
    
    /// <summary>
    /// Set a field value at the given path (generic version)
    /// </summary>
    public void SetField<T>(ISdfPath path, TfToken fieldName, T value)
    {
        SetField(path, fieldName, new VtValue(value));
    }


    /// <summary>
    /// Create a new layer in memory.
    /// </summary>
    public static SdfLayer CreateNew(string identifier, FileFormatArguments? args = null)
    {
        var fileFormat = ISdfFileFormat.FindByExtension(identifier);
        return new SdfLayer(identifier, SdfSchema.Instance, fileFormat);
    }

    /// <summary>
    /// Find or open a layer with the given identifier.
    /// </summary>
    public static ISdfLayer? FindOrOpen(string identifier, FileFormatArguments? args = null)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;
            
        // Check if layer is already in registry
        if (_layerRegistry.TryGetValue(identifier, out var existingLayer))
            return existingLayer;
            
        try
        {
            // Find appropriate file format
            var fileFormat = ISdfFileFormat.FindByExtension(identifier);
            if (fileFormat is null)
            {
                // Default to text format
                fileFormat = SdfTextFileFormat.Instance;
            }
            
            var layer = new SdfLayer(identifier, SdfSchema.Instance, fileFormat);
            
            // Try to load from file if it exists
            if (File.Exists(identifier) && fileFormat.CanRead(identifier))
            {
                if (fileFormat.Read(layer, identifier, metadataOnly: false))
                {
                    layer._isDirty = false;
                    _layerRegistry[identifier] = layer;
                    return layer;
                }
            }
            
            // Return new empty layer if file doesn't exist or couldn't be read
            _layerRegistry[identifier] = layer;
            return layer;
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Find an existing layer in the registry.
    /// </summary>
    public static ISdfLayer? Find(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;
            
        return _layerRegistry.TryGetValue(identifier, out var layer) ? layer : null;
    }

    /// <summary>
    /// Create an anonymous layer.
    /// </summary>
    public static SdfLayer CreateAnonymous(string? tag = null, FileFormatArguments? args = null)
    {
        var identifier = $"anon:{Guid.NewGuid():N}";
        if (!string.IsNullOrEmpty(tag))
            identifier += $":{tag}";
        
        // Anonymous layers default to text format
        var fileFormat = SdfTextFileFormat.Instance;
        return new SdfLayer(identifier, SdfSchema.Instance, fileFormat);
    }
    
    /// <summary>
    /// Create a new layer without adding it to the registry.
    /// </summary>
    public static SdfLayer New(ISdfFileFormat fileFormat, FileFormatArguments? args = null)
    {
        var identifier = $"anon:{Guid.NewGuid():N}:new";
        return new SdfLayer(identifier, SdfSchema.Instance, fileFormat);
    }

    #region Prim Spec Management

    /// <summary>
    /// Add a prim spec to this layer.
    /// </summary>
    public void AddPrimSpec(SdfPrimSpec primSpec)
    {
        if (primSpec?.IsValid() == true)
        {
            _primSpecs[primSpec.GetPath()] = primSpec;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Get a prim spec by path.
    /// </summary>
    public SdfPrimSpec? GetPrimSpec(ISdfPath path)
        => _primSpecs.TryGetValue(path, out var primSpec) ? primSpec : null;

    /// <summary>
    /// Get all prim specs in this layer.
    /// </summary>
    public IEnumerable<SdfPrimSpec> GetAllPrimSpecs() => _primSpecs.Values;

    /// <summary>
    /// Get all root prim specs (prims at the root level).
    /// </summary>
    public IEnumerable<SdfPrimSpec> GetRootPrimSpecs()
        =>_primSpecs.Values.Where(spec => spec.GetPath().GetParentPath().IsAbsoluteRootPath());

    /// <summary>
    /// Return true if this layer has a prim spec at the given path.
    /// </summary>
    public bool HasPrimSpec(ISdfPath path) => _primSpecs.ContainsKey(path);

    /// <summary>
    /// Remove a prim spec from this layer.
    /// </summary>
    public bool RemovePrimSpec(ISdfPath path)
    {
        if (_primSpecs.Remove(path))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    #endregion

    /// <summary>
    /// List all field names at the given path, including required fields from schema
    /// </summary>
    public IReadOnlyList<TfToken> ListFields(SdfPath path) => SdfData.ListFields(_schema, _data, path);
    public IReadOnlyList<TfToken> ListFields(ISdfPath path) => 
        path is SdfPath sdfPath ? SdfData.ListFields(_schema, _data, sdfPath) : Array.Empty<TfToken>();
    
    /// <summary>
    /// Traverse the scene description hierarchy rooted at path
    /// </summary>
    public void Traverse(ISdfPath path, Action<ISdfPath> func)
    {
        if (func == null)
            return;
            
        // Simple recursive traversal
        TraverseRecursive(path, func);
    }
    
    private void TraverseRecursive(ISdfPath path, Action<ISdfPath> func)
    {
        if (!HasSpec(path))
            return;
            
        // Call function on current path first
        func(path);
        
        // Traverse children by examining child fields
        var fields = ListFields(path);
        foreach (var fieldName in fields)
        {
            // Check if this field represents children
            if (IsChildrenField(fieldName))
            {
                TraverseChildrenField(path, fieldName, func);
            }
        }
    }
    
    private bool IsChildrenField(TfToken fieldName)
    {
        // Check if this field represents a children field
        return fieldName.Equals(SdfChildrenKeys.PrimChildren) ||
               fieldName.Equals(SdfChildrenKeys.PropertyChildren) ||
               fieldName.Equals(SdfChildrenKeys.VariantChildren) ||
               fieldName.Equals(SdfChildrenKeys.VariantSetChildren) ||
               fieldName.Equals(SdfChildrenKeys.ConnectionChildren) ||
               fieldName.Equals(SdfChildrenKeys.RelationshipTargetChildren) ||
               fieldName.Equals(SdfChildrenKeys.ExpressionChildren) ||
               fieldName.Equals(SdfChildrenKeys.MapperChildren) ||
               fieldName.Equals(SdfChildrenKeys.MapperArgChildren);
    }
    
    private void TraverseChildrenField(ISdfPath parentPath, TfToken fieldName, Action<ISdfPath> func)
    {
        var childrenValue = GetField(parentPath, fieldName);
        if (childrenValue.IsEmpty())
            return;
            
        // Children are typically stored as lists of TfTokens (names)
        if (childrenValue.IsHolding<List<TfToken>>())
        {
            var childNames = childrenValue.Get<List<TfToken>>();
            foreach (var childName in childNames)
            {
                var childPath = GetChildPath(parentPath, fieldName, childName);
                if (!childPath.IsEmpty())
                    TraverseRecursive(childPath, func);
            }
        }
        // Some children might be stored as other collection types
        else if (childrenValue.IsHolding<TfToken[]>())
        {
            var childNames = childrenValue.Get<TfToken[]>();
            foreach (var childName in childNames)
            {
                var childPath = GetChildPath(parentPath, fieldName, childName);
                if (!childPath.IsEmpty())
                    TraverseRecursive(childPath, func);
            }
        }
    }
    
    private ISdfPath GetChildPath(ISdfPath parentPath, TfToken fieldName, TfToken childName)
    {
        // Construct child path based on the type of children field
        if (fieldName.Equals(SdfChildrenKeys.PrimChildren))
        {
            return parentPath.AppendChild(childName);
        }
        else if (fieldName.Equals(SdfChildrenKeys.PropertyChildren))
        {
            return parentPath.AppendProperty(childName);
        }
        else if (fieldName.Equals(SdfChildrenKeys.VariantSetChildren))
        {
            return parentPath.AppendVariantSelection(childName, string.Empty);
        }
        else if (fieldName.Equals(SdfChildrenKeys.VariantChildren))
        {
            // Variant children need special handling - they're under a variant set
            // For now, skip them as they need more complex path construction
            return SdfPath.EmptyPath();
        }
        // Add other child types as needed
        else
        {
            // For unhandled child types, try generic property append
            return parentPath.AppendProperty(childName);
        }
    }

    /// <summary>
    /// Static helper method for listing fields (matches C++ _ListFields)
    /// </summary>
    private static IReadOnlyList<TfToken> _ListFields(ISdfSchemaBase schema, SdfData data, SdfPath path)
        => SdfData.ListFields(schema, data, path);

    /// <summary>
    /// Removes a field from a spec at the given path.
    /// </summary>
    /// <param name="path">The path to the spec.</param>
    /// <param name="fieldName">The name of the field to erase.</param>
    public void EraseField(ISdfPath path, TfToken fieldName)
    {
        if (!PermissionToEdit())
            throw new InvalidOperationException($"Cannot erase {fieldName} on <{path}>. Layer @{GetIdentifier()}@ is not editable.");

        if (!_data.Has(path, fieldName))
            return;

        // If this is a required field, only perform the erase if the current value
        // differs from the fallback. Required fields behave as if they're always
        // authored, so the effect of an "erase" is to set the value to the fallback
        // value.
        var fieldDef = _GetRequiredFieldDef(path, fieldName);
        if (fieldDef is not null)
        {
            if (GetField(path, fieldName).Equals(fieldDef.GetFallbackValue()))
                return;
        }

        // Note: with this implementation, erasing a field and undoing that
        // operation will not restore the underlying data exactly to its
        // previous state. Specifically, this may cause the order of the fields
        // for the given spec to change. There are no semantics attached to this
        // ordering, so this should be OK.
        _PrimSetField(path, fieldName, new VtValue());
    }

    /// <summary>
    /// Removes a dictionary key from a field at the given path.
    /// </summary>
    /// <param name="path">The path to the spec.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="keyPath">The key path within the dictionary to erase.</param>
    public void EraseFieldDictValueByKey(ISdfPath path, TfToken fieldName, TfToken keyPath)
    {
        if (!PermissionToEdit())
            throw new InvalidOperationException($"Cannot erase {fieldName}:{keyPath} on <{path}>. Layer @{GetIdentifier()}@ is not editable.");

        if (!_data.HasDictKey(path, fieldName, keyPath)) return;

        // Note: with this implementation, erasing a field and undoing that
        // operation will not restore the underlying data exactly to its
        // previous state. Specifically, this may cause the order of the fields
        // for the given spec to change. There are no semantics attached to this
        // ordering, so this should be OK.
        _PrimSetFieldDictValueByKey(path, fieldName, keyPath, new VtValue());
    }

    /// <summary>
    /// Gets the required field definition for a field if it exists.
    /// </summary>
    private ISdfFieldDefinition? _GetRequiredFieldDef(ISdfPath path, TfToken fieldName, SdfSpecType? specType = null)
    {
        var schema = GetSchema();
        if (schema.IsRequiredFieldName(fieldName))
        {
            specType ??= GetSpecType(path);
            var specDef = schema.GetSpecDefinition(specType.Value);
            if (specDef is not null && specDef.IsRequiredField(fieldName))
                return schema.GetFieldDefinition(fieldName);
        }
        return null;
    }
    
    /// <summary>
    /// Core method for setting field values with change notification
    /// </summary>
    private void _PrimSetField(ISdfPath path, TfToken fieldName, VtValue value)
    {
        if (!PermissionToEdit())
            return;
            
        // Create spec if it doesn't exist
        if (!_data.HasSpec(path))
        {
            // Determine spec type based on path
            SdfSpecType specType;
            if (path.IsAbsoluteRootPath())
                specType = SdfSpecType.PseudoRoot;
            else if (path.IsPropertyPath())
                specType = SdfSpecType.Attribute; // Default to attribute for property paths
            else
                specType = SdfSpecType.Prim;
                
            _data.CreateSpec(path, specType);
        }
        
        _data.Set(path, fieldName, value);
        _isDirty = true;
    }
    
    /// <summary>
    /// Core method for setting dictionary field values with change notification
    /// </summary>
    private void _PrimSetFieldDictValueByKey(ISdfPath path, TfToken fieldName, TfToken keyPath, VtValue value)
    {
        if (!PermissionToEdit())
            return;
            
        // Create spec if it doesn't exist
        if (!_data.HasSpec(path))
        {
            // Determine spec type based on path
            SdfSpecType specType;
            if (path.IsAbsoluteRootPath())
                specType = SdfSpecType.PseudoRoot;
            else if (path.IsPropertyPath())
                specType = SdfSpecType.Attribute; // Default to attribute for property paths
            else
                specType = SdfSpecType.Prim;
                
            _data.CreateSpec(path, specType);
        }
        
        _data.SetDictValueByKey(path, fieldName, keyPath, value);
        _isDirty = true;
    }
    
    /// <summary>
    /// Determines whether the spec at the given path has no significant data.
    /// </summary>
    /// <param name="path">The path to the spec to check</param>
    /// <param name="ignoreChildren">If true, skip checking children fields</param>
    /// <param name="requiredFieldOnlyPropertiesAreInert">If true, properties with only required fields are considered inert</param>
    /// <returns>True if the spec doesn't affect the scene, false otherwise</returns>
    /// <remarks>
    /// A spec is considered inert if it has only the required SpecType field (stored
    /// separately from other fields), and thus doesn't affect the scene. Special
    /// cases exist for different spec types, such as prims with defining specifiers
    /// or specific typenames, which always affect the scene.
    /// </remarks>
    internal bool _IsInert(ISdfPath path, bool ignoreChildren, bool requiredFieldOnlyPropertiesAreInert = false)
    {
        // If the spec has only the required SpecType field (stored
        // separately from other fields), then it doesn't affect the scene.
        var fields = ListFields(path);
        if (!fields.Any())
            return true;

        // If the spec is custom it affects the scene.
        if (GetFieldAs<bool>(path, SdfFieldKeys.Custom, false))
            return false;

        // Special cases for determining whether a spec affects the scene.
        var specType = GetSpecType(path);

        // Prims that are defs or with a specific typename always affect the scene
        // since they bring a prim into existence.
        if (specType == SdfSpecType.Prim)
        {
            var specifier = GetFieldAs<SdfSpecifier>(path, SdfFieldKeys.Specifier, SdfSpecifier.SdfSpecifierOver);
            if (SdfSpecifierHelpers.SdfIsDefiningSpecifier(specifier))
                return false;

            var type = GetFieldAs<TfToken>(path, SdfFieldKeys.TypeName);
            if (!type.IsEmpty)
                return false;
        }

        // For other specs, use the schema to determine the list of required fields.
        if (specType != SdfSpecType.Unknown)
        {
            var schema = GetSchema();
            var specDefinition = schema.GetSpecDefinition(specType);
            if (specDefinition is null)
                return false;

            // Special case for properties with only required fields.
            // This currently only applies to the connectability attributes on 
            // attributes in Usd shading, where the connectability of a shader's
            // input is considered non-significant scene description.
            if (requiredFieldOnlyPropertiesAreInert && path.IsPropertyPath())
            {
                foreach (var field in fields)
                {
                    if (!specDefinition.IsRequiredField(field))
                        return false;
                }
                return true;
            }

            // If any field is not a required field (and not a children field
            // that's being skipped), the spec is not inert.
            foreach (var field in fields)
            {
                // If specified, skip over children fields. This is a special case
                // to allow _IsInertSubtree to process these children separately.
                if (ignoreChildren)
                {
                    if ((specType == SdfSpecType.Prim &&
                         (field == SdfChildrenKeys.PrimChildren ||
                          field == SdfChildrenKeys.PropertyChildren ||
                          field == SdfChildrenKeys.VariantSetChildren))
                        ||
                        (specType == SdfSpecType.VariantSet &&
                         field == SdfChildrenKeys.VariantChildren))
                    {
                        continue;
                    }
                }

                // If the field is required, ignore it.
                if (specDefinition.IsRequiredField(field))
                    continue;

                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the entire subtree rooted at the given path is inert.
    /// </summary>
    /// <param name="path">The root path of the subtree to check</param>
    /// <returns>True if the entire subtree doesn't affect the scene, false otherwise</returns>
    /// <remarks>
    /// This method recursively checks if a spec and all its children are inert.
    /// It handles special cases for variant sets and prim hierarchies.
    /// </remarks>
    internal bool _IsInertSubtree(ISdfPath path)
    {
        if (!_IsInert(path, true /*ignoreChildren*/, true /* requiredFieldOnlyPropertiesAreInert */))
        {
            return false;
        }

        // Check for a variant set path first -- this is a variant selection path
        // whose selection is the empty string.
        if (path.IsPrimVariantSelectionPath() &&
            string.IsNullOrEmpty(path.GetVariantSelection().Item2))
        {
            var vsetName = path.GetVariantSelection().Item1;
            var parentPath = path.GetParentPath();

            List<TfToken> variants;
            if (HasField(path, SdfChildrenKeys.VariantChildren, out variants!))
            {
                foreach (var variant in variants ?? [])
                {
                    if (!_IsInertSubtree(parentPath.AppendVariantSelection(vsetName, variant.GetText())))
                        return false;
                }
            }
        }
        else if (path.IsPrimOrPrimVariantSelectionPath())
        {
            // Check for prim & variant set children.
            var childrenFields = new[] {
                SdfChildrenKeys.PrimChildren,
                SdfChildrenKeys.VariantSetChildren
            };

            foreach (var childrenField in childrenFields)
            {
                List<TfToken> childNames;
                if (HasField(path, childrenField, out childNames!))
                {
                    foreach (var name in childNames ?? [])
                    {
                        if (!_IsInertSubtree(path.AppendChild(name)))
                            return false;
                    }
                }
            }

            List<TfToken> properties;
            if (HasField(path, SdfChildrenKeys.PropertyChildren, out properties!))
            {
                foreach (var prop in properties ?? [])
                {
                    var propPath = path.AppendProperty(prop);
                    if (!_IsInert(propPath,
                        /* ignoreChildren = */ false,
                        /* requiredFieldOnlyPropertiesAreInert = */ true))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    
    /// <summary>
    /// Deletes the spec at the given path.
    /// </summary>
    /// <param name="path">The path to the spec to delete</param>
    /// <returns>True if the spec was deleted, false otherwise</returns>
    /// <remarks>
    /// This method is used internally by SdfSpec and other classes to remove specs
    /// from the layer. It properly handles inert subtrees by sending notifications
    /// for all affected specs before deletion.
    /// </remarks>
    internal bool _DeleteSpec(ISdfPath path)
    {
        if (!PermissionToEdit())
        {
            // TODO: Add proper error reporting
            // TF_CODING_ERROR("Cannot delete <%s>. Layer @%s@ is not editable",
            //                 path.GetText(), GetIdentifier().c_str());
            return false;
        }

        if (!HasSpec(path))
        {
            return false;
        }

        if (_IsInertSubtree(path))
        {
            // If the subtree is inert, enqueue notifications for each spec that's
            // about to be removed. _PrimDeleteSpec adds a notice for the spec
            // path it's given, but notices about inert specs don't imply anything
            // about descendants. So if we just sent out a notice for the subtree
            // root, clients would not be made aware of the removal of the other
            // specs in the subtree.
            
            // TODO: Implement SdfChangeBlock
            // SdfChangeBlock block;
            
            Traverse(path, (specPath) =>
            {
                // TODO: Implement change notifications
                // Sdf_ChangeManager::Get().DidRemoveSpec(_self, specPath, /* inert = */ true);
            });

            _PrimDeleteSpec(path, /* inert = */ true);
        }
        else
        {
            _PrimDeleteSpec(path, /* inert = */ false);
        }

        return true;
    }

    /// <summary>
    /// Core primitive spec deletion method.
    /// </summary>
    /// <param name="path">The path to the spec to delete</param>
    /// <param name="inert">Whether the spec is inert</param>
    /// <remarks>
    /// This method performs the actual deletion of a spec from the layer's data.
    /// It handles change notifications and updates the layer's dirty state.
    /// </remarks>
    private void _PrimDeleteSpec(ISdfPath path, bool inert)
    {
        // TODO: Implement SdfChangeBlock
        // SdfChangeBlock block;

        // TODO: Implement change notifications
        // Sdf_ChangeManager::Get().DidRemoveSpec(_self, path, inert);

        _data.EraseSpec(path);
        _isDirty = true;
    }

    /// <summary>
    /// Moves a spec from one path to another.
    /// </summary>
    /// <param name="oldPath">The current path of the spec</param>
    /// <param name="newPath">The new path for the spec</param>
    /// <returns>True if the spec was moved, false otherwise</returns>
    /// <remarks>
    /// This method is used internally to relocate specs within the layer hierarchy.
    /// </remarks>
    internal bool _MoveSpec(ISdfPath oldPath, ISdfPath newPath)
    {
        if (!PermissionToEdit())
        {
            return false;
        }

        if (!HasSpec(oldPath))
        {
            return false;
        }

        // TODO: Implement proper spec moving logic
        // For now, this is a simplified implementation
        _data.MoveSpec(oldPath, newPath);
        _isDirty = true;
        
        return true;
    }
    
    #region Metadata Methods
    
    /// <summary>
    /// Returns the comment string for this layer.
    /// </summary>
    public string GetComment()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Comment, string.Empty);
    }
    
    /// <summary>
    /// Sets the comment string for this layer.
    /// </summary>
    public void SetComment(string comment)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Comment, comment);
    }
    
    /// <summary>
    /// Returns the documentation string for this layer.
    /// </summary>
    public string GetDocumentation()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Documentation, string.Empty);
    }
    
    /// <summary>
    /// Sets the documentation string for this layer.
    /// </summary>
    public void SetDocumentation(string documentation)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Documentation, documentation);
    }
    
    /// <summary>
    /// Returns the default prim metadata for this layer.
    /// </summary>
    public TfToken GetDefaultPrim()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.DefaultPrim, TfToken.Empty);
    }
    
    /// <summary>
    /// Sets the default prim metadata for this layer.
    /// </summary>
    public void SetDefaultPrim(TfToken name)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.DefaultPrim, name);
    }
    
    /// <summary>
    /// Returns the default prim as an absolute path.
    /// </summary>
    public SdfPath GetDefaultPrimAsPath()
    {
        var defaultPrim = GetDefaultPrim();
        if (defaultPrim.IsEmpty)
            return SdfPath.EmptyPath();
            
        var primName = defaultPrim.GetText();
        if (primName.StartsWith("/"))
            return new SdfPath(primName);
        else
            return new SdfPath($"/{primName}");
    }
    
    /// <summary>
    /// Clears the default prim metadata.
    /// </summary>
    public void ClearDefaultPrim()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.DefaultPrim);
    }
    
    /// <summary>
    /// Returns true if default prim metadata is set.
    /// </summary>
    public bool HasDefaultPrim()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.DefaultPrim);
    }
    
    /// <summary>
    /// Returns the layer's start timeCode.
    /// </summary>
    public double GetStartTimeCode()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.StartTimeCode, 0.0);
    }
    
    /// <summary>
    /// Sets the layer's start timeCode.
    /// </summary>
    public void SetStartTimeCode(double startTimeCode)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.StartTimeCode, startTimeCode);
    }
    
    /// <summary>
    /// Returns true if the layer has a startTimeCode opinion.
    /// </summary>
    public bool HasStartTimeCode()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.StartTimeCode);
    }
    
    /// <summary>
    /// Clears the startTimeCode opinion.
    /// </summary>
    public void ClearStartTimeCode()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.StartTimeCode);
    }
    
    /// <summary>
    /// Returns the layer's end timeCode.
    /// </summary>
    public double GetEndTimeCode()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.EndTimeCode, 0.0);
    }
    
    /// <summary>
    /// Sets the layer's end timeCode.
    /// </summary>
    public void SetEndTimeCode(double endTimeCode)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.EndTimeCode, endTimeCode);
    }
    
    /// <summary>
    /// Returns true if the layer has an endTimeCode opinion.
    /// </summary>
    public bool HasEndTimeCode()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.EndTimeCode);
    }
    
    /// <summary>
    /// Clears the endTimeCode opinion.
    /// </summary>
    public void ClearEndTimeCode()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.EndTimeCode);
    }
    
    /// <summary>
    /// Returns the layer's time codes per second.
    /// </summary>
    public double GetTimeCodesPerSecond()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.TimeCodesPerSecond, 24.0);
    }
    
    /// <summary>
    /// Sets the layer's time codes per second.
    /// </summary>
    public void SetTimeCodesPerSecond(double timeCodesPerSecond)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.TimeCodesPerSecond, timeCodesPerSecond);
    }
    
    /// <summary>
    /// Returns true if the layer has a timeCodesPerSecond opinion.
    /// </summary>
    public bool HasTimeCodesPerSecond()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.TimeCodesPerSecond);
    }
    
    /// <summary>
    /// Clears the timeCodesPerSecond opinion.
    /// </summary>
    public void ClearTimeCodesPerSecond()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.TimeCodesPerSecond);
    }
    
    /// <summary>
    /// Returns the layer's frames per second.
    /// </summary>
    public double GetFramesPerSecond()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramesPerSecond, 24.0);
    }
    
    /// <summary>
    /// Sets the layer's frames per second.
    /// </summary>
    public void SetFramesPerSecond(double framesPerSecond)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramesPerSecond, framesPerSecond);
    }
    
    /// <summary>
    /// Returns true if the layer has a frames per second opinion.
    /// </summary>
    public bool HasFramesPerSecond()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramesPerSecond);
    }
    
    /// <summary>
    /// Clears the framesPerSecond opinion.
    /// </summary>
    public void ClearFramesPerSecond()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramesPerSecond);
    }
    
    /// <summary>
    /// Returns the layer's frame precision.
    /// </summary>
    public int GetFramePrecision()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramePrecision, 3);
    }
    
    /// <summary>
    /// Sets the layer's frame precision.
    /// </summary>
    public void SetFramePrecision(int framePrecision)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramePrecision, framePrecision);
    }
    
    /// <summary>
    /// Returns true if the layer has a frames precision opinion.
    /// </summary>
    public bool HasFramePrecision()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramePrecision);
    }
    
    /// <summary>
    /// Clears the framePrecision opinion.
    /// </summary>
    public void ClearFramePrecision()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.FramePrecision);
    }
    
    /// <summary>
    /// Returns the owner of this layer.
    /// </summary>
    public string GetOwner()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Owner, string.Empty);
    }
    
    /// <summary>
    /// Sets the owner of this layer.
    /// </summary>
    public void SetOwner(string owner)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Owner, owner);
    }
    
    /// <summary>
    /// Returns true if the layer has an owner opinion.
    /// </summary>
    public bool HasOwner()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Owner);
    }
    
    /// <summary>
    /// Clears the owner opinion.
    /// </summary>
    public void ClearOwner()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.Owner);
    }
    
    /// <summary>
    /// Returns the session owner of this layer.
    /// </summary>
    public string GetSessionOwner()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.SessionOwner, string.Empty);
    }
    
    /// <summary>
    /// Sets the session owner of this layer.
    /// </summary>
    public void SetSessionOwner(string sessionOwner)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.SessionOwner, sessionOwner);
    }
    
    /// <summary>
    /// Returns true if the layer has a session owner opinion.
    /// </summary>
    public bool HasSessionOwner()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.SessionOwner);
    }
    
    /// <summary>
    /// Clears the session owner opinion.
    /// </summary>
    public void ClearSessionOwner()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.SessionOwner);
    }
    
    /// <summary>
    /// Returns the CustomLayerData dictionary associated with this layer.
    /// </summary>
    public VtDictionary GetCustomLayerData()
    {
        return GetFieldAs(SdfPath.AbsoluteRootPath(), SdfFieldKeys.CustomData, new VtDictionary());
    }
    
    /// <summary>
    /// Sets the CustomLayerData dictionary associated with this layer.
    /// </summary>
    public void SetCustomLayerData(VtDictionary value)
    {
        SetField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.CustomData, value);
    }
    
    /// <summary>
    /// Returns true if the layer has CustomLayerData.
    /// </summary>
    public bool HasCustomLayerData()
    {
        return HasField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.CustomData);
    }
    
    /// <summary>
    /// Clears the CustomLayerData opinion.
    /// </summary>
    public void ClearCustomLayerData()
    {
        EraseField(SdfPath.AbsoluteRootPath(), SdfFieldKeys.CustomData);
    }
    
    #endregion
    
    #region Stub Implementations for Advanced Features
    
    // Color configuration methods (requires SdfAssetPath implementation)
    public SdfAssetPath GetColorConfiguration() => new SdfAssetPath(); 
    public void SetColorConfiguration(SdfAssetPath colorConfiguration) { /* TODO */ }
    public bool HasColorConfiguration() => false;
    public void ClearColorConfiguration() { /* TODO */ }
    
    public TfToken GetColorManagementSystem() => TfToken.Empty;
    public void SetColorManagementSystem(TfToken cms) { /* TODO */ }
    public bool HasColorManagementSystem() => false;
    public void ClearColorManagementSystem() { /* TODO */ }
    
    // Root prim management (requires proxy classes)
    public SdfPrimSpecHandleVector GetRootPrims() => new SdfPrimSpecHandleVector();
    public void SetRootPrims(SdfPrimSpecHandleVector rootPrims) { /* TODO */ }
    public void InsertRootPrim(SdfPrimSpec prim, int index = -1) { /* TODO */ }
    public void RemoveRootPrim(SdfPrimSpec prim) { /* TODO */ }
    
    public SdfNameOrderProxy GetRootPrimOrder() => new SdfNameOrderProxy();
    public void SetRootPrimOrder(IEnumerable<TfToken> names) { /* TODO */ }
    public void InsertInRootPrimOrder(TfToken name, int index = -1) { /* TODO */ }
    public void RemoveFromRootPrimOrder(TfToken name) { /* TODO */ }
    public void RemoveFromRootPrimOrderByIndex(int index) { /* TODO */ }
    public void ApplyRootPrimOrder(ref List<TfToken> vec) { /* TODO */ }
    public bool CanApplyRootPrimOrder(List<TfToken> vec) => false;
    
    // Sublayer management (requires proxy classes)
    public SdfSubLayerProxy GetSubLayerPaths() => new SdfSubLayerProxy();
    public void SetSubLayerPaths(IEnumerable<string> newPaths) { /* TODO */ }
    public int GetNumSubLayerPaths() => 0;
    public void InsertSubLayerPath(string path, int index = -1) { /* TODO */ }
    public void RemoveSubLayerPath(int index) { /* TODO */ }
    public SdfLayerOffsetVector GetSubLayerOffsets() => new SdfLayerOffsetVector();
    public SdfLayerOffset GetSubLayerOffset(int index) => new SdfLayerOffset();
    public void SetSubLayerOffset(SdfLayerOffset offset, int index) { /* TODO */ }
    
    // Muting
    public ISet<string> GetMutedLayers() => new HashSet<string>();
    public bool IsSubLayerMuted(string path) => false;
    public void SetSubLayerMuted(string path, bool muted) { /* TODO */ }
    
    // Relocates (requires proxy classes)
    public SdfRelocatesMapProxy GetRelocates() => new SdfRelocatesMapProxy();
    public void SetRelocates(SdfRelocatesMap relocatesMap) { /* TODO */ }
    public bool HasRelocates() => false;
    public void ClearRelocates() { /* TODO */ }
    
    // Time samples
    public int GetNumTimeSamplesForPath(ISdfPath path) => 0;
    public ISet<double> ListAllTimeSamples() => new HashSet<double>();
    public ISet<double> ListTimeSamplesForPath(ISdfPath path) => new HashSet<double>();
    public bool QueryTimeSample(ISdfPath path, double time, out VtValue value) { value = new VtValue(); return false; }
    public bool QueryTimeSample<T>(ISdfPath path, double time, out T value) { value = default(T)!; return false; }
    public void SetTimeSample(ISdfPath path, double time, VtValue value) { /* TODO */ }
    public void SetTimeSample<T>(ISdfPath path, double time, T value) { /* TODO */ }
    public void EraseTimeSample(ISdfPath path, double time) { /* TODO */ }
    
    // Composition
    public ISet<string> GetCompositionAssetDependencies() => new HashSet<string>();
    public bool UpdateCompositionAssetDependency(string oldAssetPath, string newAssetPath = "") => false;
    public ISet<string> GetExternalAssetDependencies() => new HashSet<string>();
    
    // Batch namespace editing (requires specialized classes)
    public SdfNamespaceEditDetail.Result CanApply(SdfBatchNamespaceEdit edit) => SdfNamespaceEditDetail.Result.Error;
    public bool Apply(SdfBatchNamespaceEdit edit) => false;
    
    // Debugging
    public void DumpLayerInfo(string filename = "") { /* TODO */ }
    
    #endregion
}