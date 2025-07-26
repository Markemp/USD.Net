namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

using FileFormatArguments = Dictionary<string, string>;

// AIDEV-NOTE: Interface based on OpenUSD layer.h - DO NOT MODIFY WITHOUT PERMISSION
/// <summary>
/// Interface for a scene description layer.
/// </summary>
/// <remarks>
/// A layer is a container for scene description that can be serialized to and from disk.
/// It provides the backing storage for specs and fields.
/// </remarks>
public interface ISdfLayer
{
    #region Schema and File Format

    /// <summary>
    /// Returns the schema that this layer adheres to.
    /// </summary>
    ISdfSchemaBase GetSchema();

    /// <summary>
    /// Returns the file format used by this layer.
    /// </summary>
    ISdfFileFormat? GetFileFormat();

    /// <summary>
    /// Returns the file format-specific arguments used during the construction of this layer.
    /// </summary>
    FileFormatArguments GetFileFormatArguments();

    #endregion

    #region File I/O

    /// <summary>
    /// Saves the layer to its current location.
    /// </summary>
    /// <param name="force">If true, saves even if the layer is not dirty or if the file exists.</param>
    /// <returns>true if successful, false if an error occurred.</returns>
    /// <remarks>
    /// Returns false if the layer has no remembered file name or the layer type cannot be saved.
    /// The layer will not be overwritten if the file exists and the layer is not dirty unless force is true.
    /// </remarks>
    bool Save(bool force = false);

    /// <summary>
    /// Exports this layer to a file.
    /// </summary>
    /// <param name="filename">The file path to export to.</param>
    /// <param name="comment">Optional comment to include in the exported file.</param>
    /// <param name="args">Additional file format-specific arguments.</param>
    /// <returns>true if successful, false if an error occurred.</returns>
    /// <remarks>
    /// Note that the file name or comment of the original layer is not updated.
    /// This only saves a copy of the layer to the given filename.
    /// Subsequent calls to Save() will still save the layer to its previously remembered file name.
    /// </remarks>
    bool Export(string filename, string comment = "", FileFormatArguments? args = null);

    /// <summary>
    /// Writes this layer to the given string.
    /// </summary>
    /// <param name="result">The string to write the layer contents to.</param>
    /// <returns>true if successful and sets result, otherwise returns false.</returns>
    bool ExportToString(out string result);

    /// <summary>
    /// Reads this layer from the given string.
    /// </summary>
    /// <param name="str">The string containing layer contents.</param>
    /// <returns>true if successful, otherwise returns false.</returns>
    bool ImportFromString(string str);

    /// <summary>
    /// Clears the layer of all content.
    /// </summary>
    /// <remarks>
    /// This restores the layer to a state as if it had just been created with CreateNew().
    /// This operation is Undo-able.
    /// The fileName and whether journaling is enabled are not affected by this method.
    /// </remarks>
    void Clear();

    /// <summary>
    /// Reloads the layer from its persistent representation.
    /// </summary>
    /// <param name="force">If true, forces reload regardless of whether the file has changed.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// This restores the layer to a state as if it had just been created with FindOrOpen().
    /// This operation is Undo-able.
    /// When called with force = false (the default), Reload attempts to avoid reloading layers
    /// that have not changed on disk by comparing the file's modification time.
    /// </remarks>
    bool Reload(bool force = false);

    /// <summary>
    /// Imports the content of the given layer path, replacing the content of the current layer.
    /// </summary>
    /// <param name="layerPath">The path to the layer to import.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// Note: If the layer path is the same as the current layer's real path,
    /// no action is taken (and a warning occurs). For this case use Reload().
    /// </remarks>
    bool Import(string layerPath);

    #endregion

    #region Layer State

    /// <summary>
    /// Returns whether this layer has no significant data.
    /// </summary>
    bool IsEmpty();

    /// <summary>
    /// Returns true if this layer streams data from its serialized data store on demand, false otherwise.
    /// </summary>
    /// <remarks>
    /// Layers with streaming data are treated differently to avoid pulling in data unnecessarily.
    /// For example, reloading a streaming layer will not perform fine-grained change notification,
    /// since doing so would require the full contents of the layer to be loaded.
    /// </remarks>
    bool StreamsData();

    /// <summary>
    /// Returns true if this layer is detached from its serialized data store, false otherwise.
    /// </summary>
    /// <remarks>
    /// Detached layers are isolated from external changes to their serialized data.
    /// </remarks>
    bool IsDetached();

    /// <summary>
    /// Returns true if this layer is an anonymous layer.
    /// </summary>
    bool IsAnonymous();

    /// <summary>
    /// Returns true if there are unsaved edits to this layer.
    /// </summary>
    bool IsDirty();

    /// <summary>
    /// Returns true if this layer is muted. Muted layers are ignored by
    /// composition and do not appear in any composition results.
    /// </summary>
    bool IsMuted();

    /// <summary>
    /// Mutes or unmutes the current layer.
    /// </summary>
    void SetMuted(bool muted);

    /// <summary>
    /// Returns true if this layer can be saved to disk.
    /// </summary>
    bool PermissionToSave();

    /// <summary>
    /// Returns true if this layer can be edited.
    /// </summary>
    bool PermissionToEdit();

    #endregion

    #region Identification

    /// <summary>
    /// Returns the layer identifier.
    /// </summary>
    /// <remarks>
    /// A layer's identifier is a string that uniquely identifies a layer.
    /// At minimum, it is the string by which the layer was created.
    /// If additional arguments were passed to creation functions, those arguments
    /// will be encoded in the identifier.
    /// </remarks>
    string GetIdentifier();

    /// <summary>
    /// Sets the layer identifier.
    /// </summary>
    /// <param name="identifier">The new identifier.</param>
    /// <remarks>
    /// Note that the new identifier must have the same arguments (if any) as the old identifier.
    /// </remarks>
    void SetIdentifier(string identifier);

    /// <summary>
    /// Update layer asset information.
    /// </summary>
    /// <remarks>
    /// Calling this method re-resolves the layer identifier, which updates asset information
    /// such as the layer's resolved path and other asset info. This may be used to update
    /// the layer after external changes to the underlying asset system.
    /// </remarks>
    void UpdateAssetInfo();

    /// <summary>
    /// Returns the layer's display name.
    /// </summary>
    /// <remarks>
    /// The display name is the base filename of the identifier.
    /// </remarks>
    string GetDisplayName();

    /// <summary>
    /// Returns the resolved path for this layer.
    /// </summary>
    /// <remarks>
    /// This is the path where this layer exists or may exist after a call to Save().
    /// </remarks>
    string GetRealPath();

    /// <summary>
    /// Returns the file extension to use for this layer.
    /// </summary>
    /// <remarks>
    /// If this layer was loaded from disk, it should match the extension of the file format
    /// it was loaded as; if this is an anonymous in-memory layer it will be the default extension.
    /// </remarks>
    string GetFileExtension();

    /// <summary>
    /// Returns the asset system version of this layer.
    /// </summary>
    /// <remarks>
    /// If a layer is loaded from a location that is not version managed, or a configured
    /// asset system is not present when the layer is loaded or created, the version is empty.
    /// </remarks>
    string GetVersion();

    /// <summary>
    /// Returns the layer identifier in asset path form.
    /// </summary>
    /// <remarks>
    /// In the presence of a properly configured path resolver, the asset path is a
    /// double-slash prefixed depot path. If the path resolver is not configured,
    /// the asset path of a layer is empty.
    /// </remarks>
    string GetRepositoryPath();

    /// <summary>
    /// Returns the asset name associated with this layer.
    /// </summary>
    string GetAssetName();

    /// <summary>
    /// Returns resolve information from the last time the layer identifier was resolved.
    /// </summary>
    VtValue GetAssetInfo();

    /// <summary>
    /// Returns the path to the asset specified by assetPath using this layer to anchor
    /// the path if necessary.
    /// </summary>
    /// <param name="assetPath">The asset path to compute an absolute path for.</param>
    /// <returns>The absolute path to the asset.</returns>
    /// <remarks>
    /// Returns assetPath if it's empty or an anonymous layer identifier.
    /// The returned path should in general not be assumed to be an absolute filesystem path
    /// or any other specific form. It is "absolute" in that it should resolve to the same
    /// asset regardless of what layer it's authored in.
    /// </remarks>
    string ComputeAbsolutePath(string assetPath);

    #endregion

    #region Fields

    /// <summary>
    /// Return the spec type for path.
    /// </summary>
    /// <param name="path">The path to query.</param>
    /// <returns>The spec type, or SdfSpecTypeUnknown if no spec exists at path.</returns>
    SdfSpecType GetSpecType(SdfPath path);

    /// <summary>
    /// Return whether a spec exists at path.
    /// </summary>
    bool HasSpec(SdfPath path);

    /// <summary>
    /// Return the names of all the fields that are set at path.
    /// </summary>
    IReadOnlyList<TfToken> ListFields(SdfPath path);

    /// <summary>
    /// Return whether a value exists for the given path and fieldName.
    /// </summary>
    /// <param name="path">The path to the spec.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">Optional output parameter for the value if it exists.</param>
    /// <returns>true if a value exists, false otherwise.</returns>
    bool HasField(SdfPath path, TfToken fieldName, out VtValue? value);

    /// <summary>
    /// Return whether a value exists for the given path and fieldName.
    /// </summary>
    bool HasField(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Returns true if the object has a non-empty value with name and type T.
    /// </summary>
    bool HasField<T>(SdfPath path, TfToken fieldName, out T? value);

    /// <summary>
    /// Return whether a value exists for the given path, fieldName and keyPath.
    /// </summary>
    /// <param name="path">The path to the spec.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="keyPath">A ':'-separated path addressing an element in sub-dictionaries.</param>
    /// <param name="value">Optional output parameter for the value if it exists.</param>
    /// <returns>true if a value exists, false otherwise.</returns>
    bool HasFieldDictKey(SdfPath path, TfToken fieldName, TfToken keyPath, out VtValue? value);

    /// <summary>
    /// Return whether a value exists for the given path, fieldName and keyPath.
    /// </summary>
    bool HasFieldDictKey(SdfPath path, TfToken fieldName, TfToken keyPath);

    /// <summary>
    /// Returns true if the object has a non-empty value with name, keyPath and type T.
    /// </summary>
    bool HasFieldDictKey<T>(SdfPath path, TfToken fieldName, TfToken keyPath, out T? value);

    /// <summary>
    /// Return the value for the given path and fieldName.
    /// </summary>
    /// <returns>The field value, or an empty VtValue if none is set.</returns>
    VtValue GetField(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Return the value for the given path and fieldName.
    /// </summary>
    /// <param name="defaultValue">The value to return if the field is not set.</param>
    /// <returns>The field value, or defaultValue if none is set.</returns>
    /// <remarks>
    /// For reference types, if no defaultValue is provided, default(T) will be null.
    /// For value types, if no defaultValue is provided, default(T) will be the zero value.
    /// </remarks>
    T GetFieldAs<T>(SdfPath path, TfToken fieldName, T defaultValue = default!);

    /// <summary>
    /// Return the value for the given path and fieldName at keyPath.
    /// </summary>
    /// <param name="keyPath">A ':'-separated path addressing an element in sub-dictionaries.</param>
    /// <returns>The field value, or an empty VtValue if none is set.</returns>
    VtValue GetFieldDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath);

    /// <summary>
    /// Set the value of the given path and fieldName.
    /// </summary>
    void SetField(SdfPath path, TfToken fieldName, VtValue value);

    /// <summary>
    /// Set the value of the given path and fieldName.
    /// </summary>
    void SetField<T>(SdfPath path, TfToken fieldName, T value);

    /// <summary>
    /// Set the value of the given path and fieldName at keyPath.
    /// </summary>
    /// <param name="keyPath">A ':'-separated path addressing an element in sub-dictionaries.</param>
    void SetFieldDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath, VtValue value);

    /// <summary>
    /// Set the value of the given path and fieldName at keyPath.
    /// </summary>
    void SetFieldDictValueByKey<T>(SdfPath path, TfToken fieldName, TfToken keyPath, T value);

    /// <summary>
    /// Remove the field at path and fieldName, if one exists.
    /// </summary>
    void EraseField(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Remove the field at path, fieldName and keyPath, if one exists.
    /// </summary>
    /// <param name="keyPath">A ':'-separated path addressing an element in sub-dictionaries.</param>
    void EraseFieldDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath);

    #endregion

    #region Traversal

    /// <summary>
    /// Traverse the scene description hierarchy rooted at path, calling func on each spec found.
    /// </summary>
    /// <param name="path">The root path to begin traversal.</param>
    /// <param name="func">The function to call for each spec.</param>
    void Traverse(SdfPath path, Action<ISdfPath> func);

    #endregion

    #region Metadata

    /// <summary>
    /// Returns the color configuration asset-path for this layer.
    /// </summary>
    /// <remarks>
    /// The default value is an empty asset-path.
    /// </remarks>
    SdfAssetPath GetColorConfiguration();

    /// <summary>
    /// Sets the color configuration asset-path for this layer.
    /// </summary>
    void SetColorConfiguration(SdfAssetPath colorConfiguration);

    /// <summary>
    /// Returns true if color configuration metadata is set in this layer.
    /// </summary>
    bool HasColorConfiguration();

    /// <summary>
    /// Clears the color configuration metadata authored in this layer.
    /// </summary>
    void ClearColorConfiguration();

    /// <summary>
    /// Returns the color management system used to interpret the color configuration
    /// asset-path authored in this layer.
    /// </summary>
    /// <remarks>
    /// The default value is an empty token, which implies that the clients will have to
    /// determine the color management system from the color configuration asset path
    /// (i.e. from its file extension), if it's specified.
    /// </remarks>
    TfToken GetColorManagementSystem();

    /// <summary>
    /// Sets the color management system used to interpret the color configuration
    /// asset-path authored this layer.
    /// </summary>
    void SetColorManagementSystem(TfToken cms);

    /// <summary>
    /// Returns true if colorManagementSystem metadata is set in this layer.
    /// </summary>
    bool HasColorManagementSystem();

    /// <summary>
    /// Clears the 'colorManagementSystem' metadata authored in this layer.
    /// </summary>
    void ClearColorManagementSystem();

    /// <summary>
    /// Returns the comment string for this layer.
    /// </summary>
    /// <remarks>
    /// The default value for comment is "".
    /// </remarks>
    string GetComment();

    /// <summary>
    /// Sets the comment string for this layer.
    /// </summary>
    void SetComment(string comment);

    /// <summary>
    /// Return the defaultPrim metadata for this layer.
    /// </summary>
    /// <remarks>
    /// This field indicates the name or path of which prim should be targeted by a
    /// reference or payload to this layer that doesn't specify a prim path.
    /// The default value is the empty token.
    /// </remarks>
    TfToken GetDefaultPrim();

    /// <summary>
    /// Return this layer's default prim metadata interpreted as an absolute prim path
    /// regardless of whether it was authored as a root prim name or a prim path.
    /// </summary>
    /// <remarks>
    /// For example, if the authored default prim value is "rootPrim", return &lt;/rootPrim&gt;.
    /// If the authored default prim value is "/path/to/non/root/prim", return &lt;/path/to/non/root/prim&gt;.
    /// If the authored default prim value cannot be interpreted as a prim path, return the empty SdfPath.
    /// The default value is an empty path.
    /// </remarks>
    SdfPath GetDefaultPrimAsPath();

    /// <summary>
    /// Set the default prim metadata for this layer.
    /// </summary>
    /// <param name="name">The prim name or path.</param>
    /// <remarks>
    /// The prim at this path will be targeted by a reference or a payload to this layer
    /// that doesn't specify a prim path. Note that this can be a name if it refers to
    /// a root prim, or a path to any prim in this layer.
    /// </remarks>
    void SetDefaultPrim(TfToken name);

    /// <summary>
    /// Clear the default prim metadata for this layer.
    /// </summary>
    void ClearDefaultPrim();

    /// <summary>
    /// Return true if the default prim metadata is set in this layer.
    /// </summary>
    bool HasDefaultPrim();

    /// <summary>
    /// Returns the documentation string for this layer.
    /// </summary>
    /// <remarks>
    /// The default value for documentation is "".
    /// </remarks>
    string GetDocumentation();

    /// <summary>
    /// Sets the documentation string for this layer.
    /// </summary>
    void SetDocumentation(string documentation);

    /// <summary>
    /// Returns the layer's start timeCode.
    /// </summary>
    /// <remarks>
    /// The start and end timeCodes of a layer represent the suggested playback range.
    /// However, time-varying content is not limited to the timeCode range of the layer.
    /// The default value for startTimeCode is 0.
    /// </remarks>
    double GetStartTimeCode();

    /// <summary>
    /// Sets the layer's start timeCode.
    /// </summary>
    void SetStartTimeCode(double startTimeCode);

    /// <summary>
    /// Returns true if the layer has a startTimeCode opinion.
    /// </summary>
    bool HasStartTimeCode();

    /// <summary>
    /// Clear the startTimeCode opinion.
    /// </summary>
    void ClearStartTimeCode();

    /// <summary>
    /// Returns the layer's end timeCode.
    /// </summary>
    /// <remarks>
    /// The start and end timeCodes of a layer represent the suggested playback range.
    /// However, time-varying content is not limited to the timeCode range of the layer.
    /// The default value for endTimeCode is 0.
    /// </remarks>
    double GetEndTimeCode();

    /// <summary>
    /// Sets the layer's end timeCode.
    /// </summary>
    void SetEndTimeCode(double endTimeCode);

    /// <summary>
    /// Returns true if the layer has an endTimeCode opinion.
    /// </summary>
    bool HasEndTimeCode();

    /// <summary>
    /// Clear the endTimeCode opinion.
    /// </summary>
    void ClearEndTimeCode();

    /// <summary>
    /// Returns the layer's time codes per second.
    /// </summary>
    /// <remarks>
    /// This is the unit for timeline values in the layer. 
    /// Scales the time ordinate for samples contained in the file to seconds.
    /// If timeCodesPerSecond is 24, then a sample at time ordinate 24 should be viewed at second 1.
    /// The default value for timeCodesPerSecond is 24.
    /// </remarks>
    double GetTimeCodesPerSecond();

    /// <summary>
    /// Sets the layer's time codes per second.
    /// </summary>
    void SetTimeCodesPerSecond(double timeCodesPerSecond);

    /// <summary>
    /// Returns true if the layer has a timeCodesPerSecond opinion.
    /// </summary>
    bool HasTimeCodesPerSecond();

    /// <summary>
    /// Clear the timeCodesPerSecond opinion.
    /// </summary>
    void ClearTimeCodesPerSecond();

    /// <summary>
    /// Returns the layer's frames per second.
    /// </summary>
    /// <remarks>
    /// This is purely informational to the user about the target playback rate,
    /// and has no impact on run-time behavior. The default value for framesPerSecond is 24.
    /// </remarks>
    double GetFramesPerSecond();

    /// <summary>
    /// Sets the layer's frames per second.
    /// </summary>
    void SetFramesPerSecond(double framesPerSecond);

    /// <summary>
    /// Returns true if the layer has a frames per second opinion.
    /// </summary>
    bool HasFramesPerSecond();

    /// <summary>
    /// Clear the framesPerSecond opinion.
    /// </summary>
    void ClearFramesPerSecond();

    /// <summary>
    /// Returns the layer's frame precision.
    /// </summary>
    int GetFramePrecision();

    /// <summary>
    /// Sets the layer's frame precision.
    /// </summary>
    void SetFramePrecision(int framePrecision);

    /// <summary>
    /// Returns true if the layer has a frames precision opinion.
    /// </summary>
    bool HasFramePrecision();

    /// <summary>
    /// Clear the framePrecision opinion.
    /// </summary>
    void ClearFramePrecision();

    /// <summary>
    /// Returns the owner of this layer.
    /// </summary>
    string GetOwner();

    /// <summary>
    /// Sets the owner of this layer.
    /// </summary>
    void SetOwner(string owner);

    /// <summary>
    /// Returns true if the layer has an owner opinion.
    /// </summary>
    bool HasOwner();

    /// <summary>
    /// Clear the owner opinion.
    /// </summary>
    void ClearOwner();

    /// <summary>
    /// Returns the session owner of this layer.
    /// </summary>
    string GetSessionOwner();

    /// <summary>
    /// Sets the session owner of this layer.
    /// </summary>
    void SetSessionOwner(string sessionOwner);

    /// <summary>
    /// Returns true if the layer has a session owner opinion.
    /// </summary>
    bool HasSessionOwner();

    /// <summary>
    /// Clear the session owner opinion.
    /// </summary>
    void ClearSessionOwner();

    /// <summary>
    /// Returns the CustomLayerData dictionary associated with this layer.
    /// </summary>
    VtDictionary GetCustomLayerData();

    /// <summary>
    /// Sets the CustomLayerData dictionary associated with this layer.
    /// </summary>
    void SetCustomLayerData(VtDictionary value);

    /// <summary>
    /// Returns true if the layer has CustomLayerData.
    /// </summary>
    bool HasCustomLayerData();

    /// <summary>
    /// Clear the CustomLayerData opinion.
    /// </summary>
    void ClearCustomLayerData();

    #endregion

    #region Prims

    /// <summary>
    /// Returns the root prims.
    /// </summary>
    SdfPrimSpecHandleVector GetRootPrims();

    /// <summary>
    /// Sets a new list of root prims.
    /// </summary>
    void SetRootPrims(SdfPrimSpecHandleVector rootPrims);

    /// <summary>
    /// Adds a new root prim at the given path.
    /// </summary>
    void InsertRootPrim(SdfPrimSpec prim, int index = -1);

    /// <summary>
    /// Remove a root prim.
    /// </summary>
    void RemoveRootPrim(SdfPrimSpec prim);

    /// <summary>
    /// Returns the layer's root prim order metadata.
    /// </summary>
    /// <remarks>
    /// This list of prim names is used to reorder the layer's root prims.
    /// It's stored in the optional SdfFieldKeys.PrimOrder field.
    /// </remarks>
    SdfNameOrderProxy GetRootPrimOrder();

    /// <summary>
    /// Sets the layer's root prim order metadata.
    /// </summary>
    void SetRootPrimOrder(IEnumerable<TfToken> names);

    /// <summary>
    /// Adds a new root prim order entry.
    /// </summary>
    void InsertInRootPrimOrder(TfToken name, int index = -1);

    /// <summary>
    /// Removes a root prim name from the root prim order.
    /// </summary>
    void RemoveFromRootPrimOrder(TfToken name);

    /// <summary>
    /// Removes a root prim name from the root prim order by index.
    /// </summary>
    void RemoveFromRootPrimOrderByIndex(int index);

    /// <summary>
    /// Reorders the given list of prim names according to the reorder rootPrims statement.
    /// </summary>
    void ApplyRootPrimOrder(ref List<TfToken> vec);

    /// <summary>
    /// Returns the list of prim names for this layer's reorder rootPrims statement.
    /// </summary>
    /// <returns>true if this layer has an explicit list of reorder rootPrims, false otherwise.</returns>
    bool CanApplyRootPrimOrder(List<TfToken> vec);

    #endregion

    #region Sublayers

    /// <summary>
    /// Returns the paths to the sublayers on this layer.
    /// </summary>
    SdfSubLayerProxy GetSubLayerPaths();

    /// <summary>
    /// Sets the paths to the sublayers.
    /// </summary>
    void SetSubLayerPaths(IEnumerable<string> newPaths);

    /// <summary>
    /// Returns the number of sublayer paths (and offsets).
    /// </summary>
    int GetNumSubLayerPaths();

    /// <summary>
    /// Inserts new sublayer path at the given index.
    /// </summary>
    void InsertSubLayerPath(string path, int index = -1);

    /// <summary>
    /// Removes sublayer path at the given index.
    /// </summary>
    void RemoveSubLayerPath(int index);

    /// <summary>
    /// Returns the layer offsets for all sublayers.
    /// </summary>
    SdfLayerOffsetVector GetSubLayerOffsets();

    /// <summary>
    /// Returns the layer offset for the sublayer at the given index.
    /// </summary>
    SdfLayerOffset GetSubLayerOffset(int index);

    /// <summary>
    /// Sets the layer offset for the sublayer at the given index.
    /// </summary>
    void SetSubLayerOffset(SdfLayerOffset offset, int index);

    #endregion

    #region Muting

    /// <summary>
    /// Returns the set of muted layer paths.
    /// </summary>
    ISet<string> GetMutedLayers();

    /// <summary>
    /// Returns true if the sublayer with the given path is muted in this layer.
    /// </summary>
    bool IsSubLayerMuted(string path);

    /// <summary>
    /// Mutes or unmutes the sublayer with the given path.
    /// </summary>
    void SetSubLayerMuted(string path, bool muted);

    #endregion

    #region Relocates

    /// <summary>
    /// Returns a map of relocates.
    /// </summary>
    SdfRelocatesMapProxy GetRelocates();

    /// <summary>
    /// Set the entire map of namespace relocations specified on this layer.
    /// </summary>
    void SetRelocates(SdfRelocatesMap relocatesMap);

    /// <summary>
    /// Returns true if this layer's metadata has relocates opinion.
    /// </summary>
    bool HasRelocates();

    /// <summary>
    /// Clears the layer's relocates opinion.
    /// </summary>
    void ClearRelocates();

    #endregion

    #region Time Sample API

    /// <summary>
    /// Returns the number of time sample times for a given path and field.
    /// </summary>
    int GetNumTimeSamplesForPath(SdfPath path);

    /// <summary>
    /// Returns all the time sample times for a given path and field.
    /// </summary>
    ISet<double> ListAllTimeSamples();

    /// <summary>
    /// Returns all the time sample times for a given path and field.
    /// </summary>
    ISet<double> ListTimeSamplesForPath(SdfPath path);

    /// <summary>
    /// Queries the time sample value at the specified path and time.
    /// </summary>
    bool QueryTimeSample(SdfPath path, double time, out VtValue value);

    /// <summary>
    /// Queries the time sample value at the specified path and time.
    /// </summary>
    bool QueryTimeSample<T>(SdfPath path, double time, out T value);

    /// <summary>
    /// Sets a time sample value.
    /// </summary>
    void SetTimeSample(SdfPath path, double time, VtValue value);

    /// <summary>
    /// Sets a time sample value.
    /// </summary>
    void SetTimeSample<T>(SdfPath path, double time, T value);

    /// <summary>
    /// Removes a time sample at the given time.
    /// </summary>
    void EraseTimeSample(SdfPath path, double time);

    #endregion

    #region Composition

    /// <summary>
    /// Return paths of all assets this layer depends on due to composition fields.
    /// </summary>
    /// <remarks>
    /// This includes the paths of all layers referred to by reference, payload, and sublayer
    /// fields in this layer. This function only returns direct composition dependencies of
    /// this layer, i.e. it does not recurse to find composition dependencies from its
    /// dependent layer assets.
    /// </remarks>
    ISet<string> GetCompositionAssetDependencies();

    /// <summary>
    /// Updates the asset path of a composition dependency in this layer.
    /// </summary>
    /// <param name="oldAssetPath">The old asset path to update.</param>
    /// <param name="newAssetPath">The new asset path, or empty to remove.</param>
    /// <returns>true if successful, false otherwise.</returns>
    /// <remarks>
    /// If newAssetPath is supplied, the update works as "rename", updating any occurrence
    /// of oldAssetPath to newAssetPath in all reference, payload, and sublayer fields.
    /// If newAssetPath is not given, this update behaves as a "delete", removing all
    /// occurrences of oldAssetPath from all reference, payload, and sublayer fields.
    /// </remarks>
    bool UpdateCompositionAssetDependency(string oldAssetPath, string newAssetPath = "");

    /// <summary>
    /// Returns a set of resolved paths to all external asset dependencies the layer
    /// needs to generate its contents.
    /// </summary>
    /// <remarks>
    /// These are additional asset dependencies that are determined by the layer's file format
    /// and will be consulted during Reload() when determining if the layer needs to be reloaded.
    /// This specifically does not include dependencies related to composition, i.e. this will
    /// not include assets from references, payloads, and sublayers.
    /// </remarks>
    ISet<string> GetExternalAssetDependencies();

    #endregion

    #region Batch namespace editing

    /// <summary>
    /// Check if a batch of namespace edits will succeed.
    /// </summary>
    SdfNamespaceEditDetail.Result CanApply(SdfBatchNamespaceEdit edit);

    /// <summary>
    /// Performs a batch of namespace edits.
    /// </summary>
    bool Apply(SdfBatchNamespaceEdit edit);

    #endregion

    #region Debugging

    /// <summary>
    /// Writes the layer contents to standard output or a file for debugging purposes.
    /// </summary>
    /// <param name="filename">Optional filename to write to. If empty, writes to stdout.</param>
    void DumpLayerInfo(string filename = "");

    #endregion
}