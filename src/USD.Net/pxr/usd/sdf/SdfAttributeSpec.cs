namespace Pxr.Usd.Sdf;

/// <summary>
/// A property that contains a typed value.
/// </summary>
/// <remarks>
/// SdfAttributeSpec objects have a required field, default (the property's default value), 
/// and an optional field, timeSamples (time-varying values). They are a subclass 
/// of SdfPropertySpec objects.
/// </remarks>
public class SdfAttributeSpec : SdfPropertySpec
{
    #region Construction

    /// <summary>
    /// Protected constructor for derived classes.
    /// </summary>
    protected SdfAttributeSpec() : base()
    {
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Returns the type of this spec.
    /// </summary>
    public override SdfSpecType GetSpecType() => SdfSpecType.Attribute;

    /// <summary>
    /// Returns a string representation of this attribute spec.
    /// </summary>
    public override string ToString()
    {
        return $"SdfAttributeSpec(name={Name}, custom={Custom}, variability={Variability})";
    }

    #endregion
}