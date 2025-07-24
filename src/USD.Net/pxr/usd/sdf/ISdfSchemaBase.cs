namespace Pxr.Usd.Sdf;

using System;
using System.Collections.Generic;
using Pxr.Base.Tf;
using Pxr.Base.Vt;

// AIDEV-NOTE: Interface based on OpenUSD SdfSchemaBase - DO NOT MODIFY WITHOUT PERMISSION
/// <summary>
/// Interface for generic class that provides information about scene description fields
/// but doesn't actually provide any fields.
/// </summary>
public interface ISdfSchemaBase
{
    /// <summary>
    /// Returns the field definition for the given field.
    /// Returns null if no definition exists for given field.
    /// </summary>
    /// <param name="fieldKey">The field key to look up</param>
    /// <returns>Field definition or null if not found</returns>
    IFieldDefinition? GetFieldDefinition(TfToken fieldKey);

    /// <summary>
    /// Returns the spec definition for the given spec type.
    /// Returns null if no definition exists for the given spec type.
    /// </summary>
    /// <param name="specType">The spec type to look up</param>
    /// <returns>Spec definition or null if not found</returns>
    ISpecDefinition? GetSpecDefinition(SdfSpecType specType);

    /// <summary>
    /// Return whether the specified field has been registered.
    /// </summary>
    /// <param name="fieldKey">Field to check</param>
    /// <param name="fallback">Optional fallback value output</param>
    /// <returns>True if field is registered</returns>
    bool IsRegistered(TfToken fieldKey, out VtValue? fallback);

    /// <summary>
    /// Returns whether the given field is a 'children' field -- that is, it
    /// indexes certain children beneath the owning spec.
    /// </summary>
    /// <param name="fieldKey">Field to check</param>
    /// <returns>True if field holds children</returns>
    bool HoldsChildren(TfToken fieldKey);

    /// <summary>
    /// Return the fallback value for the specified fieldKey or the
    /// empty value if fieldKey is not registered.
    /// </summary>
    /// <param name="fieldKey">Field key to get fallback for</param>
    /// <returns>Fallback value</returns>
    VtValue GetFallback(TfToken fieldKey);

    /// <summary>
    /// Coerce value to the correct type for the specified field.
    /// </summary>
    /// <param name="fieldKey">Field key</param>
    /// <param name="value">Value to coerce</param>
    /// <returns>Coerced value</returns>
    VtValue CastToTypeOf(TfToken fieldKey, VtValue value);

    /// <summary>
    /// Return whether the given field is valid for the given spec type.
    /// </summary>
    /// <param name="fieldKey">Field to check</param>
    /// <param name="specType">Spec type to check against</param>
    /// <returns>True if field is valid for spec type</returns>
    bool IsValidFieldForSpec(TfToken fieldKey, SdfSpecType specType);

    /// <summary>
    /// Returns all fields registered for the given spec type.
    /// </summary>
    /// <param name="specType">Spec type to get fields for</param>
    /// <returns>Collection of field tokens</returns>
    IReadOnlyList<TfToken> GetFields(SdfSpecType specType);

    /// <summary>
    /// Returns all metadata fields registered for the given spec type.
    /// </summary>
    /// <param name="specType">Spec type to get metadata fields for</param>
    /// <returns>Collection of metadata field tokens</returns>
    IReadOnlyList<TfToken> GetMetadataFields(SdfSpecType specType);

    /// <summary>
    /// Return the metadata field display group for metadata field on specType.
    /// Return the empty token if field is not a metadata field, or if it has no display group.
    /// </summary>
    /// <param name="specType">Spec type</param>
    /// <param name="metadataField">Metadata field</param>
    /// <returns>Display group token or empty token</returns>
    TfToken GetMetadataFieldDisplayGroup(SdfSpecType specType, TfToken metadataField);

    /// <summary>
    /// Returns all required fields registered for the given spec type.
    /// </summary>
    /// <param name="specType">Spec type to get required fields for</param>
    /// <returns>Collection of required field tokens</returns>
    IReadOnlyList<TfToken> GetRequiredFields(SdfSpecType specType);

    /// <summary>
    /// Return true if fieldName is a required field name for at least one
    /// spec type, return false otherwise.
    /// </summary>
    /// <param name="fieldName">Field name to check</param>
    /// <returns>True if field is required somewhere</returns>
    bool IsRequiredFieldName(TfToken fieldName);

    /// <summary>
    /// Given a value, check if it is a valid value type.
    /// This function only checks that the type of the value is valid
    /// for this schema. It does not imply that the value is valid for
    /// a particular field -- the field's validation function must be
    /// used for that.
    /// </summary>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidValue(VtValue value);

    /// <summary>
    /// Returns all registered type names.
    /// </summary>
    /// <returns>Collection of all type names</returns>
    IReadOnlyList<SdfValueTypeName> GetAllTypes();

    /// <summary>
    /// Return the type name object for the given type name token.
    /// </summary>
    /// <param name="typeName">Type name to find</param>
    /// <returns>Value type name</returns>
    SdfValueTypeName FindType(TfToken typeName);

    /// <summary>
    /// Return the type name object for the given type name string.
    /// </summary>
    /// <param name="typeName">Type name to find</param>
    /// <returns>Value type name</returns>
    SdfValueTypeName FindType(string typeName);

    /// <summary>
    /// Return the type name object for the given type and optional role.
    /// </summary>
    /// <param name="type">Type to find</param>
    /// <param name="role">Optional role token</param>
    /// <returns>Value type name</returns>
    SdfValueTypeName FindType(Type type, TfToken? role = null);

    /// <summary>
    /// Return the type name object for the value's type and optional role.
    /// </summary>
    /// <param name="value">Value to get type for</param>
    /// <param name="role">Optional role token</param>
    /// <returns>Value type name</returns>
    SdfValueTypeName FindType(VtValue value, TfToken? role = null);

    /// <summary>
    /// Return the type name object for the given type name string if it
    /// exists otherwise create a temporary type name object.
    /// </summary>
    /// <param name="typeName">Type name to find or create</param>
    /// <returns>Value type name</returns>
    SdfValueTypeName FindOrCreateType(TfToken typeName);
}

/// <summary>
/// Interface for class defining various attributes for a field.
/// </summary>
public interface IFieldDefinition
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    TfToken Name { get; }

    /// <summary>
    /// Gets the fallback value for the field.
    /// </summary>
    VtValue FallbackValue { get; }

    /// <summary>
    /// Gets additional field information.
    /// </summary>
    IReadOnlyList<KeyValuePair<TfToken, object>> Info { get; }

    /// <summary>
    /// Gets whether this is a plugin field.
    /// </summary>
    bool IsPlugin { get; }

    /// <summary>
    /// Gets whether this field is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets whether this field holds children.
    /// </summary>
    bool HoldsChildren { get; }

    /// <summary>
    /// Validates if a given value passes the registered validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidValue<T>(T value);

    /// <summary>
    /// Validates if a given list value passes the registered list validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidListValue<T>(T value);

    /// <summary>
    /// Validates if a given map key passes the registered map key validator.
    /// </summary>
    /// <typeparam name="T">Type of key to validate</typeparam>
    /// <param name="value">Key to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidMapKey<T>(T value);

    /// <summary>
    /// Validates if a given map value passes the registered map value validator.
    /// </summary>
    /// <typeparam name="T">Type of value to validate</typeparam>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    SdfAllowed IsValidMapValue<T>(T value);
}

/// <summary>
/// Interface for class representing fields and other information for a spec type.
/// </summary>
public interface ISpecDefinition
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