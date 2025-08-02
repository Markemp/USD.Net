using Pxr.Base.Tf;
using Pxr.Usd.Sdf;
using Pxr.Pcp;

namespace Pxr.Usd;

public interface IUsdPrim : IUsdObject
{
    #region Core Prim Properties

    TfToken GetTypeName();
    bool SetTypeName(TfToken typeName);
    bool ClearTypeName();
    bool HasAuthoredTypeName();
    SdfSpecifier GetSpecifier();
    bool SetSpecifier(SdfSpecifier specifier);
    IReadOnlyList<TfToken> GetAppliedSchemas();

    #endregion

    #region Active State

    bool IsActive();
    bool SetActive(bool active);
    bool ClearActive();
    bool HasAuthoredActive();

    #endregion

    #region State Queries

    bool IsLoaded();
    bool IsAbstract();
    bool IsDefined();
    bool HasDefiningSpecifier();
    bool IsModel();
    bool IsGroup();
    bool IsComponent();
    bool IsSubComponent();

    #endregion

    #region Prim Stack and Composition

    IReadOnlyList<SdfPrimSpecHandle> GetPrimStack();
    IReadOnlyList<(SdfPrimSpecHandle, SdfLayerOffset)> GetPrimStackWithLayerOffsets();

    #endregion

    #region Properties

    IReadOnlyList<TfToken> GetPropertyNames();
    IReadOnlyList<TfToken> GetAuthoredPropertyNames();
    IReadOnlyList<IUsdProperty> GetProperties();
    IReadOnlyList<IUsdProperty> GetAuthoredProperties();
    IReadOnlyList<IUsdProperty> GetPropertiesInNamespace(string namespaces);
    IReadOnlyList<IUsdProperty> GetPropertiesInNamespace(IReadOnlyList<string> namespaces);
    IReadOnlyList<IUsdProperty> GetAuthoredPropertiesInNamespace(string namespaces);
    IReadOnlyList<IUsdProperty> GetAuthoredPropertiesInNamespace(IReadOnlyList<string> namespaces);
    IReadOnlyList<TfToken> GetPropertyOrder();
    bool SetPropertyOrder(IReadOnlyList<TfToken> order);
    IUsdProperty GetProperty(TfToken propName);
    bool HasProperty(TfToken propName);
    bool RemoveProperty(TfToken propName);

    #endregion

    #region Kind

    bool GetKind(out TfToken kind);
    bool SetKind(TfToken kind);
    bool ClearKind();
    bool HasAuthoredKind();

    #endregion

    #region Schema Operations

    bool IsA(TfType schemaType);
    bool IsA(TfToken schemaIdentifier);
    bool IsA(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool IsInFamily(TfToken schemaFamily);
    bool IsInFamily(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool IsInFamily(TfType schemaType, UsdSchemaRegistry.VersionPolicy versionPolicy);
    bool IsInFamily(TfToken schemaIdentifier, UsdSchemaRegistry.VersionPolicy versionPolicy);
    bool GetVersionIfIsInFamily(TfToken schemaFamily, out UsdSchemaVersion schemaVersion);

    bool HasAPI(TfType schemaType);
    bool HasAPI(TfType schemaType, TfToken instanceName);
    bool HasAPI(TfToken schemaIdentifier);
    bool HasAPI(TfToken schemaIdentifier, TfToken instanceName);
    bool HasAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool HasAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion, TfToken instanceName);
    bool HasAPIInFamily(TfToken schemaFamily);
    bool HasAPIInFamily(TfToken schemaFamily, TfToken instanceName);
    bool HasAPIInFamily(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool HasAPIInFamily(TfToken schemaFamily, UsdSchemaVersion schemaVersion, TfToken instanceName);
    bool HasAPIInFamily(TfType schemaType, UsdSchemaRegistry.VersionPolicy versionPolicy);
    bool HasAPIInFamily(TfType schemaType, UsdSchemaRegistry.VersionPolicy versionPolicy, TfToken instanceName);
    bool HasAPIInFamily(TfToken schemaIdentifier, UsdSchemaRegistry.VersionPolicy versionPolicy);
    bool HasAPIInFamily(TfToken schemaIdentifier, UsdSchemaRegistry.VersionPolicy versionPolicy, TfToken instanceName);
    bool GetVersionIfHasAPIInFamily(TfToken schemaFamily, out UsdSchemaVersion schemaVersion);
    bool GetVersionIfHasAPIInFamily(TfToken schemaFamily, TfToken instanceName, out UsdSchemaVersion schemaVersion);

    bool CanApplyAPI(TfType schemaType, out string whyNot);
    bool CanApplyAPI(TfType schemaType, TfToken instanceName, out string whyNot);
    bool CanApplyAPI(TfToken schemaIdentifier, out string whyNot);
    bool CanApplyAPI(TfToken schemaIdentifier, TfToken instanceName, out string whyNot);
    bool CanApplyAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion, out string whyNot);
    bool CanApplyAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion, TfToken instanceName, out string whyNot);

    bool ApplyAPI(TfType schemaType);
    bool ApplyAPI(TfType schemaType, TfToken instanceName);
    bool ApplyAPI(TfToken schemaIdentifier);
    bool ApplyAPI(TfToken schemaIdentifier, TfToken instanceName);
    bool ApplyAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool ApplyAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion, TfToken instanceName);

    bool RemoveAPI(TfType schemaType);
    bool RemoveAPI(TfType schemaType, TfToken instanceName);
    bool RemoveAPI(TfToken schemaIdentifier);
    bool RemoveAPI(TfToken schemaIdentifier, TfToken instanceName);
    bool RemoveAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion);
    bool RemoveAPI(TfToken schemaFamily, UsdSchemaVersion schemaVersion, TfToken instanceName);

    bool AddAppliedSchema(TfToken appliedSchemaName);
    bool RemoveAppliedSchema(TfToken appliedSchemaName);

    #endregion

    #region Hierarchy Navigation

    IUsdPrim GetParent();
    IUsdPrim GetChild(TfToken name);
    IReadOnlyList<TfToken> GetChildrenNames();
    IReadOnlyList<TfToken> GetAllChildrenNames();
    IReadOnlyList<TfToken> GetFilteredChildrenNames(Func<IUsdPrim, bool> predicate);
    IEnumerable<IUsdPrim> GetChildren();
    IEnumerable<IUsdPrim> GetAllChildren();
    IEnumerable<IUsdPrim> GetFilteredChildren(Func<IUsdPrim, bool> predicate);
    IEnumerable<IUsdPrim> GetDescendants();
    IEnumerable<IUsdPrim> GetFilteredDescendants(Func<IUsdPrim, bool> predicate);
    IReadOnlyList<TfToken> GetChildrenReorder();
    bool SetChildrenReorder(IReadOnlyList<TfToken> order);
    IUsdPrim GetNextSibling();
    IUsdPrim GetFilteredNextSibling(Func<IUsdPrim, bool> predicate);
    bool IsPseudoRoot();

    #endregion

    #region Path-based Access

    IUsdPrim GetPrimAtPath(ISdfPath path);
    UsdObject GetObjectAtPath(ISdfPath path);
    IUsdProperty GetPropertyAtPath(ISdfPath path);
    IUsdAttribute GetAttributeAtPath(ISdfPath path);
    IUsdRelationship GetRelationshipAtPath(ISdfPath path);

    #endregion

    #region Attributes

    bool HasAttribute(TfToken attrName);
    IUsdAttribute GetAttribute(TfToken attrName);
    IReadOnlyList<IUsdAttribute> GetAttributes();
    IReadOnlyList<IUsdAttribute> GetAuthoredAttributes();
    IUsdAttribute CreateAttribute(TfToken name, SdfValueTypeName typeName, bool custom = false, SdfVariability variability = SdfVariability.Varying);
    IUsdAttribute CreateAttribute(IReadOnlyList<string> nameElts, SdfValueTypeName typeName, bool custom = false, SdfVariability variability = SdfVariability.Varying);
    IReadOnlyList<ISdfPath> FindAllAttributeConnectionPaths();
    IReadOnlyList<ISdfPath> FindAllAttributeConnectionPaths(Func<IUsdAttribute, bool> predicate);

    #endregion

    #region Relationships

    bool HasRelationship(TfToken relName);
    IUsdRelationship GetRelationship(TfToken relName);
    IReadOnlyList<IUsdRelationship> GetRelationships();
    IReadOnlyList<IUsdRelationship> GetAuthoredRelationships();
    IUsdRelationship CreateRelationship(TfToken relName, bool custom = true);
    IUsdRelationship CreateRelationship(IReadOnlyList<string> nameElts, bool custom = true);
    IReadOnlyList<ISdfPath> FindAllRelationshipTargetPaths();
    IReadOnlyList<ISdfPath> FindAllRelationshipTargetPaths(Func<IUsdRelationship, bool> predicate);

    #endregion

    #region Variants

    UsdVariantSets GetVariantSets();
    UsdVariantSet GetVariantSet(string variantSetName);
    bool HasVariantSets();

    #endregion

    #region Payloads

    UsdPayloads GetPayloads();
    bool HasAuthoredPayloads();
    bool HasPayload();
    bool SetPayload(SdfPayload payload);
    bool SetPayload(string assetPath, ISdfPath primPath);
    bool SetPayload(ISdfLayer layer, ISdfPath primPath);
    bool ClearPayload();
    void Load(UsdLoadPolicy policy = UsdLoadPolicy.UsdLoadWithDescendants);
    void Unload();

    #endregion

    #region References

    UsdReferences GetReferences();
    bool HasAuthoredReferences();

    #endregion

    #region Inherits

    UsdInherits GetInherits();
    bool HasAuthoredInherits();

    #endregion

    #region Specializes

    UsdSpecializes GetSpecializes();
    bool HasAuthoredSpecializes();

    #endregion

    #region Instancing

    bool IsInstance();
    bool IsInstanceProxy();
    bool IsInPrototype();
    bool IsInstanceable();
    bool SetInstanceable(bool instanceable);
    bool ClearInstanceable();
    bool HasAuthoredInstanceable();
    IUsdPrim GetPrototype();
    IReadOnlyList<IUsdPrim> GetInstances();

    #endregion

    #region Composition and Resolve Targets

    PcpPrimIndex ComputeExpandedPrimIndex();
    UsdResolveTarget MakeResolveTargetUpToEditTarget(UsdEditTarget editTarget);
    UsdResolveTarget MakeResolveTargetStrongerThanEditTarget(UsdEditTarget editTarget);

    #endregion
}