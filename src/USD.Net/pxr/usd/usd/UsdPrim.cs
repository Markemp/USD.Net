using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Base.Vt;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdPrim is the sole persistent scenegraph object on a UsdStage, and is the embodiment of a "Prim" as described in the USD Composition compendium.
/// </summary>
public class UsdPrim : UsdObject
{
    private string _typeName = string.Empty;
    private readonly Dictionary<string, object> _metadata = new();
    private readonly Dictionary<string, UsdAttribute> _attributes = new();
    private readonly Dictionary<string, UsdRelationship> _relationships = new();
    private bool? _cachedActiveFlag = null;

    #region Iterator Types
    
    public class SiblingIterator
    {
        private readonly IEnumerator<UsdPrim> _enumerator;
        
        internal SiblingIterator(IEnumerable<UsdPrim> siblings)
        {
            _enumerator = siblings.GetEnumerator();
        }
        
        public bool MoveNext() => _enumerator.MoveNext();
        public UsdPrim Current => _enumerator.Current;
        public void Dispose() => _enumerator.Dispose();
    }
    
    public class SiblingRange : IEnumerable<UsdPrim>
    {
        private readonly IEnumerable<UsdPrim> _siblings;
        
        internal SiblingRange(IEnumerable<UsdPrim> siblings)
        {
            _siblings = siblings;
        }
        
        public IEnumerator<UsdPrim> GetEnumerator() => _siblings.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public class SubtreeIterator
    {
        private readonly IEnumerator<UsdPrim> _enumerator;
        
        internal SubtreeIterator(IEnumerable<UsdPrim> descendants)
        {
            _enumerator = descendants.GetEnumerator();
        }
        
        public bool MoveNext() => _enumerator.MoveNext();
        public UsdPrim Current => _enumerator.Current;
        public void Dispose() => _enumerator.Dispose();
    }
    
    public class SubtreeRange : IEnumerable<UsdPrim>
    {
        private readonly IEnumerable<UsdPrim> _descendants;
        
        internal SubtreeRange(IEnumerable<UsdPrim> descendants)
        {
            _descendants = descendants;
        }
        
        public IEnumerator<UsdPrim> GetEnumerator() => _descendants.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    #endregion

    #region Construction
    
    /// <summary>
    /// Create an invalid prim.
    /// </summary>
    public UsdPrim() : base()
    {
    }
    
    /// <summary>
    /// Create a prim with stage and path.
    /// </summary>
    public UsdPrim(UsdStage stage, SdfPath path) : base(stage, path)
    {
    }
    
    #endregion

    #region Prim Metadata and Type Queries
    
    /// <summary>
    /// Return this prim's composed type name.
    /// </summary>
    public string GetTypeName()
    {
        return _typeName;
    }
    
    /// <summary>
    /// Set this prim's type name.
    /// </summary>
    public bool SetTypeName(string typeName)
    {
        _typeName = typeName ?? string.Empty;
        return true;
    }
    
    /// <summary>
    /// Clear this prim's type name.
    /// </summary>
    public bool ClearTypeName()
    {
        _typeName = string.Empty;
        return true;
    }
    
    /// <summary>
    /// Return true if this prim has a type name.
    /// </summary>
    public bool HasTypeName()
    {
        return !string.IsNullOrEmpty(_typeName);
    }
    
    /// <summary>
    /// Return whether this prim is active, and thus contributes to scene graph composition.
    /// </summary>
    public bool IsActive()
    {
        if (!IsValid())
            return false;
            
        // Check cached value first
        if (_cachedActiveFlag.HasValue)
            return _cachedActiveFlag.Value;
            
        // Check if there's authored active metadata
        if (_metadata.TryGetValue("active", out var activeValue))
        {
            _cachedActiveFlag = activeValue is bool boolVal ? boolVal : true;
            return _cachedActiveFlag.Value;
        }
        
        // Default is active unless explicitly set to false
        _cachedActiveFlag = true;
        return true;
    }
    
    /// <summary>
    /// Author scene description for this prim to set its active flag.
    /// </summary>
    public bool SetActive(bool active)
    {
        if (!IsValid())
            return false;
            
        _metadata["active"] = active;
        _cachedActiveFlag = active;
        return true;
    }
    
    /// <summary>
    /// Clear the active flag for this prim in the current edit target.
    /// </summary>
    public bool ClearActive()
    {
        if (!IsValid())
            return false;
            
        _metadata.Remove("active");
        _cachedActiveFlag = null;
        return true;
    }
    
    /// <summary>
    /// Return true if this prim has an authored active opinion.
    /// </summary>
    public bool HasAuthoredActive()
    {
        return _metadata.ContainsKey("active");
    }
    
    /// <summary>
    /// Return true if this prim is a model.
    /// </summary>
    public bool IsModel()
    {
        var kind = GetKind();
        return IsKindModel(kind);
    }
    
    /// <summary>
    /// Return true if this prim is a group.
    /// </summary>
    public bool IsGroup()
    {
        var kind = GetKind();
        return IsKindGroup(kind);
    }
    
    /// <summary>
    /// Return true if this prim is a component.
    /// </summary>
    public bool IsComponent()
    {
        var kind = GetKind();
        return IsKindComponent(kind);
    }
    
    #endregion

    #region Kind System Helpers
    
    /// <summary>
    /// Get the kind metadata for this prim.
    /// </summary>
    private string GetKind()
    {
        return _metadata.TryGetValue("kind", out var kind) ? kind.ToString() ?? string.Empty : string.Empty;
    }
    
    /// <summary>
    /// Check if a kind is a model kind.
    /// </summary>
    private static bool IsKindModel(string kind)
    {
        return kind == "model" || IsKindComponent(kind) || IsKindGroup(kind);
    }
    
    /// <summary>
    /// Check if a kind is a group kind.
    /// </summary>
    private static bool IsKindGroup(string kind)
    {
        return kind == "group" || kind == "assembly";
    }
    
    /// <summary>
    /// Check if a kind is a component kind.
    /// </summary>
    private static bool IsKindComponent(string kind)
    {
        return kind == "component";
    }
    
    #endregion

    #region Hierarchy Navigation
    
    /// <summary>
    /// Return this prim's parent prim.
    /// </summary>
    public UsdPrim GetParent()
    {
        if (!IsValid())
            return new UsdPrim();
            
        var parentPath = GetPath().GetParentPath();
        if (parentPath.IsEmpty())
            return new UsdPrim(); // Root prim has no parent
            
        var stage = GetStage();
        return stage?.GetPrimAtPath(parentPath) ?? new UsdPrim();
    }
    
    /// <summary>
    /// Return a forward iterator over this prim's direct child prims.
    /// </summary>
    public SiblingRange GetChildren()
    {
        if (!IsValid())
            return new SiblingRange(Enumerable.Empty<UsdPrim>());
            
        var stage = GetStage();
        if (stage == null)
            return new SiblingRange(Enumerable.Empty<UsdPrim>());
            
        // Get all prims from stage and filter for direct children
        var children = stage.TraverseAll()
            .Where(prim => prim.IsValid() && 
                          prim.GetPath().GetParentPath().Equals(GetPath()))
            .Where(prim => prim.GetPath() != GetPath()); // Exclude self
            
        return new SiblingRange(children);
    }
    
    /// <summary>
    /// Return a filtered view of this prim's direct child prims.
    /// </summary>
    public SiblingRange GetFilteredChildren(Func<UsdPrim, bool> predicate)
    {
        if (predicate == null)
            return GetChildren();
            
        var children = GetChildren().Where(predicate);
        return new SiblingRange(children);
    }
    
    /// <summary>
    /// Return all descendant prims below this prim in the scene graph.
    /// </summary>
    public SubtreeRange GetDescendants()
    {
        if (!IsValid())
            return new SubtreeRange(Enumerable.Empty<UsdPrim>());
            
        var stage = GetStage();
        if (stage == null)
            return new SubtreeRange(Enumerable.Empty<UsdPrim>());
            
        // Get all prims that have this prim's path as a prefix
        var descendants = stage.TraverseAll()
            .Where(prim => prim.IsValid() && 
                          prim.GetPath().HasPrefix(GetPath()) &&
                          prim.GetPath() != GetPath()); // Exclude self
            
        return new SubtreeRange(descendants);
    }
    
    /// <summary>
    /// Return a range containing the sibling prims of this prim.
    /// </summary>
    public SiblingRange GetSiblings()
    {
        var parent = GetParent();
        if (!parent.IsValid())
            return new SiblingRange(Enumerable.Empty<UsdPrim>());
            
        // Get all children of parent, excluding self
        var siblings = parent.GetChildren()
            .Where(prim => prim.GetPath() != GetPath());
            
        return new SiblingRange(siblings);
    }
    
    /// <summary>
    /// Return this prim's next sibling if it has one, otherwise return an invalid UsdPrim.
    /// </summary>
    public UsdPrim GetNextSibling()
    {
        var parent = GetParent();
        if (!parent.IsValid())
            return new UsdPrim();
            
        // Get all children of parent (including self)
        var allChildren = parent.GetChildren().ToList();
        if (allChildren.Count == 0)
            return new UsdPrim();
            
        // Find this prim in children list and return next one
        for (int i = 0; i < allChildren.Count - 1; i++)
        {
            if (allChildren[i].GetPath().Equals(GetPath()))
                return allChildren[i + 1];
        }
        
        return new UsdPrim(); // No next sibling or not found
    }
    
    /// <summary>
    /// Return the prim at the given path below this prim.
    /// </summary>
    public UsdPrim GetChild(string name)
    {
        if (!IsValid() || string.IsNullOrEmpty(name))
            return new UsdPrim();
            
        var childPath = GetPath().AppendChild(name);
        var stage = GetStage();
        return stage?.GetPrimAtPath(childPath) ?? new UsdPrim();
    }
    
    #endregion

    #region Property Management
    
    /// <summary>
    /// Create an attribute with the given name and type.
    /// </summary>
    public UsdAttribute CreateAttribute(string name, string typeName, bool custom = false)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(name));

        var stage = GetStage();
        if (stage == null)
            return new UsdAttribute(); // Invalid attribute

        var attributePath = GetPath().AppendProperty(name);
        var attribute = new UsdAttribute(stage, attributePath, typeName);
        attribute.SetCustom(custom);
        
        _attributes[name] = attribute;
        return attribute;
    }
    
    /// <summary>
    /// Return a UsdAttribute with the given name.
    /// </summary>
    public UsdAttribute GetAttribute(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new UsdAttribute();

        return _attributes.TryGetValue(name, out var attribute) ? attribute : new UsdAttribute();
    }
    
    /// <summary>
    /// Return true if the attribute with the given name exists on this prim.
    /// </summary>
    public bool HasAttribute(string name)
    {
        return !string.IsNullOrEmpty(name) && _attributes.ContainsKey(name);
    }
    
    /// <summary>
    /// Like GetAttribute(), but return a valid UsdAttribute only if an existing attribute is present.
    /// </summary>
    public UsdAttribute GetAttributeAtPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return new UsdAttribute();

        // Extract attribute name from path
        var sdfPath = new SdfPath(path);
        if (!sdfPath.IsPropertyPath())
            return new UsdAttribute();

        var attributeName = sdfPath.GetName();
        var attribute = GetAttribute(attributeName);
        
        return attribute.IsValid() ? attribute : new UsdAttribute();
    }
    
    /// <summary>
    /// Return all of this prim's attributes.
    /// </summary>
    public IEnumerable<UsdAttribute> GetAttributes()
    {
        return _attributes.Values.Where(attr => attr.IsValid());
    }
    
    /// <summary>
    /// Create a relationship with the given name.
    /// </summary>
    public UsdRelationship CreateRelationship(string name, bool custom = false)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Relationship name cannot be null or empty", nameof(name));

        var stage = GetStage();
        if (stage == null)
            return new UsdRelationship(); // Invalid relationship

        var relationshipPath = GetPath().AppendProperty(name);
        var relationship = new UsdRelationship(stage, relationshipPath);
        relationship.SetCustom(custom);
        
        _relationships[name] = relationship;
        return relationship;
    }
    
    /// <summary>
    /// Return a UsdRelationship with the given name.
    /// </summary>
    public UsdRelationship GetRelationship(string name)
    {
        if (string.IsNullOrEmpty(name))
            return new UsdRelationship();

        return _relationships.TryGetValue(name, out var relationship) ? relationship : new UsdRelationship();
    }
    
    /// <summary>
    /// Return true if the relationship with the given name exists on this prim.
    /// </summary>
    public bool HasRelationship(string name)
    {
        return !string.IsNullOrEmpty(name) && _relationships.ContainsKey(name);
    }
    
    /// <summary>
    /// Return all of this prim's relationships.
    /// </summary>
    public IEnumerable<UsdRelationship> GetRelationships()
    {
        return _relationships.Values.Where(rel => rel.IsValid());
    }
    
    #endregion

    #region Schema and API Management
    
    /// <summary>
    /// Return true if this prim is an instance of the given schema type.
    /// </summary>
    public bool IsA<T>() where T : UsdSchemaBase
    {
        // TODO: Implement when UsdSchemaBase is available
        return false;
    }
    
    /// <summary>
    /// Return true if this prim is an instance of the given schema type.
    /// </summary>
    public bool IsA(Type schemaType)
    {
        // TODO: Implement when UsdSchemaBase is available
        return false;
    }
    
    /// <summary>
    /// Return true if this prim has an applied API schema.
    /// </summary>
    public bool HasAPI<T>() where T : UsdAPISchemaBase
    {
        // TODO: Implement when UsdAPISchemaBase is available
        return false;
    }
    
    /// <summary>
    /// Apply an API schema to this prim.
    /// </summary>
    public bool ApplyAPI<T>() where T : UsdAPISchemaBase
    {
        // TODO: Implement when UsdAPISchemaBase is available
        return false;
    }
    
    /// <summary>
    /// Remove an applied API schema from this prim.
    /// </summary>
    public bool RemoveAPI<T>() where T : UsdAPISchemaBase
    {
        // TODO: Implement when UsdAPISchemaBase is available
        return false;
    }
    
    #endregion

    #region Instance and Prototype Handling
    
    /// <summary>
    /// Return true if this prim can be instanced.
    /// </summary>
    public bool IsInstanceable()
    {
        if (!IsValid())
            return false;
            
        return _metadata.TryGetValue("instanceable", out var value) && 
               value is bool boolVal && boolVal;
    }
    
    /// <summary>
    /// Set whether this prim can be instanced.
    /// </summary>
    public bool SetInstanceable(bool instanceable)
    {
        if (!IsValid())
            return false;
            
        _metadata["instanceable"] = instanceable;
        return true;
    }
    
    /// <summary>
    /// Clear the instanceable flag for this prim.
    /// </summary>
    public bool ClearInstanceable()
    {
        if (!IsValid())
            return false;
            
        return _metadata.Remove("instanceable");
    }
    
    /// <summary>
    /// Return true if this prim is an instance.
    /// </summary>
    public bool IsInstance()
    {
        if (!IsValid())
            return false;
            
        // For now, simplified implementation
        // TODO: Implement proper instance detection with stage support
        return _metadata.TryGetValue("instance", out var value) && 
               value is bool boolVal && boolVal;
    }
    
    /// <summary>
    /// If this prim is an instance, return its prototype.
    /// </summary>
    public UsdPrim GetPrototype()
    {
        if (!IsInstance())
            return new UsdPrim();
            
        // TODO: Implement proper prototype lookup with stage support
        // For now, return invalid prim as placeholder
        return new UsdPrim();
    }
    
    #endregion

    #region Composition
    
    /// <summary>
    /// Return this prim's references.
    /// </summary>
    public UsdReferences GetReferences()
    {
        // TODO: Implement when UsdReferences is available
        return new UsdReferences();
    }
    
    /// <summary>
    /// Return this prim's inherits.
    /// </summary>
    public UsdInherits GetInherits()
    {
        // TODO: Implement when UsdInherits is available
        return new UsdInherits();
    }
    
    /// <summary>
    /// Return this prim's specializes.
    /// </summary>
    public UsdSpecializes GetSpecializes()
    {
        // TODO: Implement when UsdSpecializes is available
        return new UsdSpecializes();
    }
    
    /// <summary>
    /// Return this prim's variant sets.
    /// </summary>
    public UsdVariantSets GetVariantSets()
    {
        // TODO: Implement when UsdVariantSets is available
        return new UsdVariantSets();
    }
    
    #endregion

    #region Metadata
    
    /// <summary>
    /// Set metadata for this prim.
    /// </summary>
    public bool SetMetadata<T>(string key, T value)
    {
        if (!IsValid() || string.IsNullOrEmpty(key))
            return false;
            
        _metadata[key] = value!;
        
        // Clear cached flags that might be affected
        if (key == "active")
            _cachedActiveFlag = null;
            
        return true;
    }
    
    /// <summary>
    /// Get metadata for this prim.
    /// </summary>
    public T GetMetadata<T>(string key)
    {
        if (!IsValid() || string.IsNullOrEmpty(key))
        {
            // Special case for string to return empty string instead of null
            if (typeof(T) == typeof(string))
                return (T)(object)string.Empty;
            return default(T)!;
        }
            
        if (_metadata.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                    return typedValue;
                if (value != null)
                    return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                // Conversion failed, return default
            }
        }
        
        // Special case for string to return empty string instead of null
        if (typeof(T) == typeof(string))
            return (T)(object)string.Empty;
        return default(T)!;
    }
    
    /// <summary>
    /// Return true if this prim has metadata with the given key.
    /// </summary>
    public bool HasMetadata(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;
            
        return _metadata.ContainsKey(key);
    }
    
    /// <summary>
    /// Clear metadata with the given key from this prim.
    /// </summary>
    public bool ClearMetadata(string key)
    {
        if (!IsValid() || string.IsNullOrEmpty(key))
            return false;
            
        var removed = _metadata.Remove(key);
        
        // Clear cached flags that might be affected
        if (key == "active")
            _cachedActiveFlag = null;
            
        return removed;
    }
    
    #endregion
}