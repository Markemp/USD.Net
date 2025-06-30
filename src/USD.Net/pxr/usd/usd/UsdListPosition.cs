namespace Pxr.Usd;

/// <summary>
/// UsdListPosition specifies a position to insert items in a list editor.
/// This controls the strength ordering in USD's composition system.
/// </summary>
public enum UsdListPosition
{
    /// <summary>
    /// The position at the front of the prepend list.
    /// This is the strongest position - these items will have the highest precedence.
    /// </summary>
    FrontOfPrependList,

    /// <summary>
    /// The position at the back of the prepend list.
    /// This is weaker than front of prepend, but stronger than items from weaker layers.
    /// This is the default position for most operations.
    /// </summary>
    BackOfPrependList,

    /// <summary>
    /// The position at the front of the append list.
    /// This is stronger than other items in the append list for this layer.
    /// </summary>
    FrontOfAppendList,

    /// <summary>
    /// The position at the back of the append list.
    /// This is the weakest position, but still stronger than items from weaker layers.
    /// </summary>
    BackOfAppendList
}

/// <summary>
/// Extension methods for UsdListPosition.
/// </summary>
public static class UsdListPositionExtensions
{
    /// <summary>
    /// Return true if this position is in the prepend list.
    /// </summary>
    public static bool IsPrepend(this UsdListPosition position)
    {
        return position == UsdListPosition.FrontOfPrependList ||
               position == UsdListPosition.BackOfPrependList;
    }

    /// <summary>
    /// Return true if this position is in the append list.
    /// </summary>
    public static bool IsAppend(this UsdListPosition position)
    {
        return position == UsdListPosition.FrontOfAppendList ||
               position == UsdListPosition.BackOfAppendList;
    }

    /// <summary>
    /// Return true if this position is at the front of its list.
    /// </summary>
    public static bool IsFront(this UsdListPosition position)
    {
        return position == UsdListPosition.FrontOfPrependList ||
               position == UsdListPosition.FrontOfAppendList;
    }

    /// <summary>
    /// Return true if this position is at the back of its list.
    /// </summary>
    public static bool IsBack(this UsdListPosition position)
    {
        return position == UsdListPosition.BackOfPrependList ||
               position == UsdListPosition.BackOfAppendList;
    }

    /// <summary>
    /// Get a string description of this position.
    /// </summary>
    public static string GetDescription(this UsdListPosition position)
    {
        return position switch
        {
            UsdListPosition.FrontOfPrependList => "front of prepend list (strongest)",
            UsdListPosition.BackOfPrependList => "back of prepend list (default)",
            UsdListPosition.FrontOfAppendList => "front of append list",
            UsdListPosition.BackOfAppendList => "back of append list (weakest)",
            _ => "unknown position"
        };
    }
}