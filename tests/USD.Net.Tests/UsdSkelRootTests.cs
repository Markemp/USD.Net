using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdSkel;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdSkelRootTests
{
    [Fact]
    public void UsdSkelRoot_BasicConstruction_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        
        // Act
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Assert
        Assert.True(skelRoot.IsValid);
        Assert.Equal(rootPath, skelRoot.Prim.GetPath());
        Assert.Equal("SkelRoot", skelRoot.Prim.GetTypeName());
        
        // Debug schema registration
        var registry = UsdSchemaRegistry.Instance;
        var schemaInfo = registry.FindSchemaInfo(typeof(UsdSkelRoot));
        Assert.NotNull(schemaInfo);
        Assert.Equal("SkelRoot", schemaInfo.TypeName.GetText());
    }
    
    [Fact]
    public void UsdSkelRoot_Find_FindsNearestAncestor()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var meshPath = new SdfPath("/Character/Body/Mesh");
        
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        var bodyPrim = stage.DefinePrim(new SdfPath("/Character/Body"));
        var meshPrim = stage.DefinePrim(meshPath);
        
        // Act
        var foundRoot = UsdSkelRoot.Find(meshPrim);
        
        // Assert
        Assert.True(foundRoot.IsValid);
        Assert.Equal(rootPath, foundRoot.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelRoot_Find_InvalidPrim_ReturnsInvalid()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        
        // Act
        var foundRoot = UsdSkelRoot.Find(invalidPrim);
        
        // Assert
        Assert.False(foundRoot.IsValid);
    }
    
    [Fact]
    public void UsdSkelRoot_HasSkelRoot_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var meshPath = new SdfPath("/Character/Body/Mesh");
        
        UsdSkelRoot.Define(stage, rootPath);
        var meshPrim = stage.DefinePrim(meshPath);
        var orphanPrim = stage.DefinePrim(new SdfPath("/Orphan"));
        
        // Act & Assert
        Assert.True(UsdSkelRoot.HasSkelRoot(meshPrim));
        Assert.False(UsdSkelRoot.HasSkelRoot(orphanPrim));
    }
    
    [Fact]
    public void UsdSkelRoot_GetSkeletons_FindsChildSkeletons()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelPath1 = new SdfPath("/Character/Skeleton1");
        var skelPath2 = new SdfPath("/Character/Body/Skeleton2");
        
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        UsdSkelSkeleton.Define(stage, skelPath1);
        UsdSkelSkeleton.Define(stage, skelPath2);
        
        // Act
        var skeletons = skelRoot.GetSkeletons();
        
        // Assert
        Assert.Equal(2, skeletons.Count);
        var paths = skeletons.Select(s => s.Prim.GetPath()).ToList();
        Assert.Contains(skelPath1, paths);
        Assert.Contains(skelPath2, paths);
    }
    
    [Fact]
    public void UsdSkelRoot_GetSkeleton_ReturnsFirstSkeleton()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelPath = new SdfPath("/Character/Skeleton");
        
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        UsdSkelSkeleton.Define(stage, skelPath);
        
        // Act
        var skeleton = skelRoot.GetSkeleton();
        
        // Assert
        Assert.True(skeleton.IsValid);
        Assert.Equal(skelPath, skeleton.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelRoot_GetAnimations_FindsChildAnimations()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var animPath1 = new SdfPath("/Character/Walk");
        var animPath2 = new SdfPath("/Character/Skeleton/Run");
        
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        UsdSkelAnimation.Define(stage, animPath1);
        UsdSkelAnimation.Define(stage, animPath2);
        
        // Act
        var animations = skelRoot.GetAnimations();
        
        // Assert
        Assert.Equal(2, animations.Count);
        var paths = animations.Select(a => a.Prim.GetPath()).ToList();
        Assert.Contains(animPath1, paths);
        Assert.Contains(animPath2, paths);
    }
    
    [Fact]
    public void UsdSkelRoot_CreateCharacterSetup_CreatesSkeletonAndAnimation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Act
        var skeleton = skelRoot.CreateCharacterSetup("MainSkeleton", "IdleAnim");
        
        // Assert
        Assert.True(skeleton.IsValid);
        Assert.Equal(new SdfPath("/Character/MainSkeleton"), skeleton.Prim.GetPath());
        
        // Check animation was created and linked
        var animation = skeleton.GetAnimationSource();
        Assert.True(animation.IsValid);
        Assert.Equal(new SdfPath("/Character/MainSkeleton/IdleAnim"), animation.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelRoot_CreateCharacterSetup_WithoutAnimation_CreatesSkeletonOnly()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Act
        var skeleton = skelRoot.CreateCharacterSetup("MainSkeleton", "");
        
        // Assert
        Assert.True(skeleton.IsValid);
        
        var animation = skeleton.GetAnimationSource();
        Assert.False(animation.IsValid);
    }
    
    [Fact]
    public void UsdSkelRoot_ConfigureForGameAsset_SetsMetadata()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Act
        skelRoot.ConfigureForGameAsset();
        
        // Assert
        Assert.True(skelRoot.Prim.GetMetadata<string>(new TfToken("kind")) == "component");
        
        var purposeAttr = skelRoot.GetPurposeAttr();
        Assert.True(purposeAttr.IsValid());
        Assert.True(purposeAttr.Get(out string purpose));
        Assert.Equal("default", purpose);
    }
    
    [Fact]
    public void UsdSkelRoot_HasSkeletalData_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Initially no data
        Assert.False(skelRoot.HasSkeletalData());
        
        // Add skeleton
        UsdSkelSkeleton.Define(stage, new SdfPath("/Character/Skeleton"));
        Assert.True(skelRoot.HasSkeletalData());
    }
    
    [Fact]
    public void UsdSkelRoot_GetStatistics_ReturnsCorrectCounts()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        UsdSkelSkeleton.Define(stage, new SdfPath("/Character/Skeleton1"));
        UsdSkelSkeleton.Define(stage, new SdfPath("/Character/Skeleton2"));
        UsdSkelAnimation.Define(stage, new SdfPath("/Character/Anim1"));
        
        // Act
        var (skeletonCount, animationCount, skinnedPrimCount) = skelRoot.GetStatistics();
        
        // Assert
        Assert.Equal(2, skeletonCount);
        Assert.Equal(1, animationCount);
        Assert.Equal(0, skinnedPrimCount); // No skinned prims created
    }
    
    [Fact]
    public void UsdSkelRoot_ValidateSkelRoot_ValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Character/Skeleton"));
        skeleton.Joints = ["Root", "Root/Hip"];
        
        // Act
        var (isValid, reason) = skelRoot.ValidateSkelRoot();
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(reason);
    }
    
    [Fact]
    public void UsdSkelRoot_ValidateSkelRoot_NoSkeletons_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        // Act
        var (isValid, reason) = skelRoot.ValidateSkelRoot();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("no skeletons", reason);
    }
    
    [Fact]
    public void UsdSkelRoot_InvalidPrim_HandlesGracefully()
    {
        // Arrange
        var invalidRoot = new UsdSkelRoot();
        
        // Act & Assert
        Assert.False(invalidRoot.IsValid);
        Assert.Empty(invalidRoot.GetSkeletons());
        Assert.Empty(invalidRoot.GetAnimations());
        Assert.Empty(invalidRoot.GetSkinnedPrims());
        Assert.False(invalidRoot.HasSkeletalData());
        
        var (isValid, reason) = invalidRoot.ValidateSkelRoot();
        Assert.False(isValid);
        Assert.Contains("invalid", reason);
    }
    
    [Fact]
    public void UsdSkelRoot_ToString_ShowsInformation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var rootPath = new SdfPath("/Character");
        var skelRoot = UsdSkelRoot.Define(stage, rootPath);
        
        UsdSkelSkeleton.Define(stage, new SdfPath("/Character/Skeleton"));
        UsdSkelAnimation.Define(stage, new SdfPath("/Character/Animation"));
        
        // Act
        var result = skelRoot.ToString();
        
        // Assert
        Assert.Contains("UsdSkelRoot", result);
        Assert.Contains("/Character", result);
        Assert.Contains("1 skeletons", result);
        Assert.Contains("1 animations", result);
    }
}