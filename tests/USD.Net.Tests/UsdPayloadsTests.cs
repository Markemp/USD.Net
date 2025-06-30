using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdPayloadsTests
{
    #region Basic Functionality

    [Fact]
    public void UsdPayloads_CreateFromPrim_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var payloads = prim.GetPayloads();

        // Assert
        Assert.True(payloads.IsValid());
        Assert.Same(prim, payloads.GetPrim());
        Assert.False(payloads.HasPayloads());
        Assert.Equal(0, payloads.GetNumPayloads());
        Assert.True(payloads.IsLoadable() || !payloads.HasPayloads()); // Loadable if has payloads
    }

    [Fact]
    public void UsdPayloads_InvalidPrim_ShouldBeInvalid()
    {
        // Arrange
        var invalidPrim = new UsdPrim();

        // Act
        var payloads = new UsdPayloads(invalidPrim);

        // Assert
        Assert.False(payloads.IsValid());
        Assert.False(payloads.HasPayloads());
        Assert.False(payloads.IsLoaded());
        Assert.False(payloads.IsLoadable());
    }

    #endregion

    #region External Payloads

    [Fact]
    public void UsdPayloads_AddExternalPayload_ShouldAddPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        var success = payloads.AddPayload("/path/to/asset.usd", new SdfPath("/AssetPrim"));

        // Assert
        Assert.True(success);
        Assert.True(payloads.HasPayloads());
        Assert.Equal(1, payloads.GetNumPayloads());
        Assert.True(payloads.IsLoadable());
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.Equal("/path/to/asset.usd", loads[0].GetAssetPath());
        Assert.Equal("/AssetPrim", loads[0].GetPrimPath().GetString());
        Assert.False(loads[0].IsInternal());
        Assert.False(loads[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdPayloads_AddExternalPayloadDefaultPrim_ShouldAddPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        var success = payloads.AddPayload("/path/to/asset.usd");

        // Assert
        Assert.True(success);
        Assert.True(payloads.HasPayloads());
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.Equal("/path/to/asset.usd", loads[0].GetAssetPath());
        Assert.True(loads[0].GetPrimPath().IsEmpty());
        Assert.False(loads[0].IsInternal());
        Assert.True(loads[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdPayloads_AddExternalPayloadWithLayerOffset_ShouldAddPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        var layerOffset = new SdfLayerOffset(10.0, 2.0);

        // Act
        var success = payloads.AddPayload("/path/to/asset.usd", new SdfPath("/AssetPrim"), layerOffset);

        // Assert
        Assert.True(success);
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.Equal("/path/to/asset.usd", loads[0].GetAssetPath());
        Assert.Equal("/AssetPrim", loads[0].GetPrimPath().GetString());
        Assert.True(loads[0].HasLayerOffset());
        Assert.Equal(layerOffset, loads[0].GetLayerOffset());
    }

    #endregion

    #region Internal Payloads

    [Fact]
    public void UsdPayloads_AddInternalPayload_ShouldAddPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        var success = payloads.AddInternalPayload(new SdfPath("/OtherPrim"));

        // Assert
        Assert.True(success);
        Assert.True(payloads.HasPayloads());
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.Empty(loads[0].GetAssetPath());
        Assert.Equal("/OtherPrim", loads[0].GetPrimPath().GetString());
        Assert.True(loads[0].IsInternal());
        Assert.False(loads[0].IsDefaultPrim());
    }

    [Fact]
    public void UsdPayloads_AddInternalPayloadWithLayerOffset_ShouldAddPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        var layerOffset = new SdfLayerOffset(5.0, 0.5);

        // Act
        var success = payloads.AddInternalPayload(new SdfPath("/OtherPrim"), layerOffset);

        // Assert
        Assert.True(success);
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.True(loads[0].IsInternal());
        Assert.True(loads[0].HasLayerOffset());
        Assert.Equal(layerOffset, loads[0].GetLayerOffset());
    }

    [Fact]
    public void UsdPayloads_AddInternalPayloadEmptyPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        var success = payloads.AddInternalPayload(SdfPath.EmptyPath());

        // Assert
        Assert.False(success);
        Assert.False(payloads.HasPayloads());
    }

    #endregion

    #region Payload Management

    [Fact]
    public void UsdPayloads_AddMultiplePayloads_ShouldAddAll()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        payloads.AddPayload("/asset1.usd");
        payloads.AddPayload("/asset2.usd", new SdfPath("/Prim2"));
        payloads.AddInternalPayload(new SdfPath("/InternalPrim"));

        // Assert
        Assert.Equal(3, payloads.GetNumPayloads());
        
        var loads = payloads.GetPayloads();
        Assert.Equal(3, loads.Length);
        
        // Verify each payload
        Assert.Contains(loads, p => p.GetAssetPath() == "/asset1.usd" && p.IsDefaultPrim());
        Assert.Contains(loads, p => p.GetAssetPath() == "/asset2.usd" && p.GetPrimPath().GetString() == "/Prim2");
        Assert.Contains(loads, p => p.IsInternal() && p.GetPrimPath().GetString() == "/InternalPrim");
    }

    [Fact]
    public void UsdPayloads_AddDuplicatePayload_ShouldNotDuplicate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act
        var success1 = payloads.AddPayload("/asset.usd");
        var success2 = payloads.AddPayload("/asset.usd"); // Duplicate

        // Assert
        Assert.True(success1);
        Assert.True(success2); // Should return true (already exists)
        Assert.Equal(1, payloads.GetNumPayloads()); // Should not duplicate
    }

    [Fact]
    public void UsdPayloads_RemovePayload_ShouldRemoveSpecificPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/asset1.usd");
        payloads.AddPayload("/asset2.usd");
        payloads.AddInternalPayload(new SdfPath("/InternalPrim"));

        // Act
        var success = payloads.RemovePayload("/asset1.usd");

        // Assert
        Assert.True(success);
        Assert.Equal(2, payloads.GetNumPayloads());
        Assert.False(payloads.HasPayload("/asset1.usd"));
        Assert.True(payloads.HasPayload("/asset2.usd"));
    }

    [Fact]
    public void UsdPayloads_RemoveInternalPayload_ShouldRemoveSpecificPayload()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddInternalPayload(new SdfPath("/InternalPrim1"));
        payloads.AddInternalPayload(new SdfPath("/InternalPrim2"));

        // Act
        var success = payloads.RemoveInternalPayload(new SdfPath("/InternalPrim1"));

        // Assert
        Assert.True(success);
        Assert.Equal(1, payloads.GetNumPayloads());
        
        var loads = payloads.GetPayloads();
        Assert.Single(loads);
        Assert.Equal("/InternalPrim2", loads[0].GetPrimPath().GetString());
    }

    [Fact]
    public void UsdPayloads_ClearPayloads_ShouldRemoveAllPayloads()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/asset1.usd");
        payloads.AddPayload("/asset2.usd");
        payloads.AddInternalPayload(new SdfPath("/InternalPrim"));

        // Act
        var success = payloads.ClearPayloads();

        // Assert
        Assert.True(success);
        Assert.False(payloads.HasPayloads());
        Assert.Equal(0, payloads.GetNumPayloads());
        Assert.Empty(payloads.GetPayloads());
        Assert.False(payloads.IsLoadable());
    }

    [Fact]
    public void UsdPayloads_SetPayloads_ShouldReplaceAllPayloads()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        // Add initial payloads
        payloads.AddPayload("/old1.usd");
        payloads.AddPayload("/old2.usd");
        
        // New payloads to set
        var newPayloads = new[]
        {
            SdfPayload.CreateExternal("/new1.usd"),
            SdfPayload.CreateExternal("/new2.usd", new SdfPath("/NewPrim")),
            SdfPayload.CreateInternal(new SdfPath("/NewInternal"))
        };

        // Act
        var success = payloads.SetPayloads(newPayloads);

        // Assert
        Assert.True(success);
        Assert.Equal(3, payloads.GetNumPayloads());
        
        var loads = payloads.GetPayloads();
        Assert.Contains(loads, p => p.GetAssetPath() == "/new1.usd");
        Assert.Contains(loads, p => p.GetAssetPath() == "/new2.usd");
        Assert.Contains(loads, p => p.IsInternal() && p.GetPrimPath().GetString() == "/NewInternal");
        
        // Old payloads should be gone
        Assert.DoesNotContain(loads, p => p.GetAssetPath() == "/old1.usd");
        Assert.DoesNotContain(loads, p => p.GetAssetPath() == "/old2.usd");
    }

    #endregion

    #region List Position

    [Fact]
    public void UsdPayloads_AddPayloadWithPosition_ShouldRespectPosition()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act - Add payloads with different positions
        payloads.AddPayload("/asset1.usd", position: UsdListPosition.BackOfPrependList);
        payloads.AddPayload("/asset2.usd", position: UsdListPosition.FrontOfPrependList);
        payloads.AddPayload("/asset3.usd", position: UsdListPosition.BackOfAppendList);

        // Assert
        Assert.Equal(3, payloads.GetNumPayloads());
        
        var loads = payloads.GetPayloads();
        // Front of prepend should be first
        Assert.Equal("/asset2.usd", loads[0].GetAssetPath());
    }

    #endregion

    #region Payload Filtering

    [Fact]
    public void UsdPayloads_GetExternalPayloads_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/external1.usd");
        payloads.AddPayload("/external2.usd");
        payloads.AddInternalPayload(new SdfPath("/InternalPrim"));

        // Act
        var externalPayloads = payloads.GetExternalPayloads();

        // Assert
        Assert.Equal(2, externalPayloads.Length);
        Assert.All(externalPayloads, p => Assert.False(p.IsInternal()));
        Assert.Contains(externalPayloads, p => p.GetAssetPath() == "/external1.usd");
        Assert.Contains(externalPayloads, p => p.GetAssetPath() == "/external2.usd");
    }

    [Fact]
    public void UsdPayloads_GetInternalPayloads_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/external.usd");
        payloads.AddInternalPayload(new SdfPath("/Internal1"));
        payloads.AddInternalPayload(new SdfPath("/Internal2"));

        // Act
        var internalPayloads = payloads.GetInternalPayloads();

        // Assert
        Assert.Equal(2, internalPayloads.Length);
        Assert.All(internalPayloads, p => Assert.True(p.IsInternal()));
        Assert.Contains(internalPayloads, p => p.GetPrimPath().GetString() == "/Internal1");
        Assert.Contains(internalPayloads, p => p.GetPrimPath().GetString() == "/Internal2");
    }

    [Fact]
    public void UsdPayloads_GetDefaultPrimPayloads_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/asset1.usd"); // Default prim
        payloads.AddPayload("/asset2.usd", new SdfPath("/SpecificPrim")); // Specific prim
        payloads.AddPayload("/asset3.usd"); // Default prim

        // Act
        var defaultPrimPayloads = payloads.GetDefaultPrimPayloads();

        // Assert
        Assert.Equal(2, defaultPrimPayloads.Length);
        Assert.All(defaultPrimPayloads, p => Assert.True(p.IsDefaultPrim()));
        Assert.Contains(defaultPrimPayloads, p => p.GetAssetPath() == "/asset1.usd");
        Assert.Contains(defaultPrimPayloads, p => p.GetAssetPath() == "/asset3.usd");
    }

    [Fact]
    public void UsdPayloads_GetPayloadsWithLayerOffsets_ShouldFilterCorrectly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/asset1.usd"); // No offset
        payloads.AddPayload("/asset2.usd", new SdfPath("/Prim"), new SdfLayerOffset(10.0, 1.0)); // With offset
        payloads.AddInternalPayload(new SdfPath("/Internal"), new SdfLayerOffset(5.0, 2.0)); // With offset

        // Act
        var payloadsWithOffsets = payloads.GetPayloadsWithLayerOffsets();

        // Assert
        Assert.Equal(2, payloadsWithOffsets.Length);
        Assert.All(payloadsWithOffsets, p => Assert.True(p.HasLayerOffset()));
        Assert.Contains(payloadsWithOffsets, p => p.GetAssetPath() == "/asset2.usd");
        Assert.Contains(payloadsWithOffsets, p => p.IsInternal());
    }

    #endregion

    #region Error Handling

    [Fact]
    public void UsdPayloads_InvalidOperationsOnInvalidPrim_ShouldFail()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var payloads = new UsdPayloads(invalidPrim);

        // Act & Assert
        Assert.False(payloads.AddPayload("/asset.usd"));
        Assert.False(payloads.AddInternalPayload(new SdfPath("/prim")));
        Assert.False(payloads.RemovePayload("/asset.usd"));
        Assert.False(payloads.ClearPayloads());
        Assert.False(payloads.SetPayloads([]));
    }

    [Fact]
    public void UsdPayloads_AddPayloadEmptyAssetPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Act & Assert
        Assert.False(payloads.AddPayload(""));
        Assert.False(payloads.AddPayload(null!));
        Assert.False(payloads.HasPayloads());
    }

    #endregion

    #region Loading State

    [Fact]
    public void UsdPayloads_LoadingState_ShouldReflectPayloadPresence()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();

        // Initially no payloads
        Assert.False(payloads.IsLoadable());
        Assert.False(payloads.IsLoaded());

        // Act - Add payload
        payloads.AddPayload("/asset.usd");

        // Assert
        Assert.True(payloads.IsLoadable());
        Assert.True(payloads.IsLoaded()); // For now, assume loaded if has payloads
    }

    #endregion

    #region String Representation

    [Fact]
    public void UsdPayloads_ToString_ShouldShowPrimAndCount()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var payloads = prim.GetPayloads();
        
        payloads.AddPayload("/asset1.usd");
        payloads.AddPayload("/asset2.usd");

        // Act
        var result = payloads.ToString();

        // Assert
        Assert.Contains("/testPrim", result);
        Assert.Contains("2 payloads", result);
        Assert.Contains("loaded", result); // Should show loading state
    }

    [Fact]
    public void UsdPayloads_ToStringInvalid_ShouldShowInvalid()
    {
        // Arrange
        var invalidPayloads = new UsdPayloads();

        // Act
        var result = invalidPayloads.ToString();

        // Assert
        Assert.Equal("UsdPayloads(invalid)", result);
    }

    #endregion
}