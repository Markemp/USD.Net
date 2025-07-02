using System;
using Pxr.Base.Tf;
using Pxr.Usd.Sdf;

namespace Pxr.Usd;

/// <summary>
/// Base class for all typed USD schemas.
/// Typed schemas can provide a typeName to prims and represent "IsA" relationships.
/// </summary>
[UsdSchema("Typed", UsdSchemaKind.AbstractBase, IsAbstract = true)]
public abstract class UsdTyped : UsdSchemaBase
{
    #region Construction
    
    /// <summary>
    /// Construct a typed schema from a prim.
    /// </summary>
    /// <param name="prim">The prim to wrap with this schema</param>
    protected UsdTyped(UsdPrim prim) : base(prim)
    {
    }
    
    /// <summary>
    /// Construct an invalid typed schema.
    /// </summary>
    protected UsdTyped() : base()
    {
    }
    
    #endregion
    
    #region Schema Information
    
    /// <summary>
    /// Get the type name that this schema represents.
    /// Must be implemented by derived classes.
    /// </summary>
    /// <returns>The USD type name for this schema</returns>
    protected abstract TfToken GetTypeName();
    
    /// <summary>
    /// Check if this typed schema is compatible with the held prim.
    /// For typed schemas, this checks if the prim's type matches this schema's type.
    /// </summary>
    /// <returns>True if compatible</returns>
    protected override bool IsCompatible()
    {
        if (!base.IsCompatible())
            return false;
            
        var expectedType = GetTypeName();
        var primType = _prim.GetTypeName();
        
        // Check exact type match or inheritance
        return primType == expectedType.GetText() || IsA(expectedType);
    }
    
    /// <summary>
    /// Check if the held prim is of the given type or inherits from it.
    /// </summary>
    /// <param name="typeName">The type name to check</param>
    /// <returns>True if the prim is of the given type</returns>
    protected bool IsA(TfToken typeName)
    {
        if (!_prim.IsValid())
            return false;
            
        // For now, simple string comparison
        // TODO: Implement proper type hierarchy checking when schema registry is available
        return _prim.GetTypeName() == typeName.GetText();
    }
    
    #endregion
    
    #region Static Factory Methods
    
    /// <summary>
    /// Get a typed schema object from a stage and path.
    /// This is a template method that derived classes should specialize.
    /// </summary>
    /// <typeparam name="T">The typed schema type</typeparam>
    /// <param name="stage">The stage containing the prim</param>
    /// <param name="path">The path to the prim</param>
    /// <returns>The typed schema object, or invalid if not compatible</returns>
    protected static T Get<T>(UsdStage stage, SdfPath path) where T : UsdTyped, new()
    {
        if (stage == null)
            return new T();
            
        var prim = stage.GetPrimAtPath(path);
        if (!prim.IsValid())
            return new T();
            
        var schema = (T)Activator.CreateInstance(typeof(T), prim)!;
        
        // Check compatibility
        if (!schema.IsCompatible())
            return new T(); // Return invalid schema
            
        return schema;
    }
    
    /// <summary>
    /// Define a typed prim at the given stage and path.
    /// This creates the prim and sets its type name.
    /// </summary>
    /// <typeparam name="T">The typed schema type</typeparam>
    /// <param name="stage">The stage to create the prim on</param>
    /// <param name="path">The path for the new prim</param>
    /// <returns>The typed schema object for the new prim</returns>
    protected static T Define<T>(UsdStage stage, SdfPath path) where T : UsdTyped, new()
    {
        if (stage == null)
            return new T();
            
        // Create a temporary instance to get the type name
        var tempSchema = new T();
        var typeName = tempSchema.GetTypeName();
        
        // Define the prim with the correct type
        var prim = stage.DefinePrim(path, new TfToken(typeName.GetText()));
        if (!prim.IsValid())
            return new T();
            
        // Return the schema object
        return (T)Activator.CreateInstance(typeof(T), prim)!;
    }
    
    #endregion
    
    #region Type Information
    
    /// <summary>
    /// Get the USD type name for this prim.
    /// </summary>
    /// <returns>The type name from the prim</returns>
    public string GetPrimTypeName()
    {
        return _prim.GetTypeName();
    }
    
    /// <summary>
    /// Set the type name for this prim.
    /// </summary>
    /// <param name="typeName">The type name to set</param>
    /// <returns>True if successful</returns>
    public bool SetTypeName(string typeName)
    {
        return _prim.SetTypeName(typeName);
    }
    
    /// <summary>
    /// Clear the type name for this prim.
    /// </summary>
    /// <returns>True if successful</returns>
    public bool ClearTypeName()
    {
        return _prim.ClearTypeName();
    }
    
    /// <summary>
    /// Check if this prim has a type name.
    /// </summary>
    /// <returns>True if the prim has a type name</returns>
    public bool HasTypeName()
    {
        return _prim.HasTypeName();
    }
    
    #endregion
    
    #region Schema Kind Override
    
    /// <summary>
    /// Typed schemas can be either abstract or concrete.
    /// Derived classes should override this to specify their kind.
    /// </summary>
    /// <returns>The schema kind (AbstractTyped or ConcreteTyped)</returns>
    protected override UsdSchemaKind GetSchemaKind()
    {
        // Default to concrete typed, derived classes can override for abstract
        return UsdSchemaKind.ConcreteTyped;
    }
    
    #endregion
}