using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pxr.Base.Tf.Tests;

[Trait("Category", "Unit")]
public class TfTypeTests
{
    #region Unknown and Root Type Tests

    [Fact]
    public void GetUnknownType_ReturnsConsistentInstance()
    {
        // Arrange & Act
        var unknown1 = TfType.GetUnknownType();
        var unknown2 = TfType.GetUnknownType();

        // Assert
        Assert.Same(unknown1, unknown2);
        Assert.True(unknown1.IsUnknown);
        Assert.Equal("TfType::_Unknown", unknown1.TypeName);
    }

    [Fact]
    public void GetRoot_ReturnsConsistentInstance()
    {
        // Arrange & Act
        var root1 = TfType.GetRoot();
        var root2 = TfType.GetRoot();

        // Assert
        Assert.Same(root1, root2);
        Assert.True(root1.IsRoot);
        Assert.Equal("TfType::_Root", root1.TypeName);
    }

    [Fact]
    public void DefaultConstructor_ReturnsUnknownType()
    {
        // Arrange & Act
        var type = new TfType();

        // Assert
        Assert.True(type.IsUnknown);
        Assert.Equal(TfType.GetUnknownType(), type);
    }

    #endregion

    #region Type Finding Tests

    [Fact]
    public void Find_ByGenericType_ReturnsConsistentType()
    {
        // Arrange & Act
        var type1 = TfType.Find<string>();
        var type2 = TfType.Find<string>();

        // Assert
        Assert.Same(type1, type2);
        Assert.False(type1.IsUnknown);
        Assert.Equal("String", type1.TypeName);
    }

    [Fact]
    public void Find_BySystemType_ReturnsConsistentType()
    {
        // Arrange & Act
        var type1 = TfType.Find(typeof(int));
        var type2 = TfType.Find(typeof(int));

        // Assert
        Assert.Same(type1, type2);
        Assert.False(type1.IsUnknown);
        Assert.Equal("Int32", type1.TypeName);
    }

    [Fact]
    public void Find_WithNullType_ReturnsUnknownType()
    {
        // Arrange & Act
        var type = TfType.Find((Type)null!);

        // Assert
        Assert.True(type.IsUnknown);
    }

    [Fact]
    public void Find_ByObject_ReturnsCorrectType()
    {
        // Arrange
        string testString = "test";
        int testInt = 42;

        // Act
        var stringType = TfType.Find(testString);
        var intType = TfType.Find(testInt);

        // Assert
        Assert.Equal(TfType.Find<string>(), stringType);
        Assert.Equal(TfType.Find<int>(), intType);
    }

    [Fact]
    public void Find_ByNullObject_ReturnsUnknownType()
    {
        // Arrange & Act
        var type = TfType.Find<object>(null!);

        // Assert
        Assert.True(type.IsUnknown);
    }

    [Fact]
    public void FindByName_ReturnsCorrectType()
    {
        // Arrange
        var intType = TfType.Find<int>();

        // Act
        var foundType = TfType.FindByName("Int32");

        // Assert
        Assert.Equal(intType, foundType);
    }

    [Fact]
    public void FindByName_WithUnknownName_ReturnsUnknownType()
    {
        // Arrange & Act
        var type = TfType.FindByName("NonExistentType");

        // Assert
        Assert.True(type.IsUnknown);
    }

    #endregion

    #region Type Declaration and Definition Tests

    [Fact]
    public void Declare_WithTypeName_CreatesType()
    {
        // Arrange
        var typeName = "TestDeclaredType_" + Guid.NewGuid();

        // Act
        var type = TfType.Declare(typeName);

        // Assert
        Assert.False(type.IsUnknown);
        Assert.Equal(typeName, type.TypeName);
        Assert.Equal(type, TfType.FindByName(typeName));
    }

    [Fact]
    public void Declare_SameTypeNameTwice_ReturnsSameInstance()
    {
        // Arrange
        var typeName = "TestDeclaredType2_" + Guid.NewGuid();

        // Act
        var type1 = TfType.Declare(typeName);
        var type2 = TfType.Declare(typeName);

        // Assert
        Assert.Same(type1, type2);
    }

    [Fact]
    public void Declare_WithBases_SetsInheritance()
    {
        // Arrange
        var baseName = "TestBase_" + Guid.NewGuid();
        var derivedName = "TestDerived_" + Guid.NewGuid();
        var baseType = TfType.Declare(baseName);

        // Act
        var derivedType = TfType.Declare(derivedName, new List<TfType> { baseType });

        // Assert
        Assert.Contains(baseType, derivedType.GetBaseTypes());
        Assert.True(derivedType.IsA(baseType));
    }

    [Fact]
    public void Define_GenericType_RegistersWithCSharpType()
    {
        // Act
        var type = TfType.Define<TestClassForTfType>();

        // Assert
        Assert.False(type.IsUnknown);
        Assert.Equal(typeof(TestClassForTfType), type.Typeid);
        Assert.Equal("TestClassForTfType", type.TypeName);
    }

    [Fact]
    public void Define_WithBaseType_SetsInheritance()
    {
        // Act
        var baseType = TfType.Define<TestBaseClass>();
        var derivedType = TfType.Define<TestDerivedClass, TestBaseClass>();

        // Assert
        Assert.True(derivedType.IsA(baseType));
        Assert.True(derivedType.IsA<TestBaseClass>());
    }

    #endregion

    #region Type Property Tests

    [Fact]
    public void TypeName_ReturnsCorrectName()
    {
        // Arrange
        var stringType = TfType.Find<string>();
        var listType = TfType.Find<List<int>>();

        // Act & Assert
        Assert.Equal("String", stringType.TypeName);
        Assert.Equal("List<Int32>", listType.TypeName);
    }

    [Fact]
    public void Typeid_ReturnsCorrectSystemType()
    {
        // Arrange
        var intType = TfType.Find<int>();
        var stringType = TfType.Find<string>();

        // Act & Assert
        Assert.Equal(typeof(int), intType.Typeid);
        Assert.Equal(typeof(string), stringType.Typeid);
    }

    [Fact]
    public void CanonicalType_ReturnsSelf()
    {
        // Arrange
        var type = TfType.Find<int>();

        // Act & Assert
        Assert.Same(type, type.CanonicalType);
    }

    [Fact]
    public void IsEnumType_DetectsEnums()
    {
        // Arrange
        var enumType = TfType.Define<TestEnum>();
        var classType = TfType.Define<TestClassForTfType>();

        // Act & Assert
        Assert.True(enumType.IsEnumType);
        Assert.False(classType.IsEnumType);
    }

    [Fact]
    public void IsPlainOldDataType_DetectsPODTypes()
    {
        // Arrange
        var intType = TfType.Find<int>();
        var structType = TfType.Define<TestStruct>();
        var classType = TfType.Define<TestClassForTfType>();

        // Act & Assert
        Assert.True(intType.IsPlainOldDataType);
        Assert.True(structType.IsPlainOldDataType);
        Assert.False(classType.IsPlainOldDataType);
    }

    [Fact]
    public void Sizeof_ReturnsCorrectSize()
    {
        // Arrange
        var intType = TfType.Find<int>();
        var boolType = TfType.Find<bool>();

        // Act & Assert
        Assert.Equal(sizeof(int), intType.Sizeof);
        Assert.Equal(sizeof(bool), boolType.Sizeof);
    }

    #endregion

    #region Type Hierarchy Tests

    [Fact]
    public void GetBaseTypes_ReturnsDirectBases()
    {
        // Arrange
        var baseType1 = TfType.Declare("Base1_" + Guid.NewGuid());
        var baseType2 = TfType.Declare("Base2_" + Guid.NewGuid());
        var derivedType = TfType.Declare("Derived_" + Guid.NewGuid(), 
            new List<TfType> { baseType1, baseType2 });

        // Act
        var bases = derivedType.GetBaseTypes();

        // Assert
        Assert.Equal(2, bases.Count);
        Assert.Contains(baseType1, bases);
        Assert.Contains(baseType2, bases);
    }

    [Fact]
    public void GetDirectlyDerivedTypes_ReturnsDirectDerivatives()
    {
        // Arrange
        var baseType = TfType.Declare("Base_" + Guid.NewGuid());
        var derived1 = TfType.Declare("Derived1_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var derived2 = TfType.Declare("Derived2_" + Guid.NewGuid(), 
            new List<TfType> { baseType });

        // Act
        var derivedTypes = baseType.GetDirectlyDerivedTypes();

        // Assert
        Assert.Equal(2, derivedTypes.Count);
        Assert.Contains(derived1, derivedTypes);
        Assert.Contains(derived2, derivedTypes);
    }

    [Fact]
    public void GetAllDerivedTypes_ReturnsAllDerivatives()
    {
        // Arrange
        var baseType = TfType.Declare("Base_" + Guid.NewGuid());
        var derived1 = TfType.Declare("Derived1_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var derived2 = TfType.Declare("Derived2_" + Guid.NewGuid(), 
            new List<TfType> { derived1 });

        // Act
        var allDerived = new HashSet<TfType>();
        baseType.GetAllDerivedTypes(allDerived);

        // Assert
        Assert.Equal(2, allDerived.Count);
        Assert.Contains(derived1, allDerived);
        Assert.Contains(derived2, allDerived);
    }

    [Fact]
    public void GetAllAncestorTypes_ReturnsAllAncestors()
    {
        // Arrange
        var root = TfType.GetRoot();
        var base1 = TfType.Declare("Base1_" + Guid.NewGuid());
        var base2 = TfType.Declare("Base2_" + Guid.NewGuid(), 
            new List<TfType> { base1 });
        var derived = TfType.Declare("Derived_" + Guid.NewGuid(), 
            new List<TfType> { base2 });

        // Act
        var ancestors = new List<TfType>();
        derived.GetAllAncestorTypes(ancestors);

        // Assert
        Assert.Contains(derived, ancestors);
        Assert.Contains(base2, ancestors);
        Assert.Contains(base1, ancestors);
        Assert.Contains(root, ancestors);
    }

    [Fact]
    public void IsA_WithSameType_ReturnsTrue()
    {
        // Arrange
        var type = TfType.Find<string>();

        // Act & Assert
        Assert.True(type.IsA(type));
    }

    [Fact]
    public void IsA_WithBaseType_ReturnsTrue()
    {
        // Arrange
        var baseType = TfType.Define<TestBaseClass>();
        var derivedType = TfType.Define<TestDerivedClass, TestBaseClass>();

        // Act & Assert
        Assert.True(derivedType.IsA(baseType));
        Assert.False(baseType.IsA(derivedType));
    }

    [Fact]
    public void IsA_WithRootType_AlwaysReturnsTrue()
    {
        // Arrange
        var root = TfType.GetRoot();
        var anyType = TfType.Find<string>();

        // Act & Assert
        Assert.True(anyType.IsA(root));
    }

    [Fact]
    public void IsA_WithUnknownType_ReturnsFalse()
    {
        // Arrange
        var unknown = TfType.GetUnknownType();
        var anyType = TfType.Find<string>();

        // Act & Assert
        Assert.False(anyType.IsA(unknown));
        Assert.False(unknown.IsA(anyType));
    }

    #endregion

    #region Alias Tests

    [Fact]
    public void AddAlias_CreatesAlias()
    {
        // Arrange
        var baseType = TfType.Declare("AliasBase_" + Guid.NewGuid());
        var derivedType = TfType.Declare("AliasDerived_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var aliasName = "MyAlias_" + Guid.NewGuid();

        // Act
        derivedType.AddAlias(baseType, aliasName);

        // Assert
        var foundByAlias = baseType.FindDerivedByName(aliasName);
        Assert.Equal(derivedType, foundByAlias);
    }

    [Fact]
    public void AddAlias_DuplicateAlias_Throws()
    {
        // Arrange
        var baseType = TfType.Declare("AliasBase2_" + Guid.NewGuid());
        var derived1 = TfType.Declare("Derived1_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var derived2 = TfType.Declare("Derived2_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var aliasName = "DuplicateAlias_" + Guid.NewGuid();

        // Act
        derived1.AddAlias(baseType, aliasName);

        // Assert
        Assert.Throws<InvalidOperationException>(() => 
            derived2.AddAlias(baseType, aliasName));
    }

    [Fact]
    public void GetAliases_ReturnsAllAliases()
    {
        // Arrange
        var baseType = TfType.Declare("AliasBase3_" + Guid.NewGuid());
        var derivedType = TfType.Declare("AliasDerived3_" + Guid.NewGuid(), 
            new List<TfType> { baseType });
        var alias1 = "Alias1_" + Guid.NewGuid();
        var alias2 = "Alias2_" + Guid.NewGuid();

        // Act
        derivedType.AddAlias(baseType, alias1);
        derivedType.AddAlias(baseType, alias2);
        var aliases = baseType.GetAliases(derivedType);

        // Assert
        Assert.Equal(2, aliases.Count);
        Assert.Contains(alias1, aliases);
        Assert.Contains(alias2, aliases);
    }

    [Fact]
    public void FindDerivedByName_FindsByTypeName()
    {
        // Arrange
        var baseType = TfType.Declare("FindBase_" + Guid.NewGuid());
        var derivedName = "FindDerived_" + Guid.NewGuid();
        var derivedType = TfType.Declare(derivedName, 
            new List<TfType> { baseType });

        // Act
        var found = baseType.FindDerivedByName(derivedName);

        // Assert
        Assert.Equal(derivedType, found);
    }

    [Fact]
    public void FindDerivedByName_WithUnknownName_ReturnsUnknownType()
    {
        // Arrange
        var baseType = TfType.Find<object>();

        // Act
        var found = baseType.FindDerivedByName("NonExistentDerived");

        // Assert
        Assert.True(found.IsUnknown);
    }

    #endregion

    #region Factory Tests

    [Fact]
    public void SetFactory_StoresFactory()
    {
        // Arrange
        var type = TfType.Declare("FactoryType_" + Guid.NewGuid());
        var factory = new TestFactory();

        // Act
        type.SetFactory(factory);

        // Assert
        var retrieved = type.GetFactory<TestFactory>();
        Assert.Same(factory, retrieved);
    }

    [Fact]
    public void SetFactory_Twice_Throws()
    {
        // Arrange
        var type = TfType.Declare("FactoryType2_" + Guid.NewGuid());
        var factory1 = new TestFactory();
        var factory2 = new TestFactory();

        // Act
        type.SetFactory(factory1);

        // Assert
        Assert.Throws<InvalidOperationException>(() => type.SetFactory(factory2));
    }

    [Fact]
    public void SetFactory_OnUnknownType_Throws()
    {
        // Arrange
        var unknown = TfType.GetUnknownType();
        var factory = new TestFactory();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => unknown.SetFactory(factory));
    }

    [Fact]
    public void SetFactory_OnRootType_Throws()
    {
        // Arrange
        var root = TfType.GetRoot();
        var factory = new TestFactory();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => root.SetFactory(factory));
    }

    [Fact]
    public void GetFactory_WithWrongType_ReturnsNull()
    {
        // Arrange
        var type = TfType.Declare("FactoryType3_" + Guid.NewGuid());
        type.SetFactory(new TestFactory());

        // Act
        var retrieved = type.GetFactory<DifferentTestFactory>();

        // Assert
        Assert.Null(retrieved);
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_ComparesCorrectly()
    {
        // Arrange
        var type1 = TfType.Find<string>();
        var type2 = TfType.Find<string>();
        var type3 = TfType.Find<int>();

        // Act & Assert
        Assert.True(type1 == type2);
        Assert.False(type1 == type3);
        Assert.False(type1 != type2);
        Assert.True(type1 != type3);
    }

    [Fact]
    public void EqualityOperator_HandlesNull()
    {
        // Arrange
        TfType? nullType = null;
        var type = TfType.Find<string>();

        // Act & Assert
        Assert.True(nullType == null);
        Assert.False(nullType == type);
        Assert.False(type == null);
        Assert.True(nullType != type);
    }

    [Fact]
    public void ImplicitBoolOperator_ReturnsTrueForKnownTypes()
    {
        // Arrange
        var knownType = TfType.Find<string>();
        var unknownType = TfType.GetUnknownType();

        // Act & Assert
        Assert.True(knownType);
        Assert.False(unknownType);
    }

    [Fact]
    public void ImplicitBoolOperator_ReturnsFalseForNull()
    {
        // Arrange
        TfType? nullType = null;

        // Act & Assert
        if (nullType)
        {
            Assert.True(false, "Null type should evaluate to false");
        }
    }

    [Fact]
    public void CompareTo_OrdersAlphabetically()
    {
        // Arrange
        var typeA = TfType.Declare("AType");
        var typeB = TfType.Declare("BType");
        var typeC = TfType.Declare("CType");

        // Act & Assert
        Assert.True(typeA.CompareTo(typeB) < 0);
        Assert.True(typeB.CompareTo(typeA) > 0);
        Assert.True(typeB.CompareTo(typeB) == 0);
        Assert.True(typeC.CompareTo(typeA) > 0);
    }

    [Fact]
    public void CompareTo_HandlesNull()
    {
        // Arrange
        var type = TfType.Find<string>();

        // Act & Assert
        Assert.True(type.CompareTo(null) > 0);
    }

    [Fact]
    public void GetHashCode_ConsistentForSameType()
    {
        // Arrange
        var type1 = TfType.Find<string>();
        var type2 = TfType.Find<string>();

        // Act & Assert
        Assert.Equal(type1.GetHashCode(), type2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsTypeName()
    {
        // Arrange
        var type = TfType.Find<string>();

        // Act & Assert
        Assert.Equal("String", type.ToString());
    }

    #endregion

    #region Canonical Type Name Tests

    [Fact]
    public void GetCanonicalTypeName_HandlesSimpleTypes()
    {
        // Act & Assert
        Assert.Equal("Int32", TfType.GetCanonicalTypeName(typeof(int)));
        Assert.Equal("String", TfType.GetCanonicalTypeName(typeof(string)));
        Assert.Equal("Boolean", TfType.GetCanonicalTypeName(typeof(bool)));
    }

    [Fact]
    public void GetCanonicalTypeName_HandlesGenericTypes()
    {
        // Act & Assert
        Assert.Equal("List<Int32>", TfType.GetCanonicalTypeName(typeof(List<int>)));
        Assert.Equal("Dictionary<String, Int32>", 
            TfType.GetCanonicalTypeName(typeof(Dictionary<string, int>)));
    }

    [Fact]
    public void GetCanonicalTypeName_HandlesArrayTypes()
    {
        // Act & Assert
        Assert.Equal("Int32[]", TfType.GetCanonicalTypeName(typeof(int[])));
        Assert.Equal("String[]", TfType.GetCanonicalTypeName(typeof(string[])));
    }

    [Fact]
    public void GetCanonicalTypeName_HandlesNull()
    {
        // Act & Assert
        Assert.Equal("Unknown", TfType.GetCanonicalTypeName(null!));
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void ConcurrentAccess_TypeRegistration_IsThreadSafe()
    {
        // Arrange
        var tasks = new List<System.Threading.Tasks.Task>();
        var types = new System.Collections.Concurrent.ConcurrentBag<TfType>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(System.Threading.Tasks.Task.Run(() =>
            {
                var type = TfType.Declare($"ConcurrentType_{index}");
                types.Add(type);
            }));
        }

        System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(10, types.Count);
        Assert.Equal(10, types.Distinct().Count());
    }

    [Fact]
    public void ConcurrentAccess_TypeLookup_IsThreadSafe()
    {
        // Arrange
        var testType = TfType.Define<TestClassForTfType>();
        var tasks = new List<System.Threading.Tasks.Task>();
        var results = new System.Collections.Concurrent.ConcurrentBag<TfType>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(System.Threading.Tasks.Task.Run(() =>
            {
                var found = TfType.Find<TestClassForTfType>();
                results.Add(found);
            }));
        }

        System.Threading.Tasks.Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(100, results.Count);
        Assert.True(results.All(t => t == testType));
    }

    #endregion

    #region Test Helper Types

    private class TestClassForTfType { }
    private class TestBaseClass { }
    private class TestDerivedClass : TestBaseClass { }
    private enum TestEnum { Value1, Value2 }
    private struct TestStruct 
    {
        public int X;
        public int Y;
    }

    private class TestFactory : TfType.FactoryBase { }
    private class DifferentTestFactory : TfType.FactoryBase { }

    #endregion
}