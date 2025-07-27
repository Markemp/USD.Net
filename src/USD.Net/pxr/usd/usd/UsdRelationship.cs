using Pxr.Usd.Sdf;

namespace Pxr.Usd;

public class UsdRelationship : UsdProperty, IUsdRelationship
{
    private readonly List<ISdfPath> _targets = new();
    private readonly List<ISdfPath> _explicitTargets = new();

    #region Construction

    public UsdRelationship() : base()
    {
    }

    public UsdRelationship(UsdStage stage, ISdfPath path) : base(stage, path)
    {
    }

    #endregion

    #region Target Management

    public virtual bool AddTarget(ISdfPath target, UsdListPosition position = UsdListPosition.BackOfPrependList)
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

    public virtual bool RemoveTarget(ISdfPath target)
    {
        if (target.IsEmpty())
            return false;

        return _targets.Remove(target);
    }

    public virtual bool SetTargets(IEnumerable<ISdfPath> targets)
    {
        if (targets is null)
            return false;

        var validTargets = targets.Where(IsValidTarget).ToList();
        
        _targets.Clear();
        _targets.AddRange(validTargets);
        _explicitTargets.Clear();
        _explicitTargets.AddRange(validTargets);

        return true;
    }

    public virtual bool ClearTargets(bool removeSpec = false)
    {
        _targets.Clear();
        _explicitTargets.Clear();
        // TODO: Implement removeSpec behavior - remove the relationship spec entirely if true
        return true;
    }

    public virtual bool GetTargets(out IReadOnlyList<ISdfPath> targets)
    {
        targets = _targets.AsReadOnly();
        return _targets.Count > 0;
    }
    
    public virtual IReadOnlyList<ISdfPath> GetTargets() => _targets.AsReadOnly();

    public virtual bool HasTargets() => _targets.Count > 0;
    
    public virtual bool HasAuthoredTargets()
    {
        // TODO: Implement proper authored opinion checking
        // For now, check if we have any targets or explicit targets
        return _targets.Count > 0 || _explicitTargets.Count > 0;
    }

    #endregion

    #region Target Validation

    public virtual bool IsValidTarget(ISdfPath target)
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

    public virtual bool HasValidTargets() => _targets.All(IsValidTarget);

    #endregion

    #region Target Resolution

    public virtual bool GetForwardedTargets(out IReadOnlyList<ISdfPath> targets)
    {
        var forwardedTargets = new List<ISdfPath>();
        var visited = new HashSet<ISdfPath>();

        foreach (var target in _targets)
        {
            ResolveForwardedTarget(target, forwardedTargets, visited);
        }

        targets = forwardedTargets.AsReadOnly();
        return forwardedTargets.Count > 0;
    }
    
    public virtual IReadOnlyList<ISdfPath> GetForwardedTargets()
    {
        GetForwardedTargets(out var targets);
        return targets;
    }

    /// <summary>
    /// Recursively resolve a forwarded target.
    /// </summary>
    private void ResolveForwardedTarget(ISdfPath target, List<ISdfPath> results, HashSet<ISdfPath> visited)
    {
        // Prevent infinite loops
        if (visited.Contains(target))
            return;

        visited.Add(target);

        var stage = GetStage();
        if (stage is null)
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

    public virtual bool HasTarget(ISdfPath target) => _targets.Contains(target);

    public virtual int GetNumTargets() => _targets.Count;

    public virtual IReadOnlyList<ISdfPath> GetTargets(Func<ISdfPath, bool> predicate)
        => _targets.Where(predicate).ToList().AsReadOnly();

    /// <summary>
    /// Get all prim targets (excluding attribute/relationship targets).
    /// </summary>
    public virtual ISdfPath[] GetPrimTargets()
        => _targets.Where(path => path.IsPrimPath()).ToArray();

    /// <summary>
    /// Get all property targets (attribute and relationship targets).
    /// </summary>
    public virtual ISdfPath[] GetPropertyTargets()
        => _targets.Where(path => path.IsPropertyPath()).ToArray();

    #endregion

    #region List Editing

    /// <summary>
    /// Replace a target in the relationship.
    /// </summary>
    public virtual bool ReplaceTarget(ISdfPath oldTarget, ISdfPath newTarget)
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
    public virtual bool InsertTarget(int index, ISdfPath target)
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
    /// Get the explicitly authored targets (before list composition).
    /// </summary>
    public virtual ISdfPath[] GetAuthoredTargets()
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
        => base.IsDefined() && HasValidTargets();

    #endregion
}