using System.Collections.Concurrent;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

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
public class SdfLayer
{
    private readonly Dictionary<string, object> _metadata = new();
    private readonly Dictionary<SdfPath, SdfPrimSpec> _primSpecs = new();
    private readonly string _identifier;
    private bool _isDirty = false;
    private static readonly ConcurrentDictionary<string, SdfLayer> _layerRegistry = new();
    private readonly ISdfSchemaBase _schema;
    private SdfData _data;

    public SdfLayer(string identifier, ISdfSchemaBase? schema = null)
    {
        _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        _schema = schema ?? new SdfSchema(); // You'll need a default schema
        _data = new SdfData(); // Initialize the data container

        _layerRegistry[identifier] = this;
    }

    /// <summary>
    /// Get the identifier for this layer.
    /// </summary>
    public string GetIdentifier() => _identifier;

    /// <summary>
    /// Get the display name for this layer.
    /// </summary>
    public string GetDisplayName() => _identifier;

    /// <summary>
    /// Return true if this layer has been modified.
    /// </summary>
    public bool IsDirty()
    {
        return _isDirty;
    }

    /// <summary>
    /// Save this layer to its persistent representation.
    /// </summary>
    public bool Save(bool force = false)
    {
        if (!_isDirty && !force)
            return true;
            
        try
        {
            // For now, just mark as clean since we don't have actual file I/O
            // TODO: Implement actual file writing when file format support is added
            _isDirty = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Export this layer to a file.
    /// </summary>
    public bool Export(string filename, string? comment = null)
    {
        if (string.IsNullOrEmpty(filename))
            return false;
            
        try
        {
            if (!string.IsNullOrEmpty(comment))
                SetMetadata(new TfToken("comment"), new VtValue(comment));
            
            // Basic USDA export for layers
            // TODO: Implement full layer serialization when we have proper layer content support
            var content = new System.Text.StringBuilder();
            content.AppendLine("#usda 1.0");
            
            // Write metadata if any
            if (_metadata.Count > 0 || !string.IsNullOrEmpty(comment))
            {
                content.AppendLine("(");
                
                // Write comment first if present
                if (_metadata.TryGetValue("comment", out var commentValue))
                {
                    content.AppendLine($"    doc = \"{commentValue}\"");
                }
                
                // Write other metadata
                foreach (var kvp in _metadata.Where(m => m.Key != "comment").OrderBy(m => m.Key))
                {
                    content.AppendLine($"    {kvp.Key} = {FormatMetadataValue(kvp.Value)}");
                }
                
                content.AppendLine(")");
                content.AppendLine();
            }
            
            // Write root prim specs
            foreach (var primSpec in GetRootPrimSpecs().OrderBy(p => p.GetName()))
            {
                WritePrimSpec(content, primSpec, 0);
            }
            
            // Write to file
            System.IO.File.WriteAllText(filename, content.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private string FormatMetadataValue(object value)
    {
        // Simple formatting for common types
        if (value is string s)
            return $"\"{s}\"";
        else if (value is bool b)
            return b ? "true" : "false";
        else if (value is VtValue vt)
        {
            if (vt.IsHolding<string>())
                return $"\"{vt.Get<string>()}\"";
            else
                return vt.ToString();
        }
        else
            return value.ToString() ?? "None";
    }

    /// <summary>
    /// Write a prim spec to the content buffer with proper indentation.
    /// </summary>
    private void WritePrimSpec(System.Text.StringBuilder content, SdfPrimSpec primSpec, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var typeName = primSpec.GetTypeName();
        var primName = primSpec.GetName();
        var specifier = primSpec.GetSpecifier();

        // Write prim definition
        if (string.IsNullOrEmpty(typeName))
            content.AppendLine($"{indent}{specifier} \"{primName}\"");
        else
            content.AppendLine($"{indent}{specifier} {typeName} \"{primName}\"");

        content.AppendLine($"{indent}{{");

        // Write properties (attributes)
        foreach (var property in primSpec.GetProperties().OrderBy(p => p.GetName()))
        {
            WritePropertySpec(content, property, indentLevel + 1);
        }

        // Write child prims
        foreach (var child in primSpec.GetChildren().OrderBy(c => c.GetName()))
        {
            WritePrimSpec(content, child, indentLevel + 1);
        }

        content.AppendLine($"{indent}}}");
        content.AppendLine(); // Empty line between prims
    }

    /// <summary>
    /// Write a property spec to the content buffer.
    /// </summary>
    private void WritePropertySpec(System.Text.StringBuilder content, SdfPropertySpec propertySpec, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var propName = propertySpec.GetName();
        var typeName = propertySpec.GetTypeName();
        var defaultValue = propertySpec.GetDefaultValue();
        var variability = propertySpec.GetVariability();

        if (defaultValue != null && !defaultValue.IsEmpty())
        {
            var valueStr = FormatPropertyValue(defaultValue);
            
            if (variability == UsdVariability.Uniform)
                content.AppendLine($"{indent}uniform {typeName} {propName} = {valueStr}");
            else
                content.AppendLine($"{indent}{typeName} {propName} = {valueStr}");
        }
        else
        {
            // Property without default value
            content.AppendLine($"{indent}{typeName} {propName}");
        }
    }

    /// <summary>
    /// Format a property value for USDA output.
    /// </summary>
    private string FormatPropertyValue(VtValue value)
    {
        if (value.IsHolding<string>())
            return $"\"{value.Get<string>()}\"";
        else if (value.IsHolding<bool>())
            return value.Get<bool>() ? "true" : "false";
        else if (value.IsHolding<int>())
            return value.Get<int>().ToString();
        else if (value.IsHolding<float>())
            return value.Get<float>().ToString("G");
        else if (value.IsHolding<double>())
            return value.Get<double>().ToString("G");
        else
            return value.ToString();
    }

    /// <summary>
    /// Clear all content from this layer.
    /// </summary>
    public void Clear()
    {
        _metadata.Clear();
        _isDirty = true;
    }

    /// <summary>
    /// Reload this layer from its persistent representation.
    /// </summary>
    public bool Reload(bool force = false)
    {
        try
        {
            // TODO: Implement actual file reloading when file format support is added
            // For now, this is a placeholder that clears the layer and marks as clean
            
            if (force || _isDirty)
            {
                Clear();
                _isDirty = false;
            }
            
            return true;
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
    /// Get all metadata from this layer.
    /// </summary>
    public IReadOnlyDictionary<string, object> GetAllMetadata() => _metadata;

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public void ClearMetadata(TfToken key)
    {
        if (_metadata.Remove(key.GetText()))
            _isDirty = true;
    }

    /// <summary>
    /// Get the schema for this layer
    /// </summary>
    public ISdfSchemaBase GetSchema() => _schema;

    /// <summary>
    /// Check if this layer has a spec at the given path
    /// </summary>
    public bool HasSpec(SdfPath path) => _data.HasSpec(path);

    /// <summary>
    /// Get the spec type at the given path
    /// </summary>
    public SdfSpecType GetSpecType(SdfPath path) => _data.GetSpecType(path);

    /// <summary>
    /// Get a field value at the given path
    /// </summary>
    public VtValue GetField(SdfPath path, TfToken fieldName) => _data.Get(path, fieldName);

    /// <summary>
    /// Check if a field exists at the given path
    /// </summary>
    public bool HasField(SdfPath path, TfToken fieldName) => _data.Has(path, fieldName);

    /// <summary>
    /// Set a field value at the given path
    /// </summary>
    public void SetField(SdfPath path, TfToken fieldName, VtValue value)
    {
        if (!PermissionToEdit()) return;

        _data.Set(path, fieldName, value);
        _isDirty = true;
    }

    /// <summary>
    /// Check if we have permission to edit this layer
    /// </summary>
    public bool PermissionToEdit() => true; // Simplified for now

    /// <summary>
    /// Create a new layer in memory.
    /// </summary>
    public static SdfLayer CreateNew(string identifier) => new SdfLayer(identifier);

    /// <summary>
    /// Find or open a layer with the given identifier.
    /// </summary>
    public static SdfLayer? FindOrOpen(string identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return null;
            
        // Check if layer is already in registry
        if (_layerRegistry.TryGetValue(identifier, out var existingLayer))
            return existingLayer;
            
        try
        {
            // Try to load from file if it exists
            if (File.Exists(identifier))
            {
                var layer = new SdfLayer(identifier);
                
                // Parse USDA files
                if (identifier.EndsWith(".usda", StringComparison.OrdinalIgnoreCase))
                {
                    var parser = new UsdUtils.UsdaParser();
                    if (parser.ParseFile(identifier, layer))
                    {
                        _layerRegistry[identifier] = layer;
                        return layer;
                    }
                }
            }
            
            // Fallback: create a new empty layer
            var newLayer = new SdfLayer(identifier);
            _layerRegistry[identifier] = newLayer;
            return newLayer;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Create an anonymous layer.
    /// </summary>
    public static SdfLayer CreateAnonymous(string? tag = null)
    {
        var identifier = $"anon:{Guid.NewGuid():N}";
        if (!string.IsNullOrEmpty(tag))
            identifier += $":{tag}";
        
        return new SdfLayer(identifier);
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
    public SdfPrimSpec? GetPrimSpec(SdfPath path)
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
    public bool HasPrimSpec(SdfPath path) => _primSpecs.ContainsKey(path);

    /// <summary>
    /// Remove a prim spec from this layer.
    /// </summary>
    public bool RemovePrimSpec(SdfPath path)
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
    public List<TfToken> ListFields(SdfPath path) => SdfData.ListFields(_schema, _data, path);

    /// <summary>
    /// Static helper method for listing fields (matches C++ _ListFields)
    /// </summary>
    private static List<TfToken> _ListFields(ISdfSchemaBase schema, SdfData data, SdfPath path)
        => SdfData.ListFields(schema, data, path);

    /// <summary>
    /// Removes a field from a spec at the given path.
    /// </summary>
    /// <param name="path">The path to the spec.</param>
    /// <param name="fieldName">The name of the field to erase.</param>
    public void EraseField(SdfPath path, TfToken fieldName)
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
    public void EraseFieldDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath)
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
    private SdfSchemaFieldDefinition? _GetRequiredFieldDef(SdfPath path, TfToken fieldName, SdfSpecType? specType = null)
    {
        var schema = GetSchema();
        if (schema.IsRequiredFieldName(fieldName))
        {
            specType ??= GetSpecType(path);
            var specDef = schema.GetSpecDefinition(specType.Value);
            if (specDef != null && specDef.IsRequiredField(fieldName))
            {
                return schema.GetFieldDefinition(fieldName);
            }
        }
        return null;
    }
}