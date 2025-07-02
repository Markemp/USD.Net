using System;
using Pxr.Base.Tf;

namespace Pxr.Usd;

/// <summary>
/// Enum representing the different kinds of USD schemas.
/// Based on the OpenUSD schema classification system.
/// </summary>
public enum UsdSchemaKind
{
    /// <summary>
    /// Invalid/unknown schema type.
    /// </summary>
    Invalid,
    
    /// <summary>
    /// Abstract base schema classes (UsdSchemaBase, UsdAPISchemaBase, UsdTyped).
    /// </summary>
    AbstractBase,
    
    /// <summary>
    /// Abstract typed schemas that provide a base for other typed schemas.
    /// </summary>
    AbstractTyped,
    
    /// <summary>
    /// Concrete typed schemas that can be instantiated as prim types.
    /// </summary>
    ConcreteTyped,
    
    /// <summary>
    /// Non-applied API schemas that are not automatically applied to prims.
    /// </summary>
    NonAppliedAPI,
    
    /// <summary>
    /// Single-apply API schemas that can be applied only once to a prim.
    /// </summary>
    SingleApplyAPI,
    
    /// <summary>
    /// Multiple-apply API schemas that can be applied multiple times with different instance names.
    /// </summary>
    MultipleApplyAPI
}

/// <summary>
/// Information about a registered USD schema.
/// </summary>
public class UsdSchemaInfo
{
    /// <summary>
    /// The schema identifier (e.g., "Mesh", "CollectionAPI").
    /// </summary>
    public TfToken Identifier { get; set; } = TfToken.Empty;
    
    /// <summary>
    /// The .NET type for this schema class.
    /// </summary>
    public Type? Type { get; set; }
    
    /// <summary>
    /// The schema family name (for versioning support).
    /// </summary>
    public TfToken Family { get; set; } = TfToken.Empty;
    
    /// <summary>
    /// The schema version number.
    /// </summary>
    public uint Version { get; set; } = 0;
    
    /// <summary>
    /// The kind of schema this represents.
    /// </summary>
    public UsdSchemaKind Kind { get; set; } = UsdSchemaKind.Invalid;
    
    /// <summary>
    /// Whether this schema is abstract (cannot be instantiated directly).
    /// </summary>
    public bool IsAbstract { get; set; } = false;
    
    /// <summary>
    /// For typed schemas, the type name that this schema represents.
    /// </summary>
    public TfToken TypeName { get; set; } = TfToken.Empty;
}

/// <summary>
/// Attribute to mark USD schema classes and provide metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class UsdSchemaAttribute : Attribute
{
    public UsdSchemaAttribute(string identifier, UsdSchemaKind kind)
    {
        Identifier = identifier;
        Kind = kind;
    }
    
    /// <summary>
    /// The schema identifier.
    /// </summary>
    public string Identifier { get; }
    
    /// <summary>
    /// The kind of schema.
    /// </summary>
    public UsdSchemaKind Kind { get; }
    
    /// <summary>
    /// The schema family name (for versioning).
    /// </summary>
    public string? Family { get; set; }
    
    /// <summary>
    /// The schema version number.
    /// </summary>
    public uint Version { get; set; } = 0;
    
    /// <summary>
    /// For typed schemas, the type name.
    /// </summary>
    public string? TypeName { get; set; }
    
    /// <summary>
    /// Whether this schema is abstract.
    /// </summary>
    public bool IsAbstract { get; set; } = false;
}