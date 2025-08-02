using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Interface for UsdRelationship - creates dependencies between scenegraph objects by allowing 
/// a prim to target other prims, attributes, or relationships.
/// </summary>
/// <remarks>
/// USD API compatible with OpenUSD 23.11+
/// See OpenUSD documentation: https://openusd.org/release/api/class_usd_relationship.html
/// 
/// UsdRelationship creates dependencies between scenegraph objects by allowing a prim to target 
/// other prims, attributes, or relationships. Relationships are always uniform (do not vary over time).
/// 
/// Like UsdAttribute, UsdRelationship is a data proxy; this means that it can target 
/// another UsdRelationship or UsdAttribute on the same UsdPrim, or on a completely different 
/// UsdPrim, such as /ModelRoot/Lights/KeyLight.
/// </remarks>
public interface IUsdRelationship : IUsdProperty
{
    #region Editing Relationships at Current EditTarget

    /// <summary>
    /// Adds target to the list of targets, in the position specified by position.
    /// </summary>
    /// <param name="target">The target path to add</param>
    /// <param name="position">The position where to add the target</param>
    /// <returns>True if the target was successfully added, false otherwise</returns>
    /// <remarks>
    /// Passing paths to prototype prims or any other objects in prototypes
    /// will cause an error to be issued. It is not valid to author targets to
    /// these objects.
    /// 
    /// What data this actually authors depends on what data is currently
    /// authored in the authoring layer, with respect to list-editing semantics.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool AddTarget(ISdfPath target, UsdListPosition position = UsdListPosition.BackOfPrependList);

    /// <summary>
    /// Removes target from the list of targets.
    /// </summary>
    /// <param name="target">The target path to remove</param>
    /// <returns>True if the target was successfully removed, false otherwise</returns>
    /// <remarks>
    /// Passing paths to prototype prims or any other objects in prototypes
    /// will cause an error to be issued. It is not valid to author targets to
    /// these objects.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool RemoveTarget(ISdfPath target);

    /// <summary>
    /// Make the authoring layer's opinion of the targets list explicit,
    /// and set exactly to targets.
    /// </summary>
    /// <param name="targets">The exact list of targets to set</param>
    /// <returns>True if the targets were successfully set, false otherwise</returns>
    /// <remarks>
    /// Passing paths to prototype prims or any other objects in prototypes
    /// will cause an error to be issued. It is not valid to author targets to
    /// these objects.
    /// 
    /// If any target in targets is invalid, no targets will be authored
    /// and this function will return false.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool SetTargets(IEnumerable<ISdfPath> targets);

    /// <summary>
    /// Remove all opinions about the target list from the current edit target.
    /// </summary>
    /// <param name="removeSpec">If true, remove the spec to preserve meta-data 
    /// we may have intentionally authored on the relationship</param>
    /// <returns>True if the targets were successfully cleared, false otherwise</returns>
    /// <remarks>
    /// Only remove the spec if removeSpec is true (leave the spec to
    /// preserve meta-data we may have intentionally authored on the relationship).
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool ClearTargets(bool removeSpec = false);

    #endregion

    #region Querying Relationship Targets

    /// <summary>
    /// Compose this relationship's targets and fill targets with the result.
    /// </summary>
    /// <param name="targets">Output parameter to receive the composed targets</param>
    /// <returns>True if any target path opinions have been authored and no composition 
    /// errors were encountered, false otherwise</returns>
    /// <remarks>
    /// Returns true if any target path opinions have been authored and no
    /// composition errors were encountered, returns false otherwise. 
    /// Note that authored opinions may include opinions that clear the targets 
    /// and a return value of true does not necessarily indicate that targets 
    /// will contain any target paths.
    /// 
    /// See "Scenegraph Instancing: Targets and Connections" for details on 
    /// behavior when targets point to objects beneath instance prims.
    /// 
    /// The result is not cached, so will be recomputed on every query.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool GetTargets(out IReadOnlyList<ISdfPath> targets);

    /// <summary>
    /// Compose this relationship's ultimate targets, taking into account
    /// "relationship forwarding", and fill targets with the result.
    /// </summary>
    /// <param name="targets">Output parameter to receive the forwarded targets</param>
    /// <returns>True if any of the visited relationships that are not "purely forwarding" 
    /// has an authored opinion for its target paths and no composition errors were encountered</returns>
    /// <remarks>
    /// This method never inserts relationship paths in targets.
    /// 
    /// Returns true if any of the visited relationships that are not 
    /// "purely forwarding" has an authored opinion for its target paths and
    /// no composition errors were encountered while computing any targets. 
    /// Purely forwarding, in this context, means the relationship has at least 
    /// one target but all of its targets are paths to other relationships.
    /// 
    /// When composition errors occur, this function continues to collect 
    /// successfully composed targets, but returns false to indicate to the 
    /// caller that errors occurred.
    /// 
    /// See "Relationship Forwarding" for details on the semantics.
    /// 
    /// The result is not cached, so will be recomputed on every query.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool GetForwardedTargets(out IReadOnlyList<ISdfPath> targets);

    /// <summary>
    /// Returns true if any target path opinions have been authored.
    /// </summary>
    /// <returns>True if any target path opinions have been authored, false otherwise</returns>
    /// <remarks>
    /// Note that this may include opinions that clear targets and may not 
    /// indicate that target paths will exist for this relationship.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_relationship.html#a1234567890abcdef
    /// </remarks>
    bool HasAuthoredTargets();

    #endregion

    #region USD.Net Additional Convenience Methods

    /// <summary>
    /// Returns true if this relationship has any targets.
    /// </summary>
    /// <returns>True if there are any targets, false otherwise</returns>
    bool HasTargets();

    /// <summary>
    /// Returns true if the specified target path is valid for this relationship.
    /// </summary>
    /// <param name="target">The target path to validate</param>
    /// <returns>True if the target is valid, false otherwise</returns>
    bool IsValidTarget(ISdfPath target);

    /// <summary>
    /// Returns true if all current targets are valid.
    /// </summary>
    /// <returns>True if all targets are valid, false otherwise</returns>
    bool HasValidTargets();

    /// <summary>
    /// Returns true if this relationship has the specified target.
    /// </summary>
    /// <param name="target">The target path to check for</param>
    /// <returns>True if the target exists, false otherwise</returns>
    bool HasTarget(ISdfPath target);

    /// <summary>
    /// Returns the number of targets for this relationship.
    /// </summary>
    /// <returns>The number of targets</returns>
    int GetNumTargets();

    /// <summary>
    /// Returns targets that match the specified predicate.
    /// </summary>
    /// <param name="predicate">Function to filter targets</param>
    /// <returns>Array of targets matching the predicate</returns>
    IReadOnlyList<ISdfPath> GetTargets(Func<ISdfPath, bool> predicate);

    #endregion
}