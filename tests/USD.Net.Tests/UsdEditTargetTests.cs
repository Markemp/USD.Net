using System;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdEditTargetTests
{
    #region Construction and Basic Properties

    [Fact]
    public void UsdEditTarget_DefaultConstructor_ShouldCreateNullTarget()
    {
        // Act
        var editTarget = new UsdEditTarget();

        // Assert
        Assert.True(editTarget.IsNull());
        Assert.False(editTarget.IsValid());
        Assert.Null(editTarget.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_WithLayer_ShouldCreateValidTarget()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://layer");

        // Act
        var editTarget = new UsdEditTarget(layer);

        // Assert
        Assert.False(editTarget.IsNull());
        Assert.True(editTarget.IsValid());
        Assert.Same(layer, editTarget.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_WithNullLayer_ShouldCreateNullTarget()
    {
        // Act
        var editTarget = new UsdEditTarget(null!);

        // Assert
        Assert.True(editTarget.IsNull());
        Assert.False(editTarget.IsValid());
        Assert.Null(editTarget.GetLayer());
    }

    #endregion

    #region Factory Methods

    [Fact]
    public void UsdEditTarget_ForLocalLayer_ShouldCreateValidTarget()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://local");

        // Act
        var editTarget = UsdEditTarget.ForLocalLayer(layer);

        // Assert
        Assert.True(editTarget.IsValid());
        Assert.Same(layer, editTarget.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_ForSessionLayer_ShouldCreateValidTarget()
    {
        // Arrange
        var sessionLayer = SdfLayer.CreateAnonymous("test://session");

        // Act
        var editTarget = UsdEditTarget.ForSessionLayer(sessionLayer);

        // Assert
        Assert.True(editTarget.IsValid());
        Assert.Same(sessionLayer, editTarget.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_ForLocalDirectVariant_ShouldCreateValidTarget()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://variant");
        var variantPath = new SdfPath("/prim{variantSet=variant}");

        // Act
        var editTarget = UsdEditTarget.ForLocalDirectVariant(layer, variantPath);

        // Assert
        Assert.True(editTarget.IsValid());
        Assert.Same(layer, editTarget.GetLayer());
    }

    #endregion

    #region Path Mapping

    [Fact]
    public void UsdEditTarget_MapToSpecPath_ValidPath_ShouldReturnMappedPath()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://mapping");
        var editTarget = new UsdEditTarget(layer);
        var scenePath = new SdfPath("/prim");

        // Act
        var specPath = editTarget.MapToSpecPath(scenePath);

        // Assert
        Assert.False(specPath.IsEmpty());
        Assert.Equal("/prim", specPath.GetString()); // Identity mapping
    }

    [Fact]
    public void UsdEditTarget_MapToSpecPath_EmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://empty");
        var editTarget = new UsdEditTarget(layer);

        // Act
        var specPath = editTarget.MapToSpecPath(SdfPath.EmptyPath());

        // Assert
        Assert.True(specPath.IsEmpty());
    }

    [Fact]
    public void UsdEditTarget_MapToSpecPath_NullTarget_ShouldReturnEmpty()
    {
        // Arrange
        var editTarget = new UsdEditTarget();
        var scenePath = new SdfPath("/prim");

        // Act
        var specPath = editTarget.MapToSpecPath(scenePath);

        // Assert
        Assert.True(specPath.IsEmpty());
    }

    #endregion

    #region Spec Access

    [Fact]
    public void UsdEditTarget_GetPrimSpecForScenePath_ShouldReturnPrimSpec()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://primspec");
        var editTarget = new UsdEditTarget(layer);
        var primPath = new SdfPath("/prim");

        // Act
        var primSpec = editTarget.GetPrimSpecForScenePath(primPath);

        // Assert
        Assert.NotNull(primSpec);
        Assert.Same(layer, primSpec.GetLayer());
        Assert.Equal(primPath, primSpec.GetPath());
    }

    [Fact]
    public void UsdEditTarget_GetPropertySpecForScenePath_ShouldReturnPropertySpec()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://propspec");
        var editTarget = new UsdEditTarget(layer);
        var propertyPath = new SdfPath("/prim.attr");

        // Act
        var propertySpec = editTarget.GetPropertySpecForScenePath(propertyPath);

        // Assert
        Assert.NotNull(propertySpec);
    }

    [Fact]
    public void UsdEditTarget_GetPropertySpecForScenePath_PrimPath_ShouldReturnNull()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://propspec_prim");
        var editTarget = new UsdEditTarget(layer);
        var primPath = new SdfPath("/prim");

        // Act
        var propertySpec = editTarget.GetPropertySpecForScenePath(primPath);

        // Assert
        Assert.Null(propertySpec);
    }

    [Fact]
    public void UsdEditTarget_GetSpecForScenePath_PrimPath_ShouldReturnPrimSpec()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://anyspec");
        var editTarget = new UsdEditTarget(layer);
        var primPath = new SdfPath("/prim");

        // Act
        var spec = editTarget.GetSpecForScenePath(primPath);

        // Assert
        Assert.NotNull(spec);
        Assert.IsType<SdfPrimSpec>(spec);
        var primSpec = (SdfPrimSpec)spec;
        Assert.Same(layer, primSpec.GetLayer());
        Assert.Equal(primPath, primSpec.GetPath());
    }

    [Fact]
    public void UsdEditTarget_GetSpecForScenePath_PropertyPath_ShouldReturnPropertySpec()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://anyspec_prop");
        var editTarget = new UsdEditTarget(layer);
        var propertyPath = new SdfPath("/prim.attr");

        // Act
        var spec = editTarget.GetSpecForScenePath(propertyPath);

        // Assert
        Assert.NotNull(spec);
    }

    #endregion

    #region Composition

    [Fact]
    public void UsdEditTarget_ComposeOver_ValidTargets_ShouldReturnStronger()
    {
        // Arrange
        var strongerLayer = SdfLayer.CreateAnonymous("test://stronger");
        var weakerLayer = SdfLayer.CreateAnonymous("test://weaker");
        var stronger = new UsdEditTarget(strongerLayer);
        var weaker = new UsdEditTarget(weakerLayer);

        // Act
        var composed = stronger.ComposeOver(weaker);

        // Assert
        Assert.Equal(stronger, composed);
        Assert.Same(strongerLayer, composed.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_ComposeOver_StrongerNull_ShouldReturnWeaker()
    {
        // Arrange
        var weakerLayer = SdfLayer.CreateAnonymous("test://weaker_only");
        var stronger = new UsdEditTarget();
        var weaker = new UsdEditTarget(weakerLayer);

        // Act
        var composed = stronger.ComposeOver(weaker);

        // Assert
        Assert.Equal(weaker, composed);
        Assert.Same(weakerLayer, composed.GetLayer());
    }

    [Fact]
    public void UsdEditTarget_ComposeOver_WeakerNull_ShouldReturnStronger()
    {
        // Arrange
        var strongerLayer = SdfLayer.CreateAnonymous("test://stronger_only");
        var stronger = new UsdEditTarget(strongerLayer);
        var weaker = new UsdEditTarget();

        // Act
        var composed = stronger.ComposeOver(weaker);

        // Assert
        Assert.Equal(stronger, composed);
        Assert.Same(strongerLayer, composed.GetLayer());
    }

    #endregion

    #region Equality and Hashing

    [Fact]
    public void UsdEditTarget_Equality_SameLayer_ShouldBeEqual()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://equality");
        var editTarget1 = new UsdEditTarget(layer);
        var editTarget2 = new UsdEditTarget(layer);

        // Act & Assert
        Assert.True(editTarget1.Equals(editTarget2));
        Assert.True(editTarget1 == editTarget2);
        Assert.False(editTarget1 != editTarget2);
        Assert.Equal(editTarget1.GetHashCode(), editTarget2.GetHashCode());
    }

    [Fact]
    public void UsdEditTarget_Equality_DifferentLayers_ShouldNotBeEqual()
    {
        // Arrange
        var layer1 = SdfLayer.CreateAnonymous("test://layer1");
        var layer2 = SdfLayer.CreateAnonymous("test://layer2");
        var editTarget1 = new UsdEditTarget(layer1);
        var editTarget2 = new UsdEditTarget(layer2);

        // Act & Assert
        Assert.False(editTarget1.Equals(editTarget2));
        Assert.False(editTarget1 == editTarget2);
        Assert.True(editTarget1 != editTarget2);
        // Hash codes might be different but are not required to be
    }

    [Fact]
    public void UsdEditTarget_Equality_NullTargets_ShouldBeEqual()
    {
        // Arrange
        var editTarget1 = new UsdEditTarget();
        var editTarget2 = new UsdEditTarget();

        // Act & Assert
        Assert.True(editTarget1.Equals(editTarget2));
        Assert.True(editTarget1 == editTarget2);
        Assert.False(editTarget1 != editTarget2);
    }

    #endregion

    #region String Representation

    [Fact]
    public void UsdEditTarget_ToString_ValidTarget_ShouldShowLayerName()
    {
        // Arrange
        var layer = SdfLayer.CreateAnonymous("test://tostring");
        var editTarget = new UsdEditTarget(layer);

        // Act
        var result = editTarget.ToString();

        // Assert
        Assert.Contains("UsdEditTarget", result);
        Assert.Contains("test://tostring", result);
    }

    [Fact]
    public void UsdEditTarget_ToString_NullTarget_ShouldShowNull()
    {
        // Arrange
        var editTarget = new UsdEditTarget();

        // Act
        var result = editTarget.ToString();

        // Assert
        Assert.Equal("UsdEditTarget(null)", result);
    }

    #endregion

    #region UsdPathMapping Tests

    [Fact]
    public void UsdPathMapping_Identity_ShouldMapUnchanged()
    {
        // Arrange
        var mapping = UsdPathMapping.Identity();
        var path = new SdfPath("/test/path");

        // Act
        var mappedPath = mapping.MapSourceToTarget(path);

        // Assert
        Assert.Equal(path, mappedPath);
    }

    [Fact]
    public void UsdPathMapping_EmptyPath_ShouldReturnEmpty()
    {
        // Arrange
        var mapping = UsdPathMapping.Identity();

        // Act
        var mappedPath = mapping.MapSourceToTarget(SdfPath.EmptyPath());

        // Assert
        Assert.True(mappedPath.IsEmpty());
    }

    [Fact]
    public void UsdPathMapping_VariantMapping_ShouldReturnIdentityForNow()
    {
        // Arrange
        var variantPath = new SdfPath("/prim{variant=selection}");
        var mapping = UsdPathMapping.CreateVariantMapping(variantPath);
        var testPath = new SdfPath("/test");

        // Act
        var mappedPath = mapping.MapSourceToTarget(testPath);

        // Assert
        // For now, variant mapping returns identity
        Assert.Equal(testPath, mappedPath);
    }

    [Fact]
    public void UsdPathMapping_Equality_ShouldWork()
    {
        // Arrange
        var mapping1 = UsdPathMapping.Identity();
        var mapping2 = UsdPathMapping.Identity();
        var variantPath = new SdfPath("/prim{variant=selection}");
        var mapping3 = UsdPathMapping.CreateVariantMapping(variantPath);

        // Act & Assert
        Assert.True(mapping1.Equals(mapping2));
        Assert.True(mapping1 == mapping2);
        Assert.False(mapping1.Equals(mapping3));
        Assert.False(mapping1 == mapping3);
    }

    #endregion
}