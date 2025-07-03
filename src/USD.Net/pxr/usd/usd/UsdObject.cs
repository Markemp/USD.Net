using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all USD objects that can exist on a stage.
/// </summary>
public abstract class UsdObject
{
    private readonly UsdStage? _stage;
    private readonly SdfPath _path;

    /// <summary>
    /// Protected constructor for USD objects.
    /// </summary>
    protected UsdObject() : this(null, SdfPath.EmptyPath())
    {
    }

    /// <summary>
    /// Protected constructor for USD objects with stage and path.
    /// </summary>
    protected UsdObject(UsdStage? stage, SdfPath path)
    {
        _stage = stage;
        _path = path;
    }

    /// <summary>
    /// Return true if this object is valid.
    /// </summary>
    public virtual bool IsValid() => _stage is not null && !_path.IsEmpty();

    /// <summary>
    /// Return the stage that owns this object.
    /// </summary>
    public virtual UsdStage? GetStage() => _stage;

    /// <summary>
    /// Return the path to this object.
    /// </summary>
    public virtual SdfPath GetPath() => _path;

    /// <summary>
    /// Return the name of this object.
    /// </summary>
    public virtual string GetName() => _path.GetName();

    /// <summary>
    /// Return true if this object has the same stage and path as another object.
    /// </summary>
    public virtual bool IsSameAs(UsdObject other)
    {
        if (other is null)
            return false;
            
        return ReferenceEquals(_stage, other._stage) && _path.Equals(other._path);
    }

    /// <summary>
    /// Return the string representation of this object's path.
    /// </summary>
    public override string ToString() => _path.GetString();

    public override bool Equals(object? obj) => obj is UsdObject other && IsSameAs(other);

    public override int GetHashCode() 
        => HashCode.Combine(_stage?.GetHashCode() ?? 0, _path.GetHashCode());
}