using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdReferences provides an interface to a prim's references.
/// References enable the composition of scene description from multiple layers,
/// allowing rich scenes to be built by composing scene description.
/// </summary>
public class UsdReferences
{
    private readonly UsdPrim _prim;
    private readonly List<SdfReference> _references = new();
    private readonly List<SdfReference> _explicitReferences = new();

    #region Construction

    /// <summary>
    /// Create an invalid UsdReferences object.
    /// </summary>
    public UsdReferences()
    {
        _prim = new UsdPrim();
    }

    /// <summary>
    /// Create a UsdReferences object for the given prim.
    /// </summary>
    public UsdReferences(UsdPrim prim)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
    }

    #endregion

    #region Validity

    /// <summary>
    /// Return true if this UsdReferences object is valid.
    /// </summary>
    public bool IsValid() => _prim.IsValid();

    /// <summary>
    /// Get the prim that owns these references.
    /// </summary>
    public UsdPrim GetPrim() => _prim;

    #endregion

    #region Add Operations

    /// <summary>
    /// Add a reference with the specified parameters.
    /// </summary>
    public bool AddReference(SdfReference reference, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid())
            return false;

        // Validate the reference
        if (!IsValidReference(reference))
            return false;

        // Check for duplicates
        if (_references.Contains(reference))
            return true; // Already exists, consider success

        // Add to the appropriate position
        InsertReference(reference, position);
        return true;
    }

    /// <summary>
    /// Add a reference to an external asset with the specified parameters.
    /// </summary>
    public bool AddReference(string assetPath, SdfPath primPath = default, SdfLayerOffset layerOffset = default, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (string.IsNullOrEmpty(assetPath))
            return false;

        var reference = new SdfReference(
            assetPath, 
            primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath,
            layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset
        );

        return AddReference(reference, position);
    }


    /// <summary>
    /// Add an internal reference to a prim in the same layer stack.
    /// </summary>
    public bool AddInternalReference(SdfPath primPath, SdfLayerOffset layerOffset = default, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (primPath.IsEmpty())
            return false;

        var reference = SdfReference.CreateInternal(
            primPath,
            layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset
        );

        return AddReference(reference, position);
    }

    #endregion

    #region Remove Operations

    /// <summary>
    /// Remove the specified reference.
    /// </summary>
    public bool RemoveReference(SdfReference reference)
    {
        if (!IsValid())
            return false;

        var removed = _references.Remove(reference);
        _explicitReferences.Remove(reference);
        return removed;
    }

    /// <summary>
    /// Remove a reference by asset path and prim path.
    /// </summary>
    public bool RemoveReference(string assetPath, SdfPath primPath = default)
    {
        var reference = new SdfReference(assetPath, primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath);
        return RemoveReference(reference);
    }

    /// <summary>
    /// Remove an internal reference by prim path.
    /// </summary>
    public bool RemoveInternalReference(SdfPath primPath)
    {
        var reference = SdfReference.CreateInternal(primPath);
        return RemoveReference(reference);
    }

    /// <summary>
    /// Clear all references.
    /// </summary>
    public bool ClearReferences()
    {
        if (!IsValid())
            return false;

        _references.Clear();
        _explicitReferences.Clear();
        return true;
    }

    #endregion

    #region Set Operations

    /// <summary>
    /// Set the references to the specified list, replacing any existing references.
    /// </summary>
    public bool SetReferences(IEnumerable<SdfReference> references)
    {
        if (!IsValid() || references == null)
            return false;

        var validReferences = references.Where(IsValidReference).ToList();
        
        _references.Clear();
        _references.AddRange(validReferences);
        
        _explicitReferences.Clear();
        _explicitReferences.AddRange(validReferences);

        return true;
    }

    #endregion

    #region Access Operations

    /// <summary>
    /// Get all references for this prim.
    /// </summary>
    public SdfReference[] GetReferences()
    {
        return _references.ToArray();
    }

    /// <summary>
    /// Get the number of references.
    /// </summary>
    public int GetNumReferences()
    {
        return _references.Count;
    }

    /// <summary>
    /// Return true if this prim has any references.
    /// </summary>
    public bool HasReferences()
    {
        return _references.Count > 0;
    }

    /// <summary>
    /// Return true if this prim has the specified reference.
    /// </summary>
    public bool HasReference(SdfReference reference)
    {
        return _references.Contains(reference);
    }

    /// <summary>
    /// Return true if this prim has a reference to the specified asset.
    /// </summary>
    public bool HasReference(string assetPath, SdfPath primPath = default)
    {
        var reference = new SdfReference(assetPath, primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath);
        return HasReference(reference);
    }

    #endregion

    #region Reference Filtering

    /// <summary>
    /// Get all external references (non-empty asset path).
    /// </summary>
    public SdfReference[] GetExternalReferences()
    {
        return _references.Where(r => !r.IsInternal()).ToArray();
    }

    /// <summary>
    /// Get all internal references (empty asset path).
    /// </summary>
    public SdfReference[] GetInternalReferences()
    {
        return _references.Where(r => r.IsInternal()).ToArray();
    }

    /// <summary>
    /// Get all references that target the default prim.
    /// </summary>
    public SdfReference[] GetDefaultPrimReferences()
    {
        return _references.Where(r => r.IsDefaultPrim()).ToArray();
    }

    /// <summary>
    /// Get all references with layer offsets.
    /// </summary>
    public SdfReference[] GetReferencesWithLayerOffsets()
    {
        return _references.Where(r => r.HasLayerOffset()).ToArray();
    }

    #endregion

    #region Composition and Authoring

    /// <summary>
    /// Return true if this prim has explicitly authored references.
    /// </summary>
    public bool HasAuthoredReferences()
    {
        return _explicitReferences.Count > 0;
    }

    /// <summary>
    /// Get the explicitly authored references (before list composition).
    /// </summary>
    public SdfReference[] GetAuthoredReferences()
    {
        return _explicitReferences.ToArray();
    }

    /// <summary>
    /// Return true if references are authored at the current edit target.
    /// </summary>
    public bool AreReferencesAuthoredAt(UsdEditTarget? editTarget = null)
    {
        // TODO: Check specific edit target for reference authoring
        return HasAuthoredReferences();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate that a reference is properly formed.
    /// </summary>
    private static bool IsValidReference(SdfReference reference)
    {
        // Internal references must have a valid prim path
        if (reference.IsInternal() && reference.GetPrimPath().IsEmpty())
            return false;

        // Layer offset must be valid
        if (!reference.GetLayerOffset().IsValid())
            return false;

        return true;
    }

    #endregion

    #region List Management

    /// <summary>
    /// Insert a reference at the specified position.
    /// </summary>
    private void InsertReference(SdfReference reference, UsdListPosition position)
    {
        switch (position)
        {
            case UsdListPosition.FrontOfPrependList:
                _references.Insert(0, reference);
                break;
            case UsdListPosition.BackOfPrependList:
                _references.Add(reference);
                break;
            case UsdListPosition.FrontOfAppendList:
                // For simplicity, treat append positions the same as prepend
                // In a full implementation, this would maintain separate prepend/append lists
                _references.Insert(0, reference);
                break;
            case UsdListPosition.BackOfAppendList:
                _references.Add(reference);
                break;
        }
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this UsdReferences object.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "UsdReferences(invalid)";

        var primPath = _prim.GetPath().GetString();
        var count = GetNumReferences();
        return $"UsdReferences({primPath}, {count} references)";
    }

    #endregion
}