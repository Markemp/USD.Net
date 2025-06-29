using System;

namespace Pxr.Usd;

/// <summary>
/// UsdEditContext is an RAII class that temporarily changes a stage's edit target.
/// When the context is disposed, the stage's edit target is restored to its original value.
/// This provides a convenient way to temporarily direct edits to a different layer.
/// </summary>
public readonly struct UsdEditContext : IDisposable
{
    private readonly UsdStage? _stage;
    private readonly UsdEditTarget _originalEditTarget;

    /// <summary>
    /// Construct a new edit context that sets the stage's edit target to the specified target.
    /// The original edit target will be restored when this context is disposed.
    /// </summary>
    public UsdEditContext(UsdStage stage, UsdEditTarget editTarget)
    {
        _stage = stage ?? throw new ArgumentNullException(nameof(stage));
        _originalEditTarget = stage.GetEditTarget();
        stage.SetEditTarget(editTarget);
    }

    /// <summary>
    /// Restore the stage's edit target to its original value.
    /// </summary>
    public void Dispose()
    {
        _stage?.SetEditTarget(_originalEditTarget);
    }

    /// <summary>
    /// Get the stage that this context is managing.
    /// </summary>
    public UsdStage? GetStage() => _stage;

    /// <summary>
    /// Get the original edit target that will be restored on disposal.
    /// </summary>
    public UsdEditTarget GetOriginalEditTarget() => _originalEditTarget;
}