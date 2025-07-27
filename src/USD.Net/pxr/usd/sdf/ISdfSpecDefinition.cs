using Pxr.Base.Tf;

namespace Pxr.Usd.Sdf;

/// Interface for class representing fields and other information for a spec type.
/// </summary>
public interface ISdfSpecDefinition
{
    /// <summary>
    /// Returns all fields for this spec.
    /// </summary>
    /// <returns>Collection of field tokens</returns>
    IReadOnlyList<TfToken> GetFields();

    /// <summary>
    /// Returns all value fields marked as required for this spec.
    /// </summary>
    IReadOnlyList<TfToken> RequiredFields { get; }

    /// <summary>
    /// Returns all value fields marked as metadata for this spec.
    /// </summary>
    /// <returns>Collection of metadata field tokens</returns>
    IReadOnlyList<TfToken> GetMetadataFields();

    /// <summary>
    /// Returns whether the given field is valid for this spec.
    /// </summary>
    /// <param name="name">Field name to check</param>
    /// <returns>True if field is valid</returns>
    bool IsValidField(TfToken name);

    /// <summary>
    /// Returns whether the given field is metadata for this spec.
    /// </summary>
    /// <param name="name">Field name to check</param>
    /// <returns>True if field is metadata</returns>
    bool IsMetadataField(TfToken name);

    /// <summary>
    /// Returns the display group for this metadata field.
    /// Returns the empty token if this field is not a metadata field or if this
    /// metadata field has no display group.
    /// </summary>
    /// <param name="name">Field name to get display group for</param>
    /// <returns>Display group token or empty token</returns>
    TfToken GetMetadataFieldDisplayGroup(TfToken name);

    /// <summary>
    /// Returns whether the given field is required for this spec.
    /// </summary>
    /// <param name="name">Field name to check</param>
    /// <returns>True if field is required</returns>
    bool IsRequiredField(TfToken name);
}