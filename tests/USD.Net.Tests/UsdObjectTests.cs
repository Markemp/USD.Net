using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdObjectTests
{
    [Fact]
    public void UsdStage_CreateStage_ShouldWork()
    {
        // Arrange & Act
        var stage = UsdStage.CreateInMemory();

        // Assert
        Assert.NotNull(stage);
        Assert.NotNull(stage.GetRootLayer());
    }

    [Fact]
    public void UsdPrim_CreatePrim_ShouldHaveValidPath()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var path = new SdfPath("/testPrim");

        // Act
        var prim = stage.DefinePrim(path, "TestType");

        // Assert
        Assert.True(prim.IsValid());
        Assert.Equal(path, prim.GetPath());
        Assert.Equal("testPrim", prim.GetName());
        Assert.Equal("TestType", prim.GetTypeName());
        Assert.Same(stage, prim.GetStage());
    }

    [Fact]
    public void UsdPrim_GetPrimAtPath_ShouldReturnSamePrim()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var path = new SdfPath("/testPrim");
        var originalPrim = stage.DefinePrim(path, "TestType");

        // Act
        var retrievedPrim = stage.GetPrimAtPath(path);

        // Assert
        Assert.True(originalPrim.IsSameAs(retrievedPrim));
        Assert.Equal(originalPrim.GetTypeName(), retrievedPrim.GetTypeName());
    }

    [Fact]
    public void UsdPrim_TypeNameOperations_ShouldWork()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/test");

        // Act & Assert
        Assert.False(prim.HasTypeName());
        Assert.Equal(string.Empty, prim.GetTypeName());

        Assert.True(prim.SetTypeName("NewType"));
        Assert.True(prim.HasTypeName());
        Assert.Equal("NewType", prim.GetTypeName());

        Assert.True(prim.ClearTypeName());
        Assert.False(prim.HasTypeName());
        Assert.Equal(string.Empty, prim.GetTypeName());
    }

    [Fact]
    public void UsdStage_TraversePrims_ShouldReturnValidPrims()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        stage.DefinePrim("/prim1", "Type1");
        stage.DefinePrim("/prim2", "Type2");
        stage.DefinePrim("/prim1/child", "ChildType");

        // Act
        var prims = stage.Traverse().ToList();

        // Assert
        Assert.Equal(3, prims.Count);
        Assert.All(prims, p => Assert.True(p.IsValid()));
    }
}