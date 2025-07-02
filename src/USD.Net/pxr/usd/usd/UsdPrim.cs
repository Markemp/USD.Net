using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// UsdPrim is the sole persistent scenegraph object on a UsdStage, and is the embodiment of a "Prim" as described in the USD Composition compendium.
/// </summary>
public class UsdPrim : UsdObject
{
    private readonly Dictionary<string, object> _metadata = [];
    private readonly Dictionary<string, UsdAttribute> _attributes = [];
    private readonly Dictionary<string, UsdRelationship> _relationships = [];
    private bool? _cachedActiveFlag = null;

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
    
    #region Properties
    
    /// <summary>
    /// This prim's composed type name.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this prim is active and contributes to scene graph composition.
    /// </summary>
    public bool Active
    {
        get
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
        set
        {
            if (IsValid())
            {
                _metadata["active"] = value;
                _cachedActiveFlag = value;
            }
        }
    }
    
    /// <summary>
    /// Whether this prim can be instanced.
    /// </summary>
    public bool Instanceable
    {
        get => IsValid() && _metadata.TryGetValue("instanceable", out var value) && value is bool boolVal && boolVal;
        set
        {
            if (IsValid())
                _metadata["instanceable"] = value;
        }
    }
    
    /// <summary>
    /// Access to this prim's metadata.
    /// </summary>
    public object? this[string key]
    {
        get => IsValid() && !string.IsNullOrEmpty(key) && _metadata.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (IsValid() && !string.IsNullOrEmpty(key))
            {
                if (value == null)
                    _metadata.Remove(key);
                else
                    _metadata[key] = value;
                    
                // Clear cached flags that might be affected
                if (key == "active")
                    _cachedActiveFlag = null;
            }
        }
    }
    
    /// <summary>
    /// Read-only access to this prim's attributes.
    /// </summary>
    public IReadOnlyDictionary<string, UsdAttribute> Attributes => _attributes;
    
    /// <summary>
    /// Read-only access to this prim's relationships.
    /// </summary>
    public IReadOnlyDictionary<string, UsdRelationship> Relationships => _relationships;
    
    /// <summary>
    /// Direct children of this prim.
    /// </summary>
    public IEnumerable<UsdPrim> Children
    {
        get
        {
            if (!IsValid())
                return Enumerable.Empty<UsdPrim>();
                
            var stage = GetStage();
            if (stage == null)
                return Enumerable.Empty<UsdPrim>();
                
            // Get all prims from stage and filter for direct children
            return stage.TraverseAll()
                .Where(prim => prim.IsValid() && 
                              prim.GetPath().GetParentPath().Equals(GetPath()) &&
                              prim.GetPath() != GetPath()); // Exclude self
        }
    }
    
    /// <summary>
    /// All descendant prims below this prim in the scene graph.
    /// </summary>
    public IEnumerable<UsdPrim> Descendants
    {
        get
        {
            if (!IsValid())
                return Enumerable.Empty<UsdPrim>();
                
            var stage = GetStage();
            if (stage == null)
                return Enumerable.Empty<UsdPrim>();
                
            // Get all prims that have this prim's path as a prefix
            return stage.TraverseAll()
                .Where(prim => prim.IsValid() && 
                              prim.GetPath().HasPrefix(GetPath()) &&
                              prim.GetPath() != GetPath()); // Exclude self
        }
    }
    
    /// <summary>
    /// Sibling prims of this prim.
    /// </summary>
    public IEnumerable<UsdPrim> Siblings
    {
        get
        {
            var parent = GetParent();
            if (!parent.IsValid())
                return Enumerable.Empty<UsdPrim>();
                
            // Get all children of parent, excluding self
            return parent.Children.Where(prim => prim.GetPath() != GetPath());
        }
    }
    
    /// <summary>
    /// This prim's parent.
    /// </summary>
    public UsdPrim? Parent
    {
        get
        {
            if (!IsValid())
                return null;
                
            var parentPath = GetPath().GetParentPath();
            if (parentPath.IsEmpty())
                return null; // Root prim has no parent
                
            var stage = GetStage();
            var parent = stage?.GetPrimAtPath(parentPath);
            return parent?.IsValid() == true ? parent : null;
        }
    }
    
    #endregion
    
    #region USD API Compatibility Methods
    
    /// <summary>
    /// Return this prim's composed type name.
    /// </summary>
    public string GetTypeName() => TypeName;
    
    /// <summary>
    /// Set this prim's type name.
    /// </summary>
    public bool SetTypeName(string typeName)
    {
        TypeName = typeName ?? string.Empty;
        return true;
    }
    
    /// <summary>
    /// Clear this prim's type name.
    /// </summary>
    public bool ClearTypeName()
    {
        TypeName = string.Empty;
        return true;
    }
    
    /// <summary>
    /// Return true if this prim has a type name.
    /// </summary>
    public bool HasTypeName() => !string.IsNullOrEmpty(TypeName);
    
    #endregion
    
    #region Active Flag Management
    
    /// <summary>
    /// Return whether this prim is active, and thus contributes to scene graph composition.
    /// </summary>
    public bool IsActive() => Active;
    
    /// <summary>
    /// Author scene description for this prim to set its active flag.
    /// </summary>
    public bool SetActive(bool active)
    {
        Active = active;
        return IsValid();
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
    public bool HasAuthoredActive() => _metadata.ContainsKey("active");
    
    #endregion
    
    #region Model Classification
    
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
    
    /// <summary>
    /// Return true if this prim is defined (has a defining specifier).
    /// In USD, a prim is defined if it was created with DefinePrim() regardless of whether it has a type.
    /// </summary>
    public bool IsDefined()
    {
        // For now, assume all valid prims in our stage are defined
        // since we're using DefinePrim() to create them
        // TODO: Implement proper specifier tracking when we add full USD layer support
        return IsValid();
    }
    
    /// <summary>
    /// Return true if this prim is abstract.
    /// </summary>
    public bool IsAbstract()
    {
        return GetMetadata<bool>("abstract");
    }
    
    /// <summary>
    /// Return true if this prim is loaded (payloads are loaded).
    /// </summary>
    public bool IsLoaded()
    {
        // For now, assume all prims are loaded since we don't have payload support yet
        return IsValid();
    }
    
    /// <summary>
    /// Return the name of this prim.
    /// </summary>
    public override string GetName()
    {
        return IsValid() ? GetPath().GetName() : string.Empty;
    }
    
    #endregion
    
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
    public UsdPrim GetParent() => Parent ?? new UsdPrim();
    
    /// <summary>
    /// Return a forward iterator over this prim's direct child prims.
    /// </summary>
    public IEnumerable<UsdPrim> GetChildren() => Children;
    
    /// <summary>
    /// Return a filtered view of this prim's direct child prims.
    /// </summary>
    public IEnumerable<UsdPrim> GetFilteredChildren(Func<UsdPrim, bool> predicate)
    {
        return predicate == null ? Children : Children.Where(predicate);
    }
    
    /// <summary>
    /// Return all descendant prims below this prim in the scene graph.
    /// </summary>
    public IEnumerable<UsdPrim> GetDescendants() => Descendants;
    
    /// <summary>
    /// Return a range containing the sibling prims of this prim.
    /// </summary>
    public IEnumerable<UsdPrim> GetSiblings() => Siblings;
    
    /// <summary>
    /// Return this prim's next sibling if it has one, otherwise return an invalid UsdPrim.
    /// </summary>
    public UsdPrim GetNextSibling()
    {
        var parent = Parent;
        if (parent == null)
            return new UsdPrim();
            
        // Get all children of parent (including self)
        var allChildren = parent.Children.ToList();
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
    public IEnumerable<UsdAttribute> GetAttributes() => Attributes.Values.Where(attr => attr.IsValid());
    
    /// <summary>
    /// Return all valid attributes as a list.
    /// </summary>
    public List<UsdAttribute> GetAllAttributes() => Attributes.Values.Where(attr => attr.IsValid()).ToList();
    
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
    public IEnumerable<UsdRelationship> GetRelationships() => Relationships.Values.Where(rel => rel.IsValid());
    
    /// <summary>
    /// Return all valid relationships as a list.
    /// </summary>
    public List<UsdRelationship> GetAllRelationships() => Relationships.Values.Where(rel => rel.IsValid()).ToList();
    
    #endregion

    #region Schema and API Management
    
    /// <summary>
    /// Return true if this prim is an instance of the given schema type.
    /// </summary>
    public bool IsA<T>() where T : UsdSchemaBase
    {
        return IsA(typeof(T));
    }
    
    /// <summary>
    /// Return true if this prim is an instance of the given schema type.
    /// </summary>
    public bool IsA(Type schemaType)
    {
        if (!IsValid() || schemaType == null)
            return false;
            
        var registry = UsdSchemaRegistry.Instance;
        return registry.IsSchemaCompatibleWithPrim(schemaType, this);
    }
    
    /// <summary>
    /// Return true if this prim has an applied API schema.
    /// </summary>
    public bool HasAPI<T>() where T : UsdAPISchemaBase
    {
        return HasAPI(typeof(T));
    }
    
    /// <summary>
    /// Return true if this prim has an applied API schema with instance name.
    /// </summary>
    public bool HasAPI<T>(string instanceName) where T : UsdAPISchemaBase
    {
        return HasAPI(typeof(T), instanceName);
    }
    
    /// <summary>
    /// Return true if this prim has an applied API schema.
    /// </summary>
    public bool HasAPI(Type apiSchemaType)
    {
        return HasAPI(apiSchemaType, string.Empty);
    }
    
    /// <summary>
    /// Return true if this prim has an applied API schema with instance name.
    /// </summary>
    public bool HasAPI(Type apiSchemaType, string instanceName)
    {
        if (!IsValid() || apiSchemaType == null)
            return false;
            
        var registry = UsdSchemaRegistry.Instance;
        var schemaInfo = registry.FindSchemaInfo(apiSchemaType);
        
        if (schemaInfo == null)
            return false;
            
        // For now, simplified implementation
        // TODO: Implement proper API schema tracking in metadata when available
        // Check if this type appears in our applied schemas metadata
        var appliedSchemas = GetMetadata<string[]>("apiSchemas") ?? new string[0];
        var schemaName = schemaInfo.Identifier.GetText();
        
        if (schemaInfo.Kind == UsdSchemaKind.MultipleApplyAPI && !string.IsNullOrEmpty(instanceName))
        {
            // For multiple-apply APIs, look for "SchemaName:InstanceName"
            var fullName = $"{schemaName}:{instanceName}";
            return appliedSchemas.Contains(fullName);
        }
        else
        {
            // For single-apply APIs, just look for the schema name
            return appliedSchemas.Contains(schemaName);
        }
    }
    
    /// <summary>
    /// Apply an API schema to this prim.
    /// </summary>
    public bool ApplyAPI<T>() where T : UsdAPISchemaBase
    {
        return ApplyAPI(typeof(T));
    }
    
    /// <summary>
    /// Apply an API schema to this prim with instance name.
    /// </summary>
    public bool ApplyAPI<T>(string instanceName) where T : UsdAPISchemaBase
    {
        return ApplyAPI(typeof(T), instanceName);
    }
    
    /// <summary>
    /// Apply an API schema to this prim.
    /// </summary>
    public bool ApplyAPI(Type apiSchemaType)
    {
        return ApplyAPI(apiSchemaType, string.Empty);
    }
    
    /// <summary>
    /// Apply an API schema to this prim with instance name.
    /// </summary>
    public bool ApplyAPI(Type apiSchemaType, string instanceName)
    {
        if (!IsValid() || apiSchemaType == null)
            return false;
            
        var registry = UsdSchemaRegistry.Instance;
        var schemaInfo = registry.FindSchemaInfo(apiSchemaType);
        
        if (schemaInfo == null)
            return false;
            
        // Get current applied schemas
        var appliedSchemas = GetMetadata<string[]>("apiSchemas")?.ToList() ?? new List<string>();
        var schemaName = schemaInfo.Identifier.GetText();
        
        string fullSchemaName;
        if (schemaInfo.Kind == UsdSchemaKind.MultipleApplyAPI && !string.IsNullOrEmpty(instanceName))
        {
            fullSchemaName = $"{schemaName}:{instanceName}";
        }
        else
        {
            fullSchemaName = schemaName;
        }
        
        // Add if not already present
        if (!appliedSchemas.Contains(fullSchemaName))
        {
            appliedSchemas.Add(fullSchemaName);
            SetMetadata("apiSchemas", appliedSchemas.ToArray());
        }
        
        return true;
    }
    
    /// <summary>
    /// Remove an applied API schema from this prim.
    /// </summary>
    public bool RemoveAPI<T>() where T : UsdAPISchemaBase
    {
        return RemoveAPI(typeof(T));
    }
    
    /// <summary>
    /// Remove an applied API schema from this prim with instance name.
    /// </summary>
    public bool RemoveAPI<T>(string instanceName) where T : UsdAPISchemaBase
    {
        return RemoveAPI(typeof(T), instanceName);
    }
    
    /// <summary>
    /// Remove an applied API schema from this prim.
    /// </summary>
    public bool RemoveAPI(Type apiSchemaType)
    {
        return RemoveAPI(apiSchemaType, string.Empty);
    }
    
    /// <summary>
    /// Remove an applied API schema from this prim with instance name.
    /// </summary>
    public bool RemoveAPI(Type apiSchemaType, string instanceName)
    {
        if (!IsValid() || apiSchemaType == null)
            return false;
            
        var registry = UsdSchemaRegistry.Instance;
        var schemaInfo = registry.FindSchemaInfo(apiSchemaType);
        
        if (schemaInfo == null)
            return false;
            
        // Get current applied schemas
        var appliedSchemas = GetMetadata<string[]>("apiSchemas")?.ToList() ?? new List<string>();
        var schemaName = schemaInfo.Identifier.GetText();
        
        string fullSchemaName;
        if (schemaInfo.Kind == UsdSchemaKind.MultipleApplyAPI && !string.IsNullOrEmpty(instanceName))
        {
            fullSchemaName = $"{schemaName}:{instanceName}";
        }
        else
        {
            fullSchemaName = schemaName;
        }
        
        // Remove if present
        if (appliedSchemas.Remove(fullSchemaName))
        {
            SetMetadata("apiSchemas", appliedSchemas.ToArray());
            return true;
        }
        
        return false;
    }
    
    #endregion

    #region Instance and Prototype Handling
    
    /// <summary>
    /// Return true if this prim can be instanced.
    /// </summary>
    public bool IsInstanceable() => Instanceable;
    
    /// <summary>
    /// Set whether this prim can be instanced.
    /// </summary>
    public bool SetInstanceable(bool instanceable)
    {
        Instanceable = instanceable;
        return IsValid();
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
        return new UsdReferences(this);
    }
    
    /// <summary>
    /// Return this prim's payloads.
    /// </summary>
    public UsdPayloads GetPayloads()
    {
        return new UsdPayloads(this);
    }
    
    /// <summary>
    /// Return this prim's inherits.
    /// </summary>
    public UsdInherits GetInherits()
    {
        return new UsdInherits(this);
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
        return new UsdVariantSets(this);
    }
    
    /// <summary>
    /// Return a specific variant set by name.
    /// </summary>
    public UsdVariantSet GetVariantSet(string variantSetName)
    {
        return GetVariantSets().GetVariantSet(variantSetName);
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