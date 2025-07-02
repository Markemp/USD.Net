using System;
using System.Collections.Generic;
using System.Linq;
using Pxr.Base.Tf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all USD API schema classes.
/// API schemas provide reusable functionality that can be applied to prims.
/// </summary>
[UsdSchema("APISchemaBase", UsdSchemaKind.AbstractBase, IsAbstract = true)]
public abstract class UsdAPISchemaBase : UsdSchemaBase
{
    private readonly TfToken _instanceName;
    
    #region Construction
    
    /// <summary>
    /// Construct an API schema from a prim.
    /// </summary>
    /// <param name="prim">The prim to apply this API schema to</param>
    protected UsdAPISchemaBase(UsdPrim prim) : base(prim)
    {
        _instanceName = TfToken.Empty;
    }
    
    /// <summary>
    /// Construct an API schema from a prim with an instance name (for multiple-apply APIs).
    /// </summary>
    /// <param name="prim">The prim to apply this API schema to</param>
    /// <param name="instanceName">The instance name for multiple-apply APIs</param>
    protected UsdAPISchemaBase(UsdPrim prim, TfToken instanceName) : base(prim)
    {
        _instanceName = instanceName;
    }
    
    /// <summary>
    /// Construct an invalid API schema.
    /// </summary>
    protected UsdAPISchemaBase() : base()
    {
        _instanceName = TfToken.Empty;
    }
    
    #endregion
    
    #region API Schema Properties
    
    /// <summary>
    /// The instance name for multiple-apply API schemas.
    /// Empty for single-apply APIs.
    /// </summary>
    public TfToken InstanceName => _instanceName;
    
    /// <summary>
    /// Whether this is a multiple-apply API schema.
    /// </summary>
    public bool IsMultipleApply => GetSchemaKind() == UsdSchemaKind.MultipleApplyAPI;
    
    /// <summary>
    /// Whether this API schema has been applied to the prim.
    /// </summary>
    public bool IsApplied
    {
        get
        {
            if (!_prim.IsValid())
                return false;
                
            var schemaName = GetSchemaTypeName();
            if (IsMultipleApply && !_instanceName.IsEmpty)
            {
                // For multiple-apply APIs, check for the instance
                return _prim.HasAPI(GetType(), _instanceName.GetText());
            }
            else
            {
                // For single-apply APIs, check without instance name
                return _prim.HasAPI(GetType());
            }
        }
    }
    
    #endregion
    
    #region Schema Application
    
    /// <summary>
    /// Apply this API schema to the prim.
    /// </summary>
    /// <returns>True if successfully applied</returns>
    public virtual bool Apply()
    {
        if (!_prim.IsValid())
            return false;
            
        if (IsMultipleApply && !_instanceName.IsEmpty)
        {
            return _prim.ApplyAPI(GetType(), _instanceName.GetText());
        }
        else
        {
            return _prim.ApplyAPI(GetType());
        }
    }
    
    /// <summary>
    /// Remove this API schema from the prim.
    /// </summary>
    /// <returns>True if successfully removed</returns>
    public virtual bool Remove()
    {
        if (!_prim.IsValid())
            return false;
            
        if (IsMultipleApply && !_instanceName.IsEmpty)
        {
            return _prim.RemoveAPI(GetType(), _instanceName.GetText());
        }
        else
        {
            return _prim.RemoveAPI(GetType());
        }
    }
    
    #endregion
    
    #region Instance Management (Multiple-Apply APIs)
    
    /// <summary>
    /// Get all instance names for this multiple-apply API schema type on the prim.
    /// </summary>
    /// <returns>List of instance names</returns>
    public IEnumerable<TfToken> GetInstanceNames()
    {
        if (!IsMultipleApply || !_prim.IsValid())
            return Enumerable.Empty<TfToken>();
            
        // TODO: Implement when prim metadata system supports API schema lists
        // For now, return empty collection
        return Enumerable.Empty<TfToken>();
    }
    
    /// <summary>
    /// Check if the given instance name is valid for this API schema.
    /// </summary>
    /// <param name="instanceName">The instance name to validate</param>
    /// <returns>True if valid</returns>
    protected virtual bool IsValidInstanceName(TfToken instanceName)
    {
        // Basic validation: non-empty for multiple-apply APIs
        if (IsMultipleApply)
            return !instanceName.IsEmpty;
            
        // Single-apply APIs should not have instance names
        return instanceName.IsEmpty;
    }
    
    #endregion
    
    #region Property Name Helpers
    
    /// <summary>
    /// Get the full property name including instance name prefix for multiple-apply APIs.
    /// </summary>
    /// <param name="baseName">The base property name</param>
    /// <returns>The full property name</returns>
    protected TfToken GetPropertyName(TfToken baseName)
    {
        if (!IsMultipleApply || _instanceName.IsEmpty)
            return baseName;
            
        // For multiple-apply APIs, prefix with instance name
        return new TfToken($"{_instanceName.GetText()}:{baseName.GetText()}");
    }
    
    /// <summary>
    /// Create a prefixed attribute name for multiple-apply APIs.
    /// </summary>
    /// <param name="baseName">The base attribute name</param>
    /// <returns>The prefixed attribute name</returns>
    protected TfToken MakeAttributeName(string baseName)
    {
        return GetPropertyName(new TfToken(baseName));
    }
    
    /// <summary>
    /// Create a prefixed relationship name for multiple-apply APIs.
    /// </summary>
    /// <param name="baseName">The base relationship name</param>
    /// <returns>The prefixed relationship name</returns>
    protected TfToken MakeRelationshipName(string baseName)
    {
        return GetPropertyName(new TfToken(baseName));
    }
    
    #endregion
    
    #region Schema Compatibility
    
    /// <summary>
    /// Check if this API schema is compatible with the held prim.
    /// For API schemas, this checks if the API has been applied.
    /// </summary>
    /// <returns>True if compatible</returns>
    protected override bool IsCompatible()
    {
        return base.IsCompatible() && IsApplied;
    }
    
    #endregion
    
    #region Object Overrides
    
    public override string ToString()
    {
        if (!IsValid)
            return $"Invalid {GetType().Name}";
            
        var path = _prim.GetPath().GetString();
        if (IsMultipleApply && !_instanceName.IsEmpty)
            return $"{GetType().Name}({path}, {_instanceName.GetText()})";
        else
            return $"{GetType().Name}({path})";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is UsdAPISchemaBase other)
            return base.Equals(obj) && _instanceName.Equals(other._instanceName);
        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), _instanceName);
    }
    
    #endregion
}