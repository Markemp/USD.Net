using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pxr.Base.Tf;
using Pxr.Usd;
using Pxr.Usd.Sdf;
using Pxr.Usd.UsdGeom;
using Pxr.Usd.UsdSkel;

namespace USD.Net.Tests;

[Trait("Category", "Unit")]
public class UsdSkelBindingAPITests
{
    [Fact]
    public void UsdSkelBindingAPI_CanApply_GeometricPrim_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var spherePrim = UsdGeomSphere.Define(stage, new SdfPath("/Sphere")).Prim;
        var nonGeomPrim = stage.DefinePrim(new SdfPath("/NonGeom"));
        
        // Act & Assert
        Assert.True(UsdSkelBindingAPI.CanApply(meshPrim));
        Assert.True(UsdSkelBindingAPI.CanApply(spherePrim));
        Assert.False(UsdSkelBindingAPI.CanApply(nonGeomPrim));
    }
    
    [Fact]
    public void UsdSkelBindingAPI_CanApply_InvalidPrim_ReturnsFalse()
    {
        // Arrange
        var invalidPrim = new UsdPrim();
        
        // Act & Assert
        Assert.False(UsdSkelBindingAPI.CanApply(invalidPrim));
    }
    
    [Fact]
    public void UsdSkelBindingAPI_Apply_ValidPrim_ReturnsValidAPI()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        
        // Act
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Assert
        Assert.True(bindingAPI.IsValid);
        Assert.Equal(meshPrim.GetPath(), bindingAPI.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_Apply_InvalidPrim_ReturnsInvalidAPI()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var nonGeomPrim = stage.DefinePrim(new SdfPath("/NonGeom"));
        
        // Act
        var bindingAPI = UsdSkelBindingAPI.Apply(nonGeomPrim);
        
        // Assert
        Assert.False(bindingAPI.IsValid);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_Get_ValidPrim_ReturnsAPI()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        
        // Act
        var bindingAPI = UsdSkelBindingAPI.Get(meshPrim);
        
        // Assert
        Assert.True(bindingAPI.IsValid);
        Assert.Equal(meshPrim.GetPath(), bindingAPI.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_GetFromStage_ValidPath_ReturnsAPI()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPath = new SdfPath("/Mesh");
        UsdGeomMesh.Define(stage, meshPath);
        
        // Act
        var bindingAPI = UsdSkelBindingAPI.Get(stage, meshPath);
        
        // Assert
        Assert.True(bindingAPI.IsValid);
        Assert.Equal(meshPath, bindingAPI.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_JointIndicesAndWeights_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var jointIndices = new List<List<int>>
        {
            new List<int> { 0, 1, 2, 0 },
            new List<int> { 1, 2, 3, 0 },
            new List<int> { 2, 3, 0, 1 }
        };
        
        var jointWeights = new List<List<float>>
        {
            new List<float> { 0.5f, 0.3f, 0.2f, 0.0f },
            new List<float> { 0.4f, 0.4f, 0.2f, 0.0f },
            new List<float> { 0.6f, 0.2f, 0.1f, 0.1f }
        };
        
        var time = UsdTimeCode.Create(1.0);
        
        // Act
        var setResult = bindingAPI.SetJointInfluences(jointIndices, jointWeights, time);
        var (retrievedIndices, retrievedWeights) = bindingAPI.GetJointInfluences(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(jointIndices.Count, retrievedIndices.Count);
        Assert.Equal(jointWeights.Count, retrievedWeights.Count);
        
        for (int i = 0; i < jointIndices.Count; i++)
        {
            Assert.Equal(jointIndices[i], retrievedIndices[i]);
            Assert.Equal(jointWeights[i], retrievedWeights[i]);
        }
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SetJointInfluences_MismatchedSizes_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var jointIndices = new List<List<int>> { new List<int> { 0, 1 } };
        var jointWeights = new List<List<float>> { new List<float> { 0.5f, 0.5f }, new List<float> { 1.0f, 0.0f } };
        
        // Act
        var result = bindingAPI.SetJointInfluences(jointIndices, jointWeights);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_GeomBindTransform_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var transform = GfMatrix4d.CreateTranslation(new GfVec3f(1, 2, 3));
        var time = UsdTimeCode.Create(2.0);
        
        // Act
        var setResult = bindingAPI.SetGeomBindTransform(transform, time);
        var retrievedTransform = bindingAPI.GetGeomBindTransform(time);
        
        // Assert
        Assert.True(setResult);
        Assert.Equal(transform, retrievedTransform);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_GeomBindTransform_NotSet_ReturnsIdentity()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act
        var transform = bindingAPI.GetGeomBindTransform();
        
        // Assert
        Assert.Equal(GfMatrix4d.Identity, transform);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SkinningMethod_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act & Assert - Classic Linear
        var setResult1 = bindingAPI.SetSkinningMethod("classicLinear");
        var method1 = bindingAPI.GetSkinningMethod();
        Assert.True(setResult1);
        Assert.Equal("classicLinear", method1);
        
        // Act & Assert - Dual Quaternion
        var setResult2 = bindingAPI.SetSkinningMethod("dualQuaternion");
        var method2 = bindingAPI.GetSkinningMethod();
        Assert.True(setResult2);
        Assert.Equal("dualQuaternion", method2);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SkinningMethod_InvalidMethod_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act
        var result = bindingAPI.SetSkinningMethod("invalidMethod");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SkinningMethod_NotSet_ReturnsDefault()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act
        var method = bindingAPI.GetSkinningMethod();
        
        // Assert
        Assert.Equal("classicLinear", method);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_Joints_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var joints = new List<string> { "Root", "Hip", "Spine", "Shoulder" };
        
        // Act
        bindingAPI.Joints = joints;
        var retrievedJoints = bindingAPI.Joints;
        
        // Assert
        Assert.Equal(joints, retrievedJoints);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SkeletonBinding_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        
        // Act
        var bindResult = bindingAPI.BindSkeleton(skeleton);
        var retrievedSkeleton = bindingAPI.GetSkeleton();
        
        // Assert
        Assert.True(bindResult);
        Assert.True(retrievedSkeleton.IsValid);
        Assert.Equal(skeleton.Prim.GetPath(), retrievedSkeleton.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SkeletonBinding_InvalidSkeleton_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var invalidSkeleton = new UsdSkelSkeleton();
        
        // Act
        var result = bindingAPI.BindSkeleton(invalidSkeleton);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_AnimationBinding_SetAndGet_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        
        // Act
        var bindResult = bindingAPI.BindAnimationSource(animation);
        var retrievedAnimation = bindingAPI.GetAnimationSource();
        
        // Assert
        Assert.True(bindResult);
        Assert.True(retrievedAnimation.IsValid);
        Assert.Equal(animation.Prim.GetPath(), retrievedAnimation.Prim.GetPath());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_AnimationBinding_InvalidAnimation_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var invalidAnimation = new UsdSkelAnimation();
        
        // Act
        var result = bindingAPI.BindAnimationSource(invalidAnimation);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_ValidateBinding_ValidSetup_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip", "Spine" };
        
        bindingAPI.BindSkeleton(skeleton);
        bindingAPI.Joints = new List<string> { "Root", "Hip" };
        
        var jointIndices = new List<List<int>> { new List<int> { 0, 1 } };
        var jointWeights = new List<List<float>> { new List<float> { 0.7f, 0.3f } };
        bindingAPI.SetJointInfluences(jointIndices, jointWeights);
        
        // Act
        var (isValid, reason) = bindingAPI.ValidateBinding();
        
        // Assert
        Assert.True(isValid);
        Assert.Empty(reason);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_ValidateBinding_NoSkeleton_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act
        var (isValid, reason) = bindingAPI.ValidateBinding();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("No skeleton bound", reason);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_ValidateBinding_InvalidJoint_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip" };
        
        bindingAPI.BindSkeleton(skeleton);
        bindingAPI.Joints = new List<string> { "Root", "NonExistentJoint" };
        
        var jointIndices = new List<List<int>> { new List<int> { 0, 1 } };
        var jointWeights = new List<List<float>> { new List<float> { 0.7f, 0.3f } };
        bindingAPI.SetJointInfluences(jointIndices, jointWeights);
        
        // Act
        var (isValid, reason) = bindingAPI.ValidateBinding();
        
        // Assert
        Assert.False(isValid);
        Assert.Contains("not found in skeleton", reason);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SetupGameMeshBinding_ValidData_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip", "Spine" };
        
        var jointIndices = new List<List<int>>
        {
            new List<int> { 0, 1, 2, 0 },
            new List<int> { 1, 2, 0, 0 }
        };
        var jointWeights = new List<List<float>>
        {
            new List<float> { 0.5f, 0.3f, 0.2f, 0.0f },
            new List<float> { 0.6f, 0.4f, 0.0f, 0.0f }
        };
        
        var bindTransform = GfMatrix4d.CreateTranslation(new GfVec3f(0, 1, 0));
        
        // Act
        var result = bindingAPI.SetupGameMeshBinding(skeleton, jointIndices, jointWeights, bindTransform);
        
        // Assert
        Assert.True(result);
        
        // Verify setup
        var boundSkeleton = bindingAPI.GetSkeleton();
        Assert.True(boundSkeleton.IsValid);
        Assert.Equal(skeleton.Prim.GetPath(), boundSkeleton.Prim.GetPath());
        
        var (retrievedIndices, retrievedWeights) = bindingAPI.GetJointInfluences();
        Assert.Equal(jointIndices.Count, retrievedIndices.Count);
        Assert.Equal(jointWeights.Count, retrievedWeights.Count);
        
        var transform = bindingAPI.GetGeomBindTransform();
        Assert.Equal(bindTransform, transform);
        
        var method = bindingAPI.GetSkinningMethod();
        Assert.Equal("classicLinear", method);
        
        var joints = bindingAPI.Joints;
        Assert.Equal(skeleton.Joints, joints);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_SetupGameMeshBinding_InvalidSkeleton_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var invalidSkeleton = new UsdSkelSkeleton();
        var jointIndices = new List<List<int>> { new List<int> { 0 } };
        var jointWeights = new List<List<float>> { new List<float> { 1.0f } };
        
        // Act
        var result = bindingAPI.SetupGameMeshBinding(invalidSkeleton, jointIndices, jointWeights);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_CreateRigidBinding_ValidData_ReturnsTrue()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip", "Spine" };
        
        var jointIndices = new List<int> { 0, 1, 2, 0, 1 }; // 5 vertices, each bound to one joint
        
        // Act
        var result = bindingAPI.CreateRigidBinding(skeleton, jointIndices);
        
        // Assert
        Assert.True(result);
        
        // Verify rigid binding
        var (retrievedIndices, retrievedWeights) = bindingAPI.GetJointInfluences();
        Assert.Equal(jointIndices.Count, retrievedIndices.Count);
        
        for (int i = 0; i < jointIndices.Count; i++)
        {
            Assert.Single(retrievedIndices[i]);
            Assert.Equal(jointIndices[i], retrievedIndices[i][0]);
            Assert.Single(retrievedWeights[i]);
            Assert.Equal(1.0f, retrievedWeights[i][0]);
        }
    }
    
    [Fact]
    public void UsdSkelBindingAPI_CreateRigidBinding_InvalidSkeleton_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var invalidSkeleton = new UsdSkelSkeleton();
        var jointIndices = new List<int> { 0, 1, 2 };
        
        // Act
        var result = bindingAPI.CreateRigidBinding(invalidSkeleton, jointIndices);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_CreateRigidBinding_EmptyJointIndices_ReturnsFalse()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root" };
        
        var jointIndices = new List<int>();
        
        // Act
        var result = bindingAPI.CreateRigidBinding(skeleton, jointIndices);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_NormalizeWeights_Works()
    {
        // Arrange
        var jointWeights = new List<List<float>>
        {
            new List<float> { 0.3f, 0.7f, 0.2f }, // Sum = 1.2, should normalize to 0.25, 0.583, 0.167
            new List<float> { 0.5f, 0.5f },       // Sum = 1.0, should stay the same
            new List<float> { 0.0f, 0.0f }        // Sum = 0.0, should stay the same (avoid divide by zero)
        };
        
        // Act
        UsdSkelBindingAPI.NormalizeWeights(jointWeights);
        
        // Assert
        Assert.Equal(0.25f, jointWeights[0][0], 2);
        Assert.Equal(0.583f, jointWeights[0][1], 2);
        Assert.Equal(0.167f, jointWeights[0][2], 2);
        
        Assert.Equal(0.5f, jointWeights[1][0]);
        Assert.Equal(0.5f, jointWeights[1][1]);
        
        Assert.Equal(0.0f, jointWeights[2][0]);
        Assert.Equal(0.0f, jointWeights[2][1]);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_HasValidBinding_Works()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Initially invalid
        Assert.False(bindingAPI.HasValidBinding());
        
        // Set up valid binding
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip" };
        
        var jointIndices = new List<List<int>> { new List<int> { 0, 1 } };
        var jointWeights = new List<List<float>> { new List<float> { 0.7f, 0.3f } };
        bindingAPI.SetupGameMeshBinding(skeleton, jointIndices, jointWeights);
        
        // Act & Assert
        Assert.True(bindingAPI.HasValidBinding());
    }
    
    [Fact]
    public void UsdSkelBindingAPI_GetStatistics_ReturnsCorrectInfo()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip", "Spine" };
        
        var animation = UsdSkelAnimation.Define(stage, new SdfPath("/Animation"));
        
        var jointIndices = new List<List<int>>
        {
            new List<int> { 0, 1, 2, 0 },
            new List<int> { 1, 2, 0, 0 },
            new List<int> { 2, 0, 1, 0 }
        };
        var jointWeights = new List<List<float>>
        {
            new List<float> { 0.4f, 0.3f, 0.2f, 0.1f },
            new List<float> { 0.5f, 0.3f, 0.2f, 0.0f },
            new List<float> { 0.6f, 0.2f, 0.2f, 0.0f }
        };
        
        bindingAPI.SetupGameMeshBinding(skeleton, jointIndices, jointWeights);
        bindingAPI.BindAnimationSource(animation);
        
        // Act
        var (vertexCount, maxInfluences, hasSkeleton, hasAnimation) = bindingAPI.GetStatistics();
        
        // Assert
        Assert.Equal(3, vertexCount);
        Assert.Equal(4, maxInfluences);
        Assert.True(hasSkeleton);
        Assert.True(hasAnimation);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_GetStatistics_EmptyBinding_ReturnsZeros()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/Mesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        // Act
        var (vertexCount, maxInfluences, hasSkeleton, hasAnimation) = bindingAPI.GetStatistics();
        
        // Assert
        Assert.Equal(0, vertexCount);
        Assert.Equal(0, maxInfluences);
        Assert.False(hasSkeleton);
        Assert.False(hasAnimation);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_InvalidPrim_HandlesGracefully()
    {
        // Arrange
        var invalidBinding = new UsdSkelBindingAPI();
        
        // Act & Assert
        Assert.False(invalidBinding.IsValid);
        Assert.False(invalidBinding.HasValidBinding());
        
        var (isValid, reason) = invalidBinding.ValidateBinding();
        Assert.False(isValid);
        Assert.Contains("invalid", reason);
        
        var (indices, weights) = invalidBinding.GetJointInfluences();
        Assert.Empty(indices);
        Assert.Empty(weights);
        
        var transform = invalidBinding.GetGeomBindTransform();
        Assert.Equal(GfMatrix4d.Identity, transform);
        
        var method = invalidBinding.GetSkinningMethod();
        Assert.Equal("classicLinear", method);
        
        var joints = invalidBinding.Joints;
        Assert.Empty(joints);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_ToString_ShowsInformation()
    {
        // Arrange
        var stage = UsdStage.CreateInMemory();
        var meshPrim = UsdGeomMesh.Define(stage, new SdfPath("/TestMesh")).Prim;
        var bindingAPI = UsdSkelBindingAPI.Apply(meshPrim);
        
        var skeleton = UsdSkelSkeleton.Define(stage, new SdfPath("/Skeleton"));
        skeleton.Joints = new List<string> { "Root", "Hip" };
        
        var jointIndices = new List<List<int>> { new List<int> { 0, 1, 0 }, new List<int> { 1, 0, 1 } };
        var jointWeights = new List<List<float>> { new List<float> { 0.5f, 0.3f, 0.2f }, new List<float> { 0.6f, 0.4f, 0.0f } };
        bindingAPI.SetupGameMeshBinding(skeleton, jointIndices, jointWeights);
        
        // Act
        var result = bindingAPI.ToString();
        
        // Assert
        Assert.Contains("UsdSkelBindingAPI", result);
        Assert.Contains("/TestMesh", result);
        Assert.Contains("2 vertices", result);
        Assert.Contains("3 max influences", result);
        Assert.Contains("skel:True", result);
        Assert.Contains("anim:False", result);
    }
    
    [Fact]
    public void UsdSkelBindingAPI_ToString_InvalidBinding_ShowsInvalid()
    {
        // Arrange
        var invalidBinding = new UsdSkelBindingAPI();
        
        // Act
        var result = invalidBinding.ToString();
        
        // Assert
        Assert.Contains("Invalid UsdSkelBindingAPI", result);
    }
}