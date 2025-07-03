using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all USD schema classes.
/// Provides the foundation for both typed schemas and API schemas.
/// </summary>
[UsdSchema("SchemaBase", UsdSchemaKind.AbstractBase, IsAbstract = true)]
public abstract class UsdSchemaBase
{
    protected readonly UsdPrim _prim;
    
    #region Construction
    
    /// <summary>
    /// Construct a schema object from a prim.
    /// </summary>
    /// <param name="prim">The prim to wrap with this schema</param>
    protected UsdSchemaBase(UsdPrim prim)
    {
        _prim = prim ?? throw new ArgumentNullException(nameof(prim));
    }
    
    /// <summary>
    /// Construct an invalid schema object.
    /// </summary>
    protected UsdSchemaBase()
    {
        _prim = new UsdPrim(); // Invalid prim
    }
    
    #endregion
    
    #region Basic Properties
    
    /// <summary>
    /// The prim held by this schema.
    /// </summary>
    public UsdPrim Prim => _prim;
    
    /// <summary>
    /// The path of the prim held by this schema.
    /// </summary>
    public SdfPath Path => _prim.GetPath();
    
    /// <summary>
    /// Whether this schema object is valid (has a valid prim).
    /// </summary>
    public bool IsValid => _prim.IsValid();
    
    /// <summary>
    /// The stage that owns the prim held by this schema.
    /// </summary>
    public UsdStage? Stage => _prim.GetStage();
    
    #endregion
    
    #region Schema Information
    
    /// <summary>
    /// Get the schema kind for this schema type.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <returns>The schema kind</returns>
    protected abstract UsdSchemaKind GetSchemaKind();
    
    /// <summary>
    /// Get the schema type name (identifier) for this schema.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <returns>The schema type name</returns>
    protected abstract TfToken GetSchemaTypeName();
    
    /// <summary>
    /// Check if this schema is compatible with the held prim.
    /// Base implementation checks prim validity.
    /// Derived classes should override for specific compatibility checks.
    /// </summary>
    /// <returns>True if compatible, false otherwise</returns>
    protected virtual bool IsCompatible()
    {
        return _prim.IsValid();
    }
    
    #endregion
    
    #region Property Creation Helpers
    
    /// <summary>
    /// Create an attribute on the held prim with optional fallback value.
    /// </summary>
    /// <param name="name">The attribute name</param>
    /// <param name="typeName">The attribute type name</param>
    /// <param name="custom">Whether this is a custom attribute</param>
    /// <param name="variability">The attribute variability</param>
    /// <param name="fallbackValue">Optional fallback value</param>
    /// <returns>The created attribute</returns>
    protected UsdAttribute CreateAttribute(TfToken name, string typeName, bool custom = false, 
        SdfVariability variability = SdfVariability.Varying, VtValue? fallbackValue = null)
    {
        if (!_prim.IsValid())
            return new UsdAttribute();
            
        var attr = _prim.CreateAttribute(name.GetText(), typeName, custom);
        
        // Set variability if specified
        if (variability != SdfVariability.Varying)
        {
            // TODO: Implement variability setting when SdfAttributeSpec is available
        }
        
        // Set fallback value if provided
        if (fallbackValue is not null)
            attr.Set(fallbackValue);
        
        return attr;
    }
    
    /// <summary>
    /// Create a relationship on the held prim.
    /// </summary>
    /// <param name="name">The relationship name</param>
    /// <param name="custom">Whether this is a custom relationship</param>
    /// <returns>The created relationship</returns>
    protected UsdRelationship CreateRelationship(TfToken name, bool custom = false)
    {
        if (!_prim.IsValid())
            return new UsdRelationship();
            
        return _prim.CreateRelationship(name.GetText(), custom);
    }
    
    /// <summary>
    /// Get an attribute from the held prim by name.
    /// </summary>
    /// <param name="name">The attribute name</param>
    /// <returns>The attribute, or invalid if not found</returns>
    protected UsdAttribute GetAttribute(TfToken name) => _prim.GetAttribute(name.GetText());
    
    /// <summary>
    /// Get a relationship from the held prim by name.
    /// </summary>
    /// <param name="name">The relationship name</param>
    /// <returns>The relationship, or invalid if not found</returns>
    protected UsdRelationship GetRelationship(TfToken name) => _prim.GetRelationship(name.GetText());
    
    #endregion
    
    #region Conversion Operators
    
    /// <summary>
    /// Implicit conversion to bool for validity checking.
    /// </summary>
    /// <param name="schema">The schema to check</param>
    /// <returns>True if the schema is valid</returns>
    public static implicit operator bool(UsdSchemaBase? schema) => schema?.IsValid == true;
    
    /// <summary>
    /// Explicit conversion to UsdPrim.
    /// </summary>
    /// <param name="schema">The schema to convert</param>
    /// <returns>The held prim</returns>
    public static explicit operator UsdPrim(UsdSchemaBase schema) => schema._prim;
    
    #endregion
    
    #region Object Overrides
    
    public override bool Equals(object? obj)
    {
        if (obj is UsdSchemaBase other)
            return _prim.GetPath().Equals(other._prim.GetPath()) && 
                   GetType() == other.GetType();
        return false;
    }
    
    public override int GetHashCode() => HashCode.Combine(_prim.GetPath(), GetType());
    
    public override string ToString()
    {
        if (!IsValid)
            return $"Invalid {GetType().Name}";
            
        return $"{GetType().Name}({_prim.GetPath()})";
    }
    
    #endregion
}

/// <summary>
/// Enum representing SDF variability for attributes.
/// TODO: Move this to SDF module when it's more complete.
/// </summary>
public enum SdfVariability
{
    /// <summary>
    /// Attribute value can vary over time.
    /// </summary>
    Varying,
    
    /// <summary>
    /// Attribute value is uniform (constant over time).
    /// </summary>
    Uniform
}