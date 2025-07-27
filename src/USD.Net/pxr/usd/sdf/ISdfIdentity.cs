namespace Pxr.Usd.Sdf;

/// <summary>
/// Identifies the logical object behind an SdfSpec.
/// 
/// This is simply the layer the spec belongs to and the path to the spec.
/// </summary>
public interface ISdfIdentity : IEquatable<ISdfIdentity>, IComparable<ISdfIdentity>
{
    /// <summary>
    /// Returns the layer that this identity refers to.
    /// </summary>
    ISdfLayer? GetLayer();

    /// <summary>
    /// Returns the path that this identity refers to.
    /// </summary>
    ISdfPath GetPath();
}