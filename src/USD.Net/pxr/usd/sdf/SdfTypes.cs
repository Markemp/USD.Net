using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public enum SdfSpecType
{
    SdfSpecTypeUnknown = 0,
    SdfSpecTypeAttribute,
    SdfSpecTypeConnection,
    SdfSpecTypeExpression,
    SdfSpecTypeMapper,
    SdfSpecTypeMapperArg,
    SdfSpecTypePrim,
    SdfSpecTypePseudoRoot,
    SdfSpecTypeRelationship,
    SdfSpecTypeRelationshipTarget,
    SdfSpecTypeVariant,
    SdfSpecTypeVariantSet,
    SdfNumSpecTypes
}

public enum SdfSpecifier
{
    SdfSpecifierDef,
    SdfSpecifierOver,
    SdfSpecifierClass,
    SdfNumSpecifiers
}

public enum SdfPermission
{
    SdfPermissionPublic,
    SdfPermissionPrivate,
    SdfNumPermissions
}

public enum SdfVariability
{
    SdfVariabilityVarying,
    SdfVariabilityUniform,
    SdfNumVariabilities
}

public static class SdfSpecifierHelpers
{
    public static bool SdfIsDefiningSpecifier(SdfSpecifier spec)
    {
        return spec != SdfSpecifier.SdfSpecifierOver;
    }
}

public static class SdfDataTokens
{
    public static readonly string TimeSamples = "timeSamples";
}

public class SdfTimeSampleMap : Dictionary<double, VtValue>
{
    public SdfTimeSampleMap() : base() { }
    public SdfTimeSampleMap(IDictionary<double, VtValue> dictionary) : base(dictionary) { }
}