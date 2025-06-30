using System;
using System.Collections.Generic;
using System.Linq;

namespace Pxr.Usd;

/// <summary>
/// UsdVariantSets represents the collection of all VariantSets present on a UsdPrim.
/// This provides access to multiple variant sets (e.g., "modelingVariant", "shadingVariant") 
/// that can exist simultaneously on a single prim.
/// </summary>
public class UsdVariantSets
{
    private readonly UsdPrim _prim;
    private readonly Dictionary<string, UsdVariantSet> _variantSets = new();

    #region Construction

    /// <summary>
    /// Create an invalid UsdVariantSets object.
    /// </summary>
    public UsdVariantSets()
    {
        _prim = new UsdPrim();
    }

    /// <summary>
    /// Create a UsdVariantSets object for the given prim.
    /// </summary>
    public UsdVariantSets(UsdPrim prim)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
    }

    #endregion

    #region Validity

    /// <summary>
    /// Return true if this UsdVariantSets object is valid.
    /// </summary>
    public bool IsValid() => _prim.IsValid();

    /// <summary>
    /// Get the prim that owns these variant sets.
    /// </summary>
    public UsdPrim GetPrim() => _prim;

    #endregion

    #region Variant Set Management

    /// <summary>
    /// Add a variant set with the given name.
    /// </summary>
    public UsdVariantSet AddVariantSet(string variantSetName, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantSetName))
            return new UsdVariantSet(); // Invalid variant set

        // Check if variant set already exists
        if (_variantSets.ContainsKey(variantSetName))
            return _variantSets[variantSetName];

        // Create new variant set
        var variantSet = new UsdVariantSet(_prim, variantSetName);
        _variantSets[variantSetName] = variantSet;

        return variantSet;
    }

    /// <summary>
    /// Get the variant set with the given name.
    /// </summary>
    public UsdVariantSet GetVariantSet(string variantSetName)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantSetName))
            return new UsdVariantSet(); // Invalid variant set

        if (_variantSets.TryGetValue(variantSetName, out var variantSet))
            return variantSet;

        // Create variant set on demand if it doesn't exist
        var newVariantSet = new UsdVariantSet(_prim, variantSetName);
        _variantSets[variantSetName] = newVariantSet;
        return newVariantSet;
    }

    /// <summary>
    /// Array-style access to variant sets by name.
    /// </summary>
    public UsdVariantSet this[string variantSetName] => GetVariantSet(variantSetName);

    /// <summary>
    /// Return true if this prim has a variant set with the given name.
    /// </summary>
    public bool HasVariantSet(string variantSetName)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantSetName))
            return false;

        return _variantSets.ContainsKey(variantSetName);
    }

    /// <summary>
    /// Get the names of all variant sets on this prim.
    /// </summary>
    public string[] GetNames()
    {
        if (!IsValid())
            return Array.Empty<string>();

        return _variantSets.Keys.ToArray();
    }

    /// <summary>
    /// Get the number of variant sets on this prim.
    /// </summary>
    public int GetNumVariantSets()
    {
        return _variantSets.Count;
    }

    /// <summary>
    /// Remove a variant set with the given name.
    /// </summary>
    public bool RemoveVariantSet(string variantSetName)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantSetName))
            return false;

        return _variantSets.Remove(variantSetName);
    }

    /// <summary>
    /// Clear all variant sets.
    /// </summary>
    public bool ClearVariantSets()
    {
        if (!IsValid())
            return false;

        _variantSets.Clear();
        return true;
    }

    #endregion

    #region Selection Management

    /// <summary>
    /// Get the variant selection for the specified variant set.
    /// </summary>
    public string GetVariantSelection(string variantSetName)
    {
        var variantSet = GetVariantSet(variantSetName);
        return variantSet.IsValid() ? variantSet.GetVariantSelection() : string.Empty;
    }

    /// <summary>
    /// Set the variant selection for the specified variant set.
    /// </summary>
    public bool SetSelection(string variantSetName, string variantName)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantSetName))
            return false;

        var variantSet = GetVariantSet(variantSetName);
        return variantSet.IsValid() && variantSet.SetVariantSelection(variantName);
    }

    /// <summary>
    /// Get all variant selections as a dictionary of variant set name to selected variant name.
    /// </summary>
    public Dictionary<string, string> GetAllVariantSelections()
    {
        var selections = new Dictionary<string, string>();

        if (!IsValid())
            return selections;

        foreach (var kvp in _variantSets)
        {
            var variantSet = kvp.Value;
            if (variantSet.IsValid() && variantSet.HasAuthoredVariantSelection())
            {
                selections[kvp.Key] = variantSet.GetVariantSelection();
            }
        }

        return selections;
    }

    /// <summary>
    /// Set multiple variant selections at once.
    /// </summary>
    public bool SetSelections(Dictionary<string, string> selections)
    {
        if (!IsValid() || selections == null)
            return false;

        bool allSucceeded = true;
        foreach (var kvp in selections)
        {
            if (!SetSelection(kvp.Key, kvp.Value))
                allSucceeded = false;
        }

        return allSucceeded;
    }

    #endregion

    #region Iteration Support

    /// <summary>
    /// Get all variant sets as an enumerable collection.
    /// </summary>
    public IEnumerable<UsdVariantSet> GetVariantSets()
    {
        return _variantSets.Values.Where(vs => vs.IsValid());
    }

    /// <summary>
    /// Get all variant set names and their corresponding variant sets.
    /// </summary>
    public IEnumerable<KeyValuePair<string, UsdVariantSet>> GetVariantSetsWithNames()
    {
        return _variantSets.Where(kvp => kvp.Value.IsValid());
    }

    #endregion

    #region Validation

    /// <summary>
    /// Return true if the given variant set name is valid.
    /// </summary>
    public bool IsValidVariantSetName(string variantSetName)
    {
        if (string.IsNullOrEmpty(variantSetName))
            return false;

        // Basic validation - variant set names should be valid identifiers
        // In USD, variant set names have some restrictions similar to property names
        return variantSetName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    #endregion

    #region Composition Support

    /// <summary>
    /// Return true if this prim has any authored variant sets.
    /// </summary>
    public bool HasAuthoredVariantSets()
    {
        return _variantSets.Count > 0;
    }

    /// <summary>
    /// Return true if this prim has any authored variant selections.
    /// </summary>
    public bool HasAuthoredVariantSelections()
    {
        return _variantSets.Values.Any(vs => vs.IsValid() && vs.HasAuthoredVariantSelection());
    }

    /// <summary>
    /// Get the total number of variants across all variant sets.
    /// </summary>
    public int GetTotalVariantCount()
    {
        return _variantSets.Values.Where(vs => vs.IsValid()).Sum(vs => vs.GetNumVariants());
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this variant sets collection.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "UsdVariantSets(invalid)";

        var primPath = _prim.GetPath().GetString();
        var numSets = GetNumVariantSets();
        var totalVariants = GetTotalVariantCount();
        var numSelections = _variantSets.Values.Count(vs => vs.IsValid() && vs.HasAuthoredVariantSelection());

        return $"UsdVariantSets({primPath}, {numSets} sets, {totalVariants} variants, {numSelections} selections)";
    }

    #endregion
}