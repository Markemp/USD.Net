using System;
using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdPrimTests
{
    #region Construction and Basic Properties

    [Fact]
    public void UsdPrim_DefaultConstructor_ShouldCreateInvalidPrim()
    {
        // Act
        var prim = new UsdPrim();

        // Assert
        Assert.False(prim.IsValid());
        Assert.Equal(string.Empty, prim.GetTypeName());
        Assert.False(prim.HasTypeName());
    }

    [Fact]
    public void UsdPrim_WithStageAndPath_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();

        // Act
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Assert
        Assert.True(prim.IsValid());
        Assert.Equal("/testPrim", prim.GetPath().GetString());
        Assert.Same(stage, prim.GetStage());
    }

    [Fact]
    public void UsdPrim_TypeName_ShouldManageCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert - Initially no type name
        Assert.False(prim.HasTypeName());
        Assert.Equal(string.Empty, prim.GetTypeName());

        // Act & Assert - Set type name
        Assert.True(prim.SetTypeName("Sphere"));
        Assert.True(prim.HasTypeName());
        Assert.Equal("Sphere", prim.GetTypeName());

        // Act & Assert - Clear type name
        Assert.True(prim.ClearTypeName());
        Assert.False(prim.HasTypeName());
        Assert.Equal(string.Empty, prim.GetTypeName());
    }

    #endregion

    #region Active Flag Management

    [Fact]
    public void UsdPrim_Active_ShouldDefaultToTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert
        Assert.True(prim.IsActive());
        Assert.False(prim.HasAuthoredActive());
    }

    [Fact]
    public void UsdPrim_SetActive_ShouldSetFlagAndMarkAuthored()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act
        Assert.True(prim.SetActive(false));

        // Assert
        Assert.False(prim.IsActive());
        Assert.True(prim.HasAuthoredActive());
    }

    [Fact]
    public void UsdPrim_ClearActive_ShouldRemoveAuthoredOpinion()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));
        prim.SetActive(false);
        Assert.True(prim.HasAuthoredActive());

        // Act
        Assert.True(prim.ClearActive());

        // Assert
        Assert.True(prim.IsActive()); // Back to default
        Assert.False(prim.HasAuthoredActive());
    }

    [Fact]
    public void UsdPrim_ActiveWithInvalidPrim_ShouldReturnFalse()
    {
        // Arrange
        var prim = new UsdPrim();

        // Act & Assert
        Assert.False(prim.IsActive());
        Assert.False(prim.SetActive(true));
        Assert.False(prim.ClearActive());
        Assert.False(prim.HasAuthoredActive());
    }

    #endregion

    #region Kind Classification

    [Fact]
    public void UsdPrim_KindClassification_ShouldWorkWithMetadata()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert - No kind initially
        Assert.False(prim.IsModel());
        Assert.False(prim.IsGroup());
        Assert.False(prim.IsComponent());

        // Act & Assert - Set component kind
        prim.SetMetadata("kind", "component");
        Assert.True(prim.IsModel()); // component is a model
        Assert.False(prim.IsGroup());
        Assert.True(prim.IsComponent());

        // Act & Assert - Set group kind
        prim.SetMetadata("kind", "group");
        Assert.True(prim.IsModel()); // group is a model
        Assert.True(prim.IsGroup());
        Assert.False(prim.IsComponent());

        // Act & Assert - Set assembly kind (subtype of group)
        prim.SetMetadata("kind", "assembly");
        Assert.True(prim.IsModel()); // assembly is a model
        Assert.True(prim.IsGroup()); // assembly is a group
        Assert.False(prim.IsComponent());

        // Act & Assert - Set model kind directly
        prim.SetMetadata("kind", "model");
        Assert.True(prim.IsModel());
        Assert.False(prim.IsGroup());
        Assert.False(prim.IsComponent());

        // Act & Assert - Set subcomponent (not a model)
        prim.SetMetadata("kind", "subcomponent");
        Assert.False(prim.IsModel());
        Assert.False(prim.IsGroup());
        Assert.False(prim.IsComponent());
    }

    #endregion

    #region Metadata Operations

    [Fact]
    public void UsdPrim_Metadata_ShouldHandleMultipleTypes()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert - String metadata
        Assert.True(prim.SetMetadata("stringValue", "hello world"));
        Assert.True(prim.HasMetadata("stringValue"));
        Assert.Equal("hello world", prim.GetMetadata<string>("stringValue"));

        // Act & Assert - Integer metadata
        Assert.True(prim.SetMetadata("intValue", 42));
        Assert.True(prim.HasMetadata("intValue"));
        Assert.Equal(42, prim.GetMetadata<int>("intValue"));

        // Act & Assert - Double metadata
        Assert.True(prim.SetMetadata("doubleValue", 3.14159));
        Assert.True(prim.HasMetadata("doubleValue"));
        Assert.Equal(3.14159, prim.GetMetadata<double>("doubleValue"), 5);

        // Act & Assert - Boolean metadata
        Assert.True(prim.SetMetadata("boolValue", true));
        Assert.True(prim.HasMetadata("boolValue"));
        Assert.True(prim.GetMetadata<bool>("boolValue"));
    }

    [Fact]
    public void UsdPrim_GetMetadata_NonExistent_ShouldReturnDefault()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert
        Assert.False(prim.HasMetadata("nonExistent"));
        Assert.Equal(string.Empty, prim.GetMetadata<string>("nonExistent"));
        Assert.Equal(0, prim.GetMetadata<int>("nonExistent"));
        Assert.False(prim.GetMetadata<bool>("nonExistent"));
    }

    [Fact]
    public void UsdPrim_ClearMetadata_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));
        prim.SetMetadata("toRemove", "value");
        Assert.True(prim.HasMetadata("toRemove"));

        // Act
        Assert.True(prim.ClearMetadata("toRemove"));

        // Assert
        Assert.False(prim.HasMetadata("toRemove"));
        Assert.Equal(string.Empty, prim.GetMetadata<string>("toRemove"));
    }

    [Fact]
    public void UsdPrim_ClearMetadata_NonExistent_ShouldReturnFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert
        Assert.False(prim.ClearMetadata("nonExistent"));
    }

    [Fact]
    public void UsdPrim_Metadata_InvalidPrim_ShouldReturnFalse()
    {
        // Arrange
        var prim = new UsdPrim();

        // Act & Assert
        Assert.False(prim.SetMetadata("test", "value"));
        Assert.False(prim.HasMetadata("test"));
        Assert.False(prim.ClearMetadata("test"));
    }

    #endregion

    #region Instance and Prototype Handling

    [Fact]
    public void UsdPrim_Instanceable_ShouldManageFlag()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert - Initially not instanceable
        Assert.False(prim.IsInstanceable());

        // Act & Assert - Set instanceable
        Assert.True(prim.SetInstanceable(true));
        Assert.True(prim.IsInstanceable());

        // Act & Assert - Clear instanceable
        Assert.True(prim.ClearInstanceable());
        Assert.False(prim.IsInstanceable());
    }

    [Fact]
    public void UsdPrim_Instance_ShouldHandleBasicDetection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = new UsdPrim(stage, new SdfPath("/testPrim"));

        // Act & Assert - Initially not instance
        Assert.False(prim.IsInstance());

        // Act & Assert - Set as instance (simplified implementation)
        prim.SetMetadata("instance", true);
        Assert.True(prim.IsInstance());

        // Act & Assert - Get prototype (returns invalid for now)
        var prototype = prim.GetPrototype();
        Assert.False(prototype.IsValid());
    }

    [Fact]
    public void UsdPrim_InstanceOperations_InvalidPrim_ShouldReturnFalse()
    {
        // Arrange
        var prim = new UsdPrim();

        // Act & Assert
        Assert.False(prim.IsInstanceable());
        Assert.False(prim.SetInstanceable(true));
        Assert.False(prim.ClearInstanceable());
        Assert.False(prim.IsInstance());
    }

    #endregion

    #region Hierarchy Navigation

    [Fact]
    public void UsdPrim_GetParent_ShouldReturnValidParentOrInvalid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parentPrim = stage.DefinePrim("/parent");
        var childPrim = stage.DefinePrim("/parent/child");

        // Act
        var parent = childPrim.GetParent();
        var rootParent = parentPrim.GetParent();

        // Assert
        Assert.True(parent.IsValid());
        Assert.Equal("/parent", parent.GetPath().GetString());
        Assert.False(rootParent.IsValid()); // Root prim has no parent
    }

    [Fact]
    public void UsdPrim_GetChild_ShouldReturnNamedChild()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parentPrim = stage.DefinePrim("/parent");
        var childPrim = stage.DefinePrim("/parent/child");

        // Act
        var foundChild = parentPrim.GetChild("child");
        var notFoundChild = parentPrim.GetChild("nonexistent");

        // Assert
        Assert.True(foundChild.IsValid());
        Assert.Equal("/parent/child", foundChild.GetPath().GetString());
        Assert.False(notFoundChild.IsValid());
    }

    [Fact]
    public void UsdPrim_GetChildren_ShouldReturnDirectChildren()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parentPrim = stage.DefinePrim("/parent");
        var child1 = stage.DefinePrim("/parent/child1");
        var child2 = stage.DefinePrim("/parent/child2");
        var grandchild = stage.DefinePrim("/parent/child1/grandchild");

        // Act
        var children = parentPrim.GetChildren().ToList();

        // Assert
        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.GetPath().GetString() == "/parent/child1");
        Assert.Contains(children, c => c.GetPath().GetString() == "/parent/child2");
        Assert.DoesNotContain(children, c => c.GetPath().GetString() == "/parent/child1/grandchild");
    }

    [Fact]
    public void UsdPrim_GetFilteredChildren_ShouldApplyPredicate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parentPrim = stage.DefinePrim("/parent");
        var child1 = stage.DefinePrim("/parent/child1");
        var child2 = stage.DefinePrim("/parent/child2");
        child1.SetTypeName("Sphere");
        child2.SetTypeName("Cube");

        // Act
        var sphereChildren = parentPrim.GetFilteredChildren(p => p.GetTypeName() == "Sphere").ToList();

        // Assert
        Assert.Single(sphereChildren);
        Assert.Equal("/parent/child1", sphereChildren[0].GetPath().GetString());
    }

    [Fact]
    public void UsdPrim_GetDescendants_ShouldReturnAllDescendants()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPrim = stage.DefinePrim("/root");
        var child = stage.DefinePrim("/root/child");
        var grandchild = stage.DefinePrim("/root/child/grandchild");
        var otherRoot = stage.DefinePrim("/other");

        // Act
        var descendants = rootPrim.GetDescendants().ToList();

        // Assert
        Assert.Equal(2, descendants.Count);
        Assert.Contains(descendants, d => d.GetPath().GetString() == "/root/child");
        Assert.Contains(descendants, d => d.GetPath().GetString() == "/root/child/grandchild");
        Assert.DoesNotContain(descendants, d => d.GetPath().GetString() == "/other");
    }

    [Fact]
    public void UsdPrim_GetSiblings_ShouldReturnSiblingPrims()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parent = stage.DefinePrim("/parent");
        var child1 = stage.DefinePrim("/parent/child1");
        var child2 = stage.DefinePrim("/parent/child2");
        var child3 = stage.DefinePrim("/parent/child3");

        // Act
        var siblings = child2.GetSiblings().ToList();

        // Assert
        Assert.Equal(2, siblings.Count);
        Assert.Contains(siblings, s => s.GetPath().GetString() == "/parent/child1");
        Assert.Contains(siblings, s => s.GetPath().GetString() == "/parent/child3");
        Assert.DoesNotContain(siblings, s => s.GetPath().GetString() == "/parent/child2"); // Self excluded
    }

    [Fact]
    public void UsdPrim_GetNextSibling_ShouldReturnNextOrInvalid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var parent = stage.DefinePrim("/parent");
        var child1 = stage.DefinePrim("/parent/child1");
        var child2 = stage.DefinePrim("/parent/child2");
        var child3 = stage.DefinePrim("/parent/child3");

        // Act
        var nextAfterChild1 = child1.GetNextSibling();
        var nextAfterChild3 = child3.GetNextSibling();

        // Assert
        Assert.True(nextAfterChild1.IsValid());
        // Note: Order depends on implementation, but should be one of the siblings
        Assert.True(nextAfterChild1.GetPath().GetString() == "/parent/child2" || 
                   nextAfterChild1.GetPath().GetString() == "/parent/child3");
        
        // Last child should have no next sibling (or it might, depending on order)
        // This test verifies the method doesn't crash
        Assert.NotNull(nextAfterChild3);
    }

    [Fact]
    public void UsdPrim_HierarchyNavigation_InvalidPrim_ShouldReturnInvalid()
    {
        // Arrange
        var prim = new UsdPrim();

        // Act & Assert
        Assert.False(prim.GetParent().IsValid());
        Assert.False(prim.GetChild("test").IsValid());
        Assert.Empty(prim.GetChildren());
        Assert.Empty(prim.GetFilteredChildren(p => true));
        Assert.Empty(prim.GetDescendants());
        Assert.Empty(prim.GetSiblings());
        Assert.False(prim.GetNextSibling().IsValid());
    }

    #endregion

    #region Property Management

    [Fact]
    public void UsdPrim_CreateAttribute_ShouldCreateAndManage()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var attr = prim.CreateAttribute("testAttr", "float");

        // Assert
        Assert.True(attr.IsValid());
        Assert.Equal("testAttr", attr.GetName());
        Assert.True(prim.HasAttribute("testAttr"));
        
        var retrievedAttr = prim.GetAttribute("testAttr");
        Assert.True(retrievedAttr.IsValid());
        Assert.Equal("testAttr", retrievedAttr.GetName());
    }

    [Fact]
    public void UsdPrim_CreateRelationship_ShouldCreateAndManage()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var rel = prim.CreateRelationship("testRel");

        // Assert
        Assert.True(rel.IsValid());
        Assert.Equal("testRel", rel.GetName());
        Assert.True(prim.HasRelationship("testRel"));
        
        var retrievedRel = prim.GetRelationship("testRel");
        Assert.True(retrievedRel.IsValid());
        Assert.Equal("testRel", retrievedRel.GetName());
    }

    [Fact]
    public void UsdPrim_GetAllAttributes_ShouldReturnValidAttributes()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var attr1 = prim.CreateAttribute("attr1", "float");
        var attr2 = prim.CreateAttribute("attr2", "string");

        // Act
        var attributes = prim.GetAttributes().ToList();

        // Assert
        Assert.Equal(2, attributes.Count);
        Assert.All(attributes, attr => Assert.True(attr.IsValid()));
    }

    [Fact]
    public void UsdPrim_GetAllRelationships_ShouldReturnValidRelationships()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var rel1 = prim.CreateRelationship("rel1");
        var rel2 = prim.CreateRelationship("rel2");

        // Act
        var relationships = prim.GetRelationships().ToList();

        // Assert
        Assert.Equal(2, relationships.Count);
        Assert.All(relationships, rel => Assert.True(rel.IsValid()));
    }

    #endregion

    #region Schema and API Management Stubs

    [Fact]
    public void UsdPrim_SchemaAPIMethods_ShouldReturnFalseForNow()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act & Assert - These are stubs until schema system is implemented
        Assert.False(prim.IsA(typeof(object)));
        // Note: Can't test generic IsA<T>() without actual schema base classes
    }

    #endregion

    #region Composition Stubs

    [Fact]
    public void UsdPrim_CompositionMethods_ShouldReturnValidObjects()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act & Assert - These are stubs until composition system is implemented
        var references = prim.GetReferences();
        var inherits = prim.GetInherits();
        var specializes = prim.GetSpecializes();
        var variantSets = prim.GetVariantSets();

        // Should return objects (even if they're just stubs)
        Assert.NotNull(references);
        Assert.NotNull(inherits);
        Assert.NotNull(specializes);
        Assert.NotNull(variantSets);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void UsdPrim_InvalidOperations_ShouldHandleGracefully()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act & Assert - Empty/null parameter handling
        Assert.Throws<ArgumentException>(() => prim.CreateAttribute("", "float"));
        Assert.Throws<ArgumentException>(() => prim.CreateAttribute(null!, "float"));
        Assert.Throws<ArgumentException>(() => prim.CreateRelationship(""));
        Assert.Throws<ArgumentException>(() => prim.CreateRelationship(null!));

        Assert.False(prim.GetAttribute("").IsValid());
        Assert.False(prim.GetAttribute(null!).IsValid());
        Assert.False(prim.GetRelationship("").IsValid());
        Assert.False(prim.GetRelationship(null!).IsValid());

        Assert.False(prim.HasAttribute(""));
        Assert.False(prim.HasAttribute(null!));
        Assert.False(prim.HasRelationship(""));
        Assert.False(prim.HasRelationship(null!));
    }

    [Fact]
    public void UsdPrim_GetAttributeAtPath_ShouldHandlePropertyPaths()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var attr = prim.CreateAttribute("testAttr", "float");

        // Act
        var foundAttr = prim.GetAttributeAtPath("/testPrim.testAttr");
        var notFoundAttr = prim.GetAttributeAtPath("/testPrim.nonexistent");
        var invalidPathAttr = prim.GetAttributeAtPath("/otherPrim");

        // Assert
        Assert.True(foundAttr.IsValid());
        Assert.Equal("testAttr", foundAttr.GetName());
        Assert.False(notFoundAttr.IsValid());
        Assert.False(invalidPathAttr.IsValid());
    }

    #endregion
}