using System.Linq;
using Pxr.Usd;
using Xunit;

namespace USD.Net.Tests;

public class UsdVariantSetsTests
{
    #region Basic Functionality

    [Fact]
    public void UsdVariantSets_CreateFromPrim_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var variantSets = prim.GetVariantSets();

        // Assert
        Assert.True(variantSets.IsValid());
        Assert.Same(prim, variantSets.GetPrim());
        Assert.False(variantSets.HasAuthoredVariantSets());
        Assert.Equal(0, variantSets.GetNumVariantSets());
        Assert.Empty(variantSets.GetNames());
    }

    [Fact]
    public void UsdVariantSets_InvalidPrim_ShouldBeInvalid()
    {
        // Arrange
        var invalidPrim = new UsdPrim();

        // Act
        var variantSets = new UsdVariantSets(invalidPrim);

        // Assert
        Assert.False(variantSets.IsValid());
        Assert.False(variantSets.HasAuthoredVariantSets());
        Assert.Equal(0, variantSets.GetNumVariantSets());
    }

    #endregion

    #region Variant Set Management

    [Fact]
    public void UsdVariantSets_AddVariantSet_ShouldCreateValidVariantSet()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Act
        var modelingVariant = variantSets.AddVariantSet("modelingVariant");

        // Assert
        Assert.True(modelingVariant.IsValid());
        Assert.Equal("modelingVariant", modelingVariant.GetName());
        Assert.Same(prim, modelingVariant.GetPrim());
        Assert.True(variantSets.HasVariantSet("modelingVariant"));
        Assert.Equal(1, variantSets.GetNumVariantSets());
        Assert.Contains("modelingVariant", variantSets.GetNames());
    }

    [Fact]
    public void UsdVariantSets_GetVariantSet_ShouldCreateOnDemand()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Act
        var shadingVariant = variantSets.GetVariantSet("shadingVariant");

        // Assert
        Assert.True(shadingVariant.IsValid());
        Assert.Equal("shadingVariant", shadingVariant.GetName());
        Assert.True(variantSets.HasVariantSet("shadingVariant"));
    }

    [Fact]
    public void UsdVariantSets_ArrayAccess_ShouldWork()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Act
        var variant = variantSets["testVariant"];

        // Assert
        Assert.True(variant.IsValid());
        Assert.Equal("testVariant", variant.GetName());
    }

    [Fact]
    public void UsdVariantSets_AddMultipleVariantSets_ShouldTrackAll()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Act
        var modeling = variantSets.AddVariantSet("modelingVariant");
        var shading = variantSets.AddVariantSet("shadingVariant");
        var material = variantSets.AddVariantSet("materialVariant");

        // Assert
        Assert.Equal(3, variantSets.GetNumVariantSets());
        var names = variantSets.GetNames();
        Assert.Contains("modelingVariant", names);
        Assert.Contains("shadingVariant", names);
        Assert.Contains("materialVariant", names);
    }

    [Fact]
    public void UsdVariantSets_RemoveVariantSet_ShouldRemove()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        variantSets.AddVariantSet("variant1");
        variantSets.AddVariantSet("variant2");

        // Act
        var success = variantSets.RemoveVariantSet("variant1");

        // Assert
        Assert.True(success);
        Assert.False(variantSets.HasVariantSet("variant1"));
        Assert.True(variantSets.HasVariantSet("variant2"));
        Assert.Equal(1, variantSets.GetNumVariantSets());
    }

    [Fact]
    public void UsdVariantSets_ClearVariantSets_ShouldRemoveAll()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        variantSets.AddVariantSet("variant1");
        variantSets.AddVariantSet("variant2");

        // Act
        var success = variantSets.ClearVariantSets();

        // Assert
        Assert.True(success);
        Assert.Equal(0, variantSets.GetNumVariantSets());
        Assert.Empty(variantSets.GetNames());
        Assert.False(variantSets.HasAuthoredVariantSets());
    }

    #endregion

    #region Variant Management

    [Fact]
    public void UsdVariantSet_AddVariant_ShouldAddVariant()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");

        // Act
        var success1 = variantSet.AddVariant("high");
        var success2 = variantSet.AddVariant("medium");
        var success3 = variantSet.AddVariant("low");

        // Assert
        Assert.True(success1);
        Assert.True(success2);
        Assert.True(success3);
        Assert.Equal(3, variantSet.GetNumVariants());
        
        var variants = variantSet.GetVariantNames();
        Assert.Contains("high", variants);
        Assert.Contains("medium", variants);
        Assert.Contains("low", variants);
    }

    [Fact]
    public void UsdVariantSet_HasAuthoredVariant_ShouldDetectVariants()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");

        // Act
        variantSet.AddVariant("high");

        // Assert
        Assert.True(variantSet.HasAuthoredVariant("high"));
        Assert.False(variantSet.HasAuthoredVariant("medium"));
        Assert.False(variantSet.HasAuthoredVariant("nonexistent"));
    }

    [Fact]
    public void UsdVariantSet_AddDuplicateVariant_ShouldNotDuplicate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");

        // Act
        var success1 = variantSet.AddVariant("high");
        var success2 = variantSet.AddVariant("high"); // Duplicate

        // Assert
        Assert.True(success1);
        Assert.True(success2); // Should still return true
        Assert.Equal(1, variantSet.GetNumVariants()); // Should not duplicate
    }

    #endregion

    #region Selection Management

    [Fact]
    public void UsdVariantSet_SetVariantSelection_ShouldSetSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");
        variantSet.AddVariant("high");
        variantSet.AddVariant("low");

        // Act
        var success = variantSet.SetVariantSelection("high");

        // Assert
        Assert.True(success);
        Assert.Equal("high", variantSet.GetVariantSelection());
        Assert.True(variantSet.HasAuthoredVariantSelection());
    }

    [Fact]
    public void UsdVariantSet_ClearVariantSelection_ShouldClearSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");
        variantSet.AddVariant("high");
        variantSet.SetVariantSelection("high");

        // Act
        var success = variantSet.ClearVariantSelection();

        // Assert
        Assert.True(success);
        Assert.Empty(variantSet.GetVariantSelection());
        Assert.False(variantSet.HasAuthoredVariantSelection());
    }

    [Fact]
    public void UsdVariantSet_BlockVariantSelection_ShouldBlockSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");

        // Act
        var success = variantSet.BlockVariantSelection();

        // Assert
        Assert.True(success);
        Assert.Empty(variantSet.GetVariantSelection()); // Empty string
        Assert.True(variantSet.HasAuthoredVariantSelection()); // But authored
    }

    [Fact]
    public void UsdVariantSet_HasAuthoredVariantSelectionWithOutput_ShouldReturnSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");
        variantSet.AddVariant("high");
        variantSet.SetVariantSelection("high");

        // Act
        var hasSelection = variantSet.HasAuthoredVariantSelection(out string selection);

        // Assert
        Assert.True(hasSelection);
        Assert.Equal("high", selection);
    }

    #endregion

    #region Multiple Variant Sets Selection

    [Fact]
    public void UsdVariantSets_GetSetSelection_ShouldGetIndividualSelections()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        var modeling = variantSets.AddVariantSet("modelingVariant");
        modeling.AddVariant("high");
        modeling.SetVariantSelection("high");
        
        var shading = variantSets.AddVariantSet("shadingVariant");
        shading.AddVariant("complex");
        shading.SetVariantSelection("complex");

        // Act
        var modelingSelection = variantSets.GetVariantSelection("modelingVariant");
        var shadingSelection = variantSets.GetVariantSelection("shadingVariant");

        // Assert
        Assert.Equal("high", modelingSelection);
        Assert.Equal("complex", shadingSelection);
    }

    [Fact]
    public void UsdVariantSets_SetSelection_ShouldSetIndividualSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        var modeling = variantSets.AddVariantSet("modelingVariant");
        modeling.AddVariant("high");

        // Act
        var success = variantSets.SetSelection("modelingVariant", "high");

        // Assert
        Assert.True(success);
        Assert.Equal("high", modeling.GetVariantSelection());
    }

    [Fact]
    public void UsdVariantSets_GetAllVariantSelections_ShouldReturnAllSelections()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        var modeling = variantSets.AddVariantSet("modelingVariant");
        modeling.AddVariant("high");
        modeling.SetVariantSelection("high");
        
        var shading = variantSets.AddVariantSet("shadingVariant");
        shading.AddVariant("complex");
        shading.SetVariantSelection("complex");

        // No selection on this one
        variantSets.AddVariantSet("materialVariant");

        // Act
        var selections = variantSets.GetAllVariantSelections();

        // Assert
        Assert.Equal(2, selections.Count);
        Assert.Equal("high", selections["modelingVariant"]);
        Assert.Equal("complex", selections["shadingVariant"]);
        Assert.False(selections.ContainsKey("materialVariant")); // No selection
    }

    [Fact]
    public void UsdVariantSets_SetSelections_ShouldSetMultipleSelections()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        var modeling = variantSets.AddVariantSet("modelingVariant");
        modeling.AddVariant("high");
        modeling.AddVariant("low");
        
        var shading = variantSets.AddVariantSet("shadingVariant");
        shading.AddVariant("complex");
        shading.AddVariant("simple");

        var selections = new System.Collections.Generic.Dictionary<string, string>
        {
            { "modelingVariant", "high" },
            { "shadingVariant", "simple" }
        };

        // Act
        var success = variantSets.SetSelections(selections);

        // Assert
        Assert.True(success);
        Assert.Equal("high", modeling.GetVariantSelection());
        Assert.Equal("simple", shading.GetVariantSelection());
    }

    #endregion

    #region Edit Context Support

    [Fact]
    public void UsdVariantSet_GetVariantEditTarget_ShouldReturnEditTarget()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");
        variantSet.AddVariant("high");
        variantSet.SetVariantSelection("high");

        // Act
        var editTarget = variantSet.GetVariantEditTarget();

        // Assert
        Assert.True(editTarget.IsValid());
    }

    [Fact]
    public void UsdVariantSet_GetVariantEditContext_ShouldReturnEditContext()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modelingVariant");
        variantSet.AddVariant("high");
        variantSet.SetVariantSelection("high");

        // Act
        var editContext = variantSet.GetVariantEditContext();

        // Assert
        // Basic validation - the context should be valid if we have a selection
        // More detailed testing would require actual variant path mapping
        // UsdEditContext is a value type, so just verify it was created
    }

    #endregion

    #region Iteration Support

    [Fact]
    public void UsdVariantSets_GetVariantSets_ShouldReturnAllVariantSets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        variantSets.AddVariantSet("variant1");
        variantSets.AddVariantSet("variant2");
        variantSets.AddVariantSet("variant3");

        // Act
        var allSets = variantSets.GetVariantSets().ToArray();

        // Assert
        Assert.Equal(3, allSets.Length);
        Assert.All(allSets, vs => Assert.True(vs.IsValid()));
    }

    [Fact]
    public void UsdVariantSets_GetVariantSetsWithNames_ShouldReturnNamedPairs()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        variantSets.AddVariantSet("modeling");
        variantSets.AddVariantSet("shading");

        // Act
        var namedSets = variantSets.GetVariantSetsWithNames().ToArray();

        // Assert
        Assert.Equal(2, namedSets.Length);
        Assert.Contains(namedSets, kvp => kvp.Key == "modeling" && kvp.Value.IsValid());
        Assert.Contains(namedSets, kvp => kvp.Key == "shading" && kvp.Value.IsValid());
    }

    #endregion

    #region Validation

    [Fact]
    public void UsdVariantSets_IsValidVariantSetName_ShouldValidateNames()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Act & Assert
        Assert.True(variantSets.IsValidVariantSetName("validName"));
        Assert.True(variantSets.IsValidVariantSetName("valid_name"));
        Assert.True(variantSets.IsValidVariantSetName("valid123"));
        Assert.False(variantSets.IsValidVariantSetName(""));
        Assert.False(variantSets.IsValidVariantSetName("invalid-name"));
        Assert.False(variantSets.IsValidVariantSetName("invalid.name"));
        Assert.False(variantSets.IsValidVariantSetName("invalid name"));
    }

    [Fact]
    public void UsdVariantSet_IsValidVariantName_ShouldValidateNames()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modeling");

        // Act & Assert
        Assert.True(variantSet.IsValidVariantName("valid"));
        Assert.True(variantSet.IsValidVariantName("valid_name"));
        Assert.True(variantSet.IsValidVariantName("valid123"));
        Assert.False(variantSet.IsValidVariantName(""));
        Assert.False(variantSet.IsValidVariantName("invalid-name"));
        Assert.False(variantSet.IsValidVariantName("invalid.name"));
        Assert.False(variantSet.IsValidVariantName("invalid name"));
    }

    #endregion

    #region Error Handling

    [Fact]
    public void UsdVariantSets_InvalidOperationsOnInvalidPrim_ShouldFail()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var variantSets = new UsdVariantSets(invalidPrim);

        // Act & Assert
        Assert.False(variantSets.AddVariantSet("test").IsValid());
        Assert.False(variantSets.GetVariantSet("test").IsValid());
        Assert.False(variantSets.SetSelection("test", "variant"));
        Assert.False(variantSets.RemoveVariantSet("test"));
        Assert.False(variantSets.ClearVariantSets());
    }

    [Fact]
    public void UsdVariantSet_InvalidOperationsOnInvalidVariantSet_ShouldFail()
    {
        // Arrange
        var invalidVariantSet = new UsdVariantSet();

        // Act & Assert
        Assert.False(invalidVariantSet.IsValid());
        Assert.False(invalidVariantSet.AddVariant("test"));
        Assert.False(invalidVariantSet.SetVariantSelection("test"));
        Assert.False(invalidVariantSet.ClearVariantSelection());
        Assert.False(invalidVariantSet.BlockVariantSelection());
        Assert.Empty(invalidVariantSet.GetVariantNames());
        Assert.Equal(0, invalidVariantSet.GetNumVariants());
    }

    #endregion

    #region Composition Support

    [Fact]
    public void UsdVariantSets_CompositionMethods_ShouldTrackAuthoring()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();

        // Initially no authoring
        Assert.False(variantSets.HasAuthoredVariantSets());
        Assert.False(variantSets.HasAuthoredVariantSelections());
        Assert.Equal(0, variantSets.GetTotalVariantCount());

        // Act - Add variant sets and variants
        var modeling = variantSets.AddVariantSet("modeling");
        modeling.AddVariant("high");
        modeling.AddVariant("low");
        modeling.SetVariantSelection("high");

        var shading = variantSets.AddVariantSet("shading");
        shading.AddVariant("complex");

        // Assert
        Assert.True(variantSets.HasAuthoredVariantSets());
        Assert.True(variantSets.HasAuthoredVariantSelections());
        Assert.Equal(3, variantSets.GetTotalVariantCount()); // 2 + 1
    }

    #endregion

    #region String Representation

    [Fact]
    public void UsdVariantSets_ToString_ShouldShowPrimAndCounts()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSets = prim.GetVariantSets();
        
        var modeling = variantSets.AddVariantSet("modeling");
        modeling.AddVariant("high");
        modeling.AddVariant("low");
        modeling.SetVariantSelection("high");

        // Act
        var result = variantSets.ToString();

        // Assert
        Assert.Contains("/testPrim", result);
        Assert.Contains("1 sets", result);
        Assert.Contains("2 variants", result);
        Assert.Contains("1 selections", result);
    }

    [Fact]
    public void UsdVariantSet_ToString_ShouldShowNameAndSelection()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var variantSet = prim.GetVariantSet("modeling");
        variantSet.AddVariant("high");
        variantSet.AddVariant("low");
        variantSet.SetVariantSelection("high");

        // Act
        var result = variantSet.ToString();

        // Assert
        Assert.Contains("/testPrim", result);
        Assert.Contains("[modeling]", result);
        Assert.Contains("2 variants", result);
        Assert.Contains("selected='high'", result);
    }

    [Fact]
    public void UsdVariantSets_ToStringInvalid_ShouldShowInvalid()
    {
        // Arrange
        var invalidVariantSets = new UsdVariantSets();

        // Act
        var result = invalidVariantSets.ToString();

        // Assert
        Assert.Equal("UsdVariantSets(invalid)", result);
    }

    [Fact]
    public void UsdVariantSet_ToStringInvalid_ShouldShowInvalid()
    {
        // Arrange
        var invalidVariantSet = new UsdVariantSet();

        // Act
        var result = invalidVariantSet.ToString();

        // Assert
        Assert.Equal("UsdVariantSet(invalid)", result);
    }

    #endregion

    #region Integration with UsdPrim

    [Fact]
    public void UsdPrim_GetVariantSet_ShouldReturnSameAsGetVariantSets()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var variantSet1 = prim.GetVariantSet("modeling");
        var variantSet2 = prim.GetVariantSets().GetVariantSet("modeling");

        // Assert
        Assert.Equal(variantSet1.GetName(), variantSet2.GetName());
        Assert.Same(variantSet1.GetPrim(), variantSet2.GetPrim());
    }

    #endregion
}