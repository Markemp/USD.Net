namespace Pxr.Usd.Sdf;

/// <summary>
/// Handle (smart pointer) to an SdfPrimSpec.
/// </summary>
/// <remarks>
/// In C++ USD, this is a smart pointer type. In C#, we implement it as a simple
/// wrapper that holds a reference to an SdfPrimSpec. This provides a level of
/// indirection similar to the C++ implementation.
/// </remarks>
public class SdfPrimSpecHandle
{
    private readonly SdfPrimSpec? _spec;

    /// <summary>
    /// Create an invalid/null handle.
    /// </summary>
    public SdfPrimSpecHandle()
    {
        _spec = null;
    }

    /// <summary>
    /// Create a handle to the given prim spec.
    /// </summary>
    /// <param name="spec">The prim spec to reference</param>
    public SdfPrimSpecHandle(SdfPrimSpec spec)
    {
        _spec = spec;
    }

    /// <summary>
    /// Get the referenced prim spec.
    /// </summary>
    public SdfPrimSpec? Spec => _spec;

    /// <summary>
    /// Check if this handle points to a valid prim spec.
    /// </summary>
    public bool IsValid => _spec != null;

    /// <summary>
    /// Implicit conversion from SdfPrimSpec to handle.
    /// </summary>
    public static implicit operator SdfPrimSpecHandle(SdfPrimSpec spec) => new(spec);

    /// <summary>
    /// Explicit conversion from handle to SdfPrimSpec.
    /// </summary>
    public static explicit operator SdfPrimSpec?(SdfPrimSpecHandle handle) => handle._spec;
}