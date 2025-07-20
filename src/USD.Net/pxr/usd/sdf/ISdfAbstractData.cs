using Pxr.Base.Tf;
using Pxr.Base.Vt;

namespace Pxr.Usd.Sdf;

// AIDEV-NOTE: DO NOT MODIFY THIS FILE - Interface matches OpenUSD pxr/usd/sdf/abstractData.h exactly

/// <summary>
/// SdfAbstractData provides an anonymous container for scene description data
/// storage.
///
/// An SdfAbstractData is conceptually a nested dictionary container. The 
/// first level of key is an SdfPath, and the second level of key is a TfToken.
/// 
/// An SdfAbstractData can be thought of as an STL container, but whose 
/// iterators are unspecified and which supports a "visitor" API instead.
/// 
/// Note: SdfAbstractData is not a layer. SdfAbstractData values are always 
/// anonymous. SdfAbstractData provides no undo, change notification, or 
/// strong consistency guarantees. These responsibilities are left to layers.
/// </summary>
public interface ISdfAbstractData
{
    /// <summary>
    /// Return true if this data stores its data in a streaming format, false otherwise.
    /// Streaming formats are those in which data may be pulled from the backing storage
    /// at arbitrary times and thus should not be cached, such as a memory-mapped file.
    /// </summary>
    bool StreamsData();

    /// <summary>
    /// Return true if this data is detached from its serialized data store,
    /// false otherwise. Default implementation returns !StreamsData().
    /// </summary>
    bool IsDetached();

    /// <summary>
    /// Return true if this data object contains no specs.
    /// Default implementation iterates over all specs.
    /// </summary>
    bool IsEmpty();

    /// <summary>
    /// Return true if this data object equals data, false otherwise.
    /// </summary>
    bool Equals(ISdfAbstractData? data);

    /// <summary>
    /// Write this data object's contents to stream.
    /// This is useful for debugging.
    /// </summary>
    void WriteToStream(System.IO.TextWriter stream);

    /// <summary>
    /// Copy the data from source into this object.
    /// This overwrites all existing data.
    /// </summary>
    void CopyFrom(ISdfAbstractData source);

    /// <summary>
    /// Visit all specs in this data object with visitor. Visitor will be invoked
    /// for each spec in the data object.
    /// </summary>
    void VisitSpecs(SdfAbstractDataSpecVisitor visitor);

    /// <summary>
    /// Create a new spec of type specType at path. If the spec already exists
    /// the spec type will be changed.
    /// </summary>
    void CreateSpec(SdfPath path, SdfSpecType specType);

    /// <summary>
    /// Return true if this data has a spec for path.
    /// </summary>
    bool HasSpec(SdfPath path);

    /// <summary>
    /// Erase the spec at path and any fields that are on it. Note that this
    /// does not erase child specs.
    /// </summary>
    void EraseSpec(SdfPath path);

    /// <summary>
    /// Move the spec at oldPath to newPath, including all the fields that
    /// are on it. This does not move any child specs.
    /// </summary>
    void MoveSpec(SdfPath oldPath, SdfPath newPath);

    /// <summary>
    /// Return the spec type for the spec at path. Returns SdfSpecTypeUnknown
    /// if the spec doesn't exist.
    /// </summary>
    SdfSpecType GetSpecType(SdfPath path);

    /// <summary>
    /// Return true if this data has the field identified by path and fieldName,
    /// false otherwise. If value is provided, return the value if the field exists.
    /// </summary>
    bool Has(SdfPath path, TfToken fieldName, SdfAbstractDataValue? value);

    /// <summary>
    /// Return true if this data has the field identified by path and fieldName,
    /// false otherwise. If value is provided, return the value if the field exists.
    /// </summary>
    bool Has(SdfPath path, TfToken fieldName, VtValue? value = null);

    /// <summary>
    /// Return true if this data has a spec and field at path and fieldName, false otherwise.
    /// If value is provided, return the value if the field exists. If specType is provided,
    /// return the spec type if the spec exists. Default implementation calls HasSpec() and Has().
    /// </summary>
    bool HasSpecAndField(SdfPath path, TfToken fieldName, SdfAbstractDataValue? value, out SdfSpecType specType);

    /// <summary>
    /// Return true if this data has a spec and field at path and fieldName, false otherwise.
    /// If value is provided, return the value if the field exists. If specType is provided,
    /// return the spec type if the spec exists. Default implementation calls HasSpec() and Has().
    /// </summary>
    bool HasSpecAndField(SdfPath path, TfToken fieldName, VtValue? value, out SdfSpecType specType);

    /// <summary>
    /// Return the value of the field identified by path and fieldName.
    /// Returns an empty VtValue if the field doesn't exist.
    /// </summary>
    VtValue Get(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Return a type_info for the type of the value of the field identified
    /// by path and fieldName. Returns typeid(void) if no field exists.
    /// </summary>
    Type GetTypeid(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Type-checked value accessor.
    /// Return the value of the field identified by path and fieldName if it has type T,
    /// otherwise return defaultValue.
    /// </summary>
    T GetAs<T>(SdfPath path, TfToken fieldName, T defaultValue = default!);

    /// <summary>
    /// Set the field identified by path and fieldName to value,
    /// or remove the field if value is empty.
    /// </summary>
    void Set(SdfPath path, TfToken fieldName, VtValue value);

    /// <summary>
    /// Set the field identified by path and fieldName to value,
    /// or remove the field if value is empty.
    /// </summary>
    void Set(SdfPath path, TfToken fieldName, SdfAbstractDataConstValue value);

    /// <summary>
    /// Erase the field identified by path and fieldName, if it exists.
    /// </summary>
    void Erase(SdfPath path, TfToken fieldName);

    /// <summary>
    /// Return a vector of the fieldNames for the spec at path.
    /// Returns an empty vector if the spec doesn't exist.
    /// </summary>
    List<TfToken> List(SdfPath path);

    /// <summary>
    /// Return true if the dictionary-valued field at path and fieldName
    /// contains the given key, false otherwise. If value is provided, return
    /// the value associated with the key if it exists.
    /// </summary>
    bool HasDictKey(SdfPath path, TfToken fieldName, TfToken keyPath, SdfAbstractDataValue? value);

    /// <summary>
    /// Return true if the dictionary-valued field at path and fieldName
    /// contains the given key, false otherwise. If value is provided, return
    /// the value associated with the key if it exists.
    /// </summary>
    bool HasDictKey(SdfPath path, TfToken fieldName, TfToken keyPath, VtValue? value = null);

    /// <summary>
    /// Return the value for the given key in the dictionary-valued field at path and fieldName.
    /// Returns an empty VtValue if the key doesn't exist.
    /// </summary>
    VtValue GetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath);

    /// <summary>
    /// Set the value for the given key in the dictionary-valued field at path and fieldName.
    /// If value is empty, remove the key.
    /// </summary>
    void SetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath, VtValue value);

    /// <summary>
    /// Set the value for the given key in the dictionary-valued field at path and fieldName.
    /// If value is empty, remove the key.
    /// </summary>
    void SetDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath, SdfAbstractDataConstValue value);

    /// <summary>
    /// Remove the value for the given key in the dictionary-valued field at path and fieldName.
    /// </summary>
    void EraseDictValueByKey(SdfPath path, TfToken fieldName, TfToken keyPath);

    /// <summary>
    /// Return a vector of keys in the dictionary-valued field at path and fieldName.
    /// Returns an empty vector if the field doesn't exist or is not dictionary-valued.
    /// </summary>
    List<TfToken> ListDictKeys(SdfPath path, TfToken fieldName, TfToken keyPath);

    /// <summary>
    /// Return a set of all time samples on any spec in this data.
    /// </summary>
    HashSet<double> ListAllTimeSamples();

    /// <summary>
    /// Return a set of all time samples for attributes at or under path.
    /// </summary>
    HashSet<double> ListTimeSamplesForPath(SdfPath path);

    /// <summary>
    /// Return the number of time samples stored at path.
    /// </summary>
    int GetNumTimeSamplesForPath(SdfPath path);

    /// <summary>
    /// Return true if this data has any time samples for any attribute,
    /// false otherwise. If lower and upper are not null, set them to the
    /// lowest and highest time samples found. Note that this function only
    /// checks attribute time samples, since relationship time samples are
    /// not supported.
    /// </summary>
    bool GetBracketingTimeSamples(double time, out double tLower, out double tUpper);

    /// <summary>
    /// Return true if the attribute at path has any time samples, false
    /// otherwise. If it does, and lower and upper are not null, then set
    /// them to the lowest and highest time samples found.
    /// </summary>
    bool GetBracketingTimeSamplesForPath(SdfPath path, double time, out double tLower, out double tUpper);

    /// <summary>
    /// If time is less than the smallest time sample for the attribute at path,
    /// returns false. Otherwise returns true and sets tPrevious to the greatest 
    /// sample time &lt;= time. Default implementation uses ListTimeSamplesForPath.
    /// </summary>
    bool GetPreviousTimeSampleForPath(SdfPath path, double time, out double tPrevious);

    /// <summary>
    /// Query the value of the attribute at path at the given time.
    /// Returns true if a value was found, false otherwise.
    /// If optionalValue is provided, it will be set to the value.
    /// </summary>
    bool QueryTimeSample(SdfPath path, double time, VtValue? optionalValue = null);

    /// <summary>
    /// Query the value of the attribute at path at the given time.
    /// Returns true if a value was found, false otherwise.
    /// If optionalValue is provided, it will be set to the value.
    /// </summary>
    bool QueryTimeSample(SdfPath path, double time, SdfAbstractDataValue? optionalValue);

    /// <summary>
    /// Set the value of the attribute at path at the given time.
    /// </summary>
    void SetTimeSample(SdfPath path, double time, VtValue value);

    /// <summary>
    /// Erase the time sample for the attribute at path at the given time.
    /// </summary>
    void EraseTimeSample(SdfPath path, double time);
}