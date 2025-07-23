namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Tf;

/// <summary>
/// Internal type metadata - represents the shared data between type name aliases.
/// This matches the C++ Sdf_ValueTypeImpl pattern.
/// </summary>
internal sealed class SdfValueTypeMetadata
{
    public TfType Type { get; init; } = new();
    public string CppTypeName { get; init; } = string.Empty;
    public TfToken Role { get; init; } = TfToken.Empty;
    public VtValue DefaultValue { get; init; } = VtValue.Empty;
    public TfEnum DefaultUnit { get; init; }
    public SdfTupleDimensions Dimensions { get; init; } = new();
    public List<TfToken> Aliases { get; init; } = new();

    /// <summary>
    /// Hash based on type + role (matches C++ equality logic)
    /// </summary>
    public int GetHash() => HashCode.Combine(Type, Role);

    /// <summary>
    /// Equality based on type + role (matches C++ exactly)
    /// </summary>
    public bool Equals(SdfValueTypeMetadata? other) =>
        other is not null && Type == other.Type && Role == other.Role;
}

/// <summary>
/// Implementation of SdfValueTypeName that closely matches the C++ version.
/// Uses shared metadata instances like the C++ implementation.
/// </summary>
public sealed class SdfValueTypeName : ISdfValueTypeName, IEquatable<SdfValueTypeName>
{
    private static readonly SdfValueTypeMetadata s_emptyMetadata = new();
    private static readonly SdfValueTypeName s_invalid = new(TfToken.Empty, s_emptyMetadata);

    /// <summary>
    /// Represents an invalid/empty type name (matches C++ GetEmptyTypeName())
    /// </summary>
    public static SdfValueTypeName Invalid => s_invalid;

    private readonly TfToken _name;
    private readonly SdfValueTypeMetadata _metadata;
    private readonly SdfValueTypeName? _scalarType;
    private readonly SdfValueTypeName? _arrayType;

    // Internal constructor for the registry
    internal SdfValueTypeName(
        TfToken name,
        SdfValueTypeMetadata metadata,
        SdfValueTypeName? scalarType = null,
        SdfValueTypeName? arrayType = null)
    {
        _name = name;
        _metadata = metadata;
        _scalarType = scalarType;
        _arrayType = arrayType;
    }

    // Public constructor for simple cases
    public SdfValueTypeName(TfToken name) : this(name, s_emptyMetadata)
    {
    }

    // Interface implementation
    public TfToken Name => _name;
    public Type? UnderlyingType => _metadata.Type.Typeid;
    public string CppTypeName => _metadata.CppTypeName;
    public TfToken? Role => _metadata.Role.IsEmpty ? null : _metadata.Role;
    public VtValue? DefaultValue => _metadata.DefaultValue.IsEmpty() ? null : _metadata.DefaultValue;
    public object? DefaultUnit => _metadata.DefaultUnit; // Return TfEnum directly
    public SdfTupleDimensions Dimensions => _metadata.Dimensions;
    public IReadOnlyList<TfToken> Aliases => _metadata.Aliases;

    // Key behavioral properties that match C++ exactly
    public bool IsScalar => IsValid && ReferenceEquals(this, _scalarType);
    public bool IsArray => IsValid && ReferenceEquals(this, _arrayType);
    public bool IsValid => !_IsEmpty();

    public ISdfValueTypeName? ScalarType => _scalarType;
    public ISdfValueTypeName? ArrayType => _arrayType;

    /// <summary>
    /// Returns this type name as a token (matches C++ GetAsToken())
    /// </summary>
    public TfToken GetAsToken() => _name;

    /// <summary>
    /// Returns the TfType (matches C++ GetType())
    /// </summary>
    public TfType GetTfType() => _metadata.Type;

    /// <summary>
    /// Returns default unit as TfEnum (matches C++ GetDefaultUnit())
    /// </summary>
    public TfEnum GetDefaultUnit() => _metadata.DefaultUnit;

    /// <summary>
    /// Returns all aliases as tokens (matches C++ GetAliasesAsTokens())
    /// </summary>
    public IReadOnlyList<TfToken> GetAliasesAsTokens() => _metadata.Aliases;

    /// <summary>
    /// Equality based on core type metadata (matches C++ operator== exactly)
    /// </summary>
    public bool Equals(ISdfValueTypeName? other)
    {
        if (other is not SdfValueTypeName otherTypeName) return false;
        return _metadata.Equals(otherTypeName._metadata);
    }

    public bool Equals(SdfValueTypeName? other) =>
        other is not null && _metadata.Equals(other._metadata);

    /// <summary>
    /// String equality checks aliases (matches C++ operator==(string))
    /// </summary>
    public bool Equals(string? typeName) =>
        !string.IsNullOrEmpty(typeName) && _metadata.Aliases.Any(alias => alias.ToString() == typeName);

    /// <summary>
    /// Token equality checks aliases (matches C++ operator==(TfToken))
    /// </summary>
    public bool Equals(TfToken? token) =>
        token is not null && _metadata.Aliases.Contains(token.Value);

    /// <summary>
    /// Hash based on type + role (matches C++ GetHash() exactly)
    /// </summary>
    public override int GetHashCode() => _metadata.GetHash();

    /// <summary>
    /// Checks if this is the empty type name (matches C++ _IsEmpty())
    /// </summary>
    private bool _IsEmpty() => ReferenceEquals(_metadata, s_emptyMetadata);

    public override bool Equals(object? obj) => obj is SdfValueTypeName other && Equals(other);
    public override string ToString() => _name.ToString();

    // Operators that match C++ behavior
    public static bool operator ==(SdfValueTypeName? left, SdfValueTypeName? right) =>
        ReferenceEquals(left, right) || (left?.Equals(right) == true);

    public static bool operator !=(SdfValueTypeName? left, SdfValueTypeName? right) =>
        !(left == right);

    public static bool operator ==(SdfValueTypeName? left, string? right) =>
        left?.Equals(right) == true;

    public static bool operator !=(SdfValueTypeName? left, string? right) =>
        !(left == right);

    public static bool operator ==(string? left, SdfValueTypeName? right) =>
        right?.Equals(left) == true;

    public static bool operator !=(string? left, SdfValueTypeName? right) =>
        !(left == right);

    public static bool operator ==(SdfValueTypeName? left, TfToken? right) =>
        left?.Equals(right) == true;

    public static bool operator !=(SdfValueTypeName? left, TfToken? right) =>
        !(left == right);

    public static bool operator ==(TfToken? left, SdfValueTypeName? right) =>
        right?.Equals(left) == true;

    public static bool operator !=(TfToken? left, SdfValueTypeName? right) =>
        !(left == right);

    // Explicit conversion to bool (matches C++ explicit operator bool)
    public static explicit operator bool(SdfValueTypeName typeName) => typeName.IsValid;

    // Implicit conversions for convenience
    public static implicit operator TfToken(SdfValueTypeName typeName) => typeName._name;
}

/// <summary>
/// Hash functor for SdfValueTypeName (matches C++ SdfValueTypeNameHash)
/// </summary>
public struct SdfValueTypeNameHash
{
    public int GetHashCode(SdfValueTypeName typeName) => typeName.GetHashCode();
}