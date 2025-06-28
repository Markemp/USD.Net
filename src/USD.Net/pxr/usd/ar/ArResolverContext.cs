using System;
using System.Collections.Generic;

namespace Pxr.Usd.Ar;

/// <summary>
/// ArResolverContext provides context for asset and path resolution during scene composition.
/// It allows customization of how assets are found and resolved in the USD asset resolution system.
/// </summary>
public sealed class ArResolverContext
{
    private readonly Dictionary<string, object> _contextData = new();

    public ArResolverContext()
    {
    }

    public ArResolverContext(Dictionary<string, object> contextData)
    {
        _contextData = new Dictionary<string, object>(contextData ?? throw new ArgumentNullException(nameof(contextData)));
    }

    /// <summary>
    /// Return true if this context is empty.
    /// </summary>
    public bool IsEmpty() => _contextData.Count == 0;

    /// <summary>
    /// Get a string representation of this context.
    /// </summary>
    public string GetDebugString()
    {
        if (IsEmpty())
            return "ArResolverContext()";
        
        var items = new List<string>();
        foreach (var kvp in _contextData)
        {
            items.Add($"{kvp.Key}={kvp.Value}");
        }
        return $"ArResolverContext({string.Join(", ", items)})";
    }

    /// <summary>
    /// Set context data for the given key.
    /// </summary>
    public void SetContextData(string key, object value)
    {
        _contextData[key] = value;
    }

    /// <summary>
    /// Get context data for the given key.
    /// </summary>
    public T? GetContextData<T>(string key)
    {
        if (_contextData.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return default;
    }

    /// <summary>
    /// Return true if context data exists for the given key.
    /// </summary>
    public bool HasContextData(string key) => _contextData.ContainsKey(key);

    /// <summary>
    /// Remove context data for the given key.
    /// </summary>
    public bool RemoveContextData(string key) => _contextData.Remove(key);

    /// <summary>
    /// Clear all context data.
    /// </summary>
    public void Clear() => _contextData.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not ArResolverContext other)
            return false;

        if (_contextData.Count != other._contextData.Count)
            return false;

        foreach (var kvp in _contextData)
        {
            if (!other._contextData.TryGetValue(kvp.Key, out var otherValue) ||
                !Equals(kvp.Value, otherValue))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in _contextData)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    public override string ToString() => GetDebugString();
}