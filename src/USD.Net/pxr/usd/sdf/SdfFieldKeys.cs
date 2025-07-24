namespace Pxr.Usd.Sdf;

using Pxr.Base.Tf;

/// <summary>
/// Standard field keys used in USD scene description.
/// These correspond to the SDF_FIELD_KEYS macro in the C++ implementation.
/// </summary>
public static class SdfFieldKeys
{
    // Core fields
    public static readonly TfToken Active = new("active");
    public static readonly TfToken AssetInfo = new("assetInfo");
    public static readonly TfToken ColorConfiguration = new("colorConfiguration");
    public static readonly TfToken ColorManagementSystem = new("colorManagementSystem");
    public static readonly TfToken Comment = new("comment");
    public static readonly TfToken ConnectionPaths = new("connectionPaths");
    public static readonly TfToken Custom = new("custom");
    public static readonly TfToken CustomData = new("customData");
    public static readonly TfToken Default = new("default");
    public static readonly TfToken DefaultPrim = new("defaultPrim");
    public static readonly TfToken DisplayGroup = new("displayGroup");
    public static readonly TfToken DisplayName = new("displayName");
    public static readonly TfToken DisplayUnit = new("displayUnit");
    public static readonly TfToken Documentation = new("documentation");
    public static readonly TfToken EndFrame = new("endFrame");
    public static readonly TfToken EndTimeCode = new("endTimeCode");
    public static readonly TfToken FramePrecision = new("framePrecision");
    public static readonly TfToken FramesPerSecond = new("framesPerSecond");
    public static readonly TfToken Hidden = new("hidden");
    public static readonly TfToken Inherits = new("inherits");
    public static readonly TfToken InstanceName = new("instanceName");
    public static readonly TfToken Kind = new("kind");
    public static readonly TfToken LayerRelocates = new("layerRelocates");
    public static readonly TfToken PrimOrder = new("primOrder");
    public static readonly TfToken NoLoadHint = new("noLoadHint");
    public static readonly TfToken Owner = new("owner");
    public static readonly TfToken Payload = new("payload");
    public static readonly TfToken Permission = new("permission");
    public static readonly TfToken Prefix = new("prefix");
    public static readonly TfToken PrefixSubstitutions = new("prefixSubstitutions");
    public static readonly TfToken PropertyOrder = new("propertyOrder");
    public static readonly TfToken References = new("references");
    public static readonly TfToken Relocates = new("relocates");
    public static readonly TfToken SessionOwner = new("sessionOwner");
    public static readonly TfToken Specializes = new("specializes");
    public static readonly TfToken Specifier = new("specifier");
    public static readonly TfToken StartFrame = new("startFrame");
    public static readonly TfToken StartTimeCode = new("startTimeCode");
    public static readonly TfToken SubLayers = new("subLayers");
    public static readonly TfToken SubLayerOffsets = new("subLayerOffsets");
    public static readonly TfToken Suffix = new("suffix");
    public static readonly TfToken SuffixSubstitutions = new("suffixSubstitutions");
    public static readonly TfToken SymmetricPeer = new("symmetricPeer");
    public static readonly TfToken SymmetryArgs = new("symmetryArgs");
    public static readonly TfToken SymmetryArguments = new("symmetryArguments");
    public static readonly TfToken SymmetryFunction = new("symmetryFunction");
    public static readonly TfToken TargetPaths = new("targetPaths");
    public static readonly TfToken TimeSamples = new("timeSamples");
    public static readonly TfToken TimeCodesPerSecond = new("timeCodesPerSecond");
    public static readonly TfToken TypeName = new("typeName");
    public static readonly TfToken VariantSelection = new("variantSelection");
    public static readonly TfToken Variability = new("variability");
}

/// <summary>
/// Field keys for children specs.
/// These are special fields that hold child relationships.
/// </summary>
public static class SdfChildrenKeys
{
    public static readonly TfToken ConnectionChildren = new("connectionChildren");
    public static readonly TfToken ExpressionChildren = new("expressionChildren");
    public static readonly TfToken MapperArgChildren = new("mapperArgChildren");
    public static readonly TfToken MapperChildren = new("mapperChildren");
    public static readonly TfToken PrimChildren = new("primChildren");
    public static readonly TfToken PropertyChildren = new("propertyChildren");
    public static readonly TfToken RelationshipTargetChildren = new("relationshipTargetChildren");
    public static readonly TfToken VariantChildren = new("variantChildren");
    public static readonly TfToken VariantSetChildren = new("variantSetChildren");
}