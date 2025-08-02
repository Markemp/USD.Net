using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Flags for prim traversal filtering.
/// </summary>
[Flags]
public enum UsdPrimFlags
{
    None = 0,
    Active = 1 << 0,        // UsdPrimIsActive
    Loaded = 1 << 1,        // UsdPrimIsLoaded
    Model = 1 << 2,         // UsdPrimIsModel
    Group = 1 << 3,         // UsdPrimIsGroup
    Abstract = 1 << 4,      // UsdPrimIsAbstract
    Defined = 1 << 5,       // UsdPrimIsDefined
    Instance = 1 << 6,      // UsdPrimIsInstance
    HasDefiningSpecifier = 1 << 7  // UsdPrimHasDefiningSpecifier
}

/// <summary>
/// Predicate system for efficient prim filtering during traversal.
/// </summary>
public static class UsdPrimPredicates
{
    /// <summary>
    /// Default predicate: Valid && Active && !Abstract (simplified for C#)
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Default = prim => 
        prim.IsValid() && prim.IsActive() && !prim.IsAbstract();

    /// <summary>
    /// Predicate that matches all prims (no filtering).
    /// </summary>
    public static readonly Func<IUsdPrim, bool> All = prim => prim.IsValid();

    /// <summary>
    /// Predicate for active prims only.
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Active = prim => prim.IsValid() && prim.IsActive();

    /// <summary>
    /// Predicate for defined prims only.
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Defined = prim => prim.IsValid() && prim.IsDefined();

    /// <summary>
    /// Predicate for model prims only.
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Models = prim => prim.IsValid() && prim.IsModel();

    /// <summary>
    /// Predicate for group prims only.
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Groups = prim => prim.IsValid() && prim.IsGroup();

    /// <summary>
    /// Predicate for component prims only.
    /// </summary>
    public static readonly Func<IUsdPrim, bool> Components = prim => prim.IsValid() && prim.IsComponent();

    /// <summary>
    /// Create a predicate that matches prims with specific type names.
    /// </summary>
    public static Func<IUsdPrim, bool> OfType(params string[] typeNames)
    {
        var typeSet = new HashSet<string>(typeNames);
        return prim => prim.IsValid() && typeSet.Contains(prim.GetTypeName());
    }

    /// <summary>
    /// Create a predicate that matches prims with names matching a pattern.
    /// </summary>
    public static Func<IUsdPrim, bool> WithName(Func<string, bool> nameFilter)
        => prim => prim.IsValid() && nameFilter(prim.GetName());

    /// <summary>
    /// Create an AND combination of predicates.
    /// </summary>
    public static Func<IUsdPrim, bool> And(params Func<IUsdPrim, bool>[] predicates)
        => prim => predicates.All(p => p(prim));

    /// <summary>
    /// Create an OR combination of predicates.
    /// </summary>
    public static Func<IUsdPrim, bool> Or(params Func<IUsdPrim, bool>[] predicates)
        => prim => predicates.Any(p => p(prim));

    /// <summary>
    /// Create a NOT predicate.
    /// </summary>
    public static Func<IUsdPrim, bool> Not(Func<IUsdPrim, bool> predicate)
        => prim => !predicate(prim);
}

/// <summary>
/// Traversal mode for scene graph iteration.
/// </summary>
public enum UsdTraversalMode
{
    /// <summary>
    /// Depth-first pre-order traversal (visit parent before children).
    /// </summary>
    DepthFirst,

    /// <summary>
    /// Breadth-first traversal (visit all siblings before their children).
    /// </summary>
    BreadthFirst,

    /// <summary>
    /// Pre and post-order traversal (visit each prim twice).
    /// </summary>
    PreAndPost
}

/// <summary>
/// Efficient range-based traversal of USD prim subtrees.
/// Provides depth-first iteration with optional filtering and pruning.
/// </summary>
public class UsdPrimRange : IEnumerable<IUsdPrim>
{
    private readonly IUsdPrim _start;
    protected readonly Func<IUsdPrim, bool> _predicate;
    private readonly UsdTraversalMode _mode;

    #region Construction

    /// <summary>
    /// Create a prim range starting from the given prim using the default predicate.
    /// </summary>
    public UsdPrimRange(IUsdPrim start)
        : this(start, UsdPrimPredicates.Default, UsdTraversalMode.DepthFirst)
    {
    }

    /// <summary>
    /// Create a prim range with a custom predicate.
    /// </summary>
    public UsdPrimRange(IUsdPrim start, Func<IUsdPrim, bool> predicate)
        : this(start, predicate, UsdTraversalMode.DepthFirst)
    {
    }

    /// <summary>
    /// Create a prim range with custom predicate and traversal mode.
    /// </summary>
    public UsdPrimRange(IUsdPrim start, Func<IUsdPrim, bool> predicate, UsdTraversalMode mode)
    {
        _start = start ?? throw new ArgumentNullException(nameof(start));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _mode = mode;
    }

    #endregion

    #region Static Factory Methods

    /// <summary>
    /// Create a range that visits all prims (no filtering).
    /// </summary>
    public static UsdPrimRange AllPrims(IUsdPrim start)
        => new UsdPrimRange(start, UsdPrimPredicates.All);

    /// <summary>
    /// Create a range with pre and post-order visitation.
    /// </summary>
    public static UsdPrimRange PreAndPostVisit(IUsdPrim start)
    {
        return new UsdPrimRange(start, UsdPrimPredicates.Default, UsdTraversalMode.PreAndPost);
    }

    /// <summary>
    /// Create a range for traversing an entire stage.
    /// Uses simple C# LINQ patterns instead of complex iterators.
    /// </summary>
    public static UsdPrimRange Stage(UsdStage stage, Func<IUsdPrim, bool>? predicate = null)
    {
        if (stage is null)
            throw new ArgumentNullException(nameof(stage));

        predicate ??= UsdPrimPredicates.Default;
        
        // Use simple C# LINQ filtering over the stage's prim collection
        return new SimpleUsdPrimRange(stage.TraverseAll().Where(predicate));
    }

    #endregion

    #region IEnumerable Implementation

    /// <summary>
    /// Get an iterator for this prim range using simple C# traversal.
    /// </summary>
    public virtual IEnumerator<IUsdPrim> GetEnumerator()
    {
        return _mode switch
        {
            UsdTraversalMode.DepthFirst => TraverseDepthFirst(_start, _predicate).GetEnumerator(),
            UsdTraversalMode.BreadthFirst => TraverseBreadthFirst(_start, _predicate).GetEnumerator(),
            UsdTraversalMode.PreAndPost => TraversePreAndPost(_start, _predicate).GetEnumerator(),
            _ => throw new InvalidOperationException($"Unknown traversal mode: {_mode}")
        };
    }

    /// <summary>
    /// Simple depth-first traversal using C# yield return.
    /// </summary>
    private static IEnumerable<IUsdPrim> TraverseDepthFirst(IUsdPrim start, Func<IUsdPrim, bool> predicate)
    {
        if (!start.IsValid())
            yield break;
            
        // Include the start prim if it passes the predicate AND it's not the absolute root
        if (predicate(start) && !start.GetPath().IsAbsoluteRootPath())
            yield return start;
        
        // Always traverse children regardless of whether start prim was included
        foreach (var child in start.GetChildren())
        {
            if (child.IsValid())
            {
                foreach (var descendant in TraverseDepthFirst(child, predicate))
                    yield return descendant;
            }
        }
    }

    /// <summary>
    /// Simple breadth-first traversal using C# Queue.
    /// </summary>
    private static IEnumerable<IUsdPrim> TraverseBreadthFirst(IUsdPrim start, Func<IUsdPrim, bool> predicate)
    {
        if (!start.IsValid())
            yield break;

        var queue = new Queue<IUsdPrim>();
        
        // Include start prim if it passes predicate and isn't absolute root
        if (predicate(start) && !start.GetPath().IsAbsoluteRootPath())
        {
            yield return start;
        }
        
        // Always add children to queue regardless of whether they pass predicate initially
        foreach (var child in start.GetChildren())
        {
            if (child.IsValid())
                queue.Enqueue(child);
        }
        
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            // Only yield if it passes the predicate
            if (predicate(current))
                yield return current;
            
            // Add children to queue for continued traversal
            foreach (var child in current.GetChildren())
            {
                if (child.IsValid())
                    queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// Pre and post order traversal using C# recursion.
    /// </summary>
    private static IEnumerable<IUsdPrim> TraversePreAndPost(IUsdPrim start, Func<IUsdPrim, bool> predicate)
    {
        if (!start.IsValid())
            yield break;
            
        // Pre-visit (if not absolute root and passes predicate)
        if (predicate(start) && !start.GetPath().IsAbsoluteRootPath())
            yield return start;
        
        // Visit children
        foreach (var child in start.GetChildren())
        {
            if (child.IsValid())
            {
                foreach (var descendant in TraversePreAndPost(child, predicate))
                    yield return descendant;
            }
        }
        
        // Post-visit (if not absolute root and passes predicate)
        if (predicate(start) && !start.GetPath().IsAbsoluteRootPath())
            yield return start;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

/// <summary>
/// Simple C#-style UsdPrimRange that wraps an IEnumerable<UsdPrim>.
/// Much simpler than complex C++ iterator patterns.
/// </summary>
public class SimpleUsdPrimRange : UsdPrimRange
{
    private readonly IEnumerable<IUsdPrim> _prims;

    public SimpleUsdPrimRange(IEnumerable<IUsdPrim> prims)
        : base((IUsdPrim)new UsdPrim(), UsdPrimPredicates.All) // Dummy parameters
    {
        _prims = prims ?? throw new ArgumentNullException(nameof(prims));
    }

    public override IEnumerator<IUsdPrim> GetEnumerator() => _prims.GetEnumerator();
}

