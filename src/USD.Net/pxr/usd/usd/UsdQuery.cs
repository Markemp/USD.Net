using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Utility class for querying and finding prims in USD stages.
/// Provides high-level search and filtering functionality built on UsdPrimRange.
/// </summary>
public static class UsdQuery
{
    #region Find by Path Pattern

    /// <summary>
    /// Find all prims matching a path pattern (supports wildcards).
    /// Uses C# optimizations: direct lookup for exact paths, LINQ filtering for patterns.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByPath(UsdStage stage, string pathPattern)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(pathPattern)) return Enumerable.Empty<UsdPrim>();

        // C# optimization: for exact paths (no wildcards), use direct lookup
        if (!pathPattern.Contains('*') && !pathPattern.Contains('?'))
        {
            var prim = stage.GetPrimAtPath(new SdfPath(pathPattern));
            return prim.IsValid() ? new[] { prim } : Enumerable.Empty<UsdPrim>();
        }

        // For wildcard patterns, use regex with simplified traversal
        var regexPattern = "^" + Regex.Escape(pathPattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        return stage.TraverseAll()
            .Where(prim => prim.IsValid() && regex.IsMatch(prim.GetPath().GetString()));
    }

    /// <summary>
    /// Find a single prim by exact path.
    /// Returns null if the prim doesn't exist (C# pattern).
    /// </summary>
    public static UsdPrim? FindPrimByPath(UsdStage stage, string path)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(path)) return null;

        var prim = stage.GetPrimAtPath(new SdfPath(path));
        return prim.IsValid() ? prim : null;
    }

    #endregion

    #region Find by Type

    /// <summary>
    /// Find all prims of a specific type.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByType(UsdStage stage, string typeName, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(typeName)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var typeFilter = UsdPrimPredicates.OfType(typeName);
        var combined = UsdPrimPredicates.And(predicate, typeFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all prims matching any of the specified types.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByTypes(UsdStage stage, IEnumerable<string> typeNames, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (typeNames == null) return Enumerable.Empty<UsdPrim>();

        var typeArray = typeNames.ToArray();
        if (typeArray.Length == 0) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var typeFilter = UsdPrimPredicates.OfType(typeArray);
        var combined = UsdPrimPredicates.And(predicate, typeFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find the first prim of a specific type.
    /// </summary>
    public static UsdPrim? FindFirstPrimByType(UsdStage stage, string typeName, bool includeInactive = false)
    {
        return FindPrimsByType(stage, typeName, includeInactive).FirstOrDefault();
    }

    #endregion

    #region Find by Name

    /// <summary>
    /// Find all prims with a specific name (basename only).
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByName(UsdStage stage, string name, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(name)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var nameFilter = UsdPrimPredicates.WithName(n => n == name);
        var combined = UsdPrimPredicates.And(predicate, nameFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all prims whose names match a pattern (supports wildcards).
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByNamePattern(UsdStage stage, string namePattern, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(namePattern)) return Enumerable.Empty<UsdPrim>();

        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(namePattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        
        var regex = new Regex(regexPattern, RegexOptions.Compiled);

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var nameFilter = UsdPrimPredicates.WithName(name => regex.IsMatch(name));
        var combined = UsdPrimPredicates.And(predicate, nameFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all prims whose names contain a substring.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByNameContains(UsdStage stage, string substring, bool includeInactive = false, bool ignoreCase = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(substring)) return Enumerable.Empty<UsdPrim>();

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var nameFilter = UsdPrimPredicates.WithName(name => name.Contains(substring, comparison));
        var combined = UsdPrimPredicates.And(predicate, nameFilter);

        return stage.TraverseAll().Where(combined);
    }

    #endregion

    #region Find by Kind

    /// <summary>
    /// Find all model prims (any kind that inherits from model).
    /// </summary>
    public static IEnumerable<UsdPrim> FindModels(UsdStage stage, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var modelFilter = UsdPrimPredicates.Models;
        var combined = UsdPrimPredicates.And(predicate, modelFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all group prims.
    /// </summary>
    public static IEnumerable<UsdPrim> FindGroups(UsdStage stage, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var groupFilter = UsdPrimPredicates.Groups;
        var combined = UsdPrimPredicates.And(predicate, groupFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all component prims.
    /// </summary>
    public static IEnumerable<UsdPrim> FindComponents(UsdStage stage, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var componentFilter = UsdPrimPredicates.Components;
        var combined = UsdPrimPredicates.And(predicate, componentFilter);

        return stage.TraverseAll().Where(combined);
    }

    #endregion

    #region Find by Attribute

    /// <summary>
    /// Find all prims that have a specific attribute.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsWithAttribute(UsdStage stage, string attributeName, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(attributeName)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var attrFilter = new Func<UsdPrim, bool>(prim => prim.HasAttribute(attributeName));
        var combined = UsdPrimPredicates.And(predicate, attrFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all prims where an attribute has a specific value.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByAttributeValue<T>(UsdStage stage, string attributeName, T value, bool includeInactive = false)
        where T : IEquatable<T>
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(attributeName)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var attrFilter = new Func<UsdPrim, bool>(prim =>
        {
            var attr = prim.GetAttribute(attributeName);
            if (!attr.IsValid()) return false;

            if (attr.Get(out var attrValue) && attrValue.IsHolding<T>())
            {
                var currentValue = attrValue.Get<T>();
                return value?.Equals(currentValue) == true;
            }
            return false;
        });
        var combined = UsdPrimPredicates.And(predicate, attrFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find all prims where an attribute has a specific string value.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByAttributeValue(UsdStage stage, string attributeName, string value, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(attributeName)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var attrFilter = new Func<UsdPrim, bool>(prim =>
        {
            var attr = prim.GetAttribute(attributeName);
            if (!attr.IsValid()) return false;

            if (attr.Get(out var attrValue))
            {
                var currentValue = attrValue.GetValue()?.ToString();
                return string.Equals(currentValue, value, StringComparison.Ordinal);
            }
            return false;
        });
        var combined = UsdPrimPredicates.And(predicate, attrFilter);

        return stage.TraverseAll().Where(combined);
    }

    #endregion

    #region Find by Relationship

    /// <summary>
    /// Find all prims that have a specific relationship.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsWithRelationship(UsdStage stage, string relationshipName, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (string.IsNullOrEmpty(relationshipName)) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var relFilter = new Func<UsdPrim, bool>(prim => prim.HasRelationship(relationshipName));
        var combined = UsdPrimPredicates.And(predicate, relFilter);

        return stage.TraverseAll().Where(combined);
    }

    #endregion

    #region Hierarchical Queries

    /// <summary>
    /// Find all descendants of a prim matching a predicate.
    /// </summary>
    public static IEnumerable<UsdPrim> FindDescendants(UsdPrim prim, Func<UsdPrim, bool>? predicate = null, bool includeInactive = false)
    {
        if (!prim.IsValid()) return Enumerable.Empty<UsdPrim>();

        var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var finalPredicate = predicate != null ? UsdPrimPredicates.And(basePredicate, predicate) : basePredicate;

        return new UsdPrimRange(prim, finalPredicate).Skip(1); // Skip the root prim itself
    }

    /// <summary>
    /// Find all direct children of a prim matching a predicate.
    /// By default, includes all children (including abstract) unless a predicate is provided.
    /// </summary>
    public static IEnumerable<UsdPrim> FindChildren(UsdPrim prim, Func<UsdPrim, bool>? predicate = null, bool includeInactive = false)
    {
        if (!prim.IsValid()) return Enumerable.Empty<UsdPrim>();

        // For direct children, we want all children by default (including abstract)
        // Only apply filtering if a specific predicate is provided
        if (predicate == null)
        {
            // No predicate - return all children, filtering only by active state if requested
            var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Active;
            return prim.GetChildren().Where(basePredicate);
        }
        else
        {
            // Predicate provided - combine with base predicate
            var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
            var finalPredicate = UsdPrimPredicates.And(basePredicate, predicate);
            return prim.GetChildren().Where(finalPredicate);
        }
    }

    /// <summary>
    /// Find the first ancestor of a prim matching a predicate.
    /// </summary>
    public static UsdPrim? FindAncestor(UsdPrim prim, Func<UsdPrim, bool> predicate)
    {
        if (!prim.IsValid() || predicate == null) return null;

        var current = prim.GetParent();
        while (current.IsValid())
        {
            if (predicate(current))
                return current;
            current = current.GetParent();
        }

        return null;
    }

    /// <summary>
    /// Find all ancestors of a prim matching a predicate.
    /// </summary>
    public static IEnumerable<UsdPrim> FindAncestors(UsdPrim prim, Func<UsdPrim, bool>? predicate = null)
    {
        if (!prim.IsValid()) yield break;

        var current = prim.GetParent();
        while (current.IsValid())
        {
            if (predicate == null || predicate(current))
                yield return current;
            current = current.GetParent();
        }
    }

    #endregion

    #region Collection Queries

    /// <summary>
    /// Count prims matching a predicate.
    /// </summary>
    public static int CountPrims(UsdStage stage, Func<UsdPrim, bool> predicate, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var combined = UsdPrimPredicates.And(basePredicate, predicate);

        return stage.TraverseAll().Where(combined).Count();
    }

    /// <summary>
    /// Check if any prims match a predicate.
    /// </summary>
    public static bool AnyPrims(UsdStage stage, Func<UsdPrim, bool> predicate, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var combined = UsdPrimPredicates.And(basePredicate, predicate);

        return stage.TraverseAll().Where(combined).Any();
    }

    /// <summary>
    /// Check if all prims in the stage match a predicate.
    /// </summary>
    public static bool AllPrims(UsdStage stage, Func<UsdPrim, bool> predicate, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var allPrims = stage.TraverseAll().Where(basePredicate);

        return allPrims.All(predicate);
    }

    #endregion

    #region Advanced Queries

    /// <summary>
    /// Find prims by multiple criteria with AND logic.
    /// </summary>
    public static IEnumerable<UsdPrim> FindPrimsByCriteria(UsdStage stage, 
        string? typeName = null,
        string? namePattern = null,
        string? pathPattern = null,
        bool? isModel = null,
        bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var predicates = new List<Func<UsdPrim, bool>>();
        
        var basePredicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        predicates.Add(basePredicate);

        if (!string.IsNullOrEmpty(typeName))
            predicates.Add(UsdPrimPredicates.OfType(typeName));

        if (!string.IsNullOrEmpty(namePattern))
        {
            var regexPattern = "^" + Regex.Escape(namePattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            var regex = new Regex(regexPattern, RegexOptions.Compiled);
            predicates.Add(UsdPrimPredicates.WithName(name => regex.IsMatch(name)));
        }

        if (isModel.HasValue)
        {
            if (isModel.Value)
                predicates.Add(UsdPrimPredicates.Models);
            else
                predicates.Add(UsdPrimPredicates.Not(UsdPrimPredicates.Models));
        }

        var combinedPredicate = UsdPrimPredicates.And(predicates.ToArray());
        IEnumerable<UsdPrim> results = stage.TraverseAll().Where(combinedPredicate);

        // Apply path pattern filtering if specified
        if (!string.IsNullOrEmpty(pathPattern))
        {
            var pathRegexPattern = "^" + Regex.Escape(pathPattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";
            var pathRegex = new Regex(pathRegexPattern, RegexOptions.Compiled);
            results = results.Where(prim => pathRegex.IsMatch(prim.GetPath().GetString()));
        }

        return results;
    }

    /// <summary>
    /// Find leaf prims (prims with no children).
    /// </summary>
    public static IEnumerable<UsdPrim> FindLeafPrims(UsdStage stage, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        var leafFilter = new Func<UsdPrim, bool>(prim => !prim.GetChildren().Any());
        var combined = UsdPrimPredicates.And(predicate, leafFilter);

        return stage.TraverseAll().Where(combined);
    }

    /// <summary>
    /// Find root prims (prims directly under the stage root).
    /// </summary>
    public static IEnumerable<UsdPrim> FindRootPrims(UsdStage stage, bool includeInactive = false)
    {
        if (stage == null) throw new ArgumentNullException(nameof(stage));

        var pseudoRoot = stage.GetPseudoRoot();
        if (!pseudoRoot.IsValid()) return Enumerable.Empty<UsdPrim>();

        var predicate = includeInactive ? UsdPrimPredicates.All : UsdPrimPredicates.Default;
        return FindChildren(pseudoRoot, predicate, includeInactive);
    }

    #endregion
}