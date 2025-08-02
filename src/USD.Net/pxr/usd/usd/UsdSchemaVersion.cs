using Pxr.Base.Tf;

namespace Pxr.Usd;

/// <summary>
/// Schema versions are specified as a single unsigned integer value.
/// This is equivalent to the C++ UsdSchemaVersion type alias.
/// </summary>
/// <remarks>
/// Schema versioning allows USD schemas to evolve over time while maintaining
/// backward compatibility. Version 0 represents the base version of a schema
/// family, with higher numbers representing newer versions.
/// 
/// Schema identifiers can contain version suffixes in the format "SchemaName_X"
/// where X is the version number. If no version suffix is present, version 0
/// is assumed.
/// </remarks>
public readonly struct UsdSchemaVersion : IEquatable<UsdSchemaVersion>, IComparable<UsdSchemaVersion>
{
    private readonly uint _value;

    public UsdSchemaVersion(uint value)
    {
        _value = value;
    }

    public uint Value => _value;

    /// <summary>
    /// The default/base version of any schema family.
    /// </summary>
    public static readonly UsdSchemaVersion Default = new(0);

    /// <summary>
    /// Parse schema family and version from a schema identifier.
    /// </summary>
    /// <param name="schemaIdentifier">The schema identifier to parse</param>
    /// <returns>A tuple containing the schema family and version</returns>
    /// <remarks>
    /// Schema identifiers with version suffixes are in the format "SchemaName_X"
    /// where X is the version number. If no version suffix is present, 
    /// version 0 is returned.
    /// </remarks>
    public static (TfToken family, UsdSchemaVersion version) ParseFamilyAndVersionFromIdentifier(TfToken schemaIdentifier)
    {
        var idString = schemaIdentifier.ToString();
        var delimIndex = FindVersionDelimiter(idString);

        if (delimIndex == -1)
        {
            // No version suffix found, family is the identifier and version is 0
            return (schemaIdentifier, Default);
        }

        // Parse family and version
        var family = new TfToken(idString.Substring(0, delimIndex));
        var versionString = idString.Substring(delimIndex + 1);
        
        if (uint.TryParse(versionString, out var version))
        {
            return (family, new UsdSchemaVersion(version));
        }

        // If version parsing fails, treat as no version suffix
        return (schemaIdentifier, Default);
    }

    /// <summary>
    /// Create a schema identifier from family and version.
    /// </summary>
    /// <param name="schemaFamily">The schema family name</param>
    /// <param name="schemaVersion">The schema version</param>
    /// <returns>The composed schema identifier</returns>
    public static TfToken MakeSchemaIdentifierForFamilyAndVersion(TfToken schemaFamily, UsdSchemaVersion schemaVersion)
    {
        if (schemaVersion._value == 0)
        {
            // Version 0 doesn't get a suffix
            return schemaFamily;
        }

        return new TfToken($"{schemaFamily}_{schemaVersion._value}");
    }

    private static int FindVersionDelimiter(string idString)
    {
        // Find the last underscore that's followed only by digits
        for (int i = idString.Length - 1; i >= 0; i--)
        {
            if (idString[i] == '_')
            {
                // Check if everything after this underscore is digits
                var afterUnderscore = idString.Substring(i + 1);
                if (!string.IsNullOrEmpty(afterUnderscore) && 
                    afterUnderscore.All(char.IsDigit))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    #region Equality and Comparison

    public bool Equals(UsdSchemaVersion other) => _value == other._value;

    public override bool Equals(object? obj) => obj is UsdSchemaVersion other && Equals(other);

    public override int GetHashCode() => _value.GetHashCode();

    public int CompareTo(UsdSchemaVersion other) => _value.CompareTo(other._value);

    public static bool operator ==(UsdSchemaVersion left, UsdSchemaVersion right) => left.Equals(right);

    public static bool operator !=(UsdSchemaVersion left, UsdSchemaVersion right) => !left.Equals(right);

    public static bool operator <(UsdSchemaVersion left, UsdSchemaVersion right) => left._value < right._value;

    public static bool operator <=(UsdSchemaVersion left, UsdSchemaVersion right) => left._value <= right._value;

    public static bool operator >(UsdSchemaVersion left, UsdSchemaVersion right) => left._value > right._value;

    public static bool operator >=(UsdSchemaVersion left, UsdSchemaVersion right) => left._value >= right._value;

    #endregion

    #region Implicit Conversions

    public static implicit operator UsdSchemaVersion(uint value) => new(value);

    public static implicit operator uint(UsdSchemaVersion version) => version._value;

    #endregion

    public override string ToString() => _value.ToString();
}