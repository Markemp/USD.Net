namespace Pxr.Base.Tf
{
    /// <summary>
    /// TfType represents a dynamic runtime type.
    /// 
    /// TfTypes are created and discovered at runtime, rather than compile time.
    /// 
    /// Features:
    /// - unique typename
    /// - safe across DSO boundaries
    /// - can represent C++ types, pure Python types, or Python subclasses of wrapped C++ types
    /// - lightweight value semantics -- you can copy and default construct TfType
    /// - totally ordered -- can use as a dictionary key
    /// </summary>
    public class TfType : IComparable<TfType>, IEquatable<TfType>
    {
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

        [Flags]
        public enum LegacyFlags
        {
            /// <summary>Abstract (unmanufacturable and unclonable)</summary>
            Abstract = 0x01,
            /// <summary>Not abstract</summary>
            Concrete = 0x02,
            /// <summary>Manufacturable type (implies concrete)</summary>
            Manufacturable = 0x08
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
            public static Bases Create<T1, T2, T3, T4>() => new Bases(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            public static Bases Create<T1, T2, T3, T4, T5>() => new Bases(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            public static Bases Create<T1, T2, T3, T4, T5, T6>() => new Bases(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
            public static Bases Create<T1, T2, T3, T4, T5, T6, T7>() => new Bases(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
            public static Bases Create<T1, T2, T3, T4, T5, T6, T7, T8>() => new Bases(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        }

        private object _info; // Internal type representation

        /// <summary>
        /// Construct a TfType representing an unknown type.
        /// </summary>
        public TfType()
        {
            _info = GetUnknownTypeInfo();
        }

        private TfType(object info)
        {
            _info = info;
        }

        /// <summary>
        /// Return an empty TfType, representing the unknown type.
        /// </summary>
        public static TfType GetUnknownType()
        {
            return new TfType();
        }

        /// <summary>
        /// Return the root type of the type hierarchy.
        /// </summary>
        public static TfType GetRoot()
        {
            return GetRootTypeImpl();
        }

        #region Equality and Comparison

        public override bool Equals(object? obj)
        {
            return obj is TfType other && Equals(other);
        }

        public bool Equals(TfType? other)
        {
            if (other is null) return false;
            return ReferenceEquals(_info, other._info);
        }

        public override int GetHashCode()
        {
            return _info?.GetHashCode() ?? 0;
        }

        public static bool operator ==(TfType left, TfType right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(TfType left, TfType right)
        {
            return !(left == right);
        }

        public int CompareTo(TfType? other)
        {
            if (other is null) return 1;
            return Comparer<object>.Default.Compare(_info, other._info);
        }

        public static bool operator <(TfType left, TfType right)
        {
            if (left is null) return right is not null;
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(TfType left, TfType right)
        {
            if (right is null) return left is not null;
            return right.CompareTo(left) < 0;
        }

        public static bool operator <=(TfType left, TfType right)
        {
            return !(left > right);
        }

        public static bool operator >=(TfType left, TfType right)
        {
            return !(left < right);
        }

        #endregion

        #region Finding Types

        /// <summary>
        /// Retrieve the TfType corresponding to type T.
        /// </summary>
        public static TfType Find<T>()
        {
            return Find(typeof(T));
        }

        /// <summary>
        /// Retrieve the TfType corresponding to obj.
        /// </summary>
        public static TfType Find<T>(T obj)
        {
            if (obj == null) return GetUnknownType();
            return FindByObjectImpl(obj);
        }

        /// <summary>
        /// Retrieve the TfType corresponding to an object with the given Type.
        /// </summary>
        public static TfType Find(Type type)
        {
            return FindByTypeImpl(type);
        }

        /// <summary>
        /// Retrieve the TfType corresponding to an object with the given Type.
        /// </summary>
        public static TfType FindByTypeid(Type type)
        {
            return Find(type);
        }

        /// <summary>
        /// Retrieve the TfType corresponding to the given name.
        /// </summary>
        public static TfType FindByName(string name)
        {
            return GetRoot().FindDerivedByName(name);
        }

        /// <summary>
        /// Retrieve the TfType that derives from this type and has the given alias or typename.
        /// </summary>
        public TfType FindDerivedByName(string name)
        {
            return FindDerivedByNameImpl(name);
        }

        /// <summary>
        /// Retrieve the TfType that derives from BASE and has the given alias or typename.
        /// </summary>
        public static TfType FindDerivedByName<TBase>(string name)
        {
            return Find<TBase>().FindDerivedByName(name);
        }

        #endregion

        #region Type Queries

        /// <summary>
        /// Return the machine-independent name for this type.
        /// </summary>
        public string TypeName => GetTypeNameImpl();

        /// <summary>
        /// Return a C# Type for this type.
        /// </summary>
        public Type Typeid => GetTypeidImpl();

        /// <summary>
        /// Return the canonical typeName used for a given Type.
        /// </summary>
        public static string GetCanonicalTypeName(Type type)
        {
            return GetCanonicalTypeNameImpl(type);
        }

        /// <summary>
        /// Return the canonical type for this type.
        /// </summary>
        public TfType CanonicalType => GetCanonicalTypeImpl();

        /// <summary>
        /// Returns a list of the aliases registered for the derivedType under this, the base type.
        /// </summary>
        public IList<string> GetAliases(TfType derivedType)
        {
            return GetAliasesImpl(derivedType);
        }

        /// <summary>
        /// Return a list of types from which this type was derived.
        /// </summary>
        public IList<TfType> GetBaseTypes()
        {
            return GetBaseTypesImpl();
        }

        /// <summary>
        /// Copy the first maxBases base types of this type to out, or all the base types if this type has maxBases or fewer base types.
        /// Returns this type's number of base types.
        /// </summary>
        public int GetNBaseTypes(TfType[] output, int maxBases)
        {
            return GetNBaseTypesImpl(output, maxBases);
        }

        /// <summary>
        /// Return a list of types derived directly from this type.
        /// </summary>
        public IList<TfType> GetDirectlyDerivedTypes()
        {
            return GetDirectlyDerivedTypesImpl();
        }

        /// <summary>
        /// Return the set of all types derived (directly or indirectly) from this type.
        /// </summary>
        public void GetAllDerivedTypes(ISet<TfType> result)
        {
            GetAllDerivedTypesImpl(result);
        }

        /// <summary>
        /// Build a list of all ancestor types inherited by this type.
        /// </summary>
        public void GetAllAncestorTypes(IList<TfType> result)
        {
            GetAllAncestorTypesImpl(result);
        }

        /// <summary>
        /// Return true if this type is the same as or derived from queryType.
        /// </summary>
        public bool IsA(TfType queryType)
        {
            return IsAImpl(queryType);
        }

        /// <summary>
        /// Return true if this type is the same as or derived from T.
        /// </summary>
        public bool IsA<T>()
        {
            return IsA(Find<T>());
        }

        /// <summary>
        /// Return true if this is the unknown type.
        /// </summary>
        public bool IsUnknown => this == GetUnknownType();

        /// <summary>
        /// Convert to bool -- return true if this type is not unknown.
        /// </summary>
        public static implicit operator bool(TfType type)
        {
            return type != null && !type.IsUnknown;
        }

        /// <summary>
        /// Return true if this is the root type.
        /// </summary>
        public bool IsRoot => this == GetRoot();

        /// <summary>
        /// Return true if this is an enum type.
        /// </summary>
        public bool IsEnumType => IsEnumTypeImpl();

        /// <summary>
        /// Return true if this is a plain old data type.
        /// </summary>
        public bool IsPlainOldDataType => IsPlainOldDataTypeImpl();

        /// <summary>
        /// Return the size required to hold an instance of this type on the stack.
        /// </summary>
        public int Sizeof => GetSizeofImpl();

        #endregion

        #region Registering New Types

        /// <summary>
        /// Declare a TfType with the given typeName, but no base type information.
        /// </summary>
        public static TfType Declare(string typeName)
        {
            return DeclareImpl(typeName);
        }

        /// <summary>
        /// Declare a TfType with the given typeName and bases.
        /// </summary>
        public static TfType Declare(string typeName, IList<TfType> bases, DefinitionCallback definitionCallback = null)
        {
            return DeclareImpl(typeName, bases, definitionCallback);
        }

        /// <summary>
        /// Declares a TfType with the given C# type T and C# base types.
        /// </summary>
        public static TfType Declare<T, TBases>() where TBases : new()
        {
            return DeclareGenericImpl<T, TBases>();
        }

        /// <summary>
        /// Define a TfType with the given C# type T and C# base types B.
        /// </summary>
        public static TfType Define<T, B>()
        {
            return DefineGenericImpl<T, B>();
        }

        /// <summary>
        /// Define a TfType with the given C# type T and no bases.
        /// </summary>
        public static TfType Define<T>()
        {
            return DefineGenericImpl<T, Bases>();
        }

        /// <summary>
        /// Add an alias name for this type under the given base type.
        /// </summary>
        public void AddAlias(TfType baseType, string name)
        {
            AddAliasImpl(baseType, name);
        }

        /// <summary>
        /// Add an alias for DERIVED beneath BASE.
        /// </summary>
        public static void AddAlias<TBase, TDerived>(string name)
        {
            var b = Declare(GetCanonicalTypeName(typeof(TBase)));
            var d = Declare(GetCanonicalTypeName(typeof(TDerived)));
            d.AddAlias(b, name);
        }

        /// <summary>
        /// Convenience method to add an alias and return this.
        /// </summary>
        public TfType Alias(TfType baseType, string name)
        {
            AddAlias(baseType, name);
            return this;
        }

        #endregion

        #region Pointer Casts

        /// <summary>
        /// Cast addr to the address corresponding to the type ancestor.
        /// </summary>
        public IntPtr CastToAncestor(TfType ancestor, IntPtr addr)
        {
            return CastToAncestorImpl(ancestor, addr);
        }

        /// <summary>
        /// Cast addr, which pointed to the ancestor type ancestor, to the type of this.
        /// </summary>
        public IntPtr CastFromAncestor(TfType ancestor, IntPtr addr)
        {
            return CastFromAncestorImpl(ancestor, addr);
        }

        #endregion

        #region Factory

        /// <summary>
        /// Sets the factory object for this type.
        /// </summary>
        public void SetFactory(FactoryBase factory)
        {
            SetFactoryImpl(factory);
        }

        /// <summary>
        /// Sets the factory object for this type.
        /// </summary>
        public void SetFactory<T>() where T : FactoryBase, new()
        {
            SetFactory(new T());
        }

        /// <summary>
        /// Sets the factory object for this type and returns this.
        /// </summary>
        public TfType Factory(FactoryBase factory)
        {
            SetFactory(factory);
            return this;
        }

        /// <summary>
        /// Sets the factory object for this type to be a T and returns this.
        /// </summary>
        public TfType Factory<T>() where T : FactoryBase, new()
        {
            SetFactory<T>();
            return this;
        }

        /// <summary>
        /// Returns the factory object for this type as a T, or null if there is no factory.
        /// </summary>
        public T? GetFactory<T>() where T : FactoryBase => GetFactoryImpl() as T;

        #endregion

        #region Internal Type Storage

        internal class TypeInfo
        {
            public string TypeName { get; set; }
            public Type CSharpType { get; set; }
            public List<TfType> BaseTypes { get; } = new List<TfType>();
            public List<TfType> DerivedTypes { get; } = new List<TfType>();
            public Dictionary<TfType, List<string>> DerivedAliases { get; } = new Dictionary<TfType, List<string>>();
            public Dictionary<string, TfType> AliasToType { get; } = new Dictionary<string, TfType>();
            public FactoryBase Factory { get; set; }
            public DefinitionCallback DefinitionCallback { get; set; }
            public bool IsEnum { get; set; }
            public bool IsPod { get; set; }
            public int SizeOf { get; set; }
            public TfType CanonicalType { get; set; }

            // Cached lookup for performance
            public Dictionary<string, TfType> DerivedByNameCache { get; } = new Dictionary<string, TfType>();
        }

        #endregion

        #region Private Implementation Methods

        // These would be implemented in the actual library
        private static extern object GetUnknownTypeInfo();
        private static extern TfType GetRootTypeImpl();
        private static extern TfType FindByTypeImpl(Type type);
        private static extern TfType FindByObjectImpl(object obj);
        private extern TfType FindDerivedByNameImpl(string name);
        private extern string GetTypeNameImpl();
        private extern Type GetTypeidImpl();
        private static extern string GetCanonicalTypeNameImpl(Type type);
        private extern TfType GetCanonicalTypeImpl();
        private extern IList<string> GetAliasesImpl(TfType derivedType);
        private extern IList<TfType> GetBaseTypesImpl();
        private extern int GetNBaseTypesImpl(TfType[] output, int maxBases);
        private extern IList<TfType> GetDirectlyDerivedTypesImpl();
        private extern void GetAllDerivedTypesImpl(ISet<TfType> result);
        private extern void GetAllAncestorTypesImpl(IList<TfType> result);
        private extern bool IsAImpl(TfType queryType);
        private extern bool IsEnumTypeImpl();
        private extern bool IsPlainOldDataTypeImpl();
        private extern int GetSizeofImpl();
        private static extern TfType DeclareImpl(string typeName);
        private static extern TfType DeclareImpl(string typeName, IList<TfType> bases, DefinitionCallback callback);
        private static extern TfType DeclareGenericImpl<T>(Bases bases);
        private static extern TfType DefineGenericImpl<T>(Bases bases);
        private extern void AddAliasImpl(TfType baseType, string name);
        private extern IntPtr CastToAncestorImpl(TfType ancestor, IntPtr addr);
        private extern IntPtr CastFromAncestorImpl(TfType ancestor, IntPtr addr);
        private extern void SetFactoryImpl(FactoryBase factory);
        private extern FactoryBase GetFactoryImpl();

        #endregion

        public override string ToString()
        {
            return TypeName;
        }
    }
}