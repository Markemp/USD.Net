using System.Collections.ObjectModel;

namespace Pxr.Base.Tf;

/// <summary>
/// TfToken is a lightweight, efficient string-like type used for representing identifiers and names efficiently.
/// It provides fast equality comparison and low memory overhead through string interning.
/// </summary>
public readonly struct TfToken : IEquatable<TfToken>
{
    private readonly string? _value;

    public TfToken(string? value)
    {
        _value = string.IsInterned(value ?? string.Empty) ?? string.Intern(value ?? string.Empty);
    }

    public TfToken(ReadOnlySpan<char> value)
    {
        var str = value.ToString();
        _value = string.IsInterned(str) ?? string.Intern(str);
    }

    public static implicit operator TfToken(string? value) => new(value);
    public static implicit operator string(TfToken token) => token._value ?? string.Empty;

    public bool IsEmpty => string.IsNullOrEmpty(_value);
    
    public string GetText() => _value ?? string.Empty;

    public bool Equals(TfToken other) => ReferenceEquals(_value, other._value);

    public override bool Equals(object? obj) => obj is TfToken other && Equals(other);

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public override string ToString() => _value ?? string.Empty;

    public static bool operator ==(TfToken left, TfToken right) => left.Equals(right);
    public static bool operator !=(TfToken left, TfToken right) => !left.Equals(right);

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