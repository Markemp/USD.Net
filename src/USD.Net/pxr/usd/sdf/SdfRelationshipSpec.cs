using Pxr.Base.Tf;

namespace Pxr.Usd.Sdf;

/// <summary>
/// A property that contains a reference to one or more SdfPrimSpec instances.
/// </summary>
/// <remarks>
/// A relationship may refer to one or more target prims or attributes.
/// All targets of a single relationship are considered to be playing the same
/// role. Note that role does not imply that the target prims or attributes
/// are of the same type.
/// 
/// Relationships may be annotated with relational attributes.
/// Relational attributes are named SdfAttributeSpec objects containing
/// values that describe the relationship. For example, point weights are
/// commonly expressed as relational attributes.
/// </remarks>
public class SdfRelationshipSpec : SdfPropertySpec
{
    // AIDEV-NOTE: layer-integration; all data stored in layer via SetField/GetField pattern

    #region Construction

    /// <summary>
    /// Creates a new prim relationship instance.
    /// </summary>
    /// <param name="owner">The owner prim spec that will own this relationship.</param>
    /// <param name="name">The name of the relationship.</param>
    /// <param name="custom">Whether this is a custom relationship (default: true).</param>
    /// <param name="variability">The variability of the relationship (default: SdfVariabilityUniform).</param>
    /// <returns>A new relationship spec.</returns>
    public static SdfRelationshipSpec New(
        SdfPrimSpec owner,
        string name,
        bool custom = true,
        SdfVariability variability = SdfVariability.Uniform)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        var layer = owner.GetLayer();
        var ownerPath = owner.GetPath();
        var relPath = ownerPath.AppendProperty(name);

        // Create the relationship spec with proper layer integration
        var relSpec = new SdfRelationshipSpec(layer, relPath)
        {
            Name = name,
            Custom = custom,
            Variability = variability
        };

        // Set the fields in the layer immediately
        layer.SetField(relPath, SdfFieldKeys.Custom, custom);
        layer.SetField(relPath, SdfFieldKeys.Variability, variability);

        owner.AddProperty(relSpec);
        return relSpec;
    }

    /// <summary>
    /// Protected constructor for derived classes.
    /// </summary>
    protected SdfRelationshipSpec() : base()
    {
    }

    /// <summary>
    /// Constructor with layer and path for layer integration.
    /// </summary>
    private SdfRelationshipSpec(ISdfLayer layer, ISdfPath path) : base(layer, path)
    {
    }

    #endregion

    #region Relationship targets

    /// <summary>
    /// Returns the relationship's target path list.
    /// </summary>
    /// <remarks>
    /// The list of the target paths for this relationship may be modified
    /// through direct manipulation of the returned list.
    /// </remarks>
    public IList<ISdfPath> GetTargetPathList()
    {
        var layer = GetLayer();
        if (layer == null) return new List<ISdfPath>();
        
        // Get target paths from layer field storage
        var targetPaths = layer.GetFieldAs<List<ISdfPath>>(GetPath(), SdfFieldKeys.TargetPaths);
        return targetPaths ?? new List<ISdfPath>();
    }

    /// <summary>
    /// Returns true if the relationship has any target paths.
    /// </summary>
    public bool HasTargetPathList()
    {
        var layer = GetLayer();
        if (layer == null) return false;
        
        return layer.HasField(GetPath(), SdfFieldKeys.TargetPaths);
    }

    /// <summary>
    /// Clears the list of target paths on this relationship.
    /// </summary>
    public void ClearTargetPathList()
    {
        var layer = GetLayer();
        if (layer != null)
        {
            layer.EraseField(GetPath(), SdfFieldKeys.TargetPaths);
        }
    }

    /// <summary>
    /// Updates the specified target path.
    /// </summary>
    /// <param name="oldPath">The path to replace.</param>
    /// <param name="newPath">The new path.</param>
    /// <remarks>
    /// Replaces the path given by oldPath with the one specified by
    /// newPath. Relational attributes are updated if necessary.
    /// </remarks>
    public void ReplaceTargetPath(ISdfPath oldPath, ISdfPath newPath)
    {
        if (oldPath == null || newPath == null)
            return;

        var layer = GetLayer();
        if (layer == null) return;

        var targetPaths = GetTargetPathList().ToList();
        var index = targetPaths.IndexOf(oldPath);
        if (index >= 0)
        {
            targetPaths[index] = newPath;
            layer.SetField(GetPath(), SdfFieldKeys.TargetPaths, targetPaths);
        }
    }

    /// <summary>
    /// Removes the specified target path.
    /// </summary>
    /// <param name="path">The path to remove.</param>
    /// <param name="preserveTargetOrder">If true, preserves the ordered items list.</param>
    /// <remarks>
    /// Removes the given target path and any relational attributes for the
    /// given target path. If preserveTargetOrder is true, Erase() is
    /// called on the list editor instead of RemoveItemEdits(). This preserves
    /// the ordered items list.
    /// </remarks>
    public void RemoveTargetPath(ISdfPath path, bool preserveTargetOrder = false)
    {
        if (path == null)
            return;

        var layer = GetLayer();
        if (layer == null) return;

        var targetPaths = GetTargetPathList().ToList();
        if (targetPaths.Remove(path))
        {
            if (targetPaths.Count == 0)
            {
                layer.EraseField(GetPath(), SdfFieldKeys.TargetPaths);
            }
            else
            {
                layer.SetField(GetPath(), SdfFieldKeys.TargetPaths, targetPaths);
            }
        }
        
        // TODO: Handle relational attributes when implemented
        // In full implementation, would also remove any relational attributes
        // associated with this target path
    }

    #endregion

    #region NoLoadHint

    /// <summary>
    /// Get whether loading the target of this relationship is necessary
    /// to load the prim we're attached to.
    /// </summary>
    public bool GetNoLoadHint()
    {
        var layer = GetLayer();
        if (layer == null) return false;
        
        return layer.GetFieldAs<bool>(GetPath(), SdfFieldKeys.NoLoadHint);
    }

    /// <summary>
    /// Set whether loading the target of this relationship is necessary
    /// to load the prim we're attached to.
    /// </summary>
    public void SetNoLoadHint(bool noload)
    {
        var layer = GetLayer();
        if (layer != null)
        {
            layer.SetField(GetPath(), SdfFieldKeys.NoLoadHint, noload);
        }
    }

    #endregion

    #region Overrides

    /// <summary>
    /// Returns the type of this spec.
    /// </summary>
    public override SdfSpecType GetSpecType() => SdfSpecType.Relationship;

    /// <summary>
    /// Returns a string representation of this relationship spec.
    /// </summary>
    public override string ToString()
    {
        var targets = string.Join(", ", GetTargetPathList().Select(p => p.ToString()));
        return $"SdfRelationshipSpec(name={Name}, targets=[{targets}])";
    }

    #endregion
}

/// <summary>
/// Handle (smart pointer) to an SdfRelationshipSpec.
/// </summary>
/// <remarks>
/// In C++ USD, this is a smart pointer type. In C#, we implement it as a simple
/// wrapper that holds a reference to an SdfRelationshipSpec. This provides a level of
/// indirection similar to the C++ implementation.
/// </remarks>
public class SdfRelationshipSpecHandle
{
    private readonly SdfRelationshipSpec? _spec;

    /// <summary>
    /// Create an invalid/null handle.
    /// </summary>
    public SdfRelationshipSpecHandle()
    {
        _spec = null;
    }

    /// <summary>
    /// Create a handle to the given relationship spec.
    /// </summary>
    /// <param name="spec">The relationship spec to reference</param>
    public SdfRelationshipSpecHandle(SdfRelationshipSpec spec)
    {
        _spec = spec;
    }

    /// <summary>
    /// Check if this handle is valid (non-null).
    /// </summary>
    public bool IsValid => _spec != null;

    /// <summary>
    /// Get the underlying spec.
    /// </summary>
    public SdfRelationshipSpec? GetSpec() => _spec;

    /// <summary>
    /// Implicit conversion from SdfRelationshipSpec to handle.
    /// </summary>
    public static implicit operator SdfRelationshipSpecHandle(SdfRelationshipSpec spec) => new(spec);

    /// <summary>
    /// Explicit conversion from handle to SdfRelationshipSpec.
    /// </summary>
    public static explicit operator SdfRelationshipSpec?(SdfRelationshipSpecHandle handle) => handle._spec;
}

/// <summary>
/// Helper class for creating relationship specs.
/// </summary>
public static class SdfRelationshipSpecHelper
{
    /// <summary>
    /// Convenience function to create a relationshipSpec on a primSpec at the
    /// given path, and any necessary parent primSpecs, in the given layer.
    /// </summary>
    /// <param name="layer">The layer to create the relationship in.</param>
    /// <param name="relPath">The path for the relationship.</param>
    /// <param name="variability">The variability of the relationship (default: SdfVariabilityVarying).</param>
    /// <param name="isCustom">Whether this is a custom relationship (default: false).</param>
    /// <returns>A handle to the created relationship spec, or null on failure.</returns>
    /// <remarks>
    /// If a relationshipSpec already exists at the given path, author
    /// variability and custom according to passed arguments and return
    /// a relationship spec handle.
    /// 
    /// Any newly created prim specs have SdfSpecifierOver and an empty type (as if
    /// created by SdfJustCreatePrimInLayer()). relPath must be a valid prim
    /// property path (see SdfPath.IsPrimPropertyPath()). Return null and issue
    /// an error if we fail to author the required scene description.
    /// </remarks>
    public static SdfRelationshipSpecHandle? CreateRelationshipInLayer(
        ISdfLayer layer,
        ISdfPath relPath,
        SdfVariability variability = SdfVariability.Varying,
        bool isCustom = false)
    {
        if (layer == null || relPath == null)
            return null;

        if (!relPath.IsPropertyPath())
        {
            // Path must be a property path
            return null;
        }

        // Get or create the prim at the parent path
        var primPath = relPath.GetPrimPath();
        var primSpec = layer.GetPrimAtPath(primPath);
        
        if (primSpec == null || !primSpec.IsValid)
        {
            // TODO: Create parent prim specs as needed with SdfSpecifierOver
            return null;
        }

        // Get the actual prim spec
        var prim = (SdfPrimSpec?)primSpec;
        if (prim == null)
            return null;

        // Create or get the relationship
        var relName = relPath.GetName();
        var relSpec = prim.GetRelationship(relName);
        
        if (relSpec == null)
        {
            // Create new relationship spec using proper layer integration
            relSpec = new SdfRelationshipSpec(layer, relPath)
            {
                Name = relName,
                Custom = isCustom,
                Variability = variability
            };
            
            // Set the fields in the layer immediately  
            layer.SetField(relPath, SdfFieldKeys.Custom, isCustom);
            layer.SetField(relPath, SdfFieldKeys.Variability, variability);
            
            prim.AddProperty(relSpec);
        }
        else
        {
            // Update existing spec fields in layer
            layer.SetField(relPath, SdfFieldKeys.Custom, isCustom);
            layer.SetField(relPath, SdfFieldKeys.Variability, variability);
            
            // Update property-level fields
            relSpec.Custom = isCustom;
            relSpec.Variability = variability;
        }

        return new SdfRelationshipSpecHandle(relSpec);
    }
}