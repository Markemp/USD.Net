using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Interface for UsdAttribute - scenegraph object for authoring and retrieving numeric, string, and array valued data.
/// </summary>
/// <remarks>
/// USD API compatible with OpenUSD 23.11+
/// See OpenUSD documentation: https://openusd.org/release/api/class_usd_attribute.html
/// 
/// Scenegraph object for authoring and retrieving numeric, string, and array
/// valued data, sampled over time, or animated by a spline.
/// 
/// The allowed value types for UsdAttribute are dictated by the Sdf
/// ("Scene Description Foundations") core's data model.
/// </remarks>
public interface IUsdAttribute : IUsdProperty
{
    #region Core Metadata

    /// <summary>
    /// An attribute's variability expresses whether it is intended to have
    /// time-samples or splines (SdfVariabilityVarying), or only a single 
    /// default value (SdfVariabilityUniform).
    /// </summary>
    /// <returns>The attribute's variability</returns>
    /// <remarks>
    /// Variability is required meta-data of all attributes, and its fallback
    /// value is SdfVariabilityVarying.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a79655ab3c82828093c685a1582d5e4bc
    /// </remarks>
    SdfVariability GetVariability();

    /// <summary>
    /// Set the value for variability at the current EditTarget, return true
    /// on success, false if the value can not be written.
    /// </summary>
    /// <param name="variability">The variability to set</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Note that this value should not be changed as it is typically either
    /// automatically authored or provided by a property definition. This method
    /// is provided primarily for fixing invalid scene description.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a5d5b05c4359aab959e717bad50f10aeb
    /// </remarks>
    bool SetVariability(SdfVariability variability);

    /// <summary>
    /// Return the "scene description" value type name for this attribute.
    /// </summary>
    /// <returns>The value type name</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a81ff07cc76cab8a0efd51b8938b9a124
    /// </remarks>
    SdfValueTypeName GetTypeName();

    /// <summary>
    /// Set the value for typeName at the current EditTarget, return true on
    /// success, false if the value can not be written.
    /// </summary>
    /// <param name="typeName">The type name to set</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Note that this value should not be changed as it is typically either
    /// automatically authored or provided by a property definition. This method
    /// is provided primarily for fixing invalid scene description.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a57aaffd881c108ddcc35537c2ec7e7d5
    /// </remarks>
    bool SetTypeName(SdfValueTypeName typeName);

    /// <summary>
    /// Return the roleName for this attribute's typeName.
    /// </summary>
    /// <returns>The role name token</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a1457c5b08b8c04e348e74842f2fa3e15
    /// </remarks>
    TfToken GetRoleName();

    #endregion

    #region Value & Time-Sample Accessors

    /// <summary>
    /// Populates a vector with authored sample times.
    /// Returns false only on error.
    /// </summary>
    /// <param name="times">Output array to receive the sorted, ascending timeSample ordinates</param>
    /// <returns>False only on error</returns>
    /// <remarks>
    /// This method uses the standard resolution semantics, so if a stronger
    /// default value is authored over weaker time samples, the default value
    /// will hide the underlying timesamples.
    /// 
    /// Note this function will query all value clips that may contribute 
    /// time samples for this attribute, opening them if needed. This may be
    /// expensive, especially if many clips are involved.
    /// 
    /// Any data in times will be lost, as this method clears times.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a0aba275933a77f28ab44b750964aa9a2
    /// </remarks>
    bool GetTimeSamples(out IReadOnlyList<double> times);

    /// <summary>
    /// Returns the number of time samples that have been authored.
    /// </summary>
    /// <returns>The number of authored time samples</returns>
    /// <remarks>
    /// This method uses the standard resolution semantics, so if a stronger
    /// default value is authored over weaker time samples, the default value
    /// will hide the underlying timesamples.
    /// 
    /// Note this function will query all value clips that may contribute 
    /// time samples for this attribute, opening them if needed. This may be
    /// expensive, especially if many clips are involved.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#adb2e41f8b3e68d9acbbb05918ee9fbd9
    /// </remarks>
    int GetNumTimeSamples();

    /// <summary>
    /// Populate lower and upper with the next greater and lesser
    /// value relative to the desiredTime. Return false if no value exists
    /// or an error occurs, true if either a default value or timeSamples exist.
    /// </summary>
    /// <param name="desiredTime">The time to bracket</param>
    /// <param name="lower">Output parameter for the lower bracketing time</param>
    /// <param name="upper">Output parameter for the upper bracketing time</param>
    /// <param name="hasTimeSamples">Output parameter indicating if time samples exist</param>
    /// <returns>True if successful, false on error</returns>
    /// <remarks>
    /// Use standard resolution semantics: if a stronger default value is
    /// authored over weaker time samples, the default value hides the
    /// underlying timeSamples.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a1f73bf9822e7300dcf4f009e07ae453f
    /// </remarks>
    bool GetBracketingTimeSamples(double desiredTime, out double lower, out double upper, out bool hasTimeSamples);

    /// <summary>
    /// Return true if this attribute has an authored default value, authored
    /// time samples or a fallback value provided by a registered schema.
    /// </summary>
    /// <returns>True if the attribute has a value</returns>
    /// <remarks>
    /// If the attribute has been blocked, then return true if and only if 
    /// it has a fallback value.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a7d33522bff62860c930f407afdada858
    /// </remarks>
    bool HasValue();

    /// <summary>
    /// Return true if this attribute has either an authored default value or
    /// authored time samples.
    /// </summary>
    /// <returns>True if the attribute has authored values</returns>
    /// <remarks>
    /// If the attribute has been blocked, then return false.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#af1fa2ef2a3852eb0b163183e7bbf1cb9
    /// </remarks>
    bool HasAuthoredValue();

    /// <summary>
    /// Return true if this attribute has a fallback value provided by 
    /// a registered schema.
    /// </summary>
    /// <returns>True if the attribute has a fallback value</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#ab2d1fe152bf22c9ca8fe1250ec9b77c0
    /// </remarks>
    bool HasFallbackValue();

    /// <summary>
    /// Return true if it is possible, but not certain, that this attribute's
    /// value changes over time, false otherwise.
    /// </summary>
    /// <returns>True if value might be time varying</returns>
    /// <remarks>
    /// If this function returns false, it is certain that this attribute's
    /// value remains constant over time.
    /// 
    /// This function checks if the attribute either has more than 1 time
    /// samples or is spline valued. Which is more efficient than actually
    /// counting the time samples or evaluating the spline, both of which
    /// are potentially expensive operations.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#adc2ce35114eb530bfa1d4b46c3d8fabc
    /// </remarks>
    bool ValueMightBeTimeVarying();

    /// <summary>
    /// Perform value resolution to fetch the value of this attribute at the
    /// requested UsdTimeCode time, which defaults to default.
    /// </summary>
    /// <param name="value">Output parameter to receive the resolved value</param>
    /// <param name="time">The time at which to sample the attribute</param>
    /// <returns>True if a value was successfully retrieved</returns>
    /// <remarks>
    /// If no value is authored at time but values are authored at other
    /// times, this function will return an interpolated value based on the 
    /// stage's interpolation type.
    /// 
    /// If no value is authored and no fallback value is provided by the 
    /// schema for this attribute, this function will return false.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a9d41bc223be86408ba7d7f74df7c35a9
    /// </remarks>
    bool Get(out VtValue value, UsdTimeCode time = default);

    /// <summary>
    /// Set the value of this attribute in the current UsdEditTarget to
    /// value at UsdTimeCode time, which defaults to default.
    /// </summary>
    /// <param name="value">The value to set</param>
    /// <param name="time">The time at which to author the value</param>
    /// <returns>True if the value was successfully set</returns>
    /// <remarks>
    /// Values are authored without regard to this attribute's variability. 
    /// For example, time sample values may be authored on a uniform
    /// attribute. However, the USD_VALIDATE_VARIABILITY TF_DEBUG code
    /// will cause debug information to be output if values that are
    /// inconsistent with this attribute's variability are authored.
    /// 
    /// Return false and generate an error if the time is pre-time, which is 
    /// only used for querying for values at the limit when the time is 
    /// approached from the left.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a5de2a2a51debc1f3c8f3e671a5776743
    /// </remarks>
    bool Set(VtValue value, UsdTimeCode time = default);

    /// <summary>
    /// Clears the authored default value, all time samples and spline for this
    /// attribute at the current EditTarget and returns true on success.
    /// </summary>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Calling clear when either no value is authored or no spec is present,
    /// is a silent no-op returning true.
    /// 
    /// This method does not affect any other data authored on this attribute.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a4335db09e6deaf5ab80340283b23059f
    /// </remarks>
    bool Clear();

    /// <summary>
    /// Clear the authored value for this attribute at the given 
    /// time, at the current EditTarget and return true on success.
    /// </summary>
    /// <param name="time">The time at which to clear the value</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// UsdTimeCode::Default() can be used to clear the default value.
    /// 
    /// Calling clear when either no value is authored or no spec is present,
    /// is a silent no-op returning true.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a9b493f0e1ce88dde2a17d16889997dd0
    /// </remarks>
    bool ClearAtTime(UsdTimeCode time);

    /// <summary>
    /// Shorthand for ClearAtTime(UsdTimeCode::Default()).
    /// </summary>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a3c1639b65058c3cde3ae86be53c7cbf1
    /// </remarks>
    bool ClearDefault();

    /// <summary>
    /// Remove all time samples on an attribute and author a block
    /// default value. This causes the attribute to resolve as 
    /// if there were no authored value opinions in weaker layers.
    /// </summary>
    /// <remarks>
    /// See Usd_AttributeBlocking for more information, including
    /// information on time-varying blocking.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a88e75995833fc8cb897d9ed123c3f3e0
    /// </remarks>
    void Block();

    #endregion

    #region Querying and Editing Connections

    /// <summary>
    /// Adds source to the list of connections, in the position
    /// specified by position.
    /// </summary>
    /// <param name="source">The source path to connect</param>
    /// <param name="position">The position where to add the connection</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Issue an error if source identifies a prototype prim or an object
    /// descendant to a prototype prim. It is not valid to author connections
    /// to these objects.
    /// 
    /// What data this actually authors depends on what data is currently
    /// authored in the authoring layer, with respect to list-editing
    /// semantics.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a7f1e7f8d2b691da3447903fb60bc8b73
    /// </remarks>
    bool AddConnection(ISdfPath source, UsdListPosition position = UsdListPosition.BackOfPrependList);

    /// <summary>
    /// Removes target from the list of targets.
    /// </summary>
    /// <param name="source">The source path to remove</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Issue an error if source identifies a prototype prim or an object
    /// descendant to a prototype prim. It is not valid to author connections
    /// to these objects.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a659302856df8727e408ae94896b1e137
    /// </remarks>
    bool RemoveConnection(ISdfPath source);

    /// <summary>
    /// Make the authoring layer's opinion of the connection list explicit,
    /// and set exactly to sources.
    /// </summary>
    /// <param name="sources">The exact list of source connections to set</param>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// Issue an error if source identifies a prototype prim or an object
    /// descendant to a prototype prim. It is not valid to author connections
    /// to these objects.
    /// 
    /// If any path in sources is invalid, issue an error and return false.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a744a17d080ea0257a1a59c8a0054cf68
    /// </remarks>
    bool SetConnections(IEnumerable<ISdfPath> sources);

    /// <summary>
    /// Remove all opinions about the connections list from the current edit
    /// target.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a7680db2c045d82deba23afcd50d15766
    /// </remarks>
    bool ClearConnections();

    /// <summary>
    /// Compose this attribute's connections and fill sources with the
    /// result.
    /// </summary>
    /// <param name="sources">Output parameter to receive the composed connections</param>
    /// <returns>True if any connection path opinions have been authored and no
    /// composition errors were encountered</returns>
    /// <remarks>
    /// Returns true if any connection path opinions have been authored and no
    /// composition errors were encountered, returns false otherwise. 
    /// Note that authored opinions may include opinions that clear the 
    /// connections and a return value of true does not necessarily indicate 
    /// that sources will contain any connection paths.
    /// 
    /// The result is not cached, and thus recomputed on each query.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#ab6384fe8ac90bf18c8d781e139a7d813
    /// </remarks>
    bool GetConnections(out IReadOnlyList<ISdfPath> sources);

    /// <summary>
    /// Return true if this attribute has any authored opinions regarding
    /// connections.
    /// </summary>
    /// <returns>True if connections are authored</returns>
    /// <remarks>
    /// Note that this includes opinions that remove connections,
    /// so a true return does not necessarily indicate that this attribute has
    /// connections.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a828cbae7d4d6c6f9f220307b6b81d5cf
    /// </remarks>
    bool HasAuthoredConnections();

    #endregion

    #region ColorSpace API

    /// <summary>
    /// Gets the color space in which the attribute is authored if it has been
    /// explicitly set.
    /// </summary>
    /// <returns>The color space token</returns>
    /// <remarks>
    /// If the color space is not authored, any color space
    /// set on the attribute's prim definition will be returned.
    /// Use UsdColorSpaceAPI in order to compute the color space taking
    /// into account any inherited color spaces.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a635e3fc6927db14e49302525a354bbf2
    /// </remarks>
    TfToken GetColorSpace();

    /// <summary>
    /// Sets the color space of the attribute to colorSpace.
    /// </summary>
    /// <param name="colorSpace">The target color space for this attribute</param>
    /// <remarks>
    /// UsdColorSpaceAPI provides methods to compute an attribute's resolved 
    /// color, considering any inherited colorspaces. Standard color space names 
    /// are listed in GfColorSpaceNames.
    /// 
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#ab7003182fc7e31d5ee5be80ee518edbe
    /// </remarks>
    void SetColorSpace(TfToken colorSpace);

    /// <summary>
    /// Returns whether color space is authored on the attribute.
    /// </summary>
    /// <returns>True if color space is authored</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#aadd967940959ca70d159789141db49a1
    /// </remarks>
    bool HasColorSpace();

    /// <summary>
    /// Clears authored color space value on the attribute.
    /// </summary>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// See OpenUSD documentation: 
    /// https://openusd.org/release/api/class_usd_attribute.html#a8816850ccdb05117eb34eae01630219c
    /// </remarks>
    bool ClearColorSpace();

    #endregion
}