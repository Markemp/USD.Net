using System;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all USD objects that can exist on a stage.
/// </summary>
public abstract class UsdObject
{
    /// <summary>
    /// Return true if this object is valid.
    /// </summary>
    public virtual bool IsValid()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the stage that owns this object.
    /// </summary>
    public virtual UsdStage GetStage()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the path to this object.
    /// </summary>
    public virtual SdfPath GetPath()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return the name of this object.
    /// </summary>
    public virtual string GetName()
    {
        throw new NotImplementedException();
    }
}