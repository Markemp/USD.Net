namespace Pxr.Usd;

/// <summary>
/// Controls UsdStage.Load() and UsdPrim.Load() behavior regarding whether or
/// not descendant prims are loaded.
/// </summary>
/// <remarks>
/// This enum determines the loading behavior when loading prims from a USD stage.
/// When loading payloads, you can choose to load just the prim itself or the
/// prim plus all its descendants.
/// </remarks>
public enum UsdLoadPolicy
{
    /// <summary>
    /// Load a prim plus all its descendants.
    /// </summary>
    /// <remarks>
    /// This is the default behavior. When a prim is loaded with this policy,
    /// all of its child prims and their descendants will also be loaded
    /// recursively.
    /// </remarks>
    UsdLoadWithDescendants,
    
    /// <summary>
    /// Load a prim by itself with no descendants.
    /// </summary>
    /// <remarks>
    /// With this policy, only the specified prim will be loaded. None of its
    /// child prims or descendants will be loaded. This can be useful for
    /// performance when you only need specific prims from a large hierarchy.
    /// </remarks>
    UsdLoadWithoutDescendants
}