namespace Pxr.Usd.Sdf;

// AIDEV-NOTE: DO NOT MODIFY THIS FILE - Interface matches OpenUSD pxr/usd/sdf/data.h exactly
/// <summary>
/// SdfData provides concrete scene description data storage.
/// 
/// An SdfData is an SdfAbstractData that simply stores specs and fields in a
/// map keyed by path.
/// 
/// SdfData does not provide any methods beyond those in SdfAbstractData.
/// It is a concrete implementation that stores data in memory using standard
/// C# collections. This interface exists primarily for type safety and 
/// to match the OpenUSD API structure.
/// </summary>
public interface ISdfData : ISdfAbstractData
{
    // SdfData provides no additional interface beyond the base abstract data interface.
    // All methods are inherited from ISdfAbstractData.
    // The concrete implementation provides:
    // - StreamsData() returns false (data is not streamed)
    // - IsDetached() returns true (data is detached from backing store)
    // - All other methods as defined in ISdfAbstractData
}