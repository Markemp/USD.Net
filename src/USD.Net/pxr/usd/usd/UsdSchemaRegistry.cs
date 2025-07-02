using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pxr.Base.Tf;

namespace Pxr.Usd;

/// <summary>
/// Registry for USD schema types and their metadata.
/// Provides discovery, registration, and lookup of schema classes.
/// </summary>
public class UsdSchemaRegistry
{
    private static readonly Lazy<UsdSchemaRegistry> _instance = new(() => new UsdSchemaRegistry());
    
    private readonly ConcurrentDictionary<Type, UsdSchemaInfo> _schemasByType = new();
    private readonly ConcurrentDictionary<TfToken, UsdSchemaInfo> _schemasByIdentifier = new();
    private readonly ConcurrentDictionary<TfToken, UsdSchemaInfo> _schemasByTypeName = new();
    
    #region Singleton Access
    
    /// <summary>
    /// Get the global schema registry instance.
    /// </summary>
    public static UsdSchemaRegistry Instance => _instance.Value;
    
    /// <summary>
    /// Private constructor for singleton pattern.
    /// </summary>
    private UsdSchemaRegistry()
    {
        // Auto-discover schemas from loaded assemblies
        DiscoverSchemas();
    }
    
    #endregion
    
    #region Schema Discovery and Registration
    
    /// <summary>
    /// Discover and register all USD schema classes in loaded assemblies.
    /// </summary>
    public void DiscoverSchemas()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies)
        {
            try
            {
                DiscoverSchemasInAssembly(assembly);
            }
            catch (Exception)
            {
                // Skip assemblies that can't be reflected over
                continue;
            }
        }
    }
    
    /// <summary>
    /// Discover and register USD schema classes in a specific assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan</param>
    public void DiscoverSchemasInAssembly(Assembly assembly)
    {
        var schemaTypes = assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(UsdSchemaBase)) && !type.IsAbstract)
            .Where(type => type.GetCustomAttribute<UsdSchemaAttribute>() != null);
            
        foreach (var type in schemaTypes)
        {
            RegisterSchema(type);
        }
    }
    
    /// <summary>
    /// Register a schema type with the registry.
    /// </summary>
    /// <param name="schemaType">The schema type to register</param>
    public void RegisterSchema(Type schemaType)
    {
        if (schemaType == null)
            throw new ArgumentNullException(nameof(schemaType));
            
        if (!schemaType.IsSubclassOf(typeof(UsdSchemaBase)))
            throw new ArgumentException($"Type {schemaType.Name} is not a USD schema type", nameof(schemaType));
            
        var attribute = schemaType.GetCustomAttribute<UsdSchemaAttribute>();
        if (attribute == null)
            throw new ArgumentException($"Type {schemaType.Name} is missing UsdSchemaAttribute", nameof(schemaType));
            
        var info = new UsdSchemaInfo
        {
            Identifier = new TfToken(attribute.Identifier),
            Type = schemaType,
            Family = string.IsNullOrEmpty(attribute.Family) ? TfToken.Empty : new TfToken(attribute.Family),
            Version = attribute.Version,
            Kind = attribute.Kind,
            IsAbstract = attribute.IsAbstract,
            TypeName = string.IsNullOrEmpty(attribute.TypeName) ? TfToken.Empty : new TfToken(attribute.TypeName)
        };
        
        // Register in all lookup tables
        _schemasByType[schemaType] = info;
        _schemasByIdentifier[info.Identifier] = info;
        
        if (!info.TypeName.IsEmpty)
            _schemasByTypeName[info.TypeName] = info;
    }
    
    #endregion
    
    #region Schema Lookup
    
    /// <summary>
    /// Find schema information by .NET type.
    /// </summary>
    /// <param name="schemaType">The schema type</param>
    /// <returns>Schema information, or null if not found</returns>
    public UsdSchemaInfo? FindSchemaInfo(Type schemaType)
    {
        return _schemasByType.TryGetValue(schemaType, out var info) ? info : null;
    }
    
    /// <summary>
    /// Find schema information by identifier.
    /// </summary>
    /// <param name="identifier">The schema identifier</param>
    /// <returns>Schema information, or null if not found</returns>
    public UsdSchemaInfo? FindSchemaInfo(TfToken identifier)
    {
        return _schemasByIdentifier.TryGetValue(identifier, out var info) ? info : null;
    }
    
    /// <summary>
    /// Find schema information by USD type name.
    /// </summary>
    /// <param name="typeName">The USD type name</param>
    /// <returns>Schema information, or null if not found</returns>
    public UsdSchemaInfo? FindSchemaInfoByTypeName(TfToken typeName)
    {
        return _schemasByTypeName.TryGetValue(typeName, out var info) ? info : null;
    }
    
    /// <summary>
    /// Get the schema identifier for a .NET type.
    /// </summary>
    /// <param name="schemaType">The schema type</param>
    /// <returns>The schema identifier, or empty if not found</returns>
    public TfToken GetSchemaTypeName(Type schemaType)
    {
        var info = FindSchemaInfo(schemaType);
        return info?.Identifier ?? TfToken.Empty;
    }
    
    /// <summary>
    /// Get the .NET type for a schema identifier.
    /// </summary>
    /// <param name="identifier">The schema identifier</param>
    /// <returns>The .NET type, or null if not found</returns>
    public Type? GetTypeFromSchemaTypeName(TfToken identifier)
    {
        var info = FindSchemaInfo(identifier);
        return info?.Type;
    }
    
    /// <summary>
    /// Get the USD type name for a schema type.
    /// </summary>
    /// <param name="schemaType">The schema type</param>
    /// <returns>The USD type name, or empty if not a typed schema</returns>
    public TfToken GetUsdTypeName(Type schemaType)
    {
        var info = FindSchemaInfo(schemaType);
        return info?.TypeName ?? TfToken.Empty;
    }
    
    #endregion
    
    #region Schema Classification
    
    /// <summary>
    /// Get all registered schemas of a specific kind.
    /// </summary>
    /// <param name="kind">The schema kind to filter by</param>
    /// <returns>Collection of matching schema information</returns>
    public IEnumerable<UsdSchemaInfo> GetSchemasByKind(UsdSchemaKind kind)
    {
        return _schemasByType.Values.Where(info => info.Kind == kind);
    }
    
    /// <summary>
    /// Get all registered concrete typed schemas.
    /// </summary>
    /// <returns>Collection of concrete typed schema information</returns>
    public IEnumerable<UsdSchemaInfo> GetConcreteTypedSchemas()
    {
        return GetSchemasByKind(UsdSchemaKind.ConcreteTyped);
    }
    
    /// <summary>
    /// Get all registered API schemas.
    /// </summary>
    /// <returns>Collection of API schema information</returns>
    public IEnumerable<UsdSchemaInfo> GetAPISchemas()
    {
        return _schemasByType.Values.Where(info => 
            info.Kind == UsdSchemaKind.SingleApplyAPI || 
            info.Kind == UsdSchemaKind.MultipleApplyAPI ||
            info.Kind == UsdSchemaKind.NonAppliedAPI);
    }
    
    /// <summary>
    /// Check if a type is a registered USD schema.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if it's a registered schema</returns>
    public bool IsRegisteredSchema(Type type)
    {
        return _schemasByType.ContainsKey(type);
    }
    
    #endregion
    
    #region Schema Validation
    
    /// <summary>
    /// Check if a schema type is compatible with a prim.
    /// </summary>
    /// <param name="schemaType">The schema type</param>
    /// <param name="prim">The prim to check</param>
    /// <returns>True if compatible</returns>
    public bool IsSchemaCompatibleWithPrim(Type schemaType, UsdPrim prim)
    {
        if (!prim.IsValid())
            return false;
            
        var info = FindSchemaInfo(schemaType);
        if (info == null)
            return false;
            
        switch (info.Kind)
        {
            case UsdSchemaKind.ConcreteTyped:
            case UsdSchemaKind.AbstractTyped:
                // For typed schemas, check type name
                return !info.TypeName.IsEmpty && prim.GetTypeName() == info.TypeName.GetText();
                
            case UsdSchemaKind.SingleApplyAPI:
            case UsdSchemaKind.MultipleApplyAPI:
            case UsdSchemaKind.NonAppliedAPI:
                // For API schemas, check if applied (would need prim metadata support)
                // For now, assume compatible if prim is valid
                return true;
                
            default:
                return false;
        }
    }
    
    #endregion
    
    #region Debug and Diagnostics
    
    /// <summary>
    /// Get all registered schema identifiers.
    /// </summary>
    /// <returns>Collection of all schema identifiers</returns>
    public IEnumerable<TfToken> GetAllSchemaIdentifiers()
    {
        return _schemasByIdentifier.Keys;
    }
    
    /// <summary>
    /// Get count of registered schemas by kind.
    /// </summary>
    /// <returns>Dictionary mapping schema kinds to counts</returns>
    public Dictionary<UsdSchemaKind, int> GetSchemaCountsByKind()
    {
        return _schemasByType.Values
            .GroupBy(info => info.Kind)
            .ToDictionary(group => group.Key, group => group.Count());
    }
    
    /// <summary>
    /// Clear all registered schemas (for testing).
    /// </summary>
    internal void Clear()
    {
        _schemasByType.Clear();
        _schemasByIdentifier.Clear();
        _schemasByTypeName.Clear();
    }
    
    #endregion
}