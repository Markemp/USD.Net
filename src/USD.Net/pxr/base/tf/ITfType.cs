namespace Pxr.Base.Tf;

// AIDEV-NOTE: Interface based on OpenUSD type.h - DO NOT MODIFY WITHOUT PERMISSION
/// <summary>
/// TfType represents a dynamic runtime type.
/// </summary>
/// <remarks>
/// TfTypes are created and discovered at runtime, rather than compile time.
/// Features:
/// - unique typename
/// - safe across DSO boundaries
/// - can represent C++ types, pure Python types, or Python subclasses of wrapped C++ types
/// - lightweight value semantics -- you can copy and default construct TfType, unlike std::type_info
/// - totally ordered -- can use as a std::map key
/// </remarks>
public interface ITfType : IComparable<TfType>, IEquatable<TfType>
{
    /// <summary>
    /// Return the machine-independent name for this type.
    /// This name is specified when the TfType is declared.
    /// </summary>
    string TypeName { get; }
    
    /// <summary>
    /// Return a C# Type for this type.
    /// </summary>
    /// <remarks>
    /// If this type is unknown, this will return a unique type specifically for the unknown type.
    /// If this type has been declared, but not yet had a C# type defined, typeof(void) will be returned.
    /// </remarks>
    Type Typeid { get; }
    
    /// <summary>
    /// Return the canonical type for this type.
    /// </summary>
    TfType CanonicalType { get; }
    
    /// <summary>
    /// Return true if this is the unknown type, representing a type unknown to the TfType system.
    /// </summary>
    /// <remarks>
    /// The unknown type does not derive from the root type, or any other type.
    /// </remarks>
    bool IsUnknown { get; }
    
    /// <summary>
    /// Return true if this is the root type.
    /// </summary>
    bool IsRoot { get; }
    
    /// <summary>
    /// Return true if this is an enum type.
    /// </summary>
    bool IsEnumType { get; }
    
    /// <summary>
    /// Return true if this is a plain old data type, as defined by C++.
    /// </summary>
    bool IsPlainOldDataType { get; }
    
    /// <summary>
    /// Return the size required to hold an instance of this type on the stack
    /// (does not include any heap allocated memory the instance uses).
    /// </summary>
    /// <remarks>
    /// This is what the C++ sizeof operator returns for the type, so this
    /// value is not very useful for Python types (it will always be sizeof(boost::python::object)).
    /// </remarks>
    int Sizeof { get; }

    /// <summary>
    /// Retrieve the TfType that derives from this type and has the given alias or typename.
    /// </summary>
    TfType FindDerivedByName(string name);
    
    /// <summary>
    /// Returns a vector of the aliases registered for the derivedType under this, the base type.
    /// </summary>
    IList<string> GetAliases(TfType derivedType);
    
    /// <summary>
    /// Return a vector of types from which this type was derived.
    /// </summary>
    IList<TfType> GetBaseTypes();
    
    /// <summary>
    /// Copy the first maxBases base types of this type to outTypes, or all
    /// the base types if this type has maxBases or fewer base types. Return
    /// this type's number of base types.
    /// </summary>
    /// <remarks>
    /// Note that it is supported to change a TfType to its first base type by calling this function.
    /// </remarks>
    int GetNBaseTypes(TfType[] outTypes, int maxBases);
    
    /// <summary>
    /// Return a vector of types derived directly from this type.
    /// </summary>
    IList<TfType> GetDirectlyDerivedTypes();
    
    /// <summary>
    /// Return the set of all types derived (directly or indirectly) from this type.
    /// </summary>
    void GetAllDerivedTypes(ISet<TfType> result);
    
    /// <summary>
    /// Build a vector of all ancestor types inherited by this type.
    /// The starting type is itself included, as the first element of the results vector.
    /// </summary>
    /// <remarks>
    /// Types are given in "C3" resolution order, as used for new-style
    /// classes starting in Python 2.3. This algorithm is more complicated
    /// than a simple depth-first traversal of base classes, in order to
    /// prevent some subtle errors with multiple-inheritance.
    /// Note: This can be expensive; consider caching the results. TfType
    /// does not cache this itself since it is not needed internally.
    /// </remarks>
    void GetAllAncestorTypes(IList<TfType> result);
    
    /// <summary>
    /// Return true if this type is the same as or derived from queryType.
    /// If queryType is unknown, this always returns false.
    /// </summary>
    bool IsA(TfType queryType);
    
    /// <summary>
    /// Return true if this type is the same as or derived from T.
    /// This is equivalent to: IsA(Find&lt;T&gt;())
    /// </summary>
    bool IsA<T>();
    
    /// <summary>
    /// Add an alias name for this type under the given base type.
    /// </summary>
    /// <remarks>
    /// Aliases are similar to typedefs in C++: they provide an
    /// alternate name for a type. The alias is defined with respect
    /// to the given base type. Aliases must be unique with respect to both
    /// other aliases beneath that base type and names of derived types of that base.
    /// </remarks>
    void AddAlias(TfType baseType, string name);
    
    /// <summary>
    /// Convenience method to add an alias and return this.
    /// </summary>
    TfType Alias(TfType baseType, string name);
    
    /// <summary>
    /// Cast obj to the address corresponding to the type ancestor.
    /// </summary>
    /// <remarks>
    /// (This is a dangerous function; there's probably a much better way to
    /// do whatever it is you're trying to do.)
    /// With multiple inheritance, you can't do a reinterpret_cast back to an
    /// ancestor type; this function figures out how to cast obj to the
    /// address corresponding to the type ancestor if in fact ancestor is
    /// really an ancestor of the type corresponding to this.
    /// </remarks>
    object? CastToAncestor(TfType ancestor, object? obj);
    
    /// <summary>
    /// Cast obj, which pointed to the ancestor type ancestor, to the type of this.
    /// </summary>
    /// <remarks>
    /// This function is the opposite of CastToAncestor(); the assumption
    /// is that obj was a pointer to the type corresponding to ancestor,
    /// and was then reinterpret-cast to object, but now you wish
    /// to cast the pointer to the type corresponding to this. While
    /// the fact that obj was a pointer of type ancestor is taken on
    /// faith, a runtime check is performed to verify that the underlying
    /// object pointed to by obj is of type this (or derived from this).
    /// </remarks>
    object? CastFromAncestor(TfType ancestor, object? obj);
    
    /// <summary>
    /// Sets the factory object for this type. A type's factory typically
    /// has methods to instantiate the type given various arguments and must
    /// inherit from FactoryBase. The factory cannot be changed once set.
    /// </summary>
    void SetFactory(TfType.FactoryBase factory);
    
    /// <summary>
    /// Returns the factory object for this type as a T, or null if
    /// there is no factory or the factory is not or is not derived from T.
    /// Clients can check if a factory is set using GetFactory&lt;TfType.FactoryBase&gt;().
    /// </summary>
    T? GetFactory<T>() where T : TfType.FactoryBase;
}