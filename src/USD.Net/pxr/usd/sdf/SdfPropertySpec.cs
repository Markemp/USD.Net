using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

/// <summary>
/// SdfPropertySpec represents a property specification in an SDF layer.
/// It stores the scene description for properties including attributes and relationships.
/// </summary>
public abstract class SdfPropertySpec : SdfSpec
{
    #region Fields

    /// <summary>
    /// The name of this property.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a custom property.
    /// </summary>
    public bool Custom { get; set; } = true;

    /// <summary>
    /// The variability of this property.
    /// </summary>
    public SdfVariability Variability { get; set; } = SdfVariability.Varying;

    #endregion

    #region Construction

    /// <summary>
    /// Protected constructor for derived classes.
    /// </summary>
    protected SdfPropertySpec() : base()
    {
    }

    /// <summary>
    /// Protected constructor for derived classes with layer and path.
    /// </summary>
    protected SdfPropertySpec(ISdfLayer layer, ISdfPath path) : base(layer, path)
    {
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Returns the type of this spec.
    /// </summary>
    public new abstract SdfSpecType GetSpecType();

    #endregion

    #region Property Management

    /// <summary>
    /// Add this property to a prim spec.
    /// </summary>
    internal void AddToPrim(SdfPrimSpec prim)
    {
        if (prim == null || string.IsNullOrEmpty(Name))
            return;

        // This would be called when the property is added to a prim
        // The prim manages the actual storage
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Returns a string representation of this property spec.
    /// </summary>
    public override string ToString()
    {
        var type = GetSpecType();
        return $"Sdf{type}(name={Name}, custom={Custom}, variability={Variability})";
    }

    #endregion
}