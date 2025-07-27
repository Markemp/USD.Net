namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

// AIDEV-NOTE: Interface based on OpenUSD SdfSchemaBase - DO NOT MODIFY WITHOUT PERMISSION
/// <summary>
/// Represents a value type name - an attribute's type name with associated metadata.
/// 
/// This associates a string name with a .NET Type and optional metadata like role,
/// default values, and dimensions. Multiple names can alias the same type/role pair.
/// </summary>
public interface ISdfValueTypeName : IEquatable<ISdfValueTypeName>
{
    /// <summary>
    /// Gets the type name as a token.
    /// </summary>
    TfToken Name { get; }

    /// <summary>
    /// Gets the underlying .NET type, or null if this is just a name without a known type.
    /// </summary>
    Type? UnderlyingType { get; }

    /// <summary>
    /// Gets the C++ type name for this type (for compatibility/serialization).
    /// </summary>
    string CppTypeName { get; }

    /// <summary>
    /// Gets the type's role (e.g., "Color", "Point", "Normal").
    /// </summary>
    TfToken? Role { get; }

    /// <summary>
    /// Gets the default value for this type.
    /// </summary>
    VtValue? DefaultValue { get; }

    /// <summary>
    /// Gets the default unit for this type.
    /// </summary>
    object? DefaultUnit { get; }

    /// <summary>
    /// Gets the dimensions/shape of this type (e.g., 3 for a 3D vector).
    /// </summary>
    SdfTupleDimensions Dimensions { get; }

    /// <summary>
    /// Gets whether this represents a scalar type.
    /// </summary>
    bool IsScalar { get; }

    /// <summary>
    /// Gets whether this represents an array type.
    /// </summary>
    bool IsArray { get; }

    /// <summary>
    /// Gets whether this is a valid type name (has actual type information).
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Gets the scalar version of this type if it's an array, otherwise returns this type.
    /// Returns null if no scalar version exists.
    /// </summary>
    ISdfValueTypeName? ScalarType { get; }

    /// <summary>
    /// Gets the array version of this type if it's scalar, otherwise returns this type.
    /// Returns null if no array version exists.
    /// </summary>
    ISdfValueTypeName? ArrayType { get; }

    /// <summary>
    /// Gets all aliases for this type name.
    /// </summary>
    IReadOnlyList<TfToken> Aliases { get; }

    /// <summary>
    /// Gets a hash code for this type name.
    /// </summary>
    int GetHashCode();

    /// <summary>
    /// Checks if this type name equals a string name.
    /// </summary>
    bool Equals(string? typeName);

    /// <summary>
    /// Checks if this type name equals a token.
    /// </summary>
    bool Equals(TfToken? token);
}