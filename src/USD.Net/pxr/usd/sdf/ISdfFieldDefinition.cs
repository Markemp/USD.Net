using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

// AIDEV-NOTE: Interface based on OpenUSD SdfSchemaBase - DO NOT MODIFY WITHOUT PERMISSION
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
    /// Return the metadata information for this field.
    /// </summary>
    /// <returns>Collection of metadata key-value pairs</returns>
    IReadOnlyList<KeyValuePair<TfToken, object>> GetInfo();
    
    /// <summary>
    /// Return true if this field is provided by a plugin.
    /// </summary>
    bool IsPlugin();

    /// <summary>
    /// Validates if a given value passes the registered validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidValue<T>(T value);

    /// <summary>
    /// Validates if a given list value passes the registered list validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidListValue<T>(T value);

    /// <summary>
    /// Validates if a given map key passes the registered map key validator.
    /// </summary>
    /// <typeparam name="T">Type of key to validate</typeparam>
    /// <param name="value">Key to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidMapKey<T>(T value);

    /// <summary>
    /// Validates if a given map value passes the registered map value validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidMapValue<T>(T value);
}
