using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdInherits provides an interface to a prim's inherit paths.
/// Inherits enable class-based composition within the same layer stack,
/// allowing prims to inherit scene description from class prims.
/// </summary>
public class UsdInherits
{
    private readonly UsdPrim _prim;
    private readonly List<SdfPath> _inherits = new();
    private readonly List<SdfPath> _explicitInherits = new();

    #region Construction

    /// <summary>
    /// Create an invalid UsdInherits object.
    /// </summary>
    public UsdInherits()
    {
        _prim = new UsdPrim();
    }

    /// <summary>
    /// Create a UsdInherits object for the given prim.
    /// </summary>
    public UsdInherits(UsdPrim prim)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
    }

    #endregion

    #region Validity

    /// <summary>
    /// Return true if this UsdInherits object is valid.
    /// </summary>
    public bool IsValid() => _prim.IsValid();

    /// <summary>
    /// Get the prim that owns these inherits.
    /// </summary>
    public UsdPrim GetPrim() => _prim;

    #endregion

    #region Add Operations

    /// <summary>
    /// Add an inherit path to this prim's inheritance list.
    /// </summary>
    public bool AddInherit(SdfPath primPath, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid())
            return false;

        // Validate the inherit path
        if (!IsValidInheritPath(primPath))
            return false;

        // Check for duplicates
        if (_inherits.Contains(primPath))
            return true; // Already exists, consider success

        // Add to the appropriate position
        InsertInherit(primPath, position);
        
        // Also add to explicit inherits to track authoring
        if (!_explicitInherits.Contains(primPath))
            _explicitInherits.Add(primPath);
            
        return true;
    }

    #endregion

    #region Remove Operations

    /// <summary>
    /// Remove the specified inherit path.
    /// </summary>
    public bool RemoveInherit(SdfPath primPath)
    {
        if (!IsValid())
            return false;

        var removed = _inherits.Remove(primPath);
        _explicitInherits.Remove(primPath);
        return removed;
    }

    /// <summary>
    /// Clear all inherit paths.
    /// </summary>
    public bool ClearInherits()
    {
        if (!IsValid())
            return false;

        _inherits.Clear();
        _explicitInherits.Clear();
        return true;
    }

    #endregion

    #region Set Operations

    /// <summary>
    /// Set the inherit paths to the specified list, replacing any existing inherits.
    /// </summary>
    public bool SetInherits(IEnumerable<SdfPath> inheritPaths)
    {
        if (!IsValid() || inheritPaths == null)
            return false;

        var validInherits = inheritPaths.Where(IsValidInheritPath).ToList();
        
        _inherits.Clear();
        _inherits.AddRange(validInherits);
        
        _explicitInherits.Clear();
        _explicitInherits.AddRange(validInherits);

        return true;
    }

    #endregion

    #region Access Operations

    /// <summary>
    /// Get all inherit paths for this prim in strong-to-weak order.
    /// </summary>
    public SdfPath[] GetAllDirectInherits()
    {
        return _inherits.ToArray();
    }

    /// <summary>
    /// Get the number of inherit paths.
    /// </summary>
    public int GetNumInherits()
    {
        return _inherits.Count;
    }

    /// <summary>
    /// Return true if this prim has any inherit paths.
    /// </summary>
    public bool HasInherits()
    {
        return _inherits.Count > 0;
    }

    /// <summary>
    /// Return true if this prim has the specified inherit path.
    /// </summary>
    public bool HasInherit(SdfPath primPath)
    {
        return _inherits.Contains(primPath);
    }

    #endregion

    #region Composition and Authoring

    /// <summary>
    /// Return true if this prim has explicitly authored inherit paths.
    /// </summary>
    public bool HasAuthoredInherits()
    {
        return _explicitInherits.Count > 0;
    }

    /// <summary>
    /// Get the explicitly authored inherit paths (before list composition).
    /// </summary>
    public SdfPath[] GetAuthoredInherits()
    {
        return _explicitInherits.ToArray();
    }

    /// <summary>
    /// Return true if inherit paths are authored at the current edit target.
    /// </summary>
    public bool AreInheritsAuthoredAt(UsdEditTarget? editTarget = null)
    {
        // TODO: Check specific edit target for inherit authoring
        return HasAuthoredInherits();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate that an inherit path is properly formed.
    /// </summary>
    private static bool IsValidInheritPath(SdfPath primPath)
    {
        // Inherit paths must be valid prim paths
        if (primPath.IsEmpty() || !primPath.IsPrimPath())
            return false;

        // TODO: Additional validation based on USD requirements
        return true;
    }

    #endregion

    #region List Management

    /// <summary>
    /// Insert an inherit path at the specified position.
    /// </summary>
    private void InsertInherit(SdfPath primPath, UsdListPosition position)
    {
        switch (position)
        {
            case UsdListPosition.FrontOfPrependList:
                _inherits.Insert(0, primPath);
                break;
            case UsdListPosition.BackOfPrependList:
                _inherits.Add(primPath);
                break;
            case UsdListPosition.FrontOfAppendList:
                // For simplicity, treat append positions the same as prepend
                // In a full implementation, this would maintain separate prepend/append lists
                _inherits.Insert(0, primPath);
                break;
            case UsdListPosition.BackOfAppendList:
                _inherits.Add(primPath);
                break;
        }
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this UsdInherits object.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "UsdInherits(invalid)";

        var primPath = _prim.GetPath().GetString();
        var count = GetNumInherits();
        return $"UsdInherits({primPath}, {count} inherits)";
    }

    #endregion
}