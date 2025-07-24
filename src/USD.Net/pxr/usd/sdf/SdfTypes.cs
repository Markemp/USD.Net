using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

public enum SdfSpecType
{
    Unknown = 0,
    Attribute,
    Connection,
    Expression,
    Mapper,
    MapperArg,
    Prim,
    PseudoRoot,
    Relationship,
    RelationshipTarget,
    Variant,
    VariantSet,
    Types
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
    Public,
    Private,
    NumPermissions
}

public enum SdfVariability
{
    Varying,
    Uniform,
    NumVariabilities
}

public static class SdfSpecifierHelpers
{
    public static bool SdfIsDefiningSpecifier(SdfSpecifier spec) 
        => spec != SdfSpecifier.SdfSpecifierOver;
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