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
    /// Default predicate: Active && Defined && Loaded && !Abstract
    /// </summary>
    public static readonly Func<UsdPrim, bool> Default = prim => 
        prim.IsValid() && prim.IsActive() && prim.IsDefined() && prim.IsLoaded() && !prim.IsAbstract();

    /// <summary>
    /// Predicate that matches all prims (no filtering).
    /// </summary>
    public static readonly Func<UsdPrim, bool> All = prim => prim.IsValid();

    /// <summary>
    /// Predicate for active prims only.
    /// </summary>
    public static readonly Func<UsdPrim, bool> Active = prim => prim.IsValid() && prim.IsActive();

    /// <summary>
    /// Predicate for defined prims only.
    /// </summary>
    public static readonly Func<UsdPrim, bool> Defined = prim => prim.IsValid() && prim.IsDefined();

    /// <summary>
    /// Predicate for model prims only.
    /// </summary>
    public static readonly Func<UsdPrim, bool> Models = prim => prim.IsValid() && prim.IsModel();

    /// <summary>
    /// Predicate for group prims only.
    /// </summary>
    public static readonly Func<UsdPrim, bool> Groups = prim => prim.IsValid() && prim.IsGroup();

    /// <summary>
    /// Predicate for component prims only.
    /// </summary>
    public static readonly Func<UsdPrim, bool> Components = prim => prim.IsValid() && prim.IsComponent();

    /// <summary>
    /// Create a predicate that matches prims with specific type names.
    /// </summary>
    public static Func<UsdPrim, bool> OfType(params string[] typeNames)
    {
        var typeSet = new HashSet<string>(typeNames);
        return prim => prim.IsValid() && typeSet.Contains(prim.GetTypeName());
    }

    /// <summary>
    /// Create a predicate that matches prims with names matching a pattern.
    /// </summary>
    public static Func<UsdPrim, bool> WithName(Func<string, bool> nameFilter)
    {
        return prim => prim.IsValid() && nameFilter(prim.GetName());
    }

    /// <summary>
    /// Create an AND combination of predicates.
    /// </summary>
    public static Func<UsdPrim, bool> And(params Func<UsdPrim, bool>[] predicates)
    {
        return prim => predicates.All(p => p(prim));
    }

    /// <summary>
    /// Create an OR combination of predicates.
    /// </summary>
    public static Func<UsdPrim, bool> Or(params Func<UsdPrim, bool>[] predicates)
    {
        return prim => predicates.Any(p => p(prim));
    }

    /// <summary>
    /// Create a NOT predicate.
    /// </summary>
    public static Func<UsdPrim, bool> Not(Func<UsdPrim, bool> predicate)
    {
        return prim => !predicate(prim);
    }
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
public class UsdPrimRange : IEnumerable<UsdPrim>
{
    private readonly UsdPrim _start;
    protected readonly Func<UsdPrim, bool> _predicate;
    private readonly UsdTraversalMode _mode;

    #region Construction

    /// <summary>
    /// Create a prim range starting from the given prim using the default predicate.
    /// </summary>
    public UsdPrimRange(UsdPrim start)
        : this(start, UsdPrimPredicates.Default, UsdTraversalMode.DepthFirst)
    {
    }

    /// <summary>
    /// Create a prim range with a custom predicate.
    /// </summary>
    public UsdPrimRange(UsdPrim start, Func<UsdPrim, bool> predicate)
        : this(start, predicate, UsdTraversalMode.DepthFirst)
    {
    }

    /// <summary>
    /// Create a prim range with custom predicate and traversal mode.
    /// </summary>
    public UsdPrimRange(UsdPrim start, Func<UsdPrim, bool> predicate, UsdTraversalMode mode)
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
    public static UsdPrimRange AllPrims(UsdPrim start)
    {
        return new UsdPrimRange(start, UsdPrimPredicates.All);
    }

    /// <summary>
    /// Create a range with pre and post-order visitation.
    /// </summary>
    public static UsdPrimRange PreAndPostVisit(UsdPrim start)
    {
        return new UsdPrimRange(start, UsdPrimPredicates.Default, UsdTraversalMode.PreAndPost);
    }

    /// <summary>
    /// Create a range for traversing an entire stage.
    /// </summary>
    public static UsdPrimRange Stage(UsdStage stage, Func<UsdPrim, bool>? predicate = null)
    {
        if (stage == null)
            throw new ArgumentNullException(nameof(stage));

        // Use the stage's own TraverseRange method
        return stage.TraverseRange(predicate ?? UsdPrimPredicates.Default);
    }

    #endregion

    #region IEnumerable Implementation

    /// <summary>
    /// Get an iterator for this prim range.
    /// </summary>
    public IEnumerator<UsdPrim> GetEnumerator()
    {
        return _mode switch
        {
            UsdTraversalMode.DepthFirst => new DepthFirstIterator(_start, _predicate),
            UsdTraversalMode.BreadthFirst => new BreadthFirstIterator(_start, _predicate),
            UsdTraversalMode.PreAndPost => new PreAndPostIterator(_start, _predicate),
            _ => throw new InvalidOperationException($"Unknown traversal mode: {_mode}")
        };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region Iterator Implementations

    /// <summary>
    /// Base class for prim iterators with pruning support.
    /// </summary>
    public abstract class PrimIterator : IEnumerator<UsdPrim>
    {
        protected readonly Func<UsdPrim, bool> _predicate;
        protected UsdPrim _current = new();
        protected bool _pruneCurrent = false;

        protected PrimIterator(Func<UsdPrim, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public UsdPrim Current => _current;
        object IEnumerator.Current => Current;

        public abstract bool MoveNext();
        public virtual void Reset() => throw new NotSupportedException();
        public virtual void Dispose() { }

        /// <summary>
        /// Skip the children of the current prim on the next iteration.
        /// </summary>
        public void PruneChildren()
        {
            _pruneCurrent = true;
        }

        /// <summary>
        /// Check if the current visit is a post-visit (only for PreAndPost mode).
        /// </summary>
        public virtual bool IsPostVisit() => false;
    }

    /// <summary>
    /// Depth-first iterator with pruning support.
    /// </summary>
    private class DepthFirstIterator : PrimIterator
    {
        private readonly Stack<UsdPrim> _stack = new();
        private bool _started = false;

        public DepthFirstIterator(UsdPrim start, Func<UsdPrim, bool> predicate) : base(predicate)
        {
            if (start.IsValid() && _predicate(start))
                _stack.Push(start);
        }

        public override bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                if (_stack.Count > 0)
                {
                    _current = _stack.Pop();
                    return true;
                }
                return false;
            }

            // Add children to stack if not pruning
            if (!_pruneCurrent && _current.IsValid())
            {
                var children = _current.GetChildren()
                    .Where(_predicate)
                    .Reverse() // Reverse to maintain left-to-right order
                    .ToList();

                foreach (var child in children)
                    _stack.Push(child);
            }

            _pruneCurrent = false;

            if (_stack.Count == 0)
                return false;

            _current = _stack.Pop();
            return true;
        }
    }

    /// <summary>
    /// Breadth-first iterator.
    /// </summary>
    private class BreadthFirstIterator : PrimIterator
    {
        private readonly Queue<UsdPrim> _queue = new();
        private readonly HashSet<UsdPrim> _pruned = new();
        private bool _started = false;

        public BreadthFirstIterator(UsdPrim start, Func<UsdPrim, bool> predicate) : base(predicate)
        {
            if (start.IsValid() && _predicate(start))
                _queue.Enqueue(start);
        }

        public override bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
                if (_queue.Count > 0)
                {
                    _current = _queue.Dequeue();
                    return true;
                }
                return false;
            }

            // Add children to queue if not pruning
            if (_pruneCurrent)
            {
                _pruned.Add(_current);
                _pruneCurrent = false;
            }
            else if (_current.IsValid() && !_pruned.Contains(_current))
            {
                var children = _current.GetChildren().Where(_predicate);
                foreach (var child in children)
                    _queue.Enqueue(child);
            }

            if (_queue.Count == 0)
                return false;

            _current = _queue.Dequeue();
            return true;
        }
    }

    /// <summary>
    /// Pre and post-order iterator that visits each prim twice.
    /// </summary>
    private class PreAndPostIterator : PrimIterator
    {
        private readonly Stack<(UsdPrim prim, bool isPost)> _stack = new();
        private bool _started = false;
        private bool _isPostVisit = false;

        public PreAndPostIterator(UsdPrim start, Func<UsdPrim, bool> predicate) : base(predicate)
        {
            if (start.IsValid() && _predicate(start))
            {
                _stack.Push((start, true));  // Post-visit
                _stack.Push((start, false)); // Pre-visit
            }
        }

        public override bool IsPostVisit() => _isPostVisit;

        public override bool MoveNext()
        {
            if (!_started)
            {
                _started = true;
            }
            else if (!_isPostVisit && !_pruneCurrent && _current.IsValid())
            {
                // Add children for pre and post visits
                var children = _current.GetChildren()
                    .Where(_predicate)
                    .Reverse()
                    .ToList();

                foreach (var child in children)
                {
                    _stack.Push((child, true));  // Post-visit
                    _stack.Push((child, false)); // Pre-visit
                }
            }

            _pruneCurrent = false;

            if (_stack.Count == 0)
                return false;

            var (prim, isPost) = _stack.Pop();
            _current = prim;
            _isPostVisit = isPost;
            return true;
        }
    }

    #endregion
}
