using Pxr.Base.Tf;
using Pxr.Base.Vt;
namespace Pxr.Usd.Sdf;

/// <summary>
/// Interface defining various attributes for a field.
/// </summary>
/// <remarks>
/// This interface corresponds to the SdfSchemaBase::FieldDefinition class in OpenUSD.
/// </remarks>
public interface ISdfFieldDefinition
{
    /// <summary>
    /// Return the name of this field.
    /// </summary>
    TfToken GetName();
    
    /// <summary>
    /// Return true if this field is read-only.
    /// </summary>
    bool IsReadOnly();
    
    /// <summary>
    /// Return true if this field holds children.
    /// </summary>
    bool HoldsChildren();
    
    /// <summary>
    /// Return the fallback value for this field.
    /// </summary>
    VtValue GetFallbackValue();
    
    /// <summary>
    /// Return true if this field is provided by a plugin.
    /// </summary>
    bool IsPlugin();
}
