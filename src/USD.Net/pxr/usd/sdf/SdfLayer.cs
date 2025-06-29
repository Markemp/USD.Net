using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfLayer represents a scene description container that can combine with other layers to form compositions.
/// It stores scene description data and participates in USD's layered composition system.
/// </summary>
public class SdfLayer
{
    private readonly Dictionary<string, object> _metadata = new();
    private readonly string _identifier;
    private bool _isDirty = false;
    private static readonly Dictionary<string, SdfLayer> _layerRegistry = new();

    public SdfLayer(string identifier)
    {
        _identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
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
            // TODO: Implement actual USD file format export
            // For now, this is a placeholder that always succeeds
            // When USD file format support is added, this should write proper USD files
            
            if (!string.IsNullOrEmpty(comment))
                SetMetadata(new TfToken("comment"), new VtValue(comment));
            
            return true;
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
        _metadata[key.GetText()] = value.GetValue() ?? throw new ArgumentNullException(nameof(value));
        _isDirty = true;
    }

    /// <summary>
    /// Get metadata from this layer.
    /// </summary>
    public VtValue GetMetadata(TfToken key)
    {
        return _metadata.TryGetValue(key.GetText(), out var value) 
            ? new VtValue(value) 
            : VtValue.CreateEmpty();
    }

    /// <summary>
    /// Return true if this layer has metadata with the given key.
    /// </summary>
    public bool HasMetadata(TfToken key) => _metadata.ContainsKey(key.GetText());

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public void ClearMetadata(TfToken key)
    {
        if (_metadata.Remove(key.GetText()))
            _isDirty = true;
    }

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
            // TODO: Implement actual file loading when file format support is added
            // For now, create a new layer for any identifier
            return new SdfLayer(identifier);
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
}