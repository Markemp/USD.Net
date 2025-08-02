using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Interface for UsdProperty - base class for UsdAttribute and UsdRelationship scenegraph objects.
/// </summary>
/// <remarks>
/// USD API compatible with OpenUSD 23.11+
/// See OpenUSD documentation: https://openusd.org/release/api/class_usd_property.html
/// 
/// Base class for UsdAttribute and UsdRelationship scenegraph objects.
/// 
/// UsdProperty has a bool conversion operator that validates that the property
/// IsDefined() and thus valid for querying and authoring values and metadata.
/// This is a fairly expensive query that we do not cache, so if client
/// code retains UsdProperty objects it should manage its object validity
/// closely for performance. An ideal pattern is to listen for
/// UsdNotice::StageContentsChanged notifications, and revalidate/refetch
/// retained UsdObjects only then and otherwise use them without validity
/// checking.
/// </remarks>
public interface IUsdProperty : IUsdObject
{
    #region Object and Namespace Accessors

    /// <summary>
    /// Returns a strength-ordered list of property specs that provide
    /// opinions for this property.
    /// </summary>
    /// <param name="time">Time code for value clips consideration. If UsdTimeCode::Default(),
    /// or this property is a UsdRelationship (which are never affected by clips), we will 
    /// not consider value clips for opinions.</param>
    /// <returns>Property specs ordered from strongest to weakest opinion</returns>
    /// <remarks>
    /// If time is UsdTimeCode::Default(), or this property 
    /// is a UsdRelationship (which are never affected by clips), we will 
    /// not consider value clips for opinions. For any other time, for 
    /// a UsdAttribute, clips whose samples may contribute an opinion will 
    /// be included. These specs are ordered from strongest to weakest opinion, 
    /// although if time requires interpolation between two adjacent clips, 
    /// both clips will appear, sequentially.
    /// 
    /// The results returned by this method are meant for debugging
    /// and diagnostic purposes. It is not advisable to retain a 
    /// PropertyStack for the purposes of expedited value resolution for 
    /// properties, since the makeup of an attribute's PropertyStack may
    /// itself be time-varying. To expedite repeated value resolution of
    /// attributes, you should instead retain a UsdAttributeQuery.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IReadOnlyList<SdfPropertySpec> GetPropertyStack(UsdTimeCode time = default);

    /// <summary>
    /// Returns a strength-ordered list of property specs that provide
    /// opinions for this property paired with the cumulative layer offset from
    /// the stage's root layer to the layer containing the property spec.
    /// </summary>
    /// <param name="time">Time code for value clips consideration</param>
    /// <returns>Property specs with their cumulative layer offsets</returns>
    /// <remarks>
    /// This behaves exactly the same as UsdProperty::GetPropertyStack with the 
    /// addition of providing the cumulative layer offset of each spec's layer.
    /// 
    /// The results returned by this method are meant for debugging
    /// and diagnostic purposes. It is not advisable to retain a 
    /// PropertyStack for the purposes of expedited value resolution for 
    /// properties, since the makeup of an attribute's PropertyStack may
    /// itself be time-varying. To expedite repeated value resolution of
    /// attributes, you should instead retain a UsdAttributeQuery.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IReadOnlyList<(SdfPropertySpec spec, SdfLayerOffset offset)> GetPropertyStackWithLayerOffsets(UsdTimeCode time = default);

    /// <summary>
    /// Return this property's name with all namespace prefixes removed,
    /// i.e. the last component of the return value of GetName()
    /// </summary>
    /// <returns>The base name without namespace prefixes</returns>
    /// <remarks>
    /// This is generally the property's "client name"; property namespaces are
    /// often used to group related properties together. The namespace prefixes
    /// the property name but many consumers will care only about un-namespaced
    /// name, i.e. its BaseName. For more information, see Usd_Ordering
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    TfToken GetBaseName();

    /// <summary>
    /// Return this property's complete namespace prefix. Return the empty
    /// token if this property has no namespaces.
    /// </summary>
    /// <returns>The namespace prefix without trailing delimiter</returns>
    /// <remarks>
    /// This is the complement of GetBaseName(), although it does not
    /// contain a trailing namespace delimiter
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    TfToken GetNamespace();

    /// <summary>
    /// Return this property's name elements including namespaces and its base
    /// name as the final element.
    /// </summary>
    /// <returns>Array of name components including namespaces and base name</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IReadOnlyList<string> SplitName();

    #endregion

    #region Core Metadata

    /// <summary>
    /// Return this property's display group (metadata). This returns the
    /// empty string if no display group has been set.
    /// </summary>
    /// <returns>The display group string</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    string GetDisplayGroup();

    /// <summary>
    /// Sets this property's display group (metadata). Returns true on success.
    /// </summary>
    /// <param name="displayGroup">The display group to set</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// DisplayGroup provides UI hinting for grouping related properties
    /// together for display. We define a convention for specifying nesting
    /// of groups by recognizing the property namespace separator in 
    /// displayGroup as denoting group-nesting.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool SetDisplayGroup(string displayGroup);

    /// <summary>
    /// Clears this property's display group (metadata) in
    /// the current EditTarget (only). Returns true on success.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool ClearDisplayGroup();

    /// <summary>
    /// Returns true if displayGroup was explicitly authored and GetMetadata()
    /// will return a meaningful value for displayGroup.
    /// </summary>
    /// <returns>True if display group is authored, false otherwise</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool HasAuthoredDisplayGroup();

    /// <summary>
    /// Return this property's displayGroup as a sequence of groups to be
    /// nested, or an empty array if displayGroup is empty or not authored.
    /// </summary>
    /// <returns>Array of nested display group names</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IReadOnlyList<string> GetNestedDisplayGroups();

    /// <summary>
    /// Sets this property's display group (metadata) to the nested sequence.  
    /// Returns true on success.
    /// </summary>
    /// <param name="nestedGroups">The nested group hierarchy to set</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// A displayGroup set with this method can still be retrieved with
    /// GetDisplayGroup(), with the namespace separator embedded in the result.
    /// If nestedGroups is empty, we author an empty string for displayGroup.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool SetNestedDisplayGroups(IEnumerable<string> nestedGroups);

    /// <summary>
    /// Return true if this is a custom property (i.e., not part of a
    /// prim schema).
    /// </summary>
    /// <returns>True if this is a custom property, false otherwise</returns>
    /// <remarks>
    /// The 'custom' modifier in USD serves the same function as Alembic's
    /// 'userProperties', which is to say as a categorization for ad hoc
    /// client data not formalized into any schema, and therefore not 
    /// carrying an expectation of specific processing by consuming applications.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool IsCustom();

    /// <summary>
    /// Set the value for custom at the current EditTarget, return true on
    /// success, false if the value can not be written.
    /// </summary>
    /// <param name="isCustom">Whether this property should be marked as custom</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Note that this value should not be changed as it is typically either
    /// automatically authored or provided by a property definition. This method
    /// is provided primarily for fixing invalid scene description.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool SetCustom(bool isCustom);

    #endregion

    #region Existence and Validity

    /// <summary>
    /// Return true if this is a builtin property or if the strongest
    /// authored SdfPropertySpec for this property's path matches this
    /// property's dynamic type.
    /// </summary>
    /// <returns>True if the property is defined, false otherwise</returns>
    /// <remarks>
    /// That is, SdfRelationshipSpec in case this is a
    /// UsdRelationship, and SdfAttributeSpec in case this is a UsdAttribute.
    /// Return false if this property's prim has expired.
    /// 
    /// For attributes, a true return does not imply that this attribute
    /// possesses a value, only that has been declared, is of a certain type and
    /// variability, and that it is safe to use to query and author values and
    /// metadata.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool IsDefined();

    /// <summary>
    /// Return true if there are any authored opinions for this property
    /// in any layer that contributes to this stage, false otherwise.
    /// </summary>
    /// <returns>True if the property has authored opinions, false otherwise</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool IsAuthored();

    /// <summary>
    /// Return true if there is an SdfPropertySpec authored for this
    /// property at the given editTarget, otherwise return false.
    /// </summary>
    /// <param name="editTarget">The edit target to check for authoring</param>
    /// <returns>True if authored at the specific edit target, false otherwise</returns>
    /// <remarks>
    /// Note that this method does not do partial composition. It does not consider
    /// whether authored scene description exists at editTarget or weaker,
    /// only exactly at the given editTarget.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    bool IsAuthoredAt(UsdEditTarget editTarget);

    #endregion

    #region Flattening

    /// <summary>
    /// Flattens this property to a property spec with the same name 
    /// beneath the given parent prim in the edit target of its owning stage.
    /// </summary>
    /// <param name="parent">The parent prim to flatten beneath</param>
    /// <returns>The flattened property</returns>
    /// <remarks>
    /// The parent prim may belong to a different stage than this property's 
    /// owning stage.
    /// 
    /// Flattening authors all authored resolved values and metadata for 
    /// this property into the destination property spec. If this property
    /// is a builtin property, fallback values and metadata will also be
    /// authored if the destination property has a different fallback 
    /// value or no fallback value, or if the destination property has an
    /// authored value that overrides its fallback.
    /// 
    /// Attribute connections and relationship targets that target an
    /// object beneath this property's owning prim will be remapped to
    /// target objects beneath the destination parent prim.
    /// 
    /// If the destination spec already exists, it will be overwritten.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IUsdProperty FlattenTo(IUsdPrim parent);

    /// <summary>
    /// Flattens this property to a property spec with the given
    /// propName beneath the given parent prim in the edit target of its 
    /// owning stage.
    /// </summary>
    /// <param name="parent">The parent prim to flatten beneath</param>
    /// <param name="propName">The name for the flattened property</param>
    /// <returns>The flattened property</returns>
    /// <remarks>
    /// The parent prim may belong to a different stage than this property's 
    /// owning stage.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IUsdProperty FlattenTo(IUsdPrim parent, TfToken propName);

    /// <summary>
    /// Flattens this property to a property spec for the given
    /// property in the edit target of its owning prim's stage.
    /// </summary>
    /// <param name="property">The target property to flatten to</param>
    /// <returns>The flattened property</returns>
    /// <remarks>
    /// The property owning prim may belong to a different stage than this 
    /// property's owning stage.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_property.html#a1234567890abcdef
    /// </remarks>
    IUsdProperty FlattenTo(IUsdProperty property);

    #endregion
}