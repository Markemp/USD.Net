using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdVariantSet represents a single named VariantSet on a UsdPrim.
/// Variant sets enable conditional scene description by providing alternative versions
/// of scene content that can be selected at composition time.
/// </summary>
public class UsdVariantSet
{
    private readonly UsdPrim _prim;
    private readonly string _variantSetName;
    private readonly Dictionary<string, List<string>> _variants = new();
    private string? _selection = null;
    private bool _hasAuthoredSelection = false;

    #region Construction

    /// <summary>
    /// Create an invalid UsdVariantSet.
    /// </summary>
    public UsdVariantSet()
    {
        _prim = new UsdPrim();
        _variantSetName = string.Empty;
    }

    /// <summary>
    /// Create a UsdVariantSet for the given prim and variant set name.
    /// </summary>
    public UsdVariantSet(UsdPrim prim, string variantSetName)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
        _variantSetName = variantSetName ?? throw new ArgumentNullException(nameof(variantSetName));
    }

    #endregion

    #region Validity and Identity

    /// <summary>
    /// Return true if this UsdVariantSet is valid.
    /// </summary>
    public bool IsValid() => _prim.IsValid() && !string.IsNullOrEmpty(_variantSetName);

    /// <summary>
    /// Get the prim that owns this variant set.
    /// </summary>
    public UsdPrim GetPrim() => _prim;

    /// <summary>
    /// Get the name of this variant set.
    /// </summary>
    public string GetName() => _variantSetName;

    #endregion

    #region Variant Management

    /// <summary>
    /// Add a variant with the given name to this variant set.
    /// </summary>
    public bool AddVariant(string variantName, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantName))
            return false;

        // Initialize variants list for this set if needed
        if (!_variants.ContainsKey(_variantSetName))
        {
            _variants[_variantSetName] = new List<string>();
        }

        var variantList = _variants[_variantSetName];

        // Check if variant already exists
        if (variantList.Contains(variantName))
            return true; // Already exists, consider success

        // Add to the appropriate position
        switch (position)
        {
            case UsdListPosition.FrontOfPrependList:
                variantList.Insert(0, variantName);
                break;
            case UsdListPosition.BackOfPrependList:
                variantList.Add(variantName);
                break;
            case UsdListPosition.FrontOfAppendList:
                // For simplicity, treat append positions the same as prepend
                variantList.Insert(0, variantName);
                break;
            case UsdListPosition.BackOfAppendList:
                variantList.Add(variantName);
                break;
        }

        return true;
    }

    /// <summary>
    /// Get the names of all variants in this variant set.
    /// </summary>
    public string[] GetVariantNames()
    {
        if (!IsValid() || !_variants.ContainsKey(_variantSetName))
            return Array.Empty<string>();

        return _variants[_variantSetName].ToArray();
    }

    /// <summary>
    /// Return true if this variant set has a variant with the given name.
    /// </summary>
    public bool HasAuthoredVariant(string variantName)
    {
        if (!IsValid() || string.IsNullOrEmpty(variantName))
            return false;

        return _variants.ContainsKey(_variantSetName) && 
               _variants[_variantSetName].Contains(variantName);
    }

    /// <summary>
    /// Get the number of variants in this variant set.
    /// </summary>
    public int GetNumVariants()
    {
        if (!IsValid() || !_variants.ContainsKey(_variantSetName))
            return 0;

        return _variants[_variantSetName].Count;
    }

    #endregion

    #region Selection Management

    /// <summary>
    /// Get the variant selection for this variant set.
    /// </summary>
    public string GetVariantSelection()
    {
        return _selection ?? string.Empty;
    }

    /// <summary>
    /// Return true if this variant set has an authored variant selection.
    /// </summary>
    public bool HasAuthoredVariantSelection()
    {
        return _hasAuthoredSelection;
    }

    /// <summary>
    /// Return true if this variant set has an authored variant selection, and if so, return it.
    /// </summary>
    public bool HasAuthoredVariantSelection(out string selection)
    {
        selection = _selection ?? string.Empty;
        return _hasAuthoredSelection;
    }

    /// <summary>
    /// Set the variant selection for this variant set.
    /// </summary>
    public bool SetVariantSelection(string variantName)
    {
        if (!IsValid())
            return false;

        // Empty string is valid (means no selection or blocking)
        _selection = variantName ?? string.Empty;
        _hasAuthoredSelection = true;
        return true;
    }

    /// <summary>
    /// Clear the variant selection for this variant set.
    /// This allows weaker variant selections to take effect.
    /// </summary>
    public bool ClearVariantSelection()
    {
        if (!IsValid())
            return false;

        _selection = null;
        _hasAuthoredSelection = false;
        return true;
    }

    /// <summary>
    /// Block variant selection by authoring an empty selection.
    /// This prevents weaker layers from providing variant selections.
    /// </summary>
    public bool BlockVariantSelection()
    {
        if (!IsValid())
            return false;

        _selection = string.Empty;
        _hasAuthoredSelection = true;
        return true;
    }

    #endregion

    #region Edit Context Support

    /// <summary>
    /// Get an edit target that directs edits to the currently selected variant.
    /// </summary>
    public UsdEditTarget GetVariantEditTarget(SdfLayer? layer = null)
    {
        if (!IsValid() || string.IsNullOrEmpty(_selection))
            return new UsdEditTarget(); // Invalid edit target

        // For now, return a basic edit target
        // TODO: Implement proper variant path mapping when SdfLayer supports variants
        var stage = _prim.GetStage();
        var rootLayer = layer ?? stage?.GetRootLayer();
        if (rootLayer == null)
            return new UsdEditTarget();

        return new UsdEditTarget(rootLayer);
    }

    /// <summary>
    /// Get an edit context that directs edits to the currently selected variant.
    /// This provides RAII-style scoped editing within the variant.
    /// </summary>
    public UsdEditContext GetVariantEditContext(SdfLayer? layer = null)
    {
        var editTarget = GetVariantEditTarget(layer);
        if (!editTarget.IsValid())
            return new UsdEditContext(); // Invalid context

        var stage = _prim.GetStage();
        if (stage == null)
            return new UsdEditContext();

        return new UsdEditContext(stage, editTarget);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Return true if the given variant name is valid for this variant set.
    /// </summary>
    public bool IsValidVariantName(string variantName)
    {
        if (string.IsNullOrEmpty(variantName))
            return false;

        // Basic validation - variant names should be valid identifiers
        // In USD, variant names have some restrictions similar to prim names
        return variantName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this variant set.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "UsdVariantSet(invalid)";

        var primPath = _prim.GetPath().GetString();
        var numVariants = GetNumVariants();
        var selection = GetVariantSelection();
        var selectionStr = !string.IsNullOrEmpty(selection) ? $", selected='{selection}'" : "";
        
        return $"UsdVariantSet({primPath}[{_variantSetName}], {numVariants} variants{selectionStr})";
    }

    #endregion
}