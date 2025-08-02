using System.Collections.ObjectModel;

namespace Pxr.Base.Tf;

/// <summary>
/// Token for efficient comparison, assignment, and hashing of known strings.
/// </summary>
/// <remarks>
/// A TfToken is a handle for a registered string, and can be compared,
/// assigned, and hashed in constant time.  It is useful when a bounded number
/// of strings are used as fixed symbols (but never modified).
///
/// For example, the set of avar names in a shot is large but bounded, and
/// once an avar name is discovered, it is never manipulated.  If these names
/// were passed around as strings, every comparison and hash would be linear
/// in the number of characters.  (String assignment itself is sometimes a
/// constant time operation, but it is sometimes linear in the length of the
/// string as well as requiring a memory allocation.)
///
/// To use TfToken, simply create an instance from a string or const char*.
/// If the string hasn't been seen before, a copy of it is added to a global
/// table.  The resulting TfToken is simply a wrapper around an string*,
/// pointing that canonical copy of the string.  Thus, operations on the token
/// are very fast.  (The string's hash is simply the address of the canonical
/// copy, so hashing the string is constant time.)
///
/// The free functions \c TfToTokenVector() and \c TfToStringVector() provide
/// conversions to and from vectors of \c string.
/// 
/// Note: Access to the global table is protected by a mutex.  This is a good
/// idea as long as clients do not construct tokens from strings too
/// frequently.  Construct tokens only as often as you must (for example, as
/// you read data files), and <i>never</i> in inner loops.  Of course, once
/// you have a token, feel free to compare, assign, and hash it as often as
/// you like.  (That's what it's for.)  In order to help prevent tokens from
/// being re-created over and over, auto type conversion from \c string and \c
/// char* to \c TfToken is disabled (you must use the explicit \c TfToken
/// constructors).  However, auto conversion from \c TfToken to \c string and
/// \c char* is provided.
/// </remarks>
public readonly struct TfToken : IEquatable<TfToken>, IComparable<TfToken>
{
    private readonly string? _value;

    /// <summary>
    /// Create the empty token, containing the empty string.
    /// </summary>
    public TfToken()
    {
        _value = string.Empty;
    }

    /// <summary>
    /// Acquire a token for the given string.
    /// </summary>
    /// <remarks>
    /// This constructor involves a string hash and a lookup in the global
    /// table, and so should not be done more often than necessary.  When
    /// possible, create a token once and reuse it many times.
    /// </remarks>
    public TfToken(string? value)
    {
        _value = string.IsInterned(value ?? string.Empty) ?? string.Intern(value ?? string.Empty);
    }

    /// <summary>
    /// Acquire a token for the given character span.
    /// </summary>
    /// <remarks>
    /// This constructor involves a string hash and a lookup in the global
    /// table, and so should not be done more often than necessary.  When
    /// possible, create a token once and reuse it many times.
    /// </remarks>
    public TfToken(ReadOnlySpan<char> value)
    {
        var str = value.ToString();
        _value = string.IsInterned(str) ?? string.Intern(str);
    }

    /// <summary>
    /// Find the token for the given string, if one exists.
    /// </summary>
    /// <remarks>
    /// If a token has previous been created for the given string, this
    /// will return it.  Otherwise, the empty token will be returned.
    /// </remarks>
    public static TfToken Find(string s)
    {
        var interned = string.IsInterned(s);
        return interned != null ? new TfToken(interned) : Empty;
    }

    /// <summary>
    /// Return a size_t hash for this token.
    /// </summary>
    /// <remarks>
    /// The hash is based on the token's storage identity; this is immutable
    /// as long as the token is in use anywhere in the process.
    /// </remarks>
    public int Hash() => _value?.GetHashCode() ?? 0;

    /// <summary>
    /// Return the size of the string that this token represents.
    /// </summary>
    public int Size => _value?.Length ?? 0;

    /// <summary>
    /// Return the text that this token represents.
    /// </summary>
    /// <remarks>
    /// The returned pointer value is not valid after this TfToken
    /// object has been destroyed.
    /// </remarks>
    public string GetText() => _value ?? string.Empty;

    /// <summary>
    /// Synonym for GetText().
    /// </summary>
    public string Data => GetText();

    /// <summary>
    /// Return the string that this token represents.
    /// </summary>
    public string GetString() => _value ?? string.Empty;

    /// <summary>
    /// Returns true iff this token contains the empty string ""
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(_value);

    /// <summary>
    /// Equality operator
    /// </summary>
    public bool Equals(TfToken other) => ReferenceEquals(_value, other._value);

    /// <summary>
    /// Equality operator for string. Not as fast as direct token to token equality testing
    /// </summary>
    public bool Equals(string other) => string.Equals(_value, other, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj switch
    {
        TfToken token => Equals(token),
        string str => Equals(str),
        _ => false
    };

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    /// <summary>
    /// Less-than operator that compares tokenized strings lexicographically.
    /// Allows TfToken to be used in std::set equivalent structures.
    /// </summary>
    public int CompareTo(TfToken other)
    {
        return string.Compare(_value, other._value, StringComparison.Ordinal);
    }

    public override string ToString() => _value ?? string.Empty;

    /// <summary>
    /// Allow TfToken to be auto-converted to string
    /// </summary>
    public static implicit operator string(TfToken token) => token.GetString();

    public static bool operator ==(TfToken left, TfToken right) => left.Equals(right);
    public static bool operator !=(TfToken left, TfToken right) => !left.Equals(right);

    /// <summary>
    /// Equality operator for string. Not as fast as direct token to token equality testing
    /// </summary>
    public static bool operator ==(TfToken token, string str) => token.Equals(str);
    public static bool operator !=(TfToken token, string str) => !token.Equals(str);
    public static bool operator ==(string str, TfToken token) => token.Equals(str);
    public static bool operator !=(string str, TfToken token) => !token.Equals(str);

    /// <summary>
    /// Less-than operator that compares tokenized strings lexicographically.
    /// </summary>
    public static bool operator <(TfToken left, TfToken right) => left.CompareTo(right) < 0;
    
    /// <summary>
    /// Greater-than operator that compares tokenized strings lexicographically.
    /// </summary>
    public static bool operator >(TfToken left, TfToken right) => left.CompareTo(right) > 0;
    
    /// <summary>
    /// Less-than-or-equal operator that compares tokenized strings lexicographically.
    /// </summary>
    public static bool operator <=(TfToken left, TfToken right) => left.CompareTo(right) <= 0;
    
    /// <summary>
    /// Greater-than-or-equal operator that compares tokenized strings lexicographically.
    /// </summary>
    public static bool operator >=(TfToken left, TfToken right) => left.CompareTo(right) >= 0;

    public static readonly TfToken Empty = new(string.Empty);
}

/// <summary>
/// USD Tokens - provides efficient access to commonly used string identifiers in USD
/// </summary>
public static class UsdTokens
{
    // Schema and property tokens
    public static readonly TfToken ApiSchemas = new("apiSchemas");
    public static readonly TfToken Clips = new("clips");
    public static readonly TfToken ClipSets = new("clipSets");
    public static readonly TfToken Collection = new("collection");

    // Collection API template tokens
    public static readonly TfToken CollectionMultipleApplyTemplate = new("collection:__INSTANCE_NAME__");
    public static readonly TfToken CollectionMultipleApplyTemplateExcludes = new("collection:__INSTANCE_NAME__:excludes");
    public static readonly TfToken CollectionMultipleApplyTemplateExpansionRule = new("collection:__INSTANCE_NAME__:expansionRule");
    public static readonly TfToken CollectionMultipleApplyTemplateIncludeRoot = new("collection:__INSTANCE_NAME__:includeRoot");
    public static readonly TfToken CollectionMultipleApplyTemplateIncludes = new("collection:__INSTANCE_NAME__:includes");
    public static readonly TfToken CollectionMultipleApplyTemplateMembershipExpression = new("collection:__INSTANCE_NAME__:membershipExpression");

    // Color space definition tokens
    public static readonly TfToken ColorSpaceDefinition = new("colorSpaceDefinition");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateBlueChroma = new("colorSpaceDefinition:__INSTANCE_NAME__:blueChroma");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateGamma = new("colorSpaceDefinition:__INSTANCE_NAME__:gamma");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateGreenChroma = new("colorSpaceDefinition:__INSTANCE_NAME__:greenChroma");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateLinearBias = new("colorSpaceDefinition:__INSTANCE_NAME__:linearBias");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateName = new("colorSpaceDefinition:__INSTANCE_NAME__:name");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateRedChroma = new("colorSpaceDefinition:__INSTANCE_NAME__:redChroma");
    public static readonly TfToken ColorSpaceDefinitionMultipleApplyTemplateWhitePoint = new("colorSpaceDefinition:__INSTANCE_NAME__:whitePoint");

    // General tokens
    public static readonly TfToken ColorSpaceName = new("colorSpace:name");
    public static readonly TfToken Custom = new("custom");
    public static readonly TfToken Exclude = new("exclude");
    public static readonly TfToken ExpandPrims = new("expandPrims");
    public static readonly TfToken ExpandPrimsAndProperties = new("expandPrimsAndProperties");
    public static readonly TfToken ExplicitOnly = new("explicitOnly");
    public static readonly TfToken FallbackPrimTypes = new("fallbackPrimTypes");

    // API Schema type tokens
    public static readonly TfToken APISchemaBase = new("APISchemaBase");
    public static readonly TfToken ClipsAPI = new("ClipsAPI");
    public static readonly TfToken CollectionAPI = new("CollectionAPI");
    public static readonly TfToken ColorSpaceAPI = new("ColorSpaceAPI");
    public static readonly TfToken ColorSpaceDefinitionAPI = new("ColorSpaceDefinitionAPI");
    public static readonly TfToken ModelAPI = new("ModelAPI");
    public static readonly TfToken Typed = new("Typed");

    /// <summary>
    /// Collection of all tokens for iteration and reflection scenarios
    /// </summary>
    public static readonly ReadOnlyCollection<TfToken> AllTokens = new(
    [
            ApiSchemas,
            Clips,
            ClipSets,
            Collection,
            CollectionMultipleApplyTemplate,
            CollectionMultipleApplyTemplateExcludes,
            CollectionMultipleApplyTemplateExpansionRule,
            CollectionMultipleApplyTemplateIncludeRoot,
            CollectionMultipleApplyTemplateIncludes,
            CollectionMultipleApplyTemplateMembershipExpression,
            ColorSpaceDefinition,
            ColorSpaceDefinitionMultipleApplyTemplateBlueChroma,
            ColorSpaceDefinitionMultipleApplyTemplateGamma,
            ColorSpaceDefinitionMultipleApplyTemplateGreenChroma,
            ColorSpaceDefinitionMultipleApplyTemplateLinearBias,
            ColorSpaceDefinitionMultipleApplyTemplateName,
            ColorSpaceDefinitionMultipleApplyTemplateRedChroma,
            ColorSpaceDefinitionMultipleApplyTemplateWhitePoint,
            ColorSpaceName,
            Custom,
            Exclude,
            ExpandPrims,
            ExpandPrimsAndProperties,
            ExplicitOnly,
            FallbackPrimTypes,
            APISchemaBase,
            ClipsAPI,
            CollectionAPI,
            ColorSpaceAPI,
            ColorSpaceDefinitionAPI,
            ModelAPI,
            Typed
        ]);

    /// <summary>
    /// Helper method to create instance-specific tokens from templates
    /// </summary>
    /// <param name="template">The template token containing __INSTANCE_NAME__</param>
    /// <param name="instanceName">The instance name to substitute</param>
    /// <returns>A new token with the instance name substituted</returns>
    public static TfToken CreateInstanceToken(TfToken template, string instanceName)
    {
        if (string.IsNullOrEmpty(instanceName))
            throw new ArgumentException("Instance name cannot be null or empty", nameof(instanceName));

        string tokenValue = template.GetText().Replace("__INSTANCE_NAME__", instanceName);
        return new TfToken(tokenValue);
    }

    /// <summary>
    /// Helper method to check if a token is a template (contains __INSTANCE_NAME__)
    /// </summary>
    /// <param name="token">The token to check</param>
    /// <returns>True if the token is a template</returns>
    public static bool IsTemplate(TfToken token) 
        => token.GetText().Contains("__INSTANCE_NAME__");
}

/// <summary>
/// Convert the vector of strings into a vector of TfToken
/// </summary>
public static class TfTokenUtilities
{
    /// <summary>
    /// Convert the list of strings into a list of TfToken
    /// </summary>
    public static List<TfToken> ToTokenList(IEnumerable<string> strings)
    {
        return strings.Select(s => new TfToken(s)).ToList();
    }

    /// <summary>
    /// Convert the list of TfToken into a list of strings
    /// </summary>
    public static List<string> ToStringList(IEnumerable<TfToken> tokens)
    {
        return tokens.Select(t => t.GetString()).ToList();
    }

    /// <summary>
    /// Overload hash_value for TfToken (equivalent to C++ hash_value function).
    /// </summary>
    public static int HashValue(TfToken token) => token.Hash();
}
