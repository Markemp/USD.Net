using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdRelationship creates dependencies between scenegraph objects by allowing a prim to target 
/// other prims, attributes, or relationships. Relationships are always uniform (do not vary over time).
/// </summary>
public class UsdRelationship : UsdProperty
{
    private readonly List<SdfPath> _targets = new();
    private readonly List<SdfPath> _explicitTargets = new();

    #region Construction

    /// <summary>
    /// Create an invalid relationship.
    /// </summary>
    public UsdRelationship() : base()
    {
    }

    /// <summary>
    /// Create a relationship with stage and path.
    /// </summary>
    public UsdRelationship(UsdStage stage, SdfPath path) : base(stage, path)
    {
    }

    #endregion

    #region Target Management

    /// <summary>
    /// Add a target path to this relationship.
    /// </summary>
    public virtual bool AddTarget(SdfPath target, UsdListPosition position = UsdListPosition.BackOfPrependList)
    {
        if (target.IsEmpty())
            return false;

        // Validate target path
        if (!IsValidTarget(target))
            return false;

        // Check if target already exists
        if (_targets.Contains(target))
            return true; // Already exists, consider it success

        // Add target based on position
        switch (position)
        {
            case UsdListPosition.FrontOfPrependList:
                _targets.Insert(0, target);
                break;
            case UsdListPosition.BackOfPrependList:
                _targets.Add(target);
                break;
            case UsdListPosition.FrontOfAppendList:
                // For simplicity, treat append list the same as prepend list
                _targets.Insert(0, target);
                break;
            case UsdListPosition.BackOfAppendList:
                _targets.Add(target);
                break;
        }

        return true;
    }

    /// <summary>
    /// Remove a target from this relationship.
    /// </summary>
    public virtual bool RemoveTarget(SdfPath target)
    {
        if (target.IsEmpty())
            return false;

        return _targets.Remove(target);
    }

    /// <summary>
    /// Explicitly set all targets for this relationship, replacing any existing targets.
    /// </summary>
    public virtual bool SetTargets(IEnumerable<SdfPath> targets)
    {
        if (targets == null)
            return false;

        var validTargets = targets.Where(IsValidTarget).ToList();
        
        _targets.Clear();
        _targets.AddRange(validTargets);
        _explicitTargets.Clear();
        _explicitTargets.AddRange(validTargets);

        return true;
    }

    /// <summary>
    /// Clear all target opinions from this relationship.
    /// </summary>
    public virtual bool ClearTargets()
    {
        _targets.Clear();
        _explicitTargets.Clear();
        return true;
    }

    /// <summary>
    /// Get the composed targets for this relationship.
    /// </summary>
    public virtual SdfPath[] GetTargets()
    {
        return _targets.ToArray();
    }

    /// <summary>
    /// Return true if this relationship has any targets.
    /// </summary>
    public virtual bool HasTargets()
    {
        return _targets.Count > 0;
    }

    #endregion

    #region Target Validation

    /// <summary>
    /// Return true if the given path is a valid target for this relationship.
    /// </summary>
    public virtual bool IsValidTarget(SdfPath target)
    {
        if (target.IsEmpty())
            return false;

        // Target must be absolute
        if (!target.IsAbsolutePath())
            return false;

        // TODO: Add additional validation rules:
        // - Cannot target objects within prototypes
        // - Check if target exists in composed scene namespace
        // - Validate against instancing restrictions

        return true;
    }

    /// <summary>
    /// Return true if all current targets are valid.
    /// </summary>
    public virtual bool HasValidTargets()
    {
        return _targets.All(IsValidTarget);
    }

    #endregion

    #region Target Resolution

    /// <summary>
    /// Resolve ultimate targets by following relationship forwarding chains.
    /// </summary>
    public virtual SdfPath[] GetForwardedTargets()
    {
        var forwardedTargets = new List<SdfPath>();
        var visited = new HashSet<SdfPath>();

        foreach (var target in _targets)
        {
            ResolveForwardedTarget(target, forwardedTargets, visited);
        }

        return forwardedTargets.ToArray();
    }

    /// <summary>
    /// Recursively resolve a forwarded target.
    /// </summary>
    private void ResolveForwardedTarget(SdfPath target, List<SdfPath> results, HashSet<SdfPath> visited)
    {
        // Prevent infinite loops
        if (visited.Contains(target))
            return;

        visited.Add(target);

        var stage = GetStage();
        if (stage == null)
        {
            results.Add(target);
            return;
        }

        // If target is a relationship property, follow its targets
        if (target.IsPropertyPath())
        {
            var primPath = target.GetParentPath();
            var prim = stage.GetPrimAtPath(primPath);
            
            if (prim.IsValid())
            {
                var propertyName = target.GetName();
                
                // Check if this property is actually a relationship
                if (prim.HasRelationship(propertyName))
                {
                    var relationship = prim.GetRelationship(propertyName);
                    
                    if (relationship.IsValid() && relationship.HasTargets())
                    {
                        // Follow this relationship's targets
                        foreach (var forwardedTarget in relationship.GetTargets())
                        {
                            ResolveForwardedTarget(forwardedTarget, results, visited);
                        }
                        return;
                    }
                }
            }
        }

        // Target is final, add to results
        results.Add(target);
    }

    #endregion

    #region Target Queries

    /// <summary>
    /// Return true if this relationship targets the given path.
    /// </summary>
    public virtual bool HasTarget(SdfPath target)
    {
        return _targets.Contains(target);
    }

    /// <summary>
    /// Get the number of targets for this relationship.
    /// </summary>
    public virtual int GetNumTargets()
    {
        return _targets.Count;
    }

    /// <summary>
    /// Get targets filtered by a predicate.
    /// </summary>
    public virtual SdfPath[] GetTargets(Func<SdfPath, bool> predicate)
    {
        return _targets.Where(predicate).ToArray();
    }

    /// <summary>
    /// Get all prim targets (excluding attribute/relationship targets).
    /// </summary>
    public virtual SdfPath[] GetPrimTargets()
    {
        return _targets.Where(path => path.IsPrimPath()).ToArray();
    }

    /// <summary>
    /// Get all property targets (attribute and relationship targets).
    /// </summary>
    public virtual SdfPath[] GetPropertyTargets()
    {
        return _targets.Where(path => path.IsPropertyPath()).ToArray();
    }

    #endregion

    #region List Editing

    /// <summary>
    /// Replace a target in the relationship.
    /// </summary>
    public virtual bool ReplaceTarget(SdfPath oldTarget, SdfPath newTarget)
    {
        if (oldTarget.IsEmpty() || newTarget.IsEmpty())
            return false;

        if (!IsValidTarget(newTarget))
            return false;

        var index = _targets.IndexOf(oldTarget);
        if (index < 0)
            return false;

        _targets[index] = newTarget;
        return true;
    }

    /// <summary>
    /// Insert a target at a specific index.
    /// </summary>
    public virtual bool InsertTarget(int index, SdfPath target)
    {
        if (target.IsEmpty() || !IsValidTarget(target))
            return false;

        if (index < 0 || index > _targets.Count)
            return false;

        _targets.Insert(index, target);
        return true;
    }

    /// <summary>
    /// Remove a target at a specific index.
    /// </summary>
    public virtual bool RemoveTargetAt(int index)
    {
        if (index < 0 || index >= _targets.Count)
            return false;

        _targets.RemoveAt(index);
        return true;
    }

    #endregion

    #region Composition and Authoring

    /// <summary>
    /// Return true if this relationship has explicitly authored targets.
    /// </summary>
    public virtual bool HasAuthoredTargets()
    {
        return _explicitTargets.Count > 0;
    }

    /// <summary>
    /// Get the explicitly authored targets (before list composition).
    /// </summary>
    public virtual SdfPath[] GetAuthoredTargets()
    {
        return _explicitTargets.ToArray();
    }

    /// <summary>
    /// Return true if targets are authored at the current edit target.
    /// </summary>
    public virtual bool AreTargetsAuthoredAt(UsdEditTarget? editTarget = null)
    {
        // TODO: Check specific edit target for target authoring
        return HasAuthoredTargets();
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Get a string representation of all targets.
    /// </summary>
    public virtual string GetTargetsAsString()
    {
        if (_targets.Count == 0)
            return "[]";

        var targetStrings = _targets.Select(t => t.GetString());
        return $"[{string.Join(", ", targetStrings)}]";
    }

    /// <summary>
    /// Return a string representation of this relationship.
    /// </summary>
    public override string ToString()
    {
        var path = GetPath().GetString();
        var targets = GetTargetsAsString();
        return $"UsdRelationship({path} -> {targets})";
    }

    #endregion

    #region Validation Overrides

    /// <summary>
    /// Return true if this relationship is defined and has valid targets.
    /// </summary>
    public override bool IsDefined()
    {
        return base.IsDefined() && HasValidTargets();
    }

    #endregion
}