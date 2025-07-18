using System.Text;

namespace Pxr.Base.Vt;

/// <summary>
/// A dictionary with string keys and VtValue values, providing hierarchical 
/// key path operations and dictionary composition functionality.
/// </summary>
public class VtDictionary : Dictionary<string, VtValue>
{
    /// <summary>
    /// Creates an empty VtDictionary.
    /// </summary>
    public VtDictionary() : base()
    {
    }

    /// <summary>
    /// Creates an empty VtDictionary with the specified initial capacity.
    /// </summary>
    public VtDictionary(int capacity) : base(capacity)
    {
    }

    /// <summary>
    /// Creates a VtDictionary from a sequence of key-value pairs.
    /// </summary>
    public VtDictionary(IEnumerable<KeyValuePair<string, VtValue>> collection) : base(collection)
    {
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    public VtDictionary(VtDictionary other) : base(other)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the dictionary is empty.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Gets a value at the specified hierarchical key path.
    /// Key path elements are separated by the specified delimiters.
    /// </summary>
    public VtValue? GetValueAtPath(string keyPath, string delimiters = ":")
    {
        return GetValueAtPath(keyPath.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Gets a value at the specified hierarchical key path.
    /// </summary>
    public VtValue? GetValueAtPath(IEnumerable<string> keyPath)
    {
        var keyElems = keyPath.ToArray();
        if (keyElems.Length == 0)
            return null;

        // Walk through nested dictionaries up to the last element
        var dict = this;
        for (int i = 0; i < keyElems.Length - 1; i++)
        {
            if (!dict.TryGetValue(keyElems[i], out var value) ||
                !value.IsHolding<VtDictionary>())
                return null;

            // Navigate to the nested dictionary
            dict = value.UncheckedGet<VtDictionary>();
        }

        // Look for the final key in the deepest dictionary
        return dict.TryGetValue(keyElems[^1], out var finalValue) ? finalValue : null;
    }

    /// <summary>
    /// Sets a value at the specified hierarchical key path.
    /// Creates sub-dictionaries as necessary.
    /// </summary>
    public void SetValueAtPath(string keyPath, VtValue value, string delimiters = ":")
    {
        SetValueAtPath(keyPath.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries), value);
    }

    /// <summary>
    /// Sets a value at the specified hierarchical key path.
    /// Creates sub-dictionaries as necessary.
    /// </summary>
    public void SetValueAtPath(IEnumerable<string> keyPath, VtValue value)
    {
        var keyElems = keyPath.ToArray();
        if (keyElems.Length == 0)
            return;

        SetValueAtPathImpl(keyElems, 0, value);
    }

    /// <summary>
    /// Erases the value at the specified hierarchical key path.
    /// </summary>
    public void EraseValueAtPath(string keyPath, string delimiters = ":")
    {
        EraseValueAtPath(keyPath.Split(delimiters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Erases the value at the specified hierarchical key path.
    /// </summary>
    public void EraseValueAtPath(IEnumerable<string> keyPath)
    {
        var keyElems = keyPath.ToArray();
        if (keyElems.Length == 0)
            return;

        EraseValueAtPathImpl(keyElems, 0);
    }

    private void SetValueAtPathImpl(string[] keyElems, int index, VtValue value)
    {
        // If we're at the last element, set the value directly
        if (index == keyElems.Length - 1)
        {
            this[keyElems[index]] = value;
            return;
        }

        // Get or create subdictionary
        var key = keyElems[index];
        if (!TryGetValue(key, out var existing) || !existing.IsHolding<VtDictionary>())
        {
            this[key] = new VtValue(new VtDictionary());
        }

        var subDict = this[key].UncheckedGet<VtDictionary>();
        subDict.SetValueAtPathImpl(keyElems, index + 1, value);
    }

    private void EraseValueAtPathImpl(string[] keyElems, int index)
    {
        // If we're at the last element, erase it
        if (index == keyElems.Length - 1)
        {
            Remove(keyElems[index]);
            return;
        }

        // Navigate to subdictionary
        var key = keyElems[index];
        if (TryGetValue(key, out var value) && value.IsHolding<VtDictionary>())
        {
            var subDict = value.UncheckedGet<VtDictionary>();
            subDict.EraseValueAtPathImpl(keyElems, index + 1);

            // Remove empty subdictionaries
            if (subDict.IsEmpty)
                Remove(key);
        }
    }

    /// <summary>
    /// Swaps the contents of this dictionary with another.
    /// </summary>
    public void Swap(VtDictionary other)
    {
        // Create temporary copies
        var thisItems = this.ToArray();
        var otherItems = other.ToArray();

        // Clear both dictionaries
        this.Clear();
        other.Clear();

        // Add items to swapped dictionaries
        foreach (var item in otherItems)
            this.Add(item.Key, item.Value);

        foreach (var item in thisItems)
            other.Add(item.Key, item.Value);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('{');

        bool first = true;
        foreach (var (key, value) in this)
        {
            if (!first) sb.Append(", ");
            sb.Append($"'{key}': {value}");
            first = false;
        }

        sb.Append('}');
        return sb.ToString();
    }
}

/// <summary>
/// Static utility class for VtDictionary operations.
/// </summary>
public static class VtDictionaryUtils
{
    private static readonly VtDictionary EmptyDictionary = new();

    /// <summary>
    /// Returns a reference to an empty VtDictionary.
    /// </summary>
    public static VtDictionary GetEmpty() => EmptyDictionary;

    /// <summary>
    /// Checks if a dictionary contains a key with a value of type T.
    /// </summary>
    public static bool IsHolding<T>(VtDictionary dictionary, string key)
    {
        return dictionary.TryGetValue(key, out var value) && value.IsHolding<T>();
    }

    /// <summary>
    /// Gets a value from the dictionary, throwing if key doesn't exist or type doesn't match.
    /// </summary>
    public static T Get<T>(VtDictionary dictionary, string key)
    {
        if (!dictionary.TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Key '{key}' not found in dictionary");

        return value.Get<T>();
    }

    /// <summary>
    /// Gets a value from the dictionary with a default fallback.
    /// </summary>
    public static T Get<T>(VtDictionary dictionary, string key, T defaultValue)
    {
        if (dictionary.TryGetValue(key, out var value) && value.IsHolding<T>())
            return value.UncheckedGet<T>();

        return defaultValue;
    }

    /// <summary>
    /// Creates a dictionary containing strong composed over weak.
    /// The result contains all key-value pairs from strong plus
    /// key-value pairs from weak whose keys are not in strong.
    /// </summary>
    public static VtDictionary Over(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        var result = new VtDictionary(strong);
        OverInPlace(result, weak, coerceToWeakerOpinionType);
        return result;
    }

    /// <summary>
    /// Updates strong to become strong composed over weak.
    /// The strong dictionary is modified in place.
    /// </summary>
    public static void OverInPlace(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        // Add entries from weak that don't exist in strong
        foreach (var (key, value) in weak)
        {
            if (!strong.ContainsKey(key))
                strong[key] = value;
        }

        if (coerceToWeakerOpinionType)
        {
            foreach (var (key, value) in strong)
            {
                if (weak.TryGetValue(key, out var weakValue))
                    value.CastToTypeOf(weakValue);
            }
        }
    }

    /// <summary>
    /// Updates weak to become strong composed over weak.
    /// The weak dictionary is modified in place to contain the merged result.
    /// </summary>
    public static void OverIntoWeak(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        if (coerceToWeakerOpinionType)
        {
            foreach (var (key, strongValue) in strong)
            {
                if (!weak.TryGetValue(key, out var weakValue))
                {
                    weak[key] = strongValue;
                }
                else
                {
                    weak[key] = VtValue.CastToTypeOf(strongValue, weakValue);
                }
            }
        }
        else
        {
            // Can't use Dictionary.Add here because that doesn't overwrite
            // values for keys in strong that are already in weak.
            foreach (var (key, value) in strong)
            {
                weak[key] = value;
            }
        }
    }

    /// <summary>
    /// Creates a dictionary containing strong recursively composed over weak.
    /// </summary>
    public static VtDictionary OverRecursive(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        var result = new VtDictionary(strong);
        OverRecursiveInPlace(result, weak, coerceToWeakerOpinionType);
        return result;
    }

    /// <summary>
    /// Updates strong to become strong recursively composed over weak.
    /// The strong dictionary is modified in place.
    /// </summary>
    public static void OverRecursiveInPlace(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        foreach (var (key, weakValue) in weak)
        {
            if (IsHolding<VtDictionary>(strong, key) && weakValue.IsHolding<VtDictionary>())
            {
                // Both have subdictionaries - merge recursively
                var strongSubDict = Get<VtDictionary>(strong, key);
                var weakSubDict = weakValue.UncheckedGet<VtDictionary>();
                OverRecursiveInPlace(strongSubDict, weakSubDict, coerceToWeakerOpinionType);
            }
            else if (!strong.ContainsKey(key))
            {
                // Strong doesn't have this key - add from weak
                strong[key] = weakValue;
            }
            else if (coerceToWeakerOpinionType)
            {
                // Strong has key but we want to coerce types
                strong[key].CastToTypeOf(weakValue);
            }
        }
    }

    /// <summary>
    /// Updates weak to become strong recursively composed over weak.
    /// The weak dictionary is modified in place to contain the merged result.
    /// </summary>
    public static void OverRecursiveIntoWeak(VtDictionary strong, VtDictionary weak, bool coerceToWeakerOpinionType = false)
    {
        foreach (var (key, strongValue) in strong)
        {
            if (IsHolding<VtDictionary>(weak, key) && strongValue.IsHolding<VtDictionary>())
            {
                // Both have subdictionaries - merge recursively
                var weakSubDict = Get<VtDictionary>(weak, key);
                var strongSubDict = strongValue.UncheckedGet<VtDictionary>();
                OverRecursiveIntoWeak(strongSubDict, weakSubDict, coerceToWeakerOpinionType);
            }
            else if (coerceToWeakerOpinionType && weak.TryGetValue(key, out var weakValue))
            {
                // Coerce strong value to weak value's type, then store
                weak[key] = VtValue.CastToTypeOf(strongValue, weakValue);
            }
            else
            {
                // Overwrite weak with strong (or add if not present)
                weak[key] = strongValue;
            }
        }
    }
}