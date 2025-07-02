using System;
using System.Linq;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdSchemaTests
{
    #region Test Helper Classes
    
    [UsdSchema("TestTypedSchema", UsdSchemaKind.ConcreteTyped, TypeName = "TestPrim")]
    private class TestTypedSchema : UsdTyped
    {
        public TestTypedSchema() : base() { }
        public TestTypedSchema(UsdPrim prim) : base(prim) { }
        
        protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.ConcreteTyped;
        protected override TfToken GetSchemaTypeName() => new TfToken("TestTypedSchema");
        protected override TfToken GetTypeName() => new TfToken("TestPrim");
    }
    
    [UsdSchema("TestAPISchema", UsdSchemaKind.SingleApplyAPI)]
    private class TestAPISchema : UsdAPISchemaBase
    {
        public TestAPISchema() : base() { }
        public TestAPISchema(UsdPrim prim) : base(prim) { }
        
        protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.SingleApplyAPI;
        protected override TfToken GetSchemaTypeName() => new TfToken("TestAPISchema");
    }
    
    [UsdSchema("TestMultiApplyAPI", UsdSchemaKind.MultipleApplyAPI)]
    private class TestMultiApplyAPISchema : UsdAPISchemaBase
    {
        public TestMultiApplyAPISchema() : base() { }
        public TestMultiApplyAPISchema(UsdPrim prim) : base(prim) { }
        public TestMultiApplyAPISchema(UsdPrim prim, TfToken instanceName) : base(prim, instanceName) { }
        
        protected override UsdSchemaKind GetSchemaKind() => UsdSchemaKind.MultipleApplyAPI;
        protected override TfToken GetSchemaTypeName() => new TfToken("TestMultiApplyAPI");
        
        // Public wrapper methods for testing
        public TfToken TestMakeAttributeName(string baseName) => MakeAttributeName(baseName);
        public TfToken TestMakeRelationshipName(string baseName) => MakeRelationshipName(baseName);
    }
    
    #endregion
    
    #region Schema Registry Tests
    
    [Fact]
    public void UsdSchemaRegistry_ShouldBeAccessibleAsSingleton()
    {
        // Arrange & Act
        var registry1 = UsdSchemaRegistry.Instance;
        var registry2 = UsdSchemaRegistry.Instance;
        
        // Assert
        Assert.NotNull(registry1);
        Assert.Same(registry1, registry2);
    }
    
    [Fact]
    public void UsdSchemaRegistry_RegisterSchema_ShouldRegisterValidSchemaType()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        
        // Act
        registry.RegisterSchema(typeof(TestTypedSchema));
        
        // Assert
        var schemaInfo = registry.FindSchemaInfo(typeof(TestTypedSchema));
        Assert.NotNull(schemaInfo);
        Assert.Equal("TestTypedSchema", schemaInfo.Identifier.GetText());
        Assert.Equal(UsdSchemaKind.ConcreteTyped, schemaInfo.Kind);
        Assert.Equal("TestPrim", schemaInfo.TypeName.GetText());
    }
    
    [Fact]
    public void UsdSchemaRegistry_FindSchemaInfo_ShouldFindByIdentifier()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        registry.RegisterSchema(typeof(TestAPISchema));
        
        // Act
        var schemaInfo = registry.FindSchemaInfo(new TfToken("TestAPISchema"));
        
        // Assert
        Assert.NotNull(schemaInfo);
        Assert.Equal(typeof(TestAPISchema), schemaInfo.Type);
        Assert.Equal(UsdSchemaKind.SingleApplyAPI, schemaInfo.Kind);
    }
    
    [Fact]
    public void UsdSchemaRegistry_GetSchemasByKind_ShouldFilterCorrectly()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        registry.RegisterSchema(typeof(TestTypedSchema));
        registry.RegisterSchema(typeof(TestAPISchema));
        registry.RegisterSchema(typeof(TestMultiApplyAPISchema));
        
        // Act
        var typedSchemas = registry.GetSchemasByKind(UsdSchemaKind.ConcreteTyped).ToList();
        var apiSchemas = registry.GetAPISchemas().ToList();
        
        // Assert
        Assert.Contains(typedSchemas, info => info.Type == typeof(TestTypedSchema));
        Assert.Contains(apiSchemas, info => info.Type == typeof(TestAPISchema));
        Assert.Contains(apiSchemas, info => info.Type == typeof(TestMultiApplyAPISchema));
    }
    
    #endregion
    
    #region UsdSchemaBase Tests
    
    [Fact]
    public void UsdSchemaBase_WithValidPrim_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var schema = new TestTypedSchema(prim);
        
        // Assert
        Assert.True(schema.IsValid);
        Assert.Same(prim, schema.Prim);
        Assert.Equal(prim.GetPath(), schema.Path);
        Assert.Same(stage, schema.Stage);
    }
    
    [Fact]
    public void UsdSchemaBase_WithInvalidPrim_ShouldBeInvalid()
    {
        // Arrange
        var schema = new TestTypedSchema();
        
        // Assert
        Assert.False(schema.IsValid);
        Assert.False(schema.Prim.IsValid());
    }
    
    [Fact]
    public void UsdSchemaBase_BoolConversion_ShouldReflectValidity()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var validSchema = new TestTypedSchema(prim);
        var invalidSchema = new TestTypedSchema();
        
        // Act & Assert
        Assert.True(validSchema);
        Assert.False(invalidSchema);
    }
    
    #endregion
    
    #region UsdTyped Tests
    
    [Fact]
    public void UsdTyped_WithCorrectTypeName_ShouldBeCompatible()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        prim.SetTypeName("TestPrim");
        var schema = new TestTypedSchema(prim);
        
        // Assert
        Assert.True(schema.IsValid);
        Assert.Equal("TestPrim", schema.GetPrimTypeName());
    }
    
    [Fact]
    public void UsdTyped_TypeNameManagement_ShouldWork()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var schema = new TestTypedSchema(prim);
        
        // Act & Assert
        Assert.False(schema.HasTypeName());
        
        Assert.True(schema.SetTypeName("TestPrim"));
        Assert.True(schema.HasTypeName());
        Assert.Equal("TestPrim", schema.GetPrimTypeName());
        
        Assert.True(schema.ClearTypeName());
        Assert.False(schema.HasTypeName());
    }
    
    #endregion
    
    #region UsdAPISchemaBase Tests
    
    [Fact]
    public void UsdAPISchemaBase_SingleApply_ShouldTrackApplication()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var schema = new TestAPISchema(prim);
        
        // Act & Assert
        Assert.False(schema.IsApplied);
        Assert.False(schema.IsMultipleApply);
        
        Assert.True(schema.Apply());
        Assert.True(schema.IsApplied);
        
        Assert.True(schema.Remove());
        Assert.False(schema.IsApplied);
    }
    
    [Fact]
    public void UsdAPISchemaBase_MultipleApply_ShouldHandleInstanceNames()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var schema = new TestMultiApplyAPISchema(prim, new TfToken("instance1"));
        
        // Act & Assert
        Assert.True(schema.IsMultipleApply);
        Assert.Equal("instance1", schema.InstanceName.GetText());
        
        Assert.True(schema.Apply());
        Assert.True(schema.IsApplied);
        
        // Create another instance
        var schema2 = new TestMultiApplyAPISchema(prim, new TfToken("instance2"));
        Assert.True(schema2.Apply());
        Assert.True(schema2.IsApplied);
        
        // Original should still be applied
        Assert.True(schema.IsApplied);
    }
    
    [Fact]
    public void UsdAPISchemaBase_PropertyNames_ShouldPrefixForMultipleApply()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        var schema = new TestMultiApplyAPISchema(prim, new TfToken("instance1"));
        
        // Act
        var attrName = schema.TestMakeAttributeName("color");
        var relName = schema.TestMakeRelationshipName("targets");
        
        // Assert
        Assert.Equal("instance1:color", attrName.GetText());
        Assert.Equal("instance1:targets", relName.GetText());
    }
    
    #endregion
    
    #region UsdPrim Schema Integration Tests
    
    [Fact]
    public void UsdPrim_IsA_ShouldCheckSchemaCompatibility()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        registry.RegisterSchema(typeof(TestTypedSchema));
        
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        prim.SetTypeName("TestPrim");
        
        // Act & Assert
        Assert.True(prim.IsA<TestTypedSchema>());
        Assert.True(prim.IsA(typeof(TestTypedSchema)));
    }
    
    [Fact]
    public void UsdPrim_HasAPI_ShouldCheckAPIApplication()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        registry.RegisterSchema(typeof(TestAPISchema));
        
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        
        // Act & Assert
        Assert.False(prim.HasAPI<TestAPISchema>());
        Assert.False(prim.HasAPI(typeof(TestAPISchema)));
        
        Assert.True(prim.ApplyAPI<TestAPISchema>());
        Assert.True(prim.HasAPI<TestAPISchema>());
        Assert.True(prim.HasAPI(typeof(TestAPISchema)));
        
        Assert.True(prim.RemoveAPI<TestAPISchema>());
        Assert.False(prim.HasAPI<TestAPISchema>());
    }
    
    [Fact]
    public void UsdPrim_MultipleApplyAPI_ShouldHandleInstanceNames()
    {
        // Arrange
        var registry = UsdSchemaRegistry.Instance;
        registry.RegisterSchema(typeof(TestMultiApplyAPISchema));
        
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");
        
        // Act & Assert
        Assert.False(prim.HasAPI<TestMultiApplyAPISchema>("instance1"));
        
        Assert.True(prim.ApplyAPI<TestMultiApplyAPISchema>("instance1"));
        Assert.True(prim.HasAPI<TestMultiApplyAPISchema>("instance1"));
        Assert.False(prim.HasAPI<TestMultiApplyAPISchema>("instance2"));
        
        Assert.True(prim.ApplyAPI<TestMultiApplyAPISchema>("instance2"));
        Assert.True(prim.HasAPI<TestMultiApplyAPISchema>("instance1"));
        Assert.True(prim.HasAPI<TestMultiApplyAPISchema>("instance2"));
        
        Assert.True(prim.RemoveAPI<TestMultiApplyAPISchema>("instance1"));
        Assert.False(prim.HasAPI<TestMultiApplyAPISchema>("instance1"));
        Assert.True(prim.HasAPI<TestMultiApplyAPISchema>("instance2"));
    }
    
    #endregion
}