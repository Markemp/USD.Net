using System.Linq;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Xunit;

namespace USD.Net.Tests;

public class UsdInheritsTests
{
    #region Basic Functionality

    [Fact]
    public void UsdInherits_CreateFromPrim_ShouldBeValid()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var inherits = prim.GetInherits();

        // Assert
        Assert.True(inherits.IsValid());
        Assert.Same(prim, inherits.GetPrim());
        Assert.False(inherits.HasInherits());
        Assert.Equal(0, inherits.GetNumInherits());
    }

    [Fact]
    public void UsdInherits_InvalidPrim_ShouldBeInvalid()
    {
        // Arrange
        var invalidPrim = new UsdPrim();

        // Act
        var inherits = new UsdInherits(invalidPrim);

        // Assert
        Assert.False(inherits.IsValid());
        Assert.False(inherits.HasInherits());
    }

    #endregion

    #region Inherit Management

    [Fact]
    public void UsdInherits_AddInherit_ShouldAddInheritPath()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        var classPath = new SdfPath("/BaseClass");

        // Act
        var success = inherits.AddInherit(classPath);

        // Assert
        Assert.True(success);
        Assert.True(inherits.HasInherits());
        Assert.Equal(1, inherits.GetNumInherits());
        
        var inheritPaths = inherits.GetAllDirectInherits();
        Assert.Single(inheritPaths);
        Assert.Equal("/BaseClass", inheritPaths[0].GetString());
    }

    [Fact]
    public void UsdInherits_AddMultipleInherits_ShouldAddAll()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Act
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));
        inherits.AddInherit(new SdfPath("/Interface"));

        // Assert
        Assert.Equal(3, inherits.GetNumInherits());
        
        var inheritPaths = inherits.GetAllDirectInherits();
        Assert.Equal(3, inheritPaths.Length);
        
        // Verify each inherit path
        Assert.Contains(inheritPaths, p => p.GetString() == "/BaseClass");
        Assert.Contains(inheritPaths, p => p.GetString() == "/Mixin");
        Assert.Contains(inheritPaths, p => p.GetString() == "/Interface");
    }

    [Fact]
    public void UsdInherits_AddDuplicateInherit_ShouldNotDuplicate()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        var classPath = new SdfPath("/BaseClass");

        // Act
        var success1 = inherits.AddInherit(classPath);
        var success2 = inherits.AddInherit(classPath); // Duplicate

        // Assert
        Assert.True(success1);
        Assert.True(success2); // Should return true (already exists)
        Assert.Equal(1, inherits.GetNumInherits()); // Should not duplicate
    }

    [Fact]
    public void UsdInherits_HasInherit_ShouldDetectSpecificInherits()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        var classPath = new SdfPath("/BaseClass");

        // Act
        inherits.AddInherit(classPath);

        // Assert
        Assert.True(inherits.HasInherit(classPath));
        Assert.False(inherits.HasInherit(new SdfPath("/OtherClass")));
    }

    #endregion

    #region Remove Operations

    [Fact]
    public void UsdInherits_RemoveInherit_ShouldRemoveSpecificInherit()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));
        inherits.AddInherit(new SdfPath("/Interface"));

        // Act
        var success = inherits.RemoveInherit(new SdfPath("/Mixin"));

        // Assert
        Assert.True(success);
        Assert.Equal(2, inherits.GetNumInherits());
        Assert.False(inherits.HasInherit(new SdfPath("/Mixin")));
        Assert.True(inherits.HasInherit(new SdfPath("/BaseClass")));
        Assert.True(inherits.HasInherit(new SdfPath("/Interface")));
    }

    [Fact]
    public void UsdInherits_ClearInherits_ShouldRemoveAllInherits()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));

        // Act
        var success = inherits.ClearInherits();

        // Assert
        Assert.True(success);
        Assert.False(inherits.HasInherits());
        Assert.Equal(0, inherits.GetNumInherits());
        Assert.Empty(inherits.GetAllDirectInherits());
    }

    #endregion

    #region Set Operations

    [Fact]
    public void UsdInherits_SetInherits_ShouldReplaceAllInherits()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        
        // Add initial inherits
        inherits.AddInherit(new SdfPath("/OldClass1"));
        inherits.AddInherit(new SdfPath("/OldClass2"));
        
        // New inherits to set
        var newInherits = new[]
        {
            new SdfPath("/NewClass1"),
            new SdfPath("/NewClass2"),
            new SdfPath("/NewClass3")
        };

        // Act
        var success = inherits.SetInherits(newInherits);

        // Assert
        Assert.True(success);
        Assert.Equal(3, inherits.GetNumInherits());
        
        var inheritPaths = inherits.GetAllDirectInherits();
        Assert.Contains(inheritPaths, p => p.GetString() == "/NewClass1");
        Assert.Contains(inheritPaths, p => p.GetString() == "/NewClass2");
        Assert.Contains(inheritPaths, p => p.GetString() == "/NewClass3");
        
        // Old inherits should be gone
        Assert.DoesNotContain(inheritPaths, p => p.GetString() == "/OldClass1");
        Assert.DoesNotContain(inheritPaths, p => p.GetString() == "/OldClass2");
    }

    #endregion

    #region List Position

    [Fact]
    public void UsdInherits_AddInheritWithPosition_ShouldRespectPosition()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Act - Add inherits with different positions
        inherits.AddInherit(new SdfPath("/Class1"), UsdListPosition.BackOfPrependList);
        inherits.AddInherit(new SdfPath("/Class2"), UsdListPosition.FrontOfPrependList);
        inherits.AddInherit(new SdfPath("/Class3"), UsdListPosition.BackOfAppendList);

        // Assert
        Assert.Equal(3, inherits.GetNumInherits());
        
        var inheritPaths = inherits.GetAllDirectInherits();
        // Front of prepend should be first
        Assert.Equal("/Class2", inheritPaths[0].GetString());
    }

    #endregion

    #region Validation

    [Fact]
    public void UsdInherits_AddInheritEmptyPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Act & Assert
        Assert.False(inherits.AddInherit(SdfPath.EmptyPath()));
        Assert.False(inherits.HasInherits());
    }

    [Fact]
    public void UsdInherits_AddNonPrimPath_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Act & Assert - Property paths should not be valid for inherits
        Assert.False(inherits.AddInherit(new SdfPath("/Class.property")));
        Assert.False(inherits.HasInherits());
    }

    #endregion

    #region Composition Support

    [Fact]
    public void UsdInherits_AuthoredInherits_ShouldTrackAuthoring()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Initially no authoring
        Assert.False(inherits.HasAuthoredInherits());

        // Act - Add inherits
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));

        // Assert
        Assert.True(inherits.HasAuthoredInherits());
        
        var authored = inherits.GetAuthoredInherits();
        Assert.Equal(2, authored.Length);
        Assert.Contains(authored, p => p.GetString() == "/BaseClass");
        Assert.Contains(authored, p => p.GetString() == "/Mixin");
    }

    [Fact]
    public void UsdInherits_AreInheritsAuthoredAt_ShouldCheckAuthoring()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Initially no authoring
        Assert.False(inherits.AreInheritsAuthoredAt());

        // Act - Add inherit
        inherits.AddInherit(new SdfPath("/BaseClass"));

        // Assert
        Assert.True(inherits.AreInheritsAuthoredAt());
    }

    #endregion

    #region Error Handling

    [Fact]
    public void UsdInherits_InvalidOperationsOnInvalidPrim_ShouldFail()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        var inherits = new UsdInherits(invalidPrim);

        // Act & Assert
        Assert.False(inherits.AddInherit(new SdfPath("/Class")));
        Assert.False(inherits.RemoveInherit(new SdfPath("/Class")));
        Assert.False(inherits.ClearInherits());
        Assert.False(inherits.SetInherits(new[] { new SdfPath("/Class") }));
    }

    [Fact]
    public void UsdInherits_SetInheritsWithNull_ShouldFail()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();

        // Act & Assert
        Assert.False(inherits.SetInherits(null!));
        Assert.False(inherits.HasInherits());
    }

    #endregion

    #region String Representation

    [Fact]
    public void UsdInherits_ToString_ShouldShowPrimAndCount()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");
        var inherits = prim.GetInherits();
        
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));

        // Act
        var result = inherits.ToString();

        // Assert
        Assert.Contains("/testPrim", result);
        Assert.Contains("2 inherits", result);
    }

    [Fact]
    public void UsdInherits_ToStringInvalid_ShouldShowInvalid()
    {
        // Arrange
        var invalidInherits = new UsdInherits();

        // Act
        var result = invalidInherits.ToString();

        // Assert
        Assert.Equal("UsdInherits(invalid)", result);
    }

    #endregion

    #region Integration with UsdPrim

    [Fact]
    public void UsdPrim_GetInherits_ShouldReturnValidInherits()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var prim = stage.DefinePrim("/testPrim");

        // Act
        var inherits = prim.GetInherits();

        // Assert
        Assert.True(inherits.IsValid());
        Assert.Same(prim, inherits.GetPrim());
    }

    #endregion

    #region Class-Based Composition Examples

    [Fact]
    public void UsdInherits_ClassBasedComposition_ShouldWorkEndToEnd()
    {
        // Arrange - Create a class hierarchy
        var stage = UsdStage.CreateInMemory();
        
        // Define a base class
        var baseClassPrim = stage.DefinePrim("/BaseClass");
        var baseAttr = baseClassPrim.CreateAttribute("baseProperty", "string");
        baseAttr.Set("baseValue");
        
        // Define a mixin class
        var mixinPrim = stage.DefinePrim("/Mixin");
        var mixinAttr = mixinPrim.CreateAttribute("mixinProperty", "int");
        mixinAttr.Set(42);
        
        // Define a concrete prim that inherits from both
        var concretePrim = stage.DefinePrim("/ConcretePrim");
        var inherits = concretePrim.GetInherits();

        // Act - Set up inheritance
        inherits.AddInherit(new SdfPath("/BaseClass"));
        inherits.AddInherit(new SdfPath("/Mixin"));

        // Assert - Verify inheritance structure
        Assert.Equal(2, inherits.GetNumInherits());
        Assert.True(inherits.HasInherit(new SdfPath("/BaseClass")));
        Assert.True(inherits.HasInherit(new SdfPath("/Mixin")));
        
        var inheritPaths = inherits.GetAllDirectInherits();
        Assert.Contains(inheritPaths, p => p.GetString() == "/BaseClass");
        Assert.Contains(inheritPaths, p => p.GetString() == "/Mixin");
        
        // Verify the inheritance relationship is established
        Assert.True(inherits.HasAuthoredInherits());
        Assert.True(inherits.AreInheritsAuthoredAt());
    }

    #endregion
}