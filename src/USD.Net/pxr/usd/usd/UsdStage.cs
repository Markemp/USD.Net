using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Ar;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdStage is the primary container for scene description in USD, representing a composed view of a scene's root layer.
/// It owns and presents composed prims as a scenegraph and manages scene composition through layering and referencing.
/// </summary>
public sealed class UsdStage
{
    private readonly SdfLayer _rootLayer;
    private readonly SdfLayer? _sessionLayer;
    private readonly List<SdfLayer> _layerStack = new();
    private readonly ArResolverContext _resolverContext;
    private readonly Dictionary<SdfPath, UsdPrim> _primIndex = new();
    private UsdPrim? _defaultPrim;
    private UsdStageLoadRules? _loadRules;
    private UsdStagePopulationMask? _populationMask;
    private UsdTimeCode _startTimeCode = UsdTimeCode.Create(1.0);
    private UsdTimeCode _endTimeCode = UsdTimeCode.Create(100.0);
    private double _timeCodesPerSecond = 24.0;
    private double _framesPerSecond = 24.0;
    private UsdEditTarget _editTarget;

    private UsdStage(SdfLayer rootLayer, SdfLayer? sessionLayer = null, ArResolverContext? resolverContext = null)
    {
        _rootLayer = rootLayer ?? throw new ArgumentNullException(nameof(rootLayer));
        _sessionLayer = sessionLayer;
        _resolverContext = resolverContext ?? new ArResolverContext();
        _layerStack.Add(_rootLayer);
        if (_sessionLayer != null)
            _layerStack.Insert(0, _sessionLayer);
        
        // Initialize edit target to the session layer if available, otherwise root layer
        _editTarget = sessionLayer != null 
            ? UsdEditTarget.ForSessionLayer(sessionLayer)
            : UsdEditTarget.ForLocalLayer(rootLayer);
    }

    #region Stage Creation and Lifetime

    /// <summary>
    /// Create a new stage with a root layer.
    /// </summary>
    public static UsdStage CreateNew(string identifier, SdfLayer? sessionLayer = null)
    {
        var rootLayer = SdfLayer.CreateNew(identifier);
        return new UsdStage(rootLayer, sessionLayer);
    }

    /// <summary>
    /// Create a stage in memory.
    /// </summary>
    public static UsdStage CreateInMemory(string? identifier = null, SdfLayer? sessionLayer = null)
    {
        var rootLayer = SdfLayer.CreateAnonymous(identifier ?? "InMemory");
        return new UsdStage(rootLayer, sessionLayer);
    }

    /// <summary>
    /// Open an existing stage from a file.
    /// </summary>
    public static UsdStage? Open(string filePath, ArResolverContext? context = null)
    {
        var rootLayer = SdfLayer.FindOrOpen(filePath);
        if (rootLayer == null)
            return null;

        return new UsdStage(rootLayer, null, context);
    }

    /// <summary>
    /// Open a stage with limited population.
    /// </summary>
    public static UsdStage? OpenMasked(string filePath, UsdStagePopulationMask? mask = null, ArResolverContext? context = null)
    {
        var stage = Open(filePath, context);
        if (stage != null && mask != null)
            stage._populationMask = mask;
        return stage;
    }

    #endregion

    #region Layer and Edit Target Management

    /// <summary>
    /// Get the root layer for this stage.
    /// </summary>
    public SdfLayer GetRootLayer() => _rootLayer;

    /// <summary>
    /// Get the session layer for this stage, if any.
    /// </summary>
    public SdfLayer? GetSessionLayer() => _sessionLayer;

    /// <summary>
    /// Get all layers used by this stage.
    /// </summary>
    public IReadOnlyList<SdfLayer> GetLayerStack() => _layerStack.AsReadOnly();

    /// <summary>
    /// Get the resolver context for this stage.
    /// </summary>
    public ArResolverContext GetPathResolverContext() => _resolverContext;

    #endregion

    #region Edit Target Management

    /// <summary>
    /// Get the current edit target for this stage.
    /// </summary>
    public UsdEditTarget GetEditTarget()
    {
        return _editTarget;
    }

    /// <summary>
    /// Set the current edit target for this stage.
    /// </summary>
    public void SetEditTarget(UsdEditTarget editTarget)
    {
        if (!editTarget.IsValid())
        {
            throw new ArgumentException("Cannot set an invalid UsdEditTarget as current", nameof(editTarget));
        }

        // Validate that the edit target's layer is in our layer stack
        var targetLayer = editTarget.GetLayer();
        if (targetLayer != null && !_layerStack.Contains(targetLayer))
        {
            throw new ArgumentException("Edit target layer must be in the stage's layer stack", nameof(editTarget));
        }

        _editTarget = editTarget;
    }

    /// <summary>
    /// Get an edit target for the specified layer in the stage's layer stack.
    /// </summary>
    public UsdEditTarget GetEditTargetForLocalLayer(SdfLayer layer)
    {
        if (!_layerStack.Contains(layer))
        {
            throw new ArgumentException("Layer must be in the stage's layer stack", nameof(layer));
        }

        return UsdEditTarget.ForLocalLayer(layer);
    }

    /// <summary>
    /// Get an edit target for the root layer.
    /// </summary>
    public UsdEditTarget GetEditTargetForRootLayer()
    {
        return UsdEditTarget.ForLocalLayer(_rootLayer);
    }

    /// <summary>
    /// Get an edit target for the session layer, if one exists.
    /// </summary>
    public UsdEditTarget? GetEditTargetForSessionLayer()
    {
        return _sessionLayer != null ? UsdEditTarget.ForSessionLayer(_sessionLayer) : null;
    }

    #endregion

    #region Prim Access and Mutation

    /// <summary>
    /// Retrieve a prim at a specific path.
    /// </summary>
    public UsdPrim GetPrimAtPath(SdfPath path)
    {
        if (path.IsEmpty())
            return new UsdPrim(); // Invalid prim

        if (_primIndex.TryGetValue(path, out var existingPrim))
            return existingPrim;

        // Return invalid prim if not found
        return new UsdPrim();
    }

    /// <summary>
    /// Get the pseudo-root prim for this stage.
    /// The pseudo-root serves as the parent for all root prims.
    /// </summary>
    public UsdPrim GetPseudoRoot()
    {
        var pseudoRootPath = SdfPath.AbsoluteRootPath();
        
        // Check if pseudo-root already exists
        if (_primIndex.TryGetValue(pseudoRootPath, out var existingPseudoRoot))
            return existingPseudoRoot;
            
        // Create pseudo-root if it doesn't exist
        var pseudoRoot = new UsdPrim(this, pseudoRootPath);
        _primIndex[pseudoRootPath] = pseudoRoot;
        return pseudoRoot;
    }

    /// <summary>
    /// Define a new prim at the given path.
    /// </summary>
    public UsdPrim DefinePrim(SdfPath path, TfToken? typeName = null)
    {
        if (path.IsEmpty() || !path.IsAbsolutePath())
            throw new ArgumentException("Path must be absolute and non-empty", nameof(path));

        // Ensure parent prims exist
        var parentPath = path.GetParentPath();
        if (!parentPath.IsEmpty() && !parentPath.IsAbsoluteRootPath())
            DefinePrim(parentPath);

        // Create or get existing prim
        if (_primIndex.TryGetValue(path, out var existingPrim))
        {
            // Update type name if provided
            if (typeName.HasValue && !typeName.Value.IsEmpty)
                existingPrim.SetTypeName(typeName.Value.GetText());

            return existingPrim;
        }

        // Create new prim
        var newPrim = new UsdPrim(this, path);
        if (typeName.HasValue && !typeName.Value.IsEmpty)
            newPrim.SetTypeName(typeName.Value.GetText());

        _primIndex[path] = newPrim;
        return newPrim;
    }

    /// <summary>
    /// Define a new prim at the given path (string overload).
    /// </summary>
    public UsdPrim DefinePrim(string path, TfToken? typeName = null)
        => DefinePrim(new SdfPath(path), typeName);

    /// <summary>
    /// Ensure a prim exists at the given path, creating it if necessary.
    /// </summary>
    public UsdPrim OverridePrim(SdfPath path) => DefinePrim(path);

    /// <summary>
    /// Remove a prim at the given path.
    /// </summary>
    public bool RemovePrim(SdfPath path)
    {
        if (path.IsEmpty())
            return false;

        // Remove all descendant prims first
        var toRemove = new List<SdfPath>();
        foreach (var kvp in _primIndex)
        {
            if (kvp.Key.HasPrefix(path))
                toRemove.Add(kvp.Key);
        }

        foreach (var pathToRemove in toRemove)
        {
            _primIndex.Remove(pathToRemove);
        }

        return toRemove.Count > 0;
    }

    /// <summary>
    /// Get the default prim for this stage.
    /// </summary>
    public UsdPrim? GetDefaultPrim() => _defaultPrim;

    /// <summary>
    /// Set the default prim for this stage.
    /// </summary>
    public bool SetDefaultPrim(UsdPrim prim)
    {
        _defaultPrim = prim;
        return true;
    }

    /// <summary>
    /// Clear the default prim.
    /// </summary>
    public bool ClearDefaultPrim()
    {
        _defaultPrim = null;
        return true;
    }

    /// <summary>
    /// Return true if this stage has a default prim.
    /// </summary>
    public bool HasDefaultPrim() => _defaultPrim != null;

    #endregion

    #region Stage Traversal

    /// <summary>
    /// Iterate through all prims on this stage.
    /// </summary>
    public IEnumerable<UsdPrim> Traverse()
    {
        return _primIndex.Values.Where(prim => prim.IsValid());
    }

    /// <summary>
    /// Iterate through prims with a custom predicate.
    /// </summary>
    public IEnumerable<UsdPrim> TraverseAll()
    {
        return _primIndex.Values;
    }
    
    /// <summary>
    /// Traverse the entire stage using UsdPrimRange with default predicate.
    /// </summary>
    public UsdPrimRange TraverseRange()
    {
        return TraverseRange(UsdPrimPredicates.Default);
    }
    
    /// <summary>
    /// Traverse the entire stage using UsdPrimRange with custom predicate.
    /// </summary>
    public UsdPrimRange TraverseRange(Func<UsdPrim, bool> predicate)
    {
        // For now, return a simple wrapper around filtered TraverseAll
        return new SimpleStageRange(this, predicate);
    }
    
    /// <summary>
    /// Traverse all prims in the stage (no filtering).
    /// </summary>
    public UsdPrimRange TraverseAllRange()
    {
        return UsdPrimRange.Stage(this, UsdPrimPredicates.All);
    }

    #endregion

    #region Load/Unload Working Set Management

    /// <summary>
    /// Load payloads for the given path.
    /// </summary>
    public UsdStage Load(SdfPath? path = null)
    {
        // For now, this is a no-op since we don't have payload support yet
        // TODO: Implement payload loading when payload system is ready
        return this;
    }

    /// <summary>
    /// Unload payloads for the given path.
    /// </summary>
    public UsdStage Unload(SdfPath? path = null)
    {
        // For now, this is a no-op since we don't have payload support yet
        // TODO: Implement payload unloading when payload system is ready
        return this;
    }

    /// <summary>
    /// Get the load rules for this stage.
    /// </summary>
    public UsdStageLoadRules GetLoadRules()
    {
        return _loadRules ??= new UsdStageLoadRules();
    }

    /// <summary>
    /// Set the load rules for this stage.
    /// </summary>
    public void SetLoadRules(UsdStageLoadRules loadRules)
    {
        _loadRules = loadRules;
    }

    /// <summary>
    /// Get the population mask for this stage.
    /// </summary>
    public UsdStagePopulationMask? GetPopulationMask() => _populationMask;

    /// <summary>
    /// Set the population mask for this stage.
    /// </summary>
    public void SetPopulationMask(UsdStagePopulationMask? mask)
    {
        _populationMask = mask;
    }

    #endregion

    #region Serialization and Export

    /// <summary>
    /// Save all dirty layers in this stage.
    /// </summary>
    public void Save()
    {
        foreach (var layer in _layerStack)
        {
            if (layer.IsDirty())
                layer.Save();
        }
    }

    /// <summary>
    /// Export this stage to a file.
    /// </summary>
    public bool Export(string filename, bool addSourceFileComment = true)
    {
        try
        {
            // Create a flattened representation and export it
            var flattened = Flatten(addSourceFileComment);
            return flattened.Export(filename);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Flatten this stage to a single layer.
    /// </summary>
    public SdfLayer Flatten(bool addSourceFileComment = true)
    {
        // Create a new layer to contain flattened content
        var flattenedLayer = SdfLayer.CreateAnonymous("flattened");

        if (addSourceFileComment)
            flattenedLayer.SetMetadata(new TfToken("comment"), new VtValue($"Flattened from stage with root layer: {_rootLayer.GetIdentifier()}"));

        // TODO: Implement proper layer composition and flattening
        // For now, just copy metadata from root layer
        foreach (var kvp in _rootLayer.GetType().GetProperties())
        {
            // This is a placeholder - proper implementation would flatten all composition
        }

        return flattenedLayer;
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Set metadata on this stage.
    /// </summary>
    public bool SetMetadata(TfToken key, VtValue value)
    {
        _rootLayer.SetMetadata(key, value);
        return true;
    }

    /// <summary>
    /// Get metadata from this stage.
    /// </summary>
    public VtValue GetMetadata(TfToken key) => _rootLayer.GetMetadata(key);

    /// <summary>
    /// Return true if this stage has metadata with the given key.
    /// </summary>
    public bool HasMetadata(TfToken key) => _rootLayer.HasMetadata(key);

    /// <summary>
    /// Clear metadata with the given key.
    /// </summary>
    public bool ClearMetadata(TfToken key)
    {
        _rootLayer.ClearMetadata(key);
        return true;
    }

    #endregion

    #region Time Configuration

    /// <summary>
    /// Get the start time code for this stage.
    /// </summary>
    public UsdTimeCode GetStartTimeCode() => _startTimeCode;

    /// <summary>
    /// Set the start time code for this stage.
    /// </summary>
    public void SetStartTimeCode(UsdTimeCode timeCode)
    {
        _startTimeCode = timeCode;
    }

    /// <summary>
    /// Get the end time code for this stage.
    /// </summary>
    public UsdTimeCode GetEndTimeCode() => _endTimeCode;

    /// <summary>
    /// Set the end time code for this stage.
    /// </summary>
    public void SetEndTimeCode(UsdTimeCode timeCode)
    {
        _endTimeCode = timeCode;
    }

    /// <summary>
    /// Get the time samples per second for this stage.
    /// </summary>
    public double GetTimeCodesPerSecond() => _timeCodesPerSecond;

    /// <summary>
    /// Set the time samples per second for this stage.
    /// </summary>
    public void SetTimeCodesPerSecond(double timeCodesPerSecond)
    {
        _timeCodesPerSecond = timeCodesPerSecond;
    }

    /// <summary>
    /// Get the frames per second for this stage.
    /// </summary>
    public double GetFramesPerSecond() => _framesPerSecond;

    /// <summary>
    /// Set the frames per second for this stage.
    /// </summary>
    public void SetFramesPerSecond(double framesPerSecond)
    {
        _framesPerSecond = framesPerSecond;
    }

    #endregion
}

/// <summary>
/// Simple implementation of UsdPrimRange for stage traversal.
/// </summary>
internal class SimpleStageRange : UsdPrimRange
{
    private readonly UsdStage _stage;
    private readonly Func<UsdPrim, bool> _predicate;

    public SimpleStageRange(UsdStage stage, Func<UsdPrim, bool> predicate)
        : base(new UsdPrim(), UsdPrimPredicates.All) // Dummy parameters
    {
        _stage = stage;
        _predicate = predicate;
    }

    public new IEnumerator<UsdPrim> GetEnumerator()
    {
        // Get all prims from stage, exclude pseudo-root, apply predicate
        return _stage.TraverseAll()
            .Where(prim => !prim.GetPath().IsAbsoluteRootPath())
            .Where(_predicate)
            .GetEnumerator();
    }
}