using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdReferencesTests
{
    #region Basic Functionality

    [Fact]
    public void UsdReferences_CreateFromPrim_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var references = prim.GetReferences();

        // Assert
        Assert.True(references.IsValid());
        Assert.Same(prim, references.GetPrim());
        Assert.False(references.HasReferences());
        Assert.Equal(0, references.GetNumReferences());
    }

    [Fact]
    public void UsdReferences_InvalidPrim_ShouldBeInvalid()
    {
        // Arrange
        var invalidPrim = new UsdPrim();

        // Act
        var references = new UsdReferences(invalidPrim);

        // Assert
        Assert.False(references.IsValid());
        Assert.False(references.HasReferences());
    }

    #endregion

    #region External References

    [Fact]
    public void UsdReferences_AddExternalReference_ShouldAddReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        var success = references.AddReference("/path/to/asset.usd", new SdfPath("/AssetPrim"));

        // Assert
        Assert.True(success);
        Assert.True(references.HasReferences());
        Assert.Equal(1, references.GetNumReferences());
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.Equal("/path/to/asset.usd", refs[0].GetAssetPath());
        Assert.Equal("/AssetPrim", refs[0].GetPrimPath().GetString());
        Assert.False(refs[0].IsInternal());
        Assert.False(refs[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdReferences_AddExternalReferenceDefaultPrim_ShouldAddReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        var success = references.AddReference("/path/to/asset.usd");

        // Assert
        Assert.True(success);
        Assert.True(references.HasReferences());
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.Equal("/path/to/asset.usd", refs[0].GetAssetPath());
        Assert.True(refs[0].GetPrimPath().IsEmpty());
        Assert.False(refs[0].IsInternal());
        Assert.True(refs[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdReferences_AddExternalReferenceWithLayerOffset_ShouldAddReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        var layerOffset = new SdfLayerOffset(10.0, 2.0);

        // Act
        var success = references.AddReference("/path/to/asset.usd", new SdfPath("/AssetPrim"), layerOffset);

        // Assert
        Assert.True(success);
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.Equal("/path/to/asset.usd", refs[0].GetAssetPath());
        Assert.Equal("/AssetPrim", refs[0].GetPrimPath().GetString());
        Assert.True(refs[0].HasLayerOffset());
        Assert.Equal(layerOffset, refs[0].GetLayerOffset());
    }

    #endregion

    #region Internal References

    [Fact]
    public void UsdReferences_AddInternalReference_ShouldAddReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        var success = references.AddInternalReference(new SdfPath("/OtherPrim"));

        // Assert
        Assert.True(success);
        Assert.True(references.HasReferences());
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.Empty(refs[0].GetAssetPath());
        Assert.Equal("/OtherPrim", refs[0].GetPrimPath().GetString());
        Assert.True(refs[0].IsInternal());
        Assert.False(refs[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdReferences_AddInternalReferenceWithLayerOffset_ShouldAddReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        var layerOffset = new SdfLayerOffset(5.0, 0.5);

        // Act
        var success = references.AddInternalReference(new SdfPath("/OtherPrim"), layerOffset);

        // Assert
        Assert.True(success);
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.True(refs[0].IsInternal());
        Assert.True(refs[0].HasLayerOffset());
        Assert.Equal(layerOffset, refs[0].GetLayerOffset());
    }

    [Fact]
    public void UsdReferences_AddInternalReferenceEmptyPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        var success = references.AddInternalReference(SdfPath.EmptyPath());

        // Assert
        Assert.False(success);
        Assert.False(references.HasReferences());
    }

    #endregion

    #region Reference Management

    [Fact]
    public void UsdReferences_AddMultipleReferences_ShouldAddAll()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        references.AddReference("/asset1.usd");
        references.AddReference("/asset2.usd", new SdfPath("/Prim2"));
        references.AddInternalReference(new SdfPath("/InternalPrim"));

        // Assert
        Assert.Equal(3, references.GetNumReferences());
        
        var refs = references.GetReferences();
        Assert.Equal(3, refs.Length);
        
        // Verify each reference
        Assert.Contains(refs, r => r.GetAssetPath() == "/asset1.usd" && r.IsDefaultPrim());
        Assert.Contains(refs, r => r.GetAssetPath() == "/asset2.usd" && r.GetPrimPath().GetString() == "/Prim2");
        Assert.Contains(refs, r => r.IsInternal() && r.GetPrimPath().GetString() == "/InternalPrim");
    }

    [Fact]
    public void UsdReferences_AddDuplicateReference_ShouldNotDuplicate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act
        var success1 = references.AddReference("/asset.usd");
        var success2 = references.AddReference("/asset.usd"); // Duplicate

        // Assert
        Assert.True(success1);
        Assert.True(success2); // Should return true (already exists)
        Assert.Equal(1, references.GetNumReferences()); // Should not duplicate
    }

    [Fact]
    public void UsdReferences_RemoveReference_ShouldRemoveSpecificReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/asset1.usd");
        references.AddReference("/asset2.usd");
        references.AddInternalReference(new SdfPath("/InternalPrim"));

        // Act
        var success = references.RemoveReference("/asset1.usd");

        // Assert
        Assert.True(success);
        Assert.Equal(2, references.GetNumReferences());
        Assert.False(references.HasReference("/asset1.usd"));
        Assert.True(references.HasReference("/asset2.usd"));
    }

    [Fact]
    public void UsdReferences_RemoveInternalReference_ShouldRemoveSpecificReference()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddInternalReference(new SdfPath("/InternalPrim1"));
        references.AddInternalReference(new SdfPath("/InternalPrim2"));

        // Act
        var success = references.RemoveInternalReference(new SdfPath("/InternalPrim1"));

        // Assert
        Assert.True(success);
        Assert.Equal(1, references.GetNumReferences());
        
        var refs = references.GetReferences();
        Assert.Single(refs);
        Assert.Equal("/InternalPrim2", refs[0].GetPrimPath().GetString());
    }

    [Fact]
    public void UsdReferences_ClearReferences_ShouldRemoveAllReferences()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/asset1.usd");
        references.AddReference("/asset2.usd");
        references.AddInternalReference(new SdfPath("/InternalPrim"));

        // Act
        var success = references.ClearReferences();

        // Assert
        Assert.True(success);
        Assert.False(references.HasReferences());
        Assert.Equal(0, references.GetNumReferences());
        Assert.Empty(references.GetReferences());
    }

    [Fact]
    public void UsdReferences_SetReferences_ShouldReplaceAllReferences()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        // Add initial references
        references.AddReference("/old1.usd");
        references.AddReference("/old2.usd");
        
        // New references to set
        var newReferences = new[]
        {
            SdfReference.CreateExternal("/new1.usd"),
            SdfReference.CreateExternal("/new2.usd", new SdfPath("/NewPrim")),
            SdfReference.CreateInternal(new SdfPath("/NewInternal"))
        };

        // Act
        var success = references.SetReferences(newReferences);

        // Assert
        Assert.True(success);
        Assert.Equal(3, references.GetNumReferences());
        
        var refs = references.GetReferences();
        Assert.Contains(refs, r => r.GetAssetPath() == "/new1.usd");
        Assert.Contains(refs, r => r.GetAssetPath() == "/new2.usd");
        Assert.Contains(refs, r => r.IsInternal() && r.GetPrimPath().GetString() == "/NewInternal");
        
        // Old references should be gone
        Assert.DoesNotContain(refs, r => r.GetAssetPath() == "/old1.usd");
        Assert.DoesNotContain(refs, r => r.GetAssetPath() == "/old2.usd");
    }

    #endregion

    #region List Position

    [Fact]
    public void UsdReferences_AddReferenceWithPosition_ShouldRespectPosition()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act - Add references with different positions
        references.AddReference("/asset1.usd", position: UsdListPosition.BackOfPrependList);
        references.AddReference("/asset2.usd", position: UsdListPosition.FrontOfPrependList);
        references.AddReference("/asset3.usd", position: UsdListPosition.BackOfAppendList);

        // Assert
        Assert.Equal(3, references.GetNumReferences());
        
        var refs = references.GetReferences();
        // Front of prepend should be first
        Assert.Equal("/asset2.usd", refs[0].GetAssetPath());
    }

    #endregion

    #region Reference Filtering

    [Fact]
    public void UsdReferences_GetExternalReferences_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/external1.usd");
        references.AddReference("/external2.usd");
        references.AddInternalReference(new SdfPath("/InternalPrim"));

        // Act
        var externalRefs = references.GetExternalReferences();

        // Assert
        Assert.Equal(2, externalRefs.Length);
        Assert.All(externalRefs, r => Assert.False(r.IsInternal()));
        Assert.Contains(externalRefs, r => r.GetAssetPath() == "/external1.usd");
        Assert.Contains(externalRefs, r => r.GetAssetPath() == "/external2.usd");
    }

    [Fact]
    public void UsdReferences_GetInternalReferences_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/external.usd");
        references.AddInternalReference(new SdfPath("/Internal1"));
        references.AddInternalReference(new SdfPath("/Internal2"));

        // Act
        var internalRefs = references.GetInternalReferences();

        // Assert
        Assert.Equal(2, internalRefs.Length);
        Assert.All(internalRefs, r => Assert.True(r.IsInternal()));
        Assert.Contains(internalRefs, r => r.GetPrimPath().GetString() == "/Internal1");
        Assert.Contains(internalRefs, r => r.GetPrimPath().GetString() == "/Internal2");
    }

    [Fact]
    public void UsdReferences_GetDefaultPrimReferences_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/asset1.usd"); // Default prim
        references.AddReference("/asset2.usd", new SdfPath("/SpecificPrim")); // Specific prim
        references.AddReference("/asset3.usd"); // Default prim

        // Act
        var defaultPrimRefs = references.GetDefaultPrimReferences();

        // Assert
        Assert.Equal(2, defaultPrimRefs.Length);
        Assert.All(defaultPrimRefs, r => Assert.True(r.IsDefaultPrim()));
        Assert.Contains(defaultPrimRefs, r => r.GetAssetPath() == "/asset1.usd");
        Assert.Contains(defaultPrimRefs, r => r.GetAssetPath() == "/asset3.usd");
    }

    [Fact]
    public void UsdReferences_GetReferencesWithLayerOffsets_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/asset1.usd"); // No offset
        references.AddReference("/asset2.usd", new SdfPath("/Prim"), new SdfLayerOffset(10.0, 1.0)); // With offset
        references.AddInternalReference(new SdfPath("/Internal"), new SdfLayerOffset(5.0, 2.0)); // With offset

        // Act
        var refsWithOffsets = references.GetReferencesWithLayerOffsets();

        // Assert
        Assert.Equal(2, refsWithOffsets.Length);
        Assert.All(refsWithOffsets, r => Assert.True(r.HasLayerOffset()));
        Assert.Contains(refsWithOffsets, r => r.GetAssetPath() == "/asset2.usd");
        Assert.Contains(refsWithOffsets, r => r.IsInternal());
    }

    #endregion

    #region Error Handling

    [Fact]
    public void UsdReferences_InvalidOperationsOnInvalidPrim_ShouldFail()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var references = new UsdReferences(invalidPrim);

        // Act & Assert
        Assert.False(references.AddReference("/asset.usd"));
        Assert.False(references.AddInternalReference(new SdfPath("/prim")));
        Assert.False(references.RemoveReference("/asset.usd"));
        Assert.False(references.ClearReferences());
        Assert.False(references.SetReferences([]));
    }

    [Fact]
    public void UsdReferences_AddReferenceEmptyAssetPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();

        // Act & Assert
        Assert.False(references.AddReference(""));
        Assert.False(references.AddReference(null!));
        Assert.False(references.HasReferences());
    }

    #endregion

    #region String Representation

    [Fact]
    public void UsdReferences_ToString_ShouldShowPrimAndCount()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var references = prim.GetReferences();
        
        references.AddReference("/asset1.usd");
        references.AddReference("/asset2.usd");

        // Act
        var result = references.ToString();

        // Assert
        Assert.Contains("/testPrim", result);
        Assert.Contains("2 references", result);
    }

    [Fact]
    public void UsdReferences_ToStringInvalid_ShouldShowInvalid()
    {
        // Arrange
        var invalidReferences = new UsdReferences();

        // Act
        var result = invalidReferences.ToString();

        // Assert
        Assert.Equal("UsdReferences(invalid)", result);
    }

    #endregion
}