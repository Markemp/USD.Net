using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdPayloads provides an interface to a prim's payloads.
/// Payloads enable optional loading of content for memory management.
/// Unlike references, payloads can be explicitly loaded/unloaded by the user.
/// </summary>
public class UsdPayloads
{
    private readonly UsdPrim _prim;
    private readonly List<SdfPayload> _payloads = new();
    private readonly List<SdfPayload> _explicitPayloads = new();

    #region Construction

    /// <summary>
    /// Create an invalid UsdPayloads object.
    /// </summary>
    public UsdPayloads()
    {
        _prim = new UsdPrim();
    }

    /// <summary>
    /// Create a UsdPayloads object for the given prim.
    /// </summary>
    public UsdPayloads(UsdPrim prim)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
    }

    #endregion

    #region Validity

    /// <summary>
    /// Return true if this UsdPayloads object is valid.
    /// </summary>
    public bool IsValid() => _prim.IsValid();

    /// <summary>
    /// Get the prim that owns these payloads.
    /// </summary>
    public UsdPrim GetPrim() => _prim;

    #endregion

    #region Add Operations

    /// <summary>
    /// Add a payload with the specified parameters.
    /// </summary>
    public bool AddPayload(SdfPayload payload, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (!IsValid())
            return false;

        // Validate the payload
        if (!IsValidPayload(payload))
            return false;

        // Check for duplicates
        if (_payloads.Contains(payload))
            return true; // Already exists, consider success

        // Add to the appropriate position
        InsertPayload(payload, position);
        return true;
    }

    /// <summary>
    /// Add a payload to an external asset with the specified parameters.
    /// </summary>
    public bool AddPayload(string assetPath, SdfPath primPath = default, SdfLayerOffset layerOffset = default, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (string.IsNullOrEmpty(assetPath))
            return false;

        var payload = new SdfPayload(
            assetPath, 
            primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath,
            layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset
        );

        return AddPayload(payload, position);
    }

    /// <summary>
    /// Add an internal payload to a prim in the same layer stack.
    /// </summary>
    public bool AddInternalPayload(SdfPath primPath, SdfLayerOffset layerOffset = default, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (primPath.IsEmpty())
            return false;

        var payload = SdfPayload.CreateInternal(
            primPath,
            layerOffset.IsDefault() ? SdfLayerOffset.Identity() : layerOffset
        );

        return AddPayload(payload, position);
    }

    #endregion

    #region Remove Operations

    /// <summary>
    /// Remove the specified payload.
    /// </summary>
    public bool RemovePayload(SdfPayload payload)
    {
        if (!IsValid())
            return false;

        var removed = _payloads.Remove(payload);
        _explicitPayloads.Remove(payload);
        return removed;
    }

    /// <summary>
    /// Remove a payload by asset path and prim path.
    /// </summary>
    public bool RemovePayload(string assetPath, SdfPath primPath = default)
    {
        var payload = new SdfPayload(assetPath, primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath);
        return RemovePayload(payload);
    }

    /// <summary>
    /// Remove an internal payload by prim path.
    /// </summary>
    public bool RemoveInternalPayload(SdfPath primPath)
    {
        var payload = SdfPayload.CreateInternal(primPath);
        return RemovePayload(payload);
    }

    /// <summary>
    /// Clear all payloads.
    /// </summary>
    public bool ClearPayloads()
    {
        if (!IsValid())
            return false;

        _payloads.Clear();
        _explicitPayloads.Clear();
        return true;
    }

    #endregion

    #region Set Operations

    /// <summary>
    /// Set the payloads to the specified list, replacing any existing payloads.
    /// </summary>
    public bool SetPayloads(IEnumerable<SdfPayload> payloads)
    {
        if (!IsValid() || payloads == null)
            return false;

        var validPayloads = payloads.Where(IsValidPayload).ToList();
        
        _payloads.Clear();
        _payloads.AddRange(validPayloads);
        
        _explicitPayloads.Clear();
        _explicitPayloads.AddRange(validPayloads);

        return true;
    }

    #endregion

    #region Access Operations

    /// <summary>
    /// Get all payloads for this prim.
    /// </summary>
    public SdfPayload[] GetPayloads()
    {
        return _payloads.ToArray();
    }

    /// <summary>
    /// Get the number of payloads.
    /// </summary>
    public int GetNumPayloads()
    {
        return _payloads.Count;
    }

    /// <summary>
    /// Return true if this prim has any payloads.
    /// </summary>
    public bool HasPayloads()
    {
        return _payloads.Count > 0;
    }

    /// <summary>
    /// Return true if this prim has the specified payload.
    /// </summary>
    public bool HasPayload(SdfPayload payload)
    {
        return _payloads.Contains(payload);
    }

    /// <summary>
    /// Return true if this prim has a payload to the specified asset.
    /// </summary>
    public bool HasPayload(string assetPath, SdfPath primPath = default)
    {
        var payload = new SdfPayload(assetPath, primPath.IsEmpty() ? SdfPath.EmptyPath() : primPath);
        return HasPayload(payload);
    }

    #endregion

    #region Payload Filtering

    /// <summary>
    /// Get all external payloads (non-empty asset path).
    /// </summary>
    public SdfPayload[] GetExternalPayloads()
    {
        return _payloads.Where(p => !p.IsInternal()).ToArray();
    }

    /// <summary>
    /// Get all internal payloads (empty asset path).
    /// </summary>
    public SdfPayload[] GetInternalPayloads()
    {
        return _payloads.Where(p => p.IsInternal()).ToArray();
    }

    /// <summary>
    /// Get all payloads that target the default prim.
    /// </summary>
    public SdfPayload[] GetDefaultPrimPayloads()
    {
        return _payloads.Where(p => p.IsDefaultPrim()).ToArray();
    }

    /// <summary>
    /// Get all payloads with layer offsets.
    /// </summary>
    public SdfPayload[] GetPayloadsWithLayerOffsets()
    {
        return _payloads.Where(p => p.HasLayerOffset()).ToArray();
    }

    #endregion

    #region Composition and Authoring

    /// <summary>
    /// Return true if this prim has explicitly authored payloads.
    /// </summary>
    public bool HasAuthoredPayloads()
    {
        return _explicitPayloads.Count > 0;
    }

    /// <summary>
    /// Get the explicitly authored payloads (before list composition).
    /// </summary>
    public SdfPayload[] GetAuthoredPayloads()
    {
        return _explicitPayloads.ToArray();
    }

    /// <summary>
    /// Return true if payloads are authored at the current edit target.
    /// </summary>
    public bool ArePayloadsAuthoredAt(UsdEditTarget? editTarget = null)
    {
        // TODO: Check specific edit target for payload authoring
        return HasAuthoredPayloads();
    }

    #endregion

    #region Loading State

    /// <summary>
    /// Return true if this prim's payloads are loaded.
    /// </summary>
    public bool IsLoaded()
    {
        if (!IsValid())
            return false;

        // TODO: Implement proper load state checking with stage
        // For now, assume payloads are loaded if they exist
        return HasPayloads();
    }

    /// <summary>
    /// Return true if this prim is loadable (has payloads that can be loaded).
    /// </summary>
    public bool IsLoadable()
    {
        return HasPayloads();
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validate that a payload is properly formed.
    /// </summary>
    private static bool IsValidPayload(SdfPayload payload)
    {
        // Internal payloads must have a valid prim path
        if (payload.IsInternal() && payload.GetPrimPath().IsEmpty())
            return false;

        // Layer offset must be valid
        if (!payload.GetLayerOffset().IsValid())
            return false;

        return true;
    }

    #endregion

    #region List Management

    /// <summary>
    /// Insert a payload at the specified position.
    /// </summary>
    private void InsertPayload(SdfPayload payload, UsdListPosition position)
    {
        switch (position)
        {
            case UsdListPosition.FrontOfPrependList:
                _payloads.Insert(0, payload);
                break;
            case UsdListPosition.BackOfPrependList:
                _payloads.Add(payload);
                break;
            case UsdListPosition.FrontOfAppendList:
                // For simplicity, treat append positions the same as prepend
                // In a full implementation, this would maintain separate prepend/append lists
                _payloads.Insert(0, payload);
                break;
            case UsdListPosition.BackOfAppendList:
                _payloads.Add(payload);
                break;
        }
    }

    #endregion

    #region String Representation

    /// <summary>
    /// Return a string representation of this UsdPayloads object.
    /// </summary>
    public override string ToString()
    {
        if (!IsValid())
            return "UsdPayloads(invalid)";

        var primPath = _prim.GetPath().GetString();
        var count = GetNumPayloads();
        var loadedState = IsLoaded() ? "loaded" : "unloaded";
        return $"UsdPayloads({primPath}, {count} payloads, {loadedState})";
    }

    #endregion
}