using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdTraversalTests
{
    #region Test Setup Helpers

    /// <summary>
    /// Create a test stage with a hierarchy for traversal testing.
    /// Structure:
    /// /Root
    ///   /Root/Group1 (kind: group)
    ///     /Root/Group1/Model1 (kind: component, type: Mesh)
    ///     /Root/Group1/Model2 (kind: component, type: Sphere, inactive)
    ///   /Root/Group2 (kind: group)
    ///     /Root/Group2/Model3 (kind: component, type: Cube)
    ///     /Root/Group2/SubGroup (kind: group)
    ///       /Root/Group2/SubGroup/Model4 (kind: component, type: Mesh)
    ///   /Root/Abstract1 (abstract)
    /// </summary>
    private UsdStage CreateTestStage()
    {
        var stage = UsdStage.CreateInMemory();
        
        // Root
        var root = stage.DefinePrim("/Root");
        
        // Group1
        var group1 = stage.DefinePrim("/Root/Group1");
        group1.SetMetadata("kind", "group");
        
        var model1 = stage.DefinePrim("/Root/Group1/Model1");
        model1.SetMetadata("kind", "component");
        model1.SetTypeName("Mesh");
        
        var model2 = stage.DefinePrim("/Root/Group1/Model2");
        model2.SetMetadata("kind", "component");
        model2.SetTypeName("Sphere");
        model2.SetActive(false); // Inactive
        
        // Group2
        var group2 = stage.DefinePrim("/Root/Group2");
        group2.SetMetadata("kind", "group");
        
        var model3 = stage.DefinePrim("/Root/Group2/Model3");
        model3.SetMetadata("kind", "component");
        model3.SetTypeName("Cube");
        
        var subGroup = stage.DefinePrim("/Root/Group2/SubGroup");
        subGroup.SetMetadata("kind", "group");
        
        var model4 = stage.DefinePrim("/Root/Group2/SubGroup/Model4");
        model4.SetMetadata("kind", "component");
        model4.SetTypeName("Mesh");
        
        // Abstract prim
        var abstract1 = stage.DefinePrim("/Root/Abstract1");
        abstract1.SetMetadata("abstract", true);
        abstract1.SetTypeName("Abstract");
        
        return stage;
    }

    #endregion

    #region Predicate Tests

    [Fact]
    public void UsdPrimPredicates_Default_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();

        // Act
        var filtered = prims.Where(UsdPrimPredicates.Default).ToList();

        // Assert
        // Should include active, defined, non-abstract prims
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/Model3");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
        
        // Should exclude inactive and abstract prims
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model2"); // Inactive
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root/Abstract1"); // Abstract
    }

    [Fact]
    public void UsdPrimPredicates_All_ShouldIncludeAllPrims()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();

        // Act
        var filtered = prims.Where(UsdPrimPredicates.All).ToList();

        // Assert
        Assert.Equal(prims.Count, filtered.Count);
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model2"); // Inactive
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Abstract1"); // Abstract
    }

    [Fact]
    public void UsdPrimPredicates_Models_ShouldFilterModels()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();

        // Act
        var filtered = prims.Where(UsdPrimPredicates.Models).ToList();

        // Assert
        // Should include groups and components
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/Model3");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
        
        // Should exclude non-models
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root");
    }

    [Fact]
    public void UsdPrimPredicates_OfType_ShouldFilterByType()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();
        var meshPredicate = UsdPrimPredicates.OfType("Mesh");

        // Act
        var filtered = prims.Where(meshPredicate).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
    }

    [Fact]
    public void UsdPrimPredicates_WithName_ShouldFilterByName()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();
        var modelPredicate = UsdPrimPredicates.WithName(name => name.StartsWith("Model"));

        // Act
        var filtered = prims.Where(modelPredicate).ToList();

        // Assert
        Assert.Equal(4, filtered.Count);
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model2");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/Model3");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
    }

    [Fact]
    public void UsdPrimPredicates_And_ShouldCombinePredicates()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();
        var combinedPredicate = UsdPrimPredicates.And(
            UsdPrimPredicates.Active,
            UsdPrimPredicates.OfType("Mesh")
        );

        // Act
        var filtered = prims.Where(combinedPredicate).ToList();

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
    }

    [Fact]
    public void UsdPrimPredicates_Or_ShouldCombinePredicates()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();
        var combinedPredicate = UsdPrimPredicates.Or(
            UsdPrimPredicates.OfType("Mesh"),
            UsdPrimPredicates.OfType("Cube")
        );

        // Act
        var filtered = prims.Where(combinedPredicate).ToList();

        // Assert
        Assert.Equal(3, filtered.Count);
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/Model3");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
    }

    [Fact]
    public void UsdPrimPredicates_Not_ShouldNegatePredicates()
    {
        // Arrange
        var stage = CreateTestStage();
        var prims = stage.TraverseAll().ToList();
        var notGroupPredicate = UsdPrimPredicates.Not(UsdPrimPredicates.Groups);

        // Act
        var filtered = prims.Where(notGroupPredicate).ToList();

        // Assert
        // Should exclude groups
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root/Group2");
        Assert.DoesNotContain(filtered, p => p.GetPath().GetString() == "/Root/Group2/SubGroup");
        
        // Should include non-groups
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root");
        Assert.Contains(filtered, p => p.GetPath().GetString() == "/Root/Group1/Model1");
    }

    #endregion

    #region UsdPrimRange Tests

    [Fact]
    public void UsdPrimRange_BasicTraversal_ShouldVisitAllFilteredPrims()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = new UsdPrimRange(root);

        // Act
        var visited = range.ToList();

        // Assert
        Assert.NotEmpty(visited);
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        // Model2 should be excluded (inactive)
        Assert.DoesNotContain(visited, p => p.GetPath().GetString() == "/Root/Group1/Model2");
    }

    [Fact]
    public void UsdPrimRange_AllPrims_ShouldVisitAllPrims()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = UsdPrimRange.AllPrims(root);

        // Act
        var visited = range.ToList();

        // Assert
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1/Model2"); // Inactive
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Abstract1"); // Abstract
    }

    [Fact]
    public void UsdPrimRange_CustomPredicate_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = new UsdPrimRange(root, UsdPrimPredicates.Components);

        // Act
        var visited = range.ToList();

        // Assert
        Assert.All(visited, prim => Assert.True(prim.IsComponent()));
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group2/Model3");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
        
        // Should not contain groups
        Assert.DoesNotContain(visited, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.DoesNotContain(visited, p => p.GetPath().GetString() == "/Root/Group2");
    }

    [Fact]
    public void UsdPrimRange_DepthFirstTraversal_ShouldVisitInCorrectOrder()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = new UsdPrimRange(root, UsdPrimPredicates.All);

        // Act
        var visited = range.ToList();

        // Assert
        var rootIndex = visited.FindIndex(p => p.GetPath().GetString() == "/Root");
        var group1Index = visited.FindIndex(p => p.GetPath().GetString() == "/Root/Group1");
        var model1Index = visited.FindIndex(p => p.GetPath().GetString() == "/Root/Group1/Model1");
        var group2Index = visited.FindIndex(p => p.GetPath().GetString() == "/Root/Group2");

        // Root should come before its children
        Assert.True(rootIndex < group1Index);
        Assert.True(rootIndex < group2Index);
        
        // Group1 should come before its children  
        Assert.True(group1Index < model1Index);
    }

    [Fact]
    public void UsdPrimRange_Stage_ShouldTraverseEntireStage()
    {
        // Arrange
        var stage = CreateTestStage();
        var range = UsdPrimRange.Stage(stage);

        // Act
        var visited = range.ToList();

        // Assert
        Assert.NotEmpty(visited);
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group2/SubGroup/Model4");
    }

    #endregion

    #region Iterator Tests

    [Fact]
    public void UsdPrimRange_Iterator_PruneChildren_ShouldSkipSubtree()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = new UsdPrimRange(root, UsdPrimPredicates.All);
        var visited = new List<UsdPrim>();

        // Act
        using (var enumerator = range.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                visited.Add(current);
                
                // Prune Group1 subtree
                if (current.GetPath().GetString() == "/Root/Group1")
                {
                    if (enumerator is UsdPrimRange.PrimIterator iterator)
                        iterator.PruneChildren();
                }
            }
        }

        // Assert
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1");
        
        // Group1's children should be skipped
        Assert.DoesNotContain(visited, p => p.GetPath().GetString() == "/Root/Group1/Model1");
        Assert.DoesNotContain(visited, p => p.GetPath().GetString() == "/Root/Group1/Model2");
        
        // Group2 should still be visited
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group2");
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group2/Model3");
    }

    [Fact]
    public void UsdPrimRange_PreAndPost_ShouldVisitPrimsTwice()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");
        var range = UsdPrimRange.PreAndPostVisit(root);
        var preVisits = new List<string>();
        var postVisits = new List<string>();

        // Act
        using (var enumerator = range.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (enumerator is UsdPrimRange.PrimIterator iterator)
                {
                    if (iterator.IsPostVisit())
                        postVisits.Add(current.GetPath().GetString());
                    else
                        preVisits.Add(current.GetPath().GetString());
                }
            }
        }

        // Assert
        Assert.NotEmpty(preVisits);
        Assert.NotEmpty(postVisits);
        
        // Each prim should be visited in both pre and post order
        Assert.Contains("/Root", preVisits);
        Assert.Contains("/Root", postVisits);
        Assert.Contains("/Root/Group1", preVisits);
        Assert.Contains("/Root/Group1", postVisits);
    }

    #endregion

    #region Stage Integration Tests

    [Fact]
    public void UsdStage_TraverseRange_ShouldReturnValidRange()
    {
        // Arrange
        var stage = CreateTestStage();

        // Act
        var range = stage.TraverseRange();

        // Assert
        Assert.NotNull(range);
        var visited = range.ToList();
        Assert.NotEmpty(visited);
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root");
    }

    [Fact]
    public void UsdStage_TraverseRangeWithPredicate_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = CreateTestStage();

        // Act
        var range = stage.TraverseRange(UsdPrimPredicates.Components);

        // Assert
        var visited = range.ToList();
        Assert.All(visited, prim => Assert.True(prim.IsComponent()));
    }

    [Fact]
    public void UsdStage_TraverseAllRange_ShouldIncludeAllPrims()
    {
        // Arrange
        var stage = CreateTestStage();

        // Act
        var range = stage.TraverseAllRange();

        // Assert
        var visited = range.ToList();
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Group1/Model2"); // Inactive
        Assert.Contains(visited, p => p.GetPath().GetString() == "/Root/Abstract1"); // Abstract
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void UsdPrimRange_InvalidPrim_ShouldHandleGracefully()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var range = new UsdPrimRange(invalidPrim);

        // Act & Assert
        var visited = range.ToList();
        Assert.Empty(visited);
    }

    [Fact]
    public void UsdPrimRange_NullPredicate_ShouldThrow()
    {
        // Arrange
        var stage = CreateTestStage();
        var root = stage.GetPrimAtPath("/Root");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UsdPrimRange(root, null!));
    }

    [Fact]
    public void UsdPrimRange_StageWithInvalidStage_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsdPrimRange.Stage(null!));
    }

    #endregion
}