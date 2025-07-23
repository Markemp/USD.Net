using System.Collections.Concurrent;
using System.Reflection;

namespace Pxr.Base.Tf;

/// <summary>
/// TfType represents a dynamic runtime type.
/// </summary>
public sealed class TfType : ITfType, IComparable<TfType>, IEquatable<TfType>
{
    #region Internal Type Storage

    internal class TypeInfo
    {
        public string TypeName { get; set; } = string.Empty;
        public Type? CSharpType { get; set; }
        public List<TfType> BaseTypes { get; } = [];
        public List<TfType> DerivedTypes { get; } = [];
        public Dictionary<TfType, List<string>> DerivedAliases { get; } = [];
        public Dictionary<string, TfType> AliasToType { get; } = [];
        public FactoryBase? Factory { get; set; }
        public DefinitionCallback? DefinitionCallback { get; set; }
        public bool IsEnum { get; set; }
        public bool IsPod { get; set; }
        public int SizeOf { get; set; }
        public TfType CanonicalType { get; set; } = null!;

        // Cached lookup for performance
        public Dictionary<string, TfType> DerivedByNameCache { get; } = [];
    }

    #endregion

    #region Type Registry (Singleton)

    internal class TypeRegistry
    {
        private static readonly Lazy<TypeRegistry> _instance = new(() => new TypeRegistry());
        public static TypeRegistry Instance => _instance.Value;

        private readonly ConcurrentDictionary<string, TypeInfo> _typesByName = new();
        private readonly ConcurrentDictionary<Type, TypeInfo> _typesByCSharpType = new();
        private readonly ReaderWriterLockSlim _lock = new();

        private readonly TypeInfo _unknownType;
        private readonly TypeInfo _rootType;

        private TypeRegistry()
        {
            // Initialize unknown type
            _unknownType = new TypeInfo
            {
                TypeName = "TfType::_Unknown",
                CSharpType = typeof(UnknownType)
            };
            _unknownType.CanonicalType = new TfType(_unknownType);

            // Initialize root type
            _rootType = new TypeInfo
            {
                TypeName = "TfType::_Root",
                CSharpType = typeof(object)
            };
            _rootType.CanonicalType = new TfType(_rootType);

            _typesByName[_unknownType.TypeName] = _unknownType;
            _typesByName[_rootType.TypeName] = _rootType;
            _typesByCSharpType[typeof(UnknownType)] = _unknownType;
            _typesByCSharpType[typeof(object)] = _rootType;

            // Register fundamental types
            RegisterFundamentalTypes();
        }

        private void RegisterFundamentalTypes()
        {
            // Register common built-in types
            TfType.Define<bool>();
            TfType.Define<byte>();
            TfType.Define<sbyte>();
            TfType.Define<char>();
            TfType.Define<short>();
            TfType.Define<ushort>();
            TfType.Define<int>();
            TfType.Define<uint>();
            TfType.Define<long>();
            TfType.Define<ulong>();
            TfType.Define<float>();
            TfType.Define<double>();
            TfType.Define<decimal>();
            TfType.Define<string>();

            // Register common collection types
            TfType.Define<List<bool>>();
            TfType.Define<List<int>>();
            TfType.Define<List<float>>();
            TfType.Define<List<double>>();
            TfType.Define<List<string>>();
        }

        public TypeInfo GetUnknownType() => _unknownType;
        public TypeInfo GetRootType() => _rootType;

        public TypeInfo? FindByName(string name)
        {
            _lock.EnterReadLock();
            try
            {
                return _typesByName.TryGetValue(name, out var info) ? info : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TypeInfo? FindByType(Type type)
        {
            _lock.EnterReadLock();
            try
            {
                return _typesByCSharpType.TryGetValue(type, out var info) ? info : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public TypeInfo DeclareType(string typeName, List<TfType>? bases = null, DefinitionCallback? callback = null)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_typesByName.TryGetValue(typeName, out var existing))
                {
                    // Update existing type with new base information
                    if (bases != null)
                    {
                        foreach (var baseType in bases)
                        {
                            if (!existing.BaseTypes.Contains(baseType))
                            {
                                existing.BaseTypes.Add(baseType);
                                baseType._info.DerivedTypes.Add(existing.CanonicalType);
                            }
                        }
                    }

                    if (callback != null && existing.DefinitionCallback == null)
                    {
                        existing.DefinitionCallback = callback;
                    }

                    return existing;
                }

                var info = new TypeInfo { TypeName = typeName };
                info.CanonicalType = new TfType(info);

                if (bases == null || bases.Count == 0)
                {
                    // Inherit from root if no bases specified
                    info.BaseTypes.Add(new TfType(_rootType));
                    _rootType.DerivedTypes.Add(info.CanonicalType);
                }
                else
                {
                    foreach (var baseType in bases)
                    {
                        info.BaseTypes.Add(baseType);
                        baseType._info.DerivedTypes.Add(info.CanonicalType);
                    }
                }

                info.DefinitionCallback = callback;
                _typesByName[typeName] = info;

                return info;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void DefineType(TypeInfo info, Type csharpType)
        {
            _lock.EnterWriteLock();
            try
            {
                info.CSharpType = csharpType;
                info.IsEnum = csharpType.IsEnum;
                info.IsPod = IsPrimitiveOrStruct(csharpType);
                info.SizeOf = GetSizeOf(csharpType);
                _typesByCSharpType[csharpType] = info;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private static bool IsPrimitiveOrStruct(Type type)
        {
            return type.IsPrimitive ||
                   (type.IsValueType && !type.IsEnum &&
                    type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .All(f => IsPrimitiveOrStruct(f.FieldType)));
        }

        private static int GetSizeOf(Type type)
        {
            if (type.IsValueType)
            {
                return System.Runtime.InteropServices.Marshal.SizeOf(type);
            }
            return IntPtr.Size; // Reference type size
        }

        public void AddAlias(TfType derivedType, TfType baseType, string name)
        {
            _lock.EnterWriteLock();
            try
            {
                // Check for conflicts
                if (baseType._info.AliasToType.ContainsKey(name))
                {
                    if (baseType._info.AliasToType[name] != derivedType)
                        throw new InvalidOperationException($"Alias '{name}' already exists under base type '{baseType.TypeName}'");
                    return; // Already set
                }

                // Check if name conflicts with existing type names
                var existing = FindByName(name);
                if (existing != null && existing.CanonicalType.IsA(baseType))
                {
                    throw new InvalidOperationException($"A type named '{name}' already exists derived from '{baseType.TypeName}'");
                }

                // Add the alias
                baseType._info.AliasToType[name] = derivedType;

                if (!baseType._info.DerivedAliases.ContainsKey(derivedType))
                    baseType._info.DerivedAliases[derivedType] = new List<string>();

                baseType._info.DerivedAliases[derivedType].Add(name);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SetFactory(TypeInfo typeInfo, FactoryBase? factory)
        {
            _lock.EnterWriteLock();
            try
            {
                if (typeInfo.Factory != null)
                    throw new InvalidOperationException($"Factory for {typeInfo.TypeName} has already been set");

                typeInfo.Factory = factory;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    // Marker type for unknown types
    private struct UnknownType { }

    #endregion

    #region Public API

    private readonly TypeInfo _info;

    /// <summary>
    /// Construct a TfType representing an unknown type.
    /// </summary>
    public TfType() : this(TypeRegistry.Instance.GetUnknownType())
    {
    }

    private TfType(TypeInfo info)
    {
        _info = info ?? throw new ArgumentNullException(nameof(info));
    }

    /// <summary>
    /// Callback invoked when a declared type needs to be defined.
    /// </summary>
    public delegate void DefinitionCallback(TfType type);

    /// <summary>
    /// Base class of all factory types.
    /// </summary>
    public abstract class FactoryBase
    {
    }

    /// <summary>
    /// A type-list of C# base types.
    /// </summary>
    public class Bases
    {
        public Type[] Types { get; }

        public Bases(params Type[] types)
        {
            Types = types ?? Array.Empty<Type>();
        }

        public static Bases Create() => new Bases();
        public static Bases Create<T1>() => new Bases(typeof(T1));
        public static Bases Create<T1, T2>() => new Bases(typeof(T1), typeof(T2));
        public static Bases Create<T1, T2, T3>() => new Bases(typeof(T1), typeof(T2), typeof(T3));
        // Add more overloads as needed...
    }

    [Flags]
    public enum LegacyFlags
    {
        Abstract = 0x01,
        Concrete = 0x02,
        Manufacturable = 0x08
    }

    #endregion

    #region Static Methods

    /// <summary>
    /// Return an empty TfType, representing the unknown type.
    /// </summary>
    public static TfType GetUnknownType() => TypeRegistry.Instance.GetUnknownType().CanonicalType;

    /// <summary>
    /// Return the root type of the type hierarchy.
    /// </summary>
    public static TfType GetRoot() => TypeRegistry.Instance.GetRootType().CanonicalType;

    /// <summary>
    /// Retrieve the TfType corresponding to type T.
    /// </summary>
    public static TfType Find<T>() => Find(typeof(T));

    /// <summary>
    /// Retrieve the TfType corresponding to an object with the given Type.
    /// </summary>
    public static TfType Find(Type type)
    {
        if (type is null) return GetUnknownType();

        var info = TypeRegistry.Instance.FindByType(type);
        if (info is not null)
        {
            ExecuteDefinitionCallbackIfNeeded(info);
            return info.CanonicalType;
        }

        // Try to find by canonical name
        var canonicalName = GetCanonicalTypeName(type);
        return FindByName(canonicalName);
    }

    /// <summary>
    /// Retrieve the TfType corresponding to obj.
    /// </summary>
    public static TfType Find<T>(T obj)
    {
        if (obj is null) return GetUnknownType();
        return Find(obj.GetType());
    }

    /// <summary>
    /// Retrieve the TfType corresponding to the given name.
    /// </summary>
    public static TfType FindByName(string name) => GetRoot().FindDerivedByName(name);

    /// <summary>
    /// Return the canonical typeName used for a given Type.
    /// </summary>
    public static string GetCanonicalTypeName(Type type)
    {
        if (type is null) return "Unknown";

        // For generic types, create a readable name
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            var baseName = genericDef.Name.Substring(0, genericDef.Name.IndexOf('`'));
            var args = string.Join(", ", type.GetGenericArguments().Select(GetCanonicalTypeName));
            return $"{baseName}<{args}>";
        }

        // For arrays
        if (type.IsArray)
            return $"{GetCanonicalTypeName(type.GetElementType()!)}[]";

        // For simple types, just use the full name without namespace
        return type.Name;
    }

    /// <summary>
    /// Declare a TfType with the given typeName.
    /// </summary>
    public static TfType Declare(string typeName)
    {
        var info = TypeRegistry.Instance.DeclareType(typeName);
        return info.CanonicalType;
    }

    /// <summary>
    /// Declare a TfType with the given typeName and bases.
    /// </summary>
    public static TfType Declare(string typeName, IList<TfType> bases, DefinitionCallback? definitionCallback = null)
    {
        var info = TypeRegistry.Instance.DeclareType(typeName, bases?.ToList(), definitionCallback);
        return info.CanonicalType;
    }

    /// <summary>
    /// Define a TfType with the given C# type T and no bases.
    /// </summary>
    public static TfType Define<T>() => DefineImpl<T>(new Bases());

    /// <summary>
    /// Define a TfType with the given C# type T and base type B.
    /// </summary>
    public static TfType Define<T, B>() => DefineImpl<T>(Bases.Create<B>());

    private static TfType DefineImpl<T>(Bases bases)
    {
        var type = typeof(T);
        var typeName = GetCanonicalTypeName(type);

        // Convert base types
        var baseTfTypes = new List<TfType>();
        foreach (var baseType in bases.Types)
        {
            baseTfTypes.Add(Declare(GetCanonicalTypeName(baseType)));
        }

        // Declare the type
        var info = TypeRegistry.Instance.DeclareType(typeName, baseTfTypes);

        // Define it with C# type info
        TypeRegistry.Instance.DefineType(info, type);

        return info.CanonicalType;
    }

    #endregion

    #region Instance Methods

    /// <summary>
    /// Return the machine-independent name for this type.
    /// </summary>
    public string TypeName => _info.TypeName;

    /// <summary>
    /// Return a C# Type for this type.
    /// </summary>
    public Type Typeid => _info.CSharpType ?? typeof(void);

    /// <summary>
    /// Return the canonical type for this type.
    /// </summary>
    public TfType CanonicalType => _info.CanonicalType;

    /// <summary>
    /// Return true if this is the unknown type.
    /// </summary>
    public bool IsUnknown => _info == TypeRegistry.Instance.GetUnknownType();

    /// <summary>
    /// Return true if this is the root type.
    /// </summary>
    public bool IsRoot => _info == TypeRegistry.Instance.GetRootType();

    /// <summary>
    /// Return true if this is an enum type.
    /// </summary>
    public bool IsEnumType
    {
        get
        {
            ExecuteDefinitionCallbackIfNeeded(_info);
            return _info.IsEnum;
        }
    }

    /// <summary>
    /// Return true if this is a plain old data type.
    /// </summary>
    public bool IsPlainOldDataType
    {
        get
        {
            ExecuteDefinitionCallbackIfNeeded(_info);
            return _info.IsPod;
        }
    }

    /// <summary>
    /// Return the size required to hold an instance of this type.
    /// </summary>
    public int Sizeof
    {
        get
        {
            ExecuteDefinitionCallbackIfNeeded(_info);
            return _info.SizeOf;
        }
    }

    /// <summary>
    /// Retrieve the TfType that derives from this type and has the given alias or typename.
    /// </summary>
    public TfType FindDerivedByName(string name)
    {
        if (IsUnknown) return GetUnknownType();

        var registry = TypeRegistry.Instance;

        // Check cache first
        lock (_info.DerivedByNameCache)
        {
            if (_info.DerivedByNameCache.TryGetValue(name, out var cached))
                return cached;
        }

        // Check aliases
        foreach (var kvp in _info.AliasToType)
        {
            if (kvp.Key == name)
            {
                lock (_info.DerivedByNameCache)
                {
                    _info.DerivedByNameCache[name] = kvp.Value;
                }
                return kvp.Value;
            }
        }

        // Check by type name
        var info = registry.FindByName(name);
        if (info != null)
        {
            var candidate = info.CanonicalType;
            if (candidate.IsA(this))
            {
                lock (_info.DerivedByNameCache)
                {
                    _info.DerivedByNameCache[name] = candidate;
                }
                return candidate;
            }
        }

        return GetUnknownType();
    }

    /// <summary>
    /// Returns a list of the aliases registered for the derivedType under this base type.
    /// </summary>
    public IList<string> GetAliases(TfType derivedType)
    {
        if (_info.DerivedAliases.TryGetValue(derivedType, out var aliases))
            return aliases.ToList();
        return [];
    }

    /// <summary>
    /// Return a list of types from which this type was derived.
    /// </summary>
    public IList<TfType> GetBaseTypes()
    {
        return _info.BaseTypes.ToList();
    }

    /// <summary>
    /// Copy the first maxBases base types of this type to outTypes, or all
    /// the base types if this type has maxBases or fewer base types. Return
    /// this type's number of base types.
    /// </summary>
    public int GetNBaseTypes(TfType[] outTypes, int maxBases)
    {
        if (outTypes == null) throw new ArgumentNullException(nameof(outTypes));
        
        var baseTypes = _info.BaseTypes;
        var count = Math.Min(baseTypes.Count, Math.Min(maxBases, outTypes.Length));
        
        for (int i = 0; i < count; i++)
        {
            outTypes[i] = baseTypes[i];
        }
        
        return baseTypes.Count;
    }

    /// <summary>
    /// Return a list of types derived directly from this type.
    /// </summary>
    public IList<TfType> GetDirectlyDerivedTypes()
    {
        return _info.DerivedTypes.ToList();
    }

    /// <summary>
    /// Return the set of all types derived from this type.
    /// </summary>
    public void GetAllDerivedTypes(ISet<TfType> result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        var stack = new Stack<TfType>(_info.DerivedTypes);
        while (stack.Count > 0)
        {
            var derived = stack.Pop();
            if (result.Add(derived))
            {
                foreach (var child in derived._info.DerivedTypes)
                    stack.Push(child);
            }
        }
    }

    /// <summary>
    /// Build a list of all ancestor types inherited by this type.
    /// </summary>
    public void GetAllAncestorTypes(IList<TfType> result)
    {
        if (result == null) throw new ArgumentNullException(nameof(result));

        result.Clear();

        // Use C3 linearization algorithm (simplified)
        var visited = new HashSet<TfType>();
        var queue = new Queue<TfType>();
        queue.Enqueue(this);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (visited.Add(current))
            {
                result.Add(current);
                foreach (var baseType in current._info.BaseTypes)
                    queue.Enqueue(baseType);
            }
        }
    }

    /// <summary>
    /// Return true if this type is the same as or derived from queryType.
    /// </summary>
    public bool IsA(TfType queryType)
    {
        if (queryType is null || queryType.IsUnknown) return false;
        if (IsUnknown) return false;
        if (this == queryType || queryType.IsRoot) return true;

        // Check if queryType is in our base types hierarchy
        var toCheck = new Queue<TfType>();
        toCheck.Enqueue(this);
        var visited = new HashSet<TfType>();

        while (toCheck.Count > 0)
        {
            var current = toCheck.Dequeue();
            if (!visited.Add(current)) continue;

            if (current == queryType) return true;

            foreach (var baseType in current._info.BaseTypes)
                toCheck.Enqueue(baseType);
        }

        return false;
    }

    /// <summary>
    /// Return true if this type is the same as or derived from T.
    /// </summary>
    public bool IsA<T>() => IsA(Find<T>());

    /// <summary>
    /// Add an alias name for this type under the given base type.
    /// </summary>
    public void AddAlias(TfType baseType, string name)
    {
        if (baseType is null) throw new ArgumentNullException(nameof(baseType));
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Alias name cannot be empty", nameof(name));

        TypeRegistry.Instance.AddAlias(this, baseType, name);
    }

    /// <summary>
    /// Convenience method to add an alias and return this.
    /// </summary>
    public TfType Alias(TfType baseType, string name)
    {
        AddAlias(baseType, name);
        return this;
    }

    /// <summary>
    /// Sets the factory object for this type.
    /// </summary>
    public void SetFactory(FactoryBase factory)
    {
        if (IsUnknown || IsRoot)
            throw new InvalidOperationException($"Cannot set factory for {TypeName}");

        TypeRegistry.Instance.SetFactory(_info, factory);
    }

    /// <summary>
    /// Returns the factory object for this type.
    /// </summary>
    public T? GetFactory<T>() where T : FactoryBase
    {
        ExecuteDefinitionCallbackIfNeeded(_info);
        return _info.Factory as T;
    }

    /// <summary>
    /// Cast obj to the ancestor type.
    /// In C#, this performs a standard cast operation with type checking.
    /// </summary>
    public object? CastToAncestor(TfType ancestor, object? obj)
    {
        if (ancestor is null) throw new ArgumentNullException(nameof(ancestor));
        if (obj is null) return null;
        
        if (!IsA(ancestor))
            throw new InvalidOperationException($"Type {TypeName} does not derive from {ancestor.TypeName}");
        
        // In C#, we can use the type's C# type to perform casting
        if (ancestor._info.CSharpType != null && ancestor._info.CSharpType.IsAssignableFrom(obj.GetType()))
        {
            return obj;
        }
        
        throw new InvalidCastException($"Cannot cast object of type {obj.GetType()} to {ancestor.TypeName}");
    }

    /// <summary>
    /// Cast obj from the ancestor type to this type.
    /// In C#, this performs a standard cast operation with type checking.
    /// </summary>
    public object? CastFromAncestor(TfType ancestor, object? obj)
    {
        if (ancestor is null) throw new ArgumentNullException(nameof(ancestor));
        if (obj is null) return null;
        
        if (!IsA(ancestor))
            throw new InvalidOperationException($"Type {TypeName} does not derive from {ancestor.TypeName}");
        
        // Verify the object is actually of this type or derived from it
        var objType = Find(obj.GetType());
        if (!objType.IsA(this))
            throw new InvalidCastException($"Object of type {objType.TypeName} is not compatible with {TypeName}");
        
        return obj;
    }

    #endregion

    #region Helper Methods

    private static void ExecuteDefinitionCallbackIfNeeded(TypeInfo info)
    {
        var callback = info.DefinitionCallback;
        if (callback != null)
        {
            // Clear callback first to prevent recursion
            info.DefinitionCallback = null;
            callback(info.CanonicalType);
        }
    }

    #endregion

    #region Equality and Operators

    public override bool Equals(object? obj) => obj is TfType other && Equals(other);

    public bool Equals(TfType? other)
    {
        if (other is null) return false;
        return ReferenceEquals(_info, other._info);
    }

    public override int GetHashCode() => _info?.GetHashCode() ?? 0;

    public static bool operator ==(TfType? left, TfType? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(TfType? left, TfType? right) => !(left == right);

    public int CompareTo(TfType? other)
    {
        if (other is null) return 1;
        return string.Compare(TypeName, other.TypeName, StringComparison.Ordinal);
    }

    public static implicit operator bool(TfType? type) => type is not null && !type.IsUnknown;

    public override string ToString() => TypeName;

    #endregion
}