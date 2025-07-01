using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdQueryTests
{
    #region Test Setup Helpers

    /// <summary>
    /// Create a comprehensive test stage for query testing.
    /// Structure:
    /// /World
    ///   /World/Environment
    ///     /World/Environment/Sky (type: SkyDome)
    ///     /World/Environment/Ground (type: Plane)
    ///   /World/Characters (kind: group)
    ///     /World/Characters/Hero (kind: component, type: Mesh)
    ///       /World/Characters/Hero/Body (type: Mesh)
    ///       /World/Characters/Hero/Clothing (type: Mesh, inactive)
    ///     /World/Characters/NPC_Guard1 (kind: component, type: Mesh)
    ///     /World/Characters/NPC_Guard2 (kind: component, type: Mesh)
    ///   /World/Props (kind: group)
    ///     /World/Props/Table (kind: component, type: Mesh)
    ///     /World/Props/Chair_01 (kind: component, type: Mesh)
    ///     /World/Props/Chair_02 (kind: component, type: Mesh)
    ///   /World/Lights
    ///     /World/Lights/KeyLight (type: DistantLight)
    ///     /World/Lights/FillLight (type: SphereLight)
    ///   /World/Abstract (abstract)
    /// </summary>
    private UsdStage CreateComprehensiveTestStage()
    {
        var stage = UsdStage.CreateInMemory();
        
        // World root
        var world = stage.DefinePrim("/World");
        
        // Environment
        var environment = stage.DefinePrim("/World/Environment");
        var sky = stage.DefinePrim("/World/Environment/Sky");
        sky.SetTypeName("SkyDome");
        var ground = stage.DefinePrim("/World/Environment/Ground");
        ground.SetTypeName("Plane");
        
        // Characters
        var characters = stage.DefinePrim("/World/Characters");
        characters.SetMetadata("kind", "group");
        
        var hero = stage.DefinePrim("/World/Characters/Hero");
        hero.SetMetadata("kind", "component");
        hero.SetTypeName("Mesh");
        
        var heroBody = stage.DefinePrim("/World/Characters/Hero/Body");
        heroBody.SetTypeName("Mesh");
        
        var heroClothing = stage.DefinePrim("/World/Characters/Hero/Clothing");
        heroClothing.SetTypeName("Mesh");
        heroClothing.SetActive(false); // Inactive
        
        var npcGuard1 = stage.DefinePrim("/World/Characters/NPC_Guard1");
        npcGuard1.SetMetadata("kind", "component");
        npcGuard1.SetTypeName("Mesh");
        
        var npcGuard2 = stage.DefinePrim("/World/Characters/NPC_Guard2");
        npcGuard2.SetMetadata("kind", "component");
        npcGuard2.SetTypeName("Mesh");
        
        // Props
        var props = stage.DefinePrim("/World/Props");
        props.SetMetadata("kind", "group");
        
        var table = stage.DefinePrim("/World/Props/Table");
        table.SetMetadata("kind", "component");
        table.SetTypeName("Mesh");
        
        var chair01 = stage.DefinePrim("/World/Props/Chair_01");
        chair01.SetMetadata("kind", "component");
        chair01.SetTypeName("Mesh");
        
        var chair02 = stage.DefinePrim("/World/Props/Chair_02");
        chair02.SetMetadata("kind", "component");
        chair02.SetTypeName("Mesh");
        
        // Lights
        var lights = stage.DefinePrim("/World/Lights");
        
        var keyLight = stage.DefinePrim("/World/Lights/KeyLight");
        keyLight.SetTypeName("DistantLight");
        
        var fillLight = stage.DefinePrim("/World/Lights/FillLight");
        fillLight.SetTypeName("SphereLight");
        
        // Abstract prim
        var abstractPrim = stage.DefinePrim("/World/Abstract");
        abstractPrim.SetMetadata("abstract", true);
        abstractPrim.SetTypeName("Abstract");
        
        // Add some attributes for testing
        var colorAttr = hero.CreateAttribute("displayColor", "color3f");
        colorAttr.Set("(1.0, 0.0, 0.0)");
        
        var scaleAttr = table.CreateAttribute("scale", "float");
        scaleAttr.Set("2.0");
        
        return stage;
    }

    #endregion

    #region Find by Path Tests

    [Fact]
    public void UsdQuery_FindPrimsByPath_ExactPath_ShouldFindPrim()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByPath(stage, "/World/Characters/Hero").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("/World/Characters/Hero", results[0].GetPath().GetString());
    }

    [Fact]
    public void UsdQuery_FindPrimsByPath_WildcardPattern_ShouldFindMatchingPrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByPath(stage, "/World/Characters/NPC_*").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard2");
    }

    [Fact]
    public void UsdQuery_FindPrimsByPath_RecursiveWildcard_ShouldFindAllMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByPath(stage, "/World/*/Chair_*").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_01");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_02");
    }

    [Fact]
    public void UsdQuery_FindPrimByPath_ValidPath_ShouldReturnPrim()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var result = UsdQuery.FindPrimByPath(stage, "/World/Characters/Hero");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/World/Characters/Hero", result.GetPath().GetString());
    }

    [Fact]
    public void UsdQuery_FindPrimByPath_InvalidPath_ShouldReturnNull()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var result = UsdQuery.FindPrimByPath(stage, "/NonExistent");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Find by Type Tests

    [Fact]
    public void UsdQuery_FindPrimsByType_MeshType_ShouldFindAllMeshes()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByType(stage, "Mesh").ToList();

        // Assert
        Assert.Equal(6, results.Count); // 6 active mesh prims
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero/Body");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard2");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Table");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_01");
        
        // Should not contain inactive mesh
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World/Characters/Hero/Clothing");
    }

    [Fact]
    public void UsdQuery_FindPrimsByType_IncludeInactive_ShouldFindInactivePrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByType(stage, "Mesh", includeInactive: true).ToList();

        // Assert
        Assert.Equal(7, results.Count); // 7 total mesh prims including inactive
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero/Clothing");
    }

    [Fact]
    public void UsdQuery_FindPrimsByTypes_MultipleLightTypes_ShouldFindAllLights()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();
        var lightTypes = new[] { "DistantLight", "SphereLight" };

        // Act
        var results = UsdQuery.FindPrimsByTypes(stage, lightTypes).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Lights/KeyLight");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Lights/FillLight");
    }

    [Fact]
    public void UsdQuery_FindFirstPrimByType_ShouldReturnFirstMatch()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var result = UsdQuery.FindFirstPrimByType(stage, "Mesh");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mesh", result.GetTypeName());
    }

    #endregion

    #region Find by Name Tests

    [Fact]
    public void UsdQuery_FindPrimsByName_ExactName_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByName(stage, "Hero").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("/World/Characters/Hero", results[0].GetPath().GetString());
    }

    [Fact]
    public void UsdQuery_FindPrimsByNamePattern_WildcardPattern_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByNamePattern(stage, "Chair_*").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetName() == "Chair_01");
        Assert.Contains(results, p => p.GetName() == "Chair_02");
    }

    [Fact]
    public void UsdQuery_FindPrimsByNamePattern_QuestionMarkWildcard_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByNamePattern(stage, "Chair_0?").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetName() == "Chair_01");
        Assert.Contains(results, p => p.GetName() == "Chair_02");
    }

    [Fact]
    public void UsdQuery_FindPrimsByNameContains_Substring_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByNameContains(stage, "Guard").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetName() == "NPC_Guard1");
        Assert.Contains(results, p => p.GetName() == "NPC_Guard2");
    }

    [Fact]
    public void UsdQuery_FindPrimsByNameContains_IgnoreCase_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByNameContains(stage, "HERO", ignoreCase: true).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("Hero", results[0].GetName());
    }

    #endregion

    #region Find by Kind Tests

    [Fact]
    public void UsdQuery_FindModels_ShouldFindAllModelPrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindModels(stage).ToList();

        // Assert
        // Should find groups and components
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Table");
    }

    [Fact]
    public void UsdQuery_FindGroups_ShouldFindGroupPrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindGroups(stage).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props");
    }

    [Fact]
    public void UsdQuery_FindComponents_ShouldFindComponentPrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindComponents(stage).ToList();

        // Assert
        Assert.Equal(5, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard2");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Table");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_01");
    }

    #endregion

    #region Find by Attribute Tests

    [Fact]
    public void UsdQuery_FindPrimsWithAttribute_ShouldFindPrimsWithSpecificAttribute()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsWithAttribute(stage, "displayColor").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("/World/Characters/Hero", results[0].GetPath().GetString());
    }

    [Fact]
    public void UsdQuery_FindPrimsByAttributeValue_ShouldFindPrimsWithSpecificValue()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByAttributeValue(stage, "scale", "2.0").ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("/World/Props/Table", results[0].GetPath().GetString());
    }

    #endregion

    #region Hierarchical Query Tests

    [Fact]
    public void UsdQuery_FindDescendants_ShouldFindAllDescendants()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();
        var characters = stage.GetPrimAtPath("/World/Characters");

        // Act
        var results = UsdQuery.FindDescendants(characters).ToList();

        // Assert
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero/Body");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard2");
        
        // Should not contain the root prim itself
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World/Characters");
    }

    [Fact]
    public void UsdQuery_FindChildren_ShouldFindDirectChildren()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();
        var world = stage.GetPrimAtPath("/World");

        // Act
        var results = UsdQuery.FindChildren(world).ToList();

        // Assert
        Assert.Equal(4, results.Count); // Environment, Characters, Props, Lights
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Environment");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Lights");
        
        // Should not contain grandchildren
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
    }

    [Fact]
    public void UsdQuery_FindAncestor_ShouldFindFirstMatchingAncestor()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();
        var heroBody = stage.GetPrimAtPath("/World/Characters/Hero/Body");

        // Act
        var result = UsdQuery.FindAncestor(heroBody, UsdPrimPredicates.Groups);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("/World/Characters", result.GetPath().GetString());
    }

    [Fact]
    public void UsdQuery_FindAncestors_ShouldFindAllAncestors()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();
        var heroBody = stage.GetPrimAtPath("/World/Characters/Hero/Body");

        // Act
        var results = UsdQuery.FindAncestors(heroBody).ToList();

        // Assert
        Assert.Equal(3, results.Count); // Hero, Characters, World
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World");
    }

    #endregion

    #region Collection Query Tests

    [Fact]
    public void UsdQuery_CountPrims_ShouldReturnCorrectCount()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var count = UsdQuery.CountPrims(stage, UsdPrimPredicates.OfType("Mesh"));

        // Assert
        Assert.Equal(6, count); // 6 active mesh prims
    }

    [Fact]
    public void UsdQuery_AnyPrims_WithExistingType_ShouldReturnTrue()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var result = UsdQuery.AnyPrims(stage, UsdPrimPredicates.OfType("SkyDome"));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UsdQuery_AnyPrims_WithNonExistingType_ShouldReturnFalse()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var result = UsdQuery.AnyPrims(stage, UsdPrimPredicates.OfType("NonExistentType"));

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Advanced Query Tests

    [Fact]
    public void UsdQuery_FindPrimsByCriteria_MultipleCriteria_ShouldFindMatches()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByCriteria(stage, 
            typeName: "Mesh", 
            namePattern: "*Guard*", 
            isModel: true).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard2");
    }

    [Fact]
    public void UsdQuery_FindPrimsByCriteria_PathPattern_ShouldFilterByPath()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByCriteria(stage, 
            typeName: "Mesh", 
            pathPattern: "/World/Props/*").ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Table");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_01");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Props/Chair_02");
    }

    [Fact]
    public void UsdQuery_FindLeafPrims_ShouldFindPrimsWithoutChildren()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindLeafPrims(stage).ToList();

        // Assert
        // Leaf prims should include terminal nodes
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Environment/Sky");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Environment/Ground");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/Hero/Body");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Characters/NPC_Guard1");
        Assert.Contains(results, p => p.GetPath().GetString() == "/World/Lights/KeyLight");
        
        // Should not contain prims with children
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World");
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World/Characters");
        Assert.DoesNotContain(results, p => p.GetPath().GetString() == "/World/Characters/Hero");
    }

    [Fact]
    public void UsdQuery_FindRootPrims_ShouldFindTopLevelPrims()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindRootPrims(stage).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal("/World", results[0].GetPath().GetString());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void UsdQuery_FindPrimsByType_NullStage_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => UsdQuery.FindPrimsByType(null!, "Mesh"));
    }

    [Fact]
    public void UsdQuery_FindPrimsByName_EmptyName_ShouldReturnEmpty()
    {
        // Arrange
        var stage = CreateComprehensiveTestStage();

        // Act
        var results = UsdQuery.FindPrimsByName(stage, "").ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void UsdQuery_FindDescendants_InvalidPrim_ShouldReturnEmpty()
    {
        // Arrange
        var invalidPrim = new UsdPrim();

        // Act
        var results = UsdQuery.FindDescendants(invalidPrim).ToList();

        // Assert
        Assert.Empty(results);
    }

    #endregion
}