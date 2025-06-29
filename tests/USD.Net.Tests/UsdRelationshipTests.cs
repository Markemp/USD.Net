using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdRelationshipTests
{
    [Fact]
    public void UsdRelationship_CreateRelationship_ShouldHaveValidPath()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var relationship = prim.CreateRelationship("testRel");

        // Assert
        Assert.True(relationship.IsValid());
        Assert.Equal("/testPrim.testRel", relationship.GetPath().GetString());
        Assert.Equal("testRel", relationship.GetName());
        Assert.Same(stage, relationship.GetStage());
    }

    [Fact]
    public void UsdRelationship_AddTarget_ShouldStoreTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var targetPrim = stage.DefinePrim("/target");
        var relationship = sourcePrim.CreateRelationship("rel");

        // Act
        var success = relationship.AddTarget(targetPrim.GetPath());

        // Assert
        Assert.True(success);
        Assert.True(relationship.HasTargets());
        Assert.Single(relationship.GetTargets());
        Assert.Equal("/target", relationship.GetTargets()[0].GetString());
        Assert.True(relationship.HasTarget(targetPrim.GetPath()));
    }

    [Fact]
    public void UsdRelationship_SetTargets_ShouldReplaceAllTargets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var target1 = stage.DefinePrim("/target1");
        var target2 = stage.DefinePrim("/target2");
        var target3 = stage.DefinePrim("/target3");
        var relationship = sourcePrim.CreateRelationship("rel");

        // Add initial target
        relationship.AddTarget(target1.GetPath());

        // Act
        var targets = new[] { target2.GetPath(), target3.GetPath() };
        var success = relationship.SetTargets(targets);

        // Assert
        Assert.True(success);
        Assert.Equal(2, relationship.GetNumTargets());
        Assert.False(relationship.HasTarget(target1.GetPath()));
        Assert.True(relationship.HasTarget(target2.GetPath()));
        Assert.True(relationship.HasTarget(target3.GetPath()));
    }

    [Fact]
    public void UsdRelationship_RemoveTarget_ShouldRemoveSpecificTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var target1 = stage.DefinePrim("/target1");
        var target2 = stage.DefinePrim("/target2");
        var relationship = sourcePrim.CreateRelationship("rel");

        relationship.AddTarget(target1.GetPath());
        relationship.AddTarget(target2.GetPath());

        // Act
        var success = relationship.RemoveTarget(target1.GetPath());

        // Assert
        Assert.True(success);
        Assert.Single(relationship.GetTargets());
        Assert.False(relationship.HasTarget(target1.GetPath()));
        Assert.True(relationship.HasTarget(target2.GetPath()));
    }

    [Fact]
    public void UsdRelationship_ClearTargets_ShouldRemoveAllTargets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var target1 = stage.DefinePrim("/target1");
        var target2 = stage.DefinePrim("/target2");
        var relationship = sourcePrim.CreateRelationship("rel");

        relationship.AddTarget(target1.GetPath());
        relationship.AddTarget(target2.GetPath());

        // Act
        var success = relationship.ClearTargets();

        // Assert
        Assert.True(success);
        Assert.False(relationship.HasTargets());
        Assert.Empty(relationship.GetTargets());
    }

    [Fact]
    public void UsdRelationship_TargetValidation_ShouldRejectInvalidPaths()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var relationship = sourcePrim.CreateRelationship("rel");

        // Act & Assert
        Assert.False(relationship.AddTarget(SdfPath.EmptyPath()));
        Assert.False(relationship.AddTarget(new SdfPath("relativePath")));
        Assert.True(relationship.AddTarget(new SdfPath("/validAbsolutePath")));
    }

    [Fact]
    public void UsdRelationship_GetPrimTargets_ShouldFilterPrimPaths()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var targetPrim = stage.DefinePrim("/target");
        var targetAttr = targetPrim.CreateAttribute("attr", "float");
        var relationship = sourcePrim.CreateRelationship("rel");

        relationship.AddTarget(targetPrim.GetPath());
        relationship.AddTarget(targetAttr.GetPath());

        // Act
        var primTargets = relationship.GetPrimTargets();
        var propertyTargets = relationship.GetPropertyTargets();

        // Assert
        Assert.Single(primTargets);
        Assert.Equal("/target", primTargets[0].GetString());
        Assert.Single(propertyTargets);
        Assert.Equal("/target.attr", propertyTargets[0].GetString());
    }

    [Fact]
    public void UsdRelationship_GetForwardedTargets_ShouldResolveChains()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim1 = stage.DefinePrim("/prim1");
        var prim2 = stage.DefinePrim("/prim2");
        var prim3 = stage.DefinePrim("/prim3");

        var rel1 = prim1.CreateRelationship("rel");
        var rel2 = prim2.CreateRelationship("rel");

        // Create chain: rel1 -> rel2 -> prim3
        rel1.AddTarget(rel2.GetPath());
        rel2.AddTarget(prim3.GetPath());

        // Act
        var forwardedTargets = rel1.GetForwardedTargets();

        // Assert
        Assert.Single(forwardedTargets);
        Assert.Equal("/prim3", forwardedTargets[0].GetString());
    }

    [Fact]
    public void UsdPrim_RelationshipOperations_ShouldWork()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act & Assert
        Assert.False(prim.HasRelationship("testRel"));

        var relationship = prim.CreateRelationship("testRel", custom: true);
        Assert.True(prim.HasRelationship("testRel"));
        Assert.True(relationship.IsCustom());

        var retrievedRel = prim.GetRelationship("testRel");
        Assert.True(retrievedRel.IsSameAs(relationship));

        var allRels = prim.GetRelationships().ToList();
        Assert.Single(allRels);
        Assert.True(allRels[0].IsSameAs(relationship));
    }

    [Fact]
    public void UsdRelationship_ToString_ShouldShowTargets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var sourcePrim = stage.DefinePrim("/source");
        var target1 = stage.DefinePrim("/target1");
        var target2 = stage.DefinePrim("/target2");
        var relationship = sourcePrim.CreateRelationship("rel");

        relationship.AddTarget(target1.GetPath());
        relationship.AddTarget(target2.GetPath());

        // Act
        var toString = relationship.ToString();
        var targetsString = relationship.GetTargetsAsString();

        // Assert
        Assert.Contains("/source.rel", toString);
        Assert.Contains("/target1", targetsString);
        Assert.Contains("/target2", targetsString);
    }
}